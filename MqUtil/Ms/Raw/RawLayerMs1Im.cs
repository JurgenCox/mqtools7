using MqUtil.Ms.Utils;

namespace MqUtil.Ms.Raw{
	public class RawLayerMs1Im : RawLayerIm {
		public RawLayerMs1Im(RawFileLayer layer, int frameIndex, double resolution, double gridSpacing,
			int imsHalfWidth, int imsStep) : base(layer,  frameIndex,  resolution, gridSpacing,
			imsHalfWidth, imsStep) {
		}
		public override double[] GetMassRange(int i){
			double[] result = new double[2];
			rawLayer.GetMs1MassRange(frameIndex, out result[0], out result[1]);
			return result;
		}
		public override Spectrum GetSpectrum(int j, bool readCentroids){
			if (!Buffered){
				return rawLayer.GetMs1Spectrum(frameIndex, frameIndex, j * imsStep - imsHalfWidth, j * imsStep + imsHalfWidth, readCentroids,
					resolution, gridSpacing, double.NaN, double.NaN);
			}
			if (!map.ContainsKey(j)){
				map.Clear();
				int max = Math.Min(j + Capacity, Count);
				for (int i = j; i < max; i++){
					map.Add(i,
						rawLayer.GetMs1Spectrum(frameIndex, frameIndex, i * imsStep - imsHalfWidth, i * imsStep + imsHalfWidth,
							readCentroids, resolution, gridSpacing, double.NaN, double.NaN));
				}
			}
			return map[j];
		}
		public override bool HasProfile(int j){
			return rawLayer.HasMs1Profile(frameIndex);
		}
	}
}