using MqApi.Num;

namespace MqUtil.Parse{
	public class GoParser{
		private readonly Dictionary<string, GoEntry> gomap;

		public GoParser(string gofile){
			gomap = CreateGomap(gofile);
		}

		private static Dictionary<string, GoEntry> CreateGomap(string gofile){
			Dictionary<string, GoEntry> result = new Dictionary<string, GoEntry>();
			StreamReader reader = new StreamReader(gofile);
			string line;
			List<string> buffer = new List<string>();
			while ((line = reader.ReadLine()) != null){
				line = line.Trim();
				if (line.Length == 0){
					ProcessGo(buffer.ToArray(), result);
					buffer = new List<string>();
				} else{
					buffer.Add(line);
				}
			}
			return result;
		}

		private static void ProcessGo(IList<string> strings, IDictionary<string, GoEntry> goMap){
			if (strings.Count == 0){
				return;
			}
			if (strings[0].StartsWith("[Term]")){
				GoEntry entry = new GoEntry(strings);
				goMap.Add(entry.GetId(), entry);
				string[] altIds = entry.GetAltId();
				foreach (string t in altIds){
					goMap.Add(t, entry);
				}
			}
		}

		public string[] Complete(string[] ids){
			HashSet<string> result = new HashSet<string>();
			foreach (string id in ids){
				if (gomap.ContainsKey(id)){
					GoEntry[] family = gomap[id].GetFamily(gomap);
					foreach (GoEntry t in family){
						result.Add(t.GetId());
					}
				}
			}
			return ArrayUtils.UniqueValues(result.ToArray());
		}

		public string[] GetNamespace(string[] ids, Namespace space){
			List<string> result = new List<string>();
			foreach (string id in ids){
				if (gomap.ContainsKey(id)){
					if (gomap[id].Namespace == space){
						result.Add(id);
					}
				}
			}
			return result.ToArray();
		}

		public string[] GetAllIds(){
			return gomap.Keys.ToArray();
		}

		public string GetName(string goid) {
			return gomap[goid].Name;
		}

		public string[] GetNames(string[] goids) {
			string[] result = new string[goids.Length];
			for(int i = 0; i < result.Length; i++){
				result[i] = GetName(goids[i]);
			}
			return result;
		}

		public string[] GetSlims(string[] gos) {
			List<string> result = new List<string>();
			foreach (string go in gos){
				if(!gomap.ContainsKey(go)){
					continue;
				}
				if(gomap[go].IsSlim()){
					result.Add(go);
				}
			}
			return result.ToArray();
		}
	}
}