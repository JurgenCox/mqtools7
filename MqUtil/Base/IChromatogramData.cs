using MqApi.Drawing;
using MqUtil.Ms.Enums;

namespace MqUtil.Base
{
	public interface IChromatogramData
	{
		bool IsInitialized();

		double[,] GetChromatogramValues(int blur, bool recalTimes, bool readCentroids, double minTime, double maxTime,
			int bins, int selectedFileIndex, bool positive, VisibleData visibleData, double imsIndMin, double imsIndMax,
			double resolution, double gridSpacing, int diaWindowIndex);

		double[][] GetTicOnGrid(int settingsSelectedFileIndex, bool settingsPositive, double minTime, double maxTime,
			int bins);

		List<Color2> GetMassTraceColors(VisibleData chromatogramSettingsVisibleData);

		bool[] GetMs1SelectionOnGrid(int settingsSelectedFileIndex, bool settingsPositive, double heatMapZoomYMin,
			double heatMapZoomYMax, int getLength, int settingsSelectedMs1Index);

		int GetMs1IndexFromRt(int settingsSelectedFileIndex, bool settingsPositive, float f);
	}
}

