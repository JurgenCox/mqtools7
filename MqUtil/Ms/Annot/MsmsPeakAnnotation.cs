using System.Text;
using MqApi.Util;
using MqUtil.Mol;
using MqUtil.Ms.Annot.Ions;
namespace MqUtil.Ms.Annot{
	public class MsmsPeakAnnotation : PeakAnnotation{
		public const string type = "Regular";
		private short charge;
		private int hash;
		private readonly ushort neutralLossLevel;
		private readonly short index;
		private readonly IonType ionType;
		private double mz;

		public MsmsPeakAnnotation(IonType ionType, int index, int charge, double mz, ushort neutralLossLevel){
			this.ionType = ionType;
			this.index = (short) index;
			this.charge = (short) charge;
			this.mz = mz;
			this.neutralLossLevel = neutralLossLevel;
		}
		public MsmsPeakAnnotation(IonType ionType, int index, int charge, double mz, ushort neutralLossLevel, byte crossType, CrossLinkSpecificFragmentAnnotation specificAnn) {
			this.ionType = ionType;
			this.index = (short)index;
			this.charge = (short)charge;
			this.mz = mz;
			this.neutralLossLevel = neutralLossLevel;
			base.crossType = crossType;
            CrossSpecificFragment = specificAnn;
        }

		public MsmsPeakAnnotation(BinaryReader reader){
			ionType = IonType.Read(reader);
			index = reader.ReadInt16();
			charge = reader.ReadInt16();
			mz = reader.ReadDouble();
			neutralLossLevel = reader.ReadUInt16();
		}

		public override IonType IonType => ionType;
		public override ushort NeutralLossLevel => neutralLossLevel;
		public override int Index => index;

		public override int Charge{
			get => charge;
			set => charge = (short) value;
		}

		public override double Mass{
			get => Molecule.ConvertToMass(mz, charge);
			set => mz = Molecule.ConvertToMz(value, charge);
		}

		public override double Mz{
			get => mz;
			set => mz = value;
		}

		public override void Write(BinaryWriter writer){
			IonType.Write(writer);
			writer.Write(index);
			writer.Write(charge);
			writer.Write(Mz);
			writer.Write(neutralLossLevel);
		}

		public override string ToString(){
			StringBuilder builder = new StringBuilder();
			builder.Append(ionType);
			if (index != -1){
				builder.Append(Index);
			}
			if (neutralLossLevel == 1){
				builder.Append("*");
			}else if (neutralLossLevel > 1){
				builder.Append("**");
			}
			if (Charge > 1){
				builder.Append("(" + Charge + "+)");
                //TODO
                // this format is inconsistent with fragmenttype.cs (+charge)
			    //builder.Append("(+" + Charge + ")");
            }
			return builder.ToString();
		}

		public override string ToString2(object arg){
			string result = ionType.ToString2();
			if (Index != -1){
				result += StringUtils.ToSubscript(Index, false);
			} else{
				if (Charge > 1){
					result = result.Insert(result.IndexOf("+", StringComparison.InvariantCulture) + 1,
						Parser.ToString(Charge));
				}
			}
			if (neutralLossLevel > 0){
				result += "*";
			}
			if (Charge > 1){
				result += StringUtils.ToSuperscript(Charge, true, false);
			}
			return result;
		}
		
		public override bool Equals(object anObject){
			if (this == anObject){
				return true;
			}
			if (anObject is MsmsPeakAnnotation){
				MsmsPeakAnnotation other = (MsmsPeakAnnotation) anObject;
				if (other.mz != mz){
					return false;
				}
				if (other.index != index){
					return false;
				}
				if (other.charge != charge){
					return false;
				}
				if (!other.ionType.Equals(ionType)){
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
				h = 31*h + (int) mz;
				h = 31*h + index;
				h = 31*h + charge;
				h = 31*h + ionType.GetHashCode();
				h = 31*h + (neutralLossLevel + 1);
				hash = h;
			}
			return hash;
		}

		public override string GetSequence(string peptideSeq){
			if (IonType.Equals(IonType.parent)){
				return peptideSeq;
			}
			if (IsNTerminal){
				return peptideSeq.Substring(0, Index);
			}
			if (IsCTerminal){
				return peptideSeq.Substring(peptideSeq.Length - Index, Index);
			}
			return null;
		}

		public override object Clone(){
			return new MsmsPeakAnnotation(ionType, index, charge, mz, neutralLossLevel);
		}

		public override bool IsNTerminal => IonType.IsNTerminal;
		public override bool IsCTerminal => IonType.IsCTerminal;
	}
}