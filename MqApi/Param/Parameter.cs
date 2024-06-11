using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
namespace MqApi.Param{
	public delegate void ValueChangedHandler();
	[Serializable]
	public abstract class Parameter : IXmlSerializable, ICloneable{
		public const int paramHeight = 23;
		[field: NonSerialized] public event ValueChangedHandler ValueChanged;
		public string Name{ get; protected set; }
		public string Help{ get; set; }
		public string Url{ get; set; }
		public bool Visible{ get; set; }
		public virtual ParamType Type => ParamType.WinForms;
		protected Parameter() : this(""){
		} // only for xml serialization
		internal Parameter(string name) : this(name, "", "", true){
		}
		protected Parameter(string name, string help, string url, bool visible){
			Name = name;
			Help = help;
			Url = url;
			Visible = visible;
		}
		public virtual void Read(BinaryReader reader){
			Name = reader.ReadString();
			Help = reader.ReadString();
			Url = reader.ReadString();
			Visible = reader.ReadBoolean();
		}
		public virtual void Write(BinaryWriter writer){
			writer.Write(Name);
			writer.Write(Help);
			writer.Write(Url);
			writer.Write(Visible);
		}
		public virtual void SetValueFromControl(){
		}
		public virtual void UpdateControlFromValue(){
		}
		public virtual object CreateControl(){
			return null;
		}
		public virtual void Drop(string x){
		}
		public abstract string StringValue{ get; set; }
		public abstract void ResetValue();
		public abstract void ResetDefault();
		public abstract void Clear();
		public abstract bool IsModified{ get; }
		public virtual bool IsDropTarget => false;
		public virtual float Height => paramHeight;
		public virtual string[] Markup =>
			new[]{"<parameter" + " name=\"" + Name + "\" value=\"" + StringValue + "\"></parameter>"};
		protected void ValueHasChanged(){
			ValueChanged?.Invoke();
		}
		public ValueChangedHandler[] GetPropertyChangedHandlers(){
			if (ValueChanged == null){
				return new ValueChangedHandler[0];
			}
			return ValueChanged.GetInvocationList().OfType<ValueChangedHandler>().ToArray();
		}
		public XmlSchema GetSchema(){
			return null;
		}
		public abstract void ReadXml(XmlReader reader);
		public abstract void WriteXml(XmlWriter writer);
		public abstract object Clone();
	}
	[Serializable]
	public abstract class Parameter<T> : Parameter{
		public T Value{ get; set; }
		public T Default{ get; set; }
		protected Parameter(){
		}
		protected Parameter(string name) : base(name){
		}
		protected Parameter(string name, string help, string url, bool visible, T value, T default1) : base(name, help,
			url, visible){
			Value = value;
			Default = default1;
		}
		public sealed override void ResetValue(){
			if (Value is ICloneable){
				Value = (T) ((ICloneable) Default).Clone();
			} else{
				Value = Default;
			}
			ResetSubParamValues();
		}
		public sealed override void ResetDefault(){
			if (Value is ICloneable){
				Default = (T) ((ICloneable) Value).Clone();
			} else{
				Default = Value;
			}
			ResetSubParamDefaults();
		}
		public override bool IsModified => Value != null && !Value.Equals(Default);
		public virtual void ResetSubParamValues(){
		}
		public virtual void ResetSubParamDefaults(){
		}
		public T Value2{
			get{
				SetValueFromControl();
				return Value;
			}
		}
		public void ReadBasicAttributes(XmlReader reader){
			Name = reader["Name"];
		}
		public override void ReadXml(XmlReader reader){
			ReadBasicAttributes(reader);
			bool valueExists = !reader.IsEmptyElement;
			reader.ReadStartElement();
			if (valueExists){
				Value = (T) reader.ReadElementContentAs(typeof(T), null, "Value", "");
				reader.ReadEndElement();
			}
		}
		protected void WriteBasicAttributes(XmlWriter writer){
			writer.WriteAttributeString("Type", GetType().AssemblyQualifiedName);
			writer.WriteAttributeString("Name", Name);
		}
		public override void WriteXml(XmlWriter writer){
			WriteBasicAttributes(writer);
			if (Value != null){
				writer.WriteStartElement("Value");
				writer.WriteValue(Value);
				writer.WriteEndElement();
			}
		}
	}
}