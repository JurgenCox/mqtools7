
using MqApi.Generic;
using MqApi.Param;

namespace MqApi.Spatial
{
  public interface ISpatialMultiAnalysis : ISpatialActivity, IMultiAnalysis
  {
    IAnalysisResult AnalyzeData(ISpatialData[] data, Parameters param, ProcessInfo processInfo);
    Parameters GetParameters(ISpatialData[] data, ref string errString);
  }
}
