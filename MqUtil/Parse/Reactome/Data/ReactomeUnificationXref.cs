using MqUtil.Parse.Reactome.Misc;

namespace MqUtil.Parse.Reactome.Data {
	public class ReactomeUnificationXref : ReactomeItem, ICommentableItem, IXrefItem, IDbIdItem {
		private readonly List<string> comments = new List<string>();
		public List<string> Comments { get { return comments; } }
		private readonly List<string> xrefs = new List<string>();
		public List<string> Xrefs { get { return xrefs; } }
		public Database Db { get; set; }
		public string Id { get; set; }
		public string IdVersion { get; set; }
	}
}
