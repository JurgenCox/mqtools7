using MqApi.Num;
using MqUtil.Num;

namespace MqUtil.Data{
	public static class RecalibrationFit{
		public delegate double Funcs3(int i, double[] a, out double[] dyda);

		public static void SmoothCurves3D(IList<double> rt, IList<double> mz, IList<double> inten, IList<double> dm,
			out double[] zt, out double[] zm, out double[] zi, out LinearInterpolator lint, out LinearInterpolator linm,
			out LinearInterpolator lini, double[] xv, double[] yv, double[] a, int degree, int maxCount){
			if (rt.Count == 0){
				zt = new double[0];
				zm = new double[0];
				zi = new double[0];
				lint = new LinearInterpolator(0);
				linm = new LinearInterpolator(0);
				lini = new LinearInterpolator(0);
				return;
			}
			ArrayUtils.MinMax(inten, out double minInt, out double maxInt);
			double medInt = ArrayUtils.Median(inten);
			SmoothCurve3DImpl(rt, mz, inten, dm, out lint, out linm, out lini, xv, yv, a, degree, minInt, maxInt,
				medInt, maxCount);
			zt = new double[dm.Count];
			for (int i = 0; i < zt.Length; i++){
				zt[i] = lint.Get(rt[i]);
			}
			zm = new double[dm.Count];
			for (int i = 0; i < zm.Length; i++){
				zm[i] = linm.Get(mz[i]);
			}
			zi = new double[dm.Count];
			for (int i = 0; i < zi.Length; i++){
				zi[i] = lini.Get(inten[i]);
			}
		}

		private static void SmoothCurve3DImpl(IList<double> x1, IList<double> x2, IList<double> x3, IList<double> z,
			out LinearInterpolator linx1, out LinearInterpolator linx2, out LinearInterpolator linx3, double[] x1V,
			double[] x2V, double[] a, int degree, double minInt, double maxInt, double avInt, int maxCount){
			int nx = x1V.Length;
			int ny = x2V.Length;

			double Funcs3(int index, double[] aa, out double[] dyda){
				double xx1 = x1[index];
				double xx2 = x2[index];
				double xx3 = x3[index];
				double[] aaxx1 = new double[nx];
				for (int i = 0; i < nx; i++){
					aaxx1[i] = aa[i];
				}
				double[] aaxx2 = new double[ny];
				for (int i = 0; i < ny - 1; i++){
					aaxx2[i] = aa[nx + i];
				}
				double[] aaxx3 = new double[degree];
				for (int i = 0; i < degree; i++){
					aaxx3[i] = aa[nx + ny - 1 + i];
				}
				LinearInterpolator lix1 = new LinearInterpolator(x1V, aaxx1);
				LinearInterpolator lix2 = new LinearInterpolator(x2V, aaxx2);
				dyda = new double[nx + ny - 1 + degree];
				double vx1 = lix1.Get(xx1, out double[] dydax1);
				double vx2 = lix2.Get(xx2, out double[] dydax2);
				double vx3 = GetInt(xx3, aaxx3, out double[] dydax3, degree, avInt);
				for (int i = 0; i < nx; i++){
					dyda[i] = dydax1[i];
				}
				for (int i = 0; i < ny - 1; i++){
					dyda[nx + i] = dydax2[i];
				}
				for (int i = 0; i < degree; i++){
					dyda[nx + ny - 1 + i] = dydax3[i];
				}
				return vx1 + vx2 + vx3;
			}

			FitNonlin(z, a, Funcs3, maxCount);
			double[] aax1 = a.SubArray(0, nx);
			double[] aax2 = a.SubArray(nx, nx + ny - 1);
			aax2 = ArrayUtils.Concat(aax2, new double[]{0});
			linx1 = new LinearInterpolator(x1V, aax1);
			linx2 = new LinearInterpolator(x2V, aax2);
			linx1.FlattenEnds();
			linx2.FlattenEnds();
			double[] aax3 = a.SubArray(nx + ny - 1, nx + ny - 1 + degree);
			const int nintens = 66;
			double[] x3V = new double[nintens];
			double[] y3V = new double[nintens];
			for (int i = 0; i < nintens; i++){
				double xval = minInt + i * (maxInt - minInt) / (nintens - 1.0);
				x3V[i] = xval;
				y3V[i] = Poly(xval, aax3, avInt);
			}
			linx3 = new LinearInterpolator(x3V, y3V);
		}

