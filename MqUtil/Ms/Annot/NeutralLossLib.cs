using System.Text;
using System.Xml.Serialization;
using MqApi.Util;
using MqUtil.Mol;
namespace MqUtil.Ms.Annot{
	[Serializable, System.Diagnostics.DebuggerStepThrough, XmlType(AnonymousType = true),
	 XmlRoot(ElementName = "neutral loss", IsNullable = false)]
	public class NeutralLossLib : ICloneable{
		private string formula;
		private string name2;
		private double mass = double.NaN;
		private int charge;
		private int count = 1;

		protected NeutralLossLib(){
			// default constructor for serialization
		}

		public NeutralLossLib(string name, double mass){
			this.mass = mass;
			Formula = name;
			name2 = GetName2(formula);
		}

		public NeutralLossLib(AminoAcid aa) : this(Convert.ToString(aa.Letter), aa.MonoIsotopicMass){
			By = aa.ToString();
		}

		public NeutralLossLib(string key) : this(key, Molecule.CalcMonoMass(key)) {}

		public NeutralLossLib(string key, string parent) : this(key){
			By = parent;
		}

		public NeutralLossLib(BinaryReader reader){
			charge = reader.ReadInt32();
			count = reader.ReadInt32();
			Formula = reader.ReadString();
			name2 = reader.ReadString();
			mass = reader.ReadDouble();
		}

		public double Mass{
			get{
				if (double.IsNaN(mass)){
					mass = Molecule.CalcMonoMass(formula);
				}
				return count*mass;
			}
		}
		[XmlAttribute("formula")]
		public string Formula { get => formula;
			set => formula = value;
		}
		[XmlIgnore]
		private int Charge { get => charge;
			set => charge = value;
		}
		[XmlIgnore]
		public int Count { get => count;
			set => count = value;
		}
		[XmlIgnore]
		public string By { get; set; }

		private static string GetName2(string name){
			StringBuilder s = new StringBuilder();
			int len = name.Length;
			for (int i = 0; i < len; i++){
				char c = name[i];
				if (char.IsNumber(c)){
					s.Append(StringUtils.ToSubscript(int.Parse(name.Substring(i, 1)), false));
				} else{
					s.Append(c);
				}
			}
			return s.ToString();
		}

		public string ToString2(){
			if (string.IsNullOrEmpty(name2)){
				name2 = GetName2(formula);
			}
			return count == 1 ? name2 : count + name2;
		}

		public string ToStringBy(){
			return !string.IsNullOrEmpty(By) ? $"{ToString2()}(by {By})" : ToString2();
		}

		public void Write(BinaryWriter writer){
			writer.Write(Charge);
			writer.Write(Count);
			writer.Write(Formula);
			writer.Write(name2);
			writer.Write(mass);
		}

		public override string ToString(){
			if (count == 1){
				return Formula;
			}
			return count + Formula;
		}

		public override bool Equals(object anObject){
			if (this == anObject){
				return true;
			}
			if (anObject is NeutralLossLib){
				NeutralLossLib other = (NeutralLossLib) anObject;
				if (other.Mass != Mass){
					return false;
				}
				if (other.Charge != Charge){
					return false;
				}
				if (other.Count != Count){
					return false;
				}
				if (!other.Formula.Equals(Formula)){
					return false;
				}
				return true;
			}
			return false;
		}

		public override int GetHashCode(){
			int h = 0;
			h = 31*h + (int) Mass;
			h = 31*h + Formula.GetHashCode();
			h = 31*h + Charge;
			h = 31*h + Count;
			return h;
		}

		public object Clone(){
			return new NeutralLossLib(Formula){Count = count, Charge = charge, By = By};
		}
	}
}