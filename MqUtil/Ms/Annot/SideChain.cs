using System.Xml.Serialization;
using MqUtil.Mol;
namespace MqUtil.Ms.Annot{
	[Serializable, System.Diagnostics.DebuggerStepThrough, XmlType(AnonymousType = true),
	XmlRoot(ElementName = "side chain", IsNullable = false)]
	public class SideChain{
		private static readonly double plusH = Molecule.massH - Molecule.massElectron;

		protected SideChain(){
			//default constructor for serialization
		}

		private SideChain(string formula, string charge, string aminoacid){
			Formula = formula;
			Charge = charge;
			Letter = aminoacid;
		}

		[XmlAttribute("formula")]
		public string Formula { get; set; }
		[XmlAttribute("letter")]
		public string Letter { get; set; }
		[XmlAttribute("charge")]
		public string Charge { get; set; }
		public string Name => "SC " + Letter;
		public string Name2 => "SC " + Formula + " " + Charge;

		public double Mz{
			get{
				double mz = Molecule.CalcMonoMass(Formula);
				if (Charge == "H+"){
					mz += plusH;
				}
				return mz;
			}
		}

		public bool HasModification => false;

		public override string ToString(){
			return Name2;
		}
	}
}