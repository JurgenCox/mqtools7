using System;
using System.Collections.Generic;
using System.Linq;
using MqApi.Document;
using MqApi.Drawing;
using MqApi.Generic;
using MqApi.Matrix;
using MqApi.Num;
using MqApi.Num.Vector;
using MqApi.Param;
using MqUtil.Num;
using MqUtil.Num.Test;
using PerseusPluginLib.Utils;
namespace PerseusPluginLib.Significance{
	public class SignificanceB : IMatrixProcessing{
		public bool HasButton => false;
		public Bitmap2 DisplayImage => null;
		public string[] HelpDocuments => new string[0];
		public int NumDocuments => 0;
		public string[] HelpSupplTables => new string[0];
		public int NumSupplTables => 0;
		public string Name => "Significance B";
		public string Heading => "Outliers";
		public bool IsActive => true;
		public float DisplayRank => 1;
		public string Url
			=> "https://cox-labs.github.io/coxdocs/significanceb.html";
		public string Description
			=>
				"Same as Significance A, but intensity-dependent. For details see Cox and Mann (2008) Nat. Biotech. 26, 1367-72.";
		public string HelpOutput
			=>
				"A numerical column is added containing the significance A value. Furthermore, a categorical column is added " +
				"indicating by '+' if a row is significant.";
		public int GetMaxThreads(Parameters parameters){
			return 1;
		}
		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			int[] rcols = param.GetParam<int[]>("Ratio columns").Value;
			int[] icols = param.GetParam<int[]>("Intensity columns").Value;
			if (rcols.Length == 0){
				processInfo.ErrString = "Please specify some ratio columns.";
				return;
			}
			if (rcols.Length != icols.Length){
				processInfo.ErrString = "The number of ratio and intensity columns have to be equal.";
				return;
			}
			int truncIndex = param.GetParam<int>("Use for truncation").Value;
			TestTruncation truncation = truncIndex == 0
				? TestTruncation.Pvalue
				: (truncIndex == 1 ? TestTruncation.BenjaminiHochberg : TestTruncation.PermutationBased);
			double threshold = param.GetParam<double>("Threshold value").Value;
			int sideInd = param.GetParam<int>("Side").Value;
			TestSide side;
			switch (sideInd){
				case 0:
					side = TestSide.Both;
					break;
				case 1:
					side = TestSide.Left;
					break;
				case 2:
					side = TestSide.Right;
					break;
				default:
					throw new Exception("Never get here.");
			}
			for (int i = 0; i < rcols.Length; i++){
				BaseVector r = mdata.Values.GetColumn(rcols[i]);
				BaseVector intens = icols[i] < mdata.ColumnCount
					? mdata.Values.GetColumn(icols[i])
					: new DoubleArrayVector(mdata.NumericColumns[icols[i] - mdata.ColumnCount]);
				double[] pvals = NumUtils.CalcSignificanceB(r.ToArray(), intens.ToArray(), side);
				string[][] fdr;
				switch (truncation){
					case TestTruncation.Pvalue:
						fdr = PerseusPluginUtils.CalcPvalueSignificance(pvals, threshold);
						break;
					case TestTruncation.BenjaminiHochberg:
						fdr = PerseusPluginUtils.CalcBenjaminiHochbergFdr(pvals, threshold, out double[] _);
						break;
					default:
						throw new Exception("Never get here.");
				}
				mdata.AddNumericColumn(mdata.ColumnNames[rcols[i]] + " Significance B", "", pvals);
				mdata.AddCategoryColumn(mdata.ColumnNames[rcols[i]] + " B significant", "", fdr);
			}
		}
		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			List<string> choice = mdata.ColumnNames;
			string[] choice2 = ArrayUtils.Concat(mdata.ColumnNames, mdata.NumericColumnNames);
			return
				new Parameters(new MultiChoiceParam("Ratio columns"){
					Values = choice,
					Help = "Ratio columns for which the Significance B should be calculated."
				}, new MultiChoiceParam("Intensity columns"){
					Values = choice2,
					Repeats = true,
					Help = "Intensity columns for which the Significance B should be calculated."
				}, new SingleChoiceParam("Side"){
					Values = new[]{"both", "right", "left"},
					Help =
						"'Both' stands for the two-sided test in which the the null hypothesis can be rejected regardless of the direction" +
						" of the effect. 'Left' and 'right' are the respective one sided tests."
				}, new SingleChoiceParam("Use for truncation"){
					Values = new[]{"P value", "Benjamini-Hochberg FDR"},
					Value = 1,
					Help =
						"Choose here whether the truncation should be based on the p values or if the Benjamini Hochberg correction for " +
						"multiple hypothesis testing should be applied."
				}, new DoubleParam("Threshold value", 0.05){
					Help =
						"Rows with a test result below this value are reported as significant. Depending on the choice made above this " +
						"threshold value is applied to the p value or to the Benjamini Hochberg FDR."
				});
		}
	}
}