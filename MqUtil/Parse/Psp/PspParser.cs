using MqApi.Util;
using MqUtil.Mol;

namespace MqUtil.Parse.Psp{
	public static class PspParser{
		private static readonly HashSet<string> invalidOrganisms =
			new HashSet<string>(new[]{"monkey", "hamster", "frog", "buffalo", "water buffalo", "starfish", "torpedo"});

		public static Dictionary<string, PspProtein> Parse(string folder){
			string[] filenames = {
				"Acetylation_site_dataset", "Methylation_site_dataset", "O-GlcNAc_site_dataset", "O-GalNAc_site_dataset",
				"Phosphorylation_site_dataset", "Sumoylation_site_dataset", "Ubiquitination_site_dataset"
			};
			Dictionary<string, PspProtein> map = new Dictionary<string, PspProtein>();
			Dictionary<Tuple<string, string>, string> name2Acc = new Dictionary<Tuple<string, string>, string>();
			foreach (string filename in filenames){
				string acFile = folder + filename;
				string[] names = TabSep.GetColumn("PROTEIN", acFile, 3, '\t');
				string[] accs = TabSep.GetColumn("ACC_ID", acFile, 3, '\t');
				string[] modtypes = TabSep.GetColumn("MOD_TYPE", acFile, 3, '\t');
				string[] modress = TabSep.GetColumn("MOD_RSD", acFile, 3, '\t');
				string[] orgs = TabSep.GetColumn("ORG", acFile, 3, '\t');
				string[] domains = TabSep.GetColumn("IN_DOMAIN", acFile, 3, '\t');
				string[] siteGrpIds = TabSep.GetColumn("SITE_GRP_ID", acFile, 3, '\t');
				string[] modsiteSeq = TabSep.GetColumn("MODSITE_SEQ", acFile, 3, '\t');
				for (int i = 0; i < accs.Length; i++){
					string acc = accs[i];
					if (invalidOrganisms.Contains(orgs[i])){
						continue;
					}
					PtmType modtype = GetPtmType(modtypes[i]);
					string modres = modress[i];
					char res = modres[0];
					int pos = int.Parse(modres.Substring(1));
					if (!modtype.Residues.Contains("" + res)){
						throw new Exception(modtype.PspName + " " + res);
					}
					if (!map.ContainsKey(acc)){
						map.Add(acc, new PspProtein(names[i]));
					}
					if (!map[acc].sites.ContainsKey(pos)){
						map[acc].sites.Add(pos, new PspPtmSite(acc, res, pos, Cleanup(domains[i]), modsiteSeq[i]));
					}
					map[acc].sites[pos].Add(modtype, siteGrpIds[i]);
					Tuple<string, string> key = new Tuple<string, string>(names[i], orgs[i]);
					if (!name2Acc.ContainsKey(key)){
						name2Acc.Add(key, acc);
					}
				}
			}
			ParseRegulatorySites(folder, name2Acc, map);
			ParseDiseaseAssociatedSites(folder, name2Acc, map);
			ParseVariation(folder, name2Acc, map);
			ParseKinaseSubstrateRelations(folder, name2Acc, map);
			return map;
		}

