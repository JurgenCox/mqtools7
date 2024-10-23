using MqApi.Util;
using MqUtil.Mol;
using MqUtil.Parse.Chebi;

namespace MqUtil.Masses{
	public static class MassUtil{
		private static readonly object locker = new object();
		private static ChebiModel chebiModel;

		public static ChebiModel ChebiModel{
			get{
				lock (locker){
					if (chebiModel == null){
						string chebiFolder = Path.Combine(FileUtils.GetConfigPath(), "chebi");
						if (File.Exists(Path.Combine(chebiFolder, "chebiId_inchi.tsv.gz"))){
							chebiModel = ChebiParser.Parse(chebiFolder);
						}
					}
					return chebiModel;
				}
			}
		}

		private static readonly Molecule c13 = new Molecule("Cx");
		private static readonly Molecule n15 = new Molecule("Nx");
		private static readonly Molecule s34 = new Molecule("Sx");
		private static readonly Molecule s33 = new Molecule("Sy");
		private static readonly Molecule h2 = new Molecule("Hx");
		private static readonly Molecule h2X2 = new Molecule("Hx2");
		private static readonly Molecule h2X3 = new Molecule("Hx3");
		private static readonly Molecule h2X4 = new Molecule("Hx4");
		private static readonly Molecule h2X5 = new Molecule("Hx5");
		private static readonly Molecule h2X6 = new Molecule("Hx6");
		private static readonly Molecule h2X7 = new Molecule("Hx7");
		private static readonly Molecule cl37 = new Molecule("Clx");
		private static readonly Molecule fe56 = new Molecule("Fex");
		private static readonly Molecule fe57 = new Molecule("Fey");
		private static readonly Molecule c13X2 = new Molecule("Cx2");
		private static readonly Molecule c13X3 = new Molecule("Cx3");
		private static readonly Molecule c13X4 = new Molecule("Cx4");
		private static readonly Molecule c13X5 = new Molecule("Cx5");
		private static readonly Molecule c13X6 = new Molecule("Cx6");
		private static readonly Molecule c13X5N15 = new Molecule("Cx5Nx");
		private static readonly Molecule c13X5H2 = new Molecule("Cx5Hx");
		private static readonly Molecule c13X4H2X2 = new Molecule("Cx4Hx2");
		private static readonly Molecule c13X5O18 = new Molecule("Cx5Ox");
		private static readonly Molecule c13X6O18 = new Molecule("Cx6Ox");
		private static readonly Molecule c13X6Cl37 = new Molecule("Cx6Clx");
		private static readonly Molecule n15X2 = new Molecule("Nx2");
		private static readonly Molecule k41 = new Molecule("Kx");
		private static readonly Molecule k41X2 = new Molecule("Kx2");
		private static readonly Molecule o18 = new Molecule("Ox");
		private static readonly Molecule o18X2 = new Molecule("Ox2");
		private static readonly Molecule o17 = new Molecule("Oy");
		private static readonly Molecule c13Cl37 = new Molecule("CxClx");
		private static readonly Molecule c13O18 = new Molecule("CxOx");
		private static readonly Molecule c13K41 = new Molecule("CxKx");
		private static readonly Molecule c13S34 = new Molecule("CxSx");
		private static readonly Molecule c13N15 = new Molecule("CxNx");
		private static readonly Molecule c13H2 = new Molecule("CxHx");
		private static readonly Molecule cl37Fe56 = new Molecule("ClxFex");
		private static readonly Molecule cl37Fe57 = new Molecule("ClxFey");
		private static readonly Molecule cl37X2Fe56 = new Molecule("Clx2Fex");
		private static readonly Molecule cl37X2Fe57 = new Molecule("Clx2Fey");
		private static readonly Molecule cl37X3Fe56 = new Molecule("Clx3Fex");

		public static IsotopeCombination GetCombination(Molecule molecule){
			if (molecule.IsEmpty){
				return IsotopeCombination.Monoisotope;
			}
			if (molecule.Equals(c13)){
				return IsotopeCombination.C13;
			}
			if (molecule.Equals(n15)){
				return IsotopeCombination.N15X1;
			}
			if (molecule.Equals(h2)){
				return IsotopeCombination.H2;
			}
			if (molecule.Equals(h2X2)){
				return IsotopeCombination.H2_2;
			}
			if (molecule.Equals(h2X3)){
				return IsotopeCombination.H2_3;
			}
			if (molecule.Equals(h2X4)){
				return IsotopeCombination.H2_4;
			}
			if (molecule.Equals(h2X5)){
				return IsotopeCombination.H2_5;
			}
			if (molecule.Equals(h2X6)){
				return IsotopeCombination.H2_6;
			}
			if (molecule.Equals(h2X7)){
				return IsotopeCombination.H2_7;
			}
			if (molecule.Equals(cl37)){
				return IsotopeCombination.Cl37;
			}
			if (molecule.Equals(fe56)){
				return IsotopeCombination.Fe56X1;
			}
			if (molecule.Equals(fe57)){
				return IsotopeCombination.Fe57X1;
			}
			if (molecule.Equals(s34)){
				return IsotopeCombination.S34;
			}
			if (molecule.Equals(s33)){
				return IsotopeCombination.S33;
			}
			if (molecule.Equals(c13X2)){
				return IsotopeCombination.C13_2;
			}
			if (molecule.Equals(c13X3)){
				return IsotopeCombination.C13_3;
			}
			if (molecule.Equals(c13X4)){
				return IsotopeCombination.C13_4;
			}
			if (molecule.Equals(c13X5)){
				return IsotopeCombination.C13_5;
			}
			if (molecule.Equals(c13X6)){
				return IsotopeCombination.C13_6;
			}
			if (molecule.Equals(c13X5O18)){
				return IsotopeCombination.C13X5O18X1;
			}
			if (molecule.Equals(c13X5N15)){
				return IsotopeCombination.C13X5N15X1;
			}
			if (molecule.Equals(c13X5H2)){
				return IsotopeCombination.C13X5H2X1;
			}
			if (molecule.Equals(c13X4H2X2)){
				return IsotopeCombination.C13X4H2X2;
			}
			if (molecule.Equals(c13X6O18)){
				return IsotopeCombination.C13X6O18X1;
			}
			if (molecule.Equals(c13X6Cl37)){
				return IsotopeCombination.C13X6Cl37X1;
			}
			if (molecule.Equals(n15X2)){
				return IsotopeCombination.N15X2;
			}
			if (molecule.Equals(o18)){
				return IsotopeCombination.O18;
			}
			if (molecule.Equals(o18X2)){
				return IsotopeCombination.O18_2;
			}
			if (molecule.Equals(o17)){
				return IsotopeCombination.O17;
			}
			if (molecule.Equals(c13S34)){
				return IsotopeCombination.C13X1S34X1;
			}
			if (molecule.Equals(c13O18)){
				return IsotopeCombination.C13X1O18X1;
			}
			if (molecule.Equals(c13Cl37)){
				return IsotopeCombination.C13X1Cl37X1;
			}
			if (molecule.Equals(c13N15)){
				return IsotopeCombination.C13X1N15X1;
			}
			if (molecule.Equals(c13H2)){
				return IsotopeCombination.C13X1H2X1;
			}
			if (molecule.Equals(k41)){
				return IsotopeCombination.K41;
			}
			if (molecule.Equals(c13K41)){
				return IsotopeCombination.C13X1K41X1;
			}
			if (molecule.Equals(k41X2)){
				return IsotopeCombination.K41X2;
			}
			if (molecule.Equals(cl37Fe56)){
				return IsotopeCombination.Cl37X1Fe56X1;
			}
			if (molecule.Equals(cl37Fe57)){
				return IsotopeCombination.Cl37X1Fe57X1;
			}
			if (molecule.Equals(cl37X2Fe56)){
				return IsotopeCombination.Cl37X2Fe56X1;
			}
			if (molecule.Equals(cl37X2Fe57)){
				return IsotopeCombination.Cl37X2Fe57X1;
			}
			if (molecule.Equals(cl37X3Fe56)){
				return IsotopeCombination.Cl37X3Fe56X1;
			}
			throw new Exception("Unknown isotopic combination: " + molecule.GetEmpiricalFormula(false));
		}

		public static Molecule GetMolecule(IsotopeCombination ic){
			switch (ic){
				case IsotopeCombination.Monoisotope:
					return Molecule.empty;
				case IsotopeCombination.C13:
					return c13;
				case IsotopeCombination.N15X1:
					return n15;
				case IsotopeCombination.H2:
					return h2;
				case IsotopeCombination.H2_6:
					return h2X6;
				case IsotopeCombination.Cl37:
					return cl37;
				case IsotopeCombination.Fe56X1:
					return fe56;
				case IsotopeCombination.Fe57X1:
					return fe57;
				case IsotopeCombination.S34:
					return s34;
				case IsotopeCombination.S33:
					return s33;
				case IsotopeCombination.C13_2:
					return c13X2;
				case IsotopeCombination.C13_3:
					return c13X3;
				case IsotopeCombination.C13_4:
					return c13X4;
				case IsotopeCombination.C13_5:
					return c13X5;
				case IsotopeCombination.C13_6:
					return c13X6;
				case IsotopeCombination.C13X5O18X1:
					return c13X5O18;
				case IsotopeCombination.C13X5N15X1:
					return c13X5N15;
				case IsotopeCombination.C13X5H2X1:
					return c13X5H2;
				case IsotopeCombination.C13X6O18X1:
					return c13X6O18;
				case IsotopeCombination.C13X6Cl37X1:
					return c13X6Cl37;
				case IsotopeCombination.N15X2:
					return n15X2;
				case IsotopeCombination.O18:
					return o18;
				case IsotopeCombination.O18_2:
					return o18X2;
				case IsotopeCombination.O17:
					return o17;
				case IsotopeCombination.C13X1S34X1:
					return c13S34;
				case IsotopeCombination.C13X1O18X1:
					return c13O18;
				case IsotopeCombination.C13X1Cl37X1:
					return c13Cl37;
				case IsotopeCombination.C13X1N15X1:
					return c13N15;
				case IsotopeCombination.C13X1H2X1:
					return c13H2;
				case IsotopeCombination.K41:
					return k41;
				case IsotopeCombination.C13X1K41X1:
					return c13K41;
				case IsotopeCombination.K41X2:
					return k41X2;
				case IsotopeCombination.Cl37X1Fe56X1:
					return cl37Fe56;
				case IsotopeCombination.Cl37X1Fe57X1:
					return cl37Fe57;
				case IsotopeCombination.Cl37X2Fe56X1:
					return cl37X2Fe56;
				case IsotopeCombination.Cl37X2Fe57X1:
					return cl37X2Fe57;
				case IsotopeCombination.Cl37X3Fe56X1:
					return cl37X3Fe56;
				default:
					throw new Exception("Never get here.");
			}
		}
	}
}