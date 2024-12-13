namespace MqUtil.Parse.Sbml{
	public enum SbmlSection{
		None,
		ListOfUnitDefinitions,
		ListOfCompartments,
		ListOfSpecies,
		ListOfReactions,
		ListOfParameters,
		ListOfEvents,
		ListOfRules,
		Toplevel,
		ListOfFunctionDefinitions,
		ListOfInitialAssignments,
		ListOfSpeciesTypes,
		ListOfQualitativeSpecies,
		ListOfTransitions,
		ListOfConstraints,
		ListOfFluxBounds,
		ListOfObjectives
	}
}