namespace MqUtil.Ms.Raw {
	public abstract class RawLayer {
		public bool Buffered { get; set; }
		public readonly Dictionary<int, Spectrum> map = new Dictionary<int, Spectrum>();
		public abstract int Count { get; }
		public abstract int MassRangeCount { get; }
		public abstract double[] GetMassRange(int ind);
		public abstract Spectrum GetSpectrum(int ind, bool readCentroids);
		public abstract bool HasProfile(int ind);
		public abstract byte GetMassRangeIndex(int ind);
		public abstract double[] GetTimeSpan(int ind);
		public abstract double GetTime(int ind);
		public abstract int GetIndexFromRt(double time);
		public abstract int Capacity { get; }
	}
}