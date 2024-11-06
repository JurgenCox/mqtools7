using MqApi.Num;
using MqUtil.Ms.Utils;

namespace MqUtil.Ms.Raw {
	public class RawLayerMs1SummedScans : RawLayer {
		private readonly RawFileLayer rawFile;
		private readonly int[][] inds;
        private readonly int[] firstInds;
        private readonly double[] mzGrid;

		public RawLayerMs1SummedScans(RawFileLayer rawFile, int[][] inds) {
			this.rawFile = rawFile;
			this.inds = inds;
			firstInds = new int[inds.Length];
			for (int i = 0; i < firstInds.Length; i++) {
				firstInds[i] = inds[i][0];
			}
            mzGrid = CalcMzGrid(rawFile);
		}

        internal static double[] CalcMzGrid(RawFileLayer rl) {
            Dictionary<int, List<double>> mins = new Dictionary<int, List<double>>();
            double mzMin = double.MaxValue;
            double mzMax = double.MinValue;
            for (int i = 0; i < rl.Ms1Count; i++) {
                Spectrum s = rl.GetMs1Spectrum(i, false);
                double[] m = s.Masses;
                float[] intens = s.Intensities;
                if (m.Length > 0) {
                    mzMin = Math.Min(mzMin, m[0]);
                    mzMax = Math.Max(mzMax, m[m.Length-1]);
				}
				for (int j = 0; j < m.Length - 1; j++) {
                    double dm = m[j + 1] - m[j];
                    if (intens[j + 1] <= 0 || intens[j] <= 0) {
                        continue;
                    }
                    int im = (int)Math.Floor(m[j]);
                    if (!mins.ContainsKey(im)) {
						mins.Add(im, new List<double>());
                    }
                    mins[im].Add(dm);
                }
            }
            (double[] mx, double[] dmx) = FlattenMap(mins);
            List<double> mzGrid = new List<double> {mzMin};
            while (mzGrid.Last() < mzMax) {
                int ind = ArrayUtils.ClosestIndex(mx, mzGrid.Last());
				mzGrid.Add(mzGrid.Last() + dmx[ind]);
            }
            return mzGrid.ToArray();
        }

        private static (double[], double[]) FlattenMap(Dictionary<int, List<double>> mins) {
            int[] keys = mins.Keys.ToArray();
			Array.Sort(keys);
            double[] m = new double[keys.Length];
            double[] dm = new double[keys.Length];
            for (int i = 0; i < keys.Length; i++) {
                m[i] = keys[i] + 0.5;
                dm[i] = ArrayUtils.Median(mins[keys[i]]);
            }
			return (m, dm);
        }

		public override int Count => inds.Length;
		public override int MassRangeCount => rawFile.Ms1MassRangeCount;

		/// <summary>
		/// Return a two-element vector with the min and max masses of the RawFileLayer
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public override double[] GetMassRange(int i) {
			double[] result = new double[2];
			int[] ind = inds[i];
			double[] min = new double[ind.Length];
			double[] max = new double[ind.Length];
			for (int k = 0; k < ind.Length; k++) {
				rawFile.GetMs1MassRange(ind[k], out min[k], out max[k]);
			}
			result[0] = ArrayUtils.Min(min);
			result[1] = ArrayUtils.Max(max);
			return result;
		}

		public override Spectrum GetSpectrum(int j, bool readCentroids) {
			if (!Buffered) {
				int[] ind = inds[j];
				return GetSummedSpectrumImpl(ind, readCentroids);
			}
			if (!map.ContainsKey(j)) {
				map.Clear();
				int max = Math.Min(j + Capacity, Count);
				for (int i = j; i < max; i++) {
					int[] ind = inds[i];
					map.Add(i, GetSummedSpectrumImpl(ind, readCentroids));
				}
			}
			return map[j];
		}

		public override bool HasProfile(int i) {
			int ind = inds[i][0];
			return rawFile.HasMs1Profile(ind);
		}

		public override byte GetMassRangeIndex(int i) {
			int ind = inds[i][0];
			return rawFile.GetMs1MassRangeIndex(ind);
		}

