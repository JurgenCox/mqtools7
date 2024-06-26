﻿using System.Diagnostics;
using System.IO;
using System.Linq;
using MqApi.Document;
using MqApi.Drawing;
using MqApi.Generic;
using MqApi.Matrix;
using MqApi.Param;
using MqApi.Util;
namespace PluginInterop{
	public abstract class MatrixUpload : InteropBase, IMatrixUpload{
		public abstract string Name{ get; }
		public abstract string Description{ get; }
		public float DisplayRank => 1;
		public bool IsActive => true;
		public int GetMaxThreads(Parameters parameters) => 1;
		public bool HasButton => true;
		public abstract Bitmap2 DisplayImage{ get; }
		public string Url => projectUrl;
		public string[] HelpSupplTables => new string[0];
		public int NumSupplTables => 0;
		public string[] HelpDocuments => new string[0];
		public int NumDocuments => 0;
		/// <summary>
		/// Create the parameters for the GUI with default of 'Code file' and 'Executable'. Includes buttons
		/// for preview downloads of 'Parameters' for development purposes.
		/// Overwrite this function to provide custom parameters.
		/// </summary>
		public virtual Parameters GetParameters(ref string errString){
			Parameters parameters = new Parameters();
			parameters.AddParameterGroup(SpecificParameters(ref errString), "Specific", false);
			Parameter parametersPreviewButton = Utils.ParametersPreviewButton(parameters);
			parameters.AddParameterGroup(new[]{ExecutableParam(), parametersPreviewButton}, "Generic", false);
			return parameters;
		}
		/// <summary>
		/// Create specific processing parameters. Defaults to 'Code file'. You can provide custom parameters
		/// by overriding this function. Called by <see cref="GetParameters"/>.
		/// </summary>
		protected virtual Parameter[] SpecificParameters(ref string errString){
			return new Parameter[]{CodeFileParam(), AdditionalArgumentsParam()};
		}
		public void LoadData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			string remoteExe = GetExectuable(param);
			if (string.IsNullOrWhiteSpace(remoteExe)){
				processInfo.ErrString = remoteExeNotSpecified;
			}
			string outFile = Path.GetTempFileName();
			if (!TryGetCodeFile(param, out string codeFile)){
				processInfo.ErrString = $"Code file '{codeFile}' was not found";
				return;
			}
			if (supplTables == null){
				supplTables = Enumerable.Range(0, NumSupplTables).Select(i => (IMatrixData) mdata.CreateNewInstance())
					.ToArray();
			}
			string[] suppFiles = supplTables.Select(i => Path.GetTempFileName()).ToArray();
			string commandLineArguments = GetCommandLineArguments(param);
			string args = $"{codeFile} {commandLineArguments} {outFile} {string.Join(" ", suppFiles)}";
			Debug.WriteLine($"executing > {remoteExe} {args}");
			if (Utils.RunProcess(remoteExe, args, processInfo.Status, out string processInfoErrString) != 0){
				processInfo.ErrString = processInfoErrString;
				return;
			}
			PerseusUtil.ReadMatrixFromFile(mdata, processInfo, outFile, '\t');
			for (int i = 0; i < NumSupplTables; i++){
				PerseusUtil.ReadMatrixFromFile(supplTables[i], processInfo, suppFiles[i], '\t');
			}
		}
	}
}