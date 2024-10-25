using MqApi.Util;

namespace MqUtil.Data {
	public class IsotopeClusterTims : IsotopeCluster {
		public double RetentionTimeFwhm { get; set; }
		public double IonMobilityInd { get; set; }
		public double IonMobilityIndMin { get; set; }
		public double IonMobilityIndMax { get; set; }
		public double IonMobilityIndFwhmMin { get; set; }
		public double IonMobilityIndFwhmMax { get; set; }
		public double IonMobilityIndDip { get; set; }
		public double CcsMean { get; set; }
		public double CcsMin { get; set; }
		public double CcsMax { get; set; }
		public double K0InvMean { get; set; }
		public double K0InvMin { get; set; }
		public double K0InvMax { get; set; }
		public double Mass { get; set; }
		public int Npoints { get; set; }
		public int Nframes { get; set; }
		public int Niso { get; set; }
		public int MsmsScanNumber { get; set; }
		public int[][] MsmsFrameInds { get; set; }
		public double[] ProfileIntensities { get; set; }
		public double[] ProfileK0Inv { get; set; }
		public int[] ProfileIndices { get; set; }

		public IsotopeClusterTims(int[] members, int charge) : base(members, charge) {
			MsmsFrameInds = new int[0][];
		}

		public IsotopeClusterTims(BinaryReader reader) : base(reader) {
			Mass = reader.ReadDouble();
			RetentionTimeFwhm = reader.ReadDouble();
			IonMobilityInd = reader.ReadDouble();
			IonMobilityIndMin = reader.ReadDouble();
			IonMobilityIndMax = reader.ReadDouble();
			IonMobilityIndFwhmMin = reader.ReadDouble();
			IonMobilityIndFwhmMax = reader.ReadDouble();
			IonMobilityIndDip = reader.ReadDouble();
			CcsMean = reader.ReadDouble();
			CcsMin = reader.ReadDouble();
			CcsMax = reader.ReadDouble();
			Npoints = reader.ReadInt32();
			Nframes = reader.ReadInt32();
			Niso = reader.ReadInt32();
			MsmsScanNumber = reader.ReadInt32();
			MsmsFrameInds = FileUtils.Read2DInt32Array(reader);
			K0InvMean = reader.ReadDouble();
			K0InvMin = reader.ReadDouble();
			K0InvMax = reader.ReadDouble();
			bool profileExists = reader.ReadBoolean();
			if (profileExists) {
				ProfileK0Inv = FileUtils.ReadDoubleArray(reader);
				ProfileIntensities = FileUtils.ReadDoubleArray(reader);
				ProfileIndices = FileUtils.ReadInt32Array(reader);
			}
		}

		public double IonMobilityIndFwhm => IonMobilityIndFwhmMax - IonMobilityIndFwhmMin;
		public double IonMobilityIndLength => IonMobilityIndMax - IonMobilityIndMin;

		public override void Write(BinaryWriter writer) {
			base.Write(writer);
			writer.Write(Mass);
			writer.Write(RetentionTimeFwhm);
			writer.Write(IonMobilityInd);
			writer.Write(IonMobilityIndMin);
			writer.Write(IonMobilityIndMax);
			writer.Write(IonMobilityIndFwhmMin);
			writer.Write(IonMobilityIndFwhmMax);
			writer.Write(IonMobilityIndDip);
			writer.Write(CcsMean);
			writer.Write(CcsMin);
			writer.Write(CcsMax);
			writer.Write(Npoints);
			writer.Write(Nframes);
			writer.Write(Niso);
			writer.Write(MsmsScanNumber);
			FileUtils.Write(MsmsFrameInds, writer);
			writer.Write(K0InvMean);
			writer.Write(K0InvMin);
			writer.Write(K0InvMax);
			bool profileExists = ProfileIntensities != null;
			writer.Write(profileExists);
			if (profileExists) {
				FileUtils.Write(ProfileK0Inv, writer);
				FileUtils.Write(ProfileIntensities, writer);
				FileUtils.Write(ProfileIndices, writer);
			}
		}
	}
}