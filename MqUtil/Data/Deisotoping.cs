using MqApi.Num;
using MqApi.Util;
using MqUtil.Mol;
using MqUtil.Ms.Enums;
using MqUtil.Ms.Utils;
using MqUtil.Num;
using MqUtil.Util;

namespace MqUtil.Data{
	public sealed class Deisotoping : IDisposable{
		public const int cacheSize = 1500000;
		private readonly byte minCharge;
		private readonly byte maxCharge;
		private readonly double correlationThreshold;
		private readonly double isotopeValleyFactor;
		private readonly string dataPath;
		private readonly double matchTol;
		private readonly bool matchTolInPpm;
		private readonly string tmpfile;
		private readonly BinaryReader tmpReader;
		private Cache<int, SlimPeak1> peakCache = new Cache<int, SlimPeak1>{MaxEntries = cacheSize};
		private List<byte> resultClusterCharges = new List<byte>();
		private List<int[]> resultClusters = new List<int[]>();
		private double[][] tmpCorr;
		private SlimPeak1[] tmpPeaks;
		private int[][] tmpx;
		private byte[] clusterCharges;
		private List<int[]> clusterInd;

		public Deisotoping(byte minCharge, byte maxCharge, double correlationThreshold, double isotopeValleyFactor,
			string dataPath, double matchTol, bool matchTolInPpm){
			this.minCharge = minCharge;
			this.maxCharge = maxCharge;
			this.correlationThreshold = correlationThreshold;
			this.isotopeValleyFactor = isotopeValleyFactor;
			this.dataPath = dataPath;
			this.matchTol = matchTol;
			this.matchTolInPpm = matchTolInPpm;
			tmpfile = dataPath + "_t1";
			tmpReader = FileUtils.GetBinaryReader(tmpfile);
		}

		public List<int[]> FindIsotopeClusters(out byte[] clusterCh, IDeisotopable3D peakList, int[] tmpFilePos,
			double isoCorrThres, MsDataType msDataType, bool checkMassDeficit, int minPeakLen, bool isDia, Responder responder, bool emptyResults) {
			string clusterTmpFile = dataPath + "_tmpclusters";
			int nclusters = CalcClusterIndices(peakList, minCharge, maxCharge, correlationThreshold, tmpReader,
				tmpFilePos, matchTol, matchTolInPpm, clusterTmpFile, checkMassDeficit, minPeakLen, isDia, responder, emptyResults);
			responder?.Comment("Initial clusters found.");
			peakList.MinTimes = null;
			peakList.MaxTimes = null;
			GC.Collect();
			clusterInd =
				ReadTmpClusters(clusterTmpFile, nclusters); // read the results of the CalcClusterIndices step above
			File.Delete(clusterTmpFile);
			clusterCharges = new byte[clusterInd.Count];
			// Keep looking for better isotope clusters as long as some are found. Write them to tmpFilePos.
			int c = 0;
			double[] intens = ArrayUtils.ToDoubles(peakList.Intensities);
			while (true){
				bool changed = ImproveIsotopeClusterIndices(peakList.CenterMz, intens, tmpFilePos, msDataType, 
					checkMassDeficit);
				if (c % 100 == 0){
					responder?.Comment("Improving clusters " + c);
				}
				c++;
				if (c > 100){
					break;
				}
				if (!changed){
					break;
				}
			}
			responder?.Comment("Filtering clusters.");
			int[] valids = FilterClusters(clusterInd, clusterCharges, peakList.CenterMz, intens,
				isoCorrThres);
			responder?.Comment("Done filtering clusters.");
			clusterInd = clusterInd.SubList(valids);
			clusterCharges = clusterCharges.SubArray(valids);
			clusterCh = clusterCharges;
			return clusterInd;
		}

		public static int[] FilterClusters(IList<int[]> clusterInds, IList<byte> charges, double[] centerMz,
			double[] intensities, double isoCorrThres){
			List<int> valids = new List<int>();
			for (int i = 0; i < clusterInds.Count; i++){
				FitIsotopeClusterDistribution(clusterInds[i], charges[i], centerMz, intensities, out double maxCorr,
					out int _);
				if (maxCorr >= isoCorrThres){
					valids.Add(i);
				}
			}
			return valids.ToArray();
		}

		public static List<int[]> ReadTmpClusters(string clusterTmpFile, int nclusters){
			BinaryReader reader = FileUtils.GetBinaryReader(clusterTmpFile);
			List<int[]> result = new List<int[]>();
			for (int i = 0; i < nclusters; i++){
				result.Add(FileUtils.ReadInt32Array(reader));
			}
			reader.Close();
			return result;
		}

		private bool ImproveIsotopeClusterIndices(double[] centerMz, double[] intensities, 
			int[] tmpFilePos, MsDataType msDataType, bool checkMassDeficit){
			bool hasChanged = false;
			resultClusters.Clear();
			resultClusterCharges.Clear();
			for (int i = 0; i < clusterInd.Count; i++){
				byte charge = clusterCharges[i];
				if (charge > 0){
					if (charge < minCharge || charge > maxCharge){
						hasChanged = true;
					} else{
						resultClusters.Add(clusterInd[i]);
						resultClusterCharges.Add(charge);
					}
				} else{
					tmpx = SplitIsotopeCluster(clusterInd[i], out byte ch, centerMz, intensities, 
						tmpFilePos, msDataType, checkMassDeficit);
					if (tmpx.Length > 1){
						hasChanged = true;
					}
					for (int j = 0; j < tmpx.Length; j++){
						if (tmpx[j].Length == 1){
							//do nothing (a singleton)
						} else if (tmpx[j].Length > 1){
							resultClusters.Add(tmpx[j]);
							resultClusterCharges.Add(j == 0 ? ch : (byte) 0);
						}
						tmpx[j] = null;
					}
				}
				clusterInd[i] = null;
			}
			for (int i = 0; i < clusterInd.Count; i++){
				clusterInd[i] = null;
			}
			clusterInd.Clear();
			clusterCharges = resultClusterCharges.ToArray();
			clusterInd = new List<int[]>(resultClusters);
			return hasChanged;
		}

