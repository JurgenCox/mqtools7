using MqUtil.Mol;
using MqUtil.Parse.Reactome.Misc;

namespace MqUtil.Parse.Reactome.Data {
	public class ReactomeControl : ReactomeItem, INamedItem, IShortNamedItem, ICommentableItem, IXrefItem, IControlItem {
		public List<string> Comments { get; } = new List<string>();
		public string Name { get; set; }
		public string ShortName { get; set; }
		public List<string> Xrefs { get; } = new List<string>();
		public string Controller { get; set; }
		public string Controlled { get; set; }
		public ReactionControlType ControlType { get; set; }
	}
}
