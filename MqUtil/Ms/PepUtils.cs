﻿using MqApi.Num;
using MqUtil.Data;
using MqUtil.Mol;
using MqUtil.Ms.Data;
namespace MqUtil.Ms
{
    public static class PepUtils{
		public static (Modification2[][][] dependentMods, bool hasDependentMods) CreateDependentMods(
			IList<Modification2[]> lMods, Modification[] vMods){
			bool hasDependentMods = false;
			Modification2[][][] result = new Modification2[lMods.Count][][];
			for (int i = 0; i < result.Length; i++){
				result[i] = new Modification2[lMods[i].Length][];
				for (int j = 0; j < result[i].Length; j++){
					result[i][j] = CreateDependentMods(lMods[i][j], vMods);
					if (result[i][j].Length > 0){
						hasDependentMods = true;
					}
				}
			}
			return (result, hasDependentMods);
		}

		private static Modification2[] CreateDependentMods(Modification2 labelMod, IEnumerable<Modification> varMods){
			if (labelMod.IsIsotopicLabel){
				return new Modification2[0];
			}
			if (labelMod.IsCterminal){
				return GetCterminalMods(varMods);
			}
			if (labelMod.IsNterminal){
				return GetNterminalMods(varMods);
			}
			return labelMod.AaCount > 0 ? GetInternalMods(varMods, labelMod.GetAaAt(0)) : new Modification2[0];
		}

		private static Modification2[] GetInternalMods(IEnumerable<Modification> varMods, char aa){
			List<Modification2> result = new List<Modification2>();
			foreach (Modification mod in varMods){
				if (mod.IsInternal && Contains(mod.Sites, aa) && !mod.IsIsotopicMod){
					result.Add(new Modification2(mod));
				}
			}
			return result.ToArray();
		}

		private static bool Contains(IEnumerable<ModificationSite> sites, char aa){
			foreach (ModificationSite site in sites){
				if (site.Aa == aa){
					return true;
				}
			}
			return false;
		}

		private static Modification2[] GetNterminalMods(IEnumerable<Modification> varMods){
			List<Modification2> result = new List<Modification2>();
			foreach (Modification mod in varMods){
				if (mod.IsNterminal){
					result.Add(new Modification2(mod));
				}
			}
			return result.ToArray();
		}

		private static Modification2[] GetCterminalMods(IEnumerable<Modification> varMods){
			List<Modification2> result = new List<Modification2>();
			foreach (Modification mod in varMods){
				if (mod.IsCterminal){
					result.Add(new Modification2(mod));
				}
			}
			return result.ToArray();
		}

		private static void ClusterProteins(ref string[][] proteinIds, ref string[][] pepSeqs, ref byte[][] isMutated,
			bool splitTaxonomy, TaxonomyRank rank, ProteinSet proteinSet){
			int n = proteinIds.Length;
			string[] taxIds = null;
			if (splitTaxonomy){
				taxIds = new string[proteinIds.Length];
				TaxonomyItems taxonomyItems = TaxonomyItems.GetTaxonomyItems();
				for (int i = 0; i < taxIds.Length; i++){
					Protein prot = proteinSet.Get(proteinIds[i][0]);
					taxIds[i] = prot != null ? taxonomyItems.GetTaxonomyIdOfRank(prot.TaxonomyId, rank) : "-1";
				}
			}
			for (int i = 0; i < n; i++){
				int[] o = ArrayUtils.Order(pepSeqs[i]);
				pepSeqs[i] = pepSeqs[i].SubArray(o);
				if (isMutated != null){
					isMutated[i] = isMutated[i].SubArray(o);
				}
			}
			IndexedBitMatrix contains = new IndexedBitMatrix(n, n);
			for (int i = 0; i < n; i++){
				for (int j = 0; j < n; j++){
					if (i == j){
						continue;
					}
					contains.Set(i, j,
						(!splitTaxonomy || taxIds[i].Equals(taxIds[j])) && Contains(pepSeqs[i], pepSeqs[j]));
				}
			}
			int count;
			do{
				count = 0;
				int start = 0;
				while (true){
					int container = -1;
					int contained = -1;
					for (int i = start; i < proteinIds.Length; i++){
						container = GetContainer(i, contains);
						if (container != -1){
							contained = i;
							break;
						}
					}
					if (container == -1){
						break;
					}
					for (int i = 0; i < n; i++){
						contains.Set(i, contained, false);
						contains.Set(contained, i, false);
					}
					pepSeqs[contained] = new string[0];
					if (isMutated != null){
						isMutated[contained] = new byte[0];
					}
					proteinIds[container] = ArrayUtils.Concat(proteinIds[container], proteinIds[contained]);
					proteinIds[contained] = new string[0];
					start = contained + 1;
					count++;
				}
			} while (count > 0);
			List<int> valids = new List<int>();
			for (int i = 0; i < n; i++){
				if (pepSeqs[i].Length > 0){
					valids.Add(i);
				}
			}
			proteinIds = proteinIds.SubArray(valids);
			pepSeqs = pepSeqs.SubArray(valids);
			if (isMutated != null){
				isMutated = isMutated.SubArray(valids);
			}
		}

