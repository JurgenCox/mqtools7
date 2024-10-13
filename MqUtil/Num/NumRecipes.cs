namespace MqUtil.Num{
	public class NumRecipes{
		public delegate void RtsaveFunc(double x, out double y, out double dy);
		public static void Mrqcof(double[] x, double[] y, double[] sig, int ndata, double[] a, double[,] alpha,
			double[] beta,
			out double chisq, Func<double, double[], double[], int, double> func){
			int ma = a.Length;
			double[] dyda = new double[ma];
			for (int j = 0; j < ma; j++){
				for (int k = 0; k <= j; k++){
					alpha[j, k] = 0.0;
				}
				beta[j] = 0.0;
			}
			chisq = 0.0;
			for (int i = 0; i < ndata; i++){
				double ymod = func(x[i], a, dyda, ma);
				double sig2I = 1.0 / (sig?[i] * sig[i]) ?? 1.0;
				double dy = y[i] - ymod;
				for (int j = 0, l = 0; l < ma; l++){
					double wt = dyda[l] * sig2I;
					for (int k = 0, m = 0; m <= l; m++){
						alpha[j, k++] += wt * dyda[m];
					}
					beta[j] += dy * wt;
					j++;
				}
				chisq += dy * dy * sig2I;
			}
			for (int j = 1; j < ma; j++){
				for (int k = 0; k < j; k++){
					alpha[k, j] = alpha[j, k];
				}
			}
		}
		public static void MrqcofMulti(double[] x, double[] y, double[] sig, int ndata, double[] a, double[,] alpha,
			double[] beta, out double chisq, Func<double, double[], double[], int, double> func, int nthreads){
			int ma = a.Length;
			for (int j = 0; j < ma; j++){
				for (int k = 0; k <= j; k++){
					alpha[j, k] = 0.0;
				}
				beta[j] = 0.0;
			}
			chisq = 0.0;
			double[][] dyda = new double[ndata][];
			double[] ymod = new double[ndata];
			for (int i = 0; i < ndata; i++){
				dyda[i] = new double[ma];
			}
			if (nthreads == 1){
				for (int i = 0; i < ndata; i++){
					ymod[i] = func(x[i], a, dyda[i], ma);
				}
			} else{
				nthreads = Math.Min(nthreads, ndata);
				int[] inds = new int[nthreads + 1];
				for (int i = 0; i < nthreads + 1; i++){
					inds[i] = (int) Math.Round(i / (double) (nthreads) * (ndata));
				}
				Thread[] t = new Thread[nthreads];
				for (int i = 0; i < nthreads; i++){
					int index0 = i;
					t[i] = new Thread(new ThreadStart(delegate{
						for (int i1 = inds[index0]; i1 < inds[index0 + 1]; i1++){
							ymod[i1] = func(x[i1], a, dyda[i1], ma);
						}
					}));
					t[i].Start();
				}
				for (int i = 0; i < nthreads; i++){
					t[i].Join();
				}
			}
			for (int i = 0; i < ndata; i++){
				double sig2I = 1.0 / (sig?[i] * sig[i]) ?? 1.0;
				double dy = y[i] - ymod[i];
				for (int j = 0, l = 0; l < ma; l++){
					double wt = dyda[i][l] * sig2I;
					for (int k = 0, m = 0; m <= l; m++){
						alpha[j, k++] += wt * dyda[i][m];
					}
					beta[j] += dy * wt;
					j++;
				}
				chisq += dy * dy * sig2I;
			}
			for (int j = 1; j < ma; j++){
				for (int k = 0; k < j; k++){
					alpha[k, j] = alpha[j, k];
				}
			}
		}
		public static void Mrqmin(double[] x, double[] y, double[] sig, int ndata, double[] a, double[] amin,
			double[] amax,
			double[,] covar, double[,] alpha, out double chisq, Func<double, double[], double[], int, double> func,
			ref double alamda, ref double ochisq, ref double[,] oneda, ref int mfit, ref double[] atry,
			ref double[] beta,
			ref double[] da, int nthreads){
			if (amin == null){
				amin = new double[a.Length];
				for (int i = 0; i < amin.Length; i++){
					amin[i] = double.MinValue;
				}
			}
			if (amax == null){
				amax = new double[a.Length];
				for (int i = 0; i < amax.Length; i++){
					amax[i] = double.MaxValue;
				}
			}
			int ma = a.Length;
			if (alamda < 0.0){
				atry = new double[ma];
				beta = new double[ma];
				da = new double[ma];
				mfit = ma;
				oneda = new double[mfit, 1];
				alamda = 0.001;
				if (nthreads > 1){
					MrqcofMulti(x, y, sig, ndata, a, alpha, beta, out chisq, func, nthreads);
				} else{
					Mrqcof(x, y, sig, ndata, a, alpha, beta, out chisq, func);
				}
				ochisq = chisq;
				for (int j = 0; j < ma; j++){
					atry[j] = a[j];
				}
			}
			for (int j = 0; j < ma; j++){
				for (int k = 0; k < ma; k++){
					covar[j, k] = alpha[j, k];
				}
				covar[j, j] = alpha[j, j] * (1.0 + (alamda));
				oneda[j, 0] = beta[j];
			}
			NumUtils.Gaussj(covar, mfit, oneda, 1);
			for (int j = 0; j < mfit; j++){
				da[j] = oneda[j, 0];
			}
			if (alamda == 0.0){
				NumUtils.Covsrt(covar);
				chisq = ochisq;
				return;
			}
			for (int j = 0; j < ma; j++){
				double ax = a[j] + da[j];
				if (ax >= amin[j] && ax <= amax[j]){
					atry[j] = ax;
				}
			}
			if (nthreads > 1){
				MrqcofMulti(x, y, sig, ndata, atry, covar, da, out chisq, func, nthreads);
			} else{
				Mrqcof(x, y, sig, ndata, atry, covar, da, out chisq, func);
			}
			if (chisq < ochisq){
				alamda *= 0.1;
				ochisq = chisq;
				for (int j = 0; j < ma; j++){
					for (int k = 0; k < ma; k++){
						alpha[j, k] = covar[j, k];
					}
					beta[j] = da[j];
					a[j] = atry[j];
				}
			} else{
				alamda *= 10.0;
				chisq = ochisq;
			}
		}
		private const int maxitRtsafe = 100;

		public static double Rtsafe(RtsaveFunc func, double x1, double x2, double xacc) {
			double xh, xl;
			func(x1, out double fl, out double df);
			func(x2, out double fh, out df);
			if ((fl > 0.0 && fh > 0.0) || (fl < 0.0 && fh < 0.0)) {
				throw new Exception("Root must be bracketed in Rtsafe");
			}
			if (fl == 0.0) {
				return x1;
			}
			if (fh == 0.0) {
				return x2;
			}
			if (fl < 0.0) {
				xl = x1;
				xh = x2;
			} else {
				xh = x1;
				xl = x2;
			}
			double rts = 0.5 * (x1 + x2);
			double dxold = Math.Abs(x2 - x1);
			double dx = dxold;
			func(rts, out double f, out df);
			for (int j = 0; j < maxitRtsafe; j++) {
				if ((((rts - xh) * df - f) * ((rts - xl) * df - f) >= 0.0) ||
				    (Math.Abs(2.0 * f) > Math.Abs(dxold * df))) {
					dxold = dx;
					dx = 0.5 * (xh - xl);
					rts = xl + dx;
					if (xl == rts) {
						return rts;
					}
				} else {
					dxold = dx;
					dx = f / df;
					double temp = rts;
					rts -= dx;
					if (temp == rts) {
						return rts;
					}
				}
				if (Math.Abs(dx) < xacc) {
					return rts;
				}
				func(rts, out f, out df);
				if (f < 0.0) {
					xl = rts;
				} else {
					xh = rts;
				}
			}
			throw new Exception("Maximum number of iterations exceeded in Rtsafe");
		}


	}
}