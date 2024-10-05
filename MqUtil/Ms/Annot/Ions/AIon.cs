using MqUtil.Mol;
using MqUtil.Ms.Fragment;
using MqUtil.Ms.Search;
namespace MqUtil.Ms.Annot.Ions{
	public class AIon : IonType{
		protected override string Name => "a";
		protected override string Name2 => "a";
		public override bool IsNTerminal => true;
		public override bool IsCTerminal => false;

		protected override double[] GetSeries(string sequence, PeptideModificationInfo fixedMods,
			PeptideModificationState varMods, bool neutralLoss, out PeakAnnotation[] des,
			Dictionary<ushort, Modification2> specialMods){
			if (neutralLoss){
				des = null;
				return null;
			}
			return FragmentSeries.GetASeries(sequence, fixedMods, varMods, out des, specialMods);
		}

		protected override double CalcMass(double mass, int charge){
			return (mass + Molecule.massProton*(charge - 1))/charge;
		}
	}
}