		private static int GetContainer(int contained, IndexedBitMatrix contains){
			int n = contains.RowCount;
			for (int i = 0; i < n; i++){
				if (contains.Get(i, contained)){
					return i;
				}
			}
			return -1;
		}

		private static bool Contains(string[] p1, ICollection<string> p2){
			if (p2.Count > p1.Length){
				return false;
			}
			foreach (string p in p2){
				int index = Array.BinarySearch(p1, p);
				if (index < 0){
					return false;
				}
			}
			return true;
		}

		public static (string[][] proteinNames, string[][] peptideSequences, byte[][] isMutated)
			GetProteinAndPeptideLists(Dictionary<string, Dictionary<string, byte>> protein2Pep, bool splitTaxonomy,
				TaxonomyRank rank, ProteinSet proteinSet) {
			(string[][] proteinNames, string[][] peptideSequences, byte[][] isMutated) =
				CreateProteinAndPeptideLists(protein2Pep, splitTaxonomy, rank, proteinSet);
			ClusterProteins(ref proteinNames, ref peptideSequences, ref isMutated, splitTaxonomy, rank, proteinSet);
			return (proteinNames, peptideSequences, isMutated);
		}
		public static (string[][] proteinNames, string[][] peptideSequences, byte[][] isMutated)
			GetProteinAndPeptideLists(Dictionary<string, HashSet<string>> protein2Pep, bool splitTaxonomy,
				TaxonomyRank rank, ProteinSet proteinSet) {
			(string[][] proteinNames, string[][] peptideSequences, byte[][] isMutated) =
				CreateProteinAndPeptideLists(protein2Pep, splitTaxonomy, rank, proteinSet);
			ClusterProteins(ref proteinNames, ref peptideSequences, ref isMutated, splitTaxonomy, rank, proteinSet);
			return (proteinNames, peptideSequences, isMutated);
		}

