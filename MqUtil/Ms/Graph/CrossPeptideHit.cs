using MqApi.Util;
using MqUtil.Mol;
using MqUtil.Ms.Annot;
using MqUtil.Ms.Annot.Ions;
using MqUtil.Ms.Enums;
using MqUtil.Ms.Search;
using MqUtil.Ms.Utils;
namespace MqUtil.Ms.Graph{
	public class CrossPeptideHit : PeptideHit{
		public string Sequence1{ get; set; }
		public string Sequence2{ get; set; }
		public PeptideModificationState VarMods1{ get; set; }
		public PeptideModificationState VarMods2{ get; set; }
		public IList<PeakAnnotation> Annotations1{ get; set; }
		public IList<PeakAnnotation> Annotations2{ get; set; }
		public IList<CrossLinkSpecificFragmentAnnotation>
			CLAnnotations1{ get; set; } // Crosslinking related fragment details
		public IList<CrossLinkSpecificFragmentAnnotation> CLAnnotations2{ get; set; }
		public string AllAnnotations{ get; set; }
		public string AllAnnotationMzs{ get; set; }
		public LinkPatternData LinkedPatternData{ get; set; }
		public List<CrosslinkSite> Links{ get; set; }
		public CrossPeptideHit(string sequence1, string sequence2, PeptideModificationState varMods1,
			PeptideModificationState varMods2, LinkPatternData linkedPatternData, string allAnnotations,
			string allAnnotationMzs){
			Sequence1 = sequence1;
			Sequence2 = sequence2;
			VarMods1 = varMods1;
			VarMods2 = varMods2;
			Annotations1 = new List<PeakAnnotation>();
			Annotations2 = new List<PeakAnnotation>();
			CLAnnotations1 = new List<CrossLinkSpecificFragmentAnnotation>();
			CLAnnotations2 = new List<CrossLinkSpecificFragmentAnnotation>();
			LinkedPatternData = linkedPatternData;
			AllAnnotations = allAnnotations;
			AllAnnotationMzs = allAnnotationMzs;
			GetAnnotations();
			ConvertToLinks();
		}
		/// <summary>
		/// From LinkedPatternData, generates crosslink sites.
		/// </summary>
		private void ConvertToLinks(){
			Tuple<int, int>[] interLinks = ConvertToTuple(LinkedPatternData?.InterLinks);
			Tuple<int, int>[] intraLinks1 = ConvertToTuple(LinkedPatternData?.IntraLinks1);
			Tuple<int, int>[] intraLinks2 = ConvertToTuple(LinkedPatternData?.IntraLinks2);
			int[] unsatLinks1 = ConvertToIntArr(LinkedPatternData?.UnsaturatedLinks1);
			int[] unsatLinks2 = ConvertToIntArr(LinkedPatternData?.UnsaturatedLinks2);
			Links = new List<CrosslinkSite>(interLinks.Length + intraLinks1.Length + intraLinks2.Length
			                                + unsatLinks1.Length + unsatLinks2.Length);
			foreach (Tuple<int, int> link in interLinks){
				Links.Add(new CrosslinkSite(CrossLinkType.InterLink, (byte) link.Item1, (byte) link.Item2));
			}
			foreach (Tuple<int, int> link in intraLinks1){
				Links.Add(new CrosslinkSite(CrossLinkType.IntraLink1, (byte) link.Item1, (byte) link.Item2));
			}
			foreach (Tuple<int, int> link in intraLinks2){
				Links.Add(new CrosslinkSite(CrossLinkType.IntraLink2, (byte) link.Item1, (byte) link.Item2));
			}
			foreach (int link in unsatLinks1){
				Links.Add(new CrosslinkSite(CrossLinkType.MonoLink1, (byte) link, byte.MinValue));
			}
			foreach (int link in unsatLinks2){
				Links.Add(new CrosslinkSite(CrossLinkType.MonoLink2, (byte) link, byte.MinValue));
			}
		}
		public static Tuple<int, int>[] ConvertToTuple(string bestLinksStr) {
			Tuple<int, int>[] links = new Tuple<int, int>[0];
			if (bestLinksStr == null) {
				return links;
			}
			if (bestLinksStr.Contains(';')) {
				string[] bestLinks = bestLinksStr.Split(';');
				int index = 0;
				links = new Tuple<int, int>[bestLinks.Length - 1];
				foreach (string bestLink in bestLinks) {
					if (bestLink.Length > 0) {
						int link1 = int.Parse(bestLink.Split(':')[0]);
						int link2 = bestLink.Split(':')[1].Equals("-") ? -1 : int.Parse(bestLink.Split(':')[1]);
						links[index] = new Tuple<int, int>(link1, link2);
						index++;
					}
				}
			}
			return links;
		}
		public static int[] ConvertToIntArr(string bestLinksStr) {
			int[] links = new int[0];
			if (bestLinksStr == null) {
				return links;
			}
			if (bestLinksStr.Contains(';')) {
				string[] bestLinks = bestLinksStr.Split(';');
				int index = 0;
				links = new int[bestLinks.Length - 1];
				foreach (string bestLink in bestLinks) {
					if (bestLink.Length > 0) {
						links[index] = int.Parse(bestLink);
						index++;
					}
				}
			}
			return links;
		}

