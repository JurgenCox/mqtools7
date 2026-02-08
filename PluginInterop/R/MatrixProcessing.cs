using MqApi.Drawing;
using MqApi.Param;
namespace PluginInterop.R{
	public class MatrixProcessing : PluginInterop.MatrixProcessing{
		public override string Name => "R";
		public override string Description => "Run R script";
		public override Bitmap2 DisplayImage => Bitmap2.GetImage("Rlogo.png");
		protected override string CodeFilter => "R script, *.R | *.R";
		protected override FileParam ExecutableParam(){
			return Utils.CreateCheckedFileParam(InterpreterLabel, InterpreterFilter, TryFindExecutable);
		}
		protected override bool TryFindExecutable(out string path){
			return Utils.TryFindRExecutable(out path);
		}
		public override EditorType Edit => EditorType.CodeR;
	}
}