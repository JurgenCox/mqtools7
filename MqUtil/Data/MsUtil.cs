using System.Text;
using MqApi.Util;
using MqUtil.Mol;
using MqUtil.Ms.Enums;
using MqUtil.Ms.Raw;
using MqUtil.Ms.Utils;
using MqUtil.Table;

namespace MqUtil.Data{
	public delegate RawFile RawFileInitializer();

	public static class MsUtil{
        public static string GetPosLayerBasePath(string rawFilePath) {
            string rawFileName = Path.GetFileNameWithoutExtension(rawFilePath);
            string rawFileFolder = Path.GetDirectoryName(rawFilePath);
            string posLayerFolder = Path.Combine(rawFileFolder, rawFileName, "p0");
            return Path.Combine(posLayerFolder, rawFileName);
        }
        public static string GetPosLayerBasePath(string rawFilePath, int sliceIndex, bool hasFaims) {
	        string rawFileName = Path.GetFileNameWithoutExtension(rawFilePath);
	        string rawFileFolder = Path.GetDirectoryName(rawFilePath);
	        string posLayerFolder = Path.Combine(rawFileFolder, rawFileName, "p0");
	        return hasFaims ? Path.Combine(posLayerFolder, "sl" + sliceIndex, rawFileName) : 
		        Path.Combine(posLayerFolder, rawFileName);
        }
        public static ColumnType[] ReadColTypes(string name, string serFolder) {
            BinaryReader reader = FileUtils.GetBinaryReader(Path.Combine(serFolder, name + "x"));
            int n = reader.ReadInt32();
            ColumnType[] result = new ColumnType[n];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (ColumnType)reader.ReadInt32();
            }
            reader.Close();
            return result;
        }

        public static double[,] GetValuesOnGrid(RawLayer rf, IRecalibrationData pl, double minRt, double rtStep,
			int rtCount, double minMass, double massStep, int massCount, int blur, bool recalMasses, bool recalTimes,
			bool readCentroids){
			double[,] values = new double[rtCount, massCount];
			int oldIndex = -1;
			if (pl.MzDependentCalibration == null){
				pl.ReadCalibration();
			}
			double[] mc = pl.ScanDependentCalibration;
			for (int i = 0; i < rtCount; i++){
				double rt = minRt + rtStep * i;
				if (recalTimes){
					rt = pl.GetDecalibratedRt(rt);
				}
				int index = rf.GetIndexFromRt(rt);
				if (index == -1){
					for (int j = 0; j < massCount; j++){
						values[i, j] = 0;
					}
				} else if (index == oldIndex){
					for (int j = 0; j < massCount; j++){
						values[i, j] = values[i - 1, j];
					}
				} else{
					Spectrum s = rf.GetSpectrum(index, readCentroids);
					if (s == null){
						continue;
					}
					if (recalMasses){
						s = Recalibrate(s, rf, pl, index, mc?[index] ?? 0);
					}
					if (s.Count == 0){
						continue;
					}
					CalcMaximumPerBin(minMass, massStep, massCount, i, values, s, readCentroids);
					if (blur > 0){
						int blur1 = blur / 2;
						int blur2 = blur - blur1;
						for (int ind = index - 1; ind >= index - blur1; ind--){
							if (ind >= 0){
								Spectrum s1 = rf.GetSpectrum(index, readCentroids);
								double[,] dummy = new double[1, massCount];
								CalcMaximumPerBin(minMass, massStep, massCount, 0, dummy, s1, readCentroids);
								for (int k = 0; k < massCount; k++){
									values[i, k] = Math.Max(values[i, k], dummy[0, k]);
								}
							}
						}
						for (int ind = index + 1; ind <= index + blur2; ind++){
							if (ind < rf.Count){
								Spectrum s1 = rf.GetSpectrum(index, readCentroids);
								double[,] dummy = new double[1, massCount];
								CalcMaximumPerBin(minMass, massStep, massCount, 0, dummy, s1, readCentroids);
								for (int k = 0; k < massCount; k++){
									values[i, k] = Math.Max(values[i, k], dummy[0, k]);
								}
							}
						}
					}
					oldIndex = index;
				}
			}
			return values;
		}

