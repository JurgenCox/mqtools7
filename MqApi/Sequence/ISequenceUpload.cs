using MqApi.Generic;
using MqApi.Param;
namespace MqApi.Sequence{
	public interface ISequenceUpload : ISequenceActivity, IUpload{
		void LoadData(ISequenceData sequenceData, Parameters parameters, ref IData[] supplData,
			ProcessInfo processInfo);
	}
}