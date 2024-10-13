using MqApi.Num;
using MqApi.Param;
using MqUtil.Num.Test.Univariate.OneSample;
namespace MqUtil.Num.Test.Univariate.TwoSample {
	public class TwoSampleTTest : TwoSampleTest {
		public override void Test(IList<double> data1, IList<double> data2, out double statisticTwoSided,
			out double statisticLess, out double statisticGreater, out double statisticS0, out double bothTails,
			out double leftTail, out double rightTail, out double diff, double s0, out double bothTailsS0, out double leftTailS0,
			out double rightTailS0) {
			TestImpl(data1, data2, out statisticTwoSided, out statisticS0, out bothTails, out leftTail, out rightTail, out diff,
				s0, out bothTailsS0, out leftTailS0, out rightTailS0);
			statisticLess = 0;
			statisticGreater = 0;
		}

		public override TwoSampleTestResult[] Test(IList<double> data1, IList<double> data2, IList<double> s0Values,
			out double diff) {
			return TestImpl2(data1, data2, s0Values, out diff);
		}

		public override string Name => "Student's T-test";
		public override bool HasSides => true;
		public override bool HasS0 => true;

		public static TwoSampleTestResult[] TestImpl2(IList<double> x, IList<double> y, IList<double> s0Values,
			out double diff) {
			int n = x.Count;
			int m = y.Count;
			double xmean = x.Mean();
			double ymean = y.Mean();
			diff = xmean - ymean;
			TwoSampleTestResult[] result = Enumerable.Repeat(new TwoSampleTestResult(0.0, 1.0, 1.0, 1.0), s0Values.Count)
				.ToArray();
			if (n <= 1 || m <= 1) {
				return result;
			}
			double s = 0;
			for (int i = 0; i < n; i++) {
				s += (x[i] - xmean) * (x[i] - xmean);
			}
			for (int i = 0; i < m; i++) {
				s += (y[i] - ymean) * (y[i] - ymean);
			}
			s = Math.Sqrt(s * (1.0 / n + 1.0 / m) / (n + m - 2.0));
			if (s == 0) {
				return result;
			}
			int df = n + m - 2;
			for (int i = 0; i < s0Values.Count; i++) {
				double s0 = s0Values[i];
				double stat = diff / (s + s0);
				double x2P = 0.5 * IncompleteBeta.Value(df / 2.0, 0.5, df / (df + stat * stat));
				double x2M = 1.0 - x2P;
				double bothTails = 2 * Math.Min(x2M, x2P);
				double leftTail = stat > 0 ? x2M : x2P;
				double rightTail = stat > 0 ? x2P : x2M;
				result[i] = new TwoSampleTestResult(stat, leftTail, rightTail, bothTails);
			}
			return result;
		}

		public static void TestImpl(IList<double> x, IList<double> y, out double stat, out double statS0,
			out double bothTails, out double leftTail, out double rightTail, out double diff, double s0, out double bothTailsS0,
			out double leftTailS0, out double rightTailS0) {
			int n = x.Count;
			int m = y.Count;
			stat = 0;
			double xmean = x.Mean();
			double ymean = y.Mean();
			diff = xmean - ymean;
			if (n <= 1 || m <= 1) {
				bothTails = 1.0;
				leftTail = 1.0;
				rightTail = 1.0;
				bothTailsS0 = 1.0;
				leftTailS0 = 1.0;
				rightTailS0 = 1.0;
				statS0 = 0;
				return;
			}
			int i;
			double s = 0;
			for (i = 0; i < n; i++) {
				s += (x[i] - xmean) * (x[i] - xmean);
			}
			for (i = 0; i < m; i++) {
				s += (y[i] - ymean) * (y[i] - ymean);
			}
			s = Math.Sqrt(s * (1.0 / n + 1.0 / m) / (n + m - 2.0));
			if (s == 0) {
				bothTails = 1.0;
				leftTail = 1.0;
				rightTail = 1.0;
				bothTailsS0 = 1.0;
				leftTailS0 = 1.0;
				rightTailS0 = 1.0;
				statS0 = 0;
				return;
			}
			stat = diff / s;
			double df = n + m - 2;
			double xp = 0.5 * IncompleteBeta.Value(df / 2, 0.5, df / (df + stat * stat));
			double xm = 1.0 - xp;
			bothTails = 2 * Math.Min(xm, xp);
			leftTail = stat > 0 ? xm : xp;
			rightTail = stat > 0 ? xp : xm;
			if (s0 <= 0) {
				bothTailsS0 = bothTails;
				leftTailS0 = leftTail;
				rightTailS0 = rightTail;
				statS0 = stat;
			} else {
				statS0 = diff / (s + s0);
				double x2P = 0.5 * IncompleteBeta.Value(df / 2, 0.5, df / (df + statS0 * statS0));
				double x2M = 1.0 - x2P;
				bothTailsS0 = 2 * Math.Min(x2M, x2P);
				leftTailS0 = statS0 > 0 ? x2M : x2P;
				rightTailS0 = statS0 > 0 ? x2P : x2M;
			}
		}

