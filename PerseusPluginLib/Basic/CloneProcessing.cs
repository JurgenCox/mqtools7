using MqApi.Document;
using MqApi.Drawing;
using MqApi.Generic;
using MqApi.Matrix;
using MqApi.Param;
namespace PerseusPluginLib.Basic{
	public class CloneProcessing : IMatrixProcessing{
		public string Category => IMatrixProcessingCategories.DataHandling;
        public bool HasButton => false;
		public Bitmap2 DisplayImage => Bitmap2.GetImage("sheepButton.Image.png");
		public string Description => "A copy of the input matrix is generated.";
		public string HelpOutput => "Same as input matrix.";
		public string[] HelpSupplTables => new string[0];
		public int NumSupplTables => 0;
		public string Name => "Clone";
		public string Heading => "Matrix structure operations";
		public bool IsActive => true;
		public float DisplayRank => 100;
		public string[] HelpDocuments => new string[0];
		public int NumDocuments => 0;
		public int GetMaxThreads(Parameters parameters){
			return 1;
		}
		public string Url
			=> "https://cox-labs.github.io/coxdocs/cloneprocessing.html";
		public void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo){
		}
		public Parameters GetParameters(IMatrixData mdata, ref string errorString){
			return new Parameters(new Parameter[]{ });
		}
	}
}