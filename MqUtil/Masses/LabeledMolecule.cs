using MqUtil.Mol;

namespace MqUtil.Masses{
	public class LabeledMolecule{
		private string name;

		public LabeledMolecule(string composition, string name){
			this.name = name;
			Labeled = new Molecule(composition);
			Unlabeled = Labeled.GetUnlabeledVersion();
			Combination = MassUtil.GetCombination(Molecule.GetDifferences(Labeled, Unlabeled).Item1);
		}

		public Molecule Unlabeled { get; }
		public Molecule Labeled { get; }
		public IsotopeCombination Combination { get; }
	}
}