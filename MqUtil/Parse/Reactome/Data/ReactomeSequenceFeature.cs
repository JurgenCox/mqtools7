using MqUtil.Parse.Reactome.Misc;

namespace MqUtil.Parse.Reactome.Data {
	public class ReactomeSequenceFeature : ReactomeItem, INamedItem, IShortNamedItem, IXrefItem {
		public string Name { get; set; }
		public string ShortName { get; set; }
		public string FeatureType { get; set; }
		public string FeatureLocation { get; set; }
		private readonly List<string> xrefs = new List<string>();
		public List<string> Xrefs { get { return xrefs; } }
	}
}