		private static void ParseKinaseSubstrateRelations(string folder, IDictionary<Tuple<string, string>, string> name2Acc,
			IDictionary<string, PspProtein> map){
			string file1 = folder + "Kinase_Substrate_Dataset";
			string[] kinaseNames = TabSep.GetColumn("KINASE", file1, 3, '\t');
			string[] kinaseAccs = TabSep.GetColumn("KIN_ACC_ID", file1, 3, '\t');
			string[] substrateNames = TabSep.GetColumn("SUBSTRATE", file1, 3, '\t');
			string[] substrateAccs = TabSep.GetColumn("SUB_ACC_ID", file1, 3, '\t');
			string[] modRes = TabSep.GetColumn("SUB_MOD_RSD", file1, 3, '\t');
			string[] siteGroup = TabSep.GetColumn("SITE_GRP_ID", file1, 3, '\t');
			string[] inVivo = TabSep.GetColumn("IN_VIVO_RXN", file1, 3, '\t');
			string[] inVitro = TabSep.GetColumn("IN_VITRO_RXN", file1, 3, '\t');
			string[] modsiteSeq1 = TabSep.GetColumn("MODSITE_SEQ", file1, 3, '\t');
			for (int i = 0; i < kinaseNames.Length; i++){
				string acc = substrateAccs[i];
				string x = modRes[i];
				char res = x[0];
				int pos = int.Parse(x.Substring(1));
				if (!map.ContainsKey(acc)){
					map.Add(acc, new PspProtein(substrateNames[i]));
				}
				PspProtein p = map[acc];
				if (!p.sites.ContainsKey(pos)){
					string seq = modsiteSeq1[i];
					seq = StringUtils.Replace(seq, new[]{"*"}, "");
					p.sites.Add(pos, new PspPtmSite(acc, res, pos, new string[0], seq));
				}
				PspPtmSite site = p.sites[pos];
				PtmType modtype = PtmTypes.phosphorylation;
				site.AddWriterEraserInfo(modtype, siteGroup[i], kinaseNames[i], kinaseAccs[i], inVivo[i].Trim().Equals("X"),
					inVitro[i].Trim().Equals("X"));
			}
		}

		private static void ParseVariation(string folder, IDictionary<Tuple<string, string>, string> name2Acc,
			IDictionary<string, PspProtein> map){
			string file1 = folder + "PTMVar.txt";
			//TODO more offset lines
			string[] names1 = TabSep.GetColumn("PROT_NAME", file1, 0, '\t');
			string[] accs1 = TabSep.GetColumn("ACC_ID", file1, 0, '\t');
			string[] modtypes1 = TabSep.GetColumn("MOD_TYPE", file1, 0, '\t');
			string[] modressNum = TabSep.GetColumn("MOD RSD#", file1, 0, '\t');
			string[] modressAa = TabSep.GetColumn("MOD_AA", file1, 0, '\t');
			string[] siteGrpIds1 = TabSep.GetColumn("SITE_GRP_ID", file1, 0, '\t');
			string[] modsiteSeq1 = TabSep.GetColumn("MOD-SITE_SEQ", file1, 0, '\t');
			string[] varsiteSeq1 = TabSep.GetColumn("VAR_SITE_SEQ", file1, 0, '\t');
			string[] ftids = TabSep.GetColumn("FTID", file1, 0, '\t');
			string[] dbsnps = TabSep.GetColumn("dbSNP", file1, 0, '\t');
			string[] wtAa = TabSep.GetColumn("WT AA", file1, 0, '\t');
			string[] mutResPos = TabSep.GetColumn("MUT RSD#", file1, 0, '\t');
			string[] varAas = TabSep.GetColumn("VAR AA", file1, 0, '\t');
			string[] varTypes = TabSep.GetColumn("VAR_TYPE", file1, 0, '\t');
			string[] diseases = TabSep.GetColumn("DISEASE(s)", file1, 0, '\t');
			string[] varClass = TabSep.GetColumn("VAR_CLASS", file1, 0, '\t');
			for (int i = 0; i < accs1.Length; i++){
				string acc = accs1[i];
				Tuple<string, string> key = new Tuple<string, string>(names1[i], "human");
				if (!name2Acc.ContainsKey(key)){
					name2Acc.Add(key, acc);
				}
			}
			for (int i = 0; i < accs1.Length; i++){
				string acc = accs1[i];
				if (!map.ContainsKey(acc)){
					map.Add(acc, new PspProtein(names1[i]));
				}
				PspProtein p = map[acc];
				string q = modressNum[i];
				char res = modressAa[i][0];
				PtmType modtype = GetPtmType(modtypes1[i]);
				int pos = int.Parse(q);
				if (!p.sites.ContainsKey(pos)){
					string seq = modsiteSeq1[i];
					seq = StringUtils.Replace(seq, new[]{"*"}, "");
					p.sites.Add(pos, new PspPtmSite(acc, res, pos, new string[0], seq));
				}
				map[acc].sites[pos].Add(modtype, siteGrpIds1[i]);
				string diseaseStr = diseases[i].Trim();
				if (diseaseStr.Equals("-")){
					diseaseStr = "";
				}
				string[] diseaseArr = Cleanup(diseaseStr);
				string[] disease;
				string[] mimIds;
				SplitDiseases(diseaseArr, out disease, out mimIds);
				map[acc].sites[pos].AddVariationInfo(modtype, varsiteSeq1[i], ftids[i], dbsnps[i], wtAa[i][0],
					int.Parse(mutResPos[i]), varAas[i][0], ToVarType(varTypes[i]), disease, mimIds, ToVarClass(varClass[i]));
			}
		}

