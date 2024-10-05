using MqUtil.Mol;
using MqUtil.Ms.Fragment;
using MqUtil.Ms.Search;
namespace MqUtil.Ms.Annot.Ions{
	public class PIon : IonType{
		protected override string Name => "p";
		protected override string Name2 => "p";
		public override bool IsNTerminal => false;
		public override bool IsCTerminal => true;


        protected override double[] GetSeries(string sequence, PeptideModificationInfo fixedMods,
            PeptideModificationState varMods, bool neutralLoss, out PeakAnnotation[] des,
            Dictionary<ushort, Modification2> specialMods)
        {
            return FragmentSeries.GetPSeries(sequence, fixedMods, varMods, out des);
        }

        protected override double CalcMass(double mass, int charge){
			return (mass + Molecule.massProton*(charge - 1))/charge;
		}
	}
}