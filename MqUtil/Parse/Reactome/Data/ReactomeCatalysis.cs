using MqUtil.Mol;
using MqUtil.Parse.Reactome.Misc;

namespace MqUtil.Parse.Reactome.Data {
	public class ReactomeCatalysis : ReactomeItem, IXrefItem, IControlItem {
		private readonly List<string> xrefs = new List<string>();
		public List<string> Xrefs { get { return xrefs; } }
		public string Controller { get; set; }
		public string Controlled { get; set; }
		public ReactionControlType ControlType { get; set; }
		public string Direction { get; set; }
	}
}
