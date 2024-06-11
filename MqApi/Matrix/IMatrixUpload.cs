using MqApi.Document;
using MqApi.Generic;
using MqApi.Param;
namespace MqApi.Matrix{
	public interface IMatrixUpload : IMatrixActivity, IUpload{
		void LoadData(IMatrixData mdata, Parameters param, ref IMatrixData[] supplTables, ref IDocumentData[] documents,
			ProcessInfo processInfo);
	}
}