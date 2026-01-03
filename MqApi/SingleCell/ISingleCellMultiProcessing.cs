
using MqApi.Generic;
using MqApi.Param;

namespace MqApi.SingleCell
{
  public interface ISingleCellMultiProcessing : ISingleCellActivity, IMultiProcessing
  {
    ISingleCellData ProcessData(ISingleCellData[] inputData, Parameters param, ref IData[] supplData,
      ProcessInfo processInfo);
    Parameters GetParameters(ISingleCellData[] inputData, ref string errString);
  }
}
