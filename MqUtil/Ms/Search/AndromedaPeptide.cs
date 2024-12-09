using MqApi.Util;
using MqUtil.Mol;
using MqUtil.Ms.Data;
using MqUtil.Ms.Enums;

namespace MqUtil.Ms.Search {
	public class AndromedaPeptide {
		/// <summary>
		/// If true, partial peptide scores will be used for the PEP calculation. 
		/// If false, the logarithm of the peptide length or min match counts will be used.
		/// </summary>
		private const bool usePartialScore = true;
		private const bool useLogLenXl = false;

		private const bool filterDipeptides = false;

		/// <summary>
		/// neutral peptide mass, not m/z 
		/// </summary>
		public double Mass { get; }

		public int Charge { get; }
		public double DeltaMass { get; set; }
		public double Pep { get; set; }
		public double Score { get; set; }
		public AndromedaMonoPeptide Peptide1 { get; set; }
		public int MinCountCross { get; }
		public AndromedaMonoPeptide Peptide2 { get; set; }
		public LinkPatternData LinkPatternData { get; set; }
		public DiaSpecType? DiaSpecType { get; }

		public AndromedaPeptide(AndromedaMonoPeptide peptide1, double mass, int charge, double deltaMass, double pep,
			double score, int minCountCross, AndromedaMonoPeptide peptide2, LinkPatternData linkPatternData,
			DiaSpecType? diaSpecType) {
			Mass = mass;
			Charge = charge;
			MinCountCross = minCountCross;
			DeltaMass = deltaMass;
			Pep = pep;
			Score = score;
			Peptide1 = peptide1;
			Peptide2 = peptide2;
			LinkPatternData = linkPatternData;
			DiaSpecType = diaSpecType;
		}

		// TODO merge with FromString, allowing for DIA + cross linking TODO Walter?
		public static AndromedaPeptide FromStringDia(string line, ProteinSet proteinSet) {
			string[] tokens = line.Split('\t');
			string sequence = tokens[0];
			double score = Parser.Double(tokens[1]);
			double mass = Parser.Double(tokens[2]);
			PeptideModificationState modificationState = new PeptideModificationState(tokens[3]);
			string proteinName = tokens[4];
			DiaSpecType specType = (DiaSpecType) Enum.Parse(typeof(DiaSpecType), tokens[5]);
			AndromedaMonoPeptide peptide1 = new AndromedaMonoPeptide(sequence, modificationState,
				AndromedaMonoPeptide.ParseProteins(proteinName, proteinSet.GetIndex), false, double.NaN, -1,-1);
			return new AndromedaPeptide(peptide1, mass, -1, double.NaN, 1, score, 0, null, null, specType);
		}

		// TODO must be able to parse regular peptide, xl peptide (format in AndromedaAnalysis) and dia peptide (possibly with xl as well, format in DiaUtils)
		public static AndromedaPeptide FromString(string line, ProteinSet proteinSet) {
			string[] w = line.Split('\t');
			bool isXlinked = w.Length == 23; // means that it is cross-linked peptide
			bool isMonoEitherLooplinked =
				w.Length == 17 ||
				w.Length == 16; // means that it is either monolinked or looplinked peptide - todo: update after the bug is fixed for the PartialScore implemented 
			double partialScore1 = double.NaN;
			double partialScore2 = double.NaN;
			int count1 = -1;
			int count2 = -1;
			int fragOverlap1 = -1;
			int fragOverlap2 = -1;
			int minCountCross = 0;
			if (isXlinked) {
				partialScore1 = Parser.Double(w[17]);
				partialScore2 = Parser.Double(w[18]);
				count1 = Parser.Int(w[19]);
				count2 = Parser.Int(w[20]);
				fragOverlap1 = Parser.Int(w[21]);
				fragOverlap2 = Parser.Int(w[22]);
				minCountCross = count1 > count2 ? count2 : count1;
			}
			AndromedaMonoPeptide peptide1 = new AndromedaMonoPeptide(w[0], w[4], w[5], w[7], proteinSet, partialScore1, count1, fragOverlap1);
			int pep = 1;
			double score = Parser.Double(w[1]);
			double mass = Parser.Double(w[2]);
			double deltaMass = Parser.Double(w[3]);
			int charge = int.Parse(w[6]);
			AndromedaMonoPeptide peptide2 = null;
			LinkPatternData linkPatternData = null;
			if (isXlinked) {
				if(!w[8].Equals("-")){
					peptide2 = new AndromedaMonoPeptide(w[8], w[9], w[10], w[11], proteinSet, partialScore2, count2, fragOverlap2);
				}
				linkPatternData = new LinkPatternData(w[12], w[13], w[14], w[15], w[16]);
			}
			if (isMonoEitherLooplinked) {
				linkPatternData = new LinkPatternData(w[8], w[9], w[10], w[11], w[12]);
			}
			return new AndromedaPeptide(peptide1, mass, charge, deltaMass, pep, score, minCountCross, peptide2, linkPatternData,
				null);
		}

