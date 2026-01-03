
using MqApi.Generic;
using MqApi.Param;

namespace MqApi.Spatial
{
  public interface ISpatialProcessing : ISpatialActivity, IProcessing
  {
    void ProcessData(ISpatialData data, Parameters param, ref IData[] supplData, ProcessInfo processInfo);
    Parameters GetParameters(ISpatialData data, ref string errString);
  }
}