		public static Spectrum Recalibrate(Spectrum s, RawLayer rawFile, IRecalibrationData peakList, int ms1Ind,
			double timeDependentCal){
			if (peakList.MzDependentCalibration == null){
				peakList.ReadCalibration();
			}
			bool inPpm = peakList.MassRecalibrationInPpm;
			double[] masses = s.CopyMasses();
			float[] intensities = s.CopyIntensities();
			for (int i = 0; i < masses.Length; i++){
				double m1 = masses[i];
				if (inPpm){
					m1 -= m1 * timeDependentCal * 1e-6;
				} else{
					m1 -= timeDependentCal;
				}
				try{
					double mz = m1 * (1 + MolUtil.RelativeCorrectionSpline(m1, peakList.MzDependentCalibration,
						                  rawFile.GetMassRangeIndex(ms1Ind), inPpm));
					masses[i] = mz;
				} catch (Exception){
					masses[i] = m1;
				}
			}
			return new Spectrum(masses, intensities);
		}

		public static double[,] GetMassTraces(RawLayer rf, IRecalibrationData pl, double minRt, double rtStep,
			int rtCount, double[] masses, int blur, VisibleData visibleData, bool recalTimes, bool readCentroids){
			double[,] values = new double[rtCount, masses.Length];
			int oldIndex = -1;
			for (int i = 0; i < rtCount; i++){
				double rt = minRt + rtStep * i;
				if (recalTimes){
					rt = pl.GetDecalibratedRt(rt);
				}
				int index = rf.GetIndexFromRt(rt);
				if (index == -1){
					for (int j = 0; j < masses.Length; j++){
						values[i, j] = 0;
					}
				} else if (index == oldIndex){
					for (int j = 0; j < masses.Length; j++){
						values[i, j] = values[i - 1, j];
					}
				} else{
					Spectrum s = rf.GetSpectrum(index, readCentroids);
					CalcTraceSlice(masses, i, values, s);
					if (blur > 0){
						int blur1 = blur / 2;
						int blur2 = blur - blur1;
						for (int ind = index - 1; ind >= index - blur1; ind--){
							if (ind >= 0){
								Spectrum s1 = rf.GetSpectrum(index, readCentroids);
								double[,] dummy = new double[1, masses.Length];
								CalcTraceSlice(masses, 0, dummy, s1);
								for (int k = 0; k < masses.Length; k++){
									values[i, k] = Math.Max(values[i, k], dummy[0, k]);
								}
							}
						}
						for (int ind = index + 1; ind <= index + blur2; ind++){
							if (ind < rf.Count){
								Spectrum s1 = rf.GetSpectrum(index, readCentroids);
								double[,] dummy = new double[1, masses.Length];
								CalcTraceSlice(masses, 0, dummy, s1);
								for (int k = 0; k < masses.Length; k++){
									values[i, k] = Math.Max(values[i, k], dummy[0, k]);
								}
							}
						}
					}
					oldIndex = index;
				}
			}
			return values;
		}

		public static void CalcTraceSlice(IList<double> masses, int row, double[,] values, Spectrum s){
			for (int i = 0; i < masses.Count; i++){
				values[row, i] = s.InterpolateIntensity(masses[i]);
			}
		}

