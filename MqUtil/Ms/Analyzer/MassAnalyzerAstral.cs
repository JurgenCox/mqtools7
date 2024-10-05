namespace MqUtil.Ms.Analyzer{
	public class MassAnalyzerAstral : MsmsMassAnalyzer{
		public MassAnalyzerAstral(int index) : base(index){
		}
		public override double MatchToleranceDefault => 25;
		public override bool MatchToleranceInPpmDefault => true;
		public override double DeNovoToleranceDefault => 25;
		public override bool DeNovoToleranceInPpmDefault => true;
		public override double DeisotopeToleranceDefault => 0.01;
		public override bool DeisotopeToleranceInPpmDefault => false;
		public override bool DeisotopeDefault => true;
		public override bool HigherChargesDefault => true;
		public override bool WaterDefault => true;
		public override bool AmmoniaDefault => true;
		public override bool WaterCrossDefault => false;
		public override bool AmmoniaCrossDefault => false;
		public override bool DependentLossesDefault => true;
		public override bool RecalibrationDefault => false;
		public override int TopxDefault => 16;
		public override double TopxIntervalDefault => 100;
		public override string Name => "ASTRAL";
		public override string Description => "MS/MS spectra that are measured with Astral.";
	}
}