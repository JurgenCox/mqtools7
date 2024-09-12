using MqUtil.Util;
namespace MqUtil.Base {
	public class FragmentSpectrumSettings {
		public readonly List<InputParameter> vals = new List<InputParameter> {
			new InputParameter<string>("Name", "Name"),
			new InputParameter<double>("MatchTolerance", "MatchTolerance"),
			new InputParameter<bool>("MatchToleranceInPpm", "MatchToleranceInPpm"),
			new InputParameter<double>("DeisotopeTolerance", "DeisotopeTolerance"),
			new InputParameter<bool>("DeisotopeToleranceInPpm", "DeisotopeToleranceInPpm"),
			new InputParameter<double>("DeNovoTolerance", "DeNovoTolerance"),
			new InputParameter<bool>("DeNovoToleranceInPpm", "DeNovoToleranceInPpm"),
			new InputParameter<bool>("Deisotope", "Deisotope"),
			new InputParameter<int>("Topx", "Topx"),
			new InputParameter<double>("TopxInterval", "TopxInterval", 100),
			new InputParameter<bool>("HigherCharges", "HigherCharges"),
			new InputParameter<bool>("IncludeWater", "IncludeWater"),
			new InputParameter<bool>("IncludeAmmonia", "IncludeAmmonia"),
            new InputParameter<bool>("IncludeWaterCross", "IncludeWaterCross"),
            new InputParameter<bool>("IncludeAmmoniaCross", "IncludeAmmoniaCross"),
			new InputParameter<bool>("DependentLosses", "DependentLosses"),
			new InputParameter<bool>("Recalibration", "Recalibration")
		};

		public readonly Dictionary<string, InputParameter> map;

		public FragmentSpectrumSettings() {
			map = new Dictionary<string, InputParameter>();
			foreach (InputParameter val in vals) {
				map.Add(val.Name, val);
			}
		}

		public string Name { get; set; }
		public double MatchTolerance { get; set; }
		public bool MatchToleranceInPpm { get; set; }
		public double DeisotopeTolerance { get; set; }
		public bool DeisotopeToleranceInPpm { get; set; }
		public double DeNovoTolerance { get; set; }
		public bool DeNovoToleranceInPpm { get; set; }
		public bool Deisotope { get; set; }
		public int Topx { get; set; }
		public double TopxInterval { get; set; } = 100;
		public bool HigherCharges { get; set; }
		public bool IncludeWater { get; set; }
		public bool IncludeAmmonia { get; set; }
		public bool DependentLosses { get; set; }
		public bool Recalibration { get; set; }
        public bool IncludeWaterCross { get; set; }
        public bool IncludeAmmoniaCross { get; set; }
		public FragmentSpectrumSettings(string name, int topx, double topxInterval, double matchTolerance,
			bool matchToleranceInPpm, double deNovoTolerance, bool deNovoToleranceInPpm, double deisotopeTolerance,
			bool deisotopeToleranceInPpm, bool deisotope, bool higherCharges, bool includeWater, bool includeAmmonia,
            bool includeWaterCross, bool includeAmmoniaCross,
			bool dependentLosses, bool recalibration) {
			Name = name;
			Topx = topx;
			TopxInterval = topxInterval;
			MatchToleranceInPpm = matchToleranceInPpm;
			Deisotope = deisotope;
			HigherCharges = higherCharges;
			IncludeWater = includeWater;
			IncludeAmmonia = includeAmmonia;
            IncludeWaterCross = includeWaterCross;
            IncludeAmmoniaCross = includeAmmoniaCross;
			DependentLosses = dependentLosses;
			MatchTolerance = matchTolerance;
			DeNovoTolerance = deNovoTolerance;
			DeisotopeTolerance = deisotopeTolerance;
			MatchToleranceInPpm = matchToleranceInPpm;
			DeNovoToleranceInPpm = deNovoToleranceInPpm;
			DeisotopeToleranceInPpm = deisotopeToleranceInPpm;
			Recalibration = recalibration;
		}

		public FragmentSpectrumSettings Clone() {
			return new FragmentSpectrumSettings(Name, Topx, TopxInterval, MatchTolerance, MatchToleranceInPpm,
				DeNovoTolerance, DeNovoToleranceInPpm, DeisotopeTolerance, DeisotopeToleranceInPpm, Deisotope,
				HigherCharges, IncludeWater, IncludeAmmonia, IncludeWaterCross, IncludeAmmoniaCross, DependentLosses, Recalibration);
		}

		public bool MassMatch(double m1, double m2) {
			if (MatchToleranceInPpm) {
				return Math.Abs(m1 - m2) <= 0.5 * (m1 + m2) * MatchTolerance * 1e-6;
			}
			return Math.Abs(m1 - m2) <= MatchTolerance;
		}

		public bool DeNovoMassMatch(double m1, double m2, bool wideWindow) {
			double tol = wideWindow ? MatchTolerance : DeNovoTolerance;
			if (DeNovoToleranceInPpm) {
				return Math.Abs(m1 - m2) <= 0.5 * (m1 + m2) * tol * 1e-6;
			}
			return Math.Abs(m1 - m2) <= tol;
		}
	}
}