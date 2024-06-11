using MqApi.Generic;
using MqApi.Matrix;
using MqApi.Param;
namespace MqApi.Document{
	public interface IDocumentFromMatrix : IDocumentActivity, IFromMatrix{
		void ProcessData(IMatrixData inData, IDocumentData outData, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo);
		/// <summary>
		/// Define here the parameters that determine the specifics of the processing.
		/// </summary>
		/// <param name="mdata">The parameters might depend on the data matrix.</param>
		/// <param name="errString">Set this to a value != null if an error occured. The error string will be displayed to the user.</param>
		/// <returns>The set of parameters.</returns>
		Parameters GetParameters(IMatrixData mdata, ref string errString);
	}
}