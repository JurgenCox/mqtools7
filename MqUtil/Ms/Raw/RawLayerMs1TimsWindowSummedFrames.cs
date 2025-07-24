using MqApi.Num;
using MqUtil.Ms.Utils;
namespace MqUtil.Ms.Raw{
	public class RawLayerMs1TimsWindowSummedFrames : RawLayer{
		private readonly RawFileLayer rawFile;
		private readonly int imsIndexMin;
		private readonly int imsIndexMax;
		private readonly double resolution;
		private readonly double gridSpacing;
		public readonly double[] rts;
		private readonly double minMz;
		private readonly double maxMz;
		public RawLayerMs1TimsWindowSummedFrames(RawFileLayer rawFile, int imsIndexMin, int imsIndexMax,
			double resolution, double gridSpacing, double minMz, double maxMz){
			this.rawFile = rawFile;
			this.imsIndexMin = imsIndexMin;
			this.imsIndexMax = imsIndexMax;
			this.resolution = resolution;
			this.gridSpacing = gridSpacing;
			this.minMz = minMz;
			this.maxMz = maxMz;
			rts = rawFile.Ms1Rt;
		}
		public static void Prepare(double[] ms1Rt, out int[] frameIndMin, out int[] frameIndMax, 
			out double[] newRTs){
			frameIndMin = ArrayUtils.ConsecutiveInts(ms1Rt.Length);
			frameIndMax = ArrayUtils.ConsecutiveInts(ms1Rt.Length);
			newRTs = ms1Rt;
		}
		public override int Count => rts.Length;
		public override int MassRangeCount => rawFile.Ms1MassRangeCount;
		public override double[] GetMassRange(int i){
			double[] result = new double[2];
			rawFile.GetMs1MassRange(i, out result[0], out result[1]);
			return result;
		}
		public override Spectrum GetSpectrum(int j, bool readCentroids){
			if (!Buffered){
				return rawFile.GetMs1Spectrum(j, j, imsIndexMin, imsIndexMax, readCentroids,
					resolution, gridSpacing, minMz, maxMz);
			}
			if (!map.ContainsKey(j)){
				map.Clear();
				int max = Math.Min(j + Capacity, Count);
				for (int i = j; i < max; i++){
					map.Add(i,
						rawFile.GetMs1Spectrum(i, i, imsIndexMin, imsIndexMax, readCentroids,
							resolution, gridSpacing, minMz, maxMz));
				}
			}
			return map[j];
		}
		public override bool HasProfile(int j){
			return true;
		}
		public override byte GetMassRangeIndex(int i){
			return rawFile.GetMs1MassRangeIndex(i);
		}
		public override double[] GetTimeSpan(int i){
			double[] f1 = rawFile.GetMs1TimeSpan(i);
			double[] f2 = rawFile.GetMs1TimeSpan(i);
			return [f1[0], f2[1]];
		}
		public override double GetTime(int i){
			return rawFile.GetMs1Time(i);
		}
		public override int GetIndexFromRt(double time){
			int indOrig = rawFile.GetMs1IndexFromRt(time);
			return indOrig;
		}
		public override int Capacity => 50;
	}
}