using MqUtil.Num;
namespace MqUtil.Data{
	public class AutocorrFitfunc{
		private readonly LinearInterpolator li;

		public AutocorrFitfunc(BinaryReader reader){
			li = new LinearInterpolator(reader);
		}

		public void Write(BinaryWriter writer){
			li.Write(writer);
		}

		public AutocorrFitfunc(double[] x, IList<double> z){
			int n = x.Length;
			double[] y = new double[n];
			y[n - 1] = 1.0 + z[n - 1]*z[n - 1];
			for (int i = n - 2; i >= 0; i--){
				y[i] = y[i + 1] + z[i]*z[i];
			}
			li = new LinearInterpolator(x, y);
			li.FlattenEnds();
		}

		public AutocorrFitfunc(double constant){
			li = new LinearInterpolator(new double[]{0, 1}, new[]{constant, constant});
		}

		public double Interpolate(double d){
			return li.Get(d);
		}
	}
}