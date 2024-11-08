using MqUtil.Parse.Reactome.Misc;

namespace MqUtil.Parse.Reactome.Data{
	public class ReactomePathway : ReactomeItem, INamedItem, IShortNamedItem, ICommentableItem, IXrefItem, IOrganismItem,
		ISynonymsItem{
			public string Name { get; set; }
			public string ShortName { get; set; }
			private readonly List<string> comments = new List<string>();
			public List<string> Comments { get { return comments; } }
			private readonly List<string> synonyms = new List<string>();
			public List<string> Synonyms { get { return synonyms; } }
			public string Organism { get; set; }
			
		private readonly List<string> xrefs = new List<string>();
		public List<string> Xrefs { get { return xrefs; } }
		private readonly List<string> pathwayComponents = new List<string>();
		public List<string> PathwayComponents { get { return pathwayComponents; } }
	}
}