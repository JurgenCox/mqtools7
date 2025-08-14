using MqApi.Util;

namespace MqUtil.Ms.Data {
	public class TmpPpiGroup
    {
        private const byte Version = 1;
        private readonly bool[] razorPeptide;
		public Tuple<string, string>[] ProteinIds { get; }
		public Tuple<string, string>[] PeptideSequences { get; }
		public byte[] Mutated { get; }
		public string[] MutationNames { get; set; }
		public Dictionary<string, HashSet<Tuple<string, string>>> IntraLinks { get; set; }

        public TmpPpiGroup(Tuple<string, string>[] proteinIds, Tuple<string, string>[] peptideSequences, byte[] isMutated,
			Dictionary<string, HashSet<Tuple<string, string>>> intraLinks) {
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
			IntraLinks = intraLinks;

		}
		public TmpPpiGroup(BinaryReader reader) {
            byte version = reader.ReadByte();
            if (version != Version){
                throw new Exception("Wrong version of TmpPpiGroup. " +
                                    "Expected " + Version + " but got " + version + ".");
            }
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
			bool intraNull = reader.ReadBoolean();
            if (intraNull){
                IntraLinks = new Dictionary<string, HashSet<Tuple<string, string>>>();
            } else{
                Dictionary<string, HashSet<Tuple<string, string>>> intraLinks =
                    new Dictionary<string, HashSet<Tuple<string, string>>>();
                int proteinCount = reader.ReadInt32();
                for (int i = 0; i < proteinCount; i++)
                {
                    string protein = reader.ReadString();
                    int intraLinkCount = reader.ReadInt32();
                    var set = new HashSet<Tuple<string, string>>();
                    for (int j = 0; j < intraLinkCount; j++)
                    {
                        string p1 = reader.ReadString();
                        string p2 = reader.ReadString();
                        set.Add(new Tuple<string, string>(p1, p2));
                    }
                    intraLinks[protein] = set;
                }
                IntraLinks = intraLinks;
            }
				
        }
		public void Write(BinaryWriter writer)
        {
            writer.Write(Version);
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
			bool intraNull = IntraLinks == null || IntraLinks.Count == 0;
            writer.Write(intraNull);
            if (intraNull){
                return;
            }
            writer.Write(IntraLinks.Count);
            foreach (var kvp in IntraLinks)
            {
                writer.Write(kvp.Key); // protein
                writer.Write(kvp.Value.Count);
                foreach (var pair in kvp.Value)
                {
                    writer.Write(pair.Item1);
                    writer.Write(pair.Item2);
                }
            }


        }
        public int CountRazors
        {
            get
            {
                int c = 0;
                foreach (bool t in razorPeptide)
                {
                    if (t)
                    {
                        c++;
                    }
                }
                return c;
            }
        }
        public void SetRazorPeptide(Tuple<string, string> seq)
        {
            for (int i = 0; i < PeptideSequences.Length; i++)
            {
                if (PeptideSequences[i].Equals(seq))
                {
                    razorPeptide[i] = true;
                    return;
                }
            }
            throw new Exception("Peptide not found.");
        }
        public double GetScore(Dictionary<Tuple<string,string>, double> pepSeq2Score){
			double result = 0;
            foreach (Tuple<string, string> t in PeptideSequences)
            {
                if (pepSeq2Score.TryGetValue(t, out double s)) result += s;
            }
            return result;
        }
    }
}
