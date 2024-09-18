namespace MqUtil.Ms.Analyzer{
	public class MassAnalyzerItms : MsmsMassAnalyzer{
		public MassAnalyzerItms(int index) : base(index) {}
		public override double MatchToleranceDefault => 0.5;
		public override double DeNovoToleranceDefault => 0.5;
		public override double DeisotopeToleranceDefault => 0.15;
		public override bool MatchToleranceInPpmDefault => false;
		public override bool DeNovoToleranceInPpmDefault => false;
		public override bool DeisotopeToleranceInPpmDefault => false;
		public override bool DeisotopeDefault => false;
		public override bool HigherChargesDefault => true;
		public override bool WaterDefault => true;
		public override bool AmmoniaDefault => true;
        public override bool WaterCrossDefault => false;
		public override bool AmmoniaCrossDefault => false;
		public override bool DependentLossesDefault => true;
		public override bool RecalibrationDefault => false;
		public override int TopxDefault => 8;
		public override double TopxIntervalDefault => 100;
		public override string Name => "ITMS";
		public override string Description => "MS/MS spectra that are measured in the linear ion trap.";
	}
}