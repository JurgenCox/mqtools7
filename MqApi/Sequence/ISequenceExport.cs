using MqApi.Generic;
using MqApi.Param;
namespace MqApi.Sequence{
	public interface ISequenceExport : ISequenceActivity, IExport{
		void Export(Parameters parameters, ISequenceData data, ProcessInfo processInfo);
		/// <summary>
		/// Define here the parameters that determine the specifics of the export.
		/// </summary>
		/// <param name="data">The parameters might depend on the data matrix.</param>
		/// <param name="errString">Set this to a value != null if an error occured. The error string will be displayed to the user.</param>
		/// <returns>The set of parameters.</returns>
		Parameters GetParameters(ISequenceData data, ref string errString);
	}
}