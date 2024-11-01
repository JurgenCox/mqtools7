using System.Text;
using MqApi.Num;
using MqApi.Util;
using MqUtil.Data;
using MqUtil.Mol;
using MqUtil.Ms.Data;

namespace MqUtil.Ms.Search {
	public class AndromedaMonoPeptide {
		public string Sequence { get; internal set; }
		public int[] ProteinIndices { get; internal set; }

		/// <summary>
		/// 0 = not mutated; if > 0 it encodes the class of mutation. 
		/// </summary>
		public byte[] IsMutated { get; internal set; }

		public bool Proteogenomic { get; internal set; }
		public BasePeptides BasePeptides { get; internal set; }
		public PeptideModificationState Modifications { get; set; }

		/// <summary>
		/// Used for non-cleavable cross linked peptides to quantify how well each of the linear peptides is identified.
		/// </summary>
		public double PartialScore { get; set; }

		public int Nmatches { get; internal set; }
		public int FragOverlap { get; internal set; }


		public AndromedaMonoPeptide(string sequence, string modLine, string protLine, string proteogenomic,
			ProteinSet proteinSet, double partialScore, int nmatches, int fragOverlap) : this(sequence,
			ParsePeptideModificationState(sequence, modLine), ParseProteins(protLine, p => proteinSet?.GetIndex(p) ?? -1),
			bool.Parse(proteogenomic), partialScore, nmatches, fragOverlap) { }

		public AndromedaMonoPeptide(string sequence, PeptideModificationState modifications,
			(int[] index, (byte isMutated, string basePeptide, int i)[] basePeptides) proteins, bool proteogenomic,
			double partialScore, int nmatches, int fragOverlap) {
			Sequence = sequence;
			Modifications = modifications;
			IsMutated = new byte[proteins.index.Length];
			BasePeptides = new BasePeptides();
			foreach (var (isMutated, basePeptide, i) in proteins.basePeptides) {
				BasePeptides[i] = basePeptide;
				IsMutated[i] = isMutated;
			}
			ProteinIndices = proteins.index;
			Proteogenomic = proteogenomic;
			PartialScore = partialScore;
			Nmatches = nmatches;
			FragOverlap = fragOverlap;
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.Append(string.Join("\t", Sequence, Modifications));
			sb.Append('\t');
			sb.Append(string.Join(";", ProteinIndices));
			sb.Append('\t');
			bool hasPrevious = false;
			for (int i = 0; i < IsMutated.Length; i++) {
				byte isMutated = IsMutated[i];
				if (isMutated > 0) {
					string basePeptide = BasePeptides[i];
					if (hasPrevious) {
						sb.Append(';');
					}
					sb.AppendFormat("{0},{1},{2}", isMutated, basePeptide, i);
					hasPrevious = true;
				}
			}
			sb.Append('\t');
			sb.Append(string.Join("\t", Proteogenomic, PartialScore, Nmatches, FragOverlap));
			return sb.ToString();
		}

		public static AndromedaMonoPeptide FromString(string line) {
			return FromString(line, out _);
		}

		public static AndromedaMonoPeptide FromString(string line, out string[] trailingTokens) {
			string[] tokens = line.Split('\t');
			string sequence = tokens[0];
			PeptideModificationState modifications = new PeptideModificationState(tokens[1]);
			int[] proteinIndex = tokens[2].Split(';').Select(int.Parse).ToArray();
			(byte, string, int)[] basePeptides = tokens[3].Split(';').Select(tup => {
				string[] items = tup.Split(',');
				byte isMutated = byte.Parse(items[0]);
				string basePeptide = items[1];
				int i = int.Parse(items[2]);
				return (isMutated, basePeptide, i);
			}).ToArray();
			bool proteogenomic = bool.Parse(tokens[4]);
			double partialScore = Parser.Double(tokens[5]);
			int nmatches = int.Parse(tokens[6]);
			int fragoverlap = int.Parse(tokens[7]);
			trailingTokens = tokens.SubArrayFrom(7);
			return new AndromedaMonoPeptide(sequence, modifications, (proteinIndex, basePeptides), proteogenomic, partialScore,
				nmatches, fragoverlap);
		}

