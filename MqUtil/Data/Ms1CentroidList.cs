using MqApi.Num;

namespace MqUtil.Data{
	/// <summary>
	/// Fields are arrays for the quantities characterizing peaks in a single spectrum.
	/// GrowablePeak objects, that are intersecting with this scan are also recorded.
	/// </summary>
	public class Ms1CentroidList{
		public double[] mzCentroid;
		public double[] MzMin{ get; private set; }
		public double[] MzMax { get; private set; }
		public float[] Resolution { get; private set; }
		public float[] PeakIntensity{ get; private set; }
		public bool[] HasLeftNeighbor{ get; private set; }
		public bool[] HasRightNeighbor{ get; private set; }
		public GrowablePeak[] Peaks{ get; private set; }
		private Ms1CentroidList(){
		}
		public Ms1CentroidList(double[] mzCentroid, double[] mzMin, double[] mzMax, float[] peakIntensity, 
			float[] resolution) {
			this.mzCentroid = mzCentroid;
			MzMin = mzMin;
			MzMax = mzMax;
			PeakIntensity = peakIntensity;
			Peaks = new GrowablePeak[mzCentroid.Length];
			SetNeighbors();
			Resolution = resolution;
		}
		private void SetNeighbors(){
			HasLeftNeighbor = ArrayUtils.FillArray(true, mzCentroid.Length);
			HasRightNeighbor = ArrayUtils.FillArray(true, mzCentroid.Length);
		}
		public double GetMzCentroid(int i){
			return i < 0 ? double.NaN : mzCentroid[i];
		}
		public int Count => mzCentroid.Length;
		public int GetClosestIndex(double mass){
			if (Count == 0){
				return -1;
			}
			if (mass <= mzCentroid[0]){
				return 0;
			}
			if (mass >= mzCentroid[Count - 1]){
				return Count - 1;
			}
			int index = Array.BinarySearch(mzCentroid, mass);
			if (index >= 0){
				return index;
			}
			index = -2 - index;
			if (Math.Abs(mzCentroid[index] - mass) < Math.Abs(mzCentroid[index + 1] - mass)){
				return index;
			}
			return index + 1;
		}
		public Ms1CentroidList Extract(int[] indices){
			Ms1CentroidList result = new Ms1CentroidList{
				mzCentroid = mzCentroid.SubArray(indices),
				MzMin = MzMin.SubArray(indices),
				MzMax = MzMax.SubArray(indices),
				PeakIntensity = PeakIntensity.SubArray(indices),
				HasLeftNeighbor = HasLeftNeighbor.SubArray(indices),
				HasRightNeighbor = HasRightNeighbor.SubArray(indices),
				Peaks = Peaks.SubArray(indices),
				Resolution = Resolution?.SubArray(indices)
			};
			return result;
		}
		internal void SetPeak(GrowablePeak peak, int peakIndex){
			Peaks[peakIndex] = peak;
		}
		public void Dispose(){
			mzCentroid = null;
			MzMin = null;
			MzMax = null;
			PeakIntensity = null;
			HasLeftNeighbor = null;
			HasRightNeighbor = null;
			Resolution = null;
			for (int i = 0; i < Peaks.Length; i++){
				Peaks[i] = null;
			}
			Peaks = null;
		}
		public void CalcNeighbors(double isoMatchTol, bool isoMatchTolInPpm, int minCharge, int maxCharge){
			int n = mzCentroid.Length;
			HasLeftNeighbor = new bool[n];
			HasRightNeighbor = new bool[n];
			for (int i = 0; i < n; i++){
				HasLeftNeighbor[i] = CalcLeftNeighbor(i, isoMatchTol, isoMatchTolInPpm, minCharge, maxCharge);
				HasRightNeighbor[i] = CalcRightNeighbor(i, isoMatchTol, isoMatchTolInPpm, minCharge, maxCharge);
			}
		}
		private bool CalcLeftNeighbor(int i, double isoMatchTol, bool isoMatchTolInPpm, int minCharge, int maxCharge){
			double mz = mzCentroid[i];
			double err = isoMatchTolInPpm ? isoMatchTol * mz * 1e-6 : isoMatchTol;
			double me2 = err * err;
			int ind = i - 1;
			double dm;
			while (ind >= 0 && (dm = mz - mzCentroid[ind]) < 1.05){
				if (Deisotoping.FitsMassDifference(dm, me2, minCharge, maxCharge, mz, true)){
					return true;
				}
				ind--;
			}
			return false;
		}
		private bool CalcRightNeighbor(int i, double isoMatchTol, bool isoMatchTolInPpm, int minCharge, int maxCharge){
			double mz = mzCentroid[i];
			double err = isoMatchTolInPpm ? isoMatchTol * mz * 1e-6 : isoMatchTol;
			double me2 = err * err;
			int ind = i + 1;
			double dm;
			while (ind < mzCentroid.Length && (dm = mzCentroid[ind] - mz) < 1.05){
				if (Deisotoping.FitsMassDifference(dm, me2, minCharge, maxCharge, mz, true)){
					return true;
				}
				ind++;
			}
			return false;
		}
	}
}