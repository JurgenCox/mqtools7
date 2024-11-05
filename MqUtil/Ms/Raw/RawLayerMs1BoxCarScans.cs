using MqApi.Num;
using MqUtil.Ms.Utils;
using MqUtil.Num;

namespace MqUtil.Ms.Raw{
	public class RawLayerMs1BoxCarScans : RawLayer{
		private readonly RawFileLayer rawFile;
		public readonly int[] indsFullScan;
		private readonly int[][] inds;
		private readonly int[] firstInds;
		private double[] mzGrid;
		private double[][] transmissions;

		public RawLayerMs1BoxCarScans(RawFileLayer rawFile, int[][] inds){
			this.rawFile = rawFile;
			if (inds.Length > 0 && inds[0][0] == 0){
				inds = inds.SubArray(1, inds.Length);
			}
			this.inds = inds;
			firstInds = new int[inds.Length];
			indsFullScan = new int[inds.Length];
			for (int i = 0; i < firstInds.Length; i++){
				firstInds[i] = inds[i][0];
				indsFullScan[i] = inds[i][0] - 1;
			}
			DetermineMzGrid();
		}

		public override int Count => inds.Length;
		public override int MassRangeCount => rawFile.Ms1MassRangeCount;

		/// <summary>
		/// Return a two-element vector with the min and max masses of the RawFileLayer
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public override double[] GetMassRange(int i){
			double[] result = new double[2];
			rawFile.GetMs1MassRange(indsFullScan[i], out double min, out double max);
			result[0] = min;
			result[1] = max;
			return result;
		}

		public override Spectrum GetSpectrum(int j, bool readCentroids){
			if (!Buffered){
				return GetSummedSpectrumImpl(indsFullScan[j], inds[j]);
			}
			if (!map.ContainsKey(j)){
				map.Clear();
				int max = Math.Min(j + Capacity, Count);
				for (int i = j; i < max; i++){
					map.Add(i, GetSummedSpectrumImpl(indsFullScan[i], inds[i]));
				}
			}
			return map[j];
		}

		public override bool HasProfile(int i){
			return rawFile.HasMs1Profile(indsFullScan[i]);
		}

		public override byte GetMassRangeIndex(int i){
			return rawFile.GetMs1MassRangeIndex(indsFullScan[i]);
		}

		public override double[] GetTimeSpan(int i){
			int[] ind = inds[i];
			double[] min = new double[ind.Length + 1];
			double[] max = new double[ind.Length + 1];
			for (int k = 0; k < ind.Length; k++){
				double[] mm = rawFile.GetMs1TimeSpan(ind[k]);
				min[k] = mm[0];
				max[k] = mm[1];
			}
			double[] mm1 = rawFile.GetMs1TimeSpan(indsFullScan[i]);
			min[ind.Length] = mm1[0];
			max[ind.Length] = mm1[1];
			return new[]{ArrayUtils.Min(min), ArrayUtils.Max(max)};
		}

		public override double GetTime(int i){
			int[] ind = inds[i];
			double[] t = new double[ind.Length];
			for (int k = 0; k < ind.Length; k++){
				t[k] = rawFile.GetMs1Time(ind[k]);
			}
			return t.Mean();
		}

		public override int GetIndexFromRt(double time){
			int ind = rawFile.GetMs1IndexFromRt(time);
			return ArrayUtils.ClosestIndex(firstInds, ind);
		}

		public override int Capacity => 200;