		private static VariationClass ToVarClass(string s){
			switch (s){
				case "I":
				case "Ia":
				case "II":
					return VariationClass.Unknown;
				default:
					throw new Exception("Unknown var class: " + s);
			}
		}

		private static VariationType ToVarType(string s){
			switch (s){
				case "Polymorphism":
					return VariationType.Polymorphism;
				case "Disease":
					return VariationType.Disease;
				case "Unclassified":
					return VariationType.Unknown;
				default:
					throw new Exception("Unknown var type: " + s);
			}
		}

		private static void SplitDiseases(IEnumerable<string> s, out string[] disease, out string[] mimIds){
			List<string> disease1 = new List<string>();
			List<string> mimIds1 = new List<string>();
			foreach (string q in s){
				string w = q.Trim();
				if (w.EndsWith("]")){
					int ind = w.LastIndexOf('[');
					disease1.Add(w.Substring(0, ind).Trim());
					string g = w.Substring(ind + 1);
					mimIds1.Add(g.Substring(0, g.Length - 1));
				} else{
					disease1.Add(w);
				}
			}
			disease = disease1.ToArray();
			mimIds = mimIds1.ToArray();
		}

		private static void ParseDiseaseAssociatedSites(string folder, IDictionary<Tuple<string, string>, string> name2Acc,
			IDictionary<string, PspProtein> map){
			string file1 = folder + "Disease-associated_sites";
			string[] names1 = TabSep.GetColumn("PROTEIN", file1, 3, '\t');
			string[] accs1 = TabSep.GetColumn("ACC_ID", file1, 3, '\t');
			string[] modtypes1 = TabSep.GetColumn("MOD_TYPE", file1, 3, '\t');
			string[] modress1 = TabSep.GetColumn("RESIDUE", file1, 3, '\t');
			string[] orgs1 = TabSep.GetColumn("ORG", file1, 3, '\t');
			string[] domains1 = TabSep.GetColumn("IN_DOMAIN", file1, 3, '\t');
			string[] siteGrpIds1 = TabSep.GetColumn("SITE_GRP_ID", file1, 3, '\t');
			string[] modsiteSeq1 = TabSep.GetColumn("MODSITE_SEQ", file1, 3, '\t');
			string[] notes = TabSep.GetColumn("NOTES", file1, 3, '\t');
			string[] disease = TabSep.GetColumn("DISEASE", file1, 3, '\t');
			string[] alteration = TabSep.GetColumn("ALTERATION", file1, 3, '\t');
			for (int i = 0; i < accs1.Length; i++){
				string acc = accs1[i];
				Tuple<string, string> key = new Tuple<string, string>(names1[i], orgs1[i]);
				if (!name2Acc.ContainsKey(key)){
					name2Acc.Add(key, acc);
				}
			}
			for (int i = 0; i < accs1.Length; i++){
				string acc = accs1[i];
				if (invalidOrganisms.Contains(orgs1[i])){
					continue;
				}
				if (!map.ContainsKey(acc)){
					map.Add(acc, new PspProtein(names1[i]));
				}
				PspProtein p = map[acc];
				string q = modress1[i];
				char res = q[0];
				PtmType modtype = GetPtmType(modtypes1[i]);
				int pos = int.Parse(q.Substring(1));
				if (!p.sites.ContainsKey(pos)){
					p.sites.Add(pos, new PspPtmSite(acc, res, pos, Cleanup(domains1[i]), modsiteSeq1[i]));
				}
				map[acc].sites[pos].Add(modtype, siteGrpIds1[i]);
				map[acc].sites[pos].AddDiseaseAssociatedInfo(modtype, Cleanup(disease[i]), Cleanup(alteration[i]), notes[i]);
			}
		}

