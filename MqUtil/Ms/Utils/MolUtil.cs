using System.Text;
using MqApi.Num;
using MqUtil.Mol;
using MqUtil.Ms.Annot;
using MqUtil.Num;
namespace MqUtil.Ms.Utils{
	public static class MolUtil{
		private const double massBinSize = 50;
		private static readonly object locker = new object();

		private static readonly Dictionary<(int, int), double> massDiffCache = new Dictionary<(int, int), double>();
		public static NeutralLossLib waterLossLib = new NeutralLossLib("H2O", Molecule.massWater);
		public static NeutralLossLib ammoniaLossLib = new NeutralLossLib("NH3", Molecule.massAmmonia);
		private const double averagineCompositionC = 4.9384;
		private const double averagineCompositionH = 7.7583;
		private const double averagineCompositionN = 1.3577;
		private const double averagineCompositionO = 1.4773;
		private const double averagineCompositionS = 0.0417;

		private static readonly double averagineTotal = averagineCompositionC * Molecule.massC +
		                                                averagineCompositionH * Molecule.massH +
		                                                averagineCompositionN * Molecule.massN +
		                                                averagineCompositionO * Molecule.massO +
		                                                averagineCompositionS * Molecule.massS;

		//private static readonly double averagineNomMassFactor = 0.999494;
		//private const double averagineNomMassFactor = 0.999555;
		public static double GetMassDeficit(double m){
			return m * 0.99954 - 0.04 - Math.Round(m * 0.99954 - 0.04);
		}

		/// <summary>
		/// Whether the given mass is reasonably close to the mass expected for peptides.
		/// Mass deficit is only calculated if the given mass is lighter than 2000Da,
		/// otherwise absolute value of a calculated mass deficit must be smaller than 0.25
		/// </summary>
		/// <param name="mass"></param>
		/// <returns></returns>
		public static bool IsSuitableMass(double mass) => mass > 2000.000 || Math.Abs(GetMassDeficit(mass)) < 0.25;

		private static readonly Dictionary<int, double[][]> isotopePatternListNoSulfur =
			new Dictionary<int, double[][]>();

		private static readonly Dictionary<int, double[][]> isotopePatternListSulfur =
			new Dictionary<int, double[][]>();

		public static readonly double isotopePatternDiffProt = GetAverageDifferenceToMonoisotope(10000, 10) / 10;
		public static readonly double isotopePatternDiff = GetAverageDifferenceToMonoisotope(1500, 1);

		public static double[] GetPeptideMasses(double maxMass, int maxLen, string[] fixedModifications,
			string[] variableModifications){
			double[] mx = GetBaseMasses(fixedModifications, variableModifications);
			int maxn = Math.Min(maxLen, (int) Math.Floor(maxMass / mx[0]));
			double[] result = {Molecule.massWater};
			for (int n = 1; n <= maxn; n++){
				List<double> newMasses = new List<double>();
				foreach (double t in result){
					foreach (double m1 in mx){
						double nm = t + m1;
						if (nm <= maxMass){
							newMasses.Add(nm);
						}
					}
				}
				result = ArrayUtils.Concat(result, newMasses.ToArray());
				result = UniqueMasses(result);
			}
			return result.SubArray(1, result.Length);
		}

		private static double[] UniqueMasses(double[] array, double tol = 1e-5){
			if (array.Length < 2){
				return array;
			}
			double[] sorted = (double[]) array.Clone();
			Array.Sort(sorted);
			int counter = 1;
			double lastVal = sorted[0];
			for (int i = 1; i < sorted.Length; i++){
				if (lastVal + tol < sorted[i]){
					lastVal = sorted[i];
					sorted[counter++] = lastVal;
				}
			}
			Array.Resize(ref sorted, counter);
			return sorted;
		}

		public static double[] GetBaseMasses(string[] fixedModifications, string[] variableModifications){
			Modification[] fixMod = Tables.ToModifications(fixedModifications);
			Modification[] varMod = Tables.ToModifications(variableModifications);
			double[] aamasses = new double[AminoAcids.standardAminoAcids.Length];
			for (int i = 0; i < aamasses.Length; i++){
				char let = AminoAcids.standardAminoAcids[i].Letter;
				aamasses[i] = AminoAcids.AaMonoMasses[let];
				foreach (Modification fm in fixMod){
					for (int j = 0; j < fm.AaCount; j++){
						if (fm.GetAaAt(j) == let){
							aamasses[i] += fm.DeltaMass;
							break;
						}
					}
				}
			}
			List<double> addtlMasses = new List<double>();
			for (int i = 0; i < aamasses.Length; i++){
				char let = AminoAcids.standardAminoAcids[i].Letter;
				foreach (Modification vm in varMod){
					for (int j = 0; j < vm.AaCount; j++){
						if (vm.GetAaAt(j) == let){
							addtlMasses.Add(aamasses[i] += vm.DeltaMass);
						}
					}
				}
			}
			double[] result = ArrayUtils.Concat(aamasses, addtlMasses.ToArray());
			result = UniqueMasses(result);
			return result;
		}