		/// <summary>
		/// Given a pre-cluster, split it into smaller clusters that are consistent in terms of charge, etc.
		/// </summary>
		private int[][] SplitIsotopeCluster(int[] m, out byte resultCharge, double[] centerMz,
			double[] intensities, int[] tmpFilePos, MsDataType msDataType, bool checkMassDeficit){
			int n = m.Length;
			if (n < 2){
				throw new Exception("Cannot split cluster of length " + n + ".");
			}
			int[] bestMatches;
			if (msDataType != MsDataType.Proteins && n > 120){
				//brute force split to avoid extremely long running times.
				resultCharge = 0;
				bestMatches = new int[n / 2];
				for (int i = 0; i < bestMatches.Length; i++){
					bestMatches[i] = i;
				}
			} else{
				CalcCorrelations(n, m, tmpFilePos);
				double isoPatternDiff = msDataType == MsDataType.Proteins
					? MolUtil.isotopePatternDiffProt
					: MolUtil.isotopePatternDiff;
				// Find the largest subset which match each other in terms of charge, ...
				double[] cmz = centerMz.SubArray(m);
				double[] intens = intensities.SubArray(m);
				int[][] matches = new int[maxCharge - minCharge + 1][];
				for (byte ch = minCharge; ch <= maxCharge; ch++){
					matches[ch - minCharge] =
						GetBestMatchesForCharge(n, ch, cmz, intens, isoPatternDiff, checkMassDeficit);
				}
				bestMatches = GetBestMatchesNoMsms(matches, out byte bestMatchesCharge);
				resultCharge = bestMatchesCharge;
				Nullify(n);
			}
			bestMatches = ArrayUtils.UniqueValues(bestMatches);
			return bestMatches.Length < n
				? new[]{m.SubArray(bestMatches), m.SubArray(ArrayUtils.Complement(bestMatches, n))}
				: new[]{m};
		}

		private int[] GetBestMatchesUseMsms(int[][] matches, bool[] matchesMsms, out byte bestMatchesCharge){
			int[] bestMatches = new int[0];
			bestMatchesCharge = 0;
			for (byte ch = maxCharge; ch >= minCharge; ch--){
				int[] matches1 = matches[ch - minCharge];
				bool matchesMsms1 = matchesMsms[ch - minCharge];
				if (matchesMsms1 && matches1.Length > 2 && matches1.Length > bestMatches.Length){
					bestMatches = matches1;
					bestMatchesCharge = ch;
				}
			}
			if (bestMatchesCharge == 0){
				bestMatches = GetBestMatchesNoMsms(matches, out bestMatchesCharge);
			}
			return bestMatches;
		}

		private int[] GetBestMatchesNoMsms(int[][] matches, out byte bestMatchesCharge){
			int[] bestMatches = new int[0];
			bestMatchesCharge = 0;
			for (byte ch = maxCharge; ch >= minCharge; ch--){
				int[] matches1 = matches[ch - minCharge];
				if (matches1.Length > bestMatches.Length){
					bestMatches = matches1;
					bestMatchesCharge = ch;
				}
			}
			return bestMatches;
		}

		private int[] GetBestMatchesForCharge(int n, byte ch, double[] cmz, double[] intens, double isoPatternDiff,
			bool checkMassDeficit){
			int[] bestMatches = new int[0];
			for (int i = 0; i < n; i++){
				int[] matches = GetMatches(ch, i, correlationThreshold, tmpCorr, cmz, intens, isotopeValleyFactor,
					matchTol, matchTolInPpm, isoPatternDiff, checkMassDeficit);
				if (matches.Length > bestMatches.Length){
					bestMatches = matches;
				}
			}
			return bestMatches;
		}

		private void CalcCorrelations(int n, int[] m, IList<int> tmpFilePos){
			tmpCorr = new double[n][];
			tmpPeaks = new SlimPeak1[n];
			// Find the lowest and the highest indices of any peak in the list
			for (int i = 0; i < n; i++){
				tmpPeaks[i] = GetPeak(m[i], peakCache, tmpReader, tmpFilePos);
			}
			for (int i = 0; i < n; i++){
				tmpCorr[i] = new double[i];
				for (int j = 0; j < i; j++){
					tmpCorr[i][j] = CalcCorrelation(tmpPeaks[i], tmpPeaks[j]);
				}
			}
		}

		/// <summary>
		/// Dispose of variables to be reused in the next iteration. 
		/// </summary>
		private void Nullify(int n){
			for (int i = 0; i < n; i++){
				tmpCorr[i] = null;
			}
			for (int i = 0; i < tmpPeaks.Length; i++){
				tmpPeaks[i] = null;
			}
			tmpCorr = null;
			tmpPeaks = null;
		}

