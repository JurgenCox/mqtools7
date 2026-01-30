using MqUtil.Ms.Enums;
namespace MqUtil.Ms.Instrument{
	public class ThermoFtms : ThermoInstrument{
		public ThermoFtms(int index) : base(index){
		}
		public override string Name => "Orbitrap";
		public override double DiaInitialPrecMassTolPpmDefault => 10;
		public override double DiaInitialFragMassTolPpmDefault => 20;
		public override DiaQuantMethod DiaQuantMethodDefault => DiaQuantMethod.MixedLfqSplit;
		public override bool UseMs1CentroidsDefault => false;
		public override double DiaFragIntensityThreshold1Default => 0;
		public override double DiaFragIntensityThreshold2Default => 0;
		public override double DiaFragIntensityThreshold3Default => 0;
		public override double DiaFragIntensityThreshold4Default => 0;

	}
}