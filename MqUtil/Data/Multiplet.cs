using MqApi.Num;
using MqApi.Util;
namespace MqUtil.Data{
	/// <summary>
	/// A Multiplet object encodes the information from related isotope clusters,
	/// mostly as pair-wise relationships. An isotope cluster is the series of m/q
	/// peaks due to an isotopically pure peptide (labeled or unlabeled) and the 
	/// versions of that peptide incorporating one or more heavier, naturally-
	/// occurring isotopes. The number of isotope clusters, i.e. the number 
	/// of unlabeled and labeled species of a given peptide in a given charge 
	/// state, is Niso. The number of pair-wise combinations, i.e. the number of "label 
	/// pairs", is Npairs = Niso*(Niso-1)/2.
	/// </summary>
	public class Multiplet {
		public int[] IsotopeClusterIndices { get; set; }
		public int[] IsotopePatternStarts { get; }
		public double[] MassDiffs { get; }
		public double MassError { get; set; }
		public double UncalibratedMass { get; set; }

		/// <summary>
		/// Number of data points that were used for determination of mass
		/// </summary>
		public int Npoints { get; set; }

		/// <summary>
		/// Calibrated mass of the neutral molecule. (Not m/z.)
		/// </summary>
		private double mass = double.NaN;

		/// <summary>
		/// Number of isotopic peaks including all labeled forms if applicable
		/// </summary>
		public int Niso { get; set; }

		/// <summary>
		/// Number of full scans this peptide feature is spanning 
		/// </summary>
		public int Nscans { get; set; }


		/// <summary>
		/// Length is Niso.
		/// </summary>
		public double[] HighestPeakMzs { get; set; }

		public double RetentionTime { get; set; }
		public double RetentionTimeMin { get; set; }
		public double RetentionTimeMax { get; set; }
		public double RetentionTimeFwhm { get; set; }

		/// <summary>
		/// Length is Npairs.
		/// </summary>
		public double[] Ratios { get; }

		/// <summary>
		/// Length is Npairs.
		/// </summary>
		public double[] NormalizedRatios { get; }

		/// <summary>
		/// Length is Npairs.
		/// </summary>
		public int[] Shifts { get; }

		/// <summary>
		/// Length is Niso.
		/// </summary>
		public float[] Intensities { get; set; }

		public double IntensityCorrection { get; set; }

		/// <summary>
		/// Length is Niso. Set by the constructor to the value of LabelCounts for the 
		/// label pair with the highest indices.
		/// </summary>
		public int[] AaCounts { get; private set; }

		public float TheorIsotopeCorr { get; set; }
		public float IsotopeCorr { get; set; }
		public float TimeCorr { get; set; }
		public double[][] Profiles { get; set; }
		public double[][] IsotopePatterns { get; set; }

		public Multiplet(BinaryReader reader) {
			IsotopeClusterIndices = FileUtils.ReadInt32Array(reader);
			IsotopePatternStarts = FileUtils.ReadInt32Array(reader);
			MassDiffs = FileUtils.ReadDoubleArray(reader);
			MassError = reader.ReadDouble();
			mass = reader.ReadDouble();
			UncalibratedMass = reader.ReadDouble();
			Npoints = reader.ReadInt32();
			Niso = reader.ReadInt32();
			Nscans = reader.ReadInt32();
			HighestPeakMzs = FileUtils.ReadDoubleArray(reader);
			RetentionTime = reader.ReadDouble();
			RetentionTimeMin = reader.ReadDouble();
			RetentionTimeMax = reader.ReadDouble();
			RetentionTimeFwhm = reader.ReadDouble();
			Ratios = FileUtils.ReadDoubleArray(reader);
			NormalizedRatios = FileUtils.ReadDoubleArray(reader);
			Shifts = FileUtils.ReadInt32Array(reader);
			Intensities = FileUtils.ReadFloatArray(reader);
			IntensityCorrection = reader.ReadDouble();
			int len = reader.ReadInt32();
			AaCounts = new int[len];
			for (int i = 0; i < AaCounts.Length; i++) {
				AaCounts[i] = reader.ReadInt32();
			}
			TheorIsotopeCorr = reader.ReadSingle();
			IsotopeCorr = reader.ReadSingle();
			TimeCorr = reader.ReadSingle();
			Profiles = FileUtils.Read2DDoubleArray(reader);
			IsotopePatterns = FileUtils.Read2DDoubleArray(reader);
		}

