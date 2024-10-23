using MqApi.Util;

namespace MqUtil.Masses {
	public class MoleculeListParser {
		public static Dictionary<int, List<SmallMoleculeCluster>> Parse(bool completeIsotopes, bool completeCharges) {
			string file = Path.Combine(FileUtils.GetConfigPath(), "compositions.txt");
			return Parse(completeIsotopes, completeCharges, file);
		}

		public static Dictionary<int, List<SmallMoleculeCluster>> Parse(bool completeIsotopes, bool completeCharges, string file) {
			Dictionary<int, List<SmallMoleculeCluster>> result = new Dictionary<int, List<SmallMoleculeCluster>>();
			if (!File.Exists(file)) {
				return result;
			}
			StreamReader reader = new StreamReader(file);
			reader.ReadLine();
			string line;
			while ((line = reader.ReadLine()) != null) {
				string[] w = line.Split('\t');
				int nominalMass = Parser.Int(w[0]);
				string[] w1 = w[1].Split(',');
				for (int i = 0; i < w1.Length; i++) {
					w1[i] = w1[i].Trim();
				}
				if (w1.Length == 0) {
					throw new Exception("Missing composition: " + line);
				}
				string[] c = w[2].Split(',');
				if (c.Length == 0) {
					throw new Exception("Missing charge: " + line);
				}
				int[] charges = new int[c.Length];
				for (int i = 0; i < charges.Length; i++) {
					charges[i] = int.Parse(c[i]);
				}
				if (!result.ContainsKey(nominalMass)) {
					result.Add(nominalMass, new List<SmallMoleculeCluster>());
				}
				result[nominalMass].Add(new SmallMoleculeCluster(w1, charges, completeIsotopes, completeCharges));
			}
			reader.Close();
			return result;
		}
	}
}