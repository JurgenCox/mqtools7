using MqApi.Generic;
using MqApi.Num.Matrix;

namespace MqApi.SingleCell
{
  public interface ISingleCellData : IData, IDataWithAnnotationRows, IDataWithAnnotationColumns
  {
    List<MatrixIndexer> Values { get; set; }
    List<DataWithAnnotationRows> Annot { get; set; }

  }
}