		public Multiplet(LabelPair pair) {
			IsotopeClusterIndices = new int[2];
			IsotopeClusterIndices[0] = pair.Index1;
			IsotopeClusterIndices[1] = pair.Index2;
			IsotopePatternStarts = new int[2];
			MassDiffs = new double[2];
			UncalibratedMass = double.NaN;
			HighestPeakMzs = new double[2];
			Ratios = new[] {double.NaN};
			NormalizedRatios = new[] {double.NaN};
			Shifts = new[] {0};
			Intensities = new float[2];
			AaCounts = pair.LabelCounts;
			IsotopeCorr = (float)pair.IsotopeCorrelation;
			TimeCorr = (float)pair.TimeCorrelation;
		}

		public Multiplet(LabelPair pair01, LabelPair pair02, LabelPair pair12) {
			IsotopeClusterIndices = new[] {-1, -1, -1};
			if (pair01 != null) {
				IsotopeClusterIndices[0] = pair01.Index1;
				IsotopeClusterIndices[1] = pair01.Index2;
			}
			if (pair02 != null) {
				IsotopeClusterIndices[0] = pair02.Index1;
				IsotopeClusterIndices[2] = pair02.Index2;
			}
			if (pair12 != null) {
				IsotopeClusterIndices[1] = pair12.Index1;
				IsotopeClusterIndices[2] = pair12.Index2;
			}
			IsotopePatternStarts = new int[3];
			MassDiffs = new double[3];
			HighestPeakMzs = new double[3];
			UncalibratedMass = double.NaN;
			Ratios = new[] {double.NaN, double.NaN, double.NaN};
			NormalizedRatios = new[] {double.NaN, double.NaN, double.NaN};
			Shifts = new[] {0, 0, 0};
			Intensities = new float[3];
			if (pair01 != null) {
				AaCounts = pair01.LabelCounts;
			}
			if (pair02 != null) {
				AaCounts = pair02.LabelCounts;
			}
			if (pair12 != null) {
				AaCounts = pair12.LabelCounts;
			}
		}

		public Multiplet(IList<LabelPair[]> lps) {
			int n = lps.Count;
			IsotopeClusterIndices = new int[n];
			for (int i = 0; i < n; i++) {
				IsotopeClusterIndices[i] = -1;
			}
			for (int i = 0; i < lps.Count; i++) {
				for (int j = 0; j < i; j++) {
					LabelPair lp = lps[i][j];
					if (lp != null) {
						IsotopeClusterIndices[i] = lp.Index2;
						IsotopeClusterIndices[j] = lp.Index1;
					}
				}
			}
			IsotopePatternStarts = new int[n];
			MassDiffs = new double[n];
			HighestPeakMzs = new double[n];
			UncalibratedMass = double.NaN;
			int nratios = n * (n - 1) / 2;
			Ratios = new double[nratios];
			NormalizedRatios = new double[nratios];
			Shifts = new int[nratios];
			Intensities = new float[n];
			for (int i = 0; i < nratios; i++) {
				Ratios[i] = double.NaN;
				NormalizedRatios[i] = double.NaN;
			}
			for (int i = 0; i < lps.Count; i++) {
				for (int j = 0; j < i; j++) {
					LabelPair lp = lps[i][j];
					if (lp != null) {
						AaCounts = lp.LabelCounts;
					}
				}
			}
		}

