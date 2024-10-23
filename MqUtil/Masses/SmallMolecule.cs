using MqApi.Util;
using MqUtil.Mol;

namespace MqUtil.Masses {
	public class SmallMolecule {
		/// <summary>
		/// Atomic composition of the neutral molecule, not containing negative H.
		/// </summary>
		private readonly string composition;

		private readonly double neutralMass;
		private readonly byte negativeH;
		public SmallMoleculeCluster cluster;
		public bool isCompleted;
		public SmallMolecule(string composition) : this(composition, null, false) { }

		public SmallMolecule(string composition, SmallMoleculeCluster cluster, bool isCompleted) {
			this.cluster = cluster;
			this.isCompleted = isCompleted;
			if (composition.Contains("-")) {
				int ind = composition.IndexOf('-');
				this.composition = composition.Substring(0, ind);
				string remainder = composition.Substring(ind + 1);
				if (string.IsNullOrEmpty(remainder)) {
					throw new Exception("Illegal composition:" + remainder);
				}
				if (!remainder.StartsWith("H")) {
					throw new Exception("Illegal composition:" + remainder);
				}
				string q = remainder.Substring(1).Trim();
				if (q.Length == 0) {
					negativeH = 1;
				} else {
					if (!Parser.TryInt(q, out int w)) {
						throw new Exception("Illegal composition:" + remainder);
					}
					negativeH = (byte) w;
				}
			} else {
				this.composition = composition;
			}
			Molecule mol = new Molecule(this.composition);
			this.composition = mol.GetEmpiricalFormula(false);
			neutralMass = mol.MonoIsotopicMass;
			if (negativeH > 0) {
				neutralMass -= negativeH * Molecule.massH;
			}
		}

		public SmallMolecule GetBaseMolecule() {
			Molecule baseMolecule = new Molecule(composition).GetUnlabeledVersion();
			string str = baseMolecule.GetEmpiricalFormula();
			if (negativeH == 1) {
				str = str + "-H";
			}
			if (negativeH > 1) {
				str = str + "-" + negativeH + "H";
			}
			return new SmallMolecule(str, cluster, false);
		}

		public string Composition {
			get {
				if (negativeH == 0) {
					return composition;
				}
				if (negativeH == 1) {
					return composition + "-H";
				}
				return composition + "-" + negativeH + "H";
			}
		}

		public bool IsIsotopicLabel {
			get {
				Molecule m = new Molecule(composition);
				return m.IsIsotopicLabel;
			}
		}

		public int NominalMass => new Molecule(composition).NominalMass - negativeH;

		public double GetPositiveMz(byte c) {
			return neutralMass / c + Molecule.massProton;
		}

		public double GetNegativeMz(byte c) {
			return neutralMass / c - Molecule.massProton;
		}

		public double GetMz(byte c, bool positive) {
			return positive ? GetPositiveMz(c) : GetNegativeMz(c);
		}

		public string GetComposition(byte c, bool positiveMode) {
			Molecule mol = new Molecule(composition);
			if (positiveMode) {
				switch (c) {
					case 1: return negativeH == 1 ? composition : Molecule.Sum(mol, new Molecule("H")).GetEmpiricalFormula(false);
					case 2:
						switch (negativeH) {
							case 2: return composition;
							case 1: return Molecule.Sum(mol, new Molecule("H")).GetEmpiricalFormula(false);
							default: return Molecule.Sum(mol, new Molecule("H2")).GetEmpiricalFormula(false);
						}
					default: return "";
				}
			}
			switch (c) {
				case 1: return Molecule.Subtract(mol, new Molecule("H")).GetEmpiricalFormula(false);
				case 2: return Molecule.Subtract(mol, new Molecule("H2")).GetEmpiricalFormula(false);
				default: return "";
			}
		}

		public double GetNeutralMass() {
			return neutralMass;
		}

		public override bool Equals(object obj) {
			if (!(obj is SmallMolecule)) {
				return false;
			}
			return Equals((SmallMolecule) obj);
		}

		protected bool Equals(SmallMolecule other) {
			return string.Equals(composition, other.composition) && negativeH == other.negativeH;
		}

		public override int GetHashCode() {
			unchecked {
				return ((composition != null ? composition.GetHashCode() : 0) * 397) ^ negativeH.GetHashCode();
			}
		}
	}
}