using MqApi.Num;
using MqUtil.Num.Test.Univariate.TwoSample;

namespace MqUtil.Data{
	public class PeakCluster{
		public readonly Dictionary<int, List<WritablePeak>> data = new Dictionary<int, List<WritablePeak>>();
		public PeakCluster(int rtInd, WritablePeak peak){
			Add(rtInd, peak);
		}
		private PeakCluster(Dictionary<int, List<WritablePeak>> data){
			this.data = data;
		}
		public void Add(int rtInd, WritablePeak peak){
			if (!data.ContainsKey(rtInd)){
				data.Add(rtInd, new List<WritablePeak>());
			}
			data[rtInd].Add(peak);
		}
		private float[] GetOrigIntensityProfile(){
			int[] s = data.Keys.ToArray();
			Array.Sort(s);
			float[] result = new float[s.Length];
			for (int i = 0; i < result.Length; i++){
				List<WritablePeak> x = data[s[i]];
				foreach (WritablePeak peak in x){
					result[i] += peak.p.OrigIntensityProfile.Sum();
				}
			}
			return result;
		}
		private static readonly TwoSampleTTest test = new TwoSampleTTest();
		public PeakCluster SubPeak(int start, int end){
			int[] s = data.Keys.ToArray();
			Array.Sort(s);
			Dictionary<int, List<WritablePeak>> newData = new Dictionary<int, List<WritablePeak>>();
			foreach (int key in data.Keys){
				int ind = Array.BinarySearch(s, key);
				if (ind >= start && ind < end){
					newData.Add(key, data[key]);
				}
			}
			return new PeakCluster(newData);
		}
		public void Dispose(){
			data.Clear();
		}
		public PeakCluster[] DecomposeValleys(double valleyFactor, bool advancedPeakSplitting){
			List<PeakCluster> splitCandidates = new List<PeakCluster>();
			List<PeakCluster> result = new List<PeakCluster>();
			splitCandidates.Add(this);
			while (splitCandidates.Count > 0){
				PeakCluster maybeSplitMe = splitCandidates[0];
				splitCandidates.Remove(maybeSplitMe);
				PeakCluster[] w = maybeSplitMe.SplitAtValley(valleyFactor, advancedPeakSplitting);
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
		private PeakCluster[] SplitAtValley(double valleyFactor, bool advancedPeakSplitting){
			float[] intensities = GetOrigIntensityProfile();
			const int minLen = 4;
			const int maxLen = 7;
			const double pval = 0.01;
			IEnumerable<int> minPos = Peak.CalcLocalMinPositions(intensities);
			double[] mzCentroid = GetMzCentroid();
			foreach (int pos in minPos){
				double leftMax = Peak.GetLeftMax(pos, intensities);
				double rightMax = Peak.GetRightMax(pos, intensities);
				double smallMax = Math.Min(leftMax, rightMax);
				if (smallMax / intensities[pos] > valleyFactor){
					return new[]{
						SubPeak(0, pos), SubPeak(pos + 1, data.Count)
					};
				}
				if (advancedPeakSplitting){
					if (pos >= minLen && pos < intensities.Length - minLen){
						int start = Math.Max(0, pos - maxLen);
						double[] x = mzCentroid.SubArray(start, pos);
						int end = Math.Min(mzCentroid.Length, pos + maxLen + 1);
						double[] y = mzCentroid.SubArray(pos + 1, end);
						double p = test.GetPvalue(x, y);
						if (p <= pval){
							return new[]{
								SubPeak(0, pos), SubPeak(pos + 1, data.Count)
							};
						}
					}
				}
			}
			return null;
		}
		private double[] GetMzCentroid(){
			int[] s = data.Keys.ToArray();
			Array.Sort(s);
			double[] result = new double[s.Length];
			for (int i = 0; i < result.Length; i++){
				List<WritablePeak> x = data[s[i]];
				WritablePeak peak = x[0];
				result[i] = peak.p.MzCentroidAvg;
			}
			return result;
		}
	}
}