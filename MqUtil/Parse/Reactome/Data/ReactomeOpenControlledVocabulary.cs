using MqUtil.Parse.Reactome.Misc;

namespace MqUtil.Parse.Reactome.Data {
	public class ReactomeOpenControlledVocabulary : ReactomeItem, ICommentableItem, IXrefItem {
		private readonly List<string> comments = new List<string>();
		public List<string> Comments { get { return comments; } }
		private readonly List<string> xrefs = new List<string>();
		public List<string> Xrefs { get { return xrefs; } }
		public string Term { get; set; }
	}
}
