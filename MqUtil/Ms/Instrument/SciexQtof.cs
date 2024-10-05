namespace MqUtil.Ms.Instrument{
	public class SciexQtof : QtofInstrument{
		public SciexQtof(int index) : base(index){
		}
		public override string Name => "Sciex Q-TOF";
		public override bool UseMs1CentroidsDefault => false;
		public override bool UseMs2CentroidsDefault => false;
		public override double DiaMinMsmsIntensityForQuantDefault => 0;
	}
}