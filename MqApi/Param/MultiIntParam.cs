using MqApi.Num;
using MqApi.Util;
namespace MqApi.Param{
	public class MultiIntParam : Parameter<int[]>{
		/// <summary>
		/// for xml serialization only
		/// </summary>
		public MultiIntParam() : this(""){
		}
		public MultiIntParam(string name) : this(name, []){
		}
		public MultiIntParam(string name, int[] value) : base(name){
			Value = value;
			Default = ArrayUtils.FillArray(0, Value.Length);
			for (int i = 0; i < Value.Length; i++){
				Default[i] = Value[i];
			}
		}
		protected MultiIntParam(string name, string help, string url, bool visible, int[] value,
			int[] default1) : base(name, help, url, visible, value, default1){
		}
		public override void Read(BinaryReader reader){
			base.Read(reader);
			Value = FileUtils.ReadInt32Array(reader);
			Default = FileUtils.ReadInt32Array(reader);
		}
		public override void Write(BinaryWriter writer){
			base.Write(writer);
			FileUtils.Write(Value, writer);
			FileUtils.Write(Default, writer);
		}
		public override string StringValue{
			get => StringUtils.Concat(";", Value);
			set{
				if (value.Trim().Length == 0){
					Value = [];
					return;
				}
				Value = ParseInts(value);
			}
		}
		public static int[] ParseInts(string s){
			string[] x = s.Split(';');
			int[] y = new int[x.Length];
			for (int i = 0; i < y.Length; i++){
				bool success = Parser.TryInt(x[i], out int val);
				y[i] = success ? val : 0;
			}
			return y;
		}
		public override bool IsModified => !ArrayUtils.EqualArrays(Default, Value);
		public override void Clear(){
			Value = [];
		}
		public override float Height => 150f;
		public override ParamType Type => ParamType.Server;
		public override object Clone(){
			return new MultiIntParam(Name, Help, Url, Visible, Value, Default);
		}
	}
}