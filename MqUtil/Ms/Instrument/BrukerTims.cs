namespace MqUtil.Ms.Instrument{
	public class BrukerTims : QtofInstrument{
		public BrukerTims(int index) : base(index){
		}
		public override string Name => "Bruker TIMS";
		public override double DiaMinMsmsIntensityForQuantDefault => 30;
		public override double DiaFragIntensityThreshold1Default => 15;
		public override double DiaFragIntensityThreshold2Default => 15;
		public override double DiaFragIntensityThreshold3Default => 15;
		public override double DiaFragIntensityThreshold4Default => 15;
		public sealed override bool UseMs1CentroidsDefault => false;
		public sealed override bool UseMs2CentroidsDefault => false;
	}
}