		public static void CalcMaximumPerBin(double minMass, double massStep, int massCount, int row, double[,] values,
			Spectrum s, bool readCentroids){
			int firstIndex = s.GetCeilIndex(minMass);
			if (firstIndex < 0){
				for (int i = 0; i < massCount; i++){
					values[row, i] = 0;
				}
				return;
			}
			int p = firstIndex;
			for (int index = 0; index < massCount; index++){
				double rightBorder = minMass + (index + 1) * massStep;
				double max = double.MinValue;
				while (p < s.Count && s.GetMass(p) < rightBorder){
					double yVal = s.GetIntensity(p++);
					if (yVal > max){
						max = yVal;
					}
					if (p == s.Count){
						break;
					}
				}
				if (Math.Abs(max - double.MinValue) < double.Epsilon){
					double m = rightBorder - massStep / 2;
					double dm = readCentroids ? 0.01 : double.MaxValue;
					values[row, index] = s.GetIntensityFromMass(m, dm);
				} else{
					values[row, index] = max;
				}
			}
		}

		/// <summary>
		/// Given a path like \some_folders\xyz.RAW, return \some_folders\xyz\xyz
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string CalcBasePath(string path){
			string fileWithoutExt = Path.GetFileNameWithoutExtension(path);
			return Path.Combine(Path.Combine(Path.GetDirectoryName(path), fileWithoutExt), fileWithoutExt);
		}

