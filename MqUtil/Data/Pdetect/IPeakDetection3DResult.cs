using MqUtil.Num;

namespace MqUtil.Data.Pdetect{
	public interface IPeakDetection3DResult : IDeisotopable3D{
		double GetCalibratedRtFromRt(double t);
		int GetIsotopeClusterIndex(int i);
		int GetCustomPeakColorIndex(int i);
		Peak GetPeak(int i);
		Multiplet[] Multiplets { set; }
		int MultipletCount { get; }
		Dictionary<int, LinearInterpolator> Medians10 { get; set; }
		Dictionary<int, LinearInterpolator> Medians20 { get; set; }
		Dictionary<int, LinearInterpolator> Medians21 { get; set; }
		Multiplet GetMultiplet(int i);
		void WriteQuartiles();
		int[] GetPeaksInRectangle(double xMin, double xMax, double yMin, double yMax, bool recalTimes);
		int GetMultipletCharge(int index);
		double GetIsotopeClusterMassEstimate(int isoClusterIndex);
		Dictionary<int, int> GetMultiplet2ChargePair();
	}
}