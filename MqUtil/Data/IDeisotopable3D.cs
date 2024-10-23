namespace MqUtil.Data{
	public interface IDeisotopable3D{
		float[] MinTimes { get; set; }
		float[] MaxTimes { get; set; }
		int[] MinRtIndsTmp { get; set; }
		int[] MaxRtIndsTmp { get; set; }
		double[] CenterMz { get; set; }
		float[] Intensities { get; set; }
		int Count { get; }
		int[] IsotopeClusterIndices { set; }
		int[][] GetIsoClusterIdsByCharge(int minCharge, int maxCharge);
		void SetIsoClusterIdsByCharge(int[][] ids);
		double GetIsotopeClusterIntensity(int i);
		int IsotopeClusterCount { get; }
		IsotopeCluster GetIsotopeCluster(int i);
		void PrecalcIntensities();
		void CreateIsotopeClusters(IList<int[]> clusterInd, IList<byte> clusterCharges, byte maxCharge);
	}
}