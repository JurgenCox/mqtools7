using MqApi.Num;
using MqUtil.Ms.Enums;

namespace MqUtil.Data{
	/// <summary>
	/// Similar to Peak, but based on lists instead of arrays. 
	/// Can be converted to a Peak with the method ToPeak.
	/// </summary>
	public class GrowablePeak{
		// These fields correspond to the given fields in Peak.
		private List<double> mzCentroid;
		private List<double> mzMin;
		private List<double> mzMax;
		private List<byte> massRange;
		private List<float> origIntensity;
		private List<int> scanIndex;
		private List<bool> hasLeftNeighbor;
		private List<bool> hasRightNeighbor;
		private List<bool> gaps;
		private List<float> fwhms;
		public GrowablePeak(bool calcResolution) {
			mzCentroid = new List<double>();
			mzMin = new List<double>();
			mzMax = new List<double>();
			massRange = new List<byte>();
			origIntensity = new List<float>();
			scanIndex = new List<int>();
			hasLeftNeighbor = new List<bool>();
			hasRightNeighbor = new List<bool>();
			gaps = new List<bool>();
			if (calcResolution){
				fwhms = new List<float>();
			}
		}
		public int LastScanIndex => mzCentroid.Count == 0 ? -1 : scanIndex[mzCentroid.Count - 1];
		public int FirstScanIndex => scanIndex.Count == 0 ? -1 : scanIndex[0];
		/// <summary>
		/// Extend a GrowablePeak based on one element of an Ms1CentroidList.
		/// </summary>
		public void Add(int scanInd, int peakIndex, Ms1CentroidList peakList, byte massRang, bool gap){
			// Add to the lists of this GrowablePeak, and give the Ms1CentroidList a reference to this object,
			mzCentroid.Add(peakList.GetMzCentroid(peakIndex));
			mzMin.Add(peakList.MzMin[peakIndex]);
			mzMax.Add(peakList.MzMax[peakIndex]);
			origIntensity.Add(peakList.PeakIntensity[peakIndex]);
			scanIndex.Add(scanInd);
			massRange.Add(massRang);
			hasLeftNeighbor.Add(peakList.HasLeftNeighbor[peakIndex]);
			hasRightNeighbor.Add(peakList.HasRightNeighbor[peakIndex]);
			peakList.SetPeak(this, peakIndex);
			gaps.Add(gap);
			if (fwhms != null){
				float fwhm = peakList.Resolution != null ? peakList.Resolution[peakIndex] : float.NaN;
				if (!float.IsNaN(fwhm)){
					fwhms.Add(fwhm);
				}
			}
		}
		public void Dispose(){
			mzCentroid?.Clear();
			mzMin?.Clear();
			mzMax?.Clear();
			origIntensity?.Clear();
			scanIndex?.Clear();
			massRange?.Clear();
			gaps?.Clear();
			hasLeftNeighbor?.Clear();
			hasRightNeighbor?.Clear();
			fwhms?.Clear();
			mzCentroid = null;
			mzMin = null;
			mzMax = null;
			origIntensity = null;
			scanIndex = null;
			massRange = null;
			gaps = null;
			hasLeftNeighbor = null;
			hasRightNeighbor = null;
			fwhms = null;
		}
		public bool IsDisposed(){
			return mzCentroid == null;
		}
		public Peak ToPeak(IntensityDetermination intensityDetermination){
			List<int> valids = new List<int>();
			if (origIntensity[0] > 0){
				valids.Add(0);
			}
			for (int i = 1; i < scanIndex.Count; i++){
				if (scanIndex[i] != scanIndex[i - 1] && origIntensity[i] > 0){
					valids.Add(i);
				}
			}
			int[] v = valids.ToArray();
			return new Peak(mzCentroid.SubArray(v), ArrayUtils.ToFloats(mzMin.SubArray(v)),
				ArrayUtils.ToFloats(mzMax.SubArray(v)), new float[valids.Count], origIntensity.SubArray(v),
				scanIndex.SubArray(v), massRange.SubArray(v), gaps.SubArray(v), intensityDetermination,
				hasLeftNeighbor.SubArray(v), hasRightNeighbor.SubArray(v),
				fwhms != null && fwhms.Count > 0 ? fwhms.Median() : float.NaN);
				;
		}
	}
}