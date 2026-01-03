
using MqApi.Generic;
using MqApi.Matrix;
using MqApi.Param;

namespace MqApi.Spatial
{
  public interface ISpatialMergeWithMatrix : ISpatialActivity, IMergeWithMatrix
  {
    void ProcessData(ISpatialData data, IMatrixData inMatrix, Parameters param, ref IData[] supplData,
      ProcessInfo processInfo);
    Parameters GetParameters(ISpatialData data, IMatrixData inMatrix, ref string errString);
  }
}
