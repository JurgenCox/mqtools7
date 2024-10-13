namespace MqUtil.Data{
    /// <summary>
    /// Given a Peak, create an object containing only ScanIndex and SmoothIntensities normalized to the rms value.
    /// </summary>
	public class SlimPeak1 : IDisposable{
		public int[] ScanIndices { get; private set; }
		public float[] SmoothIntensities { get; private set; }

        /// <summary>
        /// This constructor is only called by WritablePeak.WritePeak, where the only thing done with the instance is to call Write.
        /// </summary>
        /// <param name="peak"></param>
		public SlimPeak1(Peak peak){
			ScanIndices = peak.ScanIndices;
			SmoothIntensities = peak.SmoothIntensities;
            double xx = 0;  // rms value of SmoothIntensities
			foreach (double wx in SmoothIntensities){
				xx += wx*wx;
			}
			xx = Math.Sqrt(xx);
			for (int i = 0; i < SmoothIntensities.Length; i++){
				SmoothIntensities[i] = (float)(SmoothIntensities[i]/xx);
			}
		}

        /// <summary>
        /// This constructor is called only in Features.Deisotoping, but there it is called 3 times.
        /// </summary>
        /// <param name="reader"></param>
		public SlimPeak1(BinaryReader reader){
			int count = reader.ReadInt32();
			SmoothIntensities = new float[count];
			ScanIndices = new int[count];
			for (int i = 0; i < count; i++){
				SmoothIntensities[i] = reader.ReadSingle();
				ScanIndices[i] = reader.ReadInt32();
			}
		}

		public int LastIndex => ScanIndices[ScanIndices.Length - 1];
	    public int FirstIndex => ScanIndices[0];

	    public void Dispose(){
			ScanIndices = null;
			SmoothIntensities = null;
		}

        /// <summary>
        /// Write the ScanIndex and SmoothIntensities (which are the only fields anywhere).
        /// Called only by WritablePeak.WritePeak.
        /// </summary>
        /// <param name="writer"></param>
		public void Write(BinaryWriter writer){
			writer.Write(SmoothIntensities.Length);
			for (int i = 0; i < SmoothIntensities.Length; i++){
				writer.Write(SmoothIntensities[i]);
				writer.Write(ScanIndices[i]);
			}
		}
	}
}