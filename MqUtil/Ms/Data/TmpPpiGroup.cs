using MqApi.Util;

namespace MqUtil.Ms.Data {
	public class TmpPpiGroup {
		private readonly bool[] razorPeptide;
		public Tuple<string, string>[] ProteinIds { get; }
		public Tuple<string, string>[] PeptideSequences { get; }
		public byte[] Mutated { get; }
		public string[] MutationNames { get; set; }
		public TmpPpiGroup(Tuple<string, string>[] proteinIds, Tuple<string, string>[] peptideSequences, byte[] isMutated) {
			ProteinIds = proteinIds;
			PeptideSequences = peptideSequences;
			if (isMutated != null) {
				Mutated = isMutated;
				MutationNames = new string[isMutated.Length];
				for (int i = 0; i < MutationNames.Length; i++) {
					MutationNames[i] = "";
				}
			}
			razorPeptide = new bool[peptideSequences.Length];
		}
		public TmpPpiGroup(BinaryReader reader) {
			int len = reader.ReadInt32();
			ProteinIds = new Tuple<string, string>[len];
			for (int i = 0; i < len; i++) {
				string s1 = reader.ReadString();
				string s2 = reader.ReadString();
				ProteinIds[i] = new Tuple<string, string>(s1, s2);
			}
			len = reader.ReadInt32();
			PeptideSequences = new Tuple<string, string>[len];
			for (int i = 0; i < len; i++) {
				string s1 = reader.ReadString();
				string s2 = reader.ReadString();
				PeptideSequences[i] = new Tuple<string, string>(s1, s2);
			}
			razorPeptide = FileUtils.ReadBooleanArray(reader);
			bool isNull = reader.ReadBoolean();
			if (!isNull) {
				Mutated = FileUtils.ReadByteArray(reader);
				MutationNames = FileUtils.ReadStringArray(reader);
			}
		}
		public void Write(BinaryWriter writer) {
			writer.Write(ProteinIds.Length);
			foreach (Tuple<string, string> x in ProteinIds) {
				writer.Write(x.Item1);
				writer.Write(x.Item2);
			}
			writer.Write(PeptideSequences.Length);
			foreach (Tuple<string, string> x in PeptideSequences) {
				writer.Write(x.Item1);
				writer.Write(x.Item2);
			}
			FileUtils.Write(razorPeptide, writer);
			bool isNull = Mutated == null;
			writer.Write(isNull);
			if (!isNull) {
				FileUtils.Write(Mutated, writer);
				FileUtils.Write(MutationNames, writer);
			}
		}
       public double GetScore(Dictionary<Tuple<string,string>, double> pepSeq2Score){
			double result = 0;
			foreach (Tuple<string, string> t in PeptideSequences){
				result += pepSeq2Score[t];
			}
			return result;
		}
    }
}
