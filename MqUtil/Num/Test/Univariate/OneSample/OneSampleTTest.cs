using MqApi.Num;
using MqUtil.Num.Test.Univariate.TwoSample;
namespace MqUtil.Num.Test.Univariate.OneSample{
	public class OneSampleTTest : OneSampleTest{
		public override void Test(double[] data, double mean, out double statistic, out double statisticS0,
			out double bothTails, out double leftTail, out double rightTail, out double difference, double s0,
			out double bothTailsS0, out double leftTailS0, out double rightTailS0){
			TestImpl(data, mean, out statistic, out statisticS0, out bothTails, out leftTail, out rightTail,
				out difference, s0, out bothTailsS0, out leftTailS0, out rightTailS0);
		}

		public override string Name => "T-test";
		public override bool HasSides => true;
		public override bool HasS0 => true;

		public static void TestImpl(IList<double> x, double mean, out double stat, out double statS0,
			out double bothtails, out double lefttail, out double righttail, out double diff, double s0,
			out double bothTailsS0, out double leftTailS0, out double rightTailS0){
			int n = x.Count;
			stat = 0;
			statS0 = 0;
			double xmean = x.Mean();
			diff = xmean - mean;
			if (n <= 1){
				bothtails = 1.0;
				lefttail = 1.0;
				righttail = 1.0;
				bothTailsS0 = 1.0;
				leftTailS0 = 1.0;
				rightTailS0 = 1.0;
				return;
			}
			int i;
			double v1 = 0;
			for (i = 0; i < n; i++){
				v1 = v1 + (x[i] - xmean) * (x[i] - xmean);
			}
			double xvariance = v1 / (n - 1);
			if (xvariance < 0){
				xvariance = 0;
			}
			double xstddev = Math.Sqrt(xvariance);
			if (xstddev == 0){
				bothtails = 1.0;
				lefttail = 1.0;
				righttail = 1.0;
				bothTailsS0 = 1.0;
				leftTailS0 = 1.0;
				rightTailS0 = 1.0;
				return;
			}
			stat = (xmean - mean) / (xstddev / Math.Sqrt(n));
			double df = n - 1;
			try{
				double xp = 0.5 * IncompleteBeta.Value(df / 2, 0.5, df / (df + stat * stat));
				double xm = 1 - xp;
				bothtails = 2 * Math.Min(xm, xp);
				lefttail = stat > 0 ? xm : xp;
				righttail = stat > 0 ? xp : xm;
				if (s0 <= 0){
					bothTailsS0 = bothtails;
					leftTailS0 = lefttail;
					rightTailS0 = righttail;
					statS0 = stat;
				} else{
					statS0 = (xmean - mean) / (s0 + xstddev / Math.Sqrt(n));
					double xp1 = 0.5 * IncompleteBeta.Value(df / 2, 0.5, df / (df + statS0 * statS0));
					double xm1 = 1 - xp1;
					bothTailsS0 = 2 * Math.Min(xm1, xp1);
					leftTailS0 = statS0 > 0 ? xm1 : xp1;
					rightTailS0 = statS0 > 0 ? xp1 : xm1;
				}
			} catch (Exception){
				bothtails = 1.0;
				lefttail = 1.0;
				righttail = 1.0;
				bothTailsS0 = 1.0;
				leftTailS0 = 1.0;
				rightTailS0 = 1.0;
			}
		}

		public override double[][] CalcCurve(double p2, double df, double s0, double maxD, TestSide side){
			return TwoSampleTTest.CalcCurveImpl(p2, df, s0, maxD, side);
		}
	}
}