		private void GetAnnotations(){
			string[] anns = AllAnnotationMzs.Length == 0 ? new string[]{ } : AllAnnotations.Split(';');
			string[] annMzs = AllAnnotationMzs.Length == 0 ? new string[]{ } : AllAnnotationMzs.Split(';');
			for (int i = 0; i < anns.Length; i++){
				string ann = anns[i];
				double mz = Parser.Double(annMzs[i]);
				bool isAlpha = false;
				ushort neutralLossType = 0;
				if (ann.StartsWith("alpha")){
					isAlpha = true;
				}
				string[] splitted = ann.Split('-');
				string ionType = splitted[1];
				CrossLinkSpecificFragmentAnnotation clSpecificAnn =
					(splitted.Length == 3 && (splitted[2].Equals("L") || splitted[2].Equals("S") ||
					                          splitted[2].Equals("Pep") || splitted[2].Equals("ML")))
						? StringToCrosslinkSpecificFragmentAnnot(splitted[2])
						: ((splitted.Length == 4 && (splitted[3].Equals("L") || splitted[3].Equals("S") ||
						                             splitted[3].Equals("Pep") || splitted[3].Equals("ML")))
							? StringToCrosslinkSpecificFragmentAnnot(splitted[3])
							: CrossLinkSpecificFragmentAnnotation.None);
				int charge = 1;
				if (ionType.Contains('(')){
					string[] ionTypeSp = ionType.Split('(');
					ionType = ionTypeSp[0];
					charge = short.Parse(ionTypeSp[1].Split('+')[0]);
				}
				int index = -1;
				IonType ion = IonType.A;
				if (ionType.StartsWith("a")){
					index = short.Parse(ionType.Split('a')[1]);
				} else if (ionType.StartsWith("b")){
					index = short.Parse(ionType.Split('b')[1]);
					ion = IonType.B;
				} else if (ionType.StartsWith("c")){
					index = short.Parse(ionType.Split('c')[1]);
					ion = IonType.C;
				} else if (ionType.StartsWith("x")){
					index = short.Parse(ionType.Split('x')[1]);
					ion = IonType.X;
				} else if (ionType.StartsWith("y")){
					index = short.Parse(ionType.Split('y')[1]);
					ion = IonType.Y;
				} else if (ionType.StartsWith("z")){
					index = short.Parse(ionType.Split('z')[1]);
					ion = IonType.Z;
				}
				PeakAnnotation peakAnn = isAlpha
					? new MsmsPeakAnnotation(ion, index, charge, mz, neutralLossType, 1, clSpecificAnn)
					: new MsmsPeakAnnotation(ion, index, charge, mz, neutralLossType, 2, clSpecificAnn);
				string loss = splitted.Length > 3 ? splitted[2] : "";
				if (loss != ""){
					if (loss.Equals("NH3")){
						peakAnn = isAlpha
							? new LossPeakAnnotation(peakAnn, MolUtil.ammoniaLossLib, 1, clSpecificAnn)
							: new LossPeakAnnotation(peakAnn, MolUtil.ammoniaLossLib, 2, clSpecificAnn);
					}
					if (loss.Equals("H2O")){
						peakAnn = isAlpha
							? new LossPeakAnnotation(peakAnn, MolUtil.waterLossLib, 1, clSpecificAnn)
							: new LossPeakAnnotation(peakAnn, MolUtil.waterLossLib, 2, clSpecificAnn);
					}
					peakAnn.Mz = mz;
				}
				if (isAlpha){
					Annotations1.Add(peakAnn);
					CLAnnotations1.Add(clSpecificAnn);
				} else{
					Annotations2.Add(peakAnn);
					CLAnnotations2.Add(clSpecificAnn);
				}
			}
		}
		public static CrossLinkSpecificFragmentAnnotation StringToCrosslinkSpecificFragmentAnnot(string em) {
			switch (em) {
				case "None":
					return CrossLinkSpecificFragmentAnnotation.None;
				case "":
					return CrossLinkSpecificFragmentAnnotation.None;
				case "S":
					return CrossLinkSpecificFragmentAnnotation.S;
				case "L":
					return CrossLinkSpecificFragmentAnnotation.L;
				case "Pep":
					return CrossLinkSpecificFragmentAnnotation.Pep;
				case "ML":
					return CrossLinkSpecificFragmentAnnotation.ML;
				default:
					throw new Exception("Never get here");
			}
		}
	}
}