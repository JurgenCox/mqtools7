namespace MqUtil.Ms.Enums {
	public enum CrosslinkMode{
		AllProteinsXAllProteins,
		AllProteinsXIdentifiedProteins,
		AllProteinsXPeptidesWithMod,
		AllProteinsXSpecificProteins,
		AllProteinsXSpecificSites,
		IdentifiedProteinsXIdentifiedProteins,
		IdentifiedProteinsXPeptidesWithMod,
		IdentifiedProteinsXSpecificProteins,
		IdentifiedProteinsXSpecificSites,
		PeptidesWithModXPeptidesWithMod,
		PeptidesWithModXSpecificProteins,
		PeptidesWithModXSpecificSites,
		SpecificProteinsXSpecificProteins,
		SpecificProteinsXSpecificSites,
		SpecificSitesXSpecificSites,
		ProteinNetwork
	}
}