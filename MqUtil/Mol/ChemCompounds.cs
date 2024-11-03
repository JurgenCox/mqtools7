namespace MqUtil.Mol{
	public static class ChemCompounds{
		public static ChemCompound[] compounds = InitCompounds();
		public static Dictionary<string, ChemCompound> casToCompound;
		public static Dictionary<string, List<ChemCompound>> compositionToCompounds;
		private static ChemCompound[] InitCompounds(){
			//Properties.Resources.compounds;
			const string resourceData = "";
			string[] lines = resourceData.Split(new[]{Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
			List<ChemCompound> result = new List<ChemCompound>();
			for (int i = 1; i < lines.Length; i++){
				string line = lines[i];
				string[] w = line.Split('\t');
				string cas = w[0];
				string name = w[1];
				string alternativeName = w[2];
				string composition = w[3];
				if (composition.Contains("-H")){
					continue;
				}
				name = name.Replace("\"", "");
				alternativeName = alternativeName.Replace("\"", "");
				ChemCompound c = new ChemCompound(cas, name, alternativeName, composition);
				result.Add(c);
			}
			casToCompound = new Dictionary<string, ChemCompound>();
			compositionToCompounds = new Dictionary<string, List<ChemCompound>>();
			foreach (ChemCompound compound in result){
				if (!string.IsNullOrEmpty(compound.cas)){
					casToCompound.Add(compound.cas, compound);
				}
				if (!string.IsNullOrEmpty(compound.name)){
					if (!compositionToCompounds.ContainsKey(compound.composition)){
						compositionToCompounds.Add(compound.composition, new List<ChemCompound>());
					}
					compositionToCompounds[compound.composition].Add(compound);
				}
			}
			return result.ToArray();
		}
	}
}