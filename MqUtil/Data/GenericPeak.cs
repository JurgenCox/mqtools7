using MqApi.Num;
using MqUtil.Num;
namespace MqUtil.Data{
	public abstract class GenericPeak{
		public int[] ScanIndices{ get; set; }
		public float[] OrigIntensityProfile{ get; set; }
		public abstract int Count{ get; }
		public abstract int FirstScanIndex{ get; }
		public abstract int LastScanIndex{ get; }
		public abstract double CalcAverageMz(CubicSpline[] mzDependentCalibration,
			LinearInterpolator intensDependentCalibration, LinearInterpolator mobilityDependentCalibration,
			bool massRecalibrationInPpm, double intensity, int imsStep);
		public abstract double CalcAverageMzUncalibrated();
		public abstract double GetIntensityAtScanIndex(int scanInd, out double mz);
		public int GetScanIndex(int index){
			return ScanIndices[index];
		}
		public int GetMaxIntensityScanIndex(){
			return ScanIndices[OrigIntensityProfile.MaxInd()];
		}
		public float GetOriginalIntensity(int index){
			return OrigIntensityProfile[index];
		}
		public virtual void Dispose(){
			ScanIndices = null;
			OrigIntensityProfile = null;
		}
	}
}