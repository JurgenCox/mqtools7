namespace MqUtil.Ms.Raw{
	public abstract class RawLayerIm : RawLayer{
		protected readonly RawFileLayer rawLayer;
		protected readonly int frameIndex;
		protected readonly double resolution;
		protected readonly double gridSpacing;
		protected readonly int imsHalfWidth;
		protected readonly int imsStep;
		protected RawLayerIm(RawFileLayer rawLayer, int frameIndex, double resolution, double gridSpacing,
			int imsHalfWidth, int imsStep){
			this.rawLayer = rawLayer;
			this.frameIndex = frameIndex;
			this.resolution = resolution;
			this.gridSpacing = gridSpacing;
			this.imsHalfWidth = imsHalfWidth;
			this.imsStep = imsStep;
		}
		public override int Count => rawLayer.Ms1MaxNumIms / imsStep;
		public override int MassRangeCount => 1;
		public override byte GetMassRangeIndex(int i){
			return 0;
		}
		public override double[] GetTimeSpan(int i){
			return new[]{(i - 0.5) * imsStep, (i + 0.5) * imsStep};
		}
		public override double GetTime(int ind){
			return ind * imsStep;
		}
		public override int GetIndexFromRt(double time){
			return (int) Math.Round(time / imsStep);
		}
		public override int Capacity => 200;
	}
}