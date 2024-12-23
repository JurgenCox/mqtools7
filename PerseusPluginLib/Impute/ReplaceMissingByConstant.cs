using System.Collections.Generic;
using MqApi.Document;
using MqApi.Drawing;
using MqApi.Generic;
using MqApi.Matrix;
using MqApi.Num;
using MqApi.Param;
namespace PerseusPluginLib.Impute{
	public class ReplaceMissingByConstant : IMatrixProcessing{
		public bool HasButton => false;
		public Bitmap2 DisplayImage => null;
		public string Description => "Replaces all missing values in the main columns with a constant.";
		public string HelpOutput => "Same matrix but with missing values replaced.";
		public string[] HelpSupplTables => new string[0];
		public int NumSupplTables => 0;
		public string Name => "Replace missing values by constant";
		public string Heading => "Imputation";
		public bool IsActive => true;
		public float DisplayRank => 1;
		public string[] HelpDocuments => new string[0];
		public int NumDocuments => 0;
		public string Url
			=>
                "https://cox-labs.github.io/coxdocs/replacemissingbyconstant.html";
		public int GetMaxThreads(Parameters parameters){
			return 1;
		}
		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			double value = param.GetParam<double>("Value").Value;
			int[] inds = param.GetParam<int[]>("Columns").Value;
			List<int> mainInds = new List<int>();
			List<int> numInds = new List<int>();
			foreach (int ind in inds){
				if (ind < mdata.ColumnCount){
					mainInds.Add(ind);
				} else{
					numInds.Add(ind - mdata.ColumnCount);
				}
			}
			double[] numImputationsPerRow = new double[mdata.RowCount];
            ReplaceMissingsByVal(value, mdata, mainInds, numInds, numImputationsPerRow);
            mdata.AddNumericColumn("Number Of Imputations", "", numImputationsPerRow);
        }
		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			return
				new Parameters(
					new DoubleParam("Value", 0){Help = "The value that is going to be filled in for missing values."},
					new MultiChoiceParam("Columns", ArrayUtils.ConsecutiveInts(mdata.ColumnCount)){
						Values = ArrayUtils.Concat(mdata.ColumnNames, mdata.NumericColumnNames)
					});
		}
		private static void ReplaceMissingsByVal(double value, IMatrixData data, IEnumerable<int> mainInds,
			IEnumerable<int> numInds, double[] numImputationsPerRow) {
			foreach (int j in mainInds){
				for (int i = 0; i < data.RowCount; i++){
					if (double.IsNaN(data.Values.Get(i, j))){
						data.Values.Set(i, j, value);
						data.IsImputed[i, j] = true;
						numImputationsPerRow[i]++;
                    }
				}
			}
			foreach (int j in numInds){
				for (int i = 0; i < data.RowCount; i++){
					if (double.IsNaN(data.NumericColumns[j][i])){
						data.NumericColumns[j][i] = value;
					}
				}
			}
		}
	}
}