		public override double[][] CalcCurve(double p2, double df, double s0, double maxD, TestSide side) {
			return CalcCurveImpl(p2, df, s0, maxD, side);
		}

		public override Parameters GetParameters() {
			return new Parameters();
		}

		public override OneSampleTest GetOneSampleTest() {
			return new OneSampleTTest();
		}

		public static double[][] CalcCurveImpl(double p2, double df, double s0, double maxD, TestSide side1) {
			if (p2 == 0) {
				return null;
			}
			TestSide side = side1 == TestSide.Both ? side1 : TestSide.Left;
			bool isRightSide = side1 == TestSide.Right;
			double s2 = Stat(p2, df, side);
			List<double> d = new List<double>();
			List<double> p = new List<double>();
			for (double delta = maxD; delta > 0; delta -= 0.01) {
				double s1 = 1.0 / (1.0 / s2 - s0 / delta);
				double p1 = Pval(s1, df, side);
				if (p1 < 1e-50 || s1 <= 0) {
					break;
				}
				d.Add(delta);
				p.Add(p1);
			}
			double[] dd = new double[2 * d.Count];
			double[] pp = new double[2 * d.Count];
			for (int i = 0; i < d.Count; i++) {
				dd[i] = d[i];
				pp[i] = GetVal(d[i], p[i], side);
				dd[2 * d.Count - 1 - i] = -d[i];
				pp[2 * d.Count - 1 - i] = GetVal(-d[i], p[i], side);
			}
			if (isRightSide) {
				ArrayUtils.Revert(pp);
			}
			return new[] {dd, pp};
		}

		private static double GetVal(double d, double p, TestSide side) {
			switch (side) {
				case TestSide.Both: return p;
				case TestSide.Left: return d > 0 ? p : 0;
				case TestSide.Right: return d < 0 ? p : 0;
				default: throw new Exception("Never get here.");
			}
		}

		private const double eps = 1e-3;

		private static double Stat(double pval, double df, TestSide side) {
			if (side == TestSide.Right) {
				throw new ArgumentException("side has to be one of {Both, Left}");
			}
			const double min = 0;
			double max = 1;
			double logp = Math.Log10(pval);
			double logpmax = Math.Log10(Pval(max, df, side));
			double logpmin = Math.Log10(Pval(min, df, side));
			while ((logpmax - logp) * (logpmin - logp) > 0) {
				max += 0.1;
				logpmax = Math.Log10(Pval(max, df, side));
			}

			void Func(double x, out double y, out double dy){
				y = Math.Log10(Pval(x, df, side)) - logp;
				dy = (Math.Log10(Pval(x + 0.5 * eps, df, side)) - Math.Log10(Pval(x - 0.5 * eps, df, side))) / eps;
			}

			try {
				return NumRecipes.Rtsafe(Func, min, max, eps);
			} catch (Exception) {
				return double.NaN;
			}
		}

		private static double Pval(double stat, double df, TestSide side) {
			double xp;
			double xm;
			try {
				//df >=0 
				// stat != 0
				xp = 0.5 * IncompleteBeta.Value(df / 2, 0.5, df / (df + stat * stat));
				xm = 1 - xp;
			} catch (Exception) {
				return double.NaN;
			}
			switch (side) {
				case TestSide.Both: return 2 * Math.Min(xm, xp);
				case TestSide.Right: return stat > 0 ? xm : xp;
				case TestSide.Left: return stat > 0 ? xp : xm;
				default: throw new Exception("Never get here.");
			}
		}
	}
}