		private static double Poly(double xval, IList<double> aax3, double avInt){
			double result = 0;
			for (int i = 0; i < aax3.Count; i++){
				result += Math.Pow(xval - avInt, i + 1) * aax3[i];
			}
			return result;
		}

		private static double GetInt(double xx3, IList<double> aaxx3, out double[] dydax3, int degree, double avInt){
			double xx = xx3 - avInt;
			double result = 0;
			dydax3 = new double[degree];
			for (int i = 0; i < degree; i++){
				result += aaxx3[i] * xx;
				dydax3[i] = xx;
				xx *= xx3 - avInt;
			}
			return result;
		}

		public static void SmoothCurves2D(IList<double> rt, IList<double> mz, IList<double> dm, out double[] zt,
			out double[] zm, out LinearInterpolator lint, out LinearInterpolator linm, double[] xv, double[] yv,
			double[] a, int maxCount){
			if (rt.Count == 0){
				zt = new double[0];
				zm = new double[0];
				lint = new LinearInterpolator(0);
				linm = new LinearInterpolator(0);
				return;
			}
			SmoothCurve2DImpl(rt, mz, dm, out lint, out linm, xv, yv, a, maxCount);
			zt = new double[dm.Count];
			for (int i = 0; i < zt.Length; i++){
				zt[i] = lint.Get(rt[i]);
			}
			zm = new double[dm.Count];
			for (int i = 0; i < zm.Length; i++){
				zm[i] = linm.Get(mz[i]);
			}
		}

		private static void SmoothCurve2DImpl(IList<double> x, IList<double> y, IList<double> z,
			out LinearInterpolator linx, out LinearInterpolator liny, double[] xv, double[] yv, double[] a,
			int maxCount){
			int nx = xv.Length;
			int ny = yv.Length;

			double Funcs3(int index, double[] aa, out double[] dyda){
				double xx = x[index];
				double yy = y[index];
				double[] aax = new double[nx];
				for (int i = 0; i < nx; i++){
					aax[i] = aa[i];
				}
				double[] aay = new double[ny];
				for (int i = 0; i < ny - 1; i++){
					aay[i] = aa[nx + i];
				}
				LinearInterpolator lix = new LinearInterpolator(xv, aax);
				LinearInterpolator liy = new LinearInterpolator(yv, aay);
				dyda = new double[nx + ny - 1];
				double vx = lix.Get(xx, out double[] dydax);
				double vy = liy.Get(yy, out double[] dyday);
				for (int i = 0; i < nx; i++){
					dyda[i] = dydax[i];
				}
				for (int i = 0; i < ny - 1; i++){
					dyda[nx + i] = dyday[i];
				}
				return vx + vy;
			}

			FitNonlin(z, a, Funcs3, maxCount);
			double[] aax1 = a.SubArray(0, nx);
			double[] aay1 = a.SubArray(nx, nx + ny - 1);
			aay1 = ArrayUtils.Concat(aay1, new double[]{0});
			linx = new LinearInterpolator(xv, aax1);
			liny = new LinearInterpolator(yv, aay1);
			linx.FlattenEnds();
			liny.FlattenEnds();
		}