		public static double GetAverageDifferenceToMonoisotope(double mass, int peakIndex){
			if (peakIndex == 0){
				return 0;
			}
			int massBin = (int) ((mass + massBinSize / 2) / massBinSize);
			(int, int) t = (massBin, peakIndex);
			lock (locker){
				if (massDiffCache.ContainsKey(t))
				{
					return massDiffCache[t];
				}
				double massRound = (massBin + 0.5) * massBinSize;
				double diff = CalcAverageDifferenceToMonoisotope(massRound, peakIndex);
				massDiffCache.Add(t, diff);
				return diff;
			}
		}

		private static double CalcAverageDifferenceToMonoisotope(double mass, int peakIndex){
			double[] m = GetAverageIsotopePattern(mass, false)[0];
			if (peakIndex >= m.Length || peakIndex < 0){
				if (m.Length < 2){
					return 0;
				}
				return peakIndex * (m[1] - m[0]);
			}
			return m[peakIndex] - m[0];
		}

		/// <summary>
		/// Return the isotope pattern for an "average" peptide of the mass given in the 1st arg.
		/// The 2nd arg is whether to include sulfur as a possible element.
		/// The isotope pattern returned is a double[][] array, the first element of which gives 
		/// the masses of the peaks in the isotope pattern, and the second of which gives the relative 
		/// weights of those peaks.
		/// </summary>
		/// <param name="mass"></param>
		/// <param name="includeSulphur"></param>
		/// <returns></returns>
		public static double[][] GetAverageIsotopePattern(double mass, bool includeSulphur){
			int massBin = (int)((mass + massBinSize / 2) / massBinSize);
			double rounded = (massBin + 0.5) * massBinSize;
			rounded = Math.Max(rounded, 100);
			if (includeSulphur){
				lock (isotopePatternListSulfur){
					if (!isotopePatternListSulfur.ContainsKey(massBin)){
						isotopePatternListSulfur.Add(massBin,
							GetAveraginePeptideMolecule(rounded, true).GetIsotopeDistribution(0.2));
					}
					return isotopePatternListSulfur[massBin];
				}
			}
			lock (isotopePatternListNoSulfur){
				if (!isotopePatternListNoSulfur.ContainsKey(massBin)){
					isotopePatternListNoSulfur.Add(massBin,
						GetAveraginePeptideMolecule(rounded, false).GetIsotopeDistribution(0.2));
				}
				return isotopePatternListNoSulfur[massBin];
			}
		}

		public static Molecule GetAveraginePeptideMolecule(double monoMass, bool includeSulphur){
			return new Molecule(GetAveraginePeptideFormula(monoMass, includeSulphur));
		}

		public static string GetAveraginePeptideFormula(double monoMass, bool includeSulphur){
			double x;
			if (includeSulphur){
				x = monoMass / averagineTotal;
			} else{
				x = monoMass / (averagineTotal - averagineCompositionS * Molecule.massS);
			}
			int nC = (int) Math.Round(averagineCompositionC * x);
			int nH = (int) Math.Round(averagineCompositionH * x);
			int nN = (int) Math.Round(averagineCompositionN * x);
			int nO = (int) Math.Round(averagineCompositionO * x);
			int nS = (int) Math.Round(averagineCompositionS * x);
			StringBuilder result = new StringBuilder();
			if (nC > 0){
				result.Append("C" + nC);
			}
			if (nH > 0){
				result.Append("H" + nH);
			}
			if (nN > 0){
				result.Append("N" + nN);
			}
			if (nO > 0){
				result.Append("O" + nO);
			}
			if (nS > 0 && includeSulphur){
				result.Append("S" + nS);
			}
			return result.ToString();
		}

		public static int CountAas(string sequence, char aa){
			int c = 0;
			foreach (char x in sequence){
				if (x == aa){
					c++;
				}
			}
			return c;
		}

		public static AminoAcid[] CalcAllAas(string[][] labels){
			if (labels == null){
				return new AminoAcid[0];
			}
			HashSet<char> r = new HashSet<char>();
			foreach (string[] s in labels){
				foreach (string label in s){
					Modification2 m = new Modification2(label);
					for (int i = 0; i < m.AaCount; i++){
						char aa = m.GetAaAt(i);
						if (aa != '-'){
							r.Add(aa);
						}
					}
				}
			}
			char[] aas = r.ToArray();
			Array.Sort(aas);
			AminoAcid[] result = new AminoAcid[r.Count];
			for (int i = 0; i < result.Length; i++){
				result[i] = AminoAcids.LetterToAa[aas[i]];
			}
			return result;
		}

		public static bool MassMatch(double m1, double m2, double tolerance, bool ppm){
			if (ppm){
				return Math.Abs(m1 - m2) * 2.0 / (m1 + m2) * 1e6 < tolerance;
			}
			return Math.Abs(m1 - m2) < tolerance;
		}

