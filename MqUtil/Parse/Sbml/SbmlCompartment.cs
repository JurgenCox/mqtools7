namespace MqUtil.Parse.Sbml{
	public class SbmlCompartment : SbmlItem{
		public string Name { get; set; }
		public bool Constant { get; set; }
		public int SpatialDimensions { get; set; }
		public string SboTerm { get; set; }
		public double Size { get; set; }
		public string Outside { get; set; }
	}
}