		/// <summary>
		/// Given arrays with the centroid mass, minimum time, and maximum time for a set of peaks, calculate the clusters 
		/// and write them to a file. Return the number found.
		/// </summary>
		private static int CalcClusterIndices(IDeisotopable3D peakList, byte minCharge, byte maxCharge,
			double correlationThreshold, BinaryReader tmpReader, IList<int> tmppos, double matchTol, bool matchTolInPpm,
			string clusterTmpFile, bool checkMassDeficit, int minPeakLen, bool isDia, Responder responder, bool emptyResults){
			double[] centerMz = peakList.CenterMz;
			int[] o = centerMz.Order(); // the indices that will put centerMz in order, 
			// i.e. centerMz[o[i]] is less than or equal to centerMz[o[i+1]]
			double[] centerMzOrdered = centerMz.SubArray(o);
			int[] minRtIndsOrdered = peakList.MinRtIndsTmp.SubArray(o);
			int[] maxRtIndsOrdered = peakList.MaxRtIndsTmp.SubArray(o);
			Cache<int, SlimPeak1> peakCache = new Cache<int, SlimPeak1>{MaxEntries = cacheSize };
			BinaryWriter clusterWriter = FileUtils.GetBinaryWriter(clusterTmpFile);
			int clusterCount = 0;
			const int ncheck = 1000000;
			NeighbourList neighbourList = new NeighbourList(); // which elements are "neighbors" to which other elements
			int npeaks = centerMz.Length;
			if (!emptyResults) {
				for (int jOrdered = 0; jOrdered < npeaks; jOrdered++) {
					if (jOrdered % 10000 == 0) {
						responder?.Comment("1st cl. " + jOrdered / 1000 + "k/" + npeaks / 1000 + "k (" +
						                   NumUtils.GetPercentageString(jOrdered / (double)npeaks) + ")");
					}
					SlimPeak1 peakj = null;
					double massJ = centerMzOrdered[jOrdered];
					double err = matchTolInPpm ? matchTol * massJ * 1e-6 : matchTol;
					double me2 = err * err;
					int timeMinJ = minRtIndsOrdered[jOrdered];
					int timeMaxJ = maxRtIndsOrdered[jOrdered];
					int start = ArrayUtils.CeilIndex(centerMzOrdered, massJ - 1.03);
					int j = o[jOrdered];
					for (int iOrdered = start; iOrdered < jOrdered; iOrdered++) {
						int timeMinI = minRtIndsOrdered[iOrdered];
						int timeMaxI = maxRtIndsOrdered[iOrdered];
						if (isDia) {
							bool shortI = timeMaxI - timeMinI < minPeakLen - 1;
							if (shortI) {
								continue;
							}
						}
						double massDiff = Math.Abs(centerMzOrdered[iOrdered] - massJ);
						if (timeMinI > timeMaxJ || timeMinJ > timeMaxI) {
							continue;
						}
						if (Invalid(massDiff, timeMinI, timeMaxI, timeMinJ, timeMaxJ, maxCharge)) {
							continue;
						}
						if (!FitsMassDifference(massDiff, me2, minCharge, maxCharge, massJ, checkMassDeficit)) {
							continue;
						}
						if (peakj == null) {
							peakj = GetPeak(jOrdered, j, peakCache, tmpReader, tmppos);
						}
						int i = o[iOrdered];
						SlimPeak1 peaki = GetPeak(iOrdered, i, peakCache, tmpReader, tmppos);
						double xy = CalcCorrelation(peaki, peakj);
						if (xy < correlationThreshold) {
							continue;
						}
						neighbourList.Add(i, j);
					}
					if (jOrdered > 0 && jOrdered % ncheck == 0) {
						CalcClusters(neighbourList, clusterWriter, ref clusterCount, massJ - 2.2, centerMz);
					}

				}
			}
			// Then go back and find any remaining clusters.
			CalcClusters(neighbourList, clusterWriter, ref clusterCount, double.MaxValue, centerMz);
			clusterWriter.Close();
			return clusterCount;
		}

		/// <summary>
		/// To be consecutive peaks in the same isotope pattern, two mq-rt-peaks must have a mass difference of approx. 1 Dalton 
		/// divided by the charge of the ion. Mass differences where this can be clearly ruled out are invalid. Furthermore, 
		/// they must overlap in retention time, and the total elution times cannot be wildly different. Finally, the duration 
		/// of the mismatch in retention times at the ends should be smaller than the duration of the overlap.
		/// </summary>
		private static bool Invalid(double massDiff, int timeMinI, int timeMaxI, int timeMinJ, int timeMaxJ,
			byte maxCharge){
			if (InvalidMassDiff(massDiff, maxCharge)){
				return true;
			}
			int ti = timeMaxI - timeMinI;
			int tj = timeMaxJ - timeMinJ;
			if (ti > 4 * tj || 4 * ti < tj){
				return true;
			}
			if (timeMaxJ > timeMaxI && timeMaxI > timeMinJ && timeMinJ > timeMinI){
				int shorterLen = Math.Min(ti, tj);
				int overlap = timeMaxI - timeMinJ;
				if (overlap < shorterLen * 0.8){
					return true;
				}
			}
			if (timeMaxI > timeMaxJ && timeMaxJ > timeMinI && timeMinI > timeMinJ){
				int shorterLen = Math.Min(ti, tj);
				int overlap = timeMaxJ - timeMinI;
				if (overlap < shorterLen * 0.8){
					return true;
				}
			}
			return false;
		}

		public static bool InvalidMassDiff(double massDiff, byte maxCharge){
			if (massDiff < 0.97 && massDiff > 0.53){
				return true;
			}
			if (massDiff < 0.47 && massDiff > 0.37){
				return true;
			}
			if (massDiff < 1.0 / maxCharge - 0.02){
				return true;
			}
			return false;
		}

