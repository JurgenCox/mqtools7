namespace MqUtil.Num{
	public class NumRecipes{
		public delegate void RtsaveFunc(double x, out double y, out double dy);
		/// <summary>
		/// Given a matrix a(m x n) this routine computes its singular value decomposition
		/// A = U W V^T.
		/// </summary>
		/// <param name="a">Contains on input the matrix to be decomposed.the 
		/// The matrix U replaces a on output.</param>
		/// <param name="w">The diagonal matrix of singular values.</param>
		/// <param name="v">The matrix v (not its transpose).</param>
		public static void Svdcmp(double[,] a, out double[] w, out double[,] v)
		{
			int m = a.GetLength(0);
			int n = a.GetLength(1);
			w = new double[n];
			v = new double[n, n];
			int i;
			int j;
			int k, l = 0, nm = 0;
			double anorm;
			double f, h, s, scale;
			double[] rv1 = new double[n];
			double g = scale = anorm = 0.0;
			for (i = 0; i < n; i++)
			{
				l = i + 1;
				rv1[i] = scale * g;
				g = s = scale = 0.0;
				if (i < m)
				{
					for (k = i; k < m; k++)
					{
						scale += Math.Abs(a[k, i]);
					}
					if (scale != 0)
					{
						for (k = i; k < m; k++)
						{
							a[k, i] /= scale;
							s += a[k, i] * a[k, i];
						}
						f = a[i, i];
						g = -(f >= 0.0 ? Math.Abs(Math.Sqrt(s)) : -Math.Abs(Math.Sqrt(s)));
						h = f * g - s;
						a[i, i] = f - g;
						for (j = l; j < n; j++)
						{
							for (s = 0.0, k = i; k < m; k++)
							{
								s += a[k, i] * a[k, j];
							}
							f = s / h;
							for (k = i; k < m; k++)
							{
								a[k, j] += f * a[k, i];
							}
						}
						for (k = i; k < m; k++)
						{
							a[k, i] *= scale;
						}
					}
				}
				w[i] = scale * g;
				g = s = scale = 0.0;
				if (i < m && i != n - 1)
				{
					for (k = l; k < n; k++)
					{
						scale += Math.Abs(a[i, k]);
					}
					if (scale != 0)
					{
						for (k = l; k < n; k++)
						{
							a[i, k] /= scale;
							s += a[i, k] * a[i, k];
						}
						f = a[i, l];
						g = -(f >= 0.0 ? Math.Abs(Math.Sqrt(s)) : -Math.Abs(Math.Sqrt(s)));
						h = f * g - s;
						a[i, l] = f - g;
						for (k = l; k < n; k++)
						{
							rv1[k] = a[i, k] / h;
						}
						for (j = l; j < m; j++)
						{
							for (s = 0.0, k = l; k < n; k++)
							{
								s += a[j, k] * a[i, k];
							}
							for (k = l; k < n; k++)
							{
								a[j, k] += s * rv1[k];
							}
						}
						for (k = l; k < n; k++)
						{
							a[i, k] *= scale;
						}
					}
				}
				anorm = Math.Max(anorm, (Math.Abs(w[i]) + Math.Abs(rv1[i])));
			}
			for (i = n - 1; i >= 0; i--)
			{
				if (i < n - 1)
				{
					if (g != 0)
					{
						for (j = l; j < n; j++)
						{
							v[j, i] = (a[i, j] / a[i, l]) / g;
						}
						for (j = l; j < n; j++)
						{
							for (s = 0.0, k = l; k < n; k++)
							{
								s += a[i, k] * v[k, j];
							}
							for (k = l; k < n; k++)
							{
								v[k, j] += s * v[k, i];
							}
						}
					}
					for (j = l; j < n; j++)
					{
						v[i, j] = v[j, i] = 0.0;
					}
				}
				v[i, i] = 1.0;
				g = rv1[i];
				l = i;
			}
			for (i = Math.Min(m, n) - 1; i >= 0; i--)
			{
				l = i + 1;
				g = w[i];
				for (j = l; j < n; j++)
				{
					a[i, j] = 0.0;
				}
				if (g != 0)
				{
					g = 1.0 / g;
					for (j = l; j < n; j++)
					{
						for (s = 0.0, k = l; k < m; k++)
						{
							s += a[k, i] * a[k, j];
						}
						f = (s / a[i, i]) * g;
						for (k = i; k < m; k++)
						{
							a[k, j] += f * a[k, i];
						}
					}
					for (j = i; j < m; j++)
					{
						a[j, i] *= g;
					}
				}
				else
				{
					for (j = i; j < m; j++)
					{
						a[j, i] = 0.0;
					}
				}
				++a[i, i];
			}
			for (k = n - 1; k >= 0; k--)
			{
				int its;
				for (its = 1; its <= 30; its++)
				{
					int flag = 1;
					for (l = k; l >= 0; l--)
					{
						nm = l - 1;
						if ((Math.Abs(rv1[l]) + anorm) == anorm)
						{
							flag = 0;
							break;
						}
						if ((Math.Abs(w[nm]) + anorm) == anorm)
						{
							break;
						}
					}
					double c;
					double z;
					double y;
					if (flag != 0)
					{
						c = 0.0;
						s = 1.0;
						for (i = l; i <= k; i++)
						{
							f = s * rv1[i];
							rv1[i] = c * rv1[i];
							if ((Math.Abs(f) + anorm) == anorm)
							{
								break;
							}
							g = w[i];
							h = NumUtils.Pythag(f, g);
							w[i] = h;
							h = 1.0 / h;
							c = g * h;
							s = -f * h;
							for (j = 0; j < m; j++)
							{
								y = a[j, nm];
								z = a[j, i];
								a[j, nm] = y * c + z * s;
								a[j, i] = z * c - y * s;
							}
						}
					}
					z = w[k];
					if (l == k)
					{
						if (z < 0.0)
						{
							w[k] = -z;
							for (j = 0; j < n; j++)
							{
								v[j, k] = -v[j, k];
							}
						}
						break;
					}
					if (its == 30)
					{
						throw new Exception("no convergence in 30 svdcmp iterations");
					}
					double x = w[l];
					nm = k - 1;
					y = w[nm];
					g = rv1[nm];
					h = rv1[k];
					f = ((y - z) * (y + z) + (g - h) * (g + h)) / (2.0 * h * y);
					g = NumUtils.Pythag(f, 1.0);
					f = ((x - z) * (x + z) + h * ((y / (f + (f >= 0.0 ? Math.Abs(g) : -Math.Abs(g)))) - h)) / x;
					c = s = 1.0;
					for (j = l; j <= nm; j++)
					{
						i = j + 1;
						g = rv1[i];
						y = w[i];
						h = s * g;
						g = c * g;
						z = NumUtils.Pythag(f, h);
						rv1[j] = z;
						c = f / z;
						s = h / z;
						f = x * c + g * s;
						g = g * c - x * s;
						h = y * s;
						y *= c;
						int jj;
						for (jj = 0; jj < n; jj++)
						{
							x = v[jj, j];
							z = v[jj, i];
							v[jj, j] = x * c + z * s;
							v[jj, i] = z * c - x * s;
						}
						z = NumUtils.Pythag(f, h);
						w[j] = z;
						if (z != 0)
						{
							z = 1.0 / z;
							c = f * z;
							s = h * z;
						}
						f = c * g + s * y;
						x = c * y - s * g;
						for (jj = 0; jj < m; jj++)
						{
							y = a[jj, j];
							z = a[jj, i];
							a[jj, j] = y * c + z * s;
							a[jj, i] = z * c - y * s;
						}
					}
					rv1[l] = 0.0;
					rv1[k] = f;
					w[k] = x;
				}
			}
		}
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