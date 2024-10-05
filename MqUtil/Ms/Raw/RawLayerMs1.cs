using MqApi.Num;
using MqUtil.Ms.Utils;
namespace MqUtil.Ms.Raw{
	public class RawLayerMs1 : RawLayer{
		private readonly RawFileLayer rawFile;
		private readonly int[] inds;
		private readonly double resolution;
		private readonly bool inMda;
		public RawLayerMs1(RawFileLayer rawFile) : this(rawFile, null, double.NaN, false){
		}
		public RawLayerMs1(RawFileLayer rawFile, double resolution, bool inMda) :
			this(rawFile, null, resolution, inMda){
		}
		public RawLayerMs1(RawFileLayer rawFile, int[] inds) : this(rawFile, inds, double.NaN, false){
		}
		public RawLayerMs1(RawFileLayer rawFile, int[] inds, double resolution, bool inMda){
			this.rawFile = rawFile;
			this.inds = inds;
			this.resolution = resolution;
			this.inMda = inMda;
		}
		public override int Count => inds?.Length ?? rawFile.Ms1Count;
		public override int MassRangeCount => rawFile.Ms1MassRangeCount;
		/// <summary>
		/// Return a two-element vector with the min and max masses of the RawFileLayer
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public override double[] GetMassRange(int i){
			double[] result = new double[2];
			int ind = inds?[i] ?? i;
			rawFile.GetMs1MassRange(ind, out result[0], out result[1]);
			return result;
		}
		public override Spectrum GetSpectrum(int j, bool readCentroids){
			if (!Buffered){
				int ind = inds?[j] ?? j;
				return GetSpectrumImpl(ind, readCentroids);
			}
			if (!map.ContainsKey(j)){
				map.Clear();
				int max = Math.Min(j + Capacity, Count);
				for (int i = j; i < max; i++){
					int ind = inds?[i] ?? i;
					map.Add(i, GetSpectrumImpl(ind, readCentroids));
				}
			}
			return map[j];
		}
		public override bool HasProfile(int i){
			if (!double.IsNaN(resolution)){
				return true;
			}
			int ind = inds?[i] ?? i;
			return rawFile.HasMs1Profile(ind);
		}
		public override byte GetMassRangeIndex(int i){
			int ind = inds?[i] ?? i;
			return rawFile.GetMs1MassRangeIndex(ind);
		}
		public override double[] GetTimeSpan(int i){
			int ind = inds?[i] ?? i;
			return rawFile.GetMs1TimeSpan(ind);
		}
		public override double GetTime(int i){
			int ind = inds?[i] ?? i;
			return rawFile.GetMs1Time(ind);
		}
		public override int GetIndexFromRt(double time){
			int ind = rawFile.GetMs1IndexFromRt(time);
			return inds == null ? ind : ArrayUtils.ClosestIndex(inds, ind);
		}
		public override int Capacity => 200;
		private Spectrum GetSpectrumImpl(int ind, bool readCentroids){
			Spectrum spec = rawFile.GetMs1Spectrum(ind, readCentroids);
			if (double.IsNaN(resolution)){
				return spec;
			}
			return spec.Smooth(resolution, inMda);
		}
	}
}