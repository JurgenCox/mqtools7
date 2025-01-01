using System.Text;

namespace MqUtil.Mol{
	public class SmallMoleculeDbReference{
		public string ReferenceId{ get; set; }
		public string LocationInRef{ get; set; }
		public string ReferenceName{ get; set; }
		public SmallMoleculeDbReference(BinaryReader reader){
			ReferenceId = reader.ReadString();
			LocationInRef = reader.ReadString();
			ReferenceName = reader.ReadString();
		}
		public SmallMoleculeDbReference(){
		}
		public void Write(BinaryWriter writer){
			writer.Write(ReferenceId);
			writer.Write(LocationInRef);
			writer.Write(ReferenceName);
		}
		public override string ToString(){
			StringBuilder result = new StringBuilder();
			if (ReferenceId != null){
				result.Append(ReferenceId);
			}
			if (!string.IsNullOrEmpty(LocationInRef)){
				result.Append(",");
				result.Append(LocationInRef);
			}
			if (!string.IsNullOrEmpty(ReferenceName)){
				result.Append(",");
				result.Append(ReferenceName);
			}
			return result.ToString();
		}
	}
}