using MqUtil.Mol;
namespace MqUtil.Ms.Annot{
	public class ImmoniumPeakAnnotation : PeakAnnotation{
		private int charge;
		private int hash;
		private readonly ushort neutralLossLevel;
		private double mass = double.NaN;
		private double mz = double.NaN;
		public const string type = "Immonium";

		public ImmoniumPeakAnnotation(ImmoniumIon immoniumIon, int charge){
			if (immoniumIon == null){
				throw new Exception("ImmoniumIon must not be null.");
			}
			ImmoniumIon = immoniumIon;
			this.charge = charge;
			mz = immoniumIon.Mz;
			mass = mz * charge - charge * Molecule.massProton;
			neutralLossLevel = 0;
		}

		public ImmoniumPeakAnnotation(BinaryReader reader){
			throw new NotImplementedException();
		}

		public override int Charge { get => charge;
			set => charge = value;
		}
		public override double Mz { get => mz;
			set => mz = value;
		}
		public override double Mass { get => mass;
			set => mass = value;
		}
		public override ushort NeutralLossLevel => neutralLossLevel;
		public ImmoniumIon ImmoniumIon { get; }

		public override string ToString2(object arg){
			return ImmoniumIon.Name;
		}

		public override string ToString(){
			return ImmoniumIon.Name2;
		}

		public override bool Equals(object obj){
			if (this == obj){
				return true;
			}
			if (obj is ImmoniumPeakAnnotation){
				ImmoniumPeakAnnotation other = (ImmoniumPeakAnnotation) obj;
				if (other.mass != mass){
					return false;
				}
				if (other.charge != charge){
					return false;
				}
				if (other.neutralLossLevel != neutralLossLevel){
					return false;
				}
				return true;
			}
			return false;
		}

		public override int GetHashCode(){
			if (hash == 0){
				int h = 0;
				h = 31*h + (int) mass;
				h = 31*h + charge;
				h = 31*h + (neutralLossLevel + 1);
				hash = h;
			}
			return hash;
		}

		public override string GetSequence(string peptideSeq){
			return ImmoniumIon.Letter;
		}

		public override void Write(BinaryWriter writer){
			throw new NotImplementedException();
		}

		public override object Clone(){
			return new ImmoniumPeakAnnotation(ImmoniumIon, Charge);
		}
	}
}