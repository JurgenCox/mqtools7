using MqApi.Param;
using MqUtil.Num.Test.Univariate.OneSample;
namespace MqUtil.Num.Test.Univariate.TwoSample{
	public abstract class TwoSampleTest : UnivariateTest{
		//TODO
		// statisticTwoSided, statisticGreater and statisticLess are different only for KolmogorowSmirnowTest, for all other 
		//tests the statisticGreater and statisticLess equal to zero
		public abstract void Test(IList<double> data1, IList<double> data2, out double statisticTwoSided,
			out double statisticLess, out double statisticGreater, out double statisticS0, out double bothTails,
			out double leftTail, out double rightTail, out double difference, double s0, out double bothTailsS0,
			out double leftTailS0, out double rightTailS0);
		public struct TwoSampleTestResult{
			public double stat, leftTail, rightTail, bothTails;
			public TwoSampleTestResult(double stat1, double leftTail1, double rightTail1, double bothTails1){
				stat = stat1;
				leftTail = leftTail1;
				rightTail = rightTail1;
				bothTails = bothTails1;
			}
		}
		public virtual TwoSampleTestResult[] Test(IList<double> data1, IList<double> data2, IList<double> s0Values,
			out double diff){
			throw new NotImplementedException();
		}
		public abstract double[][] CalcCurve(double p2, double df, double s0, double maxD, TestSide side);
		public abstract Parameters GetParameters();
		public void Test(IList<double> data1, IList<double> data2, out double statisticTwoSided,
			out double statisticLess, out double statisticGreater, out double bothTails, out double leftTail,
			out double rightTail, out double difference){
			double dummy;
			Test(data1, data2, out statisticTwoSided, out statisticLess, out statisticGreater, out _, out bothTails,
				out leftTail, out rightTail, out difference, 0.0, out dummy, out dummy, out dummy);
		}
		public double GetPvalue(IList<double> data1, IList<double> data2){
			Test(data1, data2, out _, out _, out _, out var bothTails, out _, out _, out _);
			return bothTails;
		}
		public abstract OneSampleTest GetOneSampleTest();
	}
}