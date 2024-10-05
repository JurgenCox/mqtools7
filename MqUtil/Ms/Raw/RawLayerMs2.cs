using MqApi.Num;
using MqUtil.Ms.Utils;
namespace MqUtil.Ms.Raw{
	public class RawLayerMs2 : RawLayer{
		private readonly RawFileLayer rawFile;
		private readonly int[] indices;
		public RawLayerMs2(RawFileLayer rawFile, int[] indices){
			this.rawFile = rawFile;
			this.indices = indices;
		}
		public RawLayerMs2(RawFileLayer rawFile) : this(rawFile, null){
		}
		public override int Count => indices?.Length ?? rawFile.Ms2Count;
		public override int MassRangeCount => 1;
		public override double[] GetMassRange(int i){
			return new[]{rawFile.Ms2MassMin, rawFile.Ms2MassMax};
		}
		public override Spectrum GetSpectrum(int j, bool readCentroids){
			if (!Buffered){
				return rawFile.GetMs2Spectrum(indices?[j] ?? j, readCentroids);
			}
			if (!map.ContainsKey(j)){
				map.Clear();
				int max = Math.Min(j + Capacity, Count);
				for (int i = j; i < max; i++){
					map.Add(i, rawFile.GetMs2Spectrum(indices?[i] ?? i, readCentroids));
				}
			}
			return map[j];
		}
		public override bool HasProfile(int j){
			return rawFile.HasMs2Profile(indices?[j] ?? j);
		}
		public override byte GetMassRangeIndex(int i){
			return 0;
		}
		public override double[] GetTimeSpan(int i){
			return rawFile.GetMs2TimeSpan(indices?[i] ?? i);
		}
		public override double GetTime(int i){
			return rawFile.GetMs2Time(indices?[i] ?? i);
		}
		public override int GetIndexFromRt(double time){
			int ind = rawFile.GetMs2IndexFromRt(time);
			return indices == null ? ind : ArrayUtils.ClosestIndex(indices, ind);
		}
		public override int Capacity => 200;
	}
}