		private Spectrum GetSummedSpectrumImpl(int fullScanInd, int[] boxCarInds){
			float[] intensFull = new float[mzGrid.Length];
			Spectrum s1 = rawFile.GetMs1Spectrum(fullScanInd, false);
			LinearInterpolatorMixed li = new LinearInterpolatorMixed(s1.Masses, s1.Intensities);
			for (int j = 0; j < intensFull.Length; j++){
				intensFull[j] = li.Get(mzGrid[j]);
			}
			int nbox = Math.Min(boxCarInds.Length, transmissions.Length);
			double[][] intensBox = new double[nbox][];
			for (int i = 0; i < nbox; i++){
				intensBox[i] = new double[mzGrid.Length];
				Spectrum s = rawFile.GetMs1Spectrum(boxCarInds[i], false);
				LinearInterpolatorMixed li1 = new LinearInterpolatorMixed(s.Masses, s.Intensities);
				for (int j = 0; j < mzGrid.Length; j++){
					List<int> flankp = new List<int>();
					List<int> flankm = new List<int>();
					double trans = transmissions[i][j];
					intensBox[i][j] = trans > 0.1 ? li1.Get(mzGrid[j]) / trans : 0;
					if (j < intensFull.Length - 1){
						if (trans < 0.1 && transmissions[i][j + 1] > 0.1 && li1.Get(mzGrid[j + 1]) > 0){
							flankp.Add(j);
						}
						if (trans > 0.1 && transmissions[i][j + 1] < 0.1 && li1.Get(mzGrid[j]) > 0){
							flankm.Add(j);
						}
					}
					foreach (int i1 in flankp){
						RemoveFlankP(i1, intensBox[i]);
					}
					foreach (int i1 in flankm){
						RemoveFlankM(i1, intensBox[i]);
					}
				}
			}
			for (int i = 0; i < intensFull.Length; i++){
				int count = 0;
				double sum = 0;
					if (intensFull[i] > 0){
						sum += intensFull[i];
						count++;
					}
					foreach (double[] t in intensBox){
						if (t[i] > 0){
							sum += t[i];
							count++;
						}
					}
				if (count > 0){
					intensFull[i] = (float)(sum / count);
				}
			}
			return new Spectrum(mzGrid, intensFull).SuppressZeroes();
		}

		private static void RemoveFlankP(int ind, double[] intens){
			while (intens[ind] > 0){
				intens[ind] = 0;
				ind++;
			}
		}

		private static void RemoveFlankM(int ind, double[] intens){
			while (intens[ind] > 0){
				intens[ind] = 0;
				ind--;
			}
		}

		private void DetermineMzGrid(){
			mzGrid = CalcMzGrid(rawFile);
			if (inds.Length == 0){
				return;
			}
			double[] summedFull = GetSummedScans(mzGrid, out double[][] summedBox);
			transmissions = CalcTransmissions(summedFull, summedBox, mzGrid);
		}

		private static double[][] CalcTransmissions(double[] summedFull, double[][] summedBox, double[] mz){
			double intensThresh = ArrayUtils.Max(summedFull) * 1e-4;
			double[][] result = new double[summedBox.Length][];
			for (int i = 0; i < result.Length; i++){
				result[i] = CalcTransmission(summedFull, summedBox[i], intensThresh, mz);
			}
			return result;
		}

		private static double[] CalcTransmission(double[] summedFull, double[] summedBox, double intensThresh,
			double[] mz){
			double[] transmission = new double[summedBox.Length];
			for (int i = 0; i < transmission.Length; i++){
				transmission[i] = summedFull[i] > intensThresh && summedBox[i] > 0
					? Math.Log10(summedBox[i] / summedFull[i])
					: double.NaN;
			}
			int[] o = ArrayUtils.GetValidInds(transmission);
			double[] mzOut = mz.SubArray(o);
			transmission = transmission.SubArray(o);
			bool[] indicator = new bool[o.Length];
			for (int i = 0; i < indicator.Length; i++){
				indicator[i] = transmission[i] > -1;
			}
			GetRegions(indicator, out List<int> begin, out List<int> end);
			List<int> merge = new List<int>();
			for (int i = 0; i < begin.Count - 1; i++){
				if (begin[i + 1] - end[i] < 20){
					merge.Add(i);
				}
			}
			if (merge.Count > 0){
				Merge(merge, ref begin, ref end);
			}
			Reduce(ref begin, ref end);
			Extend(begin, end, transmission, mzOut);
			double[] result = new double[summedBox.Length];
			for (int i = 0; i < begin.Count; i++){
				double[] m1 = mzOut.SubArray(begin[i], end[i]);
				double[] i1 = transmission.SubArray(begin[i], end[i]);
				Func<double, double> func = Fit(m1, i1);
				int b = ArrayUtils.ClosestIndex(mz, mzOut[begin[i]]);
				int e = ArrayUtils.ClosestIndex(mz, mzOut[end[i] - 1]);
				for (int j = b; j <= e; j++){
					result[j] = Math.Pow(10, func(mz[j]));
				}
			}
			return result;
		}

		private static void Reduce(ref List<int> begin, ref List<int> end){
			int n = begin.Count;
			for (int i = n - 1; i >= 0; i--){
				if (end[i] - begin[i] < 10){
					begin.RemoveAt(i);
					end.RemoveAt(i);
				}
			}
		}

		private static Func<double, double> Fit(double[] m1, double[] i1){
			try{
				return FitFull(m1, i1);
			} catch (Exception){
				try{
					return FitReduced(m1, i1);
				} catch (Exception){
					return FitReduced2(m1, i1);
				}
			}
		}

