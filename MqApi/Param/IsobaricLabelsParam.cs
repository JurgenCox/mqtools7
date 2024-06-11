﻿using MqApi.Num;
using MqApi.Util;
namespace MqApi.Param{
	[Serializable]
	public class IsobaricLabelsParam : Parameter<string[][]>{
		/// <summary>
		/// for xml serialization only
		/// </summary>
		public IsobaricLabelsParam() : this(""){
		}
		public IsobaricLabelsParam(string name) : this(name, new string[0][]){
		}
		public IsobaricLabelsParam(string name, string[][] value) : base(name){
			Value = value;
			Default = new string[Value.Length][];
			for (int i = 0; i < Value.Length; i++){
				Default[i] = new string[Value[i].Length];
				for (int j = 0; j < Value[i].Length; j++){
					Default[i][j] = Value[i][j];
				}
			}
		}
		protected IsobaricLabelsParam(string name, string help, string url, bool visible, string[][] value,
			string[][] default1) : base(name, help, url, visible, value, default1){
		}
		public override void Read(BinaryReader reader){
			base.Read(reader);
			Value = FileUtils.Read2DStringArray(reader);
			Default = FileUtils.Read2DStringArray(reader);
		}
		public override void Write(BinaryWriter writer){
			base.Write(writer);
			FileUtils.Write(Value, writer);
			FileUtils.Write(Default, writer);
		}
		public override string StringValue{
			get => StringUtils.Concat(";", ",", Value);
			set{
				if (value.Trim().Length == 0){
					Value = new string[0][];
					return;
				}
				string[] x = value.Split(';');
				Value = new string[x.Length][];
				for (int i = 0; i < x.Length; i++){
					Value[i] = x[i].Split(',');
				}
			}
		}
		public override bool IsModified => !ArrayUtils.EqualArraysOfArrays(Default, Value);
		public override void Clear(){
			Value = new string[0][];
		}
		public override float Height => 200f;
		public override ParamType Type => ParamType.Server;
		public override object Clone(){
			return new IsobaricLabelsParam(Name, Help, Url, Visible, Value, Default);
		}
	}
}