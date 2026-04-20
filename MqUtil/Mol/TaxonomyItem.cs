namespace MqUtil.Mol{
	public class TaxonomyItem{
		private int divisionId;
		private int geneticCodeId;
		private int mitoGeneticCodeId;
		private readonly List<string> names = new List<string>();
		private readonly List<TaxonomyNameType> nameTypes = new List<TaxonomyNameType>();

		public TaxonomyItem(int taxId, int ParentTaxId, TaxonomyRank rank, int divisionId, int geneticCodeId,
			int mitoGeneticCodeId){
			this.TaxId = taxId;
			this.ParentTaxId = ParentTaxId;
			this.Rank = rank;
			this.divisionId = divisionId;
			this.geneticCodeId = geneticCodeId;
			this.mitoGeneticCodeId = mitoGeneticCodeId;
		}

		public TaxonomyRank Rank { get; }
		public int TaxId { get; }
		public int ParentTaxId { get; }

		public void AddName(string name, TaxonomyNameType nameType){
			names.Add(name);
			nameTypes.Add(nameType);
		}

		public string GetScientificName(){
			for (int i = 0; i < names.Count; i++){
				if (nameTypes[i] == TaxonomyNameType.ScientificName){
					return names[i];
				}
			}
			return names[0];
		}

		public TaxonomyItem GetParentOfRank(TaxonomyItems taxonomyItems, TaxonomyRank rank1){
			if (rank1 == Rank){
				return this;
			}
			if (!taxonomyItems.taxId2Item.ContainsKey(ParentTaxId)){
				return null;
			}
			TaxonomyItem parent = taxonomyItems.taxId2Item[ParentTaxId];
			return parent.GetParentOfRank(taxonomyItems, rank1);
		}
	}
}