		private static Func<double, double> FitReduced2(double[] m1, double[] i1){
			double[] a = {ArrayUtils.Median(i1), 0};
			double[] amin = {double.MinValue, double.MinValue};
			double[] amax = {double.MaxValue, double.MaxValue};

			double Func(double x, double[] a1){
				double y = a1[0] + a1[1] * x;
				return y;
			}

			const double epsilon = 5e-5;
			NumUtils.FitNonlin(m1, i1, null, a, amin, amax, out double _, Func, epsilon, 1);
			return x => Func(x, a);
		}

		private static Func<double, double> FitReduced(double[] m1, double[] i1){
			double[] a = {ArrayUtils.Median(i1), 1, m1[m1.Length - 1] + 0.01, 1};
			double[] amin = {double.MinValue, 0, m1[m1.Length - 1] + 0.0001, 0};
			double[] amax = {double.MaxValue, double.MaxValue, m1[m1.Length - 1] + 3, 200};

			double Func(double x, double[] a1){
				double y = a1[0];
				if (a1[2] > x){
					y -= a1[1] / Math.Pow(Math.Abs(a1[2] - x), a1[3]);
				}
				return y;
			}

			const double epsilon = 5e-5;
			NumUtils.FitNonlin(m1, i1, null, a, amin, amax, out double _, Func, epsilon, 1);
			return x => Func(x, a);
		}

		private static Func<double, double> FitFull(double[] m1, double[] i1){
			double[] a = {ArrayUtils.Median(i1), 1, m1[0] - 0.01, 1, 1, m1[m1.Length - 1] + 0.01, 1};
			double[] amin = {double.MinValue, 0, m1[0] - 3, 0, 0, m1[m1.Length - 1] + 0.0001, 0};
			double[] amax = {
				double.MaxValue, double.MaxValue, m1[0] - 0.0001, 200, double.MaxValue, m1[m1.Length - 1] + 3, 200
			};

			double Func(double x, double[] a1){
				double y = a1[0];
				if (x > a1[2]){
					y -= a1[1] / Math.Pow(Math.Abs(x - a1[2]), a1[3]);
				}
				if (a1[5] > x){
					y -= a1[4] / Math.Pow(Math.Abs(a1[5] - x), a1[6]);
				}
				return y;
			}

			const double epsilon = 5e-5;
			NumUtils.FitNonlin(m1, i1, null, a, amin, amax, out double _, Func, epsilon, 1);
			return x => Func(x, a);
		}

		private static void Extend(List<int> begin, List<int> end, double[] result, double[] mzOut){
			for (int i = 0; i < begin.Count; i++){
				int newBegin = begin[i];
				int c;
				while ((c = GetCandidateLower(newBegin, result)) < newBegin){
					if (Math.Abs(mzOut[newBegin] - mzOut[c]) > 0.15){
						break;
					}
					newBegin = c;
				}
				begin[i] = newBegin;
				int newEnd = end[i];
				while ((c = GetCandidateUpper(newEnd, result)) > newEnd){
					if (Math.Abs(mzOut[newEnd - 1] - mzOut[c - 1]) > 0.15){
						break;
					}
					newEnd = c;
				}
				end[i] = newEnd;
			}
		}

		private static int GetCandidateUpper(int newEnd, double[] result){
			if (newEnd == result.Length){
				return newEnd;
			}
			if (result[newEnd] <= result[newEnd - 1]){
				return newEnd + 1;
			}
			int s = Math.Min(newEnd + 11, result.Length);
			double[] r = result.SubArray(newEnd + 1, s);
			int i = ArrayUtils.MinInd(r);
			if (i >= 0 && r[i] <= result[newEnd - 1] - 0.2){
				return newEnd + i + 1;
			}
			return newEnd;
		}

		private static int GetCandidateLower(int newBegin, double[] result){
			if (newBegin == 0){
				return newBegin;
			}
			if (result[newBegin - 1] <= result[newBegin]){
				return newBegin - 1;
			}
			int s = Math.Max(newBegin - 10, 0);
			double[] r = result.SubArray(s, newBegin);
			int i = ArrayUtils.MinInd(r);
			if (i >= 0 && r[i] <= result[newBegin] - 0.2){
				return s + i;
			}
			return newBegin;
		}

		private static void Merge(List<int> merge, ref List<int> begin, ref List<int> end){
			for (int i = merge.Count - 1; i >= 0; i--){
				Merge(merge[i], ref begin, ref end);
			}
		}

