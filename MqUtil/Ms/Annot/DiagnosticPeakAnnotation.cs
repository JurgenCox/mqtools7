using MqUtil.Mol;
namespace MqUtil.Ms.Annot{
	public class DiagnosticPeakAnnotation : PeakAnnotation{
		public const string type = "Diagnostic";
		private readonly DiagnosticPeak diagnostic;
		private double mz;
		private int hash;
		public ushort ModInd { get; }
		public char Aa { get; }

		public DiagnosticPeakAnnotation(BinaryReader reader){
			mz = reader.ReadDouble();
			ModInd = reader.ReadUInt16();
			Aa = reader.ReadChar();
			diagnostic = new DiagnosticPeak(reader);
		}

		public DiagnosticPeakAnnotation(DiagnosticPeak diagnostic, double mz, ushort modInd, char aa){
			this.diagnostic = diagnostic;
			this.mz = mz;
			ModInd = modInd;
			Aa = aa;
		}

		public override void Write(BinaryWriter writer){
			writer.Write(mz);
			writer.Write(ModInd);
			writer.Write(Aa);
			diagnostic.Write(writer);
		}

		public override int Charge{
			get { return 1; }
			set { }
		}

		public override double Mz{
			get => mz;
			set => mz = value;
		}

		public override double Mass{
			get { return mz - Molecule.massProton; }
			set { }
		}

		public override ushort NeutralLossLevel => 0;

		public override string ToString(){
			return diagnostic.ShortName;
		}

		public override bool Equals(object anObject){
			if (this == anObject){
				return true;
			}
			if (anObject is DiagnosticPeakAnnotation){
				DiagnosticPeakAnnotation other = (DiagnosticPeakAnnotation) anObject;
				if (other.mz != mz){
					return false;
				}
				if (other.ModInd != ModInd){
					return false;
				}
				if (other.diagnostic != diagnostic){
					return false;
				}
				return other.Aa == Aa;
			}
			return false;
		}

		public override int GetHashCode(){
			if (hash == 0){
				int h = 0;
				h = 31*h + (int) mz;
				h = 31*h + ModInd;
				h = 31*h + diagnostic.GetHashCode();
				h = 31*h + Aa;
				hash = h;
			}
			return hash;
		}

		public override string GetSequence(string peptideSeq){
			return "";
		}

		public override object Clone(){
			return new DiagnosticPeakAnnotation((DiagnosticPeak) diagnostic.Clone(), mz, ModInd, Aa);
		}

		public override string ToString2(object arg){
			return diagnostic.ShortName;
		}
	}
}