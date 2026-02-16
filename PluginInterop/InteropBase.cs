using MqApi.Matrix;
using MqApi.Param;

namespace PluginInterop{
	/// <summary>
	/// Base class for all interop functionality.
	///
	/// Provides virtual implementations for obtaining the path to the executable (e.g. Python), the path to the script,
	/// and unstructured parameters.
	/// </summary>
	public abstract class InteropBase{
		public const string projectUrl = "https://github.com/jdrudolph/PluginInterop";
		public const string remoteExeNotSpecified =
			"Please specify an executable.You can either provide its full file path or make sure that it is " +
			"listed in the PATH and just provide its name.For more information visit the project homepage.";
		/// <summary>
		/// Interpreter path label in GUI.
		/// </summary>
		protected virtual string InterpreterLabel => "Executable";
		/// <summary>
		/// Interpreter file filter for file chooser in parameter GUI.
		/// </summary>
		protected virtual string InterpreterFilter => "Interpreter, *.exe|*.exe";
		/// <summary>
		/// Code file label in GUI.
		/// </summary>
		protected virtual string CodeLabel => "Script file";
		/// <summary>
		/// Code file filter for file chooser in parameter GUI
		/// </summary>
		protected virtual string CodeFilter => "Script";
		/// <summary>
		/// Unstructured parameters label in GUI.
		/// These parameters circumvent the usual XML serialization of parameters and are meant for simple scripts.
		/// </summary>
		protected virtual string AdditionalArgumentsLabel => "Additional arguments";
		protected virtual bool TryGetCodeFile(Parameters param, out string codeFile, out ScriptMode scriptMode){
			ParameterWithSubParams<int> scriptModeParam = param.GetParamWithSubParams<int>("Script mode");
			scriptMode = (ScriptMode)scriptModeParam.Value;
			Parameters paramsModel = scriptModeParam.GetSubParameters();
			codeFile = Path.Combine(Path.GetTempPath(), $"Perseus_{Path.GetRandomFileName()}");
			if (scriptMode == ScriptMode.Internal)
			{
				string[] scriptText = paramsModel.GetParam<string[]>("Script text").Value;
				StreamWriter writer = new(codeFile);
				foreach (string text in scriptText)
				{
					writer.WriteLine(text);
				}
				writer.Close();
			}
			else
			{
				codeFile = paramsModel.GetParam<string>(CodeLabel).Value;
				if (string.IsNullOrEmpty(codeFile) || !File.Exists(codeFile))
				{
					return false;
				}
			}
			return true;
		}
		protected virtual bool TryGetCodeFile(Parameters param, out string codeFile)
		{
			codeFile = param.GetParam<string>(CodeLabel).Value;
			if (string.IsNullOrEmpty(codeFile) || !File.Exists(codeFile))
			{
				return false;
			}
			return true;
		}

        /// <summary>
        /// Get parameters passed on the command line. Defaults to <see cref="AdditionalArgumentsLabel"/>.
        /// Other plugins might save parameters to XML file and pass the file path <see cref="Utils.WriteParametersToFile"/>.
        /// </summary>
        protected virtual string GetCommandLineArguments(Parameters param){
			Parameter parameter = param.GetParamNoException(AdditionalArgumentsLabel);
			if (parameter == null){
				throw new Exception($"Expected standard parameter \"{AdditionalArgumentsLabel}\" could not be found. " +
				                    $"You might have to override {nameof(GetCommandLineArguments)} to account for custom parameters.");
			}
			return parameter.StringValue;
		}
		/// <summary>
		/// Extract the executable name. See <see cref="ExecutableParam"/>.
		/// </summary>
		protected virtual string GetExectuable(Parameters param){
			return param.GetParam<string>(InterpreterLabel).Value;
		}
		/// <summary>
		/// FileParam for specifying the executable. See <see cref="GetExectuable"/>.
		/// </summary>
		protected virtual FileParam ExecutableParam(){
			FileParam executableParam = new FileParam(InterpreterLabel){Filter = InterpreterFilter};
			if (TryFindExecutable(out string executable)){
				executableParam.Value = executable;
			}
			return executableParam;
		}
		protected virtual FileParam CodeFileParam(){
			return new FileParam(CodeLabel){Filter = CodeFilter, Edit = Edit};
		}
        /// <summary>
        /// Returns true and the path of the executable if found.
        /// </summary>
        protected virtual bool TryFindExecutable(out string path){
			path = null;
			return false;
		}
		/// <summary>
		/// Create parameter for additional unstructured arguments.
		/// </summary>
		protected virtual StringParam AdditionalArgumentsParam(){
			return new StringParam(AdditionalArgumentsLabel);
		}
		public abstract EditorType Edit { get; }
		protected virtual Parameter[] SpecificParameters(ref string errString)
		{
			SingleChoiceWithSubParams scriptMode = new("Script mode", 1)
			{
				Values = ["External", "Internal"],
				SubParams = [new Parameters(CodeFileParam()), new Parameters(new MultiStringParam("Script text"))]

			};
			return [scriptMode, AdditionalArgumentsParam()];
		}
		protected virtual Parameter[] SpecificParameters(IMatrixData mdata,ref string errString)
		{
			SingleChoiceWithSubParams scriptMode = new("Script mode", 1)
			{
				Values = ["External", "Internal"],
				SubParams = [new Parameters(CodeFileParam()), new Parameters(new MultiStringParam("Script text"))]

			};
			return [scriptMode, AdditionalArgumentsParam()];
		}
        public static string GetAppDataPerseus()
		{
			string applicationFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			string perseusFolder = Path.Combine(applicationFolder, "Perseus");
			if (!Directory.Exists(perseusFolder))
			{
				Directory.CreateDirectory(perseusFolder);
			}
			return perseusFolder;
		}
    }
}