namespace MqUtil.Mol{
	public class ChemCompound{
		public string cas;
		public string name;
		public string alternativeName;
		public string composition;

		public ChemCompound(string cas, string name, string alternativeName, string composition){
			this.cas = cas;
			this.name = name;
			this.alternativeName = alternativeName;
			this.composition = composition;
		}
	}
}