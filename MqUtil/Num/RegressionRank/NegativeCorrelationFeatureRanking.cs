using MqApi.Num;

namespace MqUtil.Num.RegressionRank{
	public class NegativeCorrelationFeatureRanking : CorrelationFeatureRanking{
		public override double CalcScore(double[] xx, double[] yy){
			return 1 + ArrayUtils.Correlation(xx, yy);
		}
		public override string Name => "-Pearson correlation";
		public override float DisplayRank => 2;
	}
}