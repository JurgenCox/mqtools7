using MqApi.Num;
using MqApi.Num.Vector;
using MqApi.Param;
using MqUtil.Api;

namespace MqUtil.Num.RegressionRank {
	public abstract class CorrelationFeatureRanking : RegressionFeatureRankingMethod {
		public override int[] Rank(BaseVector[] x, double[] y, Parameters param, IGroupDataProvider data, int nthreads) {
			int nfeatures = x[0].Length;
			double[] s = new double[nfeatures];
			for (int i = 0; i < nfeatures; i++) {
				double[] xx = new double[x.Length];
				for (int j = 0; j < xx.Length; j++) {
					xx[j] = x[j][i];
				}
				s[i] = CalcScore(xx, y);
			}
			return s.Order();
		}

		public abstract double CalcScore(double[] xx, double[] yy);

		public override Parameters GetParameters(IGroupDataProvider data) {
			return new Parameters();
		}

		public override string Description => "";
		public override bool IsActive => true;
	}
}