using MqApi.Num;
namespace MqUtil.Num.Test.Univariate.NSample{
	public class OneWayAnovaTest : MultipleSampleTest{
		public override double Test(double[][] data, out double statistic, double s0, out double pvalS0,
			out double[] gmeans){
			return TestImpl(data, out statistic, s0, out pvalS0, out gmeans);
		}
		public override bool HasSides => false;
		public override bool HasS0 => true;
		public static double TestImpl(double[][] data, out double statistic, double s0, out double pvalS0,
			out double[] gmeansOut){
			List<int> v = new List<int>();
			for (int i = 0; i < data.Length; i++){
				if (data[i].Length > 1){
					v.Add(i);
				}
			}
			bool reduced = v.Count != data.Length;
			int orig = data.Length;
			data = data.SubArray(v);
			int g = data.Length;
			if (g < 2){
				statistic = 0;
				pvalS0 = 1;
				gmeansOut = null;
				return 1;
			}
			double[] gmeans = new double[g];
			double totMean = 0;
			int n = 0;
			for (int i = 0; i < g; i++){
				gmeans[i] = data[i].Mean();
				totMean += gmeans[i] * data[i].Length;
				n += data[i].Length;
			}
			totMean /= n;
			double ssw = 0;
			double ssb = 0;
			for (int i = 0; i < g; i++){
				ssb += data[i].Length * (gmeans[i] - totMean) * (gmeans[i] - totMean);
				for (int j = 0; j < data[i].Length; j++){
					ssw += (data[i][j] - gmeans[i]) * (data[i][j] - gmeans[i]);
				}
			}
			if (ssw <= 0.0){
				statistic = 0;
				pvalS0 = 1;
				if (reduced){
					gmeansOut = new double[orig];
					for (int i = 0; i < v.Count; i++){
						gmeansOut[v[i]] = gmeans[i];
					}
				} else{
					gmeansOut = gmeans;
				}
				return 1;
			}
			double dw = n - g;
			double db = g - 1;
			statistic = ssb * dw / db / ssw;
			double gms = GetGeomMeanSquared(data);
			double denom = n * db * ssw / dw / gms;
			double statisticS0 = n * ssb / gms / (Math.Sqrt(denom) + s0) / (Math.Sqrt(denom) + s0);
			pvalS0 = IncompleteBeta.Value(dw * 0.5, db * 0.5, dw / (dw + db * statisticS0));
			if (reduced){
				gmeansOut = new double[orig];
				for (int i = 0; i < v.Count; i++){
					gmeansOut[v[i]] = gmeans[i];
				}
			} else{
				gmeansOut = gmeans;
			}
			return IncompleteBeta.Value(dw * 0.5, db * 0.5, dw / (dw + db * statistic));
		}
		public override string Name => "ANOVA";
		private static double GetGeomMeanSquared(ICollection<double[]> x){
			double result = 1;
			foreach (double[] t in x){
				result *= Math.Pow(t.Length, 2.0 / x.Count);
			}
			return result;
		}
	}
}