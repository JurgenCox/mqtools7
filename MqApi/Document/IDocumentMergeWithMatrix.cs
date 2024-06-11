using MqApi.Generic;
using MqApi.Matrix;
using MqApi.Param;
namespace MqApi.Document{
	public interface IDocumentMergeWithMatrix : IDocumentActivity, IMergeWithMatrix{
		void ProcessData(IDocumentData data, IMatrixData inMatrix, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo);
		/// <summary>
		/// Define here the parameters that determine the specifics of the processing.
		/// </summary>
		/// <param name="data">The parameters might depend on the data.</param>
		/// <param name="inMatrix">and on the matrix as well.</param>
		/// <param name="errString">Set this to a value != null if an error occured. The error string will be displayed to the user.</param>
		/// <returns>The set of parameters.</returns>
		Parameters GetParameters(IDocumentData data, IMatrixData inMatrix, ref string errString);
	}
}