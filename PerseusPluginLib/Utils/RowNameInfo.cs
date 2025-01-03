﻿using System;
using System.IO;
using MqApi.Generic;
using MqApi.Num;
using MqApi.Util;
namespace PerseusPluginLib.Utils{
	public class RowNameInfo : INameInfo{
		public bool CutNames{ get; set; }
		public bool CutNames2{ get; set; }
		public bool CutDescriptions{ get; set; }
		public int NameColumnIndex{ get; set; }
		public int Name2ColumnIndex{ get; set; }
		public int DescriptionColumnIndex{ get; set; }
		private readonly IDataWithAnnotationColumns mdata;
		public RowNameInfo(IDataWithAnnotationColumns mdata){
			this.mdata = mdata;
		}
		public RowNameInfo(BinaryReader reader, IDataWithAnnotationColumns mdata){
			this.mdata = mdata;
			CutNames = reader.ReadBoolean();
			CutNames2 = reader.ReadBoolean();
			CutDescriptions = reader.ReadBoolean();
			NameColumnIndex = reader.ReadInt32();
			Name2ColumnIndex = reader.ReadInt32();
			DescriptionColumnIndex = reader.ReadInt32();
		}
		public void Write(BinaryWriter writer){
			writer.Write(CutNames);
			writer.Write(CutNames2);
			writer.Write(CutDescriptions);
			writer.Write(NameColumnIndex);
			writer.Write(Name2ColumnIndex);
			writer.Write(DescriptionColumnIndex);
		}
		public string[] GetRowNames(){
			string[] result = new string[mdata.RowCount];
			for (int i = 0; i < result.Length; i++){
				result[i] = GetRowName(i);
			}
			return result;
		}
		public string[] GetNameSelection(){
			return ArrayUtils.Concat(mdata.StringColumnNames, mdata.CategoryColumnNames);
		}
		private string GetRowName(int ind, int nameColumnIndex, bool cutNames){
			if (nameColumnIndex < 0){
				return "";
			}
			if (nameColumnIndex < mdata.StringColumnCount){
				string[] x = mdata.StringColumns[nameColumnIndex];
				if (ind >= 0 && ind < x.Length){
					if (cutNames && x[ind].Contains(";")){
						return x[ind].Substring(0, x[ind].IndexOf(';'));
					}
					return x[ind];
				}
			}
			int indi = nameColumnIndex - mdata.StringColumnCount;
			if (indi < mdata.CategoryColumnCount && indi >= 0){
				if (ind >= 0 && ind < mdata.RowCount){
					string[] w = mdata.GetCategoryColumnEntryAt(indi, ind);
					if (cutNames){
						return w.Length > 0 ? w[0] : "";
					}
					return StringUtils.Concat(";", w);
				}
			}
			return "";
		}
		public string GetRowName(int ind){
			return GetRowName(ind, NameColumnIndex, CutNames);
		}
		public string GetRowName2(int ind){
			return GetRowName(ind, Name2ColumnIndex, CutNames2);
		}
		public string GetRowDescription(int ind){
			return GetRowName(ind, DescriptionColumnIndex, CutDescriptions);
		}
	}
}