
using MqApi.Generic;
using MqApi.Param;

namespace MqApi.Spatial
{
  public interface ISpatialUpload : ISpatialActivity, IUpload
  {
    void LoadData(ISpatialData data, Parameters parameters, ref IData[] supplData,
      ProcessInfo processInfo);
  }
}
