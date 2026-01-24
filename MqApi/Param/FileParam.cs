namespace MqApi.Param{
	public class FileParam : Parameter<string>{
		public string Filter{ get; set; }
		public Func<string, string> ProcessFileName{ get; set; }
		public bool Save{ get; set; }
		public EditorType Edit { get; set; }

		/// <summary>
		/// for xml serialization only
		/// </summary>
		public FileParam() : this(""){
		}
		public FileParam(string name) : this(name, ""){
		}
		public FileParam(string name, string value) : base(name){
			Value = value;
			Default = value;
			Filter = null;
			Save = false;
			Edit = EditorType.None;
		}
		protected FileParam(string name, string help, string url, bool visible, string value, string default1,
			string filter, Func<string, string> processFileName, bool save, EditorType edit = EditorType.None) : base(name, help, url, visible, value,
			default1){
			Filter = filter;
			ProcessFileName = processFileName;
			Save = save;
			Edit = edit;
		}
		public override void Read(BinaryReader reader){
			base.Read(reader);
			Value = reader.ReadString();
			Default = reader.ReadString();
		}
		public override void Write(BinaryWriter writer){
			base.Write(writer);
			writer.Write(Value);
			writer.Write(Default);
		}
		public override string StringValue{
			get => Value;
			set => Value = value;
		}
		public override void Clear(){
			Value = "";
		}
		public override ParamType Type => ParamType.Server;
		public override object Clone(){
			return new FileParam(Name, Help, Url, Visible, Value, Default, Filter, ProcessFileName, Save, Edit);
		}
	}
}