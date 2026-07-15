using MqApi.Document;
using MqApi.Drawing;
using MqApi.Generic;
using MqApi.Matrix;
using MqApi.Param;
using MqUtil;
namespace PerseusPluginLib.Load{
	// Portable "Generic matrix upload" activity. It lives here (next to the matching "Generic matrix
	// export" in Export\TabSeparatedExport.cs) rather than in the WinForms PluginBase project, so the
	// headless PerseusCmd (and any non-Windows host) can load tab/CSV matrices. It has no WinForms
	// dependency: the icon uses MqApi's Bitmap2 and the actual read goes through MqUtil.PerseusUtils.
	public class GenericMatrixUpload : IMatrixUpload{
		public bool HasButton => false;
		public Bitmap2 DisplayImage => Bitmap2.GetImage("upload64.png");
		public string Name => "Generic matrix upload";
		public bool IsActive => true;
		public float DisplayRank => 0;
		public string[] HelpSupplTables => new string[0];
		public int NumSupplTables => 0;
		public string[] HelpDocuments => new string[0];
		public int NumDocuments => 0;
		public string Url => "https://cox-labs.github.io/coxdocs/genericmatrixupload.html";
		public string Heading => "Basic";
		public string Description
			=>
				"Load data from a tab-separated file. The first row should contain the column names, also separated by tab characters. " +
				"All following rows contain the tab-separated values. Such a file can for instance be generated from an excel sheet by " +
				"using the export as a tab-separated .txt file.";

		public int GetMaxThreads(Parameters parameters){
			return 1;
		}

		public Parameters GetParameters(ref string errorString){
			return
				new Parameters(new PerseusLoadMatrixParam("File"){
					Filter =
						"Text (Tab delimited) (*.txt;*.tsv)|*.txt;*.txt.gz;*.tsv;*.tsv.gz|CSV (Comma delimited) (*.csv)|*.csv;*.csv.gz",
					Help = "Please specify here the name of the file to be uploaded including its full path."
				}, new BoolWithSubParams("Limit rows", false){
					SubParamsTrue = new Parameters(new IntParam("Number of rows", 1000)),
					Help = "Only read a limited number of rows from the beginning of the file."
				});
		}

		public void LoadData(IMatrixData mdata, Parameters parameters, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			PerseusLoadMatrixParam par = (PerseusLoadMatrixParam) parameters.GetParam("File");
			string filename = par.Filename;
			if (string.IsNullOrEmpty(filename)){
				processInfo.ErrString = "Please specify a filename";
				return;
			}
			ParameterWithSubParams<bool> limitPar = parameters.GetParamWithSubParams<bool>("Limit rows");
			int nlimit = -1;
			bool limit = limitPar.Value;
			if (limit){
				nlimit = limitPar.GetSubParameters().GetParam<int>("Number of rows").Value;
			}
			PerseusUtils.ReadMatrixFromFile(mdata, processInfo, filename, par.MainColumnIndices, par.NumericalColumnIndices,
				par.CategoryColumnIndices, par.TextColumnIndices, par.MultiNumericalColumnIndices, par.MainFilterParameters,
				par.NumericalFilterParameters, par.ShortenExpressionColumnNames, limit, nlimit);
		}
	}
}
