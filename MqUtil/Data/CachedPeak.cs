using MqApi.Num;
using MqUtil.Num;

namespace MqUtil.Data{
	/// <summary>
	/// The part of a Peak that intensity precalculation and multiplet profiles need, so that a whole peak list can be
	/// held in memory and read in one pass instead of one seek per member.
	/// </summary>
	public sealed class CachedPeak : GenericPeak{
		private double[] calibCenterMz;

		public CachedPeak(int[] scanIndices, float[] origIntensityProfile, double[] calibCenterMz){
			ScanIndices = scanIndices;
			OrigIntensityProfile = origIntensityProfile;
			this.calibCenterMz = calibCenterMz;
		}

		public override int Count => ScanIndices.Length;
		public override int FirstScanIndex => ScanIndices[0];
		public override int LastScanIndex => ScanIndices[ScanIndices.Length - 1];

		public override double GetIntensityAtScanIndex(int scanInd, out double mz){
			int a = Array.BinarySearch(ScanIndices, scanInd);
			if (a < 0){
				mz = double.NaN;
				return double.NaN;
			}
			mz = calibCenterMz[a];
			return OrigIntensityProfile[a];
		}

		public override double CalcAverageMz(CubicSpline[] mzDependentCalibration,
			LinearInterpolator intensDependentCalibration, LinearInterpolator mobilityDependentCalibration,
			bool massRecalibrationInPpm, double intensity, int imsStep){
			throw new NotSupportedException();
		}

		public override double CalcAverageMzUncalibrated(){
			throw new NotSupportedException();
		}

		public override void Dispose(){
			base.Dispose();
			calibCenterMz = null;
		}
	}
}
