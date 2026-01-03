using MqApi.Generic;
using MqApi.Matrix;
using MqApi.Param;
namespace MqApi.Sequence{
	public interface ISequenceToMatrix : ISequenceActivity, IToMatrix{
		void ProcessData(ISequenceData inData, IMatrixData outData, Parameters param, ref IData[] supplData,
			ProcessInfo processInfo);
		/// <summary>
		/// Define here the parameters that determine the specifics of the processing.
		/// </summary>
		/// <param name="mdata">The parameters might depend on the data matrix.</param>
		/// <param name="errString">Set this to a value != null if an error occured. The error string will be displayed to the user.</param>
		/// <returns>The set of parameters.</returns>
		Parameters GetParameters(ISequenceData data, ref string errString);
	}
}