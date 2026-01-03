using MqApi.Generic;
using MqApi.Param;

namespace MqApi.SingleCell
{
  public interface ISingleCellAnalysis : ISingleCellActivity, IAnalysis
  {
    IAnalysisResult AnalyzeData(ISingleCellData data, Parameters param, ProcessInfo processInfo);
    Parameters GetParameters(ISingleCellData data, ref string errString);
  }
}
