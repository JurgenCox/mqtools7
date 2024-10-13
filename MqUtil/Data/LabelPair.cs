namespace MqUtil.Data{
	public class LabelPair{
		public LabelPair(int isoCluster1, int isoCluster2, int[] labelCounts, double isotopeCorrelation,
			double timeCorrelation, double ratio, int scanIndexOffset){
			Index1 = isoCluster1;
			Index2 = isoCluster2;
            IsotopeCorrelation = isotopeCorrelation;
            LabelCounts = labelCounts;
            ScanIndexOffset = scanIndexOffset;
            Ratio = ratio;
            TimeCorrelation = timeCorrelation;
		}

		public int Index1 { get; }
		public int Index2 { get; }
		public double IsotopeCorrelation { get; }
		public int[] LabelCounts { get; }
		public int ScanIndexOffset { get; }
		public double Ratio { get; }
		public double TimeCorrelation { get; }

		public bool EqualLabeledAaCounts(LabelPair other){
			for (int i = 0; i < LabelCounts.Length; i++){
				if (LabelCounts[i] != other.LabelCounts[i]){
					return false;
				}
			}
			return true;
		}
	}
}