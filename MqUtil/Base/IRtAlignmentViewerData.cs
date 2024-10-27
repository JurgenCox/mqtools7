namespace MqUtil.Base
{
	public interface IRtAlignmentViewerData : IChromatogramData {
		Tuple<double[], double[]> GetRtCalibrationFunction(int i, bool positive);
	}
}
