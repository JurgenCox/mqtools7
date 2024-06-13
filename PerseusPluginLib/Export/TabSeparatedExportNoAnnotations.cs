using MqApi.Drawing;
using MqApi.Generic;
using MqApi.Matrix;
using MqApi.Param;
using MqUtil;
namespace PerseusPluginLib.Export{
	public class TabSeparatedExportNoAnnotations : IMatrixExport{
		public bool HasButton => true;
		public Bitmap2 DisplayImage => Bitmap2.GetImage("Save-icon.png");
		public string Description =>
			"Save the matrix to a tab-separated text file. No Information on column types will be retained.";
		public string Name => "Generic matrix export (no annotations)";
		public bool IsActive => true;
		public float DisplayRank => 1;
		public int GetMaxThreads(Parameters parameters){
			return 1;
		}
		public string Url => "https://cox-labs.github.io/coxdocs/tabseparatedexport.html";
		public void Export(Parameters parameters, IMatrixData data, ProcessInfo processInfo){
			string filename = parameters.GetParam<string>("File name").Value;
			bool addtlMatrices = parameters.GetParam<bool>("Write quality and imputed matrices").Value;
			addtlMatrices = addtlMatrices && data.IsImputed != null && data.Quality != null &&
			                data.IsImputed.IsInitialized() &&
			                data.Quality.IsInitialized();
			PerseusUtils.WriteMatrixNoAnnotation(data, filename, addtlMatrices);
		}
		public Parameters GetParameters(IMatrixData matrixData, ref string errorString){
			return new Parameters(new FileParam("File name"){Filter = "Tab separated file (*.txt)|*.txt", Save = true},
				new BoolParam("Write quality and imputed matrices", false));
		}
	}
}