		private static void ParseRegulatorySites(string folder, Dictionary<Tuple<string, string>, string> name2Acc,
			IDictionary<string, PspProtein> map){
			string file1 = folder + "Regulatory_sites";
			string[] names1 = TabSep.GetColumn("PROTEIN", file1, 3, '\t');
			string[] accs1 = TabSep.GetColumn("ACC_ID", file1, 3, '\t');
			string[] modtypes1 = TabSep.GetColumn("MOD_TYPE", file1, 3, '\t');
			string[] modress1 = TabSep.GetColumn("MOD_RSD", file1, 3, '\t');
			string[] orgs1 = TabSep.GetColumn("ORG", file1, 3, '\t');
			string[] domains1 = TabSep.GetColumn("IN_DOMAIN", file1, 3, '\t');
			string[] siteGrpIds1 = TabSep.GetColumn("SITE_GRP_ID", file1, 3, '\t');
			string[] modsiteSeq1 = TabSep.GetColumn("MODSITE_SEQ", file1, 3, '\t');
			string[] function = TabSep.GetColumn("ON_FUNCTION", file1, 3, '\t');
			string[] process = TabSep.GetColumn("ON_PROCESS", file1, 3, '\t');
			string[] protInteract = TabSep.GetColumn("ON_PROT_INTERACT", file1, 3, '\t');
			string[] otherInteract = TabSep.GetColumn("ON_OTHER_INTERACT", file1, 3, '\t');
			string[] notes = TabSep.GetColumn("NOTES", file1, 3, '\t');
			for (int i = 0; i < accs1.Length; i++){
				string acc = accs1[i];
				Tuple<string, string> key = new Tuple<string, string>(names1[i], orgs1[i]);
				if (!name2Acc.ContainsKey(key)){
					name2Acc.Add(key, acc);
				}
			}
			for (int i = 0; i < accs1.Length; i++){
				string acc = accs1[i];
				if (invalidOrganisms.Contains(orgs1[i])){
					continue;
				}
				if (!map.ContainsKey(acc)){
					map.Add(acc, new PspProtein(names1[i]));
				}
				PspProtein p = map[acc];
				string q = modress1[i];
				char res = q[0];
				PtmType modtype = GetPtmType(modtypes1[i]);
				int pos = int.Parse(q.Substring(1));
				if (!p.sites.ContainsKey(pos)){
					p.sites.Add(pos, new PspPtmSite(acc, res, pos, Cleanup(domains1[i]), modsiteSeq1[i]));
				}
				map[acc].sites[pos].Add(modtype, siteGrpIds1[i]);
				Tuple<SiteFunction, SiteFunctionQualifier>[] func = ParseFunction(Cleanup(function[i]));
				Tuple<SiteProcess, SiteFunctionQualifier>[] proc = ParseProcess(Cleanup(process[i]));
				Tuple<string, ProteinInteractionInfluence>[] protInt = ParseProtInteract(Cleanup(protInteract[i]), name2Acc,
					orgs1[i]);
				Tuple<string, ProteinInteractionInfluence>[] otherInt = ParseOtherInteract(Cleanup(otherInteract[i]));
				map[acc].sites[pos].AddRegulatoryInfo(modtype, func, proc, protInt, otherInt, notes[i]);
			}
		}

		private static Tuple<string, ProteinInteractionInfluence>[] ParseOtherInteract(IList<string> s){
			Tuple<string, ProteinInteractionInfluence>[] result = new Tuple<string, ProteinInteractionInfluence>[s.Count];
			for (int i = 0; i < result.Length; i++){
				result[i] = ParseOtherInteract(s[i]);
			}
			return result;
		}

