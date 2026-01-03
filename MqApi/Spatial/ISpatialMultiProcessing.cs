
using MqApi.Generic;
using MqApi.Param;

namespace MqApi.Spatial
{
  public interface ISpatialMultiProcessing : ISpatialActivity, IMultiProcessing
  {
    ISpatialData ProcessData(ISpatialData[] inputData, Parameters param, ref IData[] supplData,
      ProcessInfo processInfo);
    Parameters GetParameters(ISpatialData[] inputData, ref string errString);
  }
}
