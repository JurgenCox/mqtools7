using System;
using MqApi.Document;
using MqApi.Drawing;
using MqApi.Generic;
using MqApi.Matrix;
using MqApi.Num;
using MqApi.Param;
using MqUtil.Num;
using PerseusPluginLib.Utils;
namespace PerseusPluginLib.Filter{
	public class FilterRandomRows : IMatrixProcessing{
		public bool HasButton => false;
		public string Category => IMatrixProcessingCategories.DataAnnotation;
        public Bitmap2 DisplayImage => null;
		public string Description => "A given number of rows is kept based on random decisions.";
		public string HelpOutput => "The filtered matrix.";
		public string[] HelpSupplTables => new string[0];
		public int NumSupplTables => 0;
		public string Name => "Filter rows based on random sampling";
		public string Heading => "Filter rows";
		public bool IsActive => true;
		public float DisplayRank => 10;
		public string[] HelpDocuments => new string[0];
		public int NumDocuments => 0;
		public int GetMaxThreads(Parameters parameters){
			return 1;
		}
		public string Url
			=> "https://cox-labs.github.io/coxdocs/filterrandomrows.html";
		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			return
				new Parameters(new IntParam("Number of rows", mdata.RowCount),
					PerseusPluginUtils.CreateFilterModeParam(true));
		}
		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
			if (param.GetParam<int>("Filter mode").Value == 2){
				supplTables = new[]{PerseusPluginUtils.CreateSupplTab(mdata)};
			}
			int nrows = param.GetParam<int>("Number of rows").Value;
			nrows = Math.Min(nrows, mdata.RowCount);
			Random2 rand = new Random2(7);
			int[] rows = rand.NextPermutation(mdata.RowCount).SubArray(nrows);
			PerseusPluginUtils.FilterRowsNew(mdata, param, rows);
		}
	}
}