using MqUtil.Ms.Enums;
namespace MqUtil.Ms.Analyzer{
	/// <summary>
	/// There are 4 implementations of this abstract class, with the following values for Name and Description:
	///   FTMS: MS/MS spectra that are measured in the Orbitrap mass analyzer or with FT-ICR.
	///   ITMS: MS/MS spectra that are measured in the linear ion trap.
	///   TOF:  MS/MS spectra that are measured with a time of flight analyzer.
	///   Unknown: Unknown measurement device.
	/// </summary>
	public abstract class MsmsMassAnalyzer{
		public static readonly MsmsMassAnalyzer ftms = new MassAnalyzerFtms(0);
		public static readonly MsmsMassAnalyzer itms = new MassAnalyzerItms(1);
		public static readonly MsmsMassAnalyzer tof = new MassAnalyzerTof(2);
		public static readonly MsmsMassAnalyzer astral = new MassAnalyzerAstral(3);
		public static readonly MsmsMassAnalyzer unknown = new MassAnalyzerUnknown(4);
		protected MsmsMassAnalyzer(int index){
			Index = index;
		}
		public static MsmsMassAnalyzer[] AllMassAnalyzers{ get; } ={ftms, itms, tof, astral, unknown};
		public abstract double MatchToleranceDefault{ get; }
		public abstract bool MatchToleranceInPpmDefault{ get; }
		public abstract double DeNovoToleranceDefault{ get; }
		public abstract bool DeNovoToleranceInPpmDefault{ get; }
		public abstract double DeisotopeToleranceDefault{ get; }
		public abstract bool DeisotopeToleranceInPpmDefault{ get; }
		public abstract bool DeisotopeDefault{ get; }
		public abstract bool HigherChargesDefault{ get; }
		public abstract bool WaterDefault{ get; }
		public abstract bool AmmoniaDefault{ get; }
		public abstract bool WaterCrossDefault{ get; }
		public abstract bool AmmoniaCrossDefault{ get; }
		public abstract bool DependentLossesDefault{ get; }
		public abstract bool RecalibrationDefault{ get; }
		public abstract int TopxDefault{ get; }
		public abstract double TopxIntervalDefault{ get; }
		public abstract string Name{ get; }
		public abstract string Description{ get; }
		public int Index{ get; }
		public static MsmsMassAnalyzer FromName(string name){
			if (name == null){
				return null;
			}
			foreach (MsmsMassAnalyzer t in AllMassAnalyzers.Where(t => t.Name.ToLower().Equals(name.ToLower()))){
				return t;
			}
			throw new Exception("Unknown mass analyzer: " + name);
		}
		public static MsmsMassAnalyzer FromEnum(MassAnalyzerEnum e){
			switch (e){
				case MassAnalyzerEnum.Ftms:
					return ftms;
				case MassAnalyzerEnum.Itms:
					return itms;
				case MassAnalyzerEnum.Tof:
					return tof;
				case MassAnalyzerEnum.Astral:
					return astral;
				case MassAnalyzerEnum.Unknown:
					return unknown;
			}
			throw new Exception("Never get here.");
		}
		public static MassAnalyzerEnum ToEnum(MsmsMassAnalyzer e){
			switch (e.Name.ToUpper()){
				case "FTMS":
					return MassAnalyzerEnum.Ftms;
				case "ITMS":
					return MassAnalyzerEnum.Itms;
				case "TOF":
					return MassAnalyzerEnum.Tof;
				case "ASTRAL":
					return MassAnalyzerEnum.Astral;
				case "UNKNOWN":
					return MassAnalyzerEnum.Unknown;
			}
			throw new Exception("Never get here.");
		}
		public override string ToString(){
			return Name;
		}
	}
}