		public static unsafe int CeilIndex(double* arraySorted, double value, int n){
			if (n == 0){
				return -1;
			}
			if (value > arraySorted[n - 1]){
				return -1;
			}
			if (value < arraySorted[0]){
				return 0;
			}
			int a = BinarySearch(arraySorted, value, n);
			if (a >= 0){
				while (a > 0 && arraySorted[a - 1] == arraySorted[a]){
					a--;
				}
				return a;
			}
			return -1 - a;
		}

		public static unsafe int BinarySearch(double* arraySorted, double key, int n){
			int first = 0;
			int upto = n;
			while (first < upto){
				int mid = (first + upto) / 2; // Compute mid point.
				if (key < arraySorted[mid]){
					upto = mid; // repeat search in bottom half.
				} else if (key > arraySorted[mid]){
					first = mid + 1; // Repeat search in top half.
				} else{
					return mid; // Found it. return position
				}
			}
			return -(first + 1); // Failed to find key
		}
		
		public static unsafe int CeilIndex(double* array, int* order, double value, int n){
			if (n == 0){
				return -1;
			}
			if (value > array[order[n - 1]]){
				return -1;
			}
			if (value < array[order[0]]){
				return 0;
			}
			int a = BinarySearch(array, order, value, n);
			if (a >= 0){
				while (a > 0 && array[order[a - 1]] == array[order[a]]){
					a--;
				}
				return a;
			}
			return -1 - a;
		}

		// TODO: migrate to Array.BinarySearch
		public static unsafe int BinarySearch(double* unsorted, int* order, double key, int n){
			int first = 0;
			int upto = n;
			while (first < upto){
				int mid = (first + upto) / 2; // Compute mid point.
				if (key < unsorted[order[mid]]){
					upto = mid; // repeat search in bottom half.
				} else if (key > unsorted[order[mid]]){
					first = mid + 1; // Repeat search in top half.
				} else{
					return mid; // Found it. return position
				}
			}
			return -(first + 1); // Failed to find key
		}

		/// <summary>
		/// Find "clusters" as neighbors and neighbors of neighbors, and write them to a file.
		/// </summary>
		/// <param name="neighbourList">for each mass, which other masses are (or could be) in the same isotope pattern</param>
		/// <param name="clusterWriter">The only output, other than changes to clusterCount, is what is written here, 
		/// the clusters as a series of int arrays.</param>
		/// <param name="clusterCount">number of clusters found; passed by reference and incremented when a cluster is found</param>
		/// <param name="mlimit">clusters are only kept if no member has a mass higher than this</param>
		/// <param name="maxInd">only consider clusters up to this index</param>
		/// <param name="centerMz">the masses corresponding to the indices in neighborList</param>
		public static void CalcClusters(NeighbourList neighbourList, BinaryWriter clusterWriter, ref int clusterCount,
			double mlimit, IList<double> centerMz){
			foreach (int i in neighbourList.Keys){
				if (!neighbourList.IsEmptyAt(i)){
					int[] currentCluster = neighbourList.GetClusterAtNoRemove(i);
					int[] c = SortByMass(currentCluster, centerMz);
					// currentCluster is now an int array of the indices which are neighbors, or neighbors of neighbors, 
					// of index i, ordered by mass
					if (centerMz[c[c.Length - 1]] < mlimit){
						neighbourList.RemoveCluster(currentCluster);
						FileUtils.Write(c, clusterWriter);
						clusterCount++;
					}
				}
			}
		}

		private static SlimPeak1 GetPeak(int iOrdered, int i, Cache<int, SlimPeak1> peakCache, BinaryReader tmpReader,
			IList<int> tmppos){
			SlimPeak1 result;
			if (!peakCache.ContainsKey(iOrdered)){
				tmpReader.BaseStream.Seek(tmppos[i], SeekOrigin.Begin);
				result = new SlimPeak1(tmpReader);
				peakCache.Add(iOrdered, result);
			} else{
				result = peakCache[iOrdered];
			}
			return result;
		}
		
		private static SlimPeak1 GetPeak(int i, ICache<int, SlimPeak1> peakCache, BinaryReader tmpReader,
			IList<int> tmppos){
			if (peakCache.ContainsKey(i)){
				return peakCache[i];
			}
			tmpReader.BaseStream.Seek(tmppos[i], SeekOrigin.Begin);
			SlimPeak1 p = new SlimPeak1(tmpReader);
			peakCache.Add(i, p);
			return p;
		}

		private static int[] SortByMass(IList<int> c, IList<double> centerMz){
			double[] m = centerMz.SubArray(c);
			int[] o = m.Order();
			return c.SubArray(o);
		}

		private static readonly double s2 = Molecule.sulphurShift * Molecule.sulphurShift;

		/// <summary>
		/// Whether there is some charge state (between minCharge and maxCharge) 
		/// such that a mass of mz plus massDiff might be the next peak after mz is some
		/// isotope pattern. The 3rd arg is the mean square error of the mass.
		/// </summary>
		/// <param name="massDiff">difference between the mass under consideration and the reference mass</param>
		/// <param name="me2">mass error squared</param>
		/// <param name="minCharge">lowest charge state considered</param>
		/// <param name="maxCharge">highest charge state considered</param>
		/// <param name="mz">reference mass</param>
		/// <param name="checkMassDeficit">should it be checked if the charge determined by the peak mass difference 
		/// would correspond to a mass in the 'allowed' region.</param>
		/// <returns></returns>
		public static bool FitsMassDifference(double massDiff, double me2, int minCharge, int maxCharge, double mz,
			bool checkMassDeficit){
			for (int charge = minCharge; charge <= maxCharge; charge++){
				double e2 = s2 + me2 * charge * charge;
				double d = massDiff * charge - MolUtil.isotopePatternDiff;
				if (d * d <= e2){
					if (checkMassDeficit && !MolUtil.IsSuitableMass((mz - 1.0072764666) * charge)){
						continue;
					}
					return true;
				}
				if (d > 0){
					break;
				}
			}
			return false;
		}

