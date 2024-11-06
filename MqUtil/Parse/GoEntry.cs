namespace MqUtil.Parse{
	public enum Namespace {
		BiologicalProcess,
		MolecularFunction,
		CellularComponent
	}

	public class GoEntry {
		private readonly string id;
		private readonly string[] altId;
		private readonly string[] isA;
		private readonly string[] subset;

		public GoEntry(IList<string> buffer) {
			List<string> altIds = new List<string>();
			List<string> isa = new List<string>();
			List<string> subsets = new List<string>();
			for (int i = 1; i < buffer.Count; i++) {
				string line = buffer[i];
				int ind = line.IndexOf(": ", StringComparison.InvariantCulture);
				string value = line.Substring(ind + 2);
				if (value.IndexOf("!", StringComparison.InvariantCulture) != -1) {
					value = value.Substring(0, value.IndexOf("!", StringComparison.InvariantCulture)).Trim();
				}
				if (line.StartsWith("id:")) {
					id = value;
				} else if (line.StartsWith("name:")) {
					Name = value;
				} else if (line.StartsWith("alt_id:")) {
					altIds.Add(value);
				} else if (line.StartsWith("is_a:")) {
					isa.Add(value);
				} else if (line.StartsWith("subset:")) {
					subsets.Add(value);
				} else if (line.StartsWith("namespace:")) {
					switch (value) {
						case "biological_process":
							Namespace = Namespace.BiologicalProcess;
							break;
						case "molecular_function":
							Namespace = Namespace.MolecularFunction;
							break;
						case "cellular_component":
							Namespace = Namespace.CellularComponent;
							break;
						default:
							throw new Exception("Namespace does not exist: " + value);
					}
				}
			}
			isA = isa.ToArray();
			altId = altIds.ToArray();
			subset = subsets.ToArray();
			Array.Sort(subset);
		}

		public bool IsSlim() {
			foreach (string s in subset){
				if(s.ToLower().Contains("slim")){
					return true;
				}
			}
			return false;
		}

		public Namespace Namespace { get; }

		public string GetId() {
			return id;
		}

		public string[] GetAltId() {
			return altId;
		}

		public string Name { get; }

		public GoEntry[] GetParents(Dictionary<string, GoEntry> gomap) {
			GoEntry[] result = new GoEntry[isA.Length];
			for (int i = 0; i < result.Length; i++) {
				result[i] = gomap[isA[i]];
			}
			return result;
		}

		public GoEntry[] GetFamily(Dictionary<string, GoEntry> gomap) {
			HashSet<GoEntry> result = new HashSet<GoEntry>();
			GoEntry[] parents = GetParents(gomap);
			if(parents.Length > 0) {
				result.Add(this);
			}
			foreach (GoEntry t in parents){
				GoEntry[] f = t.GetFamily(gomap);
				foreach (GoEntry t1 in f){
					result.Add(t1);
				}
			}
			return result.ToArray(); 
		}

		public bool HasParents() {
			return isA.Length > 0;
		}
	}
}