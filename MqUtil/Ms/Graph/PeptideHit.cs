using MqUtil.Mol;
using MqUtil.Ms.Annot;
namespace MqUtil.Ms.Graph{
	public class PeptideHit{
		public string Sequence { get; set; }
		public double Score { get; set; }
		public double Mz { get; set; }
		public int Charge { get; set; }
		public PeptideModificationState FixedMods { get; set; }
		public PeptideModificationState VarMods { get; set; }
		public string[] ProteinIDs { get; set; }
		public string[] ProteinNames { get; set; }
		public string[] Uniprots { get; set; }
		public string[] GeneNames { get; set; }
		public PeakAnnotation[] Annotation { get; set; }

		public PeptideHit() { }

		public PeptideHit(string sequence, double score, double mz, PeakAnnotation[] annotation, 
			int charge, PeptideModificationState varMods, PeptideModificationState fixedMods){
			Sequence = sequence;
			Score = score;
			Mz = mz;
			Annotation = annotation;
			Charge = charge;
			VarMods = varMods;
			FixedMods = fixedMods;
		}

		public bool HasCTermModification{
			get{
				if (VarMods != null && VarMods.CTermModification != ushort.MaxValue){
					return true;
				}
				return FixedMods != null && FixedMods.CTermModification != ushort.MaxValue;
			}
		}
		public bool HasNTermModification{
			get{
				if (VarMods != null && VarMods.NTermModification != ushort.MaxValue){
					return true;
				}
				return FixedMods != null && FixedMods.NTermModification != ushort.MaxValue;
			}
		}
		public override string ToString(){
			return "mz: " + Math.Round(Mz, 4) + "; charge: " + Charge + "+";
		}

		public override bool Equals(object obj){
			if (obj is PeptideHit){
				return Equals((PeptideHit) obj);
			}
			return false;
		}

		public bool Equals(PeptideHit obj){
			if (!obj.Sequence.Equals(Sequence)){
				return false;
			}
			if (!obj.Charge.Equals(Charge)){
				return false;
			}
			if (!obj.VarMods.Equals(VarMods)){
				return false;
			}
			if (!obj.FixedMods.Equals(FixedMods)){
				return false;
			}
			return true;
		}

		private int hash;

		public override int GetHashCode(){
			if (hash == 0){
				hash = 31*hash + Sequence.GetHashCode();
				hash = 31*hash + Charge.GetHashCode();
				hash = 31*hash + VarMods.GetHashCode();
				hash = 31*hash + FixedMods.GetHashCode();
			}
			return hash;
		}

		public void Dispose(){
			ProteinIDs = null;
			ProteinNames = null;
			GeneNames = null;
			Uniprots = null;
		}
	}
}