		public override double[] GetTimeSpan(int i) {
			int[] ind = inds[i];
			double[] min = new double[ind.Length];
			double[] max = new double[ind.Length];
			for (int k = 0; k < ind.Length; k++) {
				double[] mm = rawFile.GetMs1TimeSpan(ind[k]);
				min[k] = mm[0];
				max[k] = mm[1];
			}
			return new[] {ArrayUtils.Min(min), ArrayUtils.Max(max)};
		}

		public override double GetTime(int i) {
			int[] ind = inds[i];
			double[] t = new double[ind.Length];
			for (int k = 0; k < ind.Length; k++) {
				t[k] = rawFile.GetMs1Time(ind[k]);
			}
			return t.Mean();
		}

		public override int GetIndexFromRt(double time) {
			int ind = rawFile.GetMs1IndexFromRt(time);
			return ArrayUtils.ClosestIndex(firstInds, ind);
		}

		public override int Capacity => 200;

		private Spectrum GetSummedSpectrumImpl(int[] ind, bool readCentroids) {
            double[][] masses = new double[ind.Length][];
            float[][] intensities = new float[ind.Length][];
            for (int i = 0; i < ind.Length; i++) {
                Spectrum s = rawFile.GetMs1Spectrum(ind[i], readCentroids);
                masses[i] = s.Masses;
                intensities[i] = s.Intensities;
            }
            (double[] mergedMasses, float[] mergedIntensities) = CalcMergedSpectrum(masses, intensities);
			return new Spectrum(mergedMasses, mergedIntensities);
		}

        private (double[], float[]) CalcMergedSpectrum(double[][] masses, float[][] intensities) {
            Dictionary<int, float> result = new Dictionary<int, float>();
            for (int i = 0; i < masses.Length; i++) {
                AddSpectrum(masses[i], intensities[i], result);
            }
            int[] keys = result.Keys.ToArray();
			Array.Sort(keys);
            List<double> m1 = new List<double>();
            List<float> i1 = new List<float>();
			m1.Add(mzGrid[keys[0]]);
			i1.Add(result[keys[0]]);
            for (int i = 1; i < keys.Length; i++) {
                int keym = keys[i-1];
                int key = keys[i];
                int dkey = key - keym;
                if (dkey == 1) {
                    m1.Add(mzGrid[key]);
                    i1.Add(result[key]);
                } else if (dkey == 2) {
                    m1.Add(mzGrid[key-1]);
                    i1.Add(0);
                    m1.Add(mzGrid[key]);
                    i1.Add(result[key]);
				}else {
                    m1.Add(mzGrid[keym + 1]);
                    i1.Add(0);
					m1.Add(mzGrid[key - 1]);
                    i1.Add(0);
                    m1.Add(mzGrid[key]);
                    i1.Add(result[key]);
				}
			}
            return (m1.ToArray(), i1.ToArray());
        }

        private void AddSpectrum(double[] masses, float[] intensities, Dictionary<int, float> result) {
            for (int i = 0; i < masses.Length; i++) {
                if (intensities[i] > 0) {
                    AddPeak(masses[i], intensities[i], result);
                }
			}
        }

        private void AddPeak(double mass, float intensity, Dictionary<int, float> result) {
            int flInd = ArrayUtils.FloorIndex(mzGrid, mass);
            if (flInd < 0) {
				AddIntens(result, 0, intensity);
            }else if (flInd >= mzGrid.Length - 1) {
                AddIntens(result, mzGrid.Length - 1, intensity);
            }else {
                int ceilInd = flInd + 1;
                double m1 = mzGrid[flInd];
				double m2 = mzGrid[ceilInd];
                double dm = m2 - m1;
                double w1 = (m2 - mass) / dm;
				double w2 = (mass - m1) / dm;
                AddIntens(result, flInd, (float)w1 * intensity);
				AddIntens(result, ceilInd, (float)w2 * intensity);
            }
		}

        private void AddIntens(Dictionary<int, float> result, int ind, float intensity) {
            if (result.ContainsKey(ind)) {
                result[ind] += intensity;
            }else {
				result.Add(ind, intensity);
            }
        }
    }
}