		private static (string protein, string basePeptide, byte isMutated) ParseProteinDescription(string description) {
			string protein = description;
			string basePeptide = string.Empty;
			byte isMutated = 0;
			int ind = description.LastIndexOf(':');
			if (description.Length > 2 && ind >= 0) {
				string mut = description.Substring(ind + 1);
				isMutated = mut.StartsWith("M") ? (byte) 1 : (byte) 0;
				if (isMutated > 0) {
					int ind2 = mut.LastIndexOf('!');
					if (ind2 >= 0) {
						isMutated = byte.Parse(mut.Substring(1, ind2 - 1));
						basePeptide = mut.Substring(ind2 + 1);
					}
				}
				protein = description.Substring(0, ind); // Extract name from mutation description.
			}
			return (protein, basePeptide, isMutated);
		}

		private static PeptideModificationState ParsePeptideModificationState(string sequence, string modLine) {
			return string.IsNullOrEmpty(modLine)
				? new PeptideModificationState(sequence.Length)
				: new PeptideModificationState(modLine);
		}

		/// <summary>
		/// Parse protein string in the form of "Protein:N";"MutatedProtein:M123!BASEPEPTIDE" into indexed data.
		/// Reverses <see cref="AndromedaAnalysisResults.CombinedProteinNames"/>.
		/// </summary>
		/// <param name="protLine"></param>
		/// <param name="getIndex">Usually ProteinSet.GetIndex</param>
		/// <returns></returns>
		public static (int[] index, (byte IsMutated, string basePeptide, int i)[] basePeptides) ParseProteins(string protLine,
			Func<string, int> getIndex) {
			string[] proteins = protLine.StartsWith("\"")
				? protLine.Substring(1, protLine.Length - 2).Split(new[] {"\";\""}, StringSplitOptions.None)
				: protLine.Split(new[] {";"}, StringSplitOptions.None);
			int[] proteinIndex = new int[proteins.Length];
			List<(byte, string, int)> basePeptides = new List<(byte, string, int)>();
			for (int i = 0; i < proteins.Length; i++) {
				string protein = proteins[i];
				(string name, string basePeptide, byte isMutated) = ParseProteinDescription(protein);
				int index = getIndex(name);
				proteinIndex[i] = index;
				if (isMutated > 0) {
					basePeptides.Add((isMutated, basePeptide, i));
				}
			}
			return (proteinIndex, basePeptides.ToArray());
		}

		public byte IsAlwaysMutated => ArrayUtils.Min(IsMutated);

		public Molecule GetFixedModMolecule(Modification[] fixMod) {
			Molecule result = new Molecule();
			foreach (char c in Sequence) {
				Modification m = GetFixedMod(c, fixMod);
				if (m != null) {
					Molecule mol = m.GetMolecule();
					result = Molecule.Sum(result, mol);
				}
			}
			return result;
		}

		private static Modification GetFixedMod(char c, IEnumerable<Modification> mod) {
			foreach (Modification m in mod) {
				for (int i = 0; i < m.AaCount; i++) {
					if (c == m.GetAaAt(i)) {
						return m;
					}
				}
			}
			return null;
		}

		public Molecule CalcComposition(Modification[] fixMod) {
			Molecule m1 = AminoAcids.GetPeptideMolecule(Sequence);
			Molecule m2 = Modifications.RemoveNonCompositionMods().ProjectToCounts().GetMolecule();
			Molecule m3 = GetFixedModMolecule(fixMod);
			return Molecule.Sum(new[] {m1, m2, m3});
		}

		public AndromedaMonoPeptide(BinaryReader reader) {
			Sequence = reader.ReadString();
			Modifications = PeptideModificationState.Read(reader);
			ProteinIndices = FileUtils.ReadInt32Array(reader);
			IsMutated = FileUtils.ReadByteArray(reader);
			BasePeptides = new BasePeptides(reader);
			Proteogenomic = reader.ReadBoolean();
			PartialScore = reader.ReadDouble();
			Nmatches = reader.ReadInt32();
			FragOverlap = reader.ReadInt32();
		}

		public void Write(BinaryWriter writer) {
			writer.Write(Sequence);
			if (Modifications == null) {
				Modifications = new PeptideModificationState(Sequence.Length);
			}
			Modifications.Write(writer);
			FileUtils.Write(ProteinIndices, writer);
			FileUtils.Write(IsMutated, writer);
			BasePeptides.Write(writer);
			writer.Write(Proteogenomic);
			writer.Write(PartialScore);
			writer.Write(Nmatches);
			writer.Write(FragOverlap);
		}
	}
}