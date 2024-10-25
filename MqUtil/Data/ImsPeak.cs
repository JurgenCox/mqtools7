using MqApi.Num;
using MqApi.Util;
using MqUtil.Mol;
using MqUtil.Ms.Enums;
using MqUtil.Ms.Utils;
using MqUtil.Num;

namespace MqUtil.Data{
	public sealed class ImsPeak : GenericPeak{
		private List<Peak2> Peaks{ get; set; }
		private List<int> SliceInds{ get; set; }
		private int count = -1;
		private double mz = double.NaN;
		private float intensity = float.NaN;
		private float imsRtCorr = float.NaN;
		private float mzRtCorr = float.NaN;
		private float mzImsCorr = float.NaN;
		public int MinFrameInd{ get; set; }
		public int MaxFrameInd{ get; set; }
		/// <summary>
		/// This is reduced by step size.
		/// </summary>
		public int MinImsScanInd{ get; set; }
		/// <summary>
		/// This is reduced by step size. Inclusive maximum.
		/// </summary>
		public int MaxImsScanInd{ get; set; }
		//transient
		public float[] SmoothIntensities { get; set; }
		public ImsPeak(BinaryReader reader){
			SliceInds = new List<int>();
			int len = reader.ReadInt32();
			for (int i = 0; i < len; i++){
				SliceInds.Add(reader.ReadInt32());
			}
			ScanIndices = FileUtils.ReadInt32Array(reader);
			OrigIntensityProfile = FileUtils.ReadFloatArray(reader);
			MinFrameInd = reader.ReadInt32();
			MaxFrameInd = reader.ReadInt32();
			MinImsScanInd = reader.ReadInt32();
			MaxImsScanInd = reader.ReadInt32();
			count = reader.ReadInt32();
			mz = reader.ReadDouble();
			intensity = reader.ReadSingle();
			imsRtCorr = reader.ReadSingle();
			mzRtCorr = reader.ReadSingle();
			mzImsCorr = reader.ReadSingle();
			Peaks = new List<Peak2>();
			len = reader.ReadInt32();
			for (int i = 0; i < len; i++){
				Peaks.Add(new Peak2(reader));
			}
		}
		public ImsPeak(PeakCluster peakCluster, IntensityDetermination intensityDetermination, 
            bool hasImsRtCorrelation, bool hasMassRecal) {
			(MinFrameInd, MaxFrameInd, SliceInds, ScanIndices) = GetMinMaxFrameInds(peakCluster);
			MinImsScanInd = SliceInds[0];
			MaxImsScanInd = SliceInds[SliceInds.Count - 1];
			OrigIntensityProfile = CreateIntensityProfiles(peakCluster);
			Peaks = CreatePeaks(peakCluster, intensityDetermination, hasMassRecal);
			if (hasImsRtCorrelation){
				CalcImsRtCorrelation(Peaks);
			}
		}
		public void Write(BinaryWriter writer){
			writer.Write(SliceInds.Count);
			foreach (int t in SliceInds){
				writer.Write(t);
			}
			FileUtils.Write(ScanIndices, writer);
			FileUtils.Write(OrigIntensityProfile, writer);
			writer.Write(MinFrameInd);
			writer.Write(MaxFrameInd);
			writer.Write(MinImsScanInd);
			writer.Write(MaxImsScanInd);
			writer.Write(count);
			writer.Write(mz);
			writer.Write(intensity);
			writer.Write(imsRtCorr);
			writer.Write(mzRtCorr);
			writer.Write(mzImsCorr);
			writer.Write(Peaks.Count);
			foreach (Peak2 t in Peaks){
				t.Write(writer);
			}
		}
		public static ImsPeak ReadCompressed(BinaryReader reader){
			return FromBytes(FileUtils.Decompress(ReadByteArray1(reader)));
		}
		private static ImsPeak FromBytes(byte[] bytes){
			using (MemoryStream fs = new MemoryStream(bytes))
			using (BinaryReader br = new BinaryReader(fs))
				return new ImsPeak(br);
		}
		public override int Count{
			get{
				if (count < 0){
					count = 0;
					foreach (Peak2 t in Peaks){
						count += t.Count;
					}
				}
				return count;
			}
		}
		public override int FirstScanIndex{
			get{
				int f = int.MaxValue;
				foreach (Peak2 peak in Peaks){
					f = Math.Min(f, peak.FirstScanIndex);
				}
				return f;
			}
		}
		public override int LastScanIndex{
			get{
				int f = int.MinValue;
				foreach (Peak2 peak in Peaks){
					f = Math.Max(f, peak.LastScanIndex);
				}
				return f;
			}
		}
		public (bool, bool) GetHasNeighbors(){
			bool hasLeftNeighbor = false;
			bool hasRightNeighbor = false;
			float maxIntens = float.MinValue;
			foreach (Peak2 peak in Peaks){
				for (int i = 0; i < peak.OrigIntensityProfile.Length; i++){
					float intens = peak.OrigIntensityProfile[i];
					if (intens > maxIntens){
						maxIntens = intens;
						hasLeftNeighbor = peak.HasLeftNeighbor[i];
						hasRightNeighbor = peak.HasRightNeighbor[i];
					}
				}
			}
			return (hasLeftNeighbor, hasRightNeighbor);
		}
		public void GetProfile(out int[] frameInds, out int[] imsScanInds, out float[] smintens){
			List<int> frameInds1 = new List<int>();
			List<int> imsScanInds1 = new List<int>();
			List<float> smintens1 = new List<float>();
			for (int index = 0; index < Peaks.Count; index++){
				Peak2 peak = Peaks[index];
				for (int i = 0; i < peak.Count; i++){
					imsScanInds1.Add(SliceInds[index]);
					frameInds1.Add(peak.ScanIndices[i]);
					smintens1.Add(peak.OrigIntensityProfile[i]);
				}
			}
			frameInds = frameInds1.ToArray();
			imsScanInds = imsScanInds1.ToArray();
			smintens = smintens1.ToArray();
		}
		public int GetMaxIntensityScanIndex(out int sliceIndex){
			int[] inds = new int[Peaks.Count];
			double[] intens = new double[Peaks.Count];
			for (int i = 0; i < Peaks.Count; i++){
				Peak2 peak = Peaks[i];
				int x = peak.OrigIntensityProfile.MaxInd();
				intens[i] = peak.OrigIntensityProfile[x];
				inds[i] = peak.ScanIndices[x];
			}
			int sliceInd = ArrayUtils.MaxInd(intens);
			sliceIndex = SliceInds[sliceInd];
			return inds[sliceInd];
		}
		public double GetIntensityAtIndex(int scanInd, int sliceInd, out double mz1){
			int a = SliceInds.BinarySearch(sliceInd);
			if (a < 0 || a >= SliceInds.Count){
				mz1 = double.NaN;
				return double.NaN;
			}
			Peak2 peak = Peaks[a];
			int x = Array.BinarySearch(peak.ScanIndices, scanInd);
			if (x < 0 || x >= peak.OrigIntensityProfile.Length){
				mz1 = double.NaN;
				return double.NaN;
			}
			mz1 = peak.GetCenterMz(x);
			return peak.OrigIntensityProfile[x];
		}
		public override double CalcAverageMz(CubicSpline[] mzCalibrationPar, LinearInterpolator intensDepCal,
			LinearInterpolator mobilityDepCal, bool isPpm, double intensity1, int imsStep) {
			double av = 0;
			double norm = 0;
			for (int index = 0; index < Peaks.Count; index++){
				Peak2 peak = Peaks[index];
				double avMz = peak.CalcAverageMz(mzCalibrationPar, intensDepCal, mobilityDepCal, isPpm, intensity1, 0);
				double x;
				if (mzCalibrationPar == null){
					x = avMz;
				} else{
					x = avMz * (1 + (isPpm
						? 1e-6 * mobilityDepCal.Get(SliceInds[index] * imsStep)
						: mobilityDepCal.Get(SliceInds[index] * imsStep) / avMz));
				}
				av += x * peak.Intensity;
				norm += peak.Intensity;
			}
			return av / norm;
		}
		public override double CalcAverageMzUncalibrated(){
			double av = 0;
			double norm = 0;
			foreach (Peak2 peak in Peaks){
				av += peak.CalcAverageMzUncalibrated() * peak.Intensity;
				norm += peak.Intensity;
			}
			return av / norm;
		}
		public double GetMz(){
			if (double.IsNaN(mz)){
				double res = 0;
				double norm = 0;
				foreach (Peak2 peak in Peaks){
					res += peak.MzCentroidAvg * peak.Intensity;
					norm += peak.Intensity;
				}
				mz = res / norm;
			}
			return mz;
		}
		public double GetIntensityForFrame(int frameInd){
			double result = 0;
			foreach (Peak2 p in Peaks){
				double x = p.GetIntensityAtScanIndex(frameInd, out double _);
				if (!double.IsNaN(x)){
					result += x;
				}
			}
			return result;
		}
		public override void Dispose(){
			base.Dispose();
			if (SliceInds != null){
				SliceInds.Clear();
				SliceInds = null;
			}
			if (Peaks != null){
				foreach (Peak2 peak in Peaks){
					peak.Dispose();
				}
				Peaks.Clear();
				Peaks = null;
			}
		}
		public Peak GetPeakAtFrameInd(int frameInd){
			List<double> centerMz = new List<double>();
			List<double> minMz = new List<double>();
			List<double> maxMz = new List<double>();
			List<float> smoothIntens = new List<float>();
			List<float> origIntens = new List<float>();
			List<int> scanIndices = new List<int>();
			List<byte> massRange = new List<byte>();
			List<bool> hasLeftNeighbor = new List<bool>();
			List<bool> hasRightNeighbor = new List<bool>();
			List<bool> gaps = new List<bool>();
			if (MinFrameInd > frameInd || MaxFrameInd < frameInd){
				return null;
			}
			for (int j = 0; j < SliceInds.Count; j++){
				int imsInd = SliceInds[j];
				Peak2 p1 = Peaks[j];
				int x = SearchFrameInd(p1.ScanIndices, frameInd);
				centerMz.Add(p1.GetCenterMz(x));
				minMz.Add(p1.GetMinMz(x));
				maxMz.Add(p1.GetMaxMz(x));
				hasLeftNeighbor.Add(p1.GetHasLeftNeighbor(x));
				hasRightNeighbor.Add(p1.GetHasRightNeighbor(x));
				origIntens.Add(p1.GetOriginalIntensity(x));
				smoothIntens.Add(p1.GetOriginalIntensity(x));
				//TODO?
				scanIndices.Add(imsInd);
				massRange.Add(0);
				gaps.Add(false);
			}
			return new Peak(centerMz.ToArray(), ArrayUtils.ToFloats(minMz.ToArray()),
				ArrayUtils.ToFloats(maxMz.ToArray()), smoothIntens.ToArray(), origIntens.ToArray(),
				scanIndices.ToArray(), massRange.ToArray(), gaps.ToArray(), IntensityDetermination.SumTotal,
				hasLeftNeighbor.ToArray(), hasRightNeighbor.ToArray(), float.NaN);
		}
		public Peak GetPeak(int imsStep) {
			int[] allFrameInds = GetAllFrameInds();
			int[] minImsInds = ArrayUtils.FillArray(int.MaxValue, allFrameInds.Length);
			int[] maxImsInds = ArrayUtils.FillArray(int.MinValue, allFrameInds.Length);
			bool any = false;
			for (int index = 0; index < Peaks.Count; index++) {
				Peak2 peak = Peaks[index];
				for (int j = 0; j < peak.Count; j++) {
					double minMz1 = peak.GetMinMz(j);
					double maxMz1 = peak.GetMaxMz(j);
					double mz1 = GetMz();
					if (mz1 >= minMz1 && mz1 <= maxMz1) {
						any = true;
						int frameInd = peak.GetScanIndex(j);
						int a = Array.BinarySearch(allFrameInds, frameInd);
						int imsInd = SliceInds[index] * imsStep;
						minImsInds[a] = Math.Min(minImsInds[a], imsInd);
						maxImsInds[a] = Math.Max(maxImsInds[a], imsInd);
					}
				}
			}
			if (!any){
				return null;
			}
			List<int> valids = new List<int>();
			for (int j = 0; j < allFrameInds.Length; j++){
				if (minImsInds[j] != int.MaxValue){
					valids.Add(j);
				}
			}
			List<double> centerMz = new List<double>();
			List<double> minMz = new List<double>();
			List<double> maxMz = new List<double>();
			List<float> smoothIntens = new List<float>();
			List<float> origIntens = new List<float>();
			List<int> scanIndices = new List<int>();
			List<byte> massRange = new List<byte>();
			List<bool> hasLeftNeighbor = new List<bool>();
			List<bool> hasRightNeighbor = new List<bool>();
			List<bool> gaps = new List<bool>();
			foreach (int ind in valids){
				centerMz.Add(0.5 * (minImsInds[ind] + maxImsInds[ind]));
				minMz.Add(minImsInds[ind]);
				maxMz.Add(maxImsInds[ind]);
				hasLeftNeighbor.Add(false);
				hasRightNeighbor.Add(false);
				scanIndices.Add(allFrameInds[ind]);
				massRange.Add(0);
				origIntens.Add(1);
				smoothIntens.Add(1);
				gaps.Add(false);
			}
			return new Peak(centerMz.ToArray(), ArrayUtils.ToFloats(minMz.ToArray()),
				ArrayUtils.ToFloats(maxMz.ToArray()), smoothIntens.ToArray(), origIntens.ToArray(),
				scanIndices.ToArray(), massRange.ToArray(), gaps.ToArray(), IntensityDetermination.SumTotal,
				hasLeftNeighbor.ToArray(), hasRightNeighbor.ToArray(), float.NaN);
		}
		private int[] GetAllFrameInds(){
			HashSet<int> a = new HashSet<int>();
			foreach (Peak2 peak in Peaks){
				foreach (int scanIndex in peak.ScanIndices){
					a.Add(scanIndex);
				}
			}
			int[] result = a.ToArray();
			Array.Sort(result);
			return result;
		}
		public float[,] CalcProfile(out int minInd0, out int minInd1, out double[,] mzVals){
			minInd0 = int.MaxValue;
			int maxInd0 = int.MinValue;
			foreach (Peak2 peak in Peaks){
				ArrayUtils.MinMax(peak.ScanIndices, out int minInd, out int maxInd);
				minInd0 = Math.Min(minInd0, minInd);
				maxInd0 = Math.Max(maxInd0, maxInd);
			}
			minInd1 = MinImsScanInd;
			int maxInd1 = MaxImsScanInd;
			float[,] profile = new float[maxInd0 - minInd0 + 1, maxInd1 - minInd1 + 1];
			mzVals = new double[maxInd0 - minInd0 + 1, maxInd1 - minInd1 + 1];
			for (int index = 0; index < Peaks.Count; index++){
				Peak2 peak = Peaks[index];
				int[] scanIndices = peak.ScanIndices;
				float[] intensities = peak.OrigIntensityProfile;
				for (int i = 0; i < intensities.Length; i++){
					profile[scanIndices[i] - minInd0, index] = intensities[i];
					mzVals[scanIndices[i] - minInd0, index] = peak.GetCenterMz(i);
				}
			}
			return profile;
		}
		public void Calc1(ref int minFrameInd, ref int maxFrameInd, ref int minTimsSliceInd, ref int maxTimsSliceInd){
			foreach (Peak2 peak in Peaks){
				minFrameInd = Math.Min(minFrameInd, peak.FirstScanIndex);
				maxFrameInd = Math.Max(maxFrameInd, peak.LastScanIndex);
			}
			minTimsSliceInd = Math.Min(minTimsSliceInd, MinImsScanInd);
			maxTimsSliceInd = Math.Max(maxTimsSliceInd, MaxImsScanInd);
		}
		public void Calc2(ref int npoints, double[] rt, double[] rtIntensProfile, double[] timsIntensProfile,
			ref double minRt, ref double maxRt, int minFrameInd, int minTimsSliceInd){
			for (int index = 0; index < Peaks.Count; index++){
				Peak2 peak = Peaks[index];
				int indTims = SliceInds[index];
				for (int j = 0; j < peak.Count; j++){
					npoints += peak.Count;
					double intens = peak.OrigIntensityProfile[j];
					int ind = peak.ScanIndices[j];
					double rt1 = rt[ind];
					if (rt1 > maxRt){
						maxRt = rt1;
					}
					if (rt1 < minRt){
						minRt = rt1;
					}
					rtIntensProfile[ind - minFrameInd] += intens;
					timsIntensProfile[indTims - minTimsSliceInd] += intens;
				}
			}
		}
		public void Calc3(int minInd0, int minInd1, float[,] profile, double[,] mzVals){
			int d1 = MinImsScanInd - minInd1;
			for (int index = 0; index < Peaks.Count; index++){
				Peak2 peak = Peaks[index];
				int[] scanIndices = peak.ScanIndices;
				float[] intensities = peak.OrigIntensityProfile;
				for (int i1 = 0; i1 < intensities.Length; i1++){
					profile[scanIndices[i1] - minInd0, d1 + index] = intensities[i1];
					mzVals[scanIndices[i1] - minInd0, d1 + index] = peak.GetCenterMz(i1);
				}
			}
		}
		public void Calc4(ref int minInd0, ref int maxInd0, ref int minInd1, ref int maxInd1){
			foreach (Peak2 peak in Peaks){
				ArrayUtils.MinMax(peak.ScanIndices, out int minInd, out int maxInd);
				minInd0 = Math.Min(minInd0, minInd);
				maxInd0 = Math.Max(maxInd0, maxInd);
			}
			minInd1 = Math.Min(minInd1, MinImsScanInd);
			maxInd1 = Math.Max(maxInd1, MaxImsScanInd);
		}
		public void Calc5(int charge, int iind, ref double result, ref double intensity1, ref double avMass){
			foreach (Peak2 peak in Peaks){
				for (int index0 = 0; index0 < peak.Count; index0++){
					double mz1 = peak.GetCenterMz(index0);
					if (!double.IsNaN(mz1)){
						double intens1 = peak.OrigIntensityProfile[index0];
						intensity1 += intens1;
						double mx = (mz1 - Molecule.massProton) * charge;
						double diff = MolUtil.GetAverageDifferenceToMonoisotope(mx, iind);
						result += intens1 * (mx - diff);
						avMass += intens1 * mx;
					}
				}
			}
		}
		public void Calc6(int charge, List<double> masses, List<double> weights, int ind, double massShift){
			foreach (Peak2 peak in Peaks){
				for (int index0 = 0; index0 < peak.Count; index0++){
					weights.Add(peak.GetOriginalIntensity(index0));
					double mz1 = peak.GetCenterMz(index0);
					double m = (mz1 - Molecule.massProton) * charge;
					masses.Add(
						m - MolUtil.GetAverageDifferenceToMonoisotope(m, ind) -
						massShift);
				}
			}
		}
		public void Calc7(int charge, List<double> masses, List<double> weights, int ind, double massShift){
			foreach (Peak2 peak in Peaks){
				for (int index0 = 0; index0 < peak.Count; index0++){
					weights.Add(peak.GetOriginalIntensity(index0));
					double mz1 = peak.GetCenterMz(index0);
					double m = (mz1 - Molecule.massProton) * charge;
					masses.Add(m - (ind) *
						MolUtil.GetAverageDifferenceToMonoisotope(m, 1) - massShift);
				}
			}
		}
		public (int, int, double, double) Calc0(){
			int minFrameInd = int.MaxValue;
			int maxFrameInd = int.MinValue;
			double mzx = 0;
			double intens = 0;
			foreach (Peak2 p1 in Peaks){
				double mz1 = p1.MzCentroidAvg;
				double intens1 = p1.Intensity;
				mzx += mz1 * intens1;
				intens += intens1;
				minFrameInd = Math.Min(p1.FirstScanIndex, minFrameInd);
				maxFrameInd = Math.Max(p1.LastScanIndex, maxFrameInd);
			}
			mzx /= intens;
			return (minFrameInd, maxFrameInd, mzx, intens);
		}
		public Peak GetPeakAtImsInd(int imsInd){
			for (int j = 0; j < SliceInds.Count; j++){
				if (SliceInds[j] == imsInd){
					return Peaks[j].ToPeak();
				}
			}
			return null;
		}
		private float[] CreateIntensityProfiles(PeakCluster peakCluster){
			float[] origProfile = new float[ScanIndices.Length];
			int[] counts = new int[ScanIndices.Length];
			foreach (int frameInd in peakCluster.data.Keys){
				int index = Array.BinarySearch(ScanIndices, frameInd);
				foreach (WritablePeak peak in peakCluster.data[frameInd]){
					origProfile[index] += (float) ArrayUtils.Sum(peak.p.OrigIntensityProfile);
					counts[index]++;
				}
			}
			for (int i = 0; i < counts.Length; i++){
				if (counts[i] > 0){
					origProfile[i] /= counts[i];
				}
			}
			return origProfile;
		}
		private static (int, int, List<int>, int[]) GetMinMaxFrameInds(PeakCluster peakCluster){
			int minFrameInd = int.MaxValue;
			int maxFrameInd = int.MinValue;
			HashSet<int> sliceInds = new HashSet<int>();
			HashSet<int> frameInds = new HashSet<int>();
			foreach (int frameInd in peakCluster.data.Keys){
				frameInds.Add(frameInd);
				minFrameInd = Math.Min(minFrameInd, frameInd);
				maxFrameInd = Math.Max(maxFrameInd, frameInd);
				foreach (WritablePeak peak in peakCluster.data[frameInd]){
					foreach (int scanIndex in peak.p.ScanIndices){
						sliceInds.Add(scanIndex);
					}
				}
			}
			int[] si = sliceInds.ToArray();
			Array.Sort(si);
			int[] fi = frameInds.ToArray();
			Array.Sort(fi);
			return (minFrameInd, maxFrameInd, new List<int>(si), fi);
		}
		internal static int SearchFrameInd(int[] frameInds, int frameInd){
			int a = Array.BinarySearch(frameInds, frameInd);
			if (a >= 0){
				return a;
			}
			a = -1 - a;
			if (a < frameInds.Length){
				return a;
			}
			return a - 1;
		}
		internal static byte[] ReadByteArray1(BinaryReader reader){
			int n = reader.ReadInt32();
			return reader.ReadBytes(n);
		}
		private byte[] GetBytes(){
			using (MemoryStream fs = new MemoryStream())
			using (BinaryWriter bw = new BinaryWriter(fs)){
				Write(bw);
				bw.Flush();
				fs.Flush();
				byte[] bytes = fs.ToArray();
				fs.Close();
				return bytes;
			}
		}
		public void WriteCompressed(BinaryWriter writer){
			FileUtils.Write(FileUtils.Compress(GetBytes()), writer);
		}
		private List<Peak2> CreatePeaks(PeakCluster peakCluster, IntensityDetermination intensityDetermination,
			bool hasMassRecal){
			int[] slInds = SliceInds.ToArray();
			Tuple<int, int, int>[,] a = new Tuple<int, int, int>[ScanIndices.Length, SliceInds.Count];
			foreach (int frameInd in peakCluster.data.Keys){
				int ind0 = Array.BinarySearch(ScanIndices, frameInd);
				for (int index = 0; index < peakCluster.data[frameInd].Count; index++){
					WritablePeak peak = peakCluster.data[frameInd][index];
					for (int i = 0; i < peak.p.ScanIndices.Length; i++){
						int scanIndex = peak.p.ScanIndices[i];
						int ind1 = Array.BinarySearch(slInds, scanIndex);
						a[ind0, ind1] = new Tuple<int, int, int>(frameInd, index, i);
					}
				}
			}
			List<Peak2> peaks = new List<Peak2>();
			for (int i = 0; i < SliceInds.Count; i++){
				List<double> centerMzs = new List<double>();
				List<float> origIntensityProfile = new List<float>();
				List<int> scanIndices = new List<int>();
				List<bool> hasLeftNeighbor = new List<bool>();
				List<bool> hasRightNeighbor = new List<bool>();
				for (int j = 0; j < ScanIndices.Length; j++){
					Tuple<int, int, int> aa = a[j, i];
					if (aa != null){
						int frameInd = aa.Item1;
						int i1 = aa.Item2;
						int i2 = aa.Item3;
						Peak p1 = peakCluster.data[frameInd][i1].p;
						centerMzs.Add(p1.GetCenterMz(i2));
						origIntensityProfile.Add(p1.GetOriginalIntensity(i2));
						hasLeftNeighbor.Add(p1.GetHasLeftNeighbor(i2));
						hasRightNeighbor.Add(p1.GetHasRightNeighbor(i2));
						scanIndices.Add(frameInd);
					}
				}
				peaks.Add(new Peak2(centerMzs.ToArray(), origIntensityProfile.ToArray(), scanIndices.ToArray(),
					intensityDetermination, hasLeftNeighbor.ToArray(), hasRightNeighbor.ToArray(), 
                hasMassRecal));
			}
			return peaks;
		}
		private void CalcImsRtCorrelation(List<Peak2> peaks){
			double avIms = 0;
			double avRt = 0;
			double avMz = 0;
			double norm = 0;
			for (int i = 0; i < peaks.Count; i++){
				double imsInd = SliceInds[i];
				Peak2 p = peaks[i];
				for (int j = 0; j < p.Count; j++){
					double rtInd = p.ScanIndices[j];
					double mz1 = p.GetCenterMz(j);
					double we = p.OrigIntensityProfile[j];
					avIms += imsInd * we;
					avRt += rtInd * we;
					avMz += mz1 * we;
					norm += we;
				}
			}
			avIms /= norm;
			avRt /= norm;
			avMz /= norm;
			double sqIms = 0;
			double sqRt = 0;
			double sqMz = 0;
			double mzIms = 0;
			double mzRt = 0;
			double imsRt = 0;
			for (int i = 0; i < peaks.Count; i++){
				double imsInd = SliceInds[i] - avIms;
				Peak2 p = peaks[i];
				for (int j = 0; j < p.Count; j++){
					double rtInd = p.ScanIndices[j] - avRt;
					double mz1 = p.GetCenterMz(j) - avMz;
					double we = p.OrigIntensityProfile[j];
					sqIms += imsInd * imsInd * we;
					sqRt += rtInd * rtInd * we;
					sqMz += mz1 * mz1 * we;
					mzIms += mz1 * imsInd * we;
					mzRt += mz1 * rtInd * we;
					imsRt += imsInd * rtInd * we;
				}
			}
			sqIms /= norm;
			sqRt /= norm;
			sqMz /= norm;
			mzIms /= norm;
			mzRt /= norm;
			imsRt /= norm;
			imsRtCorr = (float) (imsRt / Math.Sqrt(sqRt * sqIms));
			mzRtCorr = (float) (mzRt / Math.Sqrt(sqRt * sqMz));
			mzImsCorr = (float) (mzIms / Math.Sqrt(sqMz * sqIms));
		}
		public override double GetIntensityAtScanIndex(int scanInd, out double mz1){
			int a = Array.BinarySearch(ScanIndices, scanInd);
			if (a < 0){
				mz1 = double.NaN;
				return double.NaN;
			}
			mz1 = GetMz();
			return OrigIntensityProfile[a];
		}
		public void WritePeak(BinaryWriter peakWriter, BinaryWriter indexWriter, double[] ms1Rt){
			(int minFrameInd, int maxFrameInd, double mzx, double intens) = Calc0();
			indexWriter.Write(mzx);
			indexWriter.Write(intens);
			indexWriter.Write(minFrameInd);
			indexWriter.Write(maxFrameInd);
			indexWriter.Write((float) ms1Rt[minFrameInd]);
			indexWriter.Write((float) ms1Rt[maxFrameInd]);
			indexWriter.Write(SliceInds[0]);
			indexWriter.Write(SliceInds[SliceInds.Count - 1]);
			(bool hasLeftNeighbor, bool hasRightNeighbor) = GetHasNeighbors();
			indexWriter.Write(hasLeftNeighbor);
			indexWriter.Write(hasRightNeighbor);
			indexWriter.Write(peakWriter.BaseStream.Position);
            Write(peakWriter);
		}
		public double GetMinMz(int i){
			double min = double.MaxValue;
			foreach (Peak2 peak in Peaks){
				min = Math.Min(min, peak.GetMinMz(0));
				;
			}
			return min;
		}
		public double GetMaxMz(int i){
			double max = double.MinValue;
			foreach (Peak2 peak in Peaks) {
				max = Math.Max(max, peak.GetMaxMz(0));
				;
			}
			return max;
		}
		public double GetCenterMz(int i){
			List<double> m = new List<double>();
			foreach (Peak2 peak in Peaks){
				m.Add(peak.GetCenterMz(0));
			}
			return ArrayUtils.Median(m);
		}
		public double Intensity{
			get{
				if (float.IsNaN(intensity)){
					intensity = CalcInensity();
				}
				return intensity;
			}
		}
		private float CalcInensity(){
			float result = 0.0f;
			foreach (Peak2 peak in Peaks){
				result += peak.Intensity;
			}
			return result;
		}
	}
}