		/// <summary>
		/// Pre-cluster proteins that have identical peptide sets.
		/// </summary>
		private static (string[][] proteinNames, string[][] peptideSequences, byte[][] isMutated)
			CreateProteinAndPeptideLists(Dictionary<string, Dictionary<string, byte>> protein2Pep, bool splitTaxonomy,
				TaxonomyRank rank, ProteinSet proteinSet) {
			string[] proteinIds = protein2Pep.Keys.ToArray();
			string[][] peptideSeq = new string[protein2Pep.Count][];
			byte[][] isMut = new byte[protein2Pep.Count][];
			for (int i = 0; i < proteinIds.Length; i++) {
				peptideSeq[i] = protein2Pep[proteinIds[i]].Keys.ToArray();
				Array.Sort(peptideSeq[i]);
				isMut[i] = new byte[peptideSeq[i].Length];
				for (int j = 0; j < isMut[i].Length; j++) {
					isMut[i][j] = protein2Pep[proteinIds[i]][peptideSeq[i][j]];
				}
			}
			return CreateProteinAndPeptideLists2(proteinIds, peptideSeq, isMut, splitTaxonomy, rank, proteinSet);
		}
		private static (string[][] proteinNames, string[][] peptideSequences, byte[][] isMutated)
			CreateProteinAndPeptideLists(Dictionary<string, HashSet<string>> protein2Pep, bool splitTaxonomy,
				TaxonomyRank rank, ProteinSet proteinSet) {
			string[] proteinIds = protein2Pep.Keys.ToArray();
			string[][] peptideSeq = new string[protein2Pep.Count][];
			for (int i = 0; i < proteinIds.Length; i++) {
				peptideSeq[i] = protein2Pep[proteinIds[i]].ToArray();
				Array.Sort(peptideSeq[i]);
			}
			return CreateProteinAndPeptideLists2(proteinIds, peptideSeq, null, splitTaxonomy, rank, proteinSet);
		}
		private static (string[][] proteinNames, string[][] peptideSequences, byte[][] isMutated)
			CreateProteinAndPeptideLists2(string[] proteinIds, string[][] peptideSeq, byte[][] isMut, bool splitTaxonomy,
				TaxonomyRank rank, ProteinSet proteinSet) {
			string[] taxIds = null;
			if (splitTaxonomy) {
				TaxonomyItems taxonomyItems = TaxonomyItems.GetTaxonomyItems();
				taxIds = new string[proteinIds.Length];
				for (int i = 0; i < taxIds.Length; i++) {
					Protein prot = proteinSet.Get(proteinIds[i]);
					taxIds[i] = prot != null ? taxonomyItems.GetTaxonomyIdOfRank(prot.TaxonomyId, rank) : "-1";
				}
			}
			bool[] taken = new bool[proteinIds.Length];
			List<int[]> groupInd = new List<int[]>();
			for (int i = 0; i < taken.Length; i++) {
				if (taken[i]) {
					continue;
				}
				List<int> indices = new List<int> { i };
				taken[i] = true;
				for (int j = i + 1; j < taken.Length; j++) {
					if (taken[j]) {
						continue;
					}
					if (ArrayUtils.EqualArrays(peptideSeq[i], peptideSeq[j])) {
						if (!splitTaxonomy || taxIds[i].Equals(taxIds[j])) {
							indices.Add(j);
							taken[j] = true;
						}
					}
				}
				groupInd.Add(indices.ToArray());
			}
			int[][] groupIndices = groupInd.ToArray();
			for (int i = 0; i < groupIndices.Length; i++) {
				string[] names = proteinIds.SubArray(groupIndices[i]);
				int[] o = ArrayUtils.Order(names);
				groupIndices[i] = groupIndices[i].SubArray(o);
			}
			string[][] proteinNames = new string[groupIndices.Length][];
			for (int i = 0; i < proteinNames.Length; i++) {
				proteinNames[i] = proteinIds.SubArray(groupIndices[i]);
				Array.Sort(proteinNames[i]);
			}
			string[][] peptideSequences = new string[groupIndices.Length][];
            for (int i = 0; i < proteinNames.Length; i++)
            {
                peptideSequences[i] = peptideSeq[groupIndices[i][0]];
                int[] o = ArrayUtils.Order(peptideSequences[i]);
                peptideSequences[i] = peptideSequences[i].SubArray(o);
            }

            byte[][] isMutated = null;
			if (isMut != null){
				isMutated = new byte[groupIndices.Length][];
				for (int i = 0; i < proteinNames.Length; i++) {
					isMutated[i] = isMut[groupIndices[i][0]];
					int[] o = ArrayUtils.Order(peptideSequences[i]);
					isMutated[i] = isMutated[i].SubArray(o);
				}
			}
			return (proteinNames, peptideSequences, isMutated);
		}
	}
}