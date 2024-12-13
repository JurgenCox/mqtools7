namespace MqUtil.Parse.Sbml{
	public class SbmlParameter : SbmlItem{
		public string Name { get; set; }
		public bool Constant { get; set; }
		public string SboTerm { get; set; }
		public double Value { get; set; }
	}
}