		/// <summary>
		///  Return a second dimension used for PEP calculation
		///  A second dimension can be i) partial score, ii) loglen or iii) min matches 
		///  i) a partial score of a shorter peptide for diPeptides or 0 for a single peptide, 
		///  ii) log of the length of a peptide which is either single peptide or a shorter peptide in crosslinked peptide pair
		///  iii) min matches for only crosslink peptides, not single ones
		/// </summary>
		public double GetSecondDimension() {
			//return IsDipeptide  && !double.IsNaN(Peptide1.PartialScore)? Math.Min(Peptide1.PartialScore, Peptide2.PartialScore) : GetLogLen();
			return usePartialScore && !double.IsNaN(Peptide1.PartialScore) ? GetPartialScore() : (useLogLenXl ? GetLogLen(): MinCountCross);
		}

		private double GetLogLen() {
			return Math.Log(IsDipeptide ? Math.Min(Peptide1.Sequence.Length, Peptide2.Sequence.Length) : Peptide1.Sequence.Length);
		}

		private double GetPartialScore() {
			return IsDipeptide ? Math.Min(Peptide1.PartialScore, Peptide2.PartialScore) : 0;
		}

		/// <summary>
		///  Isobaric cross-linked peptides species are the two adjacent peptides.  
		///  This information is valuable because the mass of mono-linked peptides from Isobaric cross-linked peptides
		///  equals to the mass of cross-linked peptides from these.   
		/// </summary>
		/// <returns> true if two peptides are adjacent, otherwise false</returns>
		public bool IsIsobaricXlSpecies(ProteinSet proteinSet) {
			bool isIsobaricXlSpecies = false;
			if (Peptide2 == null) {
				return false;
			}
			// find information about protein 1
			int pind1 = Peptide1.ProteinIndices[0];
			string protId1 = proteinSet.GetName(pind1);
			string pro1Seq = proteinSet.Get(protId1).AaSequence;

			// find information about protein 2
			int pind2 = Peptide2.ProteinIndices[0];
			string protId2 = proteinSet.GetName(pind2);
			string pro2Seq = proteinSet.Get(protId2).AaSequence;

			// first, check if these two peptides come from the same protein
			if (protId1.Equals(protId2)) {
				return false;
			}
			// otherswise, start updating based on protein positions
			int firstIndexSequenceA =
				pro1Seq.IndexOf(Peptide1.Sequence,
					StringComparison.InvariantCulture); //todo: question how does it handle with repeated sequences
			int firstIndexSequenceB = pro2Seq.IndexOf(Peptide2.Sequence, StringComparison.InvariantCulture);
			if (firstIndexSequenceA < firstIndexSequenceB) {
				int lastIndexSequenceA = firstIndexSequenceA + Peptide1.Sequence.Length;
				if (lastIndexSequenceA + 1 == firstIndexSequenceB) {
					isIsobaricXlSpecies = true;
				}
			} else if (firstIndexSequenceB < firstIndexSequenceA) {
				int lastIndexSequenceB = firstIndexSequenceB + Peptide1.Sequence.Length;
				if (lastIndexSequenceB + 1 == firstIndexSequenceA) {
					isIsobaricXlSpecies = true;
				}
			}
			return isIsobaricXlSpecies;
		}

