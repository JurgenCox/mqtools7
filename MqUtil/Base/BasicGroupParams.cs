using System.Xml.Serialization;
using MqUtil.Ms.Enums;
using MqUtil.Ms.Instrument;
namespace MqUtil.Base
{
    [Serializable, XmlRoot(ElementName = "BasicGroupParams", IsNullable = false)]
	public abstract class BasicGroupParams {
		public abstract double CentroidMatchTol { get; }
		public abstract bool CentroidMatchTolInPpm { get; }
		public abstract double CentroidHalfWidth { get; }
		public abstract bool CentroidHalfWidthInPpm { get; }
		public abstract double ValleyFactor { get; }
		public abstract bool AdvancedPeakSplitting { get; }
		public abstract bool CutPeaks { get; }
		public abstract MsInstrument MsInstrument1 { get; }
		public abstract double MinTime { get; }
		public abstract double MaxTime { get; }
		public abstract bool UseMs1Centroids { get; }
		public abstract double IntensityThresholdMs1Dda { get; }
		public abstract double IntensityThresholdMs1Dia { get; }
		public abstract double IntensityThresholdMs2 { get; }
		public abstract IntensityDetermination IntensityDetermination1 { get; }
		public abstract int GapScans { get; }
		public abstract int MinPeakLen { get; }
		public abstract int DiaMinPeakLen { get; }
		public abstract int MaxCharge { get; }
		public abstract CentroidPosition CentroidPosition1 { get;}  
	}
}