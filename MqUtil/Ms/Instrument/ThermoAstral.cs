using MqUtil.Ms.Enums;
namespace MqUtil.Ms.Instrument{
	public class ThermoAstral : ThermoInstrument{
		public ThermoAstral(int index) : base(index){
		}
		public override string Name => "Astral";
		public override double DiaInitialPrecMassTolPpmDefault => 5;
		public override double DiaInitialFragMassTolPpmDefault => 20;
		public override DiaQuantMethod DiaQuantMethodDefault => DiaQuantMethod.TopFragmentsAnnotLfqSplit;
		public override bool UseMs1CentroidsDefault => true;
	}
}