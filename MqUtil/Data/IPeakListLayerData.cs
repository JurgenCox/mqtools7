using MqUtil.Num;
namespace MqUtil.Data{
	public interface IPeakListLayerData{
		double GetCalibratedRtFromRt(double d);
		bool[] HasMsms{ get; }
		int PeakCount{ get; }
		long[] FilePos{ get; }
		int[] IsotopeClusterIndices{ get; set; }
		int[][] IsoClusterIdsByCharge{ get; set; }
		int IsotopeClusterCount{ get; }
		double[] CenterMz{ get; set; }
		float[] Intensities{ get; set; }
		Multiplet[] Multiplets{ get; set; }
		float[] MinTimes{ get; set; }
		float[] MaxTimes{ get; set; }
		int[] MinRtIndsTmp{ get; set; }
		int[] MaxRtIndsTmp{ get; set; }
		int MultipletCount{ get; }
		Dictionary<int, LinearInterpolator> Medians10{ get; set; }
		Dictionary<int, LinearInterpolator> Medians20{ get; set; }
		Dictionary<int, LinearInterpolator> Medians21{ get; set; }
		Dictionary<int, IsotopeCluster[]> IsotopeClustersDia { get; set; }
		Dictionary<int, IsotopeClusterCounts> IsotopeClusterCountsDia { get; set; }
		Dictionary<int, int[][]> IsotopeClusterIdsByChargeDia { get; set; }
		Peak GetPeak(int i);
		double GetIsotopeClusterIntensity(int i, Func<int, GenericPeak> getPeak);
		IsotopeCluster GetGenericIsotopeCluster(int i);
		Multiplet GetMultiplet(int i);
		void WriteQuartiles();
		int[] GetPeaksInRectangle(double mzRangeMin, double mzRangeMax, double rtRangeMin, double rtRangeMax, bool recalTimes);
		int GetMultipletCharge(int index);
		double GetIsotopeClusterMassEstimate(int isoClusterIndex, int imsStep);
		double GetIsotopeClusterMassEstimateNoRecal(int isoClusterIndex, int imsStep);
		double GetIsotopeClusterMassEstimateNoRecal(IsotopeCluster ic, int imsStep);
		Dictionary<int, int> GetMultipletCluster2ChargePair();
		void PrecalcIntensities();
		void CreateIsotopeClusters(IList<int[]> clusterInd, IList<byte> clusterCharges, byte maxCharge);
		int GetPeakCountDia(int diaInd);
		void SetIsotopeClusterIndicesDia(int diaInd, int[] value);
		int[] GetIsotopeClusterIndicesDia(int diaInd);
		double GetIsotopeClusterMassEstimateDia(int isoClusterIndex, int diaInd);
		int[] GetPeaksInRectangleDia(double xMin, double xMax, double yMin, double yMax, bool recalTimes, int diaInd);
		int GetMultipletChargeDia(int i, int diaInd);
		double GetIsotopeClusterIntensityDia(int i, Func<int, GenericPeak> getPeak, int diaInd);
		IsotopeCluster GetIsotopeClusterDia(int i, int diaInd);
		Multiplet GetMultipletDia(int i, int diaInd);
		double GetMultipletClusterIntensityDia(int i, Func<int, GenericPeak> getPeak, int diaInd);
		void SetIsotopeClusterIdsByChargeDia(int diaInd, int[][] value);
		Peak GetPeakDia(int diaInd, int i);
		Peak[] GetPeaksDia(int diaInd);
		int GetIsotopeClusterCountDia(int diaInd);
		void SetCenterMzDia(int diaInd, double[] value);
		double[] GetCenterMzDia(int diaInd);
		float[] GetIntensitiesDia(int diaInd);
		void SetIntensitiesDia(int diaInd, float[] value);
		void SetMultipletsDia(int diaInd, Multiplet[] value);
		float[] GetMinTimesDia(int diaInd);
		void SetMinTimesDia(int diaInd, float[] value);
		float[] GetMaxTimesDia(int diaInd);
		void SetMaxTimesDia(int diaInd, float[] value);
		int[] GetMinRtIndsDia(int diaInd);
		void SetMinRtIndsDia(int diaInd, int[] value);
		int[] GetMaxRtIndsDia(int diaInd);
		void SetMaxRtIndsDia(int diaInd, int[] value);
		int GetMultipletCountDia(int diaInd);
	}
}