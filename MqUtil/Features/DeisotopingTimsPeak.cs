using MqUtil.Data;

namespace MqUtil.Features {
	public class DeisotopingTimsPeak {
		public readonly int[] frameInds;
		public readonly int[] imsScanInds;
		public readonly float[] smIntens;
		public readonly int minFrameInd;
		public readonly int maxFrameInd;
		public readonly int minTimsScanInd;
		public readonly int maxTimsScanInd;

		public DeisotopingTimsPeak(ImsPeak p) {
			p.GetProfile(out frameInds, out imsScanInds, out smIntens);
			minFrameInd = p.MinFrameInd;
			maxFrameInd = p.MaxFrameInd;
			minTimsScanInd = p.MinImsScanInd;
			maxTimsScanInd = p.MaxImsScanInd;
		}
	}
}
