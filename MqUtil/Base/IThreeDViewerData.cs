using MqUtil.Ms.Enums;

namespace MqUtil.Base{
	public interface IThreeDViewerData{
		int GetPeakLineWidth(VisibleData mapModelThreeDVisibleData);
		bool IsInitialized();

		double[,] GetValues(int width, int height, double zoomXMin, double zoomXMax, double zoomYMin, double zoomYMax,
			int blur, bool recalMasses, bool recalTimes, bool readCentroids, int calcInd, int selectedFileIndex,
			bool positive, int iresolution, VisibleData visibleData, double imsIndMin, double imsIndMax,
			double resolution, double gridSpacing, int diaIndex, bool isDia, int faimsIndex, bool isFaims);
	}
}