namespace MqUtil.Parse.Sbml{
	public class SbmlModel : SbmlItem{
		public string Name { get; set; }
		public List<SbmlReaction> Reactions { get; set; }
		public List<SbmlSpecies> Species { get; set; }
		public List<SbmlCompartment> Compartments { get; set; }
		public List<SbmlUnitDefinition> UnitDefinitions { get; set; }
		public List<SbmlParameter> Parameters { get; set; }
		public List<SbmlEvent> Events { get; set; }
		public List<SbmlRule> Rules { get; set; }
		public List<SbmlFunctionDefinition> FunctionDefinitions { get; set; }
		public List<SbmlInitialAssignment> InitialAssignments { get; set; }
		public List<SbmlSpeciesType> SpeciesTypes { get; set; }
		public List<SbmlQualitativeSpecies> QualitativeSpecies { get; set; }
		public List<SbmlTransition> Transitions { get; set; }
		public List<SbmlConstraint> Constraints { get; set; }
		public List<SbmlFluxBound> FluxBounds { get; set; }
		public List<SbmlObjective> Objectives { get; set; }
		public int SbmlLevel { get; set; }
		public int SbmlVersion { get; set; }
		public Dictionary<string, List<int>> Uniprot2SpeciesInd { get; set; }
		public Dictionary<string, int> Id2SpeciesInd { get; set; }
	}
}