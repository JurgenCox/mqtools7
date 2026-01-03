
using MqApi.Generic;
using MqApi.Matrix;
using MqApi.Param;

namespace MqApi.Spatial
{
  public interface ISpatialToMatrix : ISpatialActivity, IToMatrix
  {
    void ProcessData(ISpatialData inData, IMatrixData outData, Parameters param, ref IData[] supplData,
      ProcessInfo processInfo);
    Parameters GetParameters(ISpatialData data, ref string errString);
  }
}
