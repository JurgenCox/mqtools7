namespace MqUtil.Mol{
	public static class LcaComputer{
		private static readonly int[] emptyIntArray = Array.Empty<int>();

		public static LcaResult Compute(TaxonomyItems tax, IEnumerable<int> taxids){
			List<int> unknown = null;
			List<int> valid = null;
			foreach (int t in taxids){
				if (tax.ContainsNode(t)){
					valid ??= new List<int>();
					valid.Add(t);
				} else{
					unknown ??= new List<int>();
					unknown.Add(t);
				}
			}
			int[] unknownArray = unknown?.ToArray() ?? emptyIntArray;
			if (valid == null || valid.Count == 0){
				return new LcaResult(0, unknownArray);
			}
			int lca = valid[0];
			for (int i = 1; i < valid.Count; i++){
				lca = PairwiseLca(tax, lca, valid[i]);
			}
			return new LcaResult(lca, unknownArray);
		}

		private static int PairwiseLca(TaxonomyItems tax, int a, int b){
			if (a == b){
				return a;
			}
			HashSet<int> ancestorsA = new HashSet<int>(tax.GetAncestors(a)){a};
			if (ancestorsA.Contains(b)){
				return b;
			}
			foreach (int anc in tax.GetAncestors(b)){
				if (ancestorsA.Contains(anc)){
					return anc;
				}
			}
			return 1;
		}
	}
}
