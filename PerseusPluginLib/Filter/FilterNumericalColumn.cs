﻿using System.Collections.Generic;
using MqApi.Calc;
using MqApi.Document;
using MqApi.Drawing;
using MqApi.Generic;
using MqApi.Matrix;
using MqApi.Num;
using MqApi.Param;
using MqApi.Util;
using MqUtil;
using PerseusPluginLib.Utils;
namespace PerseusPluginLib.Filter{
	public class FilterNumericalColumn : IMatrixProcessing{
		public bool HasButton => false;
		public Bitmap2 DisplayImage => null;
		public string HelpOutput => "The filtered matrix.";
		public string[] HelpSupplTables => new string[0];
		public int NumSupplTables => 0;
		public string Name => "Filter rows based on numerical/main column";
		public string Heading => "Filter rows";
		public bool IsActive => true;
		public float DisplayRank => 1;
		public string[] HelpDocuments => new string[0];
		public int NumDocuments => 0;
		public string Url
			=>
                "https://cox-labs.github.io/coxdocs/filternumericalcolumn.html";
		public string Description
			=>
				"Only those rows are kept that have a value in the numerical column fulfilling the " +
				"equation or inequality relation.";
		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			string[] selection = ArrayUtils.Concat(mdata.NumericColumnNames, mdata.ColumnNames);
			return
				new Parameters(ArrayUtils.Concat(PerseusUtils.GetNumFilterParams(selection),
					PerseusPluginUtils.CreateFilterModeParamNew(true)));
		}
		public int GetMaxThreads(Parameters parameters){
			return 1;
		}
		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			Relation[] relations =
				PerseusUtils.GetRelationsNumFilter(param, out string errString, out int[] colInds, out bool and);
			if (errString != null){
				processInfo.ErrString = errString;
				return;
			}
			double[][] rows = GetRows(mdata, colInds);
			List<int> valids = new List<int>();
			List<int> notvalids = new List<int>();
			for (int i = 0; i < rows.Length; i++){
				bool valid = PerseusUtil.IsValidRowNumFilter(rows[i], relations, and);
				if (!valid){
					notvalids.Add(i);
				}
			}
			if (param.GetParam<int>("Filter mode").Value == 2){
				supplTables = new[]{PerseusPluginUtils.CreateSupplTabSplit(mdata, notvalids.ToArray())};
			}
			PerseusPluginUtils.FilterRowsNew(mdata, param, GetValids(mdata, colInds, relations, and));
		}
		private static int[] GetValids(IMatrixData mdata, int[] colInds, Relation[] relations, bool and){
			double[][] rows = GetRows(mdata, colInds);
			List<int> valids = new List<int>();
			for (int i = 0; i < rows.Length; i++){
				bool valid = PerseusUtil.IsValidRowNumFilter(rows[i], relations, and);
				if (valid){
					valids.Add(i);
				}
			}
			return valids.ToArray();
		}
		private static double[][] GetRows(IMatrixData mdata, int[] colInds){
			double[][] cols = new double[colInds.Length][];
			for (int i = 0; i < cols.Length; i++){
				cols[i] = colInds[i] < mdata.NumericColumnCount
					? mdata.NumericColumns[colInds[i]]
					: ArrayUtils.ToDoubles(mdata.Values.GetColumn(colInds[i] - mdata.NumericColumnCount));
			}
			return ArrayUtils.Transpose(cols);
		}
	}
}