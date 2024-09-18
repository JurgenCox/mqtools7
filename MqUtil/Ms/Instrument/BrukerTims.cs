namespace MqUtil.Ms.Instrument{
	public class BrukerTims : QtofInstrument{
		public BrukerTims(int index) : base(index){ }
		public override string Name => "Bruker TIMS";
		public override double DiaMinMsmsIntensityForQuantDefault => 30;
		public sealed override bool UseMs1CentroidsDefault => false;
		public sealed override bool UseMs2CentroidsDefault => false;
	}
}