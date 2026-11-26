using System;
using System.Collections.Generic;
using MqApi.Document;
using MqApi.Drawing;
using MqApi.Generic;
using MqApi.Matrix;
using MqApi.Num;
using MqApi.Param;
using MqApi.Util;
namespace PerseusPluginLib.Rearrange{
	public class MergeColumns : IMatrixProcessing{
		public string Category => IMatrixProcessingCategories.DataHandling;
		public bool HasButton => false;
		public Bitmap2 DisplayImage => null;
		public string Description
			=> "Merge the values of several columns into a single new column. Rows that are empty in one of the " +
				"selected columns are filled with the value found in the other selected columns.";
		public string HelpOutput => "One new column containing the merged values of the selected columns.";
		public string[] HelpSupplTables => new string[0];
		public int NumSupplTables => 0;
		public string Name => "Merge columns";
		public string Heading => "Matrix structure operations";
		public bool IsActive => true;
		public float DisplayRank => 17.6f;
		public string[] HelpDocuments => new string[0];
		public int NumDocuments => 0;
		public string Url => "";
		public int GetMaxThreads(Parameters parameters){
			return 1;
		}
		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			List<Parameters> subParams = new List<Parameters>{
				GetCategoricalSubParams(mdata), GetTextSubParams(mdata), GetNumericalSubParams(mdata)
			};
			return new Parameters(new Parameter[]{
				new SingleChoiceWithSubParams("Column type"){
					Values = new[]{"Categorical", "Text", "Numerical"},
					Help = "Type of the columns that should be merged. All merged columns have to be of the same type.",
					SubParams = subParams,
					ParamNameWidth = 136,
					TotalWidth = 731
				}
			});
		}
		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			ParameterWithSubParams<int> sp = param.GetParamWithSubParams<int>("Column type");
			Parameters p = sp.GetSubParameters();
			int[] colInds = p.GetParam<int[]>("Columns").Value;
			if (colInds.Length == 0){
				processInfo.ErrString = "Please select at least one column.";
				return;
			}
			bool removeSource = p.GetParam<bool>("Remove source columns").Value;
			switch (sp.Value){
				case 0:
					MergeCategorical(mdata, colInds, p);
					if (removeSource){
						RemoveColumns(colInds, mdata.RemoveCategoryColumnAt);
					}
					break;
				case 1:
					MergeText(mdata, colInds, p);
					if (removeSource){
						RemoveColumns(colInds, mdata.RemoveStringColumnAt);
					}
					break;
				case 2:
					MergeNumerical(mdata, colInds, p);
					if (removeSource){
						RemoveColumns(colInds, mdata.RemoveNumericColumnAt);
					}
					break;
				default:
					throw new Exception("Never get here");
			}
		}
		private static void MergeCategorical(IMatrixData mdata, IList<int> colInds, Parameters p){
			string[][][] cols = new string[colInds.Count][][];
			for (int i = 0; i < colInds.Count; i++){
				cols[i] = mdata.GetCategoryColumnAt(colInds[i]);
			}
			bool keepFirst = p.GetParam<int>("If multiple values").Value == 1;
			string[][] result = new string[mdata.RowCount][];
			for (int row = 0; row < result.Length; row++){
				List<string> terms = new List<string>();
				foreach (string[][] col in cols){
					string[] t = col[row];
					if (t == null || t.Length == 0){
						continue;
					}
					foreach (string s in t){
						if (!string.IsNullOrEmpty(s) && !terms.Contains(s)){
							terms.Add(s);
						}
					}
					if (keepFirst && terms.Count > 0){
						break;
					}
				}
				terms.Sort();
				result[row] = terms.ToArray();
			}
			mdata.AddCategoryColumn(GetNewName(p, mdata.CategoryColumnNames, colInds), "", result);
		}
		private static void MergeText(IMatrixData mdata, IList<int> colInds, Parameters p){
			bool keepFirst = p.GetParam<int>("If multiple values").Value == 1;
			string separator = p.GetParam<string>("Separator").Value;
			bool unique = p.GetParam<bool>("Remove duplicate values").Value;
			string[][] cols = new string[colInds.Count][];
			for (int i = 0; i < colInds.Count; i++){
				cols[i] = mdata.StringColumns[colInds[i]];
			}
			string[] result = new string[mdata.RowCount];
			for (int row = 0; row < result.Length; row++){
				List<string> values = new List<string>();
				foreach (string[] col in cols){
					string v = col[row];
					if (string.IsNullOrEmpty(v)){
						continue;
					}
					if (keepFirst){
						values.Add(v);
						break;
					}
					foreach (string part in Split(v, separator)){
						if (part.Length == 0 || unique && values.Contains(part)){
							continue;
						}
						values.Add(part);
					}
				}
				result[row] = StringUtils.Concat(separator, values);
			}
			mdata.AddStringColumn(GetNewName(p, mdata.StringColumnNames, colInds), "", result);
		}
		private static void MergeNumerical(IMatrixData mdata, IList<int> colInds, Parameters p){
			int combination = p.GetParam<int>("If multiple values").Value;
			double[][] cols = new double[colInds.Count][];
			for (int i = 0; i < colInds.Count; i++){
				cols[i] = mdata.NumericColumns[colInds[i]];
			}
			double[] result = new double[mdata.RowCount];
			for (int row = 0; row < result.Length; row++){
				List<double> values = new List<double>();
				foreach (double[] col in cols){
					double v = col[row];
					if (double.IsNaN(v)){
						continue;
					}
					values.Add(v);
					if (combination == 0){
						break;
					}
				}
				result[row] = Combine(values, combination);
			}
			mdata.AddNumericColumn(GetNewName(p, mdata.NumericColumnNames, colInds), "", result);
		}
		private static double Combine(IList<double> values, int combination){
			if (values.Count == 0){
				return double.NaN;
			}
			switch (combination){
				case 0:
					return values[0];
				case 1:
					return ArrayUtils.Mean(values);
				case 2:
					return ArrayUtils.Median(values);
				case 3:
					return ArrayUtils.Sum(values);
				case 4:
					return ArrayUtils.Min(values);
				case 5:
					return ArrayUtils.Max(values);
				default:
					throw new Exception("Never get here");
			}
		}
		private static IEnumerable<string> Split(string value, string separator){
			return separator.Length == 0
				? new[]{value}
				: value.Split(new[]{separator}, StringSplitOptions.None);
		}
		private static string GetNewName(Parameters p, IList<string> names, IList<int> colInds){
			string name = p.GetParam<string>("Name of new column").Value.Trim();
			return name.Length > 0 ? name : StringUtils.Concat("_", names.SubArray(colInds));
		}
		private static void RemoveColumns(IList<int> colInds, Action<int> remove){
			int[] sorted = new int[colInds.Count];
			colInds.CopyTo(sorted, 0);
			Array.Sort(sorted);
			for (int i = sorted.Length - 1; i >= 0; i--){
				remove(sorted[i]);
			}
		}
		private static Parameters GetCategoricalSubParams(IMatrixData mdata){
			return new Parameters(
				new MultiChoiceParam("Columns"){
					Values = mdata.CategoryColumnNames,
					Help = "The categorical columns whose terms should end up in one column."
				},
				new SingleChoiceParam("If multiple values", 0){
					Values = new[]{"Collect all terms", "Take terms from first non-empty column"},
					Help = "What to do with rows that have terms in more than one of the selected columns."
				}, new StringParam("Name of new column", ""){
					Help = "Leave empty to name the new column after the merged columns."
				}, new BoolParam("Remove source columns", false));
		}
		private static Parameters GetTextSubParams(IMatrixData mdata){
			return new Parameters(
				new MultiChoiceParam("Columns"){
					Values = mdata.StringColumnNames,
					Help = "The text columns whose values should end up in one column."
				},
				new SingleChoiceParam("If multiple values", 0){
					Values = new[]{"Concatenate", "Take value of first non-empty column"},
					Help = "What to do with rows that have values in more than one of the selected columns."
				}, new StringParam("Separator", ";"){
					Help = "Separator between the values that are concatenated. It is also used to split the " +
							"existing values when duplicates are removed."
				}, new BoolParam("Remove duplicate values", true),
				new StringParam("Name of new column", ""){
					Help = "Leave empty to name the new column after the merged columns."
				}, new BoolParam("Remove source columns", false));
		}
		private static Parameters GetNumericalSubParams(IMatrixData mdata){
			return new Parameters(
				new MultiChoiceParam("Columns"){
					Values = mdata.NumericColumnNames,
					Help = "The numerical columns whose values should end up in one column."
				},
				new SingleChoiceParam("If multiple values", 0){
					Values = new[]{"Take value of first valid column", "Mean", "Median", "Sum", "Minimum", "Maximum"},
					Help = "What to do with rows that have valid values in more than one of the selected columns."
				}, new StringParam("Name of new column", ""){
					Help = "Leave empty to name the new column after the merged columns."
				}, new BoolParam("Remove source columns", false));
		}
	}
}
