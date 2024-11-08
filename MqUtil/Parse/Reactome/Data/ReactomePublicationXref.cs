using MqUtil.Parse.Reactome.Misc;

namespace MqUtil.Parse.Reactome.Data {
	public class ReactomePublicationXref : ReactomeItem, IDbIdItem {
		public Database Db { get; set; }
		public string Id { get; set; }
		public int Year { get; set; }
		public string Title { get; set; }
		private readonly List<string> authors = new List<string>();
		public List<string> Authors { get { return authors; } }
		public string Source { get; set; }
		public string Url { get; set; }
	}
}
