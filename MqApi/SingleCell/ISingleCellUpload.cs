
using MqApi.Generic;
using MqApi.Param;

namespace MqApi.SingleCell
{
  public interface ISingleCellUpload : ISingleCellActivity, IUpload
  {
    void LoadData(ISingleCellData data, Parameters parameters, ref IData[] supplData,
      ProcessInfo processInfo);
  }
}