		/// <summary>
		/// Time integral of the product of the smoothed intensities, normalized to the rms values.
		/// </summary>
		private static double CalcCorrelation(SlimPeak1 pI, SlimPeak1 pJ){
			int[] indI = pI.ScanIndices;
			int[] indJ = pJ.ScanIndices;
			float[] smintI = pI.SmoothIntensities;
			float[] smintJ = pJ.SmoothIntensities;
			int minIndex = Math.Min(indI[0], indJ[0]);
			int maxIndex = Math.Max(indI[indI.Length - 1], indJ[indJ.Length - 1]);
			int len = maxIndex - minIndex + 1;
			double[] profileI = new double[len];
			double[] profileJ = new double[len];
			for (int a = 0; a < indI.Length; a++){
				profileI[indI[a] - minIndex] = smintI[a];
			}
			for (int a = 0; a < indJ.Length; a++){
				profileJ[indJ[a] - minIndex] = smintJ[a];
			}
			return ArrayUtils.Cosine(profileI, profileJ);
		}

		public static int[] GetMatches(int charge, int startIndex, double threshold, double[][] corr, double[] masses,
			double[] intensities, double isotopeValleyFactor, double matchTol, bool matchTolInPpm,
			double isoPatternDiff, bool checkMassDeficit){
			double startMass = masses[startIndex];
			if (checkMassDeficit && !MolUtil.IsSuitableMass((startMass - Molecule.massProton) * charge)){
				return new int[0];
			}
			int[] upMatches = GetUpMatches(charge, startIndex, threshold, corr, masses, matchTol, matchTolInPpm,
				isoPatternDiff);
			int[] downMatches = GetDownMatches(charge, startIndex, threshold, corr, masses, matchTol, matchTolInPpm,
				isoPatternDiff);
			int[] result = MergeMatches(upMatches, downMatches, startIndex);
			double[] profile = intensities.SubArray(result);
			if (IsLocalMinimum(profile, downMatches.Length)){
				return new int[0];
			}
			int[] minima = GetLocalMinima(profile, isotopeValleyFactor);
			if (minima.Length > 0){
				result = CutMinima(result, minima, downMatches.Length);
			}
			if (charge * startMass < 1000){
				//For light masses remove the peak left of the monoisotope, in case it exists.
				double[] p = intensities.SubArray(result);
				double max = double.MinValue;
				int maxPos = -1;
				for (int i = 0; i < p.Length; i++){
					if (p[i] > max){
						max = p[i];
						maxPos = i;
					}
				}
				if (maxPos != 0){
					return result.SubArray(maxPos, result.Length);
				}
			}
			return result;
		}

		private static int[] CutMinima(int[] result, int[] minima, int pos){
			int lower = 0;
			foreach (int t in minima){
				if (t < pos){
					lower = t;
				}
			}
			int upper = result.Length - 1;
			for (int i = minima.Length - 1; i >= 0; i--){
				if (minima[i] > pos){
					upper = minima[i];
				}
			}
			return result.SubArray(lower, upper + 1);
		}

		private static unsafe int[] MergeMatches(int[] upMatches, int[] downMatches, int startIndex){
			int[] result = new int[upMatches.Length + downMatches.Length + 1];
			fixed (int* presult = result){
				for (int i = 0; i < downMatches.Length; i++){
					presult[i] = downMatches[downMatches.Length - i - 1];
				}
				result[downMatches.Length] = startIndex;
				for (int i = 0; i < upMatches.Length; i++){
					presult[downMatches.Length + 1 + i] = upMatches[i];
				}
			}
			return result;
		}

		private static int[] GetDownMatches(int charge, int startIndex, double threshold, double[][] corr,
			double[] masses, double matchTol, bool matchTolInPpm, double isoPatternDiff){
			double startMass = masses[startIndex];
			int[] downMatches = new int[masses.Length];
			int downMatchesLen = 0;
			for (int i = 1;; i++){
				double m = startMass - i * isoPatternDiff / charge;
				double err = matchTolInPpm ? matchTol * m * 1e-6 : matchTol;
				int[] fits = CollectFittingMasses(startIndex, m, masses, err, charge);
				if (fits.Length == 0){
					break;
				}
				if (fits.Length == 1){
					int minInd = Math.Min(startIndex, fits[0]);
					int maxInd = Math.Max(startIndex, fits[0]);
					double cor = corr[maxInd][minInd];
					if (cor >= threshold){
						if (downMatchesLen >= downMatches.Length){
							break;
						}
						downMatches[downMatchesLen++] = fits[0];
					} else{
						break;
					}
				} else{
					double[] corrs = new double[fits.Length];
					for (int j = 0; j < fits.Length; j++){
						int minInd = Math.Min(startIndex, fits[j]);
						int maxInd = Math.Max(startIndex, fits[j]);
						corrs[j] = corr[maxInd][minInd];
					}
					int[] o = corrs.Order();
					int maxIndex = o[o.Length - 1];
					if (corrs[maxIndex] >= threshold){
						if (downMatchesLen >= downMatches.Length){
							break;
						}
						downMatches[downMatchesLen++] = fits[maxIndex];
					} else{
						break;
					}
				}
			}
			Array.Resize(ref downMatches, downMatchesLen);
			return downMatches;
		}

