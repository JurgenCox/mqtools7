using MqApi.Document;
using MqApi.Drawing;
using MqApi.Generic;
using MqApi.Matrix;
using MqApi.Param;
namespace PerseusPluginLib.Impute{
	public class ReplaceImputedByNan : IMatrixProcessing{
		public string Category => IMatrixProcessingCategories.DataHandling;
        public bool HasButton => false;
		public Bitmap2 DisplayImage => null;
		public string Description => "Replaces all values that have been imputed with NaN.";
		public string HelpOutput => "Same matrix but with imputed values deleted.";
		public string[] HelpSupplTables => new string[0];
		public int NumSupplTables => 0;
		public string Name => "Replace imputed values by NaN";
		public string Heading => "Imputation";
		public bool IsActive => true;
		public float DisplayRank => 10;
		public string[] HelpDocuments => new string[0];
		public int NumDocuments => 0;
		public string Url
			=> "https://cox-labs.github.io/coxdocs/replaceimputedbynan.html";
		public int GetMaxThreads(Parameters parameters){
			return 1;
		}
		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			return new Parameters(new Parameter[]{ });
		}
		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			Replace(mdata);
		}
		public static void Replace(IMatrixData data){
			for (int i = 0; i < data.RowCount; i++){
				for (int j = 0; j < data.ColumnCount; j++){
					if (data.IsImputed[i, j]){
						data.Values.Set(i, j, double.NaN);
					}
				}
			}
		}
	}
}