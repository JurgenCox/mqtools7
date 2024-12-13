namespace MqUtil.Parse.Sbml{
	public class SbmlSpecies : SbmlItem{
		public string Name { get; set; }
		public double InitialConcentration { get; set; }
		public bool Constant { get; set; }
		public int Charge { get; set; }
		public bool HasOnlySubstanceUnits { get; set; }
		public bool BoundaryCondition { get; set; }
		public string SboTerm { get; set; }
		public string Compartment { get; set; }
		public string Formula { get; set; }
		public string Inchi { get; set; }
		public Dictionary<string,string> MiscIds { get; set; }
		public List<string> Is { get; set; } = new List<string>();
		public List<string> IsEncodedBy { get; set; } = new List<string>();
		public List<string> HasPart { get; set; } = new List<string>();
		public List<string> Chebi { get; set; } = new List<string>();
		public List<string> KeggCompound { get; set; } = new List<string>();
		public List<string> Hmdb { get; set; } = new List<string>();
		public List<string> PubchemSubstance { get; set; } = new List<string>();
		public List<string> KeggDrug { get; set; } = new List<string>();
		public List<string> Uniprot { get; set; } = new List<string>();
		public List<string> KeggGenes { get; set; } = new List<string>();
		public List<string> UniprotPart { get; set; } = new List<string>();
	}
}