		public static void FitNonlin(IList<double> y, double[] a, Funcs3 funcs3, int maxCount){
			int ma = a.Length;
			double[,] covar = new double[ma, ma];
			double[,] alpha = new double[ma, ma];
			double alamda = -1;
			double ochisq = 0;
			double[,] oneda = null;
			int mfit = 0;
			double[] atry = null;
			double[] beta = null;
			double[] da = null;
			Mrqmin(y, a, covar, alpha, out double chisq, ref alamda, ref ochisq, ref oneda, ref mfit, ref atry,
				ref beta, ref da, funcs3);
			int count1 = 0;
			while (alamda > 1e-100 && alamda < 1e100 && count1 < maxCount){
				Mrqmin(y, a, covar, alpha, out chisq, ref alamda, ref ochisq, ref oneda, ref mfit, ref atry, ref beta,
					ref da, funcs3);
				count1++;
			}
			alamda = 0;
			Mrqmin(y, a, covar, alpha, out chisq, ref alamda, ref ochisq, ref oneda, ref mfit, ref atry, ref beta,
				ref da, funcs3);
		}

		private static void Mrqmin(IList<double> y, double[] a, double[,] covar, double[,] alpha, out double chisq,
			ref double alamda, ref double ochisq, ref double[,] oneda, ref int mfit, ref double[] atry,
			ref double[] beta, ref double[] da, Funcs3 funcs3){
			int ma = a.Length;
			if (alamda < 0.0){
				atry = new double[ma];
				beta = new double[ma];
				da = new double[ma];
				mfit = ma;
				oneda = new double[mfit, 1];
				alamda = 0.001;
				Mrqcof(y, a, alpha, beta, out chisq, funcs3);
				ochisq = chisq;
				for (int j = 0; j < ma; j++){
					atry[j] = a[j];
				}
			}
			for (int l = 0; l < ma; l++){
				for (int m = 0; m < ma; m++){
					covar[l, m] = alpha[l, m];
				}
				covar[l, l] = alpha[l, l] * (1.0 + alamda);
				oneda[l, 0] = beta[l];
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
			for (int l = 0; l < ma; l++){
				atry[l] = a[l] + da[l];
			}
			Mrqcof(y, atry, covar, da, out chisq, funcs3);
			if (chisq < ochisq){
				alamda *= 0.1;
				ochisq = (chisq);
				for (int l = 0; l < ma; l++){
					for (int m = 0; m < ma; m++){
						alpha[l, m] = covar[l, m];
					}
					beta[l] = da[l];
					a[l] = atry[l];
				}
			} else{
				alamda *= 10.0;
				chisq = ochisq;
			}
		}

		private static void Mrqcof(IList<double> y, double[] a1, double[,] alpha, IList<double> beta, out double chisq,
			Funcs3 funcs3){
			int ma = a1.Length;
			int ndata = y.Count;
			for (int j = 0; j < ma; j++){
				for (int k = 0; k <= j; k++){
					alpha[j, k] = 0.0;
				}
				beta[j] = 0.0;
			}
			chisq = 0.0;
			for (int i1 = 0; i1 < ndata; i1++){
				double y1 = funcs3(i1, a1, out double[] dyda);
				double dy = y[i1] - y1;
				for (int l = 0; l < ma; l++){
					double wt = dyda[l];
					for (int m = 0; m <= l; m++){
						alpha[l, m] += wt * dyda[m];
					}
					beta[l] += dy * wt;
				}
				chisq += dy * dy;
			}
			for (int j = 1; j < ma; j++){
				for (int k = 0; k < j; k++){
					alpha[k, j] = alpha[j, k];
				}
			}
		}

		public static void SmoothCurves1D(double[] rt, double[] dm, out double[] zt, out LinearInterpolator lint,
			double[] xv, double[] a, int maxCount){
			if (rt.Length == 0){
				zt = new double[0];
				lint = new LinearInterpolator(0);
				return;
			}
			SmoothCurve1DImplNew(rt, dm, out lint, xv, a, maxCount);
			zt = new double[dm.Length];
			for (int i = 0; i < zt.Length; i++){
				zt[i] = lint.Get(rt[i]);
			}
		}

		private static void SmoothCurve1DImplNew(double[] x, double[] z, out LinearInterpolator linx, double[] xv,
			double[] a, int maxCount){
			const int splitThreshold = 500;
			const double overlapFraction = 0.3;
			if (x.Length <= splitThreshold){
				SmoothCurve1DImpl(x, z, out linx, xv, a, maxCount);
			}
			int[] o = x.Order();
			x = x.SubArray(o);
			z = z.SubArray(o);
			int nStretches = (int) Math.Max(2, Math.Round(x.Length / (double) splitThreshold));
			int[] stretchIndices = new int[nStretches + 1];
			int overlapLen = (int) ((stretchIndices[1] - stretchIndices[0]) * overlapFraction * 0.5);
			for (int i = 0; i <= nStretches; i++){
				stretchIndices[i] = (int) Math.Round(i / (double) nStretches * x.Length);
			}
			for (int i = 0; i < nStretches; i++){
				int xCentralStart = stretchIndices[i];
				int xCentralEnd = stretchIndices[i + 1];
				int xTotalStart = i == 0 ? xCentralStart : xCentralStart - overlapLen;
				int xTotalEnd = i == nStretches - 1 ? xCentralEnd : xCentralEnd + overlapLen;
				int aCentralStart = ArrayUtils.ClosestIndex(xv, x[xCentralStart]);
				int aCentralEnd = ArrayUtils.ClosestIndex(xv, x[Math.Min(xCentralEnd, x.Length - 1)]);
				int aTotalStart = ArrayUtils.ClosestIndex(xv, x[xTotalStart]);
				int aTotalEnd = ArrayUtils.ClosestIndex(xv, x[Math.Min(xTotalEnd, x.Length - 1)]);
				double[] currentX = x.SubArray(xTotalStart, xTotalEnd);
				double[] currentZ = z.SubArray(xTotalStart, xTotalEnd);
				double[] currentXv = xv.SubArray(aTotalStart, aTotalEnd);
				double[] currentA = a.SubArray(aTotalStart, aTotalEnd);
				int aTotalLen = currentA.Length;

				double Funcs3(int index, double[] aa, out double[] dyda){
					double xx = currentX[index];
					double[] aax = new double[aTotalLen];
					for (int j = 0; j < aTotalLen; j++){
						aax[j] = aa[j];
					}
					LinearInterpolator lix = new LinearInterpolator(currentXv, aax);
					dyda = new double[aTotalLen];
					double vx = lix.Get(xx, out double[] dydax);
					for (int j = 0; j < aTotalLen; j++){
						dyda[j] = dydax[j];
					}
					return vx;
				}

				FitNonlin(currentZ, currentA, Funcs3, maxCount);
				for (int j = aCentralStart; j < aCentralEnd; j++){
					a[j] = currentA[j - aTotalStart];
				}
			}
			linx = new LinearInterpolator(xv, a);
			linx.FlattenEnds();
		}

		private static void SmoothCurve1DImpl(IList<double> x, IList<double> z, out LinearInterpolator linx,
			double[] xv, double[] a, int maxCount){
			int nx = xv.Length;

			double Funcs3(int index, double[] aa, out double[] dyda){
				double xx = x[index];
				double[] aax = new double[nx];
				for (int i = 0; i < nx; i++){
					aax[i] = aa[i];
				}
				LinearInterpolator lix = new LinearInterpolator(xv, aax);
				dyda = new double[nx];
				double vx = lix.Get(xx, out double[] dydax);
				for (int i = 0; i < nx; i++){
					dyda[i] = dydax[i];
				}
				return vx;
			}

			FitNonlin(z, a, Funcs3, maxCount);
			double[] aax1 = a.SubArray(0, nx);
			linx = new LinearInterpolator(xv, aax1);
			linx.FlattenEnds();
		}
	}
}