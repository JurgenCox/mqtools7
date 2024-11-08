using MqUtil.Parse.Reactome.Misc;

namespace MqUtil.Parse.Reactome.Data {
	public class ReactomeDna : ReactomeItem, INamedItem, IShortNamedItem, ICommentableItem, IXrefItem, IOrganismItem, ISynonymsItem {
		private readonly List<string> comments = new List<string>();
		public List<string> Comments { get { return comments; } }
		public string Name { get; set; }
		public string ShortName { get; set; }
		private readonly List<string> xrefs = new List<string>();
		public List<string> Xrefs { get { return xrefs; } }
		public string Organism { get; set; }
		private readonly List<string> synonyms = new List<string>();
		public List<string> Synonyms { get { return synonyms; } }
	}
}
