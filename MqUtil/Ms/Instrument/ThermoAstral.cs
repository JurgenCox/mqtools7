using MqUtil.Ms.Enums;
namespace MqUtil.Ms.Instrument{
	public class ThermoAstral : ThermoInstrument{
		public ThermoAstral(int index) : base(index){
		}
		public override string Name => "Astral";
		public override double DiaInitialPrecMassTolPpmDefault => 4;
		public override double DiaInitialFragMassTolPpmDefault => 7;
		public override DiaQuantMethod DiaQuantMethodDefault => DiaQuantMethod.TopFragmentsAnnotLfqSplit;
		public override bool UseMs1CentroidsDefault => true;
	}
}