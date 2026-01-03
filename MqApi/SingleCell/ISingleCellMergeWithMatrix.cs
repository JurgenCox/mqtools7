using MqApi.Generic;
using MqApi.Matrix;
using MqApi.Param;

namespace MqApi.SingleCell
{
  public interface ISingleCellMergeWithMatrix : ISingleCellActivity, IMergeWithMatrix
  {
    void ProcessData(ISingleCellData data, IMatrixData inMatrix, Parameters param, ref IData[] supplData,
      ProcessInfo processInfo);
    Parameters GetParameters(ISingleCellData data, IMatrixData inMatrix, ref string errString);
  }
}
