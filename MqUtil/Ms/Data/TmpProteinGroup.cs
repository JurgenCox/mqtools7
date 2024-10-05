using MqApi.Util;
namespace MqUtil.Ms.Data{
	public class TmpProteinGroup{
		private readonly bool[] razorPeptide;
		public string[] ProteinIds{ get; }
		public string[] PeptideSequences{ get; }
		public byte[] Mutated{ get; }
		public string[] MutationNames{ get; set; }
		public TmpProteinGroup(string[] proteinIds, string[] peptideSequences, byte[] isMutated){
			ProteinIds = proteinIds;
			PeptideSequences = peptideSequences;
			if (isMutated != null){
				Mutated = isMutated;
				MutationNames = new string[isMutated.Length];
				for (int i = 0; i < MutationNames.Length; i++){
					MutationNames[i] = "";
				}
			}
			razorPeptide = new bool[peptideSequences.Length];
		}
		public TmpProteinGroup(BinaryReader reader){
			ProteinIds = FileUtils.ReadStringArray(reader);
			PeptideSequences = FileUtils.ReadStringArray(reader);
			razorPeptide = FileUtils.ReadBooleanArray(reader);
			bool isNull = reader.ReadBoolean();
			if (!isNull){
				Mutated = FileUtils.ReadByteArray(reader);
				MutationNames = FileUtils.ReadStringArray(reader);
			}
		}
		public void Write(BinaryWriter writer){
			FileUtils.Write(ProteinIds, writer);
			FileUtils.Write(PeptideSequences, writer);
			FileUtils.Write(razorPeptide, writer);
			bool isNull = Mutated == null;
			writer.Write(isNull);
			if (!isNull){
				FileUtils.Write(Mutated, writer);
				FileUtils.Write(MutationNames, writer);
			}
		}
		public int CountRazors{
			get{
				int c = 0;
				foreach (bool t in razorPeptide){
					if (t){
						c++;
					}
				}
				return c;
			}
		}
		public void SetRazorPeptide(string seq){
			for (int i = 0; i < PeptideSequences.Length; i++){
				if (PeptideSequences[i] == seq){
					razorPeptide[i] = true;
					return;
				}
			}
			throw new Exception("Peptide not found.");
		}
		public double GetScore(Dictionary<string, double> pepSeq2Score){
			double result = 0;
			foreach (string t in PeptideSequences){
				result += pepSeq2Score[t];
			}
			return result;
		}
	}
}