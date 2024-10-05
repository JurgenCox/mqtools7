using MqUtil.Mol;
namespace MqUtil.Ms.Annot{
	public class InternalPeakAnnotation : PeakAnnotation{
		private readonly AminoAcid[] aa;
		private int charge = 1;
		private int hash;
		private double mz = double.NaN;
		public const string type = "InternFragment";

		public InternalPeakAnnotation(AminoAcid[] aa, Modification[] modifications, bool hasModification, double mz){
			this.aa = aa;
			this.Modifications = modifications;
			this.mz = mz;
			HasModification = hasModification;
		}

		public InternalPeakAnnotation(BinaryReader reader){
			throw new NotImplementedException();
		}

		public override int Charge{
			get => charge;
			set => charge = value;
		}

		public override double Mz{
			get => mz;
			set => mz = value;
		}

		public override double Mass{
			get => Molecule.ConvertToMass(mz, charge);
			set => mz = Molecule.ConvertToMz(value, charge);
		}

		public override ushort NeutralLossLevel => 0;
		public char[] AminoAcids => MqUtil.Mol.AminoAcids.GetSingleLetters(aa);
		private bool HasModification { get; }
		private Modification[] Modifications { get; }

		public override string ToString(){
			string result = "";
			for (int i = 0; i < aa.Length; i++){
				result += aa[i].Letter;
				if (Modifications[i] != null){
					result += "(" + Modifications[i].Abbreviation + ")";
				}
			}
			return result;
		}

		public override string ToString2(object arg){
			return MqUtil.Mol.AminoAcids.GetSingleLetters(aa).Aggregate("", (current, c) => current + c);
		}

		public override bool Equals(object obj){
			if (this == obj){
				return true;
			}
			InternalPeakAnnotation other = obj as InternalPeakAnnotation;
			if (other != null){
				if (other.mz != mz){
					return false;
				}
				if (!ToString().Equals(other.ToString())){
					return false;
				}
				if (other.charge != charge){
					return false;
				}
			}
			return false;
		}

		public override int GetHashCode(){
			if (hash == 0){
				int h = 0;
				h = 31*h + (int) Mass;
				h = AminoAcids.Aggregate(h, (current, c) => 31*current + c.GetHashCode());
				h = 31*h + Charge;
				hash = h;
			}
			return hash;
		}

		public override string GetSequence(string peptideSeq){
			return new string(AminoAcids);
		}

		public override void Write(BinaryWriter writer){
			throw new NotImplementedException();
		}

		public override object Clone(){
			return new InternalPeakAnnotation(aa, Modifications, HasModification, Mz);
		}
	}
}