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
		public override string StringValue{
			get => StringUtils.Concat(",", Value);
			set{
				if (value.Trim().Length == 0){
					Value = new string[0];
					return;
				}
				Value = value.Split(',');
			}
		}
		public override bool IsModified => !ArrayUtils.EqualArrays(Default, Value);
		public override void Clear(){
			Value = new string[0];
		}
		public override float Height => 150f;
		public override ParamType Type => ParamType.Server;
		public override object Clone(){
			return new MultiStringParam(Name, Help, Url, Visible, Value, Default){Edit = Edit, Data = Data};
		}
	}
}