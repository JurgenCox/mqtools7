using MqApi.Document;
using MqApi.Generic;
using MqApi.Param;
namespace MqApi.Matrix{
	public interface IMatrixProcessing : IMatrixActivity, IProcessing{
        /// <summary>
        /// This is where the actual calculation is implemented. 
        /// </summary>
        /// <param name="mdata">A clone of the previous matrix. Changes can be made in place.</param>
        /// <param name="param">Set of parameters with values as they were specified by the user</param>
        /// <param name="supplTables">Additional matrices can be created which will appear as additional output nodes in the workflow.</param>
        /// <param name="documents">Documents with textual information on the analysis results can be created here.</param>
        /// <param name="processInfo">Object used to communicate with this process.</param>
        void ProcessData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents,
			ProcessInfo processInfo);
		/// <summary>
		/// Define here the parameters that determine the specifics of the processing.
		/// </summary>
		/// <param name="mdata">The parameters might depend on the data matrix.</param>
		/// <param name="errString">Set this to a value != null if an error occured. The error string will be displayed to the user.</param>
		/// <returns>The set of parameters.</returns>
		Parameters GetParameters(IMatrixData mdata, ref string errString);
	}
}