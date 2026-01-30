namespace MqUtil.Ms.Instrument{
	public class AgilentQtof : QtofInstrument{
		public AgilentQtof(int index) : base(index){
		}
		public override string Name => "Agilent Q-TOF";
		public override bool UseMs1CentroidsDefault => true;
		public override bool UseMs2CentroidsDefault => true;
		public override double DiaMinMsmsIntensityForQuantDefault => 0;
		public override double DiaFragIntensityThreshold1Default => 0;
		public override double DiaFragIntensityThreshold2Default => 0;
		public override double DiaFragIntensityThreshold3Default => 0;
		public override double DiaFragIntensityThreshold4Default => 0;
	}
}