using MqApi.Util;
using MqUtil.Mol;
using MqUtil.Ms.Annot.Ions;
namespace MqUtil.Ms.Annot{
	public class LossPeakAnnotation : PeakAnnotation{
		public const string type = "NeutralLoss";
		private int hash;
		private readonly ushort neutralLossLevel = 1;
		private double mz = double.NaN;
		private readonly SortedList<string, NeutralLossLib> neutralLosses;
		private readonly PeakAnnotation parent;

		public LossPeakAnnotation(BinaryReader reader){
			mz = reader.ReadDouble();
			neutralLossLevel = reader.ReadUInt16();
			int len = reader.ReadInt32();
			neutralLosses = new SortedList<string, NeutralLossLib>();
			for (int i = 0; i < len; i++){
				string key = reader.ReadString();
				NeutralLossLib value = new NeutralLossLib(reader);
				neutralLosses.Add(key, value);
			}
			bool hasParent = reader.ReadBoolean();
			if (hasParent){
				parent = Read(reader);
			}
		}

		public LossPeakAnnotation(PeakAnnotation p, NeutralLossLib neutralLossLib){
			if (p == null){
				return;
			}
			LossPeakAnnotation annotation = p as LossPeakAnnotation;
			if (annotation != null){
				LossPeakAnnotation a = annotation;
				neutralLosses = new SortedList<string, NeutralLossLib>();
				bool found = false;
				foreach (NeutralLossLib loss in a.NeutralLosses){
					NeutralLossLib clone = (NeutralLossLib) loss.Clone();
					if (clone.Formula.Equals(neutralLossLib.Formula)){
						clone.Count++;
						found = true;
					}
					neutralLosses.Add(clone.Formula, clone);
				}
				if (!found){
					neutralLosses.Add(neutralLossLib.Formula, neutralLossLib);
				}
				parent = (PeakAnnotation) a.Parent.Clone();
			} else{
				neutralLosses = new SortedList<string, NeutralLossLib>{{neutralLossLib.Formula, neutralLossLib}};
				parent = (PeakAnnotation) p.Clone();
			}
		}

		public LossPeakAnnotation(PeakAnnotation p, NeutralLossLib neutralLossLib, byte crossType, CrossLinkSpecificFragmentAnnotation specificAnn) {
			if (p == null) {
				return;
			}
			base.crossType = crossType;
            CrossSpecificFragment = specificAnn;
			LossPeakAnnotation annotation = p as LossPeakAnnotation;
			if (annotation != null) {
				LossPeakAnnotation a = annotation;
				neutralLosses = new SortedList<string, NeutralLossLib>();
				bool found = false;
				foreach (NeutralLossLib loss in a.NeutralLosses) {
					NeutralLossLib clone = (NeutralLossLib)loss.Clone();
					if (clone.Formula.Equals(neutralLossLib.Formula)) {
						clone.Count++;
						found = true;
					}
					neutralLosses.Add(clone.Formula, clone);
				}
				if (!found) {
					neutralLosses.Add(neutralLossLib.Formula, neutralLossLib);
				}
				parent = (PeakAnnotation)a.Parent.Clone();
			} else {
				neutralLosses = new SortedList<string, NeutralLossLib> { { neutralLossLib.Formula, neutralLossLib } };
				parent = (PeakAnnotation)p.Clone();
			}
		}
		public LossPeakAnnotation(PeakAnnotation p, IList<NeutralLossLib> nl){
			if (p == null){
				throw new Exception("parent should not be null. ");
			}
			LossPeakAnnotation annotation = p as LossPeakAnnotation;
			if (annotation != null){
				LossPeakAnnotation a = annotation;
				neutralLosses = new SortedList<string, NeutralLossLib>();
				bool[] found = new bool[nl.Count];
				foreach (NeutralLossLib loss in a.NeutralLosses){
					NeutralLossLib clone = (NeutralLossLib) loss.Clone();
					for (int i = 0; i < nl.Count; i++){
						NeutralLossLib neutralLossLib = nl[i];
						if (clone.Formula.Equals(neutralLossLib.Formula)){
							clone.Count++;
							found[i] = true;
							break;
						}
					}
					neutralLosses.Add(clone.Formula, clone);
				}
				for (int i = 0; i < nl.Count; i++){
					if (found[i] == false){
						NeutralLossLib clone = (NeutralLossLib) nl[i].Clone();
						neutralLosses.Add(clone.Formula, clone);
					}
				}
				parent = (PeakAnnotation) a.Parent.Clone();
			} else{
				neutralLosses = new SortedList<string, NeutralLossLib>();
				foreach (NeutralLossLib loss in nl){
					neutralLosses.Add(loss.Formula, loss);
				}
				parent = (PeakAnnotation) p.Clone();
			}
		}

