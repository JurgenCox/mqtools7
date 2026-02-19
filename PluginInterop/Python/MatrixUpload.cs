using MqApi.Drawing;
using MqApi.Param;
namespace PluginInterop.Python{
	public class MatrixUpload : PluginInterop.MatrixUpload{
		public override string Name => "Python upload";
		public override string Description => "Upload a matrix using Python";
		public override Bitmap2 DisplayImage => Bitmap2.GetImage("python.png");
		protected override string CodeFilter => "Python script, *.py | *.py";
		protected virtual string[] ReqiredPythonPackages => new[]{"perseuspy"};
        protected override FileParam ExecutableParam(){
			return Utils.CreateCheckedFileParam(InterpreterLabel, InterpreterFilter, TryFindExecutable,
				new[]{"perseuspy"});
		}
		protected override bool TryFindExecutable(out string path){
			return Utils.GetPythonPath(out path);
		}
		public override EditorType Edit => EditorType.CodePython;
	}
}