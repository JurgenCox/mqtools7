using MqApi.Generic;
using MqApi.Matrix;
using MqApi.Param;

namespace MqApi.SingleCell
{
  public interface ISingleCellFromMatrix : ISingleCellActivity, IFromMatrix
  {
    void ProcessData(IMatrixData inData, ISingleCellData outData, Parameters param, ref IData[] supplData,
      ProcessInfo processInfo);
    Parameters GetParameters(IMatrixData data, ref string errString);
  }
}
