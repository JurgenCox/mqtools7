using MqApi.Util;
using MqUtil.Base;
using MqUtil.Ms.Enums;
using MqUtil.Ms.Instrument;
using MqUtil.Ms.Raw;
using MqUtil.Ms.Utils;
using MqUtil.Num;
using MqUtil.Util;

namespace MqUtil.Data{
	public static class PeakDetection{
        public const int halfWidthCentroidMs = 2;
		public static WritablePeak[] Detect(double matchTol, bool matchInPpm, BasicGroupParams param, RawLayer rawFile,
			string peaksPath, bool skipBeginning, bool writeTmpFiles, bool hasMassBounds, bool isDia, bool isMsms,
			bool processPeaks, Responder responder, bool calcNeighbors, int maxCharge, double isoMatchTol, 
			bool isoMatchTolInPpm, bool calculateResolution, double halfWidth, double valleyFactor,
			bool advancedPeakSplitting, bool slicePeaks, MsInstrument msInstrument, int missingScans,
			IntensityDetermination intensDet, int minPeakLen)
		{
			bool write = !string.IsNullOrEmpty(peaksPath);
			List<WritablePeak> peaks = new List<WritablePeak>();
			bool useCentroids = param.UseMs1Centroids;
			if (calculateResolution && !isMsms){
				useCentroids = false;
			}
			double intensThreshold = isMsms ? param.IntensityThresholdMs2 : isDia? param.IntensityThresholdMs1Dia : 
				param.IntensityThresholdMs1Dda;
			const byte minCharge = 1;
			if (write){
				if (File.Exists(peaksPath)){
					File.Delete(peaksPath);
				}
				if (File.Exists(peaksPath + "i")){
					File.Delete(peaksPath + "i");
				}
			}
			BinaryWriter writer = null;
			BinaryWriter indexWriter = null;
			BinaryWriter tmpWriter1 = null;
			BinaryWriter tmpWriter2 = null;
			if (write){
				writer = InitializeWriter(peaksPath);
				indexWriter = InitializeWriter(peaksPath + "i");
				if (writeTmpFiles){
					if (File.Exists(peaksPath + "_t1")){
						File.Delete(peaksPath + "_t1");
					}
					if (File.Exists(peaksPath + "_t2")){
						File.Delete(peaksPath + "_t2");
					}
					tmpWriter1 = InitializeWriter(peaksPath + "_t1");
					tmpWriter2 = InitializeWriter(peaksPath + "_t2");
				}
			}
			int peakCount = 0;
			if (write){
				indexWriter.Write(peakCount);
			}
			int oldMinInd = -1;
			int oldMaxInd = -1;
			List<Ms1CentroidList> history = new List<Ms1CentroidList>();
			int maxMissingScans = (rawFile.MassRangeCount - 1) * (missingScans + 1) + missingScans;
			int nscans = rawFile.Count;
			SignalType signalType = rawFile.HasProfile(0) ? SignalType.Profile : SignalType.Centroid;
			if (useCentroids) {
				signalType = SignalType.Centroid;
			}
			bool calcResolution = calculateResolution && !isMsms && signalType == SignalType.Profile;
			int regionStartIndex = 0;
			List<WritablePeak> toBeWritten = new List<WritablePeak>();
			for (int scanInd = 0; scanInd < nscans; scanInd++){
				if (responder != null && scanInd % 100 == 99) {
					responder?.Comment("Scan " + (scanInd + 1) + "/" + (nscans) + " (" +
					                  NumUtils.GetPercentageString((scanInd + 1) / (double)nscans) + ")");
				}
				int minInd = Math.Max(regionStartIndex, scanInd - maxMissingScans - 1);
				int maxInd = Math.Min(nscans - 1, scanInd + maxMissingScans + 1);
				if (scanInd == 0 || history.Count == 0){
					for (int j = minInd; j <= maxInd; j++){
						Spectrum s = rawFile.GetSpectrum(j, useCentroids);
						Ms1CentroidList cl = DetectPeaks(s, signalType, useCentroids, intensThreshold, halfWidth, calcResolution);
						if (calcNeighbors){
							cl.CalcNeighbors(isoMatchTol,isoMatchTolInPpm,minCharge, maxCharge);
						}
						history.Add(cl);
						s.Dispose();
					}
				} else{
					for (int j = oldMinInd; j < minInd; j++){
						if (history.Count > 0){
							history[0].Dispose();
							history.RemoveAt(0);
						}
					}
					for (int j = oldMaxInd + 1; j <= maxInd; j++){
						Spectrum s = rawFile.GetSpectrum(j, useCentroids);
						Ms1CentroidList cl = DetectPeaks(s, signalType, useCentroids, intensThreshold, halfWidth, calcResolution);
						if (calcNeighbors) {
							cl.CalcNeighbors(isoMatchTol, isoMatchTolInPpm, minCharge, maxCharge);
						}
						history.Add(cl);
						s.Dispose();
					}
				}
				int indx = scanInd - minInd;
				if (indx < history.Count){
					Ms1CentroidList p = history[indx];
					bool[] v = new bool[p.Count];
					//This is a pre-filter which looks for possible matches into past AND future.
					for (int j = 0; j < p.Count; j++){
						v[j] = Matches(j, p, scanInd, minInd, history, matchTol, matchInPpm, missingScans, maxInd);
					}
					List<int> valids = new List<int>();
					for (int k = 0; k < v.Length; k++){
						if (v[k]){
							valids.Add(k);
						}
					}
					Ms1CentroidList reduced = minPeakLen > 1 ? p.Extract(valids.ToArray()) : p;
					history[scanInd - minInd] = reduced;
					byte range = rawFile.GetMassRangeIndex(scanInd);
					for (int j = 0; j < reduced.Count; j++){
						double centerMass = reduced.GetMzCentroid(j);
                        double minMass = reduced.MzMin[j];
                        double maxMass = reduced.MzMax[j];
                        bool match = false;
						int ntries = 0;
						for (int k = scanInd - 1; k >= minInd; k--){
							Ms1CentroidList centroidList = history[k - minInd];
							int ind = centroidList.GetClosestIndex(centerMass);
							double m = centroidList.GetMzCentroid(ind);
							if (ind != -1 && MassMatch(centerMass, minMass, maxMass, m, matchTol, matchInPpm)){
								GrowablePeak peak = centroidList.Peaks[ind];
								peak.Add(scanInd, j, reduced, range, ntries == missingScans);
								match = true;
								break;
							}
							ntries++;
							if (ntries > missingScans){
								break;
							}
						}
						if (!match){
							GrowablePeak peak = new GrowablePeak(calcResolution);
							peak.Add(scanInd, j, reduced, range, false);
						}
					}
				}
				if (history.Count > 0){
					Ms1CentroidList last = history[0];
					int nextMinInd = Math.Max(regionStartIndex, scanInd - maxMissingScans);
					for (int j = 0; j < last.Count; j++){
						GrowablePeak peak = last.Peaks[j];
						if (peak.IsDisposed()){
							continue;
						}
						//If this peak cannot be reached in the next iteration, process and dispose it.
						if (peak.LastScanIndex < nextMinInd){
							Process(peak, minPeakLen, rawFile, valleyFactor, advancedPeakSplitting, 
								slicePeaks, ref peakCount, msInstrument, toBeWritten,
								skipBeginning, intensDet, processPeaks);
							peak.Dispose();
						}
					}
				}
				oldMinInd = minInd;
				oldMaxInd = maxInd;
				if (scanInd % 100 == 0){
					Write(toBeWritten, writer, indexWriter, tmpWriter1, tmpWriter2, hasMassBounds, write, peaks);
				}
			}
			if (oldMinInd > -1){
				for (int i = nscans - maxMissingScans - 1; i < nscans; i++){
					int ind1 = i - oldMinInd;
					if (ind1 < 0 || ind1 >= history.Count){
						continue;
					}
					Ms1CentroidList last = history[i - oldMinInd];
					for (int j = 0; j < last.Count; j++){
						GrowablePeak peak = last.Peaks[j];
						if (peak.IsDisposed()){
							continue;
						}
						if (peak.LastScanIndex == i){
							Process(peak, minPeakLen, rawFile, valleyFactor, slicePeaks, 
								advancedPeakSplitting, ref peakCount, msInstrument, toBeWritten,
								skipBeginning, intensDet, processPeaks);
							peak.Dispose();
						}
					}
				}
			}
			Write(toBeWritten, writer, indexWriter, tmpWriter1, tmpWriter2, hasMassBounds, write, peaks);
			if (write){
				indexWriter.BaseStream.Seek(0, SeekOrigin.Begin);
				indexWriter.Write(peakCount);
				writer.Close();
				indexWriter.Close();
				if (tmpWriter1 != null){
					tmpWriter1.Close();
					tmpWriter2.Close();
				}
			}
			return peaks.ToArray();
		}
		/// <summary>
		/// Tries to create a BinaryWriter for the file named in the arg, 
		/// in create mode, with write access. Throws an error if not successful.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static BinaryWriter InitializeWriter(string path){
			BinaryWriter writer;
			try{
				writer = FileUtils.GetBinaryWriter(path);
			} catch (Exception){
				throw new Exception("Cannot open file " + path + ". It may be used by another program.");
			}
			return writer;
		}
		/// <summary>
		/// Call WritablePeak.WritePeak for each object in the list given as the 1st arg.
		/// </summary>
		public static void Write(ICollection<WritablePeak> toBeWritten, BinaryWriter writer, BinaryWriter indexWriter,
			BinaryWriter tmpWriter1, BinaryWriter tmpWriter2, bool hasMassBounds, bool write, List<WritablePeak> peaks){
			foreach (WritablePeak peak in toBeWritten){
				if (write){
					peak.WritePeak(writer, indexWriter, tmpWriter1, tmpWriter2, hasMassBounds);
				} else{
					peaks.Add(peak);
				}
			}
			toBeWritten.Clear();
			if (write){
				writer.Flush();
				indexWriter.Flush();
				if (tmpWriter1 != null){
					tmpWriter1.Flush();
					tmpWriter2.Flush();
				}
			}
		}
		public static bool Matches(int j, Ms1CentroidList centroidList, int i, int minInd,
			IList<Ms1CentroidList> cache, double matchTol, bool matchInPpm, int missingScans, int maxInd){
			double centerMass = centroidList.GetMzCentroid(j);
            double minMass = centroidList.MzMin[j];
            double maxMass = centroidList.MzMax[j];
			bool match = false;
			int ntries = 0;
			for (int k = i - 1; k >= minInd; k--){
				Ms1CentroidList q = cache[k - minInd];
				int ind = q.GetClosestIndex(centerMass);
				double m = q.GetMzCentroid(ind);
				if (ind != -1 && MassMatch(centerMass, minMass, maxMass, m, matchTol, matchInPpm)){
					match = true;
					break;
				}
				ntries++;
				if (ntries > missingScans){
					break;
				}
			}
			if (!match){
				ntries = 0;
				for (int k = i + 1; k <= maxInd; k++){
					int ix = k - minInd;
					if (ix >= cache.Count){
						break;
					}
					Ms1CentroidList q = cache[ix];
					int ind = q.GetClosestIndex(centerMass);
					double m = q.GetMzCentroid(ind);
					if (ind != -1 && MassMatch(centerMass, minMass, maxMass, m, matchTol, matchInPpm)){
						match = true;
						break;
					}
					ntries++;
					if (ntries > missingScans){
						break;
					}
				}
			}
			return match;
		}
        public static bool MassMatch(double centerMass, double minMass, double maxMass, double newMass,
            double matchTol, bool matchInPpm) {
            if (matchInPpm) {
                double match = 2 * Math.Abs(centerMass - newMass) / (centerMass + newMass) * 1e+6;
                if (match > matchTol) {
                    return false;
                }
            }
            else {
                double match = Math.Abs(centerMass - newMass);
                if (match > matchTol) {
                    return false;
                }
            }
            if (newMass < minMass) {
                return false;
            }
            return newMass <= maxMass;
        }
        /// <summary>
        /// Process a peak by removing double peaks, decomposing gaps, smoothing, and decomposing valleys.
        /// Each peak kept is added to a list of WritablePeak, and a count is incremented.
        /// </summary>
        /// <param name="minPeakLen">The minimum number of retention times (scans) required to keep a peak.</param>
        public static void Process(GrowablePeak gpeak, int minPeakLen, RawLayer rawFile, double valleyFactor,
			bool advancedPeakSplitting, bool split, ref int peakCount, MsInstrument msInstrument,
			ICollection<WritablePeak> toBeWritten, bool skipBeginning, IntensityDetermination intensDet,
			bool processPeaks){
			Peak peak = gpeak.ToPeak(intensDet);
			if (peak.Count >= minPeakLen){
				if (!processPeaks){
					peakCount++;
					peak.Smooth(msInstrument, intensDet);
					toBeWritten.Add(new WritablePeak(peak, rawFile));
					return;
				}
				peak.RemoveDoublePeaks(intensDet);
				if (peak.Count >= minPeakLen){
					Peak[] peaks1 = peak.DecomposeGaps(0.01, intensDet);
					foreach (Peak peak1 in peaks1){
						if (peak1.Count >= minPeakLen){
							peak1.Smooth(msInstrument, intensDet);
							Peak[] peaks =
								peak1.DecomposeValleys(valleyFactor, advancedPeakSplitting, split, intensDet);
							foreach (Peak p in peaks){
								if (p.Count >= minPeakLen){
									if (p.Count == minPeakLen && p.HasGap){
										p.Dispose();
										continue;
									}
									if (p.FirstScanIndex == 0 && skipBeginning){
										p.Dispose();
										continue;
									}
									if (3 * p.GapCount > p.Count){
										p.Dispose();
										continue;
									}
									peakCount++;
									toBeWritten.Add(new WritablePeak(p, rawFile));
								} else{
									p.Dispose();
								}
							}
						}
					}
				}
			}
		}
		/// <param name="centroidApproach"></param>
		/// <param name="spectrum">The mass vs. intensity function which is searched for peaks.</param>
		/// <param name="signalType">Either Centroid or Profile. Used to determine which detection method to call.</param>
		/// <param name="useCentroids">Used to determine which detection method to call.</param>
		public static Ms1CentroidList DetectPeaks(Spectrum spectrum, SignalType signalType, bool useCentroids,
			double intensThreshold, double halfWidth, bool calcResolution) {
			if (spectrum.Count == 0){
				spectrum = new Spectrum(new double[0], new float[0]);
			}
			if (spectrum.Count > 0){
				double[] masses = spectrum.Masses;
				float[] intensities = spectrum.Intensities;
				spectrum = new Spectrum(masses, intensities);
			} else{
				spectrum = new Spectrum(new double[0], new float[0]);
			}
			if (signalType == SignalType.Profile && !useCentroids){
				return DetectPeaksSimple(spectrum, intensThreshold, calcResolution);
			}
			return DetectPeaksCentroid(spectrum, halfWidth, intensThreshold);
		}
		/// <summary>
		/// Return an Ms1CentroidList based on a Spectrum, for the case that signalType is Centroid or useCentroids is true.
		/// (An Ms1CentroidList contains information on the positions, widths, shapes, and intensities of peaks.)
		/// </summary>
		/// <returns></returns>
		private static Ms1CentroidList DetectPeaksCentroid(Spectrum s, double centroidHalfWidth, double intensThreshold){
			double[] mzCentroid = new double[s.Count];
			double[] mzMin = new double[s.Count];
			double[] mzMax = new double[s.Count];
			float[] intensity = new float[s.Count];
			int indPosIntens = 0; // index of points of spectrum with positive intensity
			double c1 = centroidHalfWidth * 1e-6;
			for (int ind = 0; ind < s.Count; ind++){
				float thisIntens = s.GetIntensity(ind);
				if (thisIntens > intensThreshold){
					intensity[indPosIntens] = thisIntens;
					double thisMz = s.GetMass(ind);
					mzCentroid[indPosIntens] = thisMz;
					double dm = c1 * thisMz;
					mzMin[indPosIntens] = thisMz - dm;
					mzMax[indPosIntens] = thisMz + dm;
					indPosIntens++;
				}
			}
			Array.Resize(ref mzCentroid, indPosIntens);
			Array.Resize(ref mzMin, indPosIntens);
			Array.Resize(ref mzMax, indPosIntens);
			Array.Resize(ref intensity, indPosIntens);
			return new Ms1CentroidList(mzCentroid, mzMin, mzMax, intensity, null);
		}
		/// <summary>
		/// From a spectrum of intensity as a function of mass, make a list of the peaks (with information 
		/// about their positions, widths, shapes, and intensities).
		/// Return an Ms1CentroidList based on a Spectrum, for the case that signalType is Profile, 
		/// and useCentroids is false, and centroidApproach is Normal.
		/// (An Ms1CentroidList contains information on the positions, widths, shapes, and intensities of peaks.)
		/// </summary>
		/// <param name="s"></param>
		/// <param name="centroidPosition">Whether to calculate the center of mass using the gaussian or the weighted mean method.</param>
		/// <param name="intensThreshold"></param>
		public static unsafe Ms1CentroidList DetectPeaksSimple(Spectrum s, double intensThreshold, bool calcResolution){
			if (s.Count <= 4){
				return new Ms1CentroidList(new double[0], new double[0], new double[0], new float[0], new float[0]);
			}
			double[] sMasses = s.Masses;
			float[] sIntensities = s.Intensities;
			int sn = sMasses.Length;
			double[] mzCentroid = new double[sn];
			double[] mzMin = new double[sn];
			double[] mzMax = new double[sn];
			float[] intensity = new float[sn];
			float[] resolution = null;
			if (calcResolution){
				resolution = new float[sn];
			}
			int peakCount = 0;
			fixed (double* sMasses1 = sMasses){
				fixed (float* sIntensities1 = sIntensities){
					double m2 = sIntensities1[0];
					double m1 = sIntensities1[1];
					double x = sIntensities1[2];
					double p1 = sIntensities1[3];
					for (int i = 2; i < sn - 2; i++){
						if (x > 0 && ((x > m1 && x > p1) || (x > m2 && x == m1 && x > p1))){
							int minInd = i; // valley to the left
							while (minInd > 0 && sIntensities1[minInd - 1] != 0 &&
								sIntensities1[minInd - 1] < sIntensities1[minInd]){
								minInd--;
							}
							int maxInd = i; // valley to the right
							while (maxInd < sn - 1 && sIntensities1[maxInd + 1] != 0 &&
								sIntensities1[maxInd + 1] < sIntensities1[maxInd]){
								maxInd++;
							}
							if (maxInd - minInd > 2){
								// at least 4 points between minInd and maxInd, incl.
								// trim boundaries
								if (maxInd > i && minInd < i){
									// at least one point in flank on each side
									maxInd--;
									minInd++;
									// decreases dependency on boundary points that may be sensitive to external influences
								} else if (maxInd > i){
									// spectrum drops to zero on left, or drops and immediately rises again
									maxInd = i + 1; // pathological asymmetry, just use 2 points near peak
								} else if (minInd < i){
									// spectrum drops to zero on right, or drops and immediately rises again
									minInd = i - 1; // pathological asymmetry, just use 2 points near peak
								}
								// At this point there are at least 2 points between minInd and maxInd, incl.
								// Find peakIntensity and peakCenterMass
								//
								float peakIntensity = 0;
								for (int j = minInd; j <= maxInd; j++) {
									peakIntensity += sIntensities1[j];
								}
								intensity[peakCount] = peakIntensity;
								double mzpc = CalcCenterMass(minInd, i, maxInd, sMasses1, sIntensities1);
								mzCentroid[peakCount] = mzpc;
								// If either mass or intensity is not defined, reject peak.
								// Find peakMinMass and peakMaxMass (between grid points)
								if (!double.IsNaN(mzpc) && !double.IsNaN(intensity[peakCount])){
									double minMass;
									if (minInd > 0){
										minMass = 0.5 * (sMasses1[minInd] + sMasses1[minInd - 1]);
									} else{
										minMass = 1.5 * sMasses1[0] - 0.5 * sMasses1[1];
									}
									double maxMass;
									if (maxInd < sn - 1){
										maxMass = 0.5 * (sMasses1[maxInd] + sMasses1[maxInd + 1]);
									} else{
										maxMass = 1.5 * sMasses1[maxInd] - 0.5 * sMasses1[maxInd - 1];
									}
									// If fitted peak is outside the mass range, then something went wrong with the fit.
									if (mzpc < minMass || mzpc > maxMass){
										mzCentroid[peakCount] = 0.5 * (minMass + maxMass);
										intensity[peakCount] = 0;
									}
									mzMin[peakCount] = minMass;
									mzMax[peakCount] = maxMass;
									if (calcResolution){
										int centerInd = CalcCenterInd(minInd, maxInd, s);
										resolution[peakCount] = CalcFwhm(minInd, centerInd, maxInd, s);
									}
									if (intensity[peakCount] > intensThreshold){
										peakCount++;
									}
								}
							}
						}
						m2 = m1;
						m1 = x;
						x = p1;
						p1 = sIntensities1[i + 2];
					}
				}
			}
			Array.Resize(ref mzCentroid, peakCount);
			Array.Resize(ref mzMin, peakCount);
			Array.Resize(ref mzMax, peakCount);
			Array.Resize(ref intensity, peakCount);
			if (calcResolution){
				Array.Resize(ref resolution, peakCount);
			}
			return new Ms1CentroidList(mzCentroid, mzMin, mzMax, intensity, resolution);
		}
		private static int CalcCenterInd(int minInd, int maxInd, Spectrum spectrum){
			int ind = minInd;
			float maxIntens = spectrum.Intensities[minInd];
			for (int i = minInd + 1; i <= maxInd; i++){
				float intens = spectrum.Intensities[i];
				if (intens > maxIntens){
					ind = i;
				}
			}
			return ind;
		}
		private static float CalcFwhm(int minInd, int centerInd, int maxInd, Spectrum s) {
			if (minInd == maxInd) { // single spike
				return float.NaN;
			}
			if (minInd == centerInd) { // precipitous drop on left side
				return float.NaN;
			}
			if (maxInd == centerInd) { // precipitous drop on right side
				return float.NaN;
			}
			float minInt = s.GetIntensity(minInd);
			float centerInt = s.GetIntensity(centerInd);
			float maxInt = s.GetIntensity(maxInd);
			if (minInt >= centerInt / 2) { // valley on left not deep enough to get find half max point by interpolation
				return float.NaN;
			}
			if (maxInt >= centerInt / 2) { // valley on right not deep enough to get find half max point by interpolation
				return float.NaN;
			}
			int imin = centerInd; // first point on left with less than half the max intensity
			while (s.GetIntensity(imin) > centerInt / 2) {
				imin--;
			}
			double mleft = Interpol(s.GetMass(imin), s.GetIntensity(imin) / centerInt, s.GetMass(imin + 1),
				s.GetIntensity(imin + 1) / centerInt);
			int imax = centerInd; // first point on right with less than half the max intensity
			while (s.GetIntensity(imax) > centerInt / 2) {
				imax++;
			}
			double mright = Interpol(s.GetMass(imax - 1), s.GetIntensity(imax - 1) / centerInt, s.GetMass(imax),
				s.GetIntensity(imax) / centerInt);
			return (float)(mright - mleft);
		}

