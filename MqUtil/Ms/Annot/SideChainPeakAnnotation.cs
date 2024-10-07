using MqUtil.Mol;
namespace MqUtil.Ms.Annot{
	public class SideChainPeakAnnotation : PeakAnnotation{
		private int charge;
		private int hash;
		private readonly ushort neutralLossLevel;
		private double mass;
		private double mz;
		public const string type = "Sidechain";

		public SideChainPeakAnnotation(SideChain sidechainIon, int charge){
			if (sidechainIon == null){
				throw new Exception("SideChain must not be null.");
			}
			SidechainIon = sidechainIon;
			this.charge = charge;
			mz = sidechainIon.Mz;
			mass = mz * charge - charge * Molecule.massProton;
			neutralLossLevel = 0;
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
		public SideChain SidechainIon { get; }

		public override string ToString2(object arg){
			return SidechainIon.Name;
		}

		public override string ToString(){
			return SidechainIon.Name2;
		}

		public override bool Equals(object obj){
			if (this == obj){
				return true;
			}
			if (obj is SideChainPeakAnnotation){
				SideChainPeakAnnotation other = (SideChainPeakAnnotation) obj;
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
			return SidechainIon.Letter;
		}

		public override void Write(BinaryWriter writer){
			throw new NotImplementedException();
		}

		public override object Clone(){
			return new SideChainPeakAnnotation(SidechainIon, Charge);
		}
	}
}