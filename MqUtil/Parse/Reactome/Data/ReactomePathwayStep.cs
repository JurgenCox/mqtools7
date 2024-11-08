namespace MqUtil.Parse.Reactome.Data {
	public class ReactomePathwayStep : ReactomeItem {
		private readonly List<string> stepInteractions = new List<string>();
		public List<string> StepInteractions { get { return stepInteractions; } }
		private readonly List<string> nextSteps = new List<string>();
		public List<string> NextSteps { get { return nextSteps; } }
	}
}
