using MqUtil.Mol;
using MqUtil.Ms.Search;
namespace MqUtil.Ms.Annot.Ions{
	public class ParentIon : IonType{
		protected override string Name => "[M+H]";
		protected override string Name2 => "[M+H]";
		public override bool IsNTerminal => false;
		public override bool IsCTerminal => false;

		protected override double[] GetSeries(string sequence, PeptideModificationInfo fixedMods,
			PeptideModificationState varMods, bool neutralLoss, out PeakAnnotation[] des,
			Dictionary<ushort, Modification2> specialMods){
			double mass = CalcMass(sequence, fixedMods, varMods);
			const int max = 5;
			double[] mz = new double[max];
			des = new PeakAnnotation[max];
			for (int charge = 1; charge <= max; charge++){
				mz[charge - 1] = Molecule.ConvertToMz(mass, charge);
				des[charge - 1] = new MsmsPeakAnnotation(new ParentIon(), -1, 1, mz[charge - 1], 0);
			}
			return mz;
		}

		private static double CalcMass(string sequence, PeptideModificationInfo fixedMods, PeptideModificationState varMods){
			double result = AminoAcids.CalcMonoisotopicMass(sequence);
			result += fixedMods.GetDeltaMass();
			result += varMods.GetDeltaMass();
			return result;
		}

		protected override double CalcMass(double mass, int charge){
			return (mass + Molecule.massProton*(charge - 1))/charge;
		}
	}
}