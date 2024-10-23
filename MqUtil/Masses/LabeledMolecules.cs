using MqUtil.Mol;

namespace MqUtil.Masses{
	public static class LabeledMolecules{
		public static LabeledMolecule[] allLabeledMolecules;
		public static Dictionary<Molecule, List<LabeledMolecule>> map;

		static LabeledMolecules(){
			allLabeledMolecules = new[]{
				new LabeledMolecule("H12Cx6O6", "Glucose +6"), new LabeledMolecule("H12C2Cx4O6", "Glucose +4"),
				new LabeledMolecule("H12C3Cx3O6", "Glucose +3"), new LabeledMolecule("H12C4Cx2O6", "Glucose +2"),
				new LabeledMolecule("H12C5Cx1O6", "Glucose +1"), new LabeledMolecule("H13Cx6O6Cl", "Glucose +6.HCl"),
				new LabeledMolecule("H13CCx5O6Cl", "Glucose +5.HCl"), new LabeledMolecule("H13C2Cx4O6Cl", "Glucose +4.HCl"),
				new LabeledMolecule("H13C3Cx3O6Cl", "Glucose +3.HCl"), new LabeledMolecule("H13C4Cx2O6Cl", "Glucose +2.HCl"),
				new LabeledMolecule("H13C5Cx1O6Cl", "Glucose +1.HCl"), new LabeledMolecule("H9Cx5NO4", "Glutamate +5"),
				new LabeledMolecule("H9CCx4NO4", "Glutamate +4"), new LabeledMolecule("H9C2Cx3NO4", "Glutamate +3"),
				new LabeledMolecule("H9C3Cx2NO4", "Glutamate +2"), new LabeledMolecule("H9C4CxNO4", "Glutamate +1"),
				new LabeledMolecule("H10Cx5N2O3", "Glutamine +5"), new LabeledMolecule("H10CCx4N2O3", "Glutamine +4"),
				new LabeledMolecule("H10C2Cx3N2O3", "Glutamine +3"), new LabeledMolecule("H6Cx3O3", "Lactate +3"),
				new LabeledMolecule("H6CCx2O3", "Lactate +2"), new LabeledMolecule("H6C2CxO3", "Lactate +1"),
				new LabeledMolecule("H4Cx3O3", "Pyruvate +3"), new LabeledMolecule("H4CCx2O3", "Pyruvate +2"),
				new LabeledMolecule("H4C2CxO3", "Pyruvate +1"), new LabeledMolecule("H8Cx6O7", "Citrate +6"),
				new LabeledMolecule("H8C2Cx4O7", "Citrate +4"), new LabeledMolecule("H8C4Cx2O7", "Citrate +2"),
				new LabeledMolecule("H6Cx4O5", "Malate +4"), new LabeledMolecule("H6CCx3O5", "Malate +3"),
				new LabeledMolecule("H6C2Cx2O5", "Malate +2"), new LabeledMolecule("H6C3CxO5", "Malate +1"),
				new LabeledMolecule("H6Cx5O5", "alpha-ketoglutarate +5"), new LabeledMolecule("H6CCx4O5", "alpha-ketoglutarate +4"),
				new LabeledMolecule("H6C2Cx3O5", "alpha-ketoglutarate +3"),
				new LabeledMolecule("H6C3Cx2O5", "alpha-ketoglutarate +2"),
				new LabeledMolecule("H6C4CxO5", "alpha-ketoglutarate +1"), new LabeledMolecule("H8C3Cx2O5", "2-hydroxyglutarate +2")
				, new LabeledMolecule("H8C4CxO5", "2-hydroxyglutarate +1")
			};
			map = new Dictionary<Molecule, List<LabeledMolecule>>();
			foreach (LabeledMolecule m in allLabeledMolecules){
				Molecule unlabeled = m.Unlabeled;
				if (!map.ContainsKey(unlabeled)){
					map.Add(unlabeled, new List<LabeledMolecule>());
				}
				map[unlabeled].Add(m);
			}
		}

		//new LabeledMolecule("H10C4CxN2O3", "Glutamine +1")
		//new LabeledMolecule("H10C3Cx2N2O3", "Glutamine +2")
		//new LabeledMolecule("H8C2Cx3O5", "2-hydroxyglutarate +3")
		//new LabeledMolecule("H8CCx4O5", "2-hydroxyglutarate +4")
		//new LabeledMolecule("H8Cx5O5", "2-hydroxyglutarate +5")
		//new LabeledMolecule("H8C5CxO7", "Citrate +1")
		//new LabeledMolecule("H8C3Cx3O7", "Citrate +3")
		//new LabeledMolecule("H8CCx5O7", "Citrate +5"),
		//new LabeledMolecule("H12CCx5O6", "Glucose +5"),
	}
}