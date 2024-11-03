namespace MqUtil.Mol{
	public static class PtmTypes{
		public static PtmType acetylation = new PtmType{PspName = "ACETYLATION", Residues = "KRSTY"};
		public static PtmType monoMethylation = new PtmType{PspName = "MONO-METHYLATION", Residues = "KRHCL"};
		public static PtmType diMethylation = new PtmType{PspName = "DI-METHYLATION", Residues = "KR"};
		public static PtmType triMethylation = new PtmType{PspName = "TRI-METHYLATION", Residues = "K"};
		public static PtmType oglcnac = new PtmType{PspName = "O-GlcNAc", Residues = "ST"};
		public static PtmType phosphorylation = new PtmType{PspName = "PHOSPHORYLATION", Residues = "STYAHDRPEMKG"};
		public static PtmType sumoylation = new PtmType{PspName = "SUMOYLATION", Residues = "K"};
		public static PtmType ubiquitination = new PtmType{PspName = "UBIQUITINATION", Residues = "KRS"};
		public static PtmType palmitoylation = new PtmType{PspName = "PALMITOYLATION", Residues = ""};
		public static PtmType[] allTypes ={
			acetylation, monoMethylation, diMethylation, triMethylation, oglcnac,
			phosphorylation, sumoylation, ubiquitination, palmitoylation
		};
		public static Dictionary<string, PtmType> mapPspToPtmType = InitMap();

		private static Dictionary<string, PtmType> InitMap(){
			Dictionary<string, PtmType> x = new Dictionary<string, PtmType>();
			foreach (PtmType allType in allTypes){
				if (!string.IsNullOrEmpty(allType.PspName)){
					x.Add(allType.PspName, allType);
				}
			}
			x.Add("METHYLATION", monoMethylation);
			return x;
		}
	}
}