using MqApi.Num;
using MqUtil.Ms.Enums;
using MqUtil.Ms.Utils;
using MqUtil.Num;

namespace MqUtil.Data{
	public class Peak2 : GenericPeak{
		const double res = 50000;
        private bool HasMassRecal { get; }
        
        /// <summary>
        /// the centroid masses for each scan 
        /// </summary>
        private double[] MzCentroid{ get; }
		public double MzCentroidAvg{ get; set; } 
		/// <summary>
		/// A "reasonable" average or integral of the intensity over both m/z and retention time. 
		/// </summary>
		private float intensity;
		/// <summary>
		/// delta m/z to be added to the values of MzCentroid, MzMin, and MzMax at each time
		/// </summary>
		public bool[] HasLeftNeighbor{ get; set; }
		public bool[] HasRightNeighbor{ get; set; }


        public Peak2(double[] centerMz, float[] origIntensityProfile, int[] scanIndices, 
			IntensityDetermination intensityDetermination, bool[] hasLeftNeighbor, bool[] hasRightNeighbor, 
			bool hasMassRecal) {
			HasMassRecal = hasMassRecal;
			if (HasMassRecal){
				MzCentroid = centerMz;
			}
			HasLeftNeighbor = hasLeftNeighbor;
			HasRightNeighbor = hasRightNeighbor;
			OrigIntensityProfile = origIntensityProfile;
			ScanIndices = scanIndices;
			CalcAverages(centerMz, intensityDetermination);
		}
		public Peak2(BinaryReader reader){
            HasMassRecal = reader.ReadBoolean();
			MzCentroidAvg = reader.ReadDouble();
			intensity = reader.ReadSingle();
			int count = reader.ReadInt32();
			if (HasMassRecal) {
				MzCentroid = new double[count];
			}
			OrigIntensityProfile = new float[count];
			ScanIndices = new int[count];
			HasLeftNeighbor = new bool[count];
			HasRightNeighbor = new bool[count];
			for (int i = 0; i < count; i++){
				if (HasMassRecal) {
					MzCentroid[i] = reader.ReadDouble();
				}
				OrigIntensityProfile[i] = reader.ReadSingle();
				ScanIndices[i] = reader.ReadInt32();
				HasLeftNeighbor[i] = reader.ReadBoolean();
				HasRightNeighbor[i] = reader.ReadBoolean();
			}
		}
		public void Write(BinaryWriter writer){
            writer.Write(HasMassRecal);
			writer.Write(MzCentroidAvg);
			writer.Write(intensity);
			writer.Write(Count);
			for (int i = 0; i < Count; i++){
				if (HasMassRecal) {
					writer.Write(MzCentroid[i]);
				}
				writer.Write(OrigIntensityProfile[i]);
				writer.Write(ScanIndices[i]);
				writer.Write(HasLeftNeighbor[i]);
				writer.Write(HasRightNeighbor[i]);
			}
		}
		public float Intensity{
			get{
				if (intensity <= 0 || float.IsNaN(intensity)){
					intensity = (float)CalcIntensity(IntensityDetermination.SumTotal);
				}
				return intensity;
			}
		}
		public bool GetHasLeftNeighbor(int index) {
			return HasLeftNeighbor[index];
		}
		public bool GetHasRightNeighbor(int index) {
			return HasRightNeighbor[index];
		}
		/// <summary>
		/// Number of retention times in the peak.
		/// </summary>
		public override int Count => HasLeftNeighbor.Length;
		public override int LastScanIndex => Count == 0 ? -1 : ScanIndices[Count - 1];
		public override int FirstScanIndex => Count == 0 ? -1 : ScanIndices[0];
		public double GetCenterMz(int index){
			return HasMassRecal ? MzCentroid[index] : MzCentroidAvg;
		}
		public double GetMinMz(int index){
			return MzCentroidAvg - MzCentroidAvg / res;
		}
		public double GetMaxMz(int index){
			return MzCentroidAvg + MzCentroidAvg / res;
		}
		private void CalcAverages(double[] centerMz, IntensityDetermination intensDet){
			intensity = (float) CalcIntensity(intensDet);
			MzCentroidAvg = CalcAverageMzNoCalib(centerMz, OrigIntensityProfile, ArrayUtils.ConsecutiveInts(Count));
			if (double.IsNaN(MzCentroidAvg)){
				throw new Exception("double.IsNaN(CenterMass)");
			}
		}
		/// <summary>
		/// Calculate the intensity of this peak by considering in various ways the 
		/// time-behavior of either OrigIntensityProfile or SmoothIntensities.
		/// Only called from CalcAverages.
		/// </summary>
		private double CalcIntensity(IntensityDetermination intensityDetermination){
			switch (intensityDetermination){
				case IntensityDetermination.Maximum: return ArrayUtils.Max(OrigIntensityProfile);
				case IntensityDetermination.SumTotal: return ArrayUtils.Sum(OrigIntensityProfile);
				case IntensityDetermination.SumFwhm: return SumFwhm(OrigIntensityProfile);
				case IntensityDetermination.SumSmoothFwhm: return SumFwhm(OrigIntensityProfile);
				default: throw new Exception("Never get here.");
			}
		}
		/// <summary>
		/// Sum the values of the argument between the half-maximum points.
		/// </summary>
		private static double SumFwhm(IList<float> profile){
			int minInd = profile.MaxInd();
			int maxInd = minInd;
			double maxVal = profile[minInd];
			while (minInd > 0 && profile[minInd] > maxVal / 2){
				minInd--;
			}
			while (maxInd < profile.Count - 1 && profile[maxInd] > maxVal / 2){
				maxInd++;
			}
			double result = 0;
			for (int i = minInd; i <= maxInd; i++){
				result += profile[i];
			}
			return result;
		}
		/// <summary>
		/// Find the intensity weighted centroid mass over a selection of scans.
		/// </summary>
		private static double CalcAverageMzNoCalib(double[] centerMz, float[] intensityProfile, IEnumerable<int> indx){
			double norm1 = 0;
			double m = 0;
			foreach (int i in indx){
				double mz = centerMz[i];
				if (!double.IsNaN(mz)){
					double intens = intensityProfile[i];
					norm1 += intens;
					m += intens * mz;
				}
			}
			m /= norm1;
			return m;
		}

		//TODO: check
		private static double CalcAverageMz(Peak2 peak, IEnumerable<int> indx, CubicSpline[] mzCalibrationPar,
			bool isPpm){
			double norm1 = 0;
			double m = 0;
			foreach (int i in indx){
				double mz = peak.GetCenterMz(i) *
				(1 + MolUtil.RelativeCorrectionSpline(peak.GetCenterMz(i), mzCalibrationPar,
				0, isPpm));
				if (!double.IsNaN(mz)){
					norm1 += peak.OrigIntensityProfile[i];
					m += peak.OrigIntensityProfile[i] * mz;
				}
			}
			m /= norm1;
			return m;
		}
		public override double CalcAverageMz(CubicSpline[] mzCalibrationPar, LinearInterpolator intensDepCal,
			LinearInterpolator mobilityDepCal, bool isPpm, double intensity, int imsStep){
			if (MzCentroid == null){
				return MzCentroidAvg;
			}
			int[] ind = ArrayUtils.ConsecutiveInts(Count);
			if (mzCalibrationPar == null){
				return CalcAverageMzNoCalib(MzCentroid, OrigIntensityProfile, ind);
			}
			double mz = CalcAverageMz(this, ind, mzCalibrationPar, isPpm);
			if (double.IsNaN(intensity)){
				return mz;
			}
			double intens = 1.0 / Math.Log(intensity);
			return mz * (1 - (isPpm ? 1e-6 * intensDepCal.Get(intens) : intensDepCal.Get(intens) / mz));
		}
		public override double CalcAverageMzUncalibrated(){
			if (HasMassRecal) {
				int[] ind = ArrayUtils.ConsecutiveInts(Count);
				return CalcAverageMzNoCalib(MzCentroid, OrigIntensityProfile, ind);
			}
			return MzCentroidAvg;
		}
		public override void Dispose(){
			base.Dispose();
			OrigIntensityProfile = null;
			ScanIndices = null;
		}
		public override double GetIntensityAtScanIndex(int scanInd, out double mz){
			int a = Array.BinarySearch(ScanIndices, scanInd);
			if (a < 0){
				mz = double.NaN;
				return double.NaN;
			}
			mz = GetCenterMz(a);
			return OrigIntensityProfile[a];
		}
		public Peak ToPeak(){
			int n = HasLeftNeighbor.Length;
			double[] mzs = new double[n];
			float[] minMzs = new float[n];
			float[] maxMzs = new float[n];
			for (int i = 0; i < n; i++){
				double dm = MzCentroidAvg / res;
				mzs[i] = MzCentroidAvg;
				minMzs[i] = (float)(MzCentroidAvg - dm);
				maxMzs[i] = (float)(MzCentroidAvg + dm);
			}
			Peak p = new Peak(mzs, minMzs, maxMzs,ArrayUtils.FillArray(1f,n), ArrayUtils.FillArray(1f, n),
				ScanIndices, new byte[n], new bool[n], IntensityDetermination.Maximum, HasLeftNeighbor, 
				HasRightNeighbor, float.NaN);
			return p;
		}
	}
}