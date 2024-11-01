using MqUtil.Num;

namespace MqUtil.Data.Pdetect{
	public class PeakDetection3DResultMs1 : IPeakDetection3DResult{
		private readonly IPeakListLayerData peakList;

		public PeakDetection3DResultMs1(IPeakListLayerData peakList){
			this.peakList = peakList;
		}

		public double GetCalibratedRtFromRt(double t){
			return peakList.GetCalibratedRtFromRt(t);
		}

		public int Count => peakList.PeakCount;

		public long[] FilePos => peakList.FilePos;

		public int[] IsotopeClusterIndices{
			set => peakList.IsotopeClusterIndices = value;
		}

		public int GetIsotopeClusterIndex(int i){
			if (peakList.IsotopeClusterIndices == null){
				return -1;
			}
			return peakList.IsotopeClusterIndices[i];
		}

		public int GetCustomPeakColorIndex(int i){
			return -1;
		}

		public int[][] GetIsoClusterIdsByCharge(int minCharge, int maxCharge){
			return peakList.IsoClusterIdsByCharge ?? (peakList.IsoClusterIdsByCharge =
				       Deisotoping.SortIsotopeClusterIndicesByCharge(minCharge, maxCharge, this));
		}

		public void SetIsoClusterIdsByCharge(int[][] value){
			peakList.IsoClusterIdsByCharge = value;
		}

		public Peak GetPeak(int i){
			return peakList.GetPeak(i);
		}

		public int IsotopeClusterCount => peakList.IsotopeClusterCount;

		public double[] CenterMz{
			get => peakList.CenterMz;
			set => peakList.CenterMz = value;
		}

		public float[] Intensities{
			get => peakList.Intensities;
			set => peakList.Intensities = value;
		}

		public Multiplet[] Multiplets{
			set => peakList.Multiplets = value;
		}

		public float[] MinTimes{
			get => peakList.MinTimes;
			set => peakList.MinTimes = value;
		}

		public float[] MaxTimes{
			get => peakList.MaxTimes;
			set => peakList.MaxTimes = value;
		}

		public int[] MinRtIndsTmp{
			get => peakList.MinRtIndsTmp;
			set => peakList.MinRtIndsTmp = value;
		}

		public int[] MaxRtIndsTmp{
			get => peakList.MaxRtIndsTmp;
			set => peakList.MaxRtIndsTmp = value;
		}

		public int MultipletCount => peakList.MultipletCount;

		public Dictionary<int, LinearInterpolator> Medians10{
			get => peakList.Medians10;
			set => peakList.Medians10 = value;
		}

		public Dictionary<int, LinearInterpolator> Medians20{
			get => peakList.Medians20;
			set => peakList.Medians20 = value;
		}

		public Dictionary<int, LinearInterpolator> Medians21{
			get => peakList.Medians21;
			set => peakList.Medians21 = value;
		}

		public double GetIsotopeClusterIntensity(int i){
			return peakList.GetIsotopeClusterIntensity(i, GetPeak);
		}

		public IsotopeCluster GetIsotopeCluster(int i){
			return peakList.GetGenericIsotopeCluster(i);
		}

		public Multiplet GetMultiplet(int i){
			return peakList.GetMultiplet(i);
		}

		public void WriteQuartiles(){
			peakList.WriteQuartiles();
		}

		/// <summary>
		/// From the peaks as described by MinTimes, MaxTimes, and CenterMz, choose those within the given mz and rt ranges.
		/// </summary>
		/// <returns></returns>
		public int[] GetPeaksInRectangle(double mzRangeMin, double mzRangeMax, double rtRangeMin, double rtRangeMax,
			bool recalTimes){
			return peakList.GetPeaksInRectangle(mzRangeMin, mzRangeMax, rtRangeMin, rtRangeMax, recalTimes);
		}

		public int GetMultipletCharge(int index){
			return peakList.GetMultipletCharge(index);
		}

		public double GetIsotopeClusterMassEstimate(int isoClusterIndex) {
			return peakList.GetIsotopeClusterMassEstimateNoRecal(isoClusterIndex, 0);
		}
		public double GetIsotopeClusterMassEstimate(IsotopeCluster ic) {
			return peakList.GetIsotopeClusterMassEstimateNoRecal(ic, 0);
		}

		public Dictionary<int, int> GetMultiplet2ChargePair(){
			return peakList.GetMultipletCluster2ChargePair();
		}

		public void PrecalcIntensities(){
			peakList.PrecalcIntensities();
		}

		public void CreateIsotopeClusters(IList<int[]> clusterInd, IList<byte> clusterCharges, byte maxCharge){
			peakList.CreateIsotopeClusters(clusterInd, clusterCharges, maxCharge);
		}
	}
}