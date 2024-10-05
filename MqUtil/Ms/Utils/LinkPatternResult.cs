using MqApi.Num;
using MqUtil.Mol;
using MqUtil.Ms.Enums;
namespace MqUtil.Ms.Utils {
	public class LinkPatternResult {
		private readonly Tuple<int, int>[] interLinks;
		private readonly Tuple<int, int>[] intraLinks1;
		private readonly Tuple<int, int>[] intraLinks2;
		private readonly int[] unsatLinks1;
		private readonly int[] unsatLinks2;
		private readonly int[] interLinksMs3;
		public int[] ntermFragInds1 { get; private set; }
		public int[] ntermFragInds2 { get; private set; }
        public int[] ctermFragInds1 { get; private set; }
        public int[] ctermFragInds2 { get; private set; }
        public double[] ntermFragMass1 { get; private set; }
		public double[] ntermFragMass2 { get; private set; }
		public double[] ctermFragMass1 { get; private set; }
		public double[] ctermFragMass2 { get; private set; }
		public bool[] ntermFragP1 { get; private set; }
		public bool[] ntermFragP2 { get; private set; }
		public bool[] ctermFragP1 { get; private set; }
		public bool[] ctermFragP2 { get; private set; }
		public MsCleavableSymmetry mscleasymm { get; private set; }

		public Tuple<int, int>[] InterLinks => interLinks;
		public Tuple<int, int>[] IntraLinks1 => intraLinks1;
		public Tuple<int, int>[] IntraLinks2 => intraLinks2;
		public int[] UnsatLinks1 => unsatLinks1;
		public int[] UnsatLinks2 => unsatLinks2;
		public int[] InterLinksMs3 => interLinksMs3;

		public LinkSiteInfo[] NTermFragInfo1;
		public LinkSiteInfo[] CTermFragInfo1;
		public LinkSiteInfo[] NTermFragInfo2;
		public LinkSiteInfo[] CTermFragInfo2;

		public LinkPatternResult(Tuple<int, int>[] interLinks, Tuple<int, int>[] intraLinks1, Tuple<int, int>[] intraLinks2,
			int[] unsatLinks1, int[] unsatLinks2, MsCleavableSymmetry mscleasymm) {
			this.interLinks = interLinks;
			this.intraLinks1 = intraLinks1;
			this.intraLinks2 = intraLinks2;
			this.unsatLinks1 = unsatLinks1;
			this.unsatLinks2 = unsatLinks2;
			this.mscleasymm = mscleasymm;
		}

		public LinkPatternResult(Tuple<int, int>[] interLinks, Tuple<int, int>[] intraLinks1, Tuple<int, int>[] intraLinks2,
			int[] unsatLinks1, int[] unsatLinks2) {
			this.interLinks = interLinks;
			this.intraLinks1 = intraLinks1;
			this.intraLinks2 = intraLinks2;
			this.unsatLinks1 = unsatLinks1;
			this.unsatLinks2 = unsatLinks2;
		}

		public LinkPatternResult(int[] interLinksMs3, Tuple<int, int>[] intraLinks1, int[] unsatLinks1, MsCleavableSymmetry mscleasymm) {
			this.interLinksMs3 = interLinksMs3;
			this.intraLinks1 = intraLinks1;
			this.unsatLinks1 = unsatLinks1;
			this.mscleasymm = mscleasymm;
		}

		public LinkPatternResult TransformBack(int[] inds1, int[] inds2, MsCleavableSymmetry mscls) {
			Tuple<int, int>[] transformedInterLinks = Transform(interLinks, inds1, inds2);
			Tuple<int, int>[] transformedIntraLinks1 = Transform(intraLinks1, inds1, inds1);
			Tuple<int, int>[] transformedIntraLinks2 = Transform(intraLinks2, inds2, inds2);
			int[] transformedUnsatLinks1 = Transform(unsatLinks1, inds1);
			int[] transformedUnsatLinks2 = Transform(unsatLinks2, inds2);
			LinkPatternResult lpr = new LinkPatternResult(transformedInterLinks, transformedIntraLinks1, transformedIntraLinks2,
				transformedUnsatLinks1, transformedUnsatLinks2, mscls);
			return lpr;
		}

