using MqApi.Generic;
using MqApi.Matrix;

namespace MqApi.SingleCell
{
  public interface ISingleCellData : IData, IDataWithAnnotationRows, IDataWithAnnotationColumns
  {
    List<IMatrixData> Values { get; set; }
    IMatrixData CreateMatrixData();
  }
}
