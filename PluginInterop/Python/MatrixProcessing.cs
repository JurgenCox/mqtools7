using MqApi.Document;
using MqApi.Drawing;
using MqApi.Generic;
using MqApi.Matrix;
using MqApi.Param;
using MqApi.Util;
using MqUtil;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
namespace PluginInterop.Python{
	public class MatrixProcessing : PluginInterop.MatrixProcessing{
		public override string Name => "Python";
		public override string Description => "Run Python script";
		protected override string CodeFilter => "Python script, *.py | *.py";
		public override Bitmap2 DisplayImage => Bitmap2.GetImage("python.png");
		/// <summary>
		/// List of all required python packages.
		/// These packages will be checked by <see cref="ExecutableParam"/>.
		/// </summary>
		protected virtual string[] ReqiredPythonPackages => new[]{"perseuspy"};
		protected override FileParam ExecutableParam(){
			return Utils.CreateCheckedFileParam(InterpreterLabel, InterpreterFilter, TryFindExecutable,
				ReqiredPythonPackages);
		}
		protected override bool TryFindExecutable(out string path){
			return Utils.GetPythonPath(out path);
		}
		public override EditorType Edit => EditorType.CodePython;

		// Gets the user's Python code and turns it into a temporary runtime wrapper script.
		//
		// Internal mode stores code directly in the parameter.
		// External mode stores a path to a .py file, so we read that file first.
		//
		// We return the generated wrapper path instead of the original user script path.
		// The wrapper provides df, runs the user's code, checks result_df, and writes the Perseus output.
		protected override bool TryGetCodeFile(Parameters param, out string codeFile, out ScriptMode scriptMode)
		{
			codeFile = string.Empty;
			scriptMode = ScriptMode.Internal;

			ParameterWithSubParams<int> scriptModeParam = param.GetParamWithSubParams<int>("Script mode");
			scriptMode = (ScriptMode)scriptModeParam.Value;
			Parameters paramsModel = scriptModeParam.GetSubParameters();

			string rawUserCode;
			if (scriptMode == ScriptMode.Internal)
			{
				string[] scriptText = paramsModel.GetParam<string[]>("Script text").Value;
				rawUserCode = string.Join(Environment.NewLine, scriptText ?? Array.Empty<string>());
			}
			else
			{
				string rawCodeFile = paramsModel.GetParam<string>(CodeLabel).Value;
				if (string.IsNullOrWhiteSpace(rawCodeFile) || !File.Exists(rawCodeFile))
				{
					return false;
				}

				rawUserCode = File.ReadAllText(rawCodeFile);
			}

			codeFile = BuildWrappedPythonRuntimeScript(rawUserCode, NumSupplTables);
			return true;
		}

		// Runs Python processing using the generated runtime wrapper.
		// The wrapper makes the runtime behavior match the inline editor: user code receives df
		// and must assign result_df.
		public override void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo)
		{
			string remoteExe = param.GetParam<string>(InterpreterLabel).Value;
			if (string.IsNullOrWhiteSpace(remoteExe))
			{
				processInfo.ErrString = remoteExeNotSpecified;
				return;
			}

			string inFile = Path.GetTempFileName();
			PerseusUtils.WriteMatrixToFile(mdata, inFile, AdditionalMatrices);

			string outFile = Path.GetTempFileName();

			if (!TryGetCodeFile(param, out string codeFile, out ScriptMode scriptMode))
			{
				processInfo.ErrString = $"Code file '{codeFile}' was not found";
				return;
			}

			try
			{
				if (supplTables == null)
				{
					supplTables = Enumerable.Range(0, NumSupplTables)
						.Select(i => (IMatrixData)mdata.CreateNewInstance())
						.ToArray();
				}

				string[] suppFiles = supplTables.Select(i => Path.GetTempFileName()).ToArray();
				string commandLineArguments = GetCommandLineArguments(param);
				string args = $"{codeFile} {commandLineArguments} {inFile} {outFile} {string.Join(" ", suppFiles)}";

				Debug.WriteLine($"executing > {remoteExe} {args}");

				if (global::PluginInterop.Utils.RunProcess(remoteExe, args, processInfo.Status, out string processInfoErrString) != 0)
				{
					processInfo.ErrString = processInfoErrString;
					return;
				}

				mdata.Clear();
				PerseusUtil.ReadMatrixFromFile(mdata, processInfo, outFile, '\t');

				for (int i = 0; i < NumSupplTables; i++)
				{
					PerseusUtil.ReadMatrixFromFile(supplTables[i], processInfo, suppFiles[i], '\t');
				}
			}

			// Delete only the generated runtime wrapper.
			// Do not delete the user's original external script file.
			finally
			{
				try
				{
					if (!string.IsNullOrWhiteSpace(codeFile) &&
						File.Exists(codeFile) &&
						Path.GetFileName(codeFile).StartsWith("perseus_python_runtime_"))
					{
						File.SetAttributes(codeFile, FileAttributes.Normal);
						File.Delete(codeFile);
					}
				}
				catch
				{
					// best effort cleanup
				}
			}
		}

		// Builds a temporary Python script that wraps the user's code.
		//
		// The user code is stored as Base64 UTF-8 text so quotes, indentation, backslashes,
		// and non-English characters cannot break the generated wrapper.
		// The wrapper decodes the text, compiles it as "inline_user_code", runs it, then writes
		// result_df back to Perseus.
		private static string BuildWrappedPythonRuntimeScript(string rawUserCode, int numSupplTables)
		{
			string userCodeB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawUserCode ?? string.Empty));

			string script = $@"
import sys
import traceback
import base64

def fail(msg, code=1):
	sys.stderr.write(msg + ""\n"")
	sys.exit(code)

try:
	from perseuspy import pd
except Exception as e:
	fail(""perseuspy is required but could not be imported.\n"" + str(e))

num_suppl_tables = {numSupplTables}

if len(sys.argv) < 3 + num_suppl_tables:
	fail(""Not enough arguments. Expected infile, outfile, and optional supplementary output files."")

infile = sys.argv[-(num_suppl_tables + 2)]
outfile = sys.argv[-(num_suppl_tables + 1)]

try:
	df = pd.read_perseus(infile)
except Exception:
	fail(""Failed to read Perseus matrix via pd.read_perseus(infile).\n"" + traceback.format_exc())

user_code_b64 = ""{userCodeB64}""

try:
	user_code = base64.b64decode(user_code_b64.encode(""ascii"")).decode(""utf-8"", errors=""replace"")
except Exception:
	fail(""Failed to decode inline user code.\n"" + traceback.format_exc())

try:
	compiled = compile(user_code, ""inline_user_code"", ""exec"")
except Exception:
	fail(""Syntax error while compiling user code.\n"" + traceback.format_exc())

locals_dict = {{ ""df"": df, ""pd"": pd }}

try:
	exec(compiled, locals_dict, locals_dict)
except Exception:
	sys.stderr.write(""INLINE_USER_CODE_ERROR\n"")
	fail(""Error while executing user code.\n"" + traceback.format_exc())

result_df = locals_dict.get(""result_df"")
if result_df is None:
	fail(""Your code must assign a pandas DataFrame named result_df."")

try:
	result_df.to_perseus(outfile)
except Exception:
	fail(""Failed to write Perseus matrix via result_df.to_perseus(outfile).\n"" + traceback.format_exc())
";

			string tempScriptPath = Path.Combine(
				Path.GetTempPath(),
				"perseus_python_runtime_" + Guid.NewGuid().ToString("N") + ".py");

			File.WriteAllText(tempScriptPath, script, Encoding.UTF8);
			return tempScriptPath;
		}
	}
}