		private static int[] GetUpMatches(int charge, int startIndex, double threshold, double[][] corr,
			double[] masses, double matchTol, bool matchTolInPpm, double isoPatternDiff){
			double startMass = masses[startIndex];
			int[] upMatches = new int[masses.Length];
			int upMatchesLen = 0;
			for (int i = 1;; i++){
				double m = startMass + i * isoPatternDiff / charge;
				double err = matchTolInPpm ? matchTol * m * 1e-6 : matchTol;
				int[] fits = CollectFittingMasses(startIndex, m, masses, err, charge);
				if (fits.Length == 0){
					break;
				}
				if (fits.Length == 1){
					int minInd = Math.Min(startIndex, fits[0]);
					int maxInd = Math.Max(startIndex, fits[0]);
					double cor = corr[maxInd][minInd];
					if (cor >= threshold){
						if (upMatchesLen >= upMatches.Length){
							break;
						}
						upMatches[upMatchesLen++] = fits[0];
					} else{
						break;
					}
				} else{
					double[] corrs = new double[fits.Length];
					for (int j = 0; j < fits.Length; j++){
						int minInd = Math.Min(startIndex, fits[j]);
						int maxInd = Math.Max(startIndex, fits[j]);
						corrs[j] = corr[maxInd][minInd];
					}
					int[] o = corrs.Order();
					int maxIndex = o[o.Length - 1];
					if (corrs[maxIndex] >= threshold){
						if (upMatchesLen >= upMatches.Length){
							break;
						}
						upMatches[upMatchesLen++] = fits[maxIndex];
					} else{
						break;
					}
				}
			}
			Array.Resize(ref upMatches, upMatchesLen);
			return upMatches;
		}

		private static int[] CollectFittingMasses(int index, double m, IList<double> masses, double ppmErr, int charge){
			List<int> fits = new List<int>();
			for (int i = 0; i < masses.Count; i++){
				if (i == index){
					continue;
				}
				double error =
					Math.Sqrt(Molecule.sulphurShift * Molecule.sulphurShift / charge / charge + ppmErr * ppmErr);
				if (Math.Abs(m - masses[i]) <= error){
					fits.Add(i);
				}
			}
			return fits.ToArray();
		}

		private static int[] GetLocalMinima(double[] profile, double isotopeValleyFactor){
			List<int> mins = new List<int>();
			for (int i = 1; i < profile.Length - 1; i++){
				if (IsLocalMinimum(profile, i)){
					mins.Add(i);
				}
			}
			List<int> result = new List<int>();
			foreach (int min in mins){
				double maxL = GetLeftMax(profile, min);
				double maxR = GetRightMax(profile, min);
				double minval = profile[min];
				double smallMax = Math.Min(maxL, maxR);
				if (smallMax / minval >= isotopeValleyFactor){
					result.Add(min);
				}
			}
			return result.ToArray();
		}

		private static double GetLeftMax(double[] profile, int min){
			double max = double.MinValue;
			for (int i = 0; i < min; i++){
				if (profile[i] > max){
					max = profile[i];
				}
			}
			return max;
		}

		private static double GetRightMax(double[] profile, int min){
			double max = double.MinValue;
			for (int i = min + 1; i < profile.Length; i++){
				if (profile[i] > max){
					max = profile[i];
				}
			}
			return max;
		}

		private static bool IsLocalMinimum(double[] profile, int index){
			if (index == 0 || index == profile.Length - 1){
				return false;
			}
			return profile[index - 1] > profile[index] && profile[index + 1] > profile[index];
		}

		public void CloseTmpFile(){
			tmpReader.Close();
			File.Delete(tmpfile);
		}

		public void Dispose(){
			if (resultClusterCharges != null){
				resultClusterCharges.Clear();
				resultClusterCharges = null;
			}
			if (resultClusters != null){
				resultClusters.Clear();
				resultClusters = null;
			}
			if (tmpCorr != null){
				for (int i = 0; i < tmpCorr.Length; i++){
					tmpCorr[i] = null;
				}
				tmpCorr = null;
			}
			if (tmpx != null){
				for (int i = 0; i < tmpx.Length; i++){
					tmpx[i] = null;
				}
				tmpx = null;
			}
			tmpPeaks = null;
			if (peakCache != null){
				peakCache.Dispose();
				peakCache = null;
			}
		}

		public static int[][] SortIsotopeClusterIndicesByCharge(int minCharge, int maxCharge, IDeisotopable3D peakList){
			int nCharge = maxCharge - minCharge + 1;
			List<int>[] lists = new List<int>[nCharge];
			for (int i = 0; i < nCharge; i++){
				lists[i] = new List<int>();
			}
			for (int i = 0; i < peakList.IsotopeClusterCount; i++){
				IsotopeCluster c = peakList.GetIsotopeCluster(i);
				lists[Math.Max(c.Charge - minCharge, 0)].Add(i);
			}
			int[][] result = new int[nCharge][];
			for (int i = 0; i < nCharge; i++){
				result[i] = lists[i].ToArray();
			}
			return result;
		}

		/// <summary>
		/// Given an IPeakListLayer list of IsotopeClusters, set IsotopeCorrelation and IsotopePatternStart of each of
		/// its IsotopePatterns. 
		/// </summary>
		/// <param name="peakList"></param>
		public static void FitIsotopeClusterDistributions(IDeisotopable3D peakList){
			double[] centerMz = peakList.CenterMz;
			double[] intensities = ArrayUtils.ToDoubles(peakList.Intensities);
			for (int i = 0; i < peakList.IsotopeClusterCount; i++){
				FitIsotopeClusterDistribution(peakList.GetIsotopeCluster(i), centerMz, intensities);
			}
		}