		/// <summary>
		/// Construct the base path for the positive layer, i.e., if arg is .../xyz.RAW, return .../xyz/p0/xyz.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string CalcBasePathPos(string path, bool hasSlices, int sliceIndex) {
			string fileWithoutExt = Path.GetFileNameWithoutExtension(path);
			if (hasSlices){
				return Path.Combine(Path.Combine(Path.Combine(Path.GetDirectoryName(path), fileWithoutExt), "p0", 
						"sl" + sliceIndex), fileWithoutExt);
			}
			return Path.Combine(Path.Combine(Path.Combine(Path.GetDirectoryName(path), fileWithoutExt), "p0"),
				fileWithoutExt);
		}

		public static string GetModifiedSequence(string sequence, PeptideModificationState pms){
			StringBuilder result = new StringBuilder();
			result.Append("_");
			if (pms.NTermModification != ushort.MaxValue){
				result.Append("(" + Tables.ModificationList[pms.NTermModification].Name + ")");
			}
			for (int i = 0; i < sequence.Length; i++){
				ushort m = pms.GetModificationAt(i);
				if (m != ushort.MaxValue){
					result.Append(sequence[i]);
					result.Append("(" + Tables.ModificationList[m].Name + ")");
				} else{
					result.Append(sequence[i]);
				}
			}
			result.Append("_");
			if (pms.CTermModification != ushort.MaxValue){
				result.Append("(" + Tables.ModificationList[pms.CTermModification].Name + ")");
			}
			return result.ToString();
		}

        private static readonly char[] badChars = {'\\', '/', ':', '*', '?', '"', '<', '>', '|'};

		/// <summary>
		/// Replace all characters that are invalid in file names, specifically 
		/// '\\', '/', ':', '*', '?', '"', less than, greater than, and '|', with an underscore.
		/// </summary>
		/// <param name="dest"></param>
		/// <returns></returns>
		public static string ReplaceBadCharacters(string dest){
			foreach (char badChar in badChars){
				dest = dest.Replace(badChar, '_');
			}
			return dest;
		}

		public static void ReadData(out double[] centerMassArray, out long[] filePosArray, out float[] intensityArray,
			out float[] minTimeArray, out float[] maxTimeArray, out int[] minRtIndsArray, out int[] maxRtIndsArray,
			string filename, bool hasMzBounds, out double[] minMzArray, out double[] maxMzArray){
			BinaryReader reader = FileUtils.GetBinaryReader(filename);
			int n = reader.ReadInt32();
			minMzArray = null;
			maxMzArray = null;
			centerMassArray = new double[n];
			filePosArray = new long[n];
			intensityArray = new float[n];
			minTimeArray = new float[n];
			maxTimeArray = new float[n];
			minRtIndsArray = new int[n];
			maxRtIndsArray = new int[n];
			if (hasMzBounds){
				minMzArray = new double[n];
				maxMzArray = new double[n];
			}
			for (int i = 0; i < n; i++){
				centerMassArray[i] = reader.ReadDouble();
				filePosArray[i] = reader.ReadInt64();
				reader.ReadInt32();
				intensityArray[i] = reader.ReadSingle();
				minTimeArray[i] = (float)reader.ReadDouble();
				maxTimeArray[i] = (float)reader.ReadDouble();
				minRtIndsArray[i] = reader.ReadInt32();
				maxRtIndsArray[i] = reader.ReadInt32();
				reader.ReadDouble();
				if (hasMzBounds){
					minMzArray[i] = reader.ReadDouble();
					maxMzArray[i] = reader.ReadDouble();
				}
			}
			reader.Close();
		}

		public static void ReadDataTims(out double[] centerMassArray, out long[] filePosArray,
			out float[] intensityArray, out float[] minTimeArray, out float[] maxTimeArray, out int[] minSliceIndArray,
			out int[] maxSliceIndArray, string filename, out int[] minFrameArray, out int[] maxFrameArray){
			BinaryReader reader = FileUtils.GetBinaryReader(filename);
			int n = reader.ReadInt32();
			centerMassArray = new double[n];
			filePosArray = new long[n];
			intensityArray = new float[n];
			minFrameArray = new int[n];
			maxFrameArray = new int[n];
			minTimeArray = new float[n];
			maxTimeArray = new float[n];
			minSliceIndArray = new int[n];
			maxSliceIndArray = new int[n];
			for (int i = 0; i < n; i++){
				centerMassArray[i] = reader.ReadDouble();
				intensityArray[i] = (float)reader.ReadDouble();
				minFrameArray[i] = reader.ReadInt32();
				maxFrameArray[i] = reader.ReadInt32();
				minTimeArray[i] = reader.ReadSingle();
				maxTimeArray[i] = reader.ReadSingle();
				minSliceIndArray[i] = reader.ReadInt32();
				maxSliceIndArray[i] = reader.ReadInt32();
				filePosArray[i] = reader.ReadInt64();
			}
			reader.Close();
		}

		/// <summary>
		/// The file is deleted after it is read.
		/// </summary>
		public static void ReadTmpData(out int[] tmp1FilePosArray, out double[] tmpTimeArray, string filename,
			bool hasMassBounds){
			BinaryReader reader = FileUtils.GetBinaryReader(filename);
			int n = reader.ReadInt32();
			tmp1FilePosArray = new int[n];
			tmpTimeArray = new double[n];
			for (int i = 0; i < n; i++){
				reader.ReadDouble();
				reader.ReadInt64();
				tmp1FilePosArray[i] = reader.ReadInt32();
				reader.ReadSingle();
				reader.ReadDouble();
				reader.ReadDouble();
				tmpTimeArray[i] = reader.ReadDouble();
				if (hasMassBounds){
					reader.ReadDouble();
					reader.ReadDouble();
				}
			}
			reader.Close();
			File.Delete(filename);
		}

		/// <summary>
		/// Given a basePath like \some_folders\xyz\xyz, set the third arg to \some_folders\xyz\p0 (or n0),
		/// and return \some_folders\xyz\p0\xyz
		/// </summary>
		/// <param name="basePath"></param>
		/// <param name="positive"></param>
		/// <param name="layerFolder"></param>
		/// <returns></returns>
		public static string CalcLayerPath(string basePath, bool positive, out string layerFolder){
			string firstFolders = Path.GetDirectoryName(basePath);
			string lastFolder = Path.GetFileName(basePath);
			layerFolder = Path.Combine(firstFolders, (positive ? "p0" : "n0"));
			return Path.Combine(layerFolder, lastFolder);
		}
		private static int closing_bracket(string name, char symbol, int pos) {
			int end, par;
			char close = (symbol == '(' ? ')' : ']');
			for (end = pos + 1, par = 1; end<name.Length; end++) {
				char s = name[end];
				if (s == close) {
					par--;
					if (par == 0) break;
				} else if (s == symbol) par++;
			}
			return end;
		}

		public static double predict_irt(string name, double[] InSilicoRT) {
			int i, pos, l = name.Length;
			double iRT = InSilicoRT[0], scale = 1.0 / (double)l;
			for (i = pos = 0; i<name.Length; i++) {
				char symbol = name[i];
				if (symbol< 'A' || symbol> 'Z') {
					if (symbol != '(' && symbol != '[') continue;
					i++;
					int end = closing_bracket(name, symbol, i);
					if (end == name.Length) throw new Exception("incorrect peptide name format: " + name);
					//iRT += InSilicoRT[mod_rt_index(name.Substring(i, end - i))];
					i = end;
					continue;
				}
				iRT += InSilicoRT[aa_rt_nterm(symbol, pos)];
				iRT += InSilicoRT[aa_pointsterm(symbol, l - 1 - pos)];
				iRT += InSilicoRT[aa_rt_nterm_scaled(symbol, pos)] * scale;
				iRT += InSilicoRT[aa_pointsterm_scaled(symbol, l - 1 - pos)] * scale;
				pos++;
			}
			return iRT;
		}

		static double[] AA = new double[256];
		static int[] AA_index = new int[256];
		static string AAs = "GAVLIFMPWSCTYHKRQEND";
		static string MutateAAto = "LLLVVLLLLTSSSSLLNDQE";

		static MsUtil () {
			for (int i = 0; i < 256; i++){
				AA_index[i] = 0; 
				AA[i] = 0.0;
			}
			AA['G'] = 57.021464;
			AA['A'] = 71.037114;
			AA['V'] = 99.068414;
			AA['L'] = 113.084064;
			AA['I'] = 113.084064;
			AA['F'] = 147.068414;
			AA['M'] = 131.040485;
			AA['P'] = 97.052764;
			AA['W'] = 186.079313;
			AA['S'] = 87.032028;
			AA['C'] = 103.009185;
			AA['T'] = 101.047679;
			AA['Y'] = 163.06332;
			AA['H'] = 137.058912;
			AA['K'] = 128.094963;
			AA['R'] = 156.101111;
			AA['Q'] = 128.058578;
			AA['E'] = 129.042593;
			AA['N'] = 114.042927;
			AA['D'] = 115.026943;
			AA['U'] = 150.95363;
			AA['X'] = 196.995499;
			for (int i = 0; i < 20; i++){
				AA_index[AAs[i]] = i;
			}
			AA_index['U'] = AA_index['C'];
			AA_index['X'] = AA_index['M'];
		}

		static int RTTermD = 1, RTTermDScaled = 5;
		private static int aa_rt_nterm(char aa, int shift){
			return 1 + Math.Min(shift, RTTermD - 1) * 20 + AA_index[aa];
		}
		private static int aa_pointsterm(char aa, int shift){
			return aa_rt_nterm(AAs[19], RTTermD - 1) + 1 + Math.Min(shift, RTTermD - 1) * 20 + AA_index[aa];
		}
		private static int aa_rt_nterm_scaled(char aa, int shift){
			return aa_pointsterm(AAs[19], RTTermD - 1) + 1 + Math.Min(shift, RTTermDScaled - 1) * 20 + AA_index[aa];
		}
		private static int aa_pointsterm_scaled(char aa, int shift){
			return aa_rt_nterm_scaled(AAs[19], RTTermDScaled - 1) + 1 + Math.Min(shift, RTTermDScaled - 1) * 20 + AA_index[aa];
		}
		//private static int mod_rt_index(int mod){ return aa_pointsterm_scaled(AAs[19], RTTermDScaled - 1) + 1 + unimod_index_number(mod); }
		//private static int mod_rt_index(string mod){ return mod_rt_index(unimod_index(mod)); }
		//private static int in_silico_rt_size() { return UniModIndices.size() ? (mod_rt_index(UniModIndices[UniModIndices.size() - 1]) + 1) : (aa_pointsterm_scaled(AAs[19], RTTermDScaled - 1) + 1); }
	}
}