namespace MqUtil.Parse.Sbml{
	public class SbmlEvent : SbmlItem{
		public string Name { get; set; }
		public SbmlEventAssignment EventAssignment { get; set; }
	}
}