		public byte IsAlwaysMutated => Peptide2 == null
			? Peptide1.IsAlwaysMutated
			: Math.Max(Peptide1.IsAlwaysMutated, Peptide2.IsAlwaysMutated);

		public bool Proteogenomic {
			get {
				if (Peptide2 == null) {
					return Peptide1.Proteogenomic;
				}
				return Peptide1.Proteogenomic || Peptide2.Proteogenomic;
			}
		}

		public AndromedaPeptide(BinaryReader reader) {
			Mass = reader.ReadDouble();
			DeltaMass = reader.ReadDouble();
			Charge = reader.ReadInt32();
			Score = reader.ReadDouble();
			Pep = reader.ReadDouble();
			bool isDipeptide = reader.ReadBoolean();
			bool isMonoEitherLooplinked = reader.ReadBoolean();
			Peptide1 = new AndromedaMonoPeptide(reader);
			if (isDipeptide) {
				Peptide2 = new AndromedaMonoPeptide(reader);
				LinkPatternData = LinkPatternData.FromBinary(reader);
				MinCountCross = Peptide1.Nmatches > Peptide2.Nmatches ? Peptide2.Nmatches : Peptide1.Nmatches;
			}
			if (isMonoEitherLooplinked) {
				LinkPatternData = LinkPatternData.FromBinary(reader);
				MinCountCross = Peptide1.Nmatches;
			}
		}

		public double MassErrorPpm => DeltaMass / Mass * 1e6;
		public bool IsDipeptide => Peptide2 != null;

		public bool IsMonolinkedPeptide => Peptide2 == null && LinkPatternData != null &&
		                                   (LinkPatternData?.UnsaturatedLinks1 != "-");

		public bool IsLooplinkedPeptide => Peptide2 == null && LinkPatternData != null &&
		                                   (LinkPatternData?.IntraLinks1 != "-");

		public bool IsDipeptide2 => !filterDipeptides || IsDipeptide;

		public PeptideModificationState GetTrueModifications() {
			return Peptide1.Modifications.GetTrueModifications();
		}

		public void Write(BinaryWriter writer) {
			writer.Write(Mass);
			writer.Write(DeltaMass);
			writer.Write(Charge);
			writer.Write(Score);
			writer.Write(Pep);
			bool isDipeptide = Peptide2 != null;
			bool isMonoEitherLooplinked = !isDipeptide && LinkPatternData != null;
			// TODO: Must be updated maybe for MS3cleaved cross ids?
			writer.Write(isDipeptide);
			writer.Write(isMonoEitherLooplinked);
			Peptide1.Write(writer);
			if (isDipeptide) {
				Peptide2.Write(writer);
				LinkPatternData.Write(writer);
			}
			if (isMonoEitherLooplinked) {
				LinkPatternData.Write(writer);
			}
		}

		public bool FitsAaCounts(char[] aa, int[] count, HashSet<char> conflictingAas){
			return FitsAaCounts(aa, count, conflictingAas, Peptide1.Sequence);
		}

		public static bool FitsAaCounts(char[] aa, int[] count, HashSet<char> conflictingAas, string sequence) {
			for (int i = 0; i < aa.Length; i++) {
				if (conflictingAas.Contains(aa[i])) {
					continue;
				}
				if (!FitsAaCount(aa[i], count[i], sequence)) {
					return false;
				}
			}
			return true;
		}

		private static bool FitsAaCount(char aa, int count, string sequence) {
			return count == GetAaCount(aa, sequence);
		}
		public static int GetAaCount(char aa, string sequence) {
			int c = 0;
			foreach (char t in sequence) {
				if (t == aa) {
					c++;
				}
			}
			return c;
		}


		public bool IsForward(ProteinSet proteinSet) {
			return HasAForwardHit(proteinSet);
		}

		public bool IsForward(bool[] isDecoy) {
			return HasAForwardHit(isDecoy);
		}

		public bool HasAForwardHit(ProteinSet proteinSet) {
			foreach (int t1 in Peptide1.ProteinIndices) {
				if (!proteinSet.GetIsDecoy(t1)) {
					if (Peptide2 != null) {
						foreach (int t2 in Peptide2.ProteinIndices) {
							if (!proteinSet.GetIsDecoy(t2)) {
								return true;
							}
						}
					} else {
						return true;
					}
				}
			}
			return false;
		}

