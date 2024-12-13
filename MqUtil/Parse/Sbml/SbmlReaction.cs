namespace MqUtil.Parse.Sbml{
	public class SbmlReaction : SbmlItem{
		public string Name { get; set; }
		public bool Reversible { get; set; }
		public string SboTerm { get; set; }
		public List<string> Reactants { get; set; }
		public List<string> Products { get; set; }
		public List<string> Modifiers { get; set; }
		public SbmlKineticLaw KineticLaw { get; set; }
	}
}