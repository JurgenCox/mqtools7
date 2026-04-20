namespace MqUtil.Annotation{
	public static class FunctionConsensus{
		public static HashSet<string> Unanimous(IEnumerable<IEnumerable<string>?>? inputs){
			List<HashSet<string>> sources = Materialize(inputs);
			if (sources.Count == 0){
				return new HashSet<string>();
			}
			HashSet<string> result = new HashSet<string>(sources[0]);
			for (int i = 1; i < sources.Count; i++){
				result.IntersectWith(sources[i]);
			}
			return result;
		}

		public static HashSet<string> Majority(IEnumerable<IEnumerable<string>?>? inputs){
			List<HashSet<string>> sources = Materialize(inputs);
			HashSet<string> result = new HashSet<string>();
			if (sources.Count == 0){
				return result;
			}
			Dictionary<string, int> counts = new Dictionary<string, int>();
			foreach (HashSet<string> src in sources){
				foreach (string val in src){
					counts.TryGetValue(val, out int c);
					counts[val] = c + 1;
				}
			}
			int n = sources.Count;
			foreach (KeyValuePair<string, int> kv in counts){
				if (kv.Value * 2 > n){
					result.Add(kv.Key);
				}
			}
			return result;
		}

		public static HashSet<string> Union(IEnumerable<IEnumerable<string>?>? inputs){
			HashSet<string> result = new HashSet<string>();
			if (inputs == null){
				return result;
			}
			foreach (IEnumerable<string>? src in inputs){
				if (src == null){
					continue;
				}
				foreach (string val in src){
					if (val != null){
						result.Add(val);
					}
				}
			}
			return result;
		}

		private static List<HashSet<string>> Materialize(IEnumerable<IEnumerable<string>?>? inputs){
			List<HashSet<string>> sources = new List<HashSet<string>>();
			if (inputs == null){
				return sources;
			}
			foreach (IEnumerable<string>? src in inputs){
				if (src == null){
					continue;
				}
				HashSet<string> set = new HashSet<string>();
				foreach (string val in src){
					if (val != null){
						set.Add(val);
					}
				}
				sources.Add(set);
			}
			return sources;
		}
	}
}
