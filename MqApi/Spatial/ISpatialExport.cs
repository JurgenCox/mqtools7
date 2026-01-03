using MqApi.Generic;
using MqApi.Param;

namespace MqApi.Spatial
{
  public interface ISpatialExport : ISpatialActivity, IExport
  {
    void Export(Parameters parameters, ISpatialData data, ProcessInfo processInfo);
    Parameters GetParameters(ISpatialData data, ref string errString);
  }
}
