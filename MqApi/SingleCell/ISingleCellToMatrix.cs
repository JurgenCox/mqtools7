
using MqApi.Generic;
using MqApi.Matrix;
using MqApi.Param;

namespace MqApi.SingleCell
{
  public interface ISingleCellToMatrix : ISingleCellActivity, IToMatrix
  {
    void ProcessData(ISingleCellData inData, IMatrixData outData, Parameters param, ref IData[] supplData,
      ProcessInfo processInfo);
    Parameters GetParameters(ISingleCellData data, ref string errString);
  }
}
