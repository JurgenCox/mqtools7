using MqUtil.Mol;
using MqUtil.Ms.Search;
namespace MqUtil.Ms.Annot.Ions{
	public abstract class IonType : IComparable{
		public static IonType A = new AIon();
		public static IonType B = new BIon();
		public static IonType C = new CIon();
		public static IonType X = new XIon();
		public static IonType Y = new YIon();
		public static IonType Z = new ZIon();
		public static IonType Z2H = new Z2HIon();
		public static IonType ZH = new ZHIon();
		public static IonType parent = new ParentIon();
		public static IonType[] cidSeriesType = {new AIon(), new BIon(), new CIon(), new XIon(), new YIon(), new ZIon()};
		public static IonType[] etdSeriesType = {new CIon(), new YIon(), new ZIon(), new ZHIon(), new Z2HIon()};
		public static IonType[] allSeriesType = {new AIon(), new BIon(), new CIon(), new XIon(), new YIon(), new ZIon()};
		protected abstract string Name { get; }
		protected abstract string Name2 { get; }
		public abstract bool IsNTerminal { get; }
		public abstract bool IsCTerminal { get; }
		protected abstract double CalcMass(double mass, int charge);

		protected abstract double[] GetSeries(string sequence, PeptideModificationInfo fixedMods,
			PeptideModificationState varMods, bool neutralLoss, out PeakAnnotation[] des,
			Dictionary<ushort, Modification2> specialMods);

		public static double[] GetSeries(IonType iontype, string sequence, PeptideModificationInfo fm,
			PeptideModificationState varMods, bool neutralLoss, Dictionary<ushort, Modification2> specialMods){
			return iontype.GetSeries(sequence, fm, varMods, neutralLoss, out PeakAnnotation[] _, specialMods);
		}

		public static double[] GetSeries(IonType iontype, string sequence, PeptideModificationInfo fm,
			PeptideModificationState varMods, bool neutralLoss, out PeakAnnotation[] des,
			Dictionary<ushort, Modification2> specialMods){
			return iontype.GetSeries(sequence, fm, varMods, neutralLoss, out des, specialMods);
		}

		public string ToString2(){
			return Name2;
		}

		public override string ToString(){
			return Name;
		}

		public override bool Equals(object obj){
			if (obj == null || GetType() != obj.GetType()){
				return false;
			}
			IonType r = (IonType) obj;
			// Use Equals to compare instance variables.
			return Name.Equals(r.Name);
		}

		public override int GetHashCode(){
			return 31*Name.GetHashCode();
		}

		public static IonType Read(BinaryReader reader){
			int r = reader.ReadInt32();
			switch (r){
				case 0:
					return new AIon();
				case 1:
					return new BIon();
				case 2:
					return new CmIon();
				case 3:
					return new CIon();
				case 4:
					return new CpIon();
				case 5:
					return new XIon();
				case 6:
					return new YIon();
				case 7:
					return new ZIon();
				case 8:
					return new ZDotIon();
				case 9:
					return new ZPrimeIon();
				case 10:
					return new ZHIon();
				case 11:
					return new Z2HIon();
				case 12:
					return new ParentIon();
				case 13:
					return new PXIon();
				default:
					throw new Exception("Could not find specified IonType " + r);
			}
		}

		public void Write(BinaryWriter writer){
			if (this is AIon) {
				writer.Write(0);
			} else if (this is BIon) {
				writer.Write(1);
			} else if (this is CmIon) {
				writer.Write(2);
			} else if (this is CIon) {
				writer.Write(3);
			} else if (this is CpIon) {
				writer.Write(4);
			} else if (this is XIon) {
				writer.Write(5);
			} else if (this is YIon) {
				writer.Write(6);
			} else if (this is ZIon) {
				writer.Write(7);
			} else if (this is ZDotIon) {
				writer.Write(8);
			} else if (this is ZPrimeIon) {
				writer.Write(9);
			} else if (this is ZHIon) {
				writer.Write(10);
			} else if (this is Z2HIon) {
				writer.Write(11);
			} else if (this is ParentIon) {
				writer.Write(12);
			} else if (this is PXIon) {
				writer.Write(13);
			} else{
				throw new Exception("Could not find specified IonType " + GetType());
			}
		}

		public int CompareTo(object obj){
			if (obj is IonType){
				IonType o = (IonType) obj;
				return string.CompareOrdinal(Name, o.Name);
			}
			throw new ArgumentException("Object is not an IonType");
		}
	}
}