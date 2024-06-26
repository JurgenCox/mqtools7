using MqApi.Drawing;
using MqApi.Param;
namespace PluginInterop.Python{
	public class MatrixProcessing : PluginInterop.MatrixProcessing{
		public override string Name => "Matrix => Python";
		public override string Description => "Run Python script";
		protected override string CodeFilter => "Python script, *.py | *.py";
		public override Bitmap2 DisplayImage => Bitmap2.GetImage("python.png");
		/// <summary>
		/// List of all required python packages.
		/// These packages will be checked by <see cref="ExecutableParam"/>.
		/// </summary>
		protected virtual string[] ReqiredPythonPackages => new[]{"perseuspy"};
		protected override FileParam ExecutableParam(){
			return Utils.CreateCheckedFileParam(InterpreterLabel, InterpreterFilter, TryFindExecutable,
				ReqiredPythonPackages);
		}
		protected override bool TryFindExecutable(out string path){
			return Utils.TryFindPythonExecutable(out path);
		}
	}
}