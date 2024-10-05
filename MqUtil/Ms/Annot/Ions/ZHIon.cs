using MqUtil.Mol;
using MqUtil.Ms.Search;
namespace MqUtil.Ms.Annot.Ions{
	public class ZHIon : IonType{
		protected override string Name => "zH";
		protected override string Name2 => "zH";
		public override bool IsNTerminal => false;
		public override bool IsCTerminal => true;

		protected override double CalcMass(double mass, int charge){
			return (mass + Molecule.massProton*(charge - 1))/charge;
		}

		protected override double[] GetSeries(string sequence, PeptideModificationInfo fixedMods,
			PeptideModificationState varMods, bool neutralLoss, out PeakAnnotation[] des,
			Dictionary<ushort, Modification2> specialMods){
			double[] z = GetSeries(new ZIon(), sequence, fixedMods, varMods, neutralLoss, out des, specialMods);
			for (int i = 0; i < z.Length; i++){
				des[i] = new MsmsPeakAnnotation(new ZHIon(), i + 1, 1, z[i], des[i].NeutralLossLevel);
			}
			return z;
		}
	}
}