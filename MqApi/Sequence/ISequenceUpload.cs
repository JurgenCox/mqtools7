using MqApi.Generic;
using MqApi.Param;
namespace MqApi.Sequence{
	public interface ISequenceUpload : ISequenceActivity, IUpload{
		void LoadData(ISequenceData data, Parameters parameters, ref IData[] supplData,
			ProcessInfo processInfo);
	}
}