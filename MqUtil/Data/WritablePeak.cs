using MqUtil.Ms.Raw;
namespace MqUtil.Data{
	public class WritablePeak{
		public readonly Peak p;
		private readonly double minTime;
		private readonly double maxTime;
		public readonly int minRtInd;
		public readonly int maxRtInd;
		private readonly double centerTime;
		private readonly double minMz;
		private readonly double maxMz;

		public WritablePeak(Peak peak, RawLayer rawFile){
			p = peak;
			minRtInd = p.FirstScanIndex;
			maxRtInd = p.LastScanIndex;
			minTime = rawFile.GetTimeSpan(Math.Max(0, p.FirstScanIndex - 1))[0];
			maxTime = p.GetMaxTime(rawFile);
			centerTime = p.GetCenterTime(rawFile);
			minMz = p.MzMinMin;
			maxMz = p.MzMaxMax;
		}

		/// <summary>
		/// Writes a variety of information on this WritablePeak to each of the 4 writers given as arguments.
		/// </summary>
		public void WritePeak(BinaryWriter writer, BinaryWriter indexWriter, BinaryWriter tmpWriter1, BinaryWriter tmpWriter2,
			bool hasMassBounds){
			indexWriter.Write(p.MzCentroidAvg);
			indexWriter.Write(writer.BaseStream.Position);
			indexWriter.Write(tmpWriter1 != null ? (int) tmpWriter1.BaseStream.Position : 0);
			indexWriter.Write(p.Intensity);
			indexWriter.Write(minTime);
			indexWriter.Write(maxTime);
			indexWriter.Write(minRtInd);
			indexWriter.Write(maxRtInd);
			indexWriter.Write(centerTime);
			if (hasMassBounds){
				indexWriter.Write(minMz);
				indexWriter.Write(maxMz);
			}
			p.Write(writer);
			if (tmpWriter1 != null){
				new SlimPeak1(p).Write(tmpWriter1);
				new SlimPeak2(p).Write(tmpWriter2);
			}
			p.Dispose();
		}
	}
}