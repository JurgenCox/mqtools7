using MqApi.Num;
namespace MqUtil.Ms.Raw{
	public class RawLayerMs1TimsWindowSummedFrames : RawLayer{
		private readonly RawFileLayer rawFile;
		private readonly int imsIndexMin;
		private readonly int imsIndexMax;
		private readonly double resolution;
		private readonly double gridSpacing;
		private readonly int[] frameIndMin;
		private readonly int[] frameIndMax;
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
			Prepare(rawFile.Ms1ScanNumbers, rawFile.Ms1Rt, out frameIndMin, out frameIndMax, out rts);
		}

		public static void Prepare(int[] scanNumbers, double[] ms1Rt, out int[] frameIndMin,
			out int[] frameIndMax, out double[] newRTs){
			List<int> current = new List<int>();
			List<int[]> newInds = new List<int[]>();
			current.Add(0);
			for (int i = 1; i < ms1Rt.Length; i++){
				//if (scanNumbers[i] != scanNumbers[i - 1] + 1){
				newInds.Add(current.ToArray());
				current.Clear();
				//}
				current.Add(i);
			}
			newInds.Add(current.ToArray());
			frameIndMin = new int[newInds.Count];
			frameIndMax = new int[newInds.Count];
			newRTs = new double[newInds.Count];
			for (int i = 0; i < newInds.Count; i++){
				frameIndMin[i] = newInds[i][0];
				frameIndMax[i] = newInds[i][newInds[i].Length - 1];
				newRTs[i] = ms1Rt.SubArray(newInds[i]).Mean();
			}
		}

		public override int Count => frameIndMin.Length;
		public override int MassRangeCount => rawFile.Ms1MassRangeCount;

		public override double[] GetMassRange(int i){
			double[] result = new double[2];
			rawFile.GetMs1MassRange(frameIndMin[i], out result[0], out result[1]);
			return result;
		}

		public override Spectrum GetSpectrum(int j, bool readCentroids){
			if (!Buffered){
				return rawFile.GetMs1Spectrum(frameIndMin[j], frameIndMax[j], imsIndexMin, imsIndexMax, readCentroids,
					resolution, gridSpacing, minMz, maxMz);
			}
			if (!map.ContainsKey(j)){
				map.Clear();
				int max = Math.Min(j + Capacity, Count);
				for (int i = j; i < max; i++){
					map.Add(i,
						rawFile.GetMs1Spectrum(frameIndMin[i], frameIndMax[i], imsIndexMin, imsIndexMax, readCentroids,
							resolution, gridSpacing, minMz, maxMz));
				}
			}
			return map[j];
		}

		public override bool HasProfile(int j){
			return true;
		}

		public override byte GetMassRangeIndex(int i){
			return rawFile.GetMs1MassRangeIndex(frameIndMin[i]);
		}

		public override double[] GetTimeSpan(int i){
			double[] f1 = rawFile.GetMs1TimeSpan(frameIndMin[i]);
			double[] f2 = rawFile.GetMs1TimeSpan(frameIndMax[i]);
			return new[]{f1[0], f2[1]};
		}

		public override double GetTime(int i){
			return rawFile.GetMs1Time(frameIndMin[i]);
		}

		public override int GetIndexFromRt(double time){
			int indOrig = rawFile.GetMs1IndexFromRt(time);
			return ArrayUtils.FloorIndex(frameIndMin, indOrig);
		}

		public override int Capacity => 50;
	}
}