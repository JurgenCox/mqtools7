namespace MqUtil.Data{
	public class Ms3Hit{
		public ReporterResult ReporterResult { get; set; }
		public int Ms3Ind { get; set; }
		public int ScanNumber { get; set; }
		public Ms3Hit(BinaryReader reader){
			bool hasReporter = reader.ReadBoolean();
			if (hasReporter){
				ReporterResult = new ReporterResult(reader);
			}
			Ms3Ind = reader.ReadInt32();
			ScanNumber = reader.ReadInt32();
		}
		public Ms3Hit(){}
		public void Write(BinaryWriter writer){
			bool hasReporter = ReporterResult != null;
			writer.Write(hasReporter);
			if (hasReporter){
				ReporterResult.Write(writer);
			}
			writer.Write(Ms3Ind);
			writer.Write(ScanNumber);
		}
	}
}