		public bool HasAForwardHit(bool[] isDecoy) {
			foreach (int t1 in Peptide1.ProteinIndices) {
				if (!isDecoy[t1]) {
					if (Peptide2 != null) {
						foreach (int t2 in Peptide2.ProteinIndices) {
							if (!isDecoy[t2]) {
								return true;
							}
						}
					} else {
						return true;
					}
				}
			}
			return false;
		}

		public bool IsHalfDecoy(ProteinSet proteinSet) {
			return HasAHalfDecoyHit(proteinSet);
		}
		public bool IsBothDecoy(ProteinSet proteinSet) {
			return HasBothDecoyHit(proteinSet);
		}

        private bool HasAHalfDecoyHit(ProteinSet proteinSet) {
			bool isTargetA = false;
			bool isTargetB = false;
			foreach (int t1 in Peptide1.ProteinIndices) {
				if (!proteinSet.GetIsDecoy(t1)) {
					isTargetA = true;
				}
			}
			if (Peptide2 != null) {
				foreach (int t2 in Peptide2.ProteinIndices) {
					if (!proteinSet.GetIsDecoy(t2)) {
						isTargetB = true;
					}
				}
				if (isTargetA && !isTargetB) {
					return true;
				}
				if (!isTargetA && isTargetB) {
					return true;
				}
			}
			return false;
		}
		private bool HasBothDecoyHit(ProteinSet proteinSet) {
			bool isTargetA = false;
			bool isTargetB = false;
			foreach (int t1 in Peptide1.ProteinIndices) {
				if (!proteinSet.GetIsDecoy(t1)) {
					isTargetA = true;
				}
			}
			if (Peptide2 != null) {
				foreach (int t2 in Peptide2.ProteinIndices) {
					if (!proteinSet.GetIsDecoy(t2)) {
						isTargetB = true;
					}
				}
				if (!isTargetA && !isTargetB) {
					return true;
				}
			}
			return false;
		}

        public string[] GetProteinIds(ProteinSet proteinSet) {
			string[] result = new string[Peptide1.ProteinIndices.Length];
			for (int i = 0; i < result.Length; i++) {
				result[i] = proteinSet.GetName(Peptide1.ProteinIndices[i]);
			}
			return result;
		}

		public int CalcLabelIndex(char[] aas, byte multiplicity, string[][] labels) {
			foreach (char aa in aas) {
				int ind = PeptideModificationState.CalcLabelIndex(aa, Peptide1.Modifications, Peptide1.Sequence, multiplicity, labels);
				if (ind != -1) {
					return ind;
				}
			}
			int ind1 = PeptideModificationState.CalcLabelIndexNterm(Peptide1.Modifications, multiplicity, labels);
			if (ind1 >= 0) {
				return ind1;
			}
			ind1 = PeptideModificationState.CalcLabelIndexCterm(Peptide1.Modifications, multiplicity, labels);
			if (ind1 >= 0) {
				return ind1;
			}
			return -1;
		}

		public void Dispose() {
			//Peptide1.Sequence = null;
			//if (Peptide1.Modifications != null) {
			//	Peptide1.Modifications.Dispose();
			//	Peptide1.Modifications = null;
			//}
			//Peptide1.ProteinIndex = null;
			//if (Peptide2 != null) {
			//	Peptide2.Sequence = null;
			//	if (Peptide2.Modifications != null) {
			//		Peptide2.Modifications.Dispose();
			//		Peptide2.Modifications = null;
			//	}
			//	Peptide2.ProteinIndex = null;
			//}
		}

		public bool ValidModifications() {
			PeptideModificationCounts counts = Peptide1.Modifications.ProjectToCounts();
			ushort[] c = counts.ModificationCounts;
			foreach (ushort cc in c) {
				if (cc > 7) {
					return false;
				}
			}
			return true;
		}

		public Molecule CalcComposition(Modification[] fixMod) {
			return Peptide1.CalcComposition(fixMod);
		}

		public override string ToString() {
			return Peptide1.Sequence + ";" + Score + ";" + Charge;
		}
	}
}