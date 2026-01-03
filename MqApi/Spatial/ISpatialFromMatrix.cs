using MqApi.Generic;
using MqApi.Matrix;
using MqApi.Param;

namespace MqApi.Spatial
{
  public interface ISpatialFromMatrix : ISpatialActivity, IFromMatrix
  {
    void ProcessData(IMatrixData inData, ISpatialData outData, Parameters param, ref IData[] supplData,
      ProcessInfo processInfo);
    Parameters GetParameters(IMatrixData data, ref string errString);
  }
}
