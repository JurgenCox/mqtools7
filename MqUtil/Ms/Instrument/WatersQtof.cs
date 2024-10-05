namespace MqUtil.Ms.Instrument{
	public class WatersQtof : QtofInstrument{
		public WatersQtof(int index) : base(index){
		}
		public override string Name => "Waters Q-TOF";
		public override bool UseMs1CentroidsDefault => true;
		public override bool UseMs2CentroidsDefault => true;
		public override double DiaMinMsmsIntensityForQuantDefault => 0;
	}
}