		private static double Interpol(double m1, double i1, double m2, double i2) {
			return m2 - (i2 - 0.5) / (i2 - i1) * (m2 - m1);
		}
		/// <summary>
		/// Calculate centroid and intensity of a peak in a spectrum 
		/// identified by the indices of the left edge, the center, and the right edge.
		/// The last argument determines the algorithm, usually a gaussian fit.
		/// </summary>
		/// <param name="minInd">smallest index of range to use for peak</param>
		/// <param name="centerInd">index of mass with greatest intensity in this range</param>
		/// <param name="maxInd">largest index of range to use for peak</param>
		/// <param name="peakIntensity">integrated intensity of the peak (output), calculated 
		/// before starting to prune points for the determination of the centroid</param>
		/// <param name="peakCenterMass">output</param>
		private static unsafe double CalcCenterMass(int minInd, int centerInd, int maxInd, double* sMasses, 
			float* sIntensities){
			if (minInd == maxInd){
				// single point in peak (In current code, this will not happen.)
				return sMasses[maxInd];
			}
			if (minInd == centerInd || maxInd == centerInd){
				// pathological asymmetry, just use 2 points near peak
				double m1, m2, i1, i2;
				if (minInd == centerInd){
					m1 = sMasses[centerInd];
					m2 = sMasses[centerInd + 1];
					i1 = sIntensities[centerInd];
					i2 = sIntensities[centerInd + 1];
				} else{
					m1 = sMasses[centerInd - 1];
					m2 = sMasses[centerInd];
					i1 = sIntensities[centerInd - 1];
					i2 = sIntensities[centerInd];
				}
				return (m1 * i1 + m2 * i2) / (i1 + i2);
			}
			// At this point, indMin to indMax span at least 3 points, incl.
			// number of points to use left  of max (1 or 2)
			int istart = Math.Max(centerInd - halfWidthCentroidMs, minInd);
			// number of points to use right of max (1 or 2)
			int iend = Math.Min(centerInd + halfWidthCentroidMs, maxInd);
			int ilen = iend - istart + 1;
			double covar00 = 0;
			double covar10 = 0;
			double covar20 = 0;
			double covar11 = 0;
			double covar21 = 0;
			double covar22 = 0;
			double beta0 = 0;
			double beta1 = 0;
			double beta2 = 0;
			double dm = sMasses[centerInd];
			for (int i = 0; i < ilen; i++){
				double afunc1 = sMasses[istart + i] - dm;
				double afunc2 = afunc1 * afunc1;
				double ym = Math.Log(sIntensities[istart + i]);
				covar00 += 1;
				covar10 += afunc1;
				covar11 += afunc2;
				covar20 += afunc2;
				covar21 += afunc2 * afunc1;
				covar22 += afunc2 * afunc2;
				beta0 += ym;
				beta1 += ym * afunc1;
				beta2 += ym * afunc2;
			}
			double a12 = covar20 * covar21 - covar22 * covar10;
			double a13 = covar10 * covar21 - covar20 * covar11;
			double a22 = covar22 * covar00 - covar20 * covar20;
			double a23 = covar10 * covar20 - covar00 * covar21;
			double a33 = covar00 * covar11 - covar10 * covar10;
			double x2 = a12 * beta0 + a22 * beta1 + a23 * beta2;
			double x3 = a13 * beta0 + a23 * beta1 + a33 * beta2;
			return dm - x2 / x3 * 0.5;
		}
	}
}