namespace MqApi.Num{
	public static class NumUtil{
		/// <summary>
		/// Returns the error function erf(x)
		/// </summary>
		public static double Erff(double x){
			return x < 0.0 ? -Gammp(0.5, x * x) : Gammp(0.5, x * x);
		}
		/// <summary>
		/// Returns the incomplete gamma function P(a,x)
		/// </summary>
		public static double Gammp(double a, double x){
			double gamser = double.NaN;
			double gammcf = double.NaN;
			if (x < 0.0 || a <= 0.0){
				throw new Exception("Invalid arguments in routine gammq");
			}
			if (x < a + 1.0){
				Gser(ref gamser, a, x, out _);
				return gamser;
			}
			Gcf(ref gammcf, a, x, out _);
			return 1.0 - gammcf;
		}
		private const double gserEps = 3.0e-7;
		private const double gserItmax = 100;
		public static void Gser(ref double gamser, double a, double x, out double gln){
			gln = Gamma.LnValue(a);
			if (x <= 0.0){
				if (x < 0.0){
					throw new Exception("x less than 0 in routine gser");
				}
				gamser = 0.0;
				return;
			}
			double ap = a;
			double sum = 1.0 / a;
			double del = sum;
			for (int n = 1; n <= gserItmax; n++){
				++ap;
				del *= x / ap;
				sum += del;
				if (Math.Abs(del) < Math.Abs(sum) * gserEps){
					gamser = sum * Math.Exp(-x + a * Math.Log(x) - (gln));
					return;
				}
			}
			throw new Exception("a too large, ITMAX too small in routine gser");
		}
		private const double gcfEps = 3.0e-7;
		private const double gcfFpmin = 1.0e-30;
		private const double gcfItmax = 100;
		public static void Gcf(ref double gammcf, double a, double x, out double gln){
			int i;
			gln = Gamma.LnValue(a);
			double b = x + 1.0 - a;
			double c = 1.0 / gcfFpmin;
			double d = 1.0 / b;
			double h = d;
			for (i = 1; i <= gcfItmax; i++){
				double an = -i * (i - a);
				b += 2.0;
				d = an * d + b;
				if (Math.Abs(d) < gcfFpmin){
					d = gcfFpmin;
				}
				c = b + an / c;
				if (Math.Abs(c) < gcfFpmin){
					c = gcfFpmin;
				}
				d = 1.0 / d;
				double del = d * c;
				h *= del;
				if (Math.Abs(del - 1.0) < gcfEps){
					break;
				}
			}
			if (i > gcfItmax){
				throw new Exception("a too large, ITMAX too small in gcf");
			}
			gammcf = Math.Exp(-x + a * Math.Log(x) - gln) * h;
		}
		public static float Clamp(float x, float min, float max){
			return Math.Min(Math.Max(x, min), max);
		}
		public static int Clamp(int value, int min, int max){
			if (value > max){
				return max;
			}
			return value < min ? min : value;
		}
	}
}