
using MqApi.Generic;
using MqApi.Param;

namespace MqApi.SingleCell
{
  public interface ISingleCellProcessing : ISingleCellActivity, IProcessing
  {
    void ProcessData(ISingleCellData data, Parameters param, ref IData[] supplData, ProcessInfo processInfo);
    Parameters GetParameters(ISingleCellData data, ref string errString);
  }
}