		public static double GetFullIsotopePatternMzEstimate(double[] masses, float[] intensities, IList<int> members,
			int isotopePatternStart, int charge){
			double result = 0;
			double intensity = 0;
			for (int i = 0; i < members.Count; i++){
				if (i + isotopePatternStart >= 0){
					int ind = members[i];
					double mz = masses[ind];
					double m = (mz - Molecule.massProton) * charge;
					result += intensities[ind] * (m - GetAverageDifferenceToMonoisotope(m, i + isotopePatternStart));
					intensity += intensities[ind];
				}
			}
			if (intensity == 0){
				for (int i = 0; i < members.Count; i++){
					int ind = members[i];
					double mz = masses[ind];
					double m = (mz - Molecule.massProton) * charge;
					result += intensities[ind] *
					          (m - (i + isotopePatternStart) * GetAverageDifferenceToMonoisotope(m, 1));
					intensity += intensities[ind];
				}
			}
			return result / intensity / charge + Molecule.massProton;
		}

		public static double GetLabelingMassDiff(LabelModification multipletLabel){
			return multipletLabel.LabelingDiff2.GetMostLikelyMass(0.2) -
			       multipletLabel.LabelingDiff1.GetMostLikelyMass(0.2);
		}

		public static ushort[] GetInternalModifications(ushort[] mods){
			List<ushort> result = new List<ushort>();
			foreach (ushort mod in mods){
				if (Tables.ModificationList[mod].IsInternal){
					result.Add(mod);
				}
			}
			return result.ToArray();
		}

		public static HashSet<char> GetConflictingAas(string[] fixedMods){
			HashSet<char> masses = new HashSet<char>();
			foreach (string fmod in fixedMods){
				if (!Tables.Modifications.ContainsKey(fmod)){
					continue;
				}
				Modification mod = Tables.Modifications[fmod];
				if (!mod.IsInternal){
					continue;
				}
				if (mod.ModificationType != ModificationType.Label){
					continue;
				}
				LabelModification label = new AminoAcidLabel(mod.Name, mod.GetFormula(), mod.GetAaAt(0));
				if (label.IsIsotopicLabel){
					continue;
				}
				for (int i = 0; i < mod.AaCount; i++){
					if (!masses.Contains(mod.GetAaAt(i))){
						masses.Add(mod.GetAaAt(i));
					}
				}
			}
			return masses;
		}

		public static Dictionary<char, double> GetConflictingModMasses(string[] fixedMods){
			List<Modification> mods = new List<Modification>();
			foreach (string fixedMod in fixedMods){
				if (Tables.Modifications.ContainsKey(fixedMod)){
					mods.Add(Tables.Modifications[fixedMod]);
				}
			}
			return GetConflictingModMasses(mods);
		}

		public static Dictionary<char, double> GetConflictingModMasses(IList<Modification> fmods){
			Dictionary<char, double> masses = new Dictionary<char, double>();
			foreach (Modification mod in fmods){
				if (!mod.IsInternal){
					continue;
				}
				if (Modification.IsStandardVarMod(mod.ModificationType)){
					continue;
				}
				LabelModification label = new AminoAcidLabel(mod.Name, mod.GetFormula(), mod.GetAaAt(0));
				if (label.IsIsotopicLabel){
					continue;
				}
				for (int i = 0; i < mod.AaCount; i++){
					if (masses.ContainsKey(mod.GetAaAt(i))){
						throw new Exception("Conflicting fixed modifications.");
					}
					masses.Add(mod.GetAaAt(i), mod.DeltaMass);
				}
			}
			return masses;
		}

		public static void GetTerminalModifications(string[] mods, out Modification ntermMod,
			out Modification ctermMod){
			List<Modification> m = new List<Modification>();
			foreach (string mod in mods){
				if (Tables.Modifications.ContainsKey(mod)){
					m.Add(Tables.Modifications[mod]);
				}
			}
			GetTerminalModifications(m, out ntermMod, out ctermMod);
		}

		public static void GetTerminalModifications(IEnumerable<Modification> mods, out Modification ntermMod,
			out Modification ctermMod){
			ntermMod = null;
			ctermMod = null;
			foreach (Modification mod in mods){
				if (mod.IsCterminal){
					ctermMod = mod;
				}
				if (mod.IsNterminal){
					ntermMod = mod;
				}
			}
		}

		public static double RelativeCorrectionSpline(double u, CubicSpline[] mzCalibrationPar, int massRange,
			bool isPpm){
			return isPpm ? 1e-6 * mzCalibrationPar[massRange].Interpolate(u)
				: mzCalibrationPar[massRange].Interpolate(u) / u;
		}

		public static Molecule GetPeptideMolecule(string sequence){
			Molecule m = new Molecule("H2O");
			foreach (char c in sequence){
				Molecule x = AminoAcids.LetterToAa[c];
				m = Molecule.Sum(m, x);
			}
			return m;
		}
	}
}