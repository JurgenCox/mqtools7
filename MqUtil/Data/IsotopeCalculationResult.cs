namespace MqUtil.Data {
	public class IsotopeCalculationResult {
		public double DeutMass { get; }
		public double DeutRatio { get; }
		public IsotopeCalculationResult(double deutMass, double deutRatio) {
			DeutMass = deutMass;
			DeutRatio = deutRatio;
		}
		public IsotopeCalculationResult(BinaryReader reader) {
			DeutMass = reader.ReadDouble();
			DeutRatio = reader.ReadDouble();
		}
		public void Write(BinaryWriter writer) {
			writer.Write(DeutMass);
			writer.Write(DeutRatio);
		}
	}
}