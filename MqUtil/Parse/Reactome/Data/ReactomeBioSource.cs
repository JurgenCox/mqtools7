using MqUtil.Parse.Reactome.Misc;

namespace MqUtil.Parse.Reactome.Data {
	public class ReactomeBioSource : ReactomeItem,INamedItem {
		public string Name { get; set; }
		public string TaxonXref { get; set; }
	}
}