		private static void Merge(int merge, ref List<int> begin, ref List<int> end){
			begin.RemoveAt(merge + 1);
			end.RemoveAt(merge);
		}

		private static void GetRegions(bool[] indicator, out List<int> begin, out List<int> end){
			bool inRegion = false;
			int start = -1;
			List<int> begin1 = new List<int>();
			List<int> end1 = new List<int>();
			for (int i = 0; i < indicator.Length; i++){
				if (indicator[i]){
					if (!inRegion){
						start = i;
						inRegion = true;
					}
				} else{
					if (inRegion){
						begin1.Add(start);
						end1.Add(i);
						start = -1;
						inRegion = false;
					}
				}
			}
			if (inRegion){
				begin1.Add(start);
				end1.Add(indicator.Length);
			}
			begin = begin1;
			end = end1;
		}

		private double[] GetSummedScans(double[] mz, out double[][] summedBox){
			double[] summedFull = new double[mz.Length];
			summedBox = new double[inds[0].Length][];
			for (int i = 0; i < summedBox.Length; i++){
				summedBox[i] = new double[mz.Length];
			}
			foreach (int t in indsFullScan){
				Spectrum s = rawFile.GetMs1Spectrum(t, false);
				LinearInterpolatorMixed li = new LinearInterpolatorMixed(s.Masses, s.Intensities);
				for (int j = 0; j < summedFull.Length; j++){
					summedFull[j] += li.Get(mz[j]);
				}
			}
			for (int k = 0; k < summedBox.Length; k++){
				for (int i = 0; i < indsFullScan.Length; i++){
					if (k >= inds[i].Length){
						continue;
					}
					Spectrum s = rawFile.GetMs1Spectrum(inds[i][k], false);
					LinearInterpolatorMixed li = new LinearInterpolatorMixed(s.Masses, s.Intensities);
					for (int j = 0; j < summedFull.Length; j++){
						summedBox[k][j] += li.Get(mz[j]);
					}
				}
			}
			return summedFull;
		}

		private static double[] CalcMzGrid(RawFileLayer rl) {
			Dictionary<int, List<double>> diffs = new Dictionary<int, List<double>>();
			for (int i = 0; i < rl.Ms1ScanNumbers.Length; i++){
				double[] mzs = rl.GetMs1Spectrum(i, false).Masses;
				for (int j = 1; j < mzs.Length; j++){
					double diff = mzs[j] - mzs[j - 1];
					if (diff > 0.01){
						continue;
					}
					int m = (int) mzs[j - 1];
					if (!diffs.ContainsKey(m)){
						diffs.Add(m, new List<double>());
					}
					List<double> f = diffs[m];
					if (f.Count < 500){
						f.Add(diff);
					}
				}
			}
			int[] keys = diffs.Keys.ToArray();
			if (keys.Length == 0){
				return new double[0];
			}
			Array.Sort(keys);
			double[] vals = new double[keys.Length];
			for (int i = 0; i < vals.Length; i++){
				vals[i] = ArrayUtils.Quantile(diffs[keys[i]], 0.15f);
			}
			LinearInterpolator li = new LinearInterpolator(ArrayUtils.ToDoubles(keys), vals);
			li.FlattenEnds();
			List<double> mzVals = new List<double>{keys[0] - 0.1};
			double x;
			while ((x = mzVals[mzVals.Count - 1]) < keys[keys.Length - 1] + 1.01){
				mzVals.Add(x + li.Get(x));
			}
			return mzVals.ToArray();
		}

		public static RawLayer GetBoxCarLayer(RawFileLayer rawFile){
			int[][] scanInds = rawFile.GetConsecutiveSimScans();
			if (scanInds.Length == 1){
				int repreatLen = GetSimScanRepeat(rawFile);
				int[][] scanInds2 = new int[scanInds[0].Length / repreatLen][];
				for (int i = 0; i < scanInds2.Length; i++){
					scanInds2[i] = new int[repreatLen];
					for (int j = 0; j < repreatLen; j++){
						scanInds2[i][j] = scanInds[0][repreatLen * i + j];
					}
				}
				return new RawLayerMs1SummedScans(rawFile, scanInds2) {
					Buffered = true
				};
			}
			return new RawLayerMs1BoxCarScans(rawFile, scanInds){
				Buffered = true
			};
		}
		private static int GetSimScanRepeat(RawFileLayer rf){
			return rf.Ms1MassRangeCount;
		}
	}
}