using MqApi.Util;
using MqUtil.Mol;
using MqUtil.Ms.Search;
namespace MqUtil.Ms.Annot.Ions{
	public class Z2HIon : IonType{
		protected override string Name => "z2H";
		protected override string Name2 => "z" + StringUtils.ToSubscript(2, false) + "H";
		public override bool IsNTerminal => false;
		public override bool IsCTerminal => true;

		protected override double CalcMass(double mass, int charge){
			return (mass + 2*Molecule.massProton*(charge - 1))/charge;
		}

		protected override double[] GetSeries(string sequence, PeptideModificationInfo fixedMods,
			PeptideModificationState varMods, bool neutralLoss, out PeakAnnotation[] des,
			Dictionary<ushort, Modification2> specialMods){
			double[] z = GetSeries(new ZIon(), sequence, fixedMods, varMods, neutralLoss, out des, specialMods);
			for (int i = 0; i < z.Length; i++){
				des[i] = new MsmsPeakAnnotation(new Z2HIon(), i + 1, 1, z[i], des[i].NeutralLossLevel);
			}
			return z;
		}
	}
}