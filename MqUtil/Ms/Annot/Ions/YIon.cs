using MqUtil.Mol;
using MqUtil.Ms.Fragment;
using MqUtil.Ms.Search;
namespace MqUtil.Ms.Annot.Ions{
	public class YIon : IonType{
		protected override string Name => "y";
		protected override string Name2 => "y";
		public override bool IsNTerminal => false;
		public override bool IsCTerminal => true;

		protected override double[] GetSeries(string sequence, PeptideModificationInfo fixedMods,
			PeptideModificationState varMods, bool neutralLoss, out PeakAnnotation[] des,
			Dictionary<ushort, Modification2> specialMods){
			return neutralLoss
				? FragmentSeries.GetNeutralLossYSeries(sequence, fixedMods, varMods, out des, specialMods)
				: FragmentSeries.GetYSeries(sequence, fixedMods, varMods, out des, specialMods);
		}

		protected override double CalcMass(double mass, int charge){
			return (mass + Molecule.massProton*(charge - 1))/charge;
		}
	}
}