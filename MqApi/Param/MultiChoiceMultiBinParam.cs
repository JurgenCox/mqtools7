using System.Xml;
using MqApi.Num;
using MqApi.Util;
namespace MqApi.Param{
	[Serializable]
	public class MultiChoiceMultiBinParam : Parameter<int[][]>{
		public IList<string> Values{ get; set; }
		public IList<string> Bins{ get; set; }
		/// <summary>
		/// for xml serialization only
		/// </summary>
		public MultiChoiceMultiBinParam() : this(""){
		}
		public MultiChoiceMultiBinParam(string name) : this(name, new int[0][]){
		}
		public MultiChoiceMultiBinParam(string name, int[][] value) : base(name){
			Value = value;
			Default = new int[value.Length][];
			for (int i = 0; i < value.Length; i++){
				Default[i] = new int[value[i].Length];
				for (int j = 0; j < value[i].Length; j++){
					Default[i][j] = value[i][j];
				}
			}
			Values = new string[0];
			Bins = new string[0];
		}
		protected MultiChoiceMultiBinParam(string name, string help, string url, bool visible, int[][] value,
			int[][] default1, IList<string> values, IList<string> bins) : base(name, help, url, visible, value,
			default1){
			Values = values;
			Bins = bins;
		}
		public override void Read(BinaryReader reader){
			base.Read(reader);
			Value = FileUtils.Read2DInt32Array(reader);
			Default = FileUtils.Read2DInt32Array(reader);
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
					Value = new int[0][];
					return;
				}
				string[] q = value.Trim().Split(';');
				Value = new int[q.Length][];
				for (int i = 0; i < Value.Length; i++){
					string[] r = q[i].Trim().Split();
					Value[i] = new int[r.Length];
					for (int j = 0; j < r.Length; j++){
						Value[i][j] = Parser.Int(r[j]);
					}
				}
			}
		}
		public string[][] SelectedValues{
			get{
				string[][] result = new string[Value.Length][];
				for (int i = 0; i < result.Length; i++){
					result[i] = ArrayUtils.SubArray(Values, Value[i]);
				}
				return result;
			}
		}
		public string[][] SelectedValues2{
			get{
				SetValueFromControl();
				return SelectedValues;
			}
		}
		public override bool IsModified => !ArrayUtils.EqualArraysOfArrays(Value, Default);
		public override void Clear(){
			Value = new int[0][];
		}
		public override float Height => 310f;
		public override void ResetSubParamValues(){
			Value = Default;
		}
		public override void ResetSubParamDefaults(){
			Default = Value;
		}
		public override ParamType Type => ParamType.Server;
		public override void ReadXml(XmlReader reader){
			ReadBasicAttributes(reader);
			reader.ReadStartElement();
			Value = reader.ReadJagged2DArrayInto(new List<List<int>>()).Select(x => x.ToArray()).ToArray();
			Values = reader.ReadInto(new List<string>()).ToArray();
			Bins = reader.ReadInto(new List<string>()).ToArray();
			reader.ReadEndElement();
		}
		public override void WriteXml(XmlWriter writer){
			WriteBasicAttributes(writer);
			writer.WriteStartElement("Value");
			foreach (int[] labels in Value){
				writer.WriteStartElement("Items");
				foreach (int label in labels){
					writer.WriteStartElement("Item");
					writer.WriteValue(label);
					writer.WriteEndElement();
				}
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
			writer.WriteValues("Values", Values);
			writer.WriteValues("Bins", Bins);
		}
		public override object Clone(){
			return new MultiChoiceMultiBinParam(Name, Help, Url, Visible, Value, Default, Values, Bins);
		}
	}
}