		/// <summary>
		/// Given an IsotopeCluster with Members and Charge already set, and a spectrum (centerMz and intensities), 
		/// set IsotopeCorrelation and IsotopePatternStart of that IsotopeCluster (and set MassError to NaN).
		/// </summary>
		private static void FitIsotopeClusterDistribution(IsotopeCluster ic, double[] centerMz, double[] intensities){
			FitIsotopeClusterDistribution(ic.Members, ic.Charge, centerMz, intensities, out double maxCorr,
				out int maxCorrInd);
			ic.IsotopeCorrelation = maxCorr;
			ic.IsotopePatternStart = maxCorrInd;
			ic.MassError = double.NaN;
		}

		/// <summary>
		/// Given an isotope cluster (indices of members and charge), and a spectrum (centerMz and intensities), 
		/// set the maximum correlation, and the offset at which that correlation is found.
		/// </summary>
		/// <param name="members">The indices of centerMz and intensities that belong to one isotope cluster.</param>
		/// <param name="charge">Ionic charge (to determine spacing of peaks).</param>
		/// <param name="centerMz">Masses of the measured peaks.</param>
		/// <param name="intensities">Intensities of the measured peaks.</param>
		/// <param name="maxCorr">The correlation of the theoretical intensities with the measured intensities, 
		/// when the patterns are optimally aligned.</param>
		/// <param name="maxCorrInd">The number of places the theoretical intensity pattern has to be shifted to the left 
		/// to have the maximum correlation with the measured intensity pattern.</param>
		private static void FitIsotopeClusterDistribution(int[] members, int charge, double[] centerMz,
			double[] intensities, out double maxCorr, out int maxCorrInd){
			double[][] isotopePattern1 = GetIsotopePattern(members, charge, centerMz, intensities);
			double[] weights = isotopePattern1[1]; // 2nd element of an isotope pattern is the relative weights of the peaks
			double approxMass = isotopePattern1[0][0];
			// mass of the lowest mass (and therefore probably the most abundant) peak in the pattern
			double[][] averagePattern = MolUtil.GetAverageIsotopePattern(approxMass, true);
			maxCorrInd = NumUtils.Fit(averagePattern[1], weights, out maxCorr);
		}

		/// <summary>
		/// Given the indices of the peaks in a spectrum that belong to an isotope pattern and the charge of the ion, 
		/// return the corresponding isotope pattern (an array double[2][], where the first vector is the masses 
		/// and the 2nd is the relative intensities).
		/// </summary>
		/// <param name="members">indices of the elements of centerMz and intensities that belong to the isotope pattern</param>
		public static double[][] GetIsotopePattern(int[] members, int charge, double[] centerMz, double[] intensities){
			double[][] result = new double[2][];
			result[0] = new double[members.Length];
			result[1] = new double[members.Length];
			for (int i = 0; i < members.Length; i++){
				result[0][i] = charge * (centerMz[members[i]] - Molecule.massProton);
				result[1][i] = intensities[members[i]];
			}
			return result;
		}

		public static void FindIsotopeClusters(byte minCharge, byte maxCharge, double timeCorrThresh,
			double isoCorrThresh, IDeisotopable3D peakList, string dataPath, double matchTol, bool matchTolInPpm,
			string filename, bool hasMzBounds, double isotopeValleyFactor, bool precalcIntensities,
			MsDataType msDataType, bool checkMassDeficit, int minPeakLen, bool isDia, Responder responder, bool emptyResults) {
			ReadTmpData(filename, hasMzBounds, out int[] tmpFilePos, out double[] tmpTime);
			Deisotoping deiso = new Deisotoping(minCharge, maxCharge, timeCorrThresh, isotopeValleyFactor, dataPath,
				matchTol, matchTolInPpm);
			List<int[]> clusterInd = deiso.FindIsotopeClusters(out byte[] clusterCharges, peakList, tmpFilePos,
				isoCorrThresh, msDataType, checkMassDeficit, minPeakLen, isDia, responder, emptyResults);
			deiso.CloseTmpFile();
			deiso.Dispose();
			RegisterIsotopeClusters(minCharge, maxCharge, peakList, clusterInd, clusterCharges, tmpTime,
				precalcIntensities, responder);
		}

		private static void ReadTmpData(string filename, bool hasMzBounds, out int[] tmpFilePos, out double[] tmpTime){
			// Read tmpFilePos and tmpTime from filename, then delete filename.
			BinaryReader reader = FileUtils.GetBinaryReader(filename);
			int n = reader.ReadInt32();
			tmpFilePos = new int[n];
			tmpTime = new double[n];
			for (int i = 0; i < n; i++){
				reader.ReadDouble();
				reader.ReadInt64();
				tmpFilePos[i] = reader.ReadInt32();
				reader.ReadSingle();
				reader.ReadDouble();
				reader.ReadDouble();
				reader.ReadInt32();
				reader.ReadInt32();
				tmpTime[i] = reader.ReadDouble();
				if (hasMzBounds){
					reader.ReadDouble();
					reader.ReadDouble();
				}
			}
			reader.Close();
			File.Delete(filename);
		}