		public LinkPatternResult TransformBackMs3Cleavable(int[] inds1, MsCleavableSymmetry mscls) {
			int[]  transformedInterLinks = Transform(interLinksMs3, inds1);
			Tuple<int, int>[] transformedIntraLinks1 = Transform(intraLinks1, inds1, inds1);
			int[] transformedUnsatLinks1 = Transform(unsatLinks1, inds1);
			LinkPatternResult lpr = new LinkPatternResult(transformedInterLinks, transformedIntraLinks1, transformedUnsatLinks1,  mscls);
			return lpr;
		}

		public void CalcFragmentation(int seqLen1, int seqLen2, CrossLinker crossLinker, CrossLinkingType crossLinkingType) {
			if (crossLinker.IsMsCleavableCrosslinker && crossLinkingType == CrossLinkingType.CleavableMs3) {
				CalcFragmentationMs3Cleavable(seqLen1, seqLen2, crossLinker);
			} else {
				CalcFragmentation(seqLen1, seqLen2, crossLinker);
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="seqLen1"></param>
		/// <param name="seqLen2"></param>
		/// <param name="crossLinker"></param>
		public void CalcFragmentation(int seqLen1, int seqLen2, CrossLinker crossLinker) {
			bool[] ntermFrag1 = ArrayUtils.FillArray(true, seqLen1 - 1);
			bool[] ctermFrag1 = ArrayUtils.FillArray(true, seqLen1 - 1);
			ntermFragMass1 = new double[seqLen1 - 1];
			ctermFragMass1 = new double[seqLen1 - 1];
			ntermFragP1 = new bool[seqLen1 - 1];
			ctermFragP1 = new bool[seqLen1 - 1];

			NTermFragInfo1 = new LinkSiteInfo[seqLen1 - 1];
			CTermFragInfo1 = new LinkSiteInfo[seqLen1 - 1];

			for (int i = 0; i < seqLen1 - 1; i++) {
				NTermFragInfo1[i] = new LinkSiteInfo();
				CTermFragInfo1[i] = new LinkSiteInfo();
			}

			AddIntraLinks(ntermFrag1, ctermFrag1, ntermFragMass1, ctermFragMass1,
				NTermFragInfo1, CTermFragInfo1, intraLinks1, crossLinker);
			AddUnsatLinks(ntermFragMass1, ctermFragMass1, unsatLinks1, crossLinker.HydrolyzedMass, 
				NTermFragInfo1, CTermFragInfo1);

			if (seqLen2 > 0 && interLinks.Length > 0) {
				bool[] ntermFrag2 = ArrayUtils.FillArray(true, seqLen2 - 1);
				bool[] ctermFrag2 = ArrayUtils.FillArray(true, seqLen2 - 1);
				ntermFragMass2 = new double[seqLen2 - 1];
				ctermFragMass2 = new double[seqLen2 - 1];
				ntermFragP2 = new bool[seqLen2 - 1];
				ctermFragP2 = new bool[seqLen2 - 1];
				NTermFragInfo2 = new LinkSiteInfo[seqLen2 - 1];
				CTermFragInfo2 = new LinkSiteInfo[seqLen2 - 1];

				for (int j = 0; j < seqLen2-1; j++) {
					NTermFragInfo2 [j] = new LinkSiteInfo();
					CTermFragInfo2 [j] = new LinkSiteInfo();
				}

				AddIntraLinks(ntermFrag2, ctermFrag2, ntermFragMass2, ctermFragMass2, 
					NTermFragInfo2, CTermFragInfo2, intraLinks2, crossLinker);
				AddUnsatLinks(ntermFragMass2, ctermFragMass2, unsatLinks2, crossLinker.HydrolyzedMass, NTermFragInfo2, CTermFragInfo2);
				AddInterLinks(ntermFrag1, ctermFrag1, ntermFrag2, ctermFrag2, 
					ntermFragMass1, ctermFragMass1, ntermFragMass2, ctermFragMass2,
					ntermFragP1, ctermFragP1, ntermFragP2, ctermFragP2,
					NTermFragInfo1, CTermFragInfo1, NTermFragInfo2, CTermFragInfo2,
					crossLinker, interLinks);

				ntermFragInds2 = ArrayUtils.IndicesOf(ntermFrag2, true);
				ctermFragInds2 = ArrayUtils.IndicesOf(ctermFrag2, true);
				//ntermFragMass2 = ntermFragMass2.SubArray(ntermFragInds2);
				//ctermFragMass2 = ctermFragMass2.SubArray(ctermFragInds2);
				//ntermFragP2 = ntermFragP2.SubArray(ntermFragInds2);
				//ctermFragP2 = ctermFragP2.SubArray(ctermFragInds2);
			}
			ntermFragInds1 = ArrayUtils.IndicesOf(ntermFrag1, true);
			ctermFragInds1 = ArrayUtils.IndicesOf(ctermFrag1, true);
			//ntermFragMass1 = ntermFragMass1.SubArray(ntermFragInds1);
			//ctermFragMass1 = ctermFragMass1.SubArray(ctermFragInds1);
			//ntermFragP1 = ntermFragP1.SubArray(ntermFragInds1);
			//ctermFragP1 = ctermFragP1.SubArray(ctermFragInds1);
		}

		/// <summary>
		///  TODO: check how ntermFragP fills out
		/// </summary>
		/// <param name="seqLen1"></param>
		/// <param name="seqLen2"></param>
		/// <param name="crossLinker"></param>
		public void CalcFragmentationMs3Cleavable(int seqLen1, int seqLen2, CrossLinker crossLinker) {
			bool[] ntermFrag1 = ArrayUtils.FillArray(true, seqLen1 - 1);
			bool[] ctermFrag1 = ArrayUtils.FillArray(true, seqLen1 - 1);
			ntermFragMass1 = new double[seqLen1 - 1];
			ctermFragMass1 = new double[seqLen1 - 1];
			ntermFragP1 = new bool[seqLen1 - 1];
			ctermFragP1 = new bool[seqLen1 - 1];

			for (int i = 0; i < seqLen1 - 1; i++) {
				NTermFragInfo1[i] = new LinkSiteInfo();
				CTermFragInfo1[i] = new LinkSiteInfo();
			}

			double cleavableMass = crossLinker.CrossFragmentLongMass;
			if (mscleasymm.Equals(MsCleavableSymmetry.ShorterArmCrosslinker)) {
				cleavableMass = crossLinker.CrossFragmentShortMass;
			} else if (mscleasymm.Equals(MsCleavableSymmetry.None)) {
				throw new Exception("Contact to a developer, to improve your MS-cleavable crosslinker.");
			}
			
			AddUnsatLinks(ntermFragMass1, ctermFragMass1, interLinksMs3, cleavableMass, NTermFragInfo1, CTermFragInfo1); // Adding linked residue part like unsat but different masses
			AddUnsatLinks(ntermFragMass1, ctermFragMass1, unsatLinks1, crossLinker.HydrolyzedMass, NTermFragInfo1, CTermFragInfo1);
			AddIntraLinks(ntermFrag1, ctermFrag1, ntermFragMass1, ctermFragMass1, 
				NTermFragInfo1,CTermFragInfo1, intraLinks1, crossLinker);

			ntermFragInds1 = ArrayUtils.IndicesOf(ntermFrag1, true);
			ctermFragInds1 = ArrayUtils.IndicesOf(ctermFrag1, true);
			ntermFragMass1 = ntermFragMass1.SubArray(ntermFragInds1);
			ctermFragMass1 = ctermFragMass1.SubArray(ctermFragInds1);
			ntermFragP1 = ntermFragP1.SubArray(ntermFragInds1);
			ctermFragP1 = ctermFragP1.SubArray(ctermFragInds1);
		}


		private static void AddUnsatLinks(IList<double> ntermFragMass, IList<double> ctermFragMass,
			IEnumerable<int> unsatLinks, double dm, 
			IList<LinkSiteInfo> ntermInfo, IList<LinkSiteInfo> ctermInfo) {
			foreach (int unsatLink in unsatLinks){
				int min = unsatLink - 1;
				min = Math.Max(0, min);
				min = Math.Min(ntermFragMass.Count, min);
				for (int i = min; i < ntermFragMass.Count; i++){
					ntermFragMass[i] += dm;
					ntermInfo[i].NumUnsat ++;
				}
				for (int i = 0; i < min; i++){
					ctermFragMass[ctermFragMass.Count - 1 - i] += dm;
					ctermInfo[ctermFragMass.Count - 1 - i].NumUnsat ++;
				}
			}
		}

		public static void ProcessX(IList<bool> ntermFrag, IList<bool> ctermFrag, 
			IList<double> ntermFragMass, IList<double> ctermFragMass, 
			IList<bool> ntermFragP, IList<bool> ctermFragP,
			IList<LinkSiteInfo> ntermInfo, IList<LinkSiteInfo> ctermInfo,
			IList<int> links, CrossLinker crossLinker) {
			ArrayUtils.MinMax(links, out int min, out int max);
			double dm = 0;
			sbyte interlinkNum = 0;
			for (int i = min; i < ntermFragMass.Count; i++) {
				if (links.Contains(i)) {
					dm += crossLinker.LinkedMass;
					interlinkNum++;
				}
				ntermFragMass[i] += dm;
				if (ntermFragP != null) {
					ntermFragP[i] = true;
					ntermInfo[i].NumInter = interlinkNum;
				}
			}
			dm = 0;
			interlinkNum = 0;
            max = Math.Min(ctermFragMass.Count, max);
			for (int i = max; i >0; i--) {
				if (links.Contains(i)) {
					dm += crossLinker.LinkedMass;
					interlinkNum++;
				}
				ctermFragMass[ctermFragMass.Count - i] += dm ;
				if (ctermFragP != null){
					ctermFragP[ctermFragP.Count - i] = true;
					ctermInfo[ctermFragP.Count - i].NumInter = interlinkNum;
				}
			}
		}

		private static void ProcessIntra(IList<double> ntermFragMass, IList<double> ctermFragMass,
			 IList<LinkSiteInfo> ntermInfo, IList<LinkSiteInfo> ctermInfo, int min, int max, CrossLinker crosslinker) {
			double dm = crosslinker.LinkedMass;
			min = Math.Max(0, min);
			max = Math.Min(ntermFragMass.Count, max);

			// The fragment ions between the link1 and link2 of one intra-peptide cannot be discriminated, so ignored (ie by MeroX)
			for (int i = 0; i < min; i++) {
				// n-terms do not have crosslinkers.
				// c-terms have crosslinker s
				ctermFragMass[ctermFragMass.Count - 1 - i] += dm;
				ctermInfo[ctermFragMass.Count - 1 - i].NumIntra++;
			}

			// between min and max, no fragments are generated containing a complete crosslinker
			// but information filled in order to generate fragment ions for MS2 cleavable crosslinkers 
			for (int i = min; i < max; i++) {
				int ctermInd = ctermFragMass.Count - 1 - i;
				ctermInfo[ctermInd].NumIntra++;
				ntermInfo[i].NumIntra++;
				if (!crosslinker.IsMsCleavableCrosslinker) {
					ctermFragMass[ctermInd] = -1;
					ntermFragMass[i] = -1;
				}
				
			}

			// continue  
			for (int i = max; i < ctermFragMass.Count; i++) {
				ntermFragMass[i] += dm;
				ntermInfo[i].NumIntra++;
			}
		}

		public static void AddInterLinks(IList<bool> ntermFrag1, IList<bool> ctermFrag1, IList<bool> ntermFrag2,
			IList<bool> ctermFrag2, IList<double> ntermFrag1Mass, IList<double> ctermFrag1Mass, IList<double> ntermFrag2Mass,
			IList<double> ctermFrag2Mass, bool[] ntermFragP1, bool[] ctermFragP1, bool[] ntermFragP2, bool[] ctermFragP2,
			LinkSiteInfo[] ntermInfo1, LinkSiteInfo[] ctermInfo1, LinkSiteInfo[] ntermInfo2, LinkSiteInfo[] ctermInfo2,
			CrossLinker crossLinker, Tuple<int, int>[] interLinks){
			if (interLinks.Length < 1){
		        return;
		    }
			int[] links1 = new int[interLinks.Length];
			int[] links2 = new int[interLinks.Length];
			for (int i = 0; i < interLinks.Length; i++){
				links1[i] = interLinks[i].Item1 == 0 ? interLinks[i].Item1: interLinks[i].Item1 - 1;
				links2[i] = interLinks[i].Item2 == 0 ? interLinks[i].Item2 : interLinks[i].Item2 - 1;
			}
            ProcessX(ntermFrag1, ctermFrag1, ntermFrag1Mass, ctermFrag1Mass, ntermFragP1, ctermFragP1,
	            ntermInfo1, ctermInfo1, links1.ToList(), crossLinker);
            ProcessX(ntermFrag2, ctermFrag2, ntermFrag2Mass, ctermFrag2Mass, ntermFragP2, ctermFragP2,
	            ntermInfo2, ctermInfo2, links2.ToList(), crossLinker);
		}


		private static void AddIntraLinks(IList<bool> ntermFrag, IList<bool> ctermFrag, IList<double> ntermFragMass, IList<double> ctermFragMass,
			IList<LinkSiteInfo> ntermInfo, IList<LinkSiteInfo> ctermInfo, IEnumerable<Tuple<int, int>> intraLinks, CrossLinker crosslinker){
			foreach (Tuple<int, int> intraLink in intraLinks){
				int link1 = intraLink.Item1 == 0 ? intraLink.Item1 : intraLink.Item1 - 1;
				int link2 = intraLink.Item2 == 0 ? intraLink.Item2 : intraLink.Item2 - 1;
				int min = Math.Min(link1, link2);
				int max = Math.Max(link1, link2);
				
				ProcessIntra( ntermFragMass, ctermFragMass, ntermInfo, ctermInfo, min, max, crosslinker);
			}
		}

		private static int[] Transform(IList<int> links, IList<int> inds){
			if (links != null) {
				int[] result = new int[links.Count];
				for (int i = 0; i < result.Length; i++){
					result[i] = inds[links[i]];
				}
				return result;
			}

			return new int[0];
		}

		private static Tuple<int, int>[] Transform(IList<Tuple<int, int>> links, IList<int> inds1, IList<int> inds2){
			if (links != null) {
				Tuple<int, int>[] result = new Tuple<int, int>[links.Count];
				for (int i = 0; i < result.Length; i++){
					if (inds1.Count > 0 && inds2.Count > 0) {
						result[i] = new Tuple<int, int>(inds1[links[i].Item1], inds2[links[i].Item2]);
						// rest to make sure to support monolinked peptides 
					} else if (inds1.Count > 0 && inds2.Count == 0) {
						result[i] = new Tuple<int, int>(inds1[links[i].Item1], 0);
					}else if (inds1.Count == 0 && inds2.Count > 0) {
						result[i] = new Tuple<int, int>(0, inds2[links[i].Item2]);
					} else {
						result[i] = new Tuple<int, int>(0, 0);
					}
				}
				return result;
			}
			return new Tuple<int, int>[0];
		}

		public double GetDeltaMass1(CrossLinker crossLinker, bool isMs3Cleavable){
			if (crossLinker.IsMsCleavableCrosslinker && isMs3Cleavable) {
				if (mscleasymm == MsCleavableSymmetry.LongerArmCrosslinker) {
					return (interLinksMs3.Length * crossLinker.CrossFragmentLongMass) + (intraLinks1.Length * crossLinker.LinkedMass) + (unsatLinks1.Length * crossLinker.HydrolyzedMass);
				}
				if (mscleasymm == MsCleavableSymmetry.ShorterArmCrosslinker) {
					return (interLinksMs3.Length * crossLinker.CrossFragmentShortMass) + (intraLinks1.Length * crossLinker.LinkedMass) + (unsatLinks1.Length * crossLinker.HydrolyzedMass);
				}
			}
			return intraLinks1.Length*crossLinker.LinkedMass + unsatLinks1.Length*crossLinker.HydrolyzedMass;
		}

		public double GetDeltaMass2(CrossLinker crossLinker) {
			return intraLinks2.Length * crossLinker.LinkedMass + unsatLinks2.Length * crossLinker.HydrolyzedMass;
		}


        /// <summary>
        /// This method returns the linking pattern for an inter-crosslink
        /// </summary>
        public string GetInterLinks() {
	        string interLinkStr = "";
	        if (interLinks != null && interLinks.Length == 0) {
		        interLinkStr = "-";
	        }

	        if (interLinks != null)
		        for (int i = 0; i < interLinks.Length; i++) {
			        Tuple<int, int> tmp = interLinks[i];
			        interLinkStr += tmp.Item1 + ":" + tmp.Item2 + ";";
		        }

	        if (interLinksMs3 != null && interLinksMs3.Length == 0) {
		        interLinkStr = "-";
	        }
	        if (interLinksMs3 != null)
		        for (int i = 0; i < interLinksMs3.Length; i++) {
			        int tmp = interLinksMs3[i];
			        interLinkStr += tmp + ":" + "-" + ";";
		        }

			return interLinkStr;
        }

        /// <summary>
        /// This method returns the linking pattern for the first part (residue) of an intra-crosslink
        /// </summary>
        public string GetIntraLinks1() {
	        string intraLinkStr = "";
	        if (intraLinks1 != null && intraLinks1.Length == 0) {
		        intraLinkStr = "-";
	        }

	        if (intraLinks1 != null)
		        for (int i = 0; i < intraLinks1.Length; i++) {
			        Tuple<int, int> tmp = intraLinks1[i];
			        intraLinkStr += tmp.Item1 + ":" + tmp.Item2 + ";";
		        }

	        return intraLinkStr;
        }

        /// <summary>
        /// This method returns the linking pattern for the second part (residue) of an intra-crosslink
        /// </summary>
        public string GetIntraLinks2() {
	        string intraLinkStr = "";
	        if (intraLinks2 == null)
		        return "-";
			if (intraLinks2.Length == 0) {
		        intraLinkStr = "-";
	        }

	        for (int i = 0; i < intraLinks2.Length; i++) {
		        Tuple<int, int> tmp = intraLinks2[i];
		        intraLinkStr += tmp.Item1 + ":" + tmp.Item2 + ";";
	        }

	        return intraLinkStr;
        }

        /// <summary>
        /// This method returns the linking pattern for the first part (residue) of an unsaturated-crosslink
        /// </summary>
        public string GetUnsatLinks1() {
	        string unsatLinks = "";
	        if (unsatLinks1 != null && unsatLinks1.Length == 0) {
		        unsatLinks = "-";
	        }

	        if (unsatLinks1 != null)
		        for (int i = 0; i < unsatLinks1.Length; i++) {
			        int tmp = unsatLinks1[i];
			        unsatLinks += tmp + ";";
		        }

	        return unsatLinks;
        }

        /// <summary>
        /// This method returns the linking pattern for the second part (residue) of an unsaturated-crosslink
        /// </summary>
        public string GetUnsatLinks2() {
	        string unsatLinks = "";
			if (unsatLinks2 == null)
				return "-";
			if (unsatLinks2.Length == 0) {
		        unsatLinks = "-";
	        }
			for (int i = 0; i < unsatLinks2.Length; i++) {
		        int tmp = unsatLinks2[i];
		        unsatLinks += tmp + ";";
	        }

	        return unsatLinks;
        }

        protected bool Equals(LinkPatternResult other) {
	        if (interLinks?.Length != other?.interLinks?.Length ||
	            intraLinks1?.Length!= other?.intraLinks1?.Length ||
	            intraLinks2?.Length != other?.intraLinks2?.Length ||
	            unsatLinks1?.Length != other?.unsatLinks1?.Length||
	            unsatLinks2?.Length != other?.unsatLinks2?.Length ||
	            interLinksMs3?.Length != other?.interLinksMs3?.Length) {
		        return false;
	        }

	        if (mscleasymm != other?.mscleasymm) {
		        return false;
	        }
			for (int i = 0; i < interLinks?.Length; i++) {
		        if (interLinks?[i].Item1 != other?.InterLinks[i]?.Item1) {
			        return false;
		        }
		        if (interLinks?[i].Item2 != other?.InterLinks[i]?.Item2) {
			        return false;
		        }
			}

			for (int i = 0; i < intraLinks1?.Length; i++) {
		        if (intraLinks1?[i].Item1 != other.intraLinks1?[i].Item1) {
			        return false;
		        }
		        if (intraLinks1?[i].Item2 != other.intraLinks1?[i].Item2) {
			        return false;
		        }
	        }
			for (int i = 0; i < intraLinks2?.Length; i++) {
				if (intraLinks2?[i].Item1 != other.intraLinks2?[i].Item1) {
					return false;
				}
				if (intraLinks2?[i].Item2 != other.intraLinks2?[i].Item2) {
					return false;
				}
			}
			if (unsatLinks1 != null && unsatLinks1.Where((t, i) => t != other.unsatLinks1[i]).Any()) {
				return false;
			}
			if (unsatLinks2 != null && unsatLinks2.Where((t, i) => t != other.unsatLinks2[i]).Any()) {
				return false;
			}
			if (interLinksMs3 != null && !interLinksMs3.Where((t, i) => t != other.interLinksMs3[i]).Any()) {
				return false;
			}
			return true;
        }

        public override bool Equals(object obj) {
	        if (ReferenceEquals(null, obj)) return false;
	        if (ReferenceEquals(this, obj)) return true;
	        if (obj.GetType() != this.GetType()) return false;
	        return Equals((LinkPatternResult) obj);
        }

        public override int GetHashCode() {
	        unchecked {
		        int hash = interLinks.Aggregate(0, (current, t) => (current * 397) ^ t.GetHashCode());
				hash = intraLinks1.Aggregate(hash, (current, t) => (current * 397) ^ t.GetHashCode());
				hash = intraLinks2.Aggregate(hash, (current, t) => (current * 397) ^ t.GetHashCode());
				hash = unsatLinks1.Aggregate(hash, (current, t) => (current * 397) ^ t.GetHashCode());
				hash = unsatLinks2.Aggregate(hash, (current, t) => (current * 397) ^ t.GetHashCode());
				hash = (hash * 397) ^ mscleasymm.GetHashCode();
				return hash;
	        }
        }
	}


	public class LinkSiteInfo {
		public int Index = -1;
		public sbyte NumUnsat { get; set; }
		public sbyte NumIntra { get; set; }
		public sbyte NumInter { get;  set; }

		public LinkSiteInfo() {
			NumUnsat = 0;
			NumIntra = 0;
			NumInter = 0;
		}

		public LinkSiteInfo(sbyte numUnsat, sbyte numIntra, sbyte numInter) {
			NumUnsat = numUnsat;
			NumIntra = numIntra;
			NumInter = numInter;
		}
	}
}
