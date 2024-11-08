using MqUtil.Parse.Reactome.Misc;

namespace MqUtil.Parse.Reactome.Data{
	public class ReactomeComplex : ReactomeItem, INamedItem, IShortNamedItem, IXrefItem, IOrganismItem,
		ISynonymsItem{
		public string Name { get; set; }
		private readonly List<string> components = new List<string>();
		public List<string> Components { get { return components; } }

		public string ShortName { get; set; }
		private readonly List<string> xrefs = new List<string>();
		public List<string> Xrefs { get { return xrefs; } }
		public string Organism { get; set; }
		private readonly List<string> synonyms = new List<string>();
		public List<string> Synonyms { get { return synonyms; } }
	}
}