		private static void RegisterIsotopeClusters(byte minCharge, byte maxCharge, IDeisotopable3D peakList,
			List<int[]> clusterInd, byte[] clusterCharges, double[] tmpTimes, bool precalcIntensities,
			Responder responder){
			responder?.Comment("Done filtering clusters. 001");
			int[] isotopeClusterIndices = new int[peakList.Count];
			for (int i = 0; i < peakList.Count; i++){
				isotopeClusterIndices[i] = -1;
			}
			for (int i = 0; i < clusterInd.Count; i++){
				for (int j = 0; j < clusterInd[i].Length; j++){
					if (isotopeClusterIndices[clusterInd[i][j]] != -1){ }
					isotopeClusterIndices[clusterInd[i][j]] = i;
				}
			}
			responder?.Comment("Done filtering clusters. 002");
			peakList.IsotopeClusterIndices = isotopeClusterIndices;
			peakList.CreateIsotopeClusters(clusterInd, clusterCharges, maxCharge);
			responder?.Comment("Done filtering clusters. 003");
			FitIsotopeClusterDistributions(peakList);
			responder?.Comment("Done filtering clusters. 004");
			SetIntensities(peakList, precalcIntensities);
			responder?.Comment("Done filtering clusters. 005");
			peakList.SetIsoClusterIdsByCharge(SortIsotopeClusterIndicesByCharge(minCharge, maxCharge, peakList));
			responder?.Comment("Done filtering clusters. 006");
			peakList.CenterMz = null;
			GC.Collect();
			CalcIsotopeRetentionTimes(peakList, peakList.MinTimes, peakList.MaxTimes, peakList.Intensities, tmpTimes);
			responder?.Comment("Done filtering clusters. 007");
		}

		private static void SetIntensities(IDeisotopable3D peakList, bool precalcIntensities){
			if (precalcIntensities){
				peakList.PrecalcIntensities();
			} else{
				for (int i = 0; i < peakList.IsotopeClusterCount; i++){
					peakList.GetIsotopeCluster(i).Intensity = peakList.GetIsotopeClusterIntensity(i);
				}
			}
		}

		private static void CalcIsotopeRetentionTimes(IDeisotopable3D peakList, float[] minTimes,
			float[] maxTimes, float[] intensities, IList<double> tmpTimes){
			for (int i = 0; i < peakList.IsotopeClusterCount; i++){
				IsotopeCluster ic = peakList.GetIsotopeCluster(i);
				double tt = 0;
				double norm = 0;
				double minT = double.MaxValue;
				double maxT = double.MinValue;
				foreach (int w in ic.Members){
					if (minTimes[w] < minT){
						minT = minTimes[w];
					}
					if (maxTimes[w] > maxT){
						maxT = maxTimes[w];
					}
					tt += intensities[w] * tmpTimes[w];
					norm += intensities[w];
				}
				tt /= norm;
				ic.RetentionTime = tt;
				ic.RetentionTimeMin = minT;
				ic.RetentionTimeMax = maxT;
			}
		}

		public static void PrecalcIntensities(int isotopeClusterCount, Func<int, IsotopeCluster> getIsotopeCluster,
			float[] intensities, Func<HashSet<int>, Dictionary<int, GenericPeak>> getCache){
			const int capacity = 500000;
			Dictionary<int, GenericPeak> cache = new Dictionary<int, GenericPeak>();
			for (int i = 0; i < isotopeClusterCount; i++){
				IsotopeCluster ic = getIsotopeCluster(i);
				int[] members = ic.Members;
				GenericPeak[] ps = new Peak[members.Length];
				for (int j = 0; j < ps.Length; j++){
					if (!cache.ContainsKey(members[j])){
						cache.Clear();
						HashSet<int> inds = GetCacheIndices(getIsotopeCluster, isotopeClusterCount, capacity, i);
						cache = getCache(inds);
					}
					ps[j] = cache[members[j]];
				}
				float[] intens = intensities.SubArray(members);
				GenericPeak p = ps[intens.MaxInd()];
				int scanInd = p.GetMaxIntensityScanIndex();
				int charge = ic.Charge;
				int isoPaternStart = ic.IsotopePatternStart;
				List<double> masses1 = new List<double>();
				List<double> intensities1 = new List<double>();
				List<int> patternInd1 = new List<int>();
				for (int j = 0; j < ps.Length; j++){
					GenericPeak p1 = ps[j];
					double int1 = p1.GetIntensityAtScanIndex(scanInd, out double mz);
					if (!double.IsNaN(int1)){
						masses1.Add((mz - Molecule.massProton) * charge);
						intensities1.Add(int1);
						patternInd1.Add(j + isoPaternStart);
					}
				}
				ic.Intensity = CalcIntensity(masses1[0], intensities1, patternInd1);
			}
		}

		public static double CalcIntensity(double mass, IList<double> intensities, IList<int> patternInd){
			double[][] averagePattern = MolUtil.GetAverageIsotopePattern(mass, true);
			double[] weights = averagePattern[1];
			double sumI = 0;
			double sumW = 0;
			for (int i = 0; i < patternInd.Count; i++){
				int pind = patternInd[i];
				if (pind >= 0 && pind < weights.Length){
					sumI += intensities[i];
					sumW += weights[pind];
				}
			}
			return (ArrayUtils.Sum(weights) / sumW * sumI);
		}

		private static HashSet<int> GetCacheIndices(Func<int, IsotopeCluster> getIsotopeCluster,
			int isotopeClusterCount, int capacity, int istart){
			HashSet<int> indices = new HashSet<int>();
			for (int i = istart; i < isotopeClusterCount; i++){
				IsotopeCluster ic = getIsotopeCluster(i);
				int[] members = ic.Members;
				foreach (int member in members){
					indices.Add(member);
				}
				if (indices.Count >= capacity){
					break;
				}
			}
			return indices;
		}
	}
}