		public Multiplet(int index1) {
			IsotopeClusterIndices = new[] {index1};
			IsotopePatternStarts = new int[1];
			MassDiffs = new double[1];
			HighestPeakMzs = new double[1];
			UncalibratedMass = double.NaN;
			NormalizedRatios = new double[0];
			Ratios = new double[0];
			Shifts = new int[0];
			Intensities = new float[1];
			AaCounts = new int[0];
		}

		public double GetMassError(AutocorrFitfunc factor, double m1) {
			return factor.Interpolate(MassError / m1 * 1e6) * MassError;
		}

		/// <summary>
		/// Mass of the neutral molecule. (Not m/z.)
		/// </summary>
		public double Mass {
			get {
				if (double.IsNaN(mass)) {
					throw new Exception("Mass is not set.");
				}
				return mass;
			}
		}

		public void SetMass(double val) {
			mass = val;
		}

		/// <summary>
		/// Mass of the neutral molecule. (Not m/z.)
		/// </summary>
		public double MassUnchecked => mass;

		public double GetMassError(AutocorrFitfunc factor) {
			//if (double.IsNaN(mass)){
			//	throw new Exception("Mass is not set.");
			//}
			if (factor == null) {
				return double.NaN;
			}
			return factor.Interpolate(MassError / mass * 1e6) * MassError;
		}

		public double RetentionTimeLength => RetentionTimeMax - RetentionTimeMin;

		public virtual void Write(BinaryWriter writer) {
			FileUtils.Write(IsotopeClusterIndices, writer);
			FileUtils.Write(IsotopePatternStarts, writer);
			FileUtils.Write(MassDiffs, writer);
			writer.Write(MassError);
			writer.Write(mass);
			writer.Write(UncalibratedMass);
			writer.Write(Npoints);
			writer.Write(Niso);
			writer.Write(Nscans);
			FileUtils.Write(HighestPeakMzs, writer);
			writer.Write(RetentionTime);
			writer.Write(RetentionTimeMin);
			writer.Write(RetentionTimeMax);
			writer.Write(RetentionTimeFwhm);
			FileUtils.Write(Ratios, writer);
			FileUtils.Write(NormalizedRatios, writer);
			FileUtils.Write(Shifts, writer);
			FileUtils.Write(Intensities, writer);
			writer.Write(IntensityCorrection);
			int len = AaCounts?.Length ?? 0;
			writer.Write(len);
			for (int i = 0; i < len; i++) {
				writer.Write(AaCounts[i]);
			}
			writer.Write(TheorIsotopeCorr);
			writer.Write(IsotopeCorr);
			writer.Write(TimeCorr);
			if (Profiles == null){
				Profiles = new double[0][];
			}
			FileUtils.Write(Profiles, writer);
			if (IsotopePatterns == null){
				IsotopePatterns = new double[0][];
			}
			FileUtils.Write(IsotopePatterns, writer);
		}

		public void SetRatio(int i, int j, double r) {
			Ratios[ArrayUtils.TriangleToLinearIndex(i, j)] = r;
		}

		public void SetShift(int i, int j, int r) {
			Shifts[ArrayUtils.TriangleToLinearIndex(i, j)] = r;
		}

		public void SetNormalizedRatio(int i, int j, double r) {
			NormalizedRatios[ArrayUtils.TriangleToLinearIndex(i, j)] = r;
		}

		public double GetRatio(int i, int j) {
			return Ratios[ArrayUtils.TriangleToLinearIndex(i, j)];
		}

		public int GetShift(int i, int j) {
			return Shifts[ArrayUtils.TriangleToLinearIndex(i, j)];
		}

		public double GetNormalizedRatio(int i, int j) {
			return NormalizedRatios[ArrayUtils.TriangleToLinearIndex(i, j)];
		}

		public void Dispose() {
			AaCounts = null;
		}
	}
}