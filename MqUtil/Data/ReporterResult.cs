using MqApi.Util;

namespace MqUtil.Data {
	public class ReporterResult {
		public double[] ReporterIntensities { get; set; }
		public double[] ReporterIntensitiesNoCorrection { get; set; }
		public double[] ReporterMassDevs { get; set; }
		public double ReporterPif { get; set; }
		public double ReporterFraction { get; set; }
		public double BasePeakFraction { get; set; }
		public double PrecursorIntensity { get; set; }
		public double InjectionTime { get; set; }
		public int IsotopeIndex { get; set; }

		public ReporterResult(BinaryReader reader) {
			ReporterPif = reader.ReadDouble();
			ReporterFraction = reader.ReadDouble();
			BasePeakFraction = reader.ReadDouble();
			PrecursorIntensity = reader.ReadDouble();
			InjectionTime = reader.ReadDouble();
			IsotopeIndex = reader.ReadInt32();
			bool hasIntens = reader.ReadBoolean();
			if (hasIntens) {
				ReporterIntensities = FileUtils.ReadDoubleArray(reader);
				ReporterIntensitiesNoCorrection = FileUtils.ReadDoubleArray(reader);
			}
			bool hasMassDevs = reader.ReadBoolean();
			if (hasMassDevs) {
				ReporterMassDevs = FileUtils.ReadDoubleArray(reader);
			}
		}

		public ReporterResult() { }

		public void Write(BinaryWriter writer) {
			writer.Write(ReporterPif);
			writer.Write(ReporterFraction);
			writer.Write(BasePeakFraction);
			writer.Write(PrecursorIntensity);
			writer.Write(InjectionTime);
			writer.Write(IsotopeIndex);
			bool hasIntens = ReporterPif != -1 && ReporterIntensities != null;
			writer.Write(hasIntens);
			if (hasIntens) {
				FileUtils.Write(ReporterIntensities, writer);
				FileUtils.Write(ReporterIntensitiesNoCorrection, writer);
			}
			bool hasMassDevs = ReporterMassDevs != null;
			writer.Write(hasMassDevs);
			if (hasMassDevs) {
				FileUtils.Write(ReporterMassDevs, writer);
			}
		}
	}
}