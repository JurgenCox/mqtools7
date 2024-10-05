namespace MqUtil.Ms.Instrument{
	public class BrukerQtof : QtofInstrument{
		public BrukerQtof(int index) : base(index){
		}
		public override string Name => "Bruker Q-TOF";
		public override bool UseMs1CentroidsDefault => true;
		public override bool UseMs2CentroidsDefault => true;
		public override double DiaMinMsmsIntensityForQuantDefault => 30;
	}
}