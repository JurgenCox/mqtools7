using MqApi.Generic;
using MqApi.Param;

namespace MqApi.SingleCell
{
  public interface ISingleCellExport : ISingleCellActivity, IExport
  {
    void Export(Parameters parameters, ISingleCellData data, ProcessInfo processInfo);
    Parameters GetParameters(ISingleCellData data, ref string errString);
  }
}
