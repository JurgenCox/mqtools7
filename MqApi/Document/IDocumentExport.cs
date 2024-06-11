using MqApi.Generic;
using MqApi.Param;
namespace MqApi.Document{
	public interface IDocumentExport : IDocumentActivity, IExport{
		void Export(Parameters parameters, IDocumentData data, ProcessInfo processInfo);
		/// <summary>
		/// Define here the parameters that determine the specifics of the export.
		/// </summary>
		/// <param name="mdata">The parameters might depend on the data matrix.</param>
		/// <param name="errString">Set this to a value != null if an error occured. The error string will be displayed to the user.</param>
		/// <returns>The set of parameters.</returns>
		Parameters GetParameters(IDocumentData mdata, ref string errString);
	}
}