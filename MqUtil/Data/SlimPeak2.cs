namespace MqUtil.Data{
	public class SlimPeak2 : IDisposable{
		private double[] centerMz;
		private float[] origIntensityProfile;

		public SlimPeak2(Peak peak){
			centerMz = peak.MzCentroid;
			origIntensityProfile = peak.OrigIntensityProfile;
		}

		public SlimPeak2(BinaryReader reader){
			int count = reader.ReadInt32();
			centerMz = new double[count];
			origIntensityProfile = new float[count];
			for (int i = 0; i < count; i++){
				centerMz[i] = reader.ReadDouble();
				origIntensityProfile[i] = reader.ReadSingle();
			}
		}

		public int Count => centerMz.Length;

		public void Dispose(){
			centerMz = null;
			origIntensityProfile = null;
		}

		public double GetMz(int i){
			return centerMz[i];
		}

		public double GetIntensity(int i){
			return origIntensityProfile[i];
		}

		public void Write(BinaryWriter writer){
			writer.Write(centerMz.Length);
			for (int i = 0; i < centerMz.Length; i++){
				writer.Write(centerMz[i]);
				writer.Write(origIntensityProfile[i]);
			}
		}
	}
}