		private static Tuple<string, ProteinInteractionInfluence>[] ParseProtInteract(IList<string> s,
			IDictionary<Tuple<string, string>, string> name2Acc, string org){
			Tuple<string, ProteinInteractionInfluence>[] result = new Tuple<string, ProteinInteractionInfluence>[s.Count];
			for (int i = 0; i < result.Length; i++){
				result[i] = ParseProtInteract(s[i], name2Acc, org);
			}
			return result;
		}

		private static Tuple<string, ProteinInteractionInfluence> ParseOtherInteract(string s){
			int openInd = s.LastIndexOf('(');
			int closeInd = s.LastIndexOf(')');
			string name = s.Substring(0, openInd);
			ProteinInteractionInfluence q = GetInteractQualifier(s.Substring(openInd + 1, closeInd - openInd - 1));
			return new Tuple<string, ProteinInteractionInfluence>(name, q);
		}

		private static Tuple<string, ProteinInteractionInfluence> ParseProtInteract(string s,
			IDictionary<Tuple<string, string>, string> name2Acc, string org){
			int openInd = s.LastIndexOf('(');
			int closeInd = s.LastIndexOf(')');
			string name = s.Substring(0, openInd);
			Tuple<string, string> key = new Tuple<string, string>(name, org);
			ProteinInteractionInfluence q = GetInteractQualifier(s.Substring(openInd + 1, closeInd - openInd - 1));
			return !name2Acc.ContainsKey(key)
				? new Tuple<string, ProteinInteractionInfluence>("name:" + name, q)
				: new Tuple<string, ProteinInteractionInfluence>(name2Acc[key], q);
		}

		private static ProteinInteractionInfluence GetInteractQualifier(string s){
			switch (s){
				case "DISRUPTS":
					return ProteinInteractionInfluence.Disrupts;
				case "INDUCES":
					return ProteinInteractionInfluence.Induces;
				case "NOT_REPORTED":
					return ProteinInteractionInfluence.Unknown;
				default:
					throw new Exception("Unknown qualifier: " + s);
			}
		}

		private static Tuple<SiteProcess, SiteFunctionQualifier>[] ParseProcess(IList<string> procs){
			Tuple<SiteProcess, SiteFunctionQualifier>[] result = new Tuple<SiteProcess, SiteFunctionQualifier>[procs.Count];
			for (int i = 0; i < result.Length; i++){
				result[i] = ParseProcess(procs[i]);
			}
			return result;
		}

		private static Tuple<SiteFunction, SiteFunctionQualifier>[] ParseFunction(IList<string> funcs){
			Tuple<SiteFunction, SiteFunctionQualifier>[] result = new Tuple<SiteFunction, SiteFunctionQualifier>[funcs.Count];
			for (int i = 0; i < result.Length; i++){
				result[i] = ParseFunction(funcs[i]);
			}
			return result;
		}

		private static Tuple<SiteProcess, SiteFunctionQualifier> ParseProcess(string s){
			string func;
			string qualifier = null;
			int ind = s.IndexOf(',');
			if (ind >= 0){
				func = s.Substring(0, ind).Trim();
				qualifier = s.Substring(ind + 1).Trim();
			} else{
				func = s.Trim();
			}
			SiteFunctionQualifier sfq = qualifier != null ? GetSiteFunctionQualifier(qualifier) : SiteFunctionQualifier.Unknown;
			SiteProcess sf = GetSiteProcess(func);
			return new Tuple<SiteProcess, SiteFunctionQualifier>(sf, sfq);
		}

		private static Tuple<SiteFunction, SiteFunctionQualifier> ParseFunction(string s){
			string func;
			string qualifier = null;
			int ind = s.IndexOf(',');
			if (ind >= 0){
				func = s.Substring(0, ind).Trim();
				qualifier = s.Substring(ind + 1).Trim();
			} else{
				func = s.Trim();
			}
			SiteFunctionQualifier sfq = qualifier != null ? GetSiteFunctionQualifier(qualifier) : SiteFunctionQualifier.Unknown;
			SiteFunction sf = GetSiteFunction(func);
			return new Tuple<SiteFunction, SiteFunctionQualifier>(sf, sfq);
		}

