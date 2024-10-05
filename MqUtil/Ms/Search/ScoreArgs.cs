namespace MqUtil.Ms.Search{
	public class ScoreArgs{
		public readonly double tolerance;
		public readonly bool isPpm;
		public readonly bool higherCharges;
		public readonly bool includeWater;
		public readonly bool includeAmmonia;
		public readonly bool dependentLosses;
		public readonly int ncombinations;
		public readonly int topx;
		public bool useIntensityPrediction;
		public bool useSequencebasedModifier;
		public CrossGroupSearchParam crossSearchGroupParam;

		public ScoreArgs(double tolerance, bool isPpm, bool higherCharges, bool includeWater, bool includeAmmonia,
			bool dependentLosses, int ncombinations, int topx, bool useIntensityPrediction, bool useSequencebasedModifier) {
			this.tolerance = tolerance;
			this.isPpm = isPpm;
			this.higherCharges = higherCharges;
			this.includeWater = includeWater;
			this.includeAmmonia = includeAmmonia;
			this.dependentLosses = dependentLosses;
			this.ncombinations = ncombinations;
			this.topx = topx;
			this.useIntensityPrediction = useIntensityPrediction;
			this.useSequencebasedModifier = useSequencebasedModifier;
		}


		public ScoreArgs(double tolerance, bool isPpm, bool higherCharges, bool includeWater, bool includeAmmonia,
			bool dependentLosses, int ncombinations, int topx, bool useIntensityPrediction, bool useSequencebasedModifier, CrossGroupSearchParam crossGroupSearchParam) {
			this.tolerance = tolerance;
			this.isPpm = isPpm;
			this.higherCharges = higherCharges;
			this.includeWater = includeWater;
			this.includeAmmonia = includeAmmonia;
			this.dependentLosses = dependentLosses;
			this.ncombinations = ncombinations;
			this.topx = topx;
			this.useIntensityPrediction = useIntensityPrediction;
			crossSearchGroupParam = crossGroupSearchParam;
            this.useSequencebasedModifier = useSequencebasedModifier;
        }
	}
}