using MqUtil.Num;
namespace MqUtil.Data{
	public interface IRecalibrationData {
		CubicSpline[] MzDependentCalibration { get; set; }
		double[] ScanDependentCalibration { get; }
		bool MassRecalibrationInPpm { get; set; }
		void ReadCalibration();
		double GetDecalibratedRt(double rt);
	}
}