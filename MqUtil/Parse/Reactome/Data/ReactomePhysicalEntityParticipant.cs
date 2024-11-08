using MqUtil.Parse.Reactome.Misc;

namespace MqUtil.Parse.Reactome.Data {
	public class ReactomePhysicalEntityParticipant : ReactomeItem, ICommentableItem, IParticipantItem {
		private readonly List<string> comments = new List<string>();
		public List<string> Comments { get { return comments; } }
		private readonly List<string> cellularLocation = new List<string>();
		public List<string> CellularLocation { get { return cellularLocation; } }
		public string PhysicalEntity { get; set; }
		public int StoichiometricCoefficient { get; set; }
	}
}
