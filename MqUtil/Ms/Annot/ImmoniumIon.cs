using System.Xml.Serialization;
using MqApi.Util;
using MqUtil.Mol;
namespace MqUtil.Ms.Annot{
	[Serializable, System.Diagnostics.DebuggerStepThrough, XmlType(AnonymousType = true),
	 XmlRoot(ElementName = "immonium ion", IsNullable = false)]
	public class ImmoniumIon{
		//public static ImmoniumIon Carboxymethyl = new ImmoniumIon(134.0276, 'C', "CmC", "Carboxymethyl");
		//public static ImmoniumIon Pyridylethyl = new ImmoniumIon(181.0799, 'C', "PeC", "Pyridylethyl");
		//public static ImmoniumIon AcrylamideAdduct = new ImmoniumIon(147.0643, 'C', "AaC", "Acrylamide adduct");        
		private string name;
		private string name2;
		private string aa;
		private double mz = double.NaN;

		protected ImmoniumIon(){
			//default constructor for serialization
		}

		public ImmoniumIon(string aminoacid){
			aa = aminoacid;
			if (char.TryParse(aminoacid, out char letter)){
				AminoAcid amino = AminoAcids.FromLetter(letter);
				name = Parser.ToString(amino.Letter);
				name2 = amino.Name ?? Parser.ToString(amino.Letter);
			}
		}

		public ImmoniumIon(string aminoacid, string mod, string name, string name2){
			aa = aminoacid;
			if (char.TryParse(aminoacid, out char letter)){
				this.name = name;
				this.name2 = name2;
				Modification = mod;
			}
		}

		public double Mz{
			get{
				if (double.IsNaN(mz)){
					double mass = AminoAcid.MonoIsotopicMass - Molecule.CalcMonoMass("CO");
					if (HasModification){
						mass += Molecule.CalcMonoMass(Modification);
					}
					mz = Molecule.ConvertToMz(mass, 1);
				}
				return mz;
			}
		}
		private AminoAcid AminoAcid{
			get{
				if (!char.TryParse(Letter, out char letter)) {
					if (Letter.Length > 1) {
						letter = Letter[0];
					}
				}
				return AminoAcids.FromLetter(letter);
			}
		}
		[XmlAttribute("name")]
		public string Name{
			get{
				if (string.IsNullOrEmpty(name)){
					name = Parser.ToString(AminoAcid.Letter);
				}
				return "IM " + name;
			}
			set => name = value.Replace("IM ", "");
		}
		[XmlAttribute("name2")]
		public string Name2{
			get{
				if (string.IsNullOrEmpty(name2)){
					name2 = AminoAcid.Name ?? Parser.ToString(AminoAcid.Letter);
				}
				return "IM " + name2;
			}
			set => name2 = value.Replace("IM ", "");
		}
		[XmlAttribute("letter")]
		public string Letter { get => aa;
			set => aa = value;
		}
		[XmlAttribute("modification")]
		public string Modification { get; set; }
		public bool HasModification => !string.IsNullOrEmpty(Modification);

		public void Write(BinaryWriter writer){
			writer.Write(aa);
			writer.Write(name);
			writer.Write(name2);
			writer.Write(Mz);
		}

		public override string ToString(){
			return Name;
		}

		public static ImmoniumIon FindImmoniumIon(string aa){
			foreach (ImmoniumIon i in AnnotationConfig.GetImmoniumArray()){
				ImmoniumIon r = i.name == aa || i.name2 == aa ? i : aa.Length == 1 && aa.Equals(i.Letter) ? i : null;
				if (r != null){
					return r;
				}
			}
			return null;
		}
	}
}