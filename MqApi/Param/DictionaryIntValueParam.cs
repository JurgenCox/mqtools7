﻿using System.Xml;
using System.Xml.Serialization;
using MqApi.Util;
namespace MqApi.Param{
	[Serializable]
	public class DictionaryIntValueParam : Parameter<Dictionary<string, int>>{
		protected string[] keys;
		public int DefaultValue{ get; set; }
		public virtual string[] Keys{
			get => keys;
			set => keys = value;
		}
		/// <summary>
		/// for xml serialization only
		/// </summary>
		public DictionaryIntValueParam() : this("", new Dictionary<string, int>(), new string[0]){
		}
		public DictionaryIntValueParam(string name, Dictionary<string, int> value, string[] keys) : base(name){
			Value = value;
			Default = new Dictionary<string, int>();
			foreach (KeyValuePair<string, int> pair in value){
				Default.Add(pair.Key, pair.Value);
			}
			this.keys = keys;
		}
		protected DictionaryIntValueParam(string name, string help, string url, bool visible,
			Dictionary<string, int> value, Dictionary<string, int> default1, string[] keys, int defaultValue) : base(
			name, help, url, visible, value, default1){
			Keys = keys;
			DefaultValue = defaultValue;
		}
		public override void Read(BinaryReader reader){
			base.Read(reader);
			Value = FileUtils.ReadDictionaryStringInt(reader);
			Default = FileUtils.ReadDictionaryStringInt(reader);
		}
		public override void Write(BinaryWriter writer){
			base.Write(writer);
			FileUtils.Write(Value, writer);
			FileUtils.Write(Default, writer);
		}
		public override string StringValue{
			get => StringUtils.ToString(Value);
			set => Value = DictionaryFromString(value);
		}
		public static Dictionary<string, int> DictionaryFromString(string s){
			Dictionary<string, int> result = new Dictionary<string, int>();
			foreach (string s1 in s.Split('\r')){
				string[] w = s1.Trim().Split('\t');
				result.Add(w[0], Parser.Int(w[1]));
			}
			return result;
		}
		public override bool IsModified{
			get{
				if (Value.Count != Default.Count){
					return true;
				}
				foreach (string k in Value.Keys){
					if (!Default.ContainsKey(k)){
						return true;
					}
					if (Default[k] != Value[k]){
						return true;
					}
				}
				return false;
			}
		}
		public override void Clear(){
			Value = new Dictionary<string, int>();
		}
		public override ParamType Type => ParamType.Server;
		public override void WriteXml(XmlWriter writer){
			WriteBasicAttributes(writer);
			SerializableDictionary<string, int> value = new SerializableDictionary<string, int>(Value);
			XmlSerializer serializer = new XmlSerializer(value.GetType());
			writer.WriteStartElement("Value");
			serializer.Serialize(writer, value);
			writer.WriteEndElement();
			writer.WriteStartElement("Keys");
			foreach (string key in Keys){
				writer.WriteElementString("Key", key);
			}
			writer.WriteEndElement();
		}
		public override void ReadXml(XmlReader reader){
			ReadBasicAttributes(reader);
			XmlSerializer serializer = new XmlSerializer(typeof(SerializableDictionary<string, int>));
			reader.ReadStartElement();
			reader.ReadStartElement("Value");
			Value = ((SerializableDictionary<string, int>) serializer.Deserialize(reader)).ToDictionary();
			reader.ReadEndElement();
			Keys = reader.ReadInto(new List<string>()).ToArray();
			reader.ReadEndElement();
		}
		public override object Clone(){
			return new DictionaryIntValueParam(Name, Help, Url, Visible, Value, Default, Keys, DefaultValue);
		}
	}
}