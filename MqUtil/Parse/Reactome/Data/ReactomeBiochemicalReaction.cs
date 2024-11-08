using MqUtil.Parse.Reactome.Misc;

namespace MqUtil.Parse.Reactome.Data {
	public class ReactomeBiochemicalReaction : ReactomeItem, INamedItem, IShortNamedItem, ICommentableItem, IXrefItem, ISynonymsItem {
		public string ShortName { get; set; }
		private readonly List<string> synonyms = new List<string>();
		public List<string> Synonyms { get { return synonyms; } }

		public string Name { get; set; }
		public List<string> EcNumber { get { return ecNumber; } }
		private readonly List<string> ecNumber = new List<string>();
		public List<string> Left { get { return left; } }
		private readonly List<string> left = new List<string>();
		public List<string> Right { get { return right; } }
		private readonly List<string> right = new List<string>();
		public List<string> Comments { get { return comments; } }
		private readonly List<string> comments = new List<string>();

		private readonly List<string> xrefs = new List<string>();
		public List<string> Xrefs { get { return xrefs; } }
	}
}
