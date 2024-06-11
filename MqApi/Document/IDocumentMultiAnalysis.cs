using MqApi.Generic;
using MqApi.Param;
namespace MqApi.Document{
	public interface IDocumentMultiAnalysis{
		IAnalysisResult AnalyzeData(IDocumentData[] mdata, Parameters param, ProcessInfo processInfo);
		/// <summary>
		/// Define here the parameters that determine the specifics of the analysis.
		/// </summary>
		/// <param name="mdata">The parameters might depend on the document.</param>
		/// <param name="errString">Set this to a value != null if an error occured. The error string will be displayed to the user.</param>
		/// <returns>The set of parameters.</returns>
		Parameters GetParameters(IDocumentData[] mdata, ref string errString);
	}
}