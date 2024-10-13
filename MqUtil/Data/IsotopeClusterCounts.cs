using MqApi.Util;
namespace MqUtil.Data{
	public class IsotopeClusterCounts{
		public int totalMemberCount;
		public int totalIsoClusterCount;
		public int[] chargeCounts;

		public IsotopeClusterCounts(int maxCharge){
			chargeCounts = new int[maxCharge + 1];
		}

		public IsotopeClusterCounts(BinaryReader reader){
			totalMemberCount = reader.ReadInt32();
			totalIsoClusterCount = reader.ReadInt32();
			chargeCounts = FileUtils.ReadInt32Array(reader);
		}

		public void Write(BinaryWriter writer){
			writer.Write(totalMemberCount);
			writer.Write(totalIsoClusterCount);
			FileUtils.Write(chargeCounts, writer);
		}
	}
}