		private static SiteFunctionQualifier GetSiteFunctionQualifier(string s){
			switch (s){
				case "regulation":
					return SiteFunctionQualifier.Regulation;
				case "inhibited":
					return SiteFunctionQualifier.Inhibited;
				case "induced":
					return SiteFunctionQualifier.Induced;
				case "altered":
					return SiteFunctionQualifier.Altered;
				default:
					throw new Exception("Unknown qualifier: " + s);
			}
		}

		private static SiteProcess GetSiteProcess(string s){
			switch (s){
				case "translation":
					return SiteProcess.Translation;
				case "cell adhesion":
					return SiteProcess.CellAdhesion;
				case "apoptosis":
					return SiteProcess.Apoptosis;
				case "transcription":
					return SiteProcess.Transcription;
				case "cell growth":
					return SiteProcess.CellGrowth;
				case "cell differentiation":
					return SiteProcess.CellDifferentiation;
				case "cell motility":
					return SiteProcess.CellMotility;
				case "cytoskeletal reorganization":
					return SiteProcess.CytoskeletalReorganization;
				case "signaling pathway regulation":
					return SiteProcess.SignalingPathwayRegulation;
				case "cell cycle regulation":
					return SiteProcess.CellCycleRegulation;
				case "endocytosis":
					return SiteProcess.Endocytosis;
				case "carcinogenesis":
					return SiteProcess.Carcinogenesis;
				case "exocytosis":
					return SiteProcess.Exocytosis;
				case "chromatin organization":
					return SiteProcess.ChromatinOrganization;
				case "autophagy":
					return SiteProcess.Autophagy;
				case "RNA splicing":
					return SiteProcess.RnaSplicing;
				case "DNA repair":
					return SiteProcess.DnaRepair;
				case "RNA stability":
					return SiteProcess.RnaStability;
				default:
					throw new Exception("Unknown process: " + s);
			}
		}

		private static SiteFunction GetSiteFunction(string s){
			switch (s){
				case "protein degradation":
					return SiteFunction.ProteinDegradation;
				case "intracellular localization":
					return SiteFunction.IntracellularLocalization;
				case "phosphorylation":
					return SiteFunction.Phosphorylation;
				case "molecular association":
					return SiteFunction.MolecularAssociation;
				case "enzymatic activity":
					return SiteFunction.EnzymaticActivity;
				case "activity":
					return SiteFunction.Activity;
				case "protein stabilization":
					return SiteFunction.ProteinStabilization;
				case "protein processing":
					return SiteFunction.ProteinProcessing;
				case "protein conformation":
					return SiteFunction.ProteinConformation;
				case "receptor desensitization":
					return SiteFunction.ReceptorDesensitization;
				case "ubiquitination":
					return SiteFunction.Ubiquitination;
				case "receptor internalization":
					return SiteFunction.ReceptorInternalization;
				case "sumoylation":
					return SiteFunction.Sumoylation;
				case "receptor inactivation":
					return SiteFunction.ReceptorInactivation;
				case "receptor recycling":
					return SiteFunction.ReceptorRecycling;
				case "methylation":
					return SiteFunction.Methylation;
				case "acetylation":
					return SiteFunction.Acetylation;
				case "O-GlcNAc glycosylation":
					return SiteFunction.OGlcNAcGlycosylation;
				case "palmitoylation":
					return SiteFunction.Palmitoylation;
				case "not reported":
					return SiteFunction.Unknown;
				case "neddylation":
					return SiteFunction.Neddylation;
				default:
					throw new Exception("Unknown function: " + s);
			}
		}

		private static string[] Cleanup(string s){
			if (s.Trim().Length == 0){
				return new string[0];
			}
			string[] w = s.Split(';');
			for (int i = 0; i < w.Length; i++){
				w[i] = w[i].Trim();
			}
			return w;
		}

		public static PtmType GetPtmType(string s){
			s = s.ToUpper();
			if (s == "O-GLCNAC"){
				s = "O-GlcNAc";
			}
			if (PtmTypes.mapPspToPtmType.ContainsKey(s)){
				return PtmTypes.mapPspToPtmType[s];
			}
			throw new Exception("Unknown mod:" + s);
		}
	}
}