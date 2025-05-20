using MqUtil.Ms.Annot.Ions;
namespace MqUtil.Ms.Annot{
	public abstract class PeakAnnotation{
		public abstract int Charge { get; set; }
		public abstract double Mz { get; set; }
		public abstract double Mass { get; set; }
		public abstract ushort NeutralLossLevel { get; }
		public virtual int Index => -1;
        public virtual int PIonIndex => -1;
        public virtual IonType IonType => null;
		public abstract override string ToString();
		public abstract string ToString2(object arg);
		public abstract override bool Equals(object obj);
		public abstract override int GetHashCode();
		public abstract string GetSequence(string peptideSeq);
		public double Score { get; set; } = double.NaN;
		protected byte crossType = 0; // 0: Single, 1: Alpha crosslinked and 2: Beta crosslinked peptides
        protected CrossLinkSpecificFragmentAnnotation CrossSpecificFragment = CrossLinkSpecificFragmentAnnotation.None;

		public byte CurrentCrossType {
			get => crossType;
			set => crossType = value;
		}

        public CrossLinkSpecificFragmentAnnotation CurrentCrossSpecificFragment {
            get => CrossSpecificFragment;
            set => CrossSpecificFragment = value;
        }

		public static string[] ToStrings(PeakAnnotation[] annot){
			string[] result = new string[annot.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = annot[i].ToString();
			}
			return result;
		}

		public abstract void Write(BinaryWriter writer);
		public abstract object Clone();

		public virtual bool IsNTerminal => false;
		public virtual bool IsCTerminal => false;
		public string Name => ToString();

		protected static PeakAnnotation Read(BinaryReader reader){
			int type = reader.ReadInt32();
			if (type == 0){
				return new MsmsPeakAnnotation(reader);
			}
			if (type == 1){
				return new DiagnosticPeakAnnotation(reader);
			}
			if (type == 2){
				return new ImmoniumPeakAnnotation(reader);
			}
			if (type == 3){
				return new InternalPeakAnnotation(reader);
			}
			if (type == 4){
				return new LossPeakAnnotation(reader);
			}
			return null;
		}
	}
}