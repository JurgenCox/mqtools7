using MqApi.Generic;
using MqApi.Matrix;
using MqApi.Param;
namespace MqApi.Document{
	public interface IDocumentUpload : IDocumentActivity, IUpload{
		void LoadData(IDocumentData matrixData, Parameters parameters, ref IMatrixData[] supplTables,
			ref IDocumentData[] documents, ProcessInfo processInfo);
	}
}