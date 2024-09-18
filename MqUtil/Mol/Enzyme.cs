using System.Xml.Serialization;
namespace MqUtil.Mol{
	public class Enzyme : StorableItem{
		private HashSet<string> specificity;

		[XmlArray("specificity")]
		public string[] Specificity{
			get => specificity.ToArray();
			set => specificity = new HashSet<string>(value);
		}

		public bool Cleaves(char c1, char c2){
			return specificity.Contains("" + c1 + c2);
		}

		public bool CleavesAnyCterm(char c) {
			foreach (string s in specificity) {
				if (s[0] == c) {
					return true;
				}
			}
			return false;
		}

		public HashSet<char> Sites0() {
			HashSet<char> result = new HashSet<char>();
			foreach (string s in specificity){
				result.Add(s[0]);
			}
			return result;
		}

		public bool CleavesAnyNterm(char c){
			foreach (string s in specificity){
				if (s[1] == c){
					return true;
				}
			}
			return false;
		}

		public string GetSpecialAas() {
			Dictionary<char, int> count1 = new Dictionary<char, int>();
			Dictionary<char, int> count2 = new Dictionary<char, int>();
			foreach (string s in specificity) {
				char c1 = s[0];
				char c2 = s[1];
				if (!count1.ContainsKey(c1)) {
					count1.Add(c1, 0);
				}
				if (!count2.ContainsKey(c2)) {
					count2.Add(c2, 0);
				}
				count1[c1]++;
				count2[c2]++;
			}
			HashSet<char> result = new HashSet<char>();
			foreach (char c in count1.Keys) {
				if (count1[c] >= 17) {
					result.Add(c);
				}
			}
			foreach (char c in count2.Keys) {
				if (count2[c] >= 17) {
					result.Add(c);
				}
			}
			return new string(result.ToArray());
		}
		public bool CleavesCterminally() {
			Dictionary<char, int> count1 = new Dictionary<char, int>();
			foreach (string s in specificity) {
				char c1 = s[0];
				if (!count1.ContainsKey(c1)) {
					count1.Add(c1, 0);
				}
				count1[c1]++;
			}
			foreach (char c in count1.Keys) {
				if (count1[c] >= 17) {
					return true;
				}
			}
			return false;
		}
		public bool CleavesNterminally() {
			Dictionary<char, int> count2 = new Dictionary<char, int>();
			foreach (string s in specificity) {
				char c2 = s[1];
				if (!count2.ContainsKey(c2)) {
					count2.Add(c2, 0);
				}
				count2[c2]++;
			}
			foreach (char c in count2.Keys) {
				if (count2[c] >= 17) {
					return true;
				}
			}
			return false;
		}
	}
}