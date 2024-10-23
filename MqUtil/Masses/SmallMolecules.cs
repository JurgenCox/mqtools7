using MqApi.Num;
using MqUtil.Mol;

namespace MqUtil.Masses {
	public static class SmallMolecules {
		private static SmallMoleculeCluster[] compositionsOrig;
		private static SmallMoleculeCluster[] compositionsCompleted;

		private static void Init() {
			compositionsOrig = InitCompositions(false);
			compositionsCompleted = InitCompositions(true);
		}

		/// <summary>
		/// Get all library m/z values and their correponding charges for the given polarity (positive = true/false).
		/// </summary>
		/// <param name="positive">Whether the library for positive or negative polarity is wanted.</param>
		/// <param name="smallMolecules">Returns objects containing the elemental compositions corresponding to the 
		/// m/z values.</param>
		/// <param name="charges">Returns the charges of the library peaks.</param>
		/// <param name="complete">Whether automatically completed peaks, e.g. from CheBI or additional isotopes,
		///  should be added.</param>
		/// <returns>Sorted m/z values in ascending order of library peaks.</returns>
		public static double[] GetData(bool positive, out SmallMolecule[] smallMolecules, out byte[] charges, bool complete) {
			double[] m1 = GetData(1, positive, out SmallMolecule[] sm1, complete);
			double[] m2 = GetData(2, positive, out SmallMolecule[] sm2, complete);
			SmallMolecule[] sm = ArrayUtils.Concat(sm1, sm2);
			double[] m = ArrayUtils.Concat(m1, m2);
			byte[] ch = new byte[m1.Length + m2.Length];
			for (int i = 0; i < m1.Length; i++) {
				ch[i] = 1;
			}
			for (int i = 0; i < m2.Length; i++) {
				ch[m1.Length + i] = 2;
			}
			int[] o = ArrayUtils.Order(m);
			charges = ArrayUtils.SubArray(ch, o);
			smallMolecules = ArrayUtils.SubArray(sm, o);
			return ArrayUtils.SubArray(m, o);
		}

		private static double[] GetData(byte charge, bool positive, out SmallMolecule[] smallMolecules, bool complete) {
			if (compositionsOrig == null) {
				Init();
			}
			List<double> mzX = new List<double>();
			List<SmallMolecule> molX = new List<SmallMolecule>();
			foreach (SmallMoleculeCluster m1 in complete ? compositionsCompleted : compositionsOrig) {
				foreach (SmallMolecule m in m1.SmallMolecules) {
					if (positive) {
						if (m1.HasPositiveCharge(charge)) {
							mzX.Add(m.GetPositiveMz(charge));
							molX.Add(m);
						}
					} else {
						if (m1.HasNegativeCharge(charge)) {
							mzX.Add(m.GetNegativeMz(charge));
							molX.Add(m);
						}
					}
				}
			}
			double[] mz = mzX.ToArray();
			SmallMolecule[] mol = molX.ToArray();
			int[] o0 = ArrayUtils.Order(mz);
			mz = ArrayUtils.SubArray(mz, o0);
			mol = ArrayUtils.SubArray(mol, o0);
			smallMolecules = mol;
			return mz;
		}

		private static SmallMoleculeCluster[] InitCompositions(bool complete) {
			Dictionary<int, List<SmallMoleculeCluster>> result = MoleculeListParser.Parse(complete, complete);
			Dictionary<string, SmallMoleculeCluster> x = new Dictionary<string, SmallMoleculeCluster>();
			foreach (KeyValuePair<int, List<SmallMoleculeCluster>> pair in result) {
				foreach (SmallMoleculeCluster cluster in pair.Value) {
					string comp = cluster.SmallMolecules[0].Composition;
					x.Add(comp, cluster);
				}
			}
			if (complete) {
				InitChebi(x, true);
			}
			return x.Values.ToArray();
		}

		private static void InitChebi(IDictionary<string, SmallMoleculeCluster> x, bool completeIsotopes) {
			foreach (string comp in MassUtil.ChebiModel.Compositions) {
				if (comp.Contains("-")) {
					continue;
				}
				Molecule m = new Molecule(comp);
				if (m.CountAtoms("T") > 0) {
					continue;
				}
				if (!x.ContainsKey(comp)) {
					x.Add(comp, new SmallMoleculeCluster(comp, new[] {-1, 1}, completeIsotopes, false) {FromChebi = true});
				}
			}
		}
	}
}