		public override ushort NeutralLossLevel => (ushort) (neutralLosses?.Count ?? 0);
		public override int Index => Parent.Index;

		public override double Mz{
			get{
				if (double.IsNaN(mz)){
					double massloss = 0;
					foreach (NeutralLossLib nl in neutralLosses.Values)
					{
						massloss = massloss + nl.Mass;
					}
					mz = parent.Mz - massloss;
				}
				return mz;
			}
			set => mz = value;
		}

		public override double Mass
		{
			get => Molecule.ConvertToMass(mz, Charge);
			set => mz = Molecule.ConvertToMz(value, Charge);
		}

		public override int Charge{
			get{
				return 1;
			}
			set {  }
		}

		public IList<NeutralLossLib> NeutralLosses => neutralLosses.Values;

		public int NeutralLossCount{
			get{
				int count = 0;
				foreach (NeutralLossLib loss in NeutralLosses){
					count += loss.Count;
				}
				return count;
			}
		}

		public PeakAnnotation Parent => parent;

		public override void Write(BinaryWriter writer){
			writer.Write(Mz);
			writer.Write(neutralLossLevel);
			writer.Write(NeutralLosses.Count);
			foreach (KeyValuePair<string, NeutralLossLib> pair in neutralLosses){
				writer.Write(pair.Key);
				pair.Value.Write(writer);
			}
			if (Parent != null){
				writer.Write(true);
				if (Parent is MsmsPeakAnnotation){
					writer.Write(0);
				} else if (Parent is DiagnosticPeakAnnotation){
					writer.Write(1);
				} else if (Parent is ImmoniumPeakAnnotation){
					writer.Write(2);
				} else if (Parent is InternalPeakAnnotation){
					writer.Write(3);
				} else if (Parent is LossPeakAnnotation){
					writer.Write(4);
				}
				Parent.Write(writer);
			} else{
				writer.Write(false);
			}
		}

		public override bool Equals(object anObject){
			if (this == anObject){
				return true;
			}
			if (anObject is LossPeakAnnotation){
				LossPeakAnnotation other = (LossPeakAnnotation) anObject;
				if (other.Mass != Mass){
					return false;
				}
				if (other.Charge != Charge){
					return false;
				}
				if (!other.Parent.Equals(Parent)){
					return false;
				}
				foreach (NeutralLossLib loss in neutralLosses.Values){
					if (!other.NeutralLosses.Contains(loss)){
						return false;
					}
				}
				if (!other.ToString().Equals(ToString())){
					return false;
				}
				return true;
			}
			return false;
		}

		public override int GetHashCode(){
			if (hash == 0){
				int h = 0;
				h = 31*h + (int) Mass;
				h = 31*h + Charge;
				foreach (NeutralLossLib loss in NeutralLosses){
					h = 31*h + loss.GetHashCode();
				}
				h = 31*h + Parent.GetHashCode();
				hash = h;
			}
			return hash;
		}

		public override string GetSequence(string peptideSeq){
			return Parent.GetSequence(peptideSeq);
		}

		public override string ToString(){
			string result = Parent.ToString();
			if (Charge > 1){
				result = result.Replace("(" + Charge + "+)", "");
			}
			foreach (NeutralLossLib neutralLoss in neutralLosses.Values){
				result += "-";
				result += neutralLoss;
				if (neutralLoss.By != null){
					result += "(by " + neutralLoss.By + ")";
				}
			}
			if (Charge > 1){
				result += "(" + Charge + "+)";
			}
			return result;
		}

		public override string ToString2(object arg){
			string result = parent.ToString2(arg);
			if (Charge > 1){
				result = result.Replace(StringUtils.ToSuperscript(Charge, false, false) + '\u207A', "");
			}
			foreach (NeutralLossLib neutralLoss in neutralLosses.Values){
				result += "-";
				result += arg is bool && (bool) arg ? neutralLoss.ToStringBy() : neutralLoss.ToString2();
			}
			if (Charge > 1){
				result += StringUtils.ToSuperscript(Charge, false, false) + '\u207A';
			}
			return result;
		}

		public override object Clone(){
			List<NeutralLossLib> losses = new List<NeutralLossLib>(neutralLosses.Count);
			foreach (NeutralLossLib loss in neutralLosses.Values){
				losses.Add((NeutralLossLib) loss.Clone());
			}
			return new LossPeakAnnotation((PeakAnnotation) parent.Clone(), losses);
		}

		public override bool IsNTerminal => Parent.IsNTerminal;
		public override bool IsCTerminal => Parent.IsCTerminal;
		public override IonType IonType => Parent.IonType;
	}
}