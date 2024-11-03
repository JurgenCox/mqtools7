using MqApi.Util;

namespace MqUtil.Mol{
	public static class SpeciesItems{
		private static List<SpeciesItem> species;
		public static List<SpeciesItem> Species => species ?? (species = Init());

		private static List<SpeciesItem> Init(){
			string folder = FileUtils.GetConfigPath() + Path.DirectorySeparatorChar+"data"+ Path.DirectorySeparatorChar;
			string file = folder + "species.txt.gz";
			StreamReader reader = FileUtils.GetReader(file);
			reader.ReadLine();
			string line;
			List<SpeciesItem> result = new List<SpeciesItem>();
			while ((line = reader.ReadLine()) != null){
				string[] w = line.Split('\t');
				if (w.Length < 1){
					continue;
				}
				SpeciesItem pr = new SpeciesItem{
					Name = w[0],
					TaxonomyId = int.Parse(w[1]),
					CommonName = w[2],
					PspName = w[3],
					ReactomeName = w[4],
					EnsemblDivision = ToEnsemblDivision(w[5]),
					EnsemblSubspecies = w[6],
					SubnodeIds = Array(w[7]),
					Superkingdom = w[8],
					Kingdom = w[9],
					Subkingdom = w[10],
					Superphylum = w[11],
					Phylum = w[12],
					Subphylum = w[13],
					Superclass = w[14],
					Class = w[15],
					Subclass = w[16],
					Infraclass = w[17],
					Superorder = w[18],
					Order = w[19],
					Suborder = w[20],
					Infraorder = w[21],
					Parvorder = w[22],
					Superfamily = w[23],
					Family = w[24],
					Subfamily = w[25],
					Tribe = w[26],
					Subtribe = w[27],
					Genus = w[28],
					Subgenus = w[29],
					SpeciesGroup = w[30],
					SpeciesSubgroup = w[31]
				};
				result.Add(pr);
			}
			return result;
		}
		private static EnsemblDivision ToEnsemblDivision(string s){
			switch (s){
				case "main":
					return EnsemblDivision.Main;
				default:
					return EnsemblDivision.NotAvalilable;
			}
		}

		public static int[] Array(string s){
			string[] x = SArray(s);
			int[] result = new int[x.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = int.Parse(x[i]);
			}
			return result;
		}

		public static string[] SArray(string s){
			return string.IsNullOrEmpty(s) ? new string[0] : s.Split(';');
		}
	}
}