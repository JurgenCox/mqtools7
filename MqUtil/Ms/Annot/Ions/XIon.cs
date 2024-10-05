using MqUtil.Mol;
using MqUtil.Ms.Fragment;
using MqUtil.Ms.Search;
namespace MqUtil.Ms.Annot.Ions{
	public class XIon : IonType{
		protected override string Name => "x";
		protected override string Name2 => "x";
		public override bool IsNTerminal => false;
		public override bool IsCTerminal => true;

		protected override double[] GetSeries(string sequence, PeptideModificationInfo fixedMods,
			PeptideModificationState varMods, bool neutralLoss, out PeakAnnotation[] des,
			Dictionary<ushort, Modification2> specialMods){
			return neutralLoss
				? FragmentSeries.GetNeutralLossXSeries(sequence, fixedMods, varMods, out des, specialMods)
				: FragmentSeries.GetXSeries(sequence, fixedMods, varMods, out des, specialMods);
		}

		protected override double CalcMass(double mass, int charge){
			return (mass + Molecule.massProton*(charge - 1))/charge;
		}
	}
}