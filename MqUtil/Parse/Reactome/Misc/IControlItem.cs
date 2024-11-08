using MqUtil.Mol;

namespace MqUtil.Parse.Reactome.Misc {
	public interface IControlItem {
		string Controller { get; set; }
		string Controlled { get; set; }
		ReactionControlType ControlType { get; set; }
	}
}
