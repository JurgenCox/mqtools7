using MqUtil.Base;
using MqUtil.Ms.Analyzer;
using MqUtil.Ms.Fragment;
using MqUtil.Ms.Utils;
namespace MqUtil.Ms.Graph {
	public class SpectrumInfo {
		public string RawFilePath { get; set; }
		public string CombinedPath { get; set; }
		private FragmentSpectrumSettings fragmentationTypeData;
		public string RawFileName { get; set; }
		public int ScanNumber { get; set; }
		public MsmsSpectrum spectrum;
		public List<Annotation> AllAnnotations { get; set; }
		public List<Annotation> AllAnnotations1 { get; set; }
		public List<Annotation> AllAnnotations2 { get; set; }

		public string[] FixedModifications { get; set; }
		public MsmsMassAnalyzer MassAnalyzer { get; set; }
		public FragmentationType FragmentationType { get; set; }
		public bool IsCrosslink { get; set; }

		public SpectrumInfo() {
			MassAnalyzer = MsmsMassAnalyzer.unknown;
			AllAnnotations = new List<Annotation>();
			AllAnnotations1 = new List<Annotation>();
			AllAnnotations2 = new List<Annotation>();
			ScanNumber = -1;
			IsCrosslink = false;
		}

        
        public PeptideHit MainHit { get; set; }
		public PeptideHit SecondPeptideHit { get; set; }
		public PeptideHit BestHit => MainHit ?? SecondPeptideHit;

		public CrossPeptideHit CrossPeptideHit { get; set; }

		public CrossPeptideHit SecondCrossPeptideHit { get; set; }
		
		public FragmentSpectrumSettings FragmentSpectrumSettings {
			get {
				if (fragmentationTypeData == null && FragmentationType != null) {
					fragmentationTypeData = new FragmentSpectrumSettings(FragmentationType.Name,
						MassAnalyzer.TopxDefault, MassAnalyzer.TopxIntervalDefault, MassAnalyzer.MatchToleranceDefault,
						MassAnalyzer.MatchToleranceInPpmDefault, MassAnalyzer.DeNovoToleranceDefault,
						MassAnalyzer.DeNovoToleranceInPpmDefault, MassAnalyzer.DeisotopeToleranceDefault,
						MassAnalyzer.DeNovoToleranceInPpmDefault, MassAnalyzer.DeisotopeDefault,
						MassAnalyzer.HigherChargesDefault, MassAnalyzer.WaterDefault, MassAnalyzer.AmmoniaDefault, 
                        MassAnalyzer.WaterCrossDefault, MassAnalyzer.AmmoniaCrossDefault,
						MassAnalyzer.DependentLossesDefault, MassAnalyzer.RecalibrationDefault);
				}
				return fragmentationTypeData;
			}
			set => fragmentationTypeData = value;
		}

		public virtual void Dispose() {
			if (MainHit != null) {
				MainHit.Dispose();
				MainHit = null;
			}
			if (SecondPeptideHit != null) {
				SecondPeptideHit.Dispose();
				SecondPeptideHit = null;
			}
			spectrum?.Dispose();
			AllAnnotations.Clear();
			FragmentSpectrumSettings = null;
			FragmentationType = null;
			FixedModifications = null;
		}

		public double MinMass() {
			return spectrum?.MinMass ?? double.NaN;
		}

		public double MaxMass() {
			return spectrum?.MaxMass ?? double.NaN;
		}
	}
}