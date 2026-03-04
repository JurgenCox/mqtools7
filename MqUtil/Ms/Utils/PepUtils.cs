using System.ComponentModel;
using MqApi.Num;
using MqApi.Util;
using MqUtil.Data;
using MqUtil.Mol;
using MqUtil.Ms.Data;
using MqUtil.Util;

namespace MqUtil.Ms.Utils{
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
			bool splitTaxonomy, TaxonomyRank rank, ProteinSet proteinSet, int nThreads, Responder responder){
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
			string[][] ps = new string[pepSeqs.Length][];
			for (int i = 0; i < n; i++){
				int[] o = pepSeqs[i].Order();
				pepSeqs[i] = pepSeqs[i].SubArray(o);
				ps[i] = pepSeqs[i];
				if (isMutated != null){
					isMutated[i] = isMutated[i].SubArray(o);
				}
			}
			SparseBitMatrix contains = new SparseRowBitMatrix(n, n);
			ThreadDistributor td = new ThreadDistributor(nThreads, n, i =>
			{
				string[] p1 = ps[i];
				for (int j = 0; j < n; j++)
				{
					if (i == j)
					{
						continue;
					}
					contains.Set(i, j,
						(!splitTaxonomy || taxIds[i].Equals(taxIds[j])) && Contains(p1, ps[j]));
				}
			});
			responder?.Comment("Clustering proteins...1");
			td.Start();
			responder?.Comment("Clustering proteins...2");
			Cluster(contains, proteinIds, pepSeqs, isMutated);
			responder?.Comment("Clustering proteins...3");
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
			responder?.Comment("Clustering proteins...4");
		}

		private static void Cluster(SparseBitMatrix contains, string[][] proteinIds, string[][] pepSeqs, 
			byte[][] isMutated)
		{
			int n = proteinIds.Length;
			int count;
			do
			{
				count = 0;
				int start = 0;
				while (true)
				{
					int container = -1;
					int contained = -1;
					for (int i = start; i < n; i++)
					{
						container = GetContainer(i, contains);
						if (container != -1)
						{
							contained = i;
							break;
						}
					}
					if (container == -1)
					{
						break;
					}
					contains.SetRowFalse(contained);
					contains.SetColumnFalse(contained);
					pepSeqs[contained] = [];
					if (isMutated != null)
					{
						isMutated[contained] = [];
					}
					proteinIds[container] = ArrayUtils.Concat(proteinIds[container], proteinIds[contained]);
					proteinIds[contained] = [];
					start = contained + 1;
					count++;
				}
			} while (count > 0);
		}
		private static int GetContainer(int contained, SparseBitMatrix contains){
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
				TaxonomyRank rank, ProteinSet proteinSet, Responder responder, int nThreads){
			(string[][] proteinNames, string[][] peptideSequences, byte[][] isMutated) =
				CreateProteinAndPeptideLists(protein2Pep, splitTaxonomy, rank, proteinSet, responder);
			ClusterProteins(ref proteinNames, ref peptideSequences, ref isMutated, splitTaxonomy, rank, proteinSet, nThreads, responder);
			return (proteinNames, peptideSequences, isMutated);
		}
		public static (string[][] proteinNames, string[][] peptideSequences, byte[][] isMutated)
			GetProteinAndPeptideLists(Dictionary<string, HashSet<string>> protein2Pep, bool splitTaxonomy,
				TaxonomyRank rank, ProteinSet proteinSet, Responder responder, int nThreads)
		{
			(string[][] proteinNames, string[][] peptideSequences, byte[][] isMutated) =
				CreateProteinAndPeptideLists(protein2Pep, splitTaxonomy, rank, proteinSet, responder);
			ClusterProteins(ref proteinNames, ref peptideSequences, ref isMutated, splitTaxonomy, rank, proteinSet, nThreads, responder);
			return (proteinNames, peptideSequences, isMutated);
		}
		public static (string[][] proteinNames, string[][] peptideSequences, byte[][] isMutated)
			GetProteinAndPeptideLists(Dictionary<string, ISet<string>> protein2Pep, bool splitTaxonomy,
				TaxonomyRank rank, ProteinSet proteinSet, Responder responder, int nThreads)
		{
			responder?.Comment("GetProteinAndPeptideLists1");
			(string[][] proteinNames, string[][] peptideSequences, byte[][] isMutated) =
				CreateProteinAndPeptideLists(protein2Pep, splitTaxonomy, rank, proteinSet, responder);
			responder?.Comment("GetProteinAndPeptideLists2");
			ClusterProteins(ref proteinNames, ref peptideSequences, ref isMutated, splitTaxonomy, rank, proteinSet, nThreads, responder);
			responder?.Comment("GetProteinAndPeptideLists3");
			return (proteinNames, peptideSequences, isMutated);
		}
		/// <summary>
		/// Pre-cluster proteins that have identical peptide sets.
		/// </summary>
		private static (string[][] proteinNames, string[][] peptideSequences, byte[][] isMutated)
			CreateProteinAndPeptideLists(Dictionary<string, Dictionary<string, byte>> protein2Pep, bool splitTaxonomy,
				TaxonomyRank rank, ProteinSet proteinSet, Responder responder){
			string[] proteinIds = protein2Pep.Keys.ToArray();
			string[][] peptideSeq = new string[protein2Pep.Count][];
			byte[][] isMut = new byte[protein2Pep.Count][];
			for (int i = 0; i < proteinIds.Length; i++){
				peptideSeq[i] = protein2Pep[proteinIds[i]].Keys.ToArray();
				Array.Sort(peptideSeq[i]);
				isMut[i] = new byte[peptideSeq[i].Length];
				for (int j = 0; j < isMut[i].Length; j++){
					isMut[i][j] = protein2Pep[proteinIds[i]][peptideSeq[i][j]];
				}
			}
			return CreateProteinAndPeptideLists2(proteinIds, peptideSeq, isMut, splitTaxonomy, rank, proteinSet, responder);
		}
		private static (string[][] proteinNames, string[][] peptideSequences, byte[][] isMutated)
			CreateProteinAndPeptideLists(Dictionary<string, HashSet<string>> protein2Pep, bool splitTaxonomy,
				TaxonomyRank rank, ProteinSet proteinSet, Responder responder)
		{
			string[] proteinIds = protein2Pep.Keys.ToArray();
			string[][] peptideSeq = new string[protein2Pep.Count][];
			for (int i = 0; i < proteinIds.Length; i++)
			{
				peptideSeq[i] = protein2Pep[proteinIds[i]].ToArray();
				Array.Sort(peptideSeq[i]);
			}
			return CreateProteinAndPeptideLists2(proteinIds, peptideSeq, null, splitTaxonomy, rank, proteinSet, responder);
		}
		private static (string[][] proteinNames, string[][] peptideSequences, byte[][] isMutated)
			CreateProteinAndPeptideLists(Dictionary<string, ISet<string>> protein2Pep, bool splitTaxonomy,
				TaxonomyRank rank, ProteinSet proteinSet, Responder responder)
		{
			string[] proteinIds = protein2Pep.Keys.ToArray();
			string[][] peptideSeq = new string[protein2Pep.Count][];
			for (int i = 0; i < proteinIds.Length; i++)
			{
				peptideSeq[i] = protein2Pep[proteinIds[i]].ToArray();
				Array.Sort(peptideSeq[i]);
			}
			return CreateProteinAndPeptideLists2(proteinIds, peptideSeq, null, splitTaxonomy, rank, proteinSet, responder);
		}
		private static (string[][] proteinNames, string[][] peptideSequences, byte[][] isMutated)
			CreateProteinAndPeptideLists2(string[] proteinIds, string[][] peptideSeq, byte[][] isMut,
				bool splitTaxonomy, TaxonomyRank rank, ProteinSet proteinSet, Responder responder){
			string[] taxIds = null;
			responder?.Comment("CreateProteinAndPeptideLists2-1");
			if (splitTaxonomy){
				TaxonomyItems taxonomyItems = TaxonomyItems.GetTaxonomyItems();
				taxIds = new string[proteinIds.Length];
				for (int i = 0; i < taxIds.Length; i++){
					Protein prot = proteinSet.Get(proteinIds[i]);
					taxIds[i] = prot != null ? taxonomyItems.GetTaxonomyIdOfRank(prot.TaxonomyId, rank) : "-1";
				}
			}
			responder?.Comment("CreateProteinAndPeptideLists2-2");
			int[][] groupIndices = GetNonredGroupInds2(proteinIds, peptideSeq, splitTaxonomy, taxIds);
			responder?.Comment("CreateProteinAndPeptideLists2-3");
			for (int i = 0; i < groupIndices.Length; i++){
				string[] names = proteinIds.SubArray(groupIndices[i]);
				int[] o = names.Order();
				groupIndices[i] = groupIndices[i].SubArray(o);
			}
			responder?.Comment("CreateProteinAndPeptideLists2-4");
			string[][] proteinNames = new string[groupIndices.Length][];
			for (int i = 0; i < proteinNames.Length; i++){
				proteinNames[i] = proteinIds.SubArray(groupIndices[i]);
				Array.Sort(proteinNames[i]);
			}
			responder?.Comment("CreateProteinAndPeptideLists2-5");
			string[][] peptideSequences = new string[groupIndices.Length][];
			for (int i = 0; i < proteinNames.Length; i++){
				peptideSequences[i] = peptideSeq[groupIndices[i][0]];
				int[] o = peptideSequences[i].Order();
				peptideSequences[i] = peptideSequences[i].SubArray(o);
			}
			responder?.Comment("CreateProteinAndPeptideLists2-6");
			byte[][] isMutated = null;
			if (isMut != null){
				isMutated = new byte[groupIndices.Length][];
				for (int i = 0; i < proteinNames.Length; i++){
					isMutated[i] = isMut[groupIndices[i][0]];
					int[] o = peptideSequences[i].Order();
					isMutated[i] = isMutated[i].SubArray(o);
				}
			}
			responder?.Comment("CreateProteinAndPeptideLists2-7");
			return (proteinNames, peptideSequences, isMutated);
		}
		public static int[][] GetNonredGroupIndsOld(string[] proteinIds, string[][] peptideSeq,
			bool splitTaxonomy, string[] taxIds)
		{
			bool[] taken = new bool[proteinIds.Length];
			List<int[]> groupInd = new List<int[]>();
			for (int i = 0; i < taken.Length; i++)
			{
				if (taken[i])
				{
					continue;
				}
				List<int> indices = new List<int> { i };
				taken[i] = true;
				for (int j = i + 1; j < taken.Length; j++)
				{
					if (taken[j])
					{
						continue;
					}
					if (ArrayUtils.EqualArrays(peptideSeq[i], peptideSeq[j]))
					{
						if (!splitTaxonomy || taxIds[i].Equals(taxIds[j]))
						{
							indices.Add(j);
							taken[j] = true;
						}
					}
				}
				groupInd.Add(indices.ToArray());
			}
			return groupInd.ToArray();
		}
		public static int[][] GetNonredGroupInds(string[] proteinIds, string[][] peptideSeq,
			bool splitTaxonomy, string[] taxIds)
		{
			string[] pepConcat = new string[peptideSeq.Length];
			for(int i = 0; i < peptideSeq.Length; i++)
			{
				pepConcat[i] = string.Join("|", peptideSeq[i]);
				if (splitTaxonomy)
				{
					pepConcat[i] = taxIds[i] + "|" + pepConcat[i];
				}
			}
			bool[] taken = new bool[proteinIds.Length];
			List<int[]> groupInd = new List<int[]>();
			for (int i = 0; i < taken.Length; i++)
			{
				if (taken[i])
				{
					continue;
				}
				List<int> indices = new List<int> { i };
				taken[i] = true;
				for (int j = i + 1; j < taken.Length; j++)
				{
					if (taken[j])
					{
						continue;
					}
					if (pepConcat[i].Equals(pepConcat[j]))
					{
						indices.Add(j);
						taken[j] = true;
					}
				}
				groupInd.Add(indices.ToArray());
			}
			return groupInd.ToArray();
		}
		public static int[][] GetNonredGroupInds2(string[] proteinIds, string[][] peptideSeq,
			bool splitTaxonomy, string[] taxIds)
		{
			string[] pepConcat = new string[peptideSeq.Length];
			for (int i = 0; i < peptideSeq.Length; i++)
			{
				pepConcat[i] = string.Join("|", peptideSeq[i]);
				if (splitTaxonomy)
				{
					pepConcat[i] = taxIds[i] + "|" + pepConcat[i];
				}
			}
			int[] o = pepConcat.Order();
			List<int[]> groupInds = new List<int[]>();
			List<int> currentGroup = new List<int> { o[0] };
			for (int i = 1; i < o.Length; i++)
			{
				if (pepConcat[o[i]].Equals(pepConcat[o[i - 1]]))
				{
					currentGroup.Add(o[i]);
				}
				else
				{
					groupInds.Add(currentGroup.ToArray());
					currentGroup = new List<int> { o[i] };
				}
			}
			groupInds.Add(currentGroup.ToArray());
			return groupInds.ToArray();
		}
	}
}