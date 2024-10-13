using MqApi.Num;
using MqUtil.Ms.Enums;
using MqUtil.Ms.Instrument;
using MqUtil.Ms.Raw;
using MqUtil.Ms.Utils;
using MqUtil.Num;
using MqUtil.Num.Test.Univariate.TwoSample;
namespace MqUtil.Data{
	public class Peak : GenericPeak{
		public double[] MzCentroid{ get; set; }
		/// <summary>
		/// the minimum m/z included in the peak at each scan 
		/// </summary>
		private float[] mzMin;
		/// <summary>
		/// the maximum m/z included in the peak at each scan 
		/// </summary>
		private float[] mzMax;
		private byte[] MassRange{ get; set; }
		/// <summary>
		/// Transient. Tells whether there is a gap at this position. If gaps[i] == true then there is a gap
		/// between position i-1 and i. This implies gaps[0] is always false.
		/// </summary>
		private bool[] gaps; //transient
		/// <summary>
		/// The largest m/z included in any peak at any time; "summary" of MzMax 
		/// </summary>
		public double MzMaxMax{ get; set; }
		/// <summary>
		/// The smallest m/z included in any peak at any time; "summary" of MzMin 
		/// </summary>
		public double MzMinMin{ get; set; }
		public double MzCentroidAvg{ get; set; }
		public double MzCentroidUncalibAvg{ get; set; }
		private float intensity{ get; set; }
		/// <summary>
		/// delta m/z to be added to the values of MzCentroid, MzMin, and MzMax at each time
		/// </summary>
		private double[] MzCorrection{ get; set; }
		public bool[] HasLeftNeighbor{ get; set; }
		public bool[] HasRightNeighbor{ get; set; }
		public bool Flagged{ get; set; }
		public float Jitter{ get; set; }
		public bool Refined { get; set; }
		public float Resolution { get; set; }
		public float[] SmoothIntensities { get; set; }
		public Peak(double[] centerMz, float[] minMzs, float[] maxMzs, float[] smoothIntensity,
			float[] origIntensityProfile, int[] scanIndices, byte[] massRange, bool[] gaps,
			IntensityDetermination intensityDetermination, bool[] hasLeftNeighbor, bool[] hasRightNeighbor, float fwhm) {
			MzCentroid = centerMz;
			MzCorrection = new double[centerMz.Length];
			mzMin = minMzs;
			mzMax = maxMzs;
			HasLeftNeighbor = hasLeftNeighbor;
			HasRightNeighbor = hasRightNeighbor;
			SmoothIntensities = smoothIntensity;
			OrigIntensityProfile = origIntensityProfile;
			ScanIndices = scanIndices;
			MassRange = massRange;
			if (gaps.Length > 0){
				gaps[0] = false;
			}
			this.gaps = gaps;
			CalcAverages(intensityDetermination);
			Resolution = !float.IsNaN(fwhm) ? (float)MzCentroidAvg / fwhm: float.NaN;
		}
		public Peak(Peak[] peaks, IntensityDetermination intensityDetermination){
			ScanIndices = JoinScanIndices(peaks);
			int n = ScanIndices.Length;
			MzCentroid = new double[n];
			MzCorrection = new double[n];
			mzMin = ArrayUtils.FillArray(float.MaxValue, n);
			mzMax = ArrayUtils.FillArray(float.MinValue, n);
			HasLeftNeighbor = new bool[n];
			HasRightNeighbor = new bool[n];
			SmoothIntensities = new float[n];
			OrigIntensityProfile = new float[n];
			MassRange = new byte[n];
			foreach (Peak peak in peaks){
				for (int i = 0; i < peak.ScanIndices.Length; i++){
					int index = peak.ScanIndices[i];
					int ind = Array.BinarySearch(ScanIndices, index);
					mzMin[ind] = Math.Min(mzMin[ind], peak.mzMin[i]);
					mzMax[ind] = Math.Max(mzMax[ind], peak.mzMax[i]);
					SmoothIntensities[ind] += peak.SmoothIntensities[i];
					OrigIntensityProfile[ind] += peak.OrigIntensityProfile[i];
					MzCentroid[ind] += peak.MzCentroid[i] * peak.OrigIntensityProfile[i];
					MzCorrection[ind] += peak.MzCorrection[i] * peak.OrigIntensityProfile[i];
					MassRange[ind] = peak.MassRange[i];
					HasLeftNeighbor[ind] = HasLeftNeighbor[ind] || peak.HasLeftNeighbor[i];
					HasRightNeighbor[ind] = HasRightNeighbor[ind] || peak.HasRightNeighbor[i];
				}
			}
			for (int i = 0; i < ScanIndices.Length; i++){
				MzCentroid[i] /= OrigIntensityProfile[i];
				MzCorrection[i] /= OrigIntensityProfile[i];
			}
			gaps = new bool[n];
			gaps[0] = false;
			for (int i = 1; i < n; i++){
				if (ScanIndices[i - 1] < ScanIndices[i] - 1){
					gaps[i] = true;
				}
			}
			CalcAverages(intensityDetermination);
		}
		public Peak(BinaryReader reader){
			MzCentroidAvg = reader.ReadDouble();
			MzCentroidUncalibAvg = reader.ReadDouble();
			intensity = reader.ReadSingle();
			MzMinMin = reader.ReadDouble();
			MzMaxMax = reader.ReadDouble();
			int count = reader.ReadInt32();
			MzCentroid = new double[count];
			MzCorrection = new double[count];
			mzMin = new float[count];
			mzMax = new float[count];
			SmoothIntensities = new float[count];
			OrigIntensityProfile = new float[count];
			ScanIndices = new int[count];
			MassRange = new byte[count];
			HasLeftNeighbor = new bool[count];
			HasRightNeighbor = new bool[count];
			for (int i = 0; i < count; i++){
				MzCentroid[i] = reader.ReadDouble();
				MzCorrection[i] = reader.ReadDouble();
				mzMin[i] = reader.ReadSingle();
				mzMax[i] = reader.ReadSingle();
				SmoothIntensities[i] = reader.ReadSingle();
				OrigIntensityProfile[i] = reader.ReadSingle();
				ScanIndices[i] = reader.ReadInt32();
				MassRange[i] = reader.ReadByte();
				HasLeftNeighbor[i] = reader.ReadBoolean();
				HasRightNeighbor[i] = reader.ReadBoolean();
			}
			Flagged = reader.ReadBoolean();
			Jitter = reader.ReadSingle();
			Refined = reader.ReadBoolean();
			Resolution = reader.ReadSingle();
		}
		public void Write(BinaryWriter writer){
			writer.Write(MzCentroidAvg);
			writer.Write(MzCentroidUncalibAvg);
			writer.Write(intensity);
			writer.Write(MzMinMin);
			writer.Write(MzMaxMax);
			writer.Write(Count);
			for (int i = 0; i < Count; i++){
				writer.Write(MzCentroid[i]);
				writer.Write(MzCorrection[i]);
				writer.Write(mzMin[i]);
				writer.Write(mzMax[i]);
				writer.Write(SmoothIntensities[i]);
				writer.Write(OrigIntensityProfile[i]);
				writer.Write(ScanIndices[i]);
				writer.Write(MassRange[i]);
				writer.Write(HasLeftNeighbor[i]);
				writer.Write(HasRightNeighbor[i]);
			}
			writer.Write(Flagged);
			writer.Write(Jitter);
			writer.Write(Refined);
			writer.Write(Resolution);
		}
		public float Intensity {
			get {
				if (intensity <= 0 || float.IsNaN(intensity)) {
					intensity = (float)CalcIntensity(IntensityDetermination.SumTotal);
				}
				return intensity;
			}
		}
		private static int[] JoinScanIndices(Peak[] peaks){
			HashSet<int> inds = new HashSet<int>();
			foreach (Peak peak in peaks){
				foreach (int index in peak.ScanIndices){
					inds.Add(index);
				}
			}
			int[] result = inds.ToArray();
			Array.Sort(result);
			return result;
		}
		public double GetCalibCenterMz(int index){
			return MzCentroid[index] + MzCorrection[index];
		}
		public double GetCalibMinMz(int index){
			return GetMinMz(index) + MzCorrection[index];
		}
		public double GetCalibMaxMz(int index){
			return GetMaxMz(index) + MzCorrection[index];
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
		public override int Count => MzCentroid.Length;
		public override int LastScanIndex => Count == 0 ? -1 : ScanIndices[Count - 1];
		public override int FirstScanIndex => Count == 0 ? -1 : ScanIndices[0];
		public int GapCount{
			get{
				int count = 0;
				for (int i = 1; i < gaps.Length; i++){
					if (gaps[i]){
						count++;
					}
				}
				return count;
			}
		}
		public bool HasGap{
			get{
				for (int i = 1; i < gaps.Length; i++){
					if (gaps[i]){
						return true;
					}
				}
				return false;
			}
		}
		/// <summary>
		/// Return the time at which this mz-rt-peak started.
		/// </summary>
		public double GetMinTime(RawLayer rawFile){
			return rawFile.GetTimeSpan(Math.Max(0, FirstScanIndex - 1))[0];
		}
		/// <summary>
		/// Return the intensity-weighted elution time of this mz-rt-peak.
		/// </summary>
		public double GetCenterTime(RawLayer rawFile){
			int ind = OrigIntensityProfile.MaxInd();
			return rawFile.GetTime(ScanIndices[ind]);
		}
		/// <summary>
		/// Return the time at which this mz-rt-peak finishes.
		/// </summary>
		public double GetMaxTime(RawLayer rawFile){
			return rawFile.GetTimeSpan(LastScanIndex)[1];
		}
		public double GetOriginalIntensity(int index, double[] ionInjectionTimes){
			return OrigIntensityProfile[index] * ionInjectionTimes[GetScanIndex(index)];
		}
		public double GetCenterMz(int index){
			return MzCentroid[index];
		}
		public double GetMinMz(int index){
			return mzMin[index];
		}
		public double GetMaxMz(int index){
			return mzMax[index];
		}
		public double GetCenterMzCalib(int index, CubicSpline[] mzCalibrationPar, bool isPpm){
			return GetCalibCenterMz(index) *
			       (1 + MolUtil.RelativeCorrectionSpline(GetCalibCenterMz(index), mzCalibrationPar, MassRange[index],
				       isPpm));
		}
		public double GetMinMzCalib(int index, CubicSpline[] mzCalibrationPar, bool isPpm){
			return GetCalibMinMz(index) *
			       (1 + MolUtil.RelativeCorrectionSpline(GetCalibMinMz(index), mzCalibrationPar, MassRange[index],
				       isPpm));
		}
		public double GetMaxMzCalib(int index, CubicSpline[] mzCalibrationPar, bool isPpm){
			return GetCalibMaxMz(index) *
			       (1 + MolUtil.RelativeCorrectionSpline(GetCalibMaxMz(index), mzCalibrationPar, MassRange[index],
				       isPpm));
		}
		public double GetCenterMzCalibMetaboliteAt(int index, CubicSpline[] mzCalibrationPar,
			LinearInterpolator intensDependentCalibration, bool isPpm, double[] iit){
			double ccmz = GetCalibCenterMz(index);
			double mz;
			if (mzCalibrationPar != null){
				mz = ccmz * (1 + MolUtil.RelativeCorrectionSpline(ccmz, mzCalibrationPar, MassRange[index], isPpm));
			} else{
				mz = ccmz;
			}
			if (intensDependentCalibration != null){
				double intens = Math.Log(GetOriginalIntensity(index, iit));
				return mz * (1 - (isPpm ? 1e-6 * intensDependentCalibration.Get(intens)
					: intensDependentCalibration.Get(intens) / mz));
			}
			return mz;
		}
		public double GetCenterMzCalibMetabolite(CubicSpline[] mzCalibrationPar,
			LinearInterpolator intensDependentCalibration, bool isPpm, double[] iit){
			double mz = 0;
			double w = 0;
			for (int index = 0; index < Count; index++){
				double mz1 =
					GetCenterMzCalibMetaboliteAt(index, mzCalibrationPar, intensDependentCalibration, isPpm, iit);
				double w1 = Math.Sqrt(GetOriginalIntensity(index, iit));
				mz += mz1 * w1;
				w += w1;
			}
			return mz / w;
		}
		public double GetCenterMzUncalibMetabolite(double[] iit){
			double mz = 0;
			double w = 0;
			for (int index = 0; index < Count; index++){
				double mz1 = GetCenterMz(index);
				double w1 = Math.Sqrt(GetOriginalIntensity(index, iit));
				mz += mz1 * w1;
				w += w1;
			}
			return mz / w;
		}
		public double GetMinMzCalibMetaboliteAt(int index, CubicSpline[] mzCalibrationPar,
			LinearInterpolator intensDependentCalibration, bool isPpm, double[] iit){
			double mz = GetCalibMinMz(index) *
			            (1 + MolUtil.RelativeCorrectionSpline(GetCalibMinMz(index), mzCalibrationPar, MassRange[index],
				            isPpm));
			double intens = Math.Log(GetOriginalIntensity(index, iit));
			return mz * (1 - (isPpm
				? 1e-6 * intensDependentCalibration.Get(intens)
				: intensDependentCalibration.Get(intens) / mz));
		}
		public double GetMaxMzCalibMetaboliteAt(int index, CubicSpline[] mzCalibrationPar,
			LinearInterpolator intensDependentCalibration, bool isPpm, double[] iit){
			double mz = GetCalibMaxMz(index) *
			            (1 + MolUtil.RelativeCorrectionSpline(GetCalibMaxMz(index), mzCalibrationPar, MassRange[index],
				            isPpm));
			double intens = Math.Log(GetOriginalIntensity(index, iit));
			return mz * (1 - (isPpm
				? 1e-6 * intensDependentCalibration.Get(intens)
				: intensDependentCalibration.Get(intens) / mz));
		}
		private void CalcAverages(IntensityDetermination intensDet){
			MzMinMin = double.MaxValue;
			MzMaxMax = double.MinValue;
			for (int i = 0; i < Count; i++){
				if (GetMinMz(i) < MzMinMin){
					MzMinMin = GetMinMz(i);
				}
				if (GetMaxMz(i) > MzMaxMax){
					MzMaxMax = GetMaxMz(i);
				}
			}
			intensity = (float) CalcIntensity(intensDet);
			MzCentroidAvg = CalcAverageMzNoCalib(this, ArrayUtils.ConsecutiveInts(Count));
			if (double.IsNaN(MzCentroidAvg)){
				throw new Exception("double.IsNaN(CenterMass)");
			}
		}
		private void CalcAveragesM(IntensityDetermination intensDet, CubicSpline[] mzCalibrationPar,
			LinearInterpolator intensDependentCalibration, bool isPpm, double[] iit){
			MzMinMin = double.MaxValue;
			MzMaxMax = double.MinValue;
			for (int i = 0; i < Count; i++){
				if (GetMinMz(i) < MzMinMin){
					MzMinMin = GetMinMz(i);
				}
				if (GetMaxMz(i) > MzMaxMax){
					MzMaxMax = GetMaxMz(i);
				}
			}
			intensity = (float) CalcIntensity(intensDet);
			MzCentroidAvg = GetCenterMzCalibMetabolite(mzCalibrationPar, intensDependentCalibration, isPpm, iit);
			if (double.IsNaN(MzCentroidAvg)){
				throw new Exception("double.IsNaN(CenterMass)");
			}
			MzCentroidUncalibAvg = GetCenterMzUncalibMetabolite(iit);
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
				case IntensityDetermination.SumSmoothFwhm: return SumFwhm(SmoothIntensities);
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
		private static double CalcAverageMzNoCalib(Peak peak, IEnumerable<int> indx){
			double norm1 = 0;
			double m = 0;
			foreach (int i in indx){
				double mz = peak.MzCentroid[i];
				if (!double.IsNaN(mz)){
					double intens = peak.OrigIntensityProfile[i];
					norm1 += intens;
					m += intens * mz;
				}
			}
			m /= norm1;
			return m;
		}

		//TODO: check
		private static double CalcAverageMz(Peak peak, IEnumerable<int> indx, CubicSpline[] mzCalibrationPar,
			bool isPpm){
			double norm1 = 0;
			double m = 0;
			foreach (int i in indx){
				double mz = peak.GetCalibCenterMz(i) *
				            (1 + MolUtil.RelativeCorrectionSpline(peak.GetCalibCenterMz(i), mzCalibrationPar,
					            peak.MassRange[i], isPpm));
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
			int[] ind = ArrayUtils.ConsecutiveInts(Count);
			if (mzCalibrationPar == null){
				return CalcAverageMzNoCalib(this, ind);
			}
			double mz = CalcAverageMz(this, ind, mzCalibrationPar, isPpm);
			if (double.IsNaN(intensity)){
				return mz;
			}
			double intens = 1.0 / Math.Log(intensity);
			return mz * (1 - (isPpm ? 1e-6 * intensDepCal.Get(intens) : intensDepCal.Get(intens) / mz));
		}
		public override double CalcAverageMzUncalibrated(){
			int[] ind = ArrayUtils.ConsecutiveInts(Count);
			return CalcAverageMzNoCalib(this, ind);
		}
		public void RemoveDoublePeaks(IntensityDetermination intensDet){
			CalcAverages(intensDet);
			int len = LastScanIndex - FirstScanIndex + 1;
			List<int>[] lists = new List<int>[len];
			for (int i = 0; i < len; i++){
				lists[i] = new List<int>();
			}
			for (int i = 0; i < Count; i++){
				lists[ScanIndices[i] - FirstScanIndex].Add(i);
			}
			List<int> valids = new List<int>();
			foreach (List<int> list in lists){
				if (list.Count == 1){
					valids.Add(list[0]);
				} else if (list.Count > 1){
					double[] dm = new double[list.Count];
					for (int i = 0; i < list.Count; i++){
						dm[i] = Math.Abs(MzCentroid[list[i]] - MzCentroidAvg);
					}
					int ind = ArrayUtils.Order(dm)[0];
					valids.Add(list[ind]);
				}
			}
			if (valids.Count < len){
				MzCentroid = MzCentroid.SubArray(valids);
				mzMin = mzMin.SubArray(valids);
				mzMax = mzMax.SubArray(valids);
				gaps = gaps.SubArray(valids);
				HasLeftNeighbor = HasLeftNeighbor.SubArray(valids);
				HasRightNeighbor = HasRightNeighbor.SubArray(valids);
				SmoothIntensities = SmoothIntensities.SubArray(valids);
				OrigIntensityProfile = OrigIntensityProfile.SubArray(valids);
				ScanIndices = ScanIndices.SubArray(valids);
				CalcAverages(intensDet);
			}
		}
		public void Smooth(MsInstrument msInstrument, IntensityDetermination intensityDetermination){
			mzMin = SmoothMean2(mzMin, 2);
			mzMax = SmoothMean2(mzMax, 2);
			SmoothIntensities = msInstrument.SmoothIntensityProfile(OrigIntensityProfile);
			CalcAverages(intensityDetermination);
		}
		private Peak[] SplitAtValley(double valleyFactor, bool advancedPeakSplitting,
			IntensityDetermination intensityDetermination){
			const int minLen = 4;
			const int maxLen = 7;
			const double pval = 0.01;
			IEnumerable<int> minPos = CalcLocalMinPositions(SmoothIntensities);
			foreach (int pos in minPos){
				double leftMax = GetLeftMax(pos, SmoothIntensities);
				double rightMax = GetRightMax(pos, SmoothIntensities);
				double smallMax = Math.Min(leftMax, rightMax);
				if (smallMax / SmoothIntensities[pos] > valleyFactor){
					return new[]{
						SubPeak(0, pos, intensityDetermination), SubPeak(pos + 1, Count, intensityDetermination)
					};
				}
				if (advancedPeakSplitting){
					if (pos >= minLen && pos < SmoothIntensities.Length - minLen){
						int start = Math.Max(0, pos - maxLen);
						double[] x = MzCentroid.SubArray(start, pos);
						int end = Math.Min(MzCentroid.Length, pos + maxLen + 1);
						double[] y = MzCentroid.SubArray(pos + 1, end);
						double p = test.GetPvalue(x, y);
						if (p <= pval){
							return new[]{
								SubPeak(0, pos, intensityDetermination), SubPeak(pos + 1, Count, intensityDetermination)
							};
						}
					}
				}
			}
			return null;
		}
		public static int[] CalcLocalMinPositions(float[] y){
			List<int> result = new List<int>();
			for (int i = 2; i < y.Length - 2; i++){
				double m2 = y[i - 2];
				double m1 = y[i - 1];
				double x = y[i];
				double p1 = y[i + 1];
				double p2 = y[i + 2];
				if (IsMin(x, m1, p1, m2, p2)){
					result.Add(i);
				}
			}
			int[] minPos = result.ToArray();
			float[] minY = y.SubArray(minPos);
			return minPos.SubArray(ArrayUtils.Order(minY));
		}
		private static bool IsMin(double x, double m1, double p1, double m2, double p2){
			if (x < m1 && x < p1){
				return true;
			}
			if (Math.Abs(x - m1) < double.Epsilon && x < m2 && x < p1){
				return true;
			}
			if (x < m1 && Math.Abs(x - p1) < double.Epsilon && x < p2){
				return true;
			}
			return x < m2 && Math.Abs(x - m1) < double.Epsilon && Math.Abs(x - p1) < double.Epsilon && x < p2;
		}
		private Peak[] SplitAtGap(double gapFactor, IntensityDetermination intensityDetermination){
			float[] intensities = OrigIntensityProfile;
			double totalIntens = ArrayUtils.Sum(intensities);
			double sumIntens = intensities[0];
			for (int i = 1; i < intensities.Length; i++){
				if (gaps[i]){
					double r = sumIntens / totalIntens;
					if (r < gapFactor || 1 - r < gapFactor){
						return new[]{SubPeak(0, i, intensityDetermination), SubPeak(i, Count, intensityDetermination)};
					}
					if (CenterMassesDiffer(i)){
						return new[]{SubPeak(0, i, intensityDetermination), SubPeak(i, Count, intensityDetermination)};
					}
				}
				sumIntens += intensities[i];
			}
			return null;
		}
		private static readonly TwoSampleTTest test = new TwoSampleTTest();
		private bool CenterMassesDiffer(int index){
			double[] x1 = MzCentroid.SubArray(index);
			double[] x2 = MzCentroid.SubArray(index, MzCentroid.Length);
			double p = test.GetPvalue(x1, x2);
			return p <= 0.01;
		}
		public static float[] SmoothMean2(float[] m, int width){
			float[] result = new float[m.Length];
			for (int i = 0; i < result.Length; i++){
				int min = Math.Max(0, i - width);
				int max = Math.Min(result.Length - 1, i + width);
				result[i] = (float) Average(m, min, max, max - min + 1);
			}
			return result;
		}
		private static double Average(IList<float> m, int min, int max, int len){
			double sum = 0;
			for (int i = min; i <= max; i++){
				sum += m[i];
			}
			return sum / len;
		}
		/// <summary>
		/// Reduce a Peak to a subset of scans.
		/// </summary>
		public Peak SubPeak(int start, int end, IntensityDetermination intensityDetermination){
			Peak result = new Peak(MzCentroid.SubArray(start, end), mzMin.SubArray(start, end),
				mzMax.SubArray(start, end), SmoothIntensities.SubArray(start, end),
				OrigIntensityProfile.SubArray(start, end), ScanIndices.SubArray(start, end),
				MassRange.SubArray(start, end), gaps.SubArray(start, end), intensityDetermination,
				HasLeftNeighbor.SubArray(start, end), HasRightNeighbor.SubArray(start, end), Resolution);
			result.CalcAverages(intensityDetermination);
			return result;
		}
		/// <summary>
		/// "Translate" <code>dmx</code> into MzCorrection. 
		/// </summary>
		/// <param name="dmx"></param>
		/// <param name="isPpm">tells you whether <code>dmx</code> is expressed in ppm or absolute m/z.</param>
		/// <param name="intensityDetermination"></param>
		public void CorrectMasses(double[] dmx, bool isPpm, IntensityDetermination intensityDetermination){
			for (int i = 0; i < MzCentroid.Length; i++){
				int ind = ScanIndices[i];
				double dm = dmx[ind];
				if (isPpm){
					MzCorrection[i] = -MzCentroid[i] * dm * 1e-6;
				} else{
					MzCorrection[i] = -dm;
				}
			}
			CalcAverages(intensityDetermination);
		}
		public void CorrectMassesM(double[] dmx, bool isPpm, IntensityDetermination intensityDetermination,
			CubicSpline[] mzCalibrationPar, LinearInterpolator intensDependentCalibration, double[] iit){
			for (int i = 0; i < MzCentroid.Length; i++){
				int ind = ScanIndices[i];
				double dm = dmx[ind];
				if (isPpm){
					MzCorrection[i] -= MzCentroid[i] * dm * 1e-6;
				} else{
					MzCorrection[i] -= dm;
				}
			}
			double[] mz1 = new double[Count];
			double[] w1 = new double[Count];
			for (int index = 0; index < Count; index++){
				mz1[index] =
					GetCenterMzCalibMetaboliteAt(index, mzCalibrationPar, intensDependentCalibration, isPpm, iit);
				w1[index] = Math.Sqrt(GetOriginalIntensity(index, iit));
			}
			double err = 0;
			double norm = 0;
			for (int index = 0; index < Count - 1; index++){
				double m1 = mz1[index];
				double m2 = mz1[index + 1];
				double ppmErr = Math.Abs(m1 - m2) * 2.0 / (m1 + m2) * 1e6;
				double w = Math.Min(w1[index], w1[index + 1]);
				err += ppmErr * w;
				norm += w;
			}
			err /= norm;
			Jitter = (float) (err * Math.Pow(ArrayUtils.Max(w1), 0.44));
			CalcAveragesM(intensityDetermination, mzCalibrationPar, intensDependentCalibration, isPpm, iit);
		}
		public void CalcCalibrationMatrix(double[,] a, double[] b, bool[] valid){
			int[] inds = ScanIndices;
			double[] mzs = MzCentroid;
			int np = mzs.Length;
			double sumM = 0;
			foreach (double t in MzCentroid){
				sumM += t;
			}
			//a[k,j]
			//k..equation index
			foreach (int k in inds){
				b[k] += mzs[k] * mzs[k] - mzs[k] * sumM / np;
				a[k, k] += mzs[k] * mzs[k] * 1.0e-6;
				valid[k] = true;
				foreach (int j in inds){
					a[k, j] -= mzs[k] * mzs[j] * 1.0e-6 / np;
				}
			}
		}
		public static double GetLeftMax(int pos, float[] y){
			double max = double.MinValue;
			for (int i = 0; i < pos; i++){
				if (y[i] > max){
					max = y[i];
				}
			}
			return max;
		}
		public static double GetRightMax(int pos, float[] y){
			double max = double.MinValue;
			for (int i = pos + 1; i < y.Length; i++){
				if (y[i] > max){
					max = y[i];
				}
			}
			return max;
		}
		public Peak[] DecomposeGaps(double gapFactor, IntensityDetermination intensityDetermination){
			List<Peak> splitCandidates = new List<Peak>();
			List<Peak> result = new List<Peak>();
			splitCandidates.Add(this);
			while (splitCandidates.Count > 0){
				Peak maybeSplitMe = splitCandidates[0];
				splitCandidates.Remove(maybeSplitMe);
				Peak[] w = maybeSplitMe.SplitAtGap(gapFactor, intensityDetermination);
				if (w == null){
					result.Add(maybeSplitMe);
				} else{
					splitCandidates.Add(w[0]);
					splitCandidates.Add(w[1]);
					maybeSplitMe.Dispose();
				}
			}
			return result.ToArray();
		}
		public Peak[] DecomposeValleys(double valleyFactor, bool advancedPeakSplitting, bool split,
			IntensityDetermination intensityDetermination){
			if (!split){
				return new[]{this};
			}
			List<Peak> splitCandidates = new List<Peak>();
			List<Peak> result = new List<Peak>();
			splitCandidates.Add(this);
			while (splitCandidates.Count > 0){
				Peak maybeSplitMe = splitCandidates[0];
				splitCandidates.Remove(maybeSplitMe);
				Peak[] w = maybeSplitMe.SplitAtValley(valleyFactor, advancedPeakSplitting, intensityDetermination);
				if (w == null){
					result.Add(maybeSplitMe);
				} else{
					splitCandidates.Add(w[0]);
					splitCandidates.Add(w[1]);
					maybeSplitMe.Dispose();
				}
			}
			return result.ToArray();
		}
		public override void Dispose(){
			base.Dispose();
			MzCentroid = null;
			MzCorrection = null;
			mzMin = null;
			mzMax = null;
			SmoothIntensities = null;
			OrigIntensityProfile = null;
			ScanIndices = null;
			MassRange = null;
		}
		public bool Contains(double mass, int ms1Ind){
			int ind = ArrayUtils.FloorIndex(ScanIndices, ms1Ind);
			if (ind < 0 || ind >= Count){
				return false;
			}
			if (mass > GetMaxMz(ind)){
				return false;
			}
			return !(mass < GetMinMz(ind));
		}
		public override double GetIntensityAtScanIndex(int scanInd, out double mz){
			int a = Array.BinarySearch(ScanIndices, scanInd);
			if (a < 0){
				mz = double.NaN;
				return double.NaN;
			}
			mz = GetCalibCenterMz(a);
			return OrigIntensityProfile[a];
		}
		public void GetPolygon(RawLayer rf, out double[] x, out double[] y){
			List<double> x1 = new List<double>();
			List<double> y1 = new List<double>();
			for (int i = 0; i < ScanIndices.Length; i++){
				x1.Add(GetMinMz(i));
				y1.Add(rf.GetTime(ScanIndices[i]));
			}
			for (int i = ScanIndices.Length - 1; i >= 0; i--){
				x1.Add(GetMaxMz(i));
				y1.Add(rf.GetTime(ScanIndices[i]));
			}
			x = x1.ToArray();
			y = y1.ToArray();
		}
		public float GetSmoothIntensity(int index) {
			return SmoothIntensities[index];
		}
	}
}