using MqApi.Matrix;
using MqApi.Param;
using MqApi.Util;

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
		/// Name of the hidden parameter carrying a copy of the script text. It is not shown in the GUI;
		/// it exists so a saved session can be opened on a machine where the script path does not exist.
		/// </summary>
		protected virtual string ScriptTextLabel => "Script text";
		/// <summary>
		/// Extension used when a script has to be materialized on disk.
		/// </summary>
		protected virtual string CodeExtension =>
			Edit switch{
				EditorType.CodeR => ".R",
				EditorType.CodePython => ".py",
				_ => ".txt"
			};
		/// <summary>
		/// Unstructured parameters label in GUI.
		/// These parameters circumvent the usual XML serialization of parameters and are meant for simple scripts.
		/// </summary>
		protected virtual string AdditionalArgumentsLabel => "Additional arguments";
		/// <summary>
		/// Resolves the script file to run. A script is always backed by a file, so when the stored path
		/// is empty or gone (a session written on another machine, or a script that was only ever typed
		/// into the editor) the stored <see cref="ScriptTextLabel"/> is written into
		/// <see cref="GetScriptFilesFolder"/> and the path parameter is repointed at it.
		/// </summary>
		protected virtual bool ResolveCodeFile(Parameters param, out string codeFile, out string errString){
			errString = null;
			Parameter<string> pathParam = param.GetParam<string>(CodeLabel);
			codeFile = pathParam.Value;
			if (!string.IsNullOrWhiteSpace(codeFile) && File.Exists(codeFile)){
				return true;
			}
			string[] scriptText = param.GetParamNoException(ScriptTextLabel) is Parameter<string[]> textParam
				? textParam.Value
				: null;
			if (scriptText == null || scriptText.All(string.IsNullOrWhiteSpace)){
				errString = string.IsNullOrWhiteSpace(codeFile)
					? "No script has been selected. Use 'Select' to pick a script file, or 'Edit' to write one."
					: $"The script file '{codeFile}' was not found on this machine and this step carries no " +
					  "stored script text to fall back on.";
				return false;
			}
			try{
				string baseName = string.IsNullOrWhiteSpace(codeFile)
					? "script"
					: Path.GetFileNameWithoutExtension(codeFile);
				codeFile = FileUtils.GetNextAvailableFileName(Path.Combine(GetScriptFilesFolder(), baseName),
					CodeExtension);
				File.WriteAllLines(codeFile, scriptText);
				pathParam.Value = codeFile;
				return true;
			} catch (Exception ex){
				errString = $"The stored script text could not be written to disk: {ex.Message}";
				return false;
			}
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
		/// Resolves the interpreter stored in the parameters against this machine. A session saved on one
		/// machine keeps the interpreter it was configured with (e.g. "python" on Windows), which need not
		/// exist elsewhere (Linux/Mac usually only have "python3"), so fall back to <see cref="TryFindExecutable"/>.
		/// </summary>
		protected bool TryResolveExecutable(Parameters param, out string exe, out string errString){
			errString = null;
			exe = GetExectuable(param);
			if (string.IsNullOrWhiteSpace(exe)){
				errString = remoteExeNotSpecified;
				return false;
			}
			if (Utils.TryResolveExecutable(exe, out string resolved)){
				exe = resolved;
				return true;
			}
			if (TryFindExecutable(out string fallback) && Utils.TryResolveExecutable(fallback, out string resolvedFallback)){
				exe = resolvedFallback;
				return true;
			}
			errString = $"The executable '{exe}' configured in this step was not found on this machine and no " +
			            $"replacement could be located. " + remoteExeNotSpecified;
			return false;
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
		protected virtual FileParam CodeFileParam(IMatrixData mdata){
			return new FileParam(CodeLabel){Filter = CodeFilter, Edit = Edit, Data = Edit != EditorType.None? mdata : null};
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
		protected Parameter[] SpecificParameters(ref string errString, IMatrixData mdata)
		{
			return [CodeFileParam(mdata), ScriptTextParam(), AdditionalArgumentsParam()];
		}
		protected virtual Parameter[] SpecificParameters(IMatrixData mdata,ref string errString)
		{
			return [CodeFileParam(mdata), ScriptTextParam(), AdditionalArgumentsParam()];
		}
		/// <summary>
		/// Hidden copy of the script text, kept beside the script path so a session stays runnable when
		/// it is opened where that path does not exist. Never edited directly - the script file is the
		/// single source of truth and this is refreshed from it whenever the session is saved.
		/// </summary>
		protected virtual MultiStringParam ScriptTextParam(){
			return new MultiStringParam(ScriptTextLabel){Visible = false};
		}
        public static string GetAppDataPerseus()
		{
			// Per-user config folder, so several users can share one Perseus installation without
			// overwriting each other's settings (RPath, pyPath, ...):
			//   Windows   -> %LocalAppData%\Perseus
			//   Linux/Mac -> $XDG_DATA_HOME/Perseus or ~/.local/share/Perseus (never the shared install dir)
			string applicationFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			if (string.IsNullOrEmpty(applicationFolder))
			{
				// SpecialFolder.LocalApplicationData can be empty in some headless/Unix environments; fall
				// back to the user's home so the location stays user-specific rather than shared.
				string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
				if (string.IsNullOrEmpty(home))
				{
					home = Environment.GetEnvironmentVariable("HOME") ??
					       Environment.GetEnvironmentVariable("USERPROFILE") ?? ".";
				}
				applicationFolder = Path.Combine(home, ".local", "share");
			}
			string perseusFolder = Path.Combine(applicationFolder, "Perseus");
			if (!Directory.Exists(perseusFolder))
			{
				Directory.CreateDirectory(perseusFolder);
			}
			return perseusFolder;
		}
		/// <summary>
		/// Where scripts are materialized when they have no file of their own yet: a script typed into
		/// the editor, or one restored from a session whose original path does not exist here.
		/// </summary>
		public static string GetScriptFilesFolder()
		{
			string scriptFilesFolder = Path.Combine(GetAppDataPerseus(), "scriptFiles");
			if (!Directory.Exists(scriptFilesFolder))
			{
				Directory.CreateDirectory(scriptFilesFolder);
			}
			return scriptFilesFolder;
		}
    }
}