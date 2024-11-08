namespace MqUtil.Parse.Psp{
	public class PspProtein{
		public Dictionary<int, PspPtmSite> sites = new Dictionary<int, PspPtmSite>();
		public string Name { get; set; }

		public PspProtein(string name){
			Name = name;
		}
	}
}