using MqUtil.Mol;
namespace MqUtil.Ms.Predict {
	public class PeptideInstance {
		public string Sequence { get; set; }
		public PeptideModificationState Modifications { get; set; }
	}
}
