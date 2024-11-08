namespace MqUtil.Parse.Reactome.Misc {
	public interface IParticipantItem {
		string PhysicalEntity { get; set; }
		int StoichiometricCoefficient { get; set; }
		List<string> CellularLocation { get; }
	}
}
