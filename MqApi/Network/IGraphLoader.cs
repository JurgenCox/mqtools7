using MqApi.Generic;
using MqApi.Param;
namespace MqApi.Network{
	public interface IGraphLoader{
		Parameters GetParameters();
		void Load(INetworkData ndata, Parameters parameters, ProcessInfo processInfo);
	}
}