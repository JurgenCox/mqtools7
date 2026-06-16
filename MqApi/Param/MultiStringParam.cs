using System.Xml;
using MqApi.Matrix;
using MqApi.Num;
using MqApi.Util;
namespace MqApi.Param{
	public class MultiStringParam : Parameter<string[]>{
		/// <summary>
		/// When set to something other than <see cref="EditorType.None"/>, the GUI shows an
		/// "Edit" button next to the text area that opens the inline code editor (same editor as
		/// the external script <see cref="FileParam"/>). Used e.g. for the internal "Script text".
		/// </summary>
		public EditorType Edit{ get; set; }
		/// <summary>
		/// Matrix data passed to the inline editor so it can export the current table for
		/// Inspect / Test runs. Only relevant when <see cref="Edit"/> is not None.
		/// </summary>
		public IMatrixData Data{ get; set; }
		/// <summary>
		/// for xml serialization only
		/// </summary>
		public MultiStringParam() : this(""){
		}
		public MultiStringParam(string name) : this(name, new string[0]){
		}
		public MultiStringParam(string name, string[] value) : base(name){
			Value = value;
			Default = new string[Value.Length];
			for (int i = 0; i < Value.Length; i++){
				Default[i] = Value[i];
			}
		}
		protected MultiStringParam(string name, string help, string url, bool visible, string[] value,
			string[] default1) : base(name, help, url, visible, value, default1){
		}
		public override void Read(BinaryReader reader){
			base.Read(reader);
			Value = FileUtils.ReadStringArray(reader);
			Default = FileUtils.ReadStringArray(reader);
		}
		public override void Write(BinaryWriter writer){
			base.Write(writer);
			FileUtils.Write(Value, writer);
			FileUtils.Write(Default, writer);
		}
		// These are lines of (possibly multi-line script) text that routinely contain commas, so a comma
		// delimiter corrupts them whenever the value is copied through StringValue - e.g. when a template's
		// parameters are adapted onto a new matrix (PerseusMainControl.ApplyTemplateValues does
		// def.StringValue = tmpl.StringValue). A line never contains a newline, so newline is the only
		// lossless separator for an array of lines.
		public override string StringValue{
			get => Value == null ? "" : string.Join("\n", Value);
			set{
				if (string.IsNullOrEmpty(value)){
					Value = new string[0];
					return;
				}
				Value = value.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
			}
		}
		public override bool IsModified => !ArrayUtils.EqualArrays(Default, Value);
		public override void Clear(){
			Value = new string[0];
		}
		public override float Height => 150f;
		public override ParamType Type => ParamType.Server;
		// The base Parameter<string[]>.WriteXml/ReadXml serialize a string[] as a single
		// whitespace-separated <Value> (XSD list semantics): every element that contains a space is
		// split into separate words and all line boundaries are lost. That silently corrupts any
		// multi-line text stored here - e.g. the inline "Script text" of the Python/R activities, where
		// a comment line like "# ------------" comes back as a bare "------------" and breaks the script.
		// Serialize each element as its own <Item> child instead (the pattern used for choice option
		// lists); this preserves spaces and line structure and XML-escapes special characters. Files
		// written in the old single-<Value> form are still read, for backward compatibility.
		public override void WriteXml(XmlWriter writer){
			WriteBasicAttributes(writer);
			writer.WriteValues("Values", Value ?? new string[0]);
		}
		public override void ReadXml(XmlReader reader){
			ReadBasicAttributes(reader);
			bool isEmpty = reader.IsEmptyElement;
			reader.ReadStartElement();
			if (isEmpty){
				Value = new string[0];
				return;
			}
			if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "Value"){
				// Legacy format: a single whitespace-joined <Value> list.
				Value = (string[]) reader.ReadElementContentAs(typeof(string[]), null, "Value", "");
			} else{
				Value = reader.ReadInto(new List<string>()).ToArray();
			}
			reader.ReadEndElement();
		}
		public override object Clone(){
			return new MultiStringParam(Name, Help, Url, Visible, Value, Default){Edit = Edit, Data = Data};
		}
	}
}