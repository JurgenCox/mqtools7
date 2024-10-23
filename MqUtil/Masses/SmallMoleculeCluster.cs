using MqApi.Num;
using MqUtil.Mol;

namespace MqUtil.Masses {
	public class SmallMoleculeCluster {
		public bool FromChebi { get; set; } = false;
		public SmallMolecule[] SmallMolecules { get; set; }
		public bool[] moleculeIsCompleted;
		public int[] PositiveCharges { get; set; }
		public int[] NegativeCharges { get; set; }
		internal bool PositiveChargeCompleted { get; }
		internal bool NegativeChargeCompleted { get; }

		public SmallMoleculeCluster(string composition, int[] charges, bool completeIsotopes, bool completeCharges) : this(
			new[] {composition}, charges, completeIsotopes, completeCharges) { }

		public SmallMoleculeCluster(string[] compositions, int[] charges, bool completeIsotopes, bool completeCharges) {
			PositiveCharges = GetPositiveCharges(charges);
			NegativeCharges = GetNegativeCharges(charges);
			if (completeCharges) {
				if (PositiveCharges.Length == 0) {
					PositiveCharges = new[] {1};
					PositiveChargeCompleted = true;
				}
				if (NegativeCharges.Length == 0) {
					NegativeCharges = new[] {1};
					NegativeChargeCompleted = true;
				}
			}
			compositions = Complete(compositions, out moleculeIsCompleted, completeIsotopes);
			SmallMolecules = new SmallMolecule[compositions.Length];
			for (int i = 0; i < SmallMolecules.Length; i++) {
				SmallMolecules[i] = new SmallMolecule(compositions[i], this, moleculeIsCompleted[i]);
			}
		}

		public int[] GetCharges() {
			int[] neg = new int[NegativeCharges.Length];
			for (int i = 0; i < neg.Length; i++) {
				neg[i] = -NegativeCharges[i];
			}
			return ArrayUtils.Concat(neg, PositiveCharges);
		}

		public string[] GetCompositions() {
			double[] masses = new double[SmallMolecules.Length];
			for (int i = 0; i < masses.Length; i++) {
				masses[i] = SmallMolecules[i].GetNeutralMass();
			}
			int[] o = ArrayUtils.Order(masses);
			string[] result = new string[masses.Length];
			for (int i = 0; i < result.Length; i++) {
				result[i] = SmallMolecules[o[i]].Composition;
			}
			return result;
		}

		private static int[] GetPositiveCharges(int[] charges) {
			HashSet<int> result = new HashSet<int>();
			foreach (int charge in charges) {
				if (charge > 0) {
					result.Add(charge);
				}
			}
			int[] x = result.ToArray();
			Array.Sort(x);
			return x;
		}

		private static int[] GetNegativeCharges(int[] charges) {
			HashSet<int> result = new HashSet<int>();
			foreach (int charge in charges) {
				if (charge < 0) {
					result.Add(Math.Abs(charge));
				}
			}
			int[] x = result.ToArray();
			Array.Sort(x);
			return x;
		}

		public bool HasPositiveCharge(byte c) {
			return Array.BinarySearch(PositiveCharges, c) >= 0;
		}

		public bool HasNegativeCharge(byte c) {
			return Array.BinarySearch(NegativeCharges, c) >= 0;
		}

		private static string[] Complete(string[] compositions, out bool[] moleculeIsCompleted, bool complete) {
			if (compositions[0].Contains("-")) {
				moleculeIsCompleted = new bool[compositions.Length];
				return compositions;
			}
			if (!complete) {
				moleculeIsCompleted = new bool[compositions.Length];
				return compositions;
			}
			HashSet<Molecule> origMolecules = new HashSet<Molecule>();
			List<string> result = new List<string>();
			List<bool> completed = new List<bool>();
			foreach (string s in compositions) {
				origMolecules.Add(new Molecule(s));
				result.Add(s);
				completed.Add(false);
			}
			Molecule m = new Molecule(compositions[0]).GetUnlabeledVersion();
			if (m.GetCCount() > 1) {
				Add(result, completed, m, IsotopeCombination.C13, origMolecules);
			}
			if (m.GetHCount() > 1) {
				//Add(result, completed, m, IsotopeCombination.H2, origMolecules);
			}
			if (m.CountAtoms("N") > 1) {
				//Add(result, completed, m, IsotopeCombination.N15X1, origMolecules);
			}
			if (m.GetCCount() > 2) {
				Add(result, completed, m, IsotopeCombination.C13_2, origMolecules);
			}
			if (m.CountAtoms("N") > 1 && m.GetCCount() > 1) {
				//Add(result, completed, m, IsotopeCombination.C13X1N15X1, origMolecules);
			}
			if (m.GetOCount() > 1) {
				Add(result, completed, m, IsotopeCombination.O18, origMolecules);
			}
			if (m.GetSCount() > 0) {
				Add(result, completed, m, IsotopeCombination.S34, origMolecules);
				//Add(result, completed, m, IsotopeCombination.S33, origMolecules);
			}
			if (m.GetCCount() > 3) {
				Add(result, completed, m, IsotopeCombination.C13_3, origMolecules);
			}
			if (m.GetCCount() > 1 && m.GetOCount() > 1) {
				//Add(result, completed, m, IsotopeCombination.C13X1O18X1, origMolecules);
			}
			if (m.GetCCount() > 4) {
				//Add(result, completed, m, IsotopeCombination.C13_4, origMolecules);
			}
			if (m.GetClCount() > 0) {
				Add(result, completed, m, IsotopeCombination.Cl37, origMolecules);
			}
			moleculeIsCompleted = completed.ToArray();
			return result.ToArray();
		}

		private static void Add(List<string> result, List<bool> completed, Molecule molecule, IsotopeCombination ic,
			HashSet<Molecule> origMolecules) {
			Molecule l = MassUtil.GetMolecule(ic);
			Molecule n = l.GetUnlabeledVersion();
			Molecule toBeAdded = Molecule.Sum(Molecule.Subtract(molecule, n), l);
			if (!origMolecules.Contains(toBeAdded)) {
				result.Add(toBeAdded.GetEmpiricalFormula());
				completed.Add(true);
			}
		}
	}
}