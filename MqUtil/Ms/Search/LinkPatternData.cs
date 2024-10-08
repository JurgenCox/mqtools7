namespace MqUtil.Ms.Search {
	public class LinkPatternData {
		public string InterLinks { get; private set; }
        public string IntraLinks1 { get; private set; }
		public string IntraLinks2 { get; private set; }
        public string UnsaturatedLinks1 { get; private set; }
        public string UnsaturatedLinks2 { get; private set; }

		/// <summary>
		/// LinkPatternData contains information about the linked residues(Links) and these Links are separated by ";". 
		/// The first linked residue position is 1 because a protein N terminus part is counted as 0.
		/// If there is no Link for a given type, then "-" should be given.   
		/// </summary>
		/// <param name="interLinks"> linked reside pairs between two different peptides, i.e. 4:9;</param>
		/// <param name="intraLinks1"> linked residue pairs within only the peptide1, i.e. 3:9; </param>
		/// <param name="intraLinks2"> linked residue pairs within only the peptide2, i.e. 3:9; </param>
		/// <param name="unsaturatedLinks1"> unsaturated residue on the peptide1, i.e. 4;</param>
		/// <param name="unsaturatedLinks2"> unsaturated residue on the peptide2, i.e. 9; </param>
		public LinkPatternData(string interLinks, string intraLinks1, string intraLinks2, string unsaturatedLinks1, string unsaturatedLinks2) {
			InterLinks = interLinks;
			IntraLinks1 = intraLinks1;
			IntraLinks2 = intraLinks2;
			UnsaturatedLinks1 = unsaturatedLinks1;
			UnsaturatedLinks2 = unsaturatedLinks2;
		}
		
		public static LinkPatternData FromBinary(BinaryReader reader) {
			string interLinks = reader.ReadString();
			string intraLinks1 = reader.ReadString();
			string intraLinks2 = reader.ReadString();
			string unsaturatedLinks1 = reader.ReadString();
			string unsaturatedLinks2 = reader.ReadString();
			return new LinkPatternData(interLinks, intraLinks1, intraLinks2, unsaturatedLinks1, unsaturatedLinks2);
		}

		public void Update() {
			string changeToIntraLinks1 = IntraLinks1;
			string changeToIntraLinks2 = IntraLinks2;
			IntraLinks1 = changeToIntraLinks2;
			IntraLinks2 = changeToIntraLinks1;

			string changeToUnsaturatedLinks1 = UnsaturatedLinks1;
			string changeToUnsaturatedLinks2 = UnsaturatedLinks2;
			UnsaturatedLinks1 = changeToUnsaturatedLinks2;
			UnsaturatedLinks2 = changeToUnsaturatedLinks1;

			string updatedInterLinks = "";
			foreach (string link in InterLinks.Split(';')) {
				if (link.Length > 0 && !link.Equals("-")) {
					string updatedLink = link.Split(':')[1] + ':' + link.Split(':')[0];
					updatedInterLinks += updatedLink + ';';
				}
			}
			InterLinks = updatedInterLinks;
		}

		public static LinkPatternData FromString(string line) {
			string[] tokens = line.Split('\t');
			string interLinks = tokens[0];
			string intraLinks1 = tokens[1];
			string intraLinks2 = tokens[2];
			string unsaturatedLinks1 = tokens[3];
			string unsaturatedLinks2 = tokens[4];
			return new LinkPatternData(interLinks, intraLinks1, intraLinks2, unsaturatedLinks1, unsaturatedLinks2);
		}

		

		/// <summary>
		/// Returns a string containing a tab separated InterLinks, IntraLinks1, IntraLinks2, UnsaturatedLinks1 and UnsaturatedLinks2, respectively.
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			return InterLinks + "\t" + IntraLinks1 + "\t" + IntraLinks2 + "\t" + UnsaturatedLinks1 + "\t" + UnsaturatedLinks2;
		}

		public void Write(BinaryWriter writer) {
			writer.Write(InterLinks);
			writer.Write(IntraLinks1);
			writer.Write(IntraLinks2);
			writer.Write(UnsaturatedLinks1);
			writer.Write(UnsaturatedLinks2);
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((LinkPatternData) obj);
		}

		public override int GetHashCode() {
			unchecked {
				int hashCode = (InterLinks != null ? InterLinks.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (IntraLinks1 != null ? IntraLinks1.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (IntraLinks2 != null ? IntraLinks2.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (UnsaturatedLinks1 != null ? UnsaturatedLinks1.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (UnsaturatedLinks2 != null ? UnsaturatedLinks2.GetHashCode() : 0);
				return hashCode;
			}
		}

		private sealed class LinkPatternDataEqualityComparer : IEqualityComparer<LinkPatternData> {
			public bool Equals(LinkPatternData x, LinkPatternData y) {
				if (ReferenceEquals(x, y)) return true;
				if (ReferenceEquals(x, null)) return false;
				if (ReferenceEquals(y, null)) return false;
				if (x.GetType() != y.GetType()) return false;
				return string.Equals(x.InterLinks, y.InterLinks) && string.Equals(x.IntraLinks1, y.IntraLinks1) && string.Equals(x.IntraLinks2, y.IntraLinks2) && string.Equals(x.UnsaturatedLinks1, y.UnsaturatedLinks1) && string.Equals(x.UnsaturatedLinks2, y.UnsaturatedLinks2);
			}

			public int GetHashCode(LinkPatternData obj) {
				unchecked {
					int hashCode = (obj.InterLinks != null ? obj.InterLinks.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (obj.IntraLinks1 != null ? obj.IntraLinks1.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (obj.IntraLinks2 != null ? obj.IntraLinks2.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (obj.UnsaturatedLinks1 != null ? obj.UnsaturatedLinks1.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (obj.UnsaturatedLinks2 != null ? obj.UnsaturatedLinks2.GetHashCode() : 0);
					return hashCode;
				}
			}
		}

		public static IEqualityComparer<LinkPatternData> LinkPatternDataComparer { get; } = new LinkPatternDataEqualityComparer();

		protected bool Equals(LinkPatternData other) {
			return string.Equals(InterLinks, other.InterLinks) && string.Equals(IntraLinks1, other.IntraLinks1) && string.Equals(IntraLinks2, other.IntraLinks2) && string.Equals(UnsaturatedLinks1, other.UnsaturatedLinks1) && string.Equals(UnsaturatedLinks2, other.UnsaturatedLinks2);
		}

	}
}