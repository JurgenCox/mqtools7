using System.Text;
using MqApi.Num;
using MqApi.Util;
using MqUtil.Mol;

namespace MqUtil.Parse.Chebi{
	public static class ChebiParser{
		public static void Serialize(string chebiFolderName, string outName){
			ChebiModel m = Parse(chebiFolderName);
			BinaryWriter writer = FileUtils.GetBinaryWriter(outName);
			m.Write(writer);
			writer.Close();
		}

		public static ChebiModel Parse(string chebiFolderName){
			return Parse(chebiFolderName, 2000, false);
		}

		public static ChebiModel Parse(string chebiFolderName, double maxMass, bool parseReferences){
			Dictionary<int, ChebiEntry> x = ParseImpl(chebiFolderName, parseReferences);
			int[] keys = x.Keys.ToArray();
			Array.Sort(keys);
			Dictionary<int, ChebiEntry> main = new Dictionary<int, ChebiEntry>();
			Dictionary<int, ChebiEntry> series = new Dictionary<int, ChebiEntry>();
			foreach (int t in keys){
				ChebiEntry e = x[t];
				if (e.Formula == null){
					continue;
				}
				if (e.SeriesFormula == null){
					continue;
				}
				e.CompoundId = t;
				if (e.MonoisotopicMass == 0 && e.SeriesFormula.Length == 0){
					continue;
				}
				if (e.SeriesFormula.Length == 0){
					main.Add(t, e);
				} else{
					series.Add(t, e);
				}
			}
			return new ChebiModel(main.Values.ToArray(), series.Values.ToArray(), maxMass);
		}

		private static Dictionary<int, ChebiEntry> ParseImpl(string folderName, bool parseReferences){
			Dictionary<int, ChebiEntry> map = new Dictionary<int, ChebiEntry>();
			ParseChemicalData(folderName, map);
			ParseCompounds(folderName, map);
			ParseInchi(folderName, map);
			ParseNames(folderName, map);
			if (parseReferences){
				ParseReference(folderName, map);
			}
			ParseDatabaseAccession(folderName, map);
			return map;
		}

		private static StreamReader GetReader(string file){
			if (File.Exists(file)){
				return new StreamReader(file);
			}
			return File.Exists(file + ".gz") ? FileUtils.GetReader(file + ".gz") : null;
		}

		private static void ParseReference(string folderName, IDictionary<int, ChebiEntry> map){
			StreamReader compReader = GetReader(Path.Combine(folderName, "reference.tsv"));
			compReader.ReadLine();
			string line;
			while ((line = compReader.ReadLine()) != null){
				string[] w = line.Split('\t');
				if (!Parser.TryInt(w[0], out int compoundId)){
					continue;
				}
				if (!map.ContainsKey(compoundId)){
					continue;
				}
				string referenceId = w[1];
				string referenceDbName = w[2];
				string locationInRef = w[3];
				string referenceName = w[4];
				SmallMoleculeDbType dtype = GetDatabaseType(referenceDbName);
				Dictionary<SmallMoleculeDbType, List<SmallMoleculeDbReference>> e = map[compoundId].references;
				if (!e.ContainsKey(dtype)){
					e.Add(dtype, new List<SmallMoleculeDbReference>());
				}
				e[dtype].Add(new SmallMoleculeDbReference{
					LocationInRef = locationInRef, ReferenceId = referenceId, ReferenceName = referenceName
				});
			}
		}

		private static SmallMoleculeDbType GetDatabaseType(string referenceDbName){
			switch (referenceDbName){
				case "Patent":
					return SmallMoleculeDbType.Patent;
				case "IEDB":
					return SmallMoleculeDbType.Iedb;
				case "IntAct":
					return SmallMoleculeDbType.IntAct;
				case "Reactome":
					return SmallMoleculeDbType.Reactome;
				case "UniProt":
					return SmallMoleculeDbType.UniProt;
				case "ArrayExpressAtlas":
					return SmallMoleculeDbType.ArrayExpressAtlas;
				case "PubChem":
					return SmallMoleculeDbType.PubChem;
				case "SABIO-RK":
					return SmallMoleculeDbType.SabioRk;
				case "BioModels":
					return SmallMoleculeDbType.BioModels;
				case "BRENDA":
					return SmallMoleculeDbType.Brenda;
				case "Rhea":
					return SmallMoleculeDbType.Rhea;
				case "IntEnz":
					return SmallMoleculeDbType.IntEnz;
				case "NMRShiftDB":
					return SmallMoleculeDbType.NmrShiftDb;
				case "Golm":
					return SmallMoleculeDbType.Golm;
				case "NURSA":
					return SmallMoleculeDbType.Nursa;
				case "EnzymePortal":
					return SmallMoleculeDbType.EnzymePortal;
				case "MassBank":
					return SmallMoleculeDbType.MassBank;
				case "ArrayExpress":
					return SmallMoleculeDbType.ArrayExpress;
				case "ChEMBL":
					return SmallMoleculeDbType.ChEMBL;
				default:
					throw new Exception("Unknown db: " + referenceDbName);
			}
		}

		private static void ParseDatabaseAccession(string folderName, IDictionary<int, ChebiEntry> map){
			StreamReader compReader = GetReader(Path.Combine(folderName, "database_accession.tsv"));
			compReader.ReadLine();
			string line;
			while ((line = compReader.ReadLine()) != null){
				string[] w = line.Split('\t');
				int compoundId = int.Parse(w[1]);
				if (!map.ContainsKey(compoundId)){
					continue;
				}
				Dictionary<SmallMoleculeDbAccessionType, List<string>> acc = map[compoundId].accessions;
				string type = w[3];
				string accession = w[4];
				switch (type){
					case "KEGG COMPOUND accession":
						Add(SmallMoleculeDbAccessionType.KeggCompoundAccession, accession, acc);
						break;
					case "CAS Registry Number":
						Add(SmallMoleculeDbAccessionType.CasRegistryNumber, accession, acc);
						break;
					case "Beilstein Registry Number":
						Add(SmallMoleculeDbAccessionType.BeilsteinRegistryNumber, accession, acc);
						break;
					case "UM-BBD compID":
						Add(SmallMoleculeDbAccessionType.UmBbdCompId, accession, acc);
						break;
					case "LIPID MAPS instance accession":
						Add(SmallMoleculeDbAccessionType.LipidMapsInstanceAccession, accession, acc);
						break;
					case "KEGG DRUG accession":
						Add(SmallMoleculeDbAccessionType.KeggDrugAccession, accession, acc);
						break;
					case "Gmelin Registry Number":
						Add(SmallMoleculeDbAccessionType.GmelinRegistryNumber, accession, acc);
						break;
					case "LIPID MAPS class accession":
						Add(SmallMoleculeDbAccessionType.LipidMapsClassAccession, accession, acc);
						break;
					case "PDBeChem accession":
						Add(SmallMoleculeDbAccessionType.PdbeChemAccession, accession, acc);
						break;
					case "KEGG GLYCAN accession":
						Add(SmallMoleculeDbAccessionType.KeggGlycanAccession, accession, acc);
						break;
					case "COMe accession":
						Add(SmallMoleculeDbAccessionType.ComeAccession, accession, acc);
						break;
					case "RESID accession":
						Add(SmallMoleculeDbAccessionType.ResidAccession, accession, acc);
						break;
					case "MolBase accession":
						Add(SmallMoleculeDbAccessionType.MolBaseAccession, accession, acc);
						break;
					case "DrugBank accession":
						Add(SmallMoleculeDbAccessionType.DrugBankAccession, accession, acc);
						break;
					case "PDB accession":
						Add(SmallMoleculeDbAccessionType.PdbAccession, accession, acc);
						break;
					case "Patent accession":
						Add(SmallMoleculeDbAccessionType.PatentAccession, accession, acc);
						break;
					case "Reaxys Registry Number":
						Add(SmallMoleculeDbAccessionType.ReaxysRegistryNumber, accession, acc);
						break;
					case "WebElements accession":
						Add(SmallMoleculeDbAccessionType.WebElementsAccession, accession, acc);
						break;
					case "PubMed citation":
						Add(SmallMoleculeDbAccessionType.PubMedCitation, accession, acc);
						break;
					case "MetaCyc accession":
						Add(SmallMoleculeDbAccessionType.MetaCycAccession, accession, acc);
						break;
					case "Agricola citation":
						Add(SmallMoleculeDbAccessionType.AgricolaCitation, accession, acc);
						break;
					case "Wikipedia accession":
						Add(SmallMoleculeDbAccessionType.WikipediaAccession, accession, acc);
						break;
					case "HMDB accession":
						Add(SmallMoleculeDbAccessionType.HmdbAccession, accession, acc);
						break;
					case "Chinese Abstracts citation":
						Add(SmallMoleculeDbAccessionType.ChineseAbstractsCitation, accession, acc);
						break;
					case "Pubchem accession":
						Add(SmallMoleculeDbAccessionType.PubchemAccession, accession, acc);
						break;
					case "Chemspider accession":
						Add(SmallMoleculeDbAccessionType.ChemspiderAccession, accession, acc);
						break;
					case "ChemIDplus accession":
						Add(SmallMoleculeDbAccessionType.ChemIDplusAccession, accession, acc);
						break;
					case "PubMed Central citation":
						Add(SmallMoleculeDbAccessionType.PubMedCentralCitation, accession, acc);
						break;
					case "CiteXplore citation":
						Add(SmallMoleculeDbAccessionType.CiteXploreCitation, accession, acc);
						break;
					case "ETH":
						Add(SmallMoleculeDbAccessionType.Eth, accession, acc);
						break;
					case "ChEMBL COMPOUND":
						Add(SmallMoleculeDbAccessionType.ChEmblCompound, accession, acc);
						break;
					case "KNApSAcK accession":
						Add(SmallMoleculeDbAccessionType.KnapsackAccession, accession, acc);
						break;
					case "YMDB accession":
						Add(SmallMoleculeDbAccessionType.YmdbAccession, accession, acc);
						break;
					case "ECMDB accession":
						Add(SmallMoleculeDbAccessionType.EcmdbAccession, accession, acc);
						break;
					case "SMID accession":
						Add(SmallMoleculeDbAccessionType.SmidbAccession, accession, acc);
						break;
					case "Pesticides accession":
						Add(SmallMoleculeDbAccessionType.PesticidesAccession, accession, acc);
						break;
					case "LINCS accession":
						Add(SmallMoleculeDbAccessionType.LincsAccession, accession, acc);
						break;
					case "Drug Central accession":
						Add(SmallMoleculeDbAccessionType.DrugCentralAccession, accession, acc);
						break;
					case "FooDB accession":
						Add(SmallMoleculeDbAccessionType.FooDbAccession, accession, acc);
						break;
					case "FAO/WHO standards accession":
						Add(SmallMoleculeDbAccessionType.FaoWhoStandardsAccession, accession, acc);
						break;
					case "GlyTouCan accession":
						Add(SmallMoleculeDbAccessionType.GlyTouCan, accession, acc);
						break;
					case "PPDB accession":
						Add(SmallMoleculeDbAccessionType.PpdbAccession, accession, acc);
						break;
					case "VSDB accession":
						Add(SmallMoleculeDbAccessionType.VsdbAccession, accession, acc);
						break;
					case "BPDB accession":
						Add(SmallMoleculeDbAccessionType.BpdbAccession, accession, acc);
						break;
					case "PPR":
						Add(SmallMoleculeDbAccessionType.Ppr, accession, acc);
						break;
					default:
						throw new Exception("Unknown type: " + type);
				}
			}
		}

		private static void Add(SmallMoleculeDbAccessionType type, string accession,
			IDictionary<SmallMoleculeDbAccessionType, List<string>> dic){
			if (!dic.ContainsKey(type)){
				dic.Add(type, new List<string>());
			}
			dic[type].Add(accession);
		}

		private static void ParseNames(string folderName, IDictionary<int, ChebiEntry> map){
			StreamReader compReader = GetReader(Path.Combine(folderName, "names.tsv"));
			compReader.ReadLine();
			string line;
			while ((line = compReader.ReadLine()) != null){
				string[] w = line.Split('\t');
				int id = int.Parse(w[1]);
				if (!map.ContainsKey(id)){
					continue;
				}
				if (!string.IsNullOrEmpty(map[id].Name)){
					continue;
				}
				string name = w[4];
				if (!string.IsNullOrEmpty(name)){
					map[id].Name = name;
				}
			}
		}

		private static void ParseInchi(string folderName, IDictionary<int, ChebiEntry> map){
			StreamReader compReader = GetReader(Path.Combine(folderName, "chebiId_inchi.tsv"));
			compReader.ReadLine();
			string line;
			while ((line = compReader.ReadLine()) != null){
				string[] w = line.Split('\t');
				int id = int.Parse(w[0]);
				string inchi = w[1];
				if (!map.ContainsKey(id)){
					continue;
				}
				map[id].Inchi = inchi;
			}
		}

		private static void ParseCompounds(string folderName, Dictionary<int, ChebiEntry> map){
			StreamReader compReader = GetReader(Path.Combine(folderName, "compounds.tsv"));
			compReader.ReadLine();
			string line;
			while ((line = compReader.ReadLine()) != null){
				string[] w = line.Split('\t');
				string name = w[5];
				if (name.ToLower().Equals("null")){
					name = null;
				}
				string source = w[3];
				int compoundId = int.Parse(w[0]);
				if (!map.ContainsKey(compoundId)){
					map.Add(compoundId, new ChebiEntry());
				}
				map[compoundId].Name = name;
				map[compoundId].Source = source;
			}
			foreach (ChebiEntry e in map.Values){
				string f = e.Formula;
				if (f == null || f.Contains("Mu") || f.Equals("e") || f.Contains("[3He]") || f.Contains("[4He]") ||
				    f.Contains("[6He]") || f.Contains("[8He]") || f.Contains("[6Li]") || f.Contains("[7Li]") ||
				    f.Contains("[9Be]") || f.Contains("[10B]") || f.Contains("[11B]") || f.Contains("[10C]") ||
				    f.Contains("[11C]") || f.Contains("[12C]") || f.Contains("[14C]") || f.Contains("[13N]") ||
				    f.Contains("[15O]") || f.Contains("[16O]") || f.Contains("[17O]") || f.Contains("[18O]") ||
				    f.Contains("[19O]") || f.Contains("[18F]") || f.Contains("[19F]") || f.Contains("[23Na]") ||
				    f.Contains("[25Mg]") || f.Contains("[26Al]") || f.Contains("[27Al]") || f.Contains("[28Al]") ||
				    f.Contains("[28Si]") || f.Contains("[29Si]") || f.Contains("[30Si]") || f.Contains("[31Si]") ||
				    f.Contains("[32Si]") || f.Contains("[31P]") || f.Contains("[32P]") || f.Contains("[33P]") ||
				    f.Contains("[32S]") || f.Contains("[33S]") || f.Contains("[34S]") || f.Contains("[35S]") ||
				    f.Contains("[36S]") || f.Contains("[37S]") || f.Contains("[38S]") || f.Contains("[39K]") ||
				    f.Contains("[39Ar]") || f.Contains("[45Sc]") || f.Contains("[51Cr]") || f.Contains("[51V]") ||
				    f.Contains("[57Fe]") || f.Contains("[59Fe]") || f.Contains("[63Cu]") || f.Contains("[65Zn]") ||
				    f.Contains("[67Zn]") || f.Contains("[67Ga]") || f.Contains("[73Ge]") || f.Contains("[75Se]") ||
				    f.Contains("[77Se]") || f.Contains("[79Br]") || f.Contains("[81Kr]") || f.Contains("[87Rb]") ||
				    f.Contains("[89Y]") || f.Contains("[93Nb]") || f.Contains("[95Mo]") || f.Contains("[99Tc]") ||
				    f.Contains("[99Mo]") || f.Contains("[111Cd]") || f.Contains("[113Cd]") || f.Contains("[115Sn]") ||
				    f.Contains("[116Sn]") || f.Contains("[117Sn]") || f.Contains("[118Sn]") || f.Contains("[119Sn]") ||
				    f.Contains("[120Sn]") || f.Contains("[121Sb]") || f.Contains("[123Sb]") || f.Contains("[125Te]") ||
				    f.Contains("[123I]") || f.Contains("[125I]") || f.Contains("[127I]") || f.Contains("[129I]") ||
				    f.Contains("[131I]") || f.Contains("[129Xe]") || f.Contains("[139La]") || f.Contains("[151Eu]") ||
				    f.Contains("[183W]") || f.Contains("[197Hg]") || f.Contains("[203Hg]") || f.Contains("[197Au]") ||
				    f.Contains("[199Tl]") || f.Contains("[201Tl]") || f.Contains("[203Tl]") || f.Contains("[205Tl]") ||
				    f.Contains("[207Pb]") || f.Contains("[190Po]") || f.Contains("[191Po]") || f.Contains("[192Po]") ||
				    f.Contains("[193Po]") || f.Contains("[194Po]") || f.Contains("[195Po]") || f.Contains("[196Po]") ||
				    f.Contains("[197Po]") || f.Contains("[198Po]") || f.Contains("[199Po]") || f.Contains("[200Po]") ||
				    f.Contains("[201Po]") || f.Contains("[202Po]") || f.Contains("[203Po]") || f.Contains("[204Po]") ||
				    f.Contains("[205Po]") || f.Contains("[206Po]") || f.Contains("[207Po]") || f.Contains("[208Po]") ||
				    f.Contains("[209Po]") || f.Contains("[210Po]") || f.Contains("[211Po]") || f.Contains("[212Po]") ||
				    f.Contains("[213Po]") || f.Contains("[214Po]") || f.Contains("[215Po]") || f.Contains("[216Po]") ||
				    f.Contains("[217Po]") || f.Contains("[218Po]") || f.Contains("[219Rn]") || f.Contains("[220Rn]") ||
				    f.Contains("[222Rn]") || f.Contains("[223Ra]") || f.Contains("[224Ra]") || f.Contains("[226Ra]") ||
				    f.Contains("[228Ra]")){
					continue;
				}
				e.Formula = e.Formula.Replace("nH2O", "(H2O)n");
				if (e.Formula.Equals("CD3(CD2)12CO2H")){
					e.Formula = "HHx27C14O2";
				}
				e.Formula = ReplaceD(e.Formula);
				if (e.Formula.Equals("((C10H24N2)mod.(C3H6Br2)mod)n")){
					e.Formula = "(C13H30N2Br2)n";
				}
				if (e.Formula.Equals("((C19H21N2O3)2.Ca)2.H2O")){
					e.Formula = "(C38H42N4O6Ca)2.H2O";
				}
				if (e.Formula.Equals("(C9H17O10P).(C3H7O5P).H4O2")){
					e.Formula = "C12H28O17P2";
				}
				if (e.Formula.Equals("4(CH4).23(H2O)")){
					e.Formula = "H62C4O23";
				}
				int[] residuePos = GetResiduePos(e.Formula, out int[] residueLen, out string[] residueNames);
				int[] openingPos = GetPos(e.Formula, residuePos, residueLen, "(");
				int[] closingPos = GetClosingPos(e.Formula, out int[] closingLen, out string[] closingNames, residuePos,
					residueLen);
				if (openingPos.Length != closingPos.Length){
					throw new Exception("Different number of opening and closing elements.");
				}
				if (openingPos.Length > 1){
					for (int i = 1; i < openingPos.Length; i++){
						if (openingPos[i] < closingPos[i - 1]){
							throw new Exception("Nested braces are not supported.");
						}
					}
				}
				FindSegments(e.Formula, openingPos, closingPos, closingLen, closingNames, residuePos, residueLen,
					residueNames, out string mainFormula, out string[] mainResNames, out string[] seriesFormulas,
					out string[][] seriesResNames);
				Molecule m = GetMolecule(mainFormula, mainResNames);
				e.MonoisotopicMass = m.MonoIsotopicMass;
				e.Formula = m.GetEmpiricalFormula(false);
				e.ResNames = mainResNames;
				int ns = seriesFormulas.Length;
				e.SeriesMonoisotopicMass = new double[ns];
				e.SeriesFormula = new string[ns];
				e.SeriesResNames = new string[ns][];
				for (int i = 0; i < ns; i++){
					Molecule m1 = GetMolecule(seriesFormulas[i], seriesResNames[i]);
					e.SeriesMonoisotopicMass[i] = m1.MonoIsotopicMass;
					e.SeriesFormula[i] = m1.GetEmpiricalFormula(false);
					e.SeriesResNames[i] = seriesResNames[i];
				}
			}
		}

		private static Molecule GetMolecule(string formula, string[] resNames){
			Molecule m = new Molecule(formula);
			if (resNames.Length > 0){
				m = Molecule.Sum(m, new Molecule("H" + resNames.Length));
			}
			return m;
		}

		private static void FindSegments(string formula, int[] openingPos, int[] closingPos, int[] closingLen,
			string[] closingNames, int[] residuePos, int[] residueLen, string[] residueNames, out string mainFormula,
			out string[] mainResNames, out string[] seriesFormulas, out string[][] seriesResNames){
			FindSegments(formula, openingPos, closingPos, closingLen, closingNames, residuePos, residueLen,
				residueNames, out List<string> formulas, out List<string[]> resNames, out List<string> typeStr);
			List<int> toplevel = new List<int>();
			List<int> number = new List<int>();
			List<int> others = new List<int>();
			for (int i = 0; i < typeStr.Count; i++){
				if (formulas[i].Equals(".")){
					continue;
				}
				if (formulas[i].StartsWith(".")){
					formulas[i] = formulas[i].Substring(1);
				}
				if (formulas[i].EndsWith(".")){
					formulas[i] = formulas[i].Substring(0, formulas[i].Length - 1);
				}
				if (typeStr[i].Equals("top") || string.IsNullOrEmpty(typeStr[i])){
					toplevel.Add(i);
				} else if (Parser.TryInt(typeStr[i], out int _)){
					number.Add(i);
				} else{
					others.Add(i);
				}
			}
			mainFormula = GetMainFormula(formulas.SubArray(toplevel), formulas.SubArray(number),
				typeStr.SubArray(number));
			mainResNames = ArrayUtils.Concat(resNames.SubArray(toplevel));
			seriesFormulas = formulas.SubArray(others);
			seriesResNames = resNames.SubArray(others);
			int[][] w = GetRedundantInds(seriesFormulas);
			int[] q = new int[w.Length];
			for (int i = 0; i < q.Length; i++){
				q[i] = w[i][0];
			}
			seriesFormulas = seriesFormulas.SubArray(q);
			seriesResNames = seriesResNames.SubArray(q);
		}

		private static int[][] GetRedundantInds(string[] seriesFormulas){
			Dictionary<string, List<int>> map = new Dictionary<string, List<int>>();
			for (int i = 0; i < seriesFormulas.Length; i++){
				string f = seriesFormulas[i];
				if (!map.ContainsKey(f)){
					map.Add(f, new List<int>());
				}
				map[f].Add(i);
			}
			List<int[]> result = new List<int[]>();
			foreach (List<int> x in map.Values){
				result.Add(x.ToArray());
			}
			return result.ToArray();
		}

		private static string GetMainFormula(string[] form, string[] formNum, string[] numbers){
			if (form.Length == 1 && formNum.Length == 0){
				return form[0];
			}
			Molecule x = new Molecule("");
			foreach (string s in form){
				x = Molecule.Sum(x, new Molecule(s));
			}
			for (int i = 0; i < formNum.Length; i++){
				for (int j = 0; j < int.Parse(numbers[i]); j++){
					x = Molecule.Sum(x, new Molecule(formNum[i]));
				}
			}
			return x.GetEmpiricalFormula();
		}

		private static void FindSegments(string formula, int[] openingPos, int[] closingPos, int[] closingLen,
			string[] closingNames, int[] residuePos, int[] residueLen, string[] residueNames, out List<string> formulas,
			out List<string[]> resNames, out List<string> typeStr){
			int n = formula.Length;
			List<int> start = new List<int>();
			List<int> end = new List<int>();
			typeStr = new List<string>();
			if (openingPos.Length == 0){
				start.Add(0);
				end.Add(n - 1);
				typeStr.Add("top");
			} else{
				if (openingPos[0] > 0){
					start.Add(0);
					end.Add(openingPos[0] - 1);
					typeStr.Add("top");
				}
				for (int i = 0; i < openingPos.Length; i++){
					start.Add(openingPos[i] + 1);
					end.Add(closingPos[i] - 1);
					typeStr.Add(closingNames[i]);
				}
				if (closingPos.Last() + closingLen.Last() < n){
					start.Add(closingPos.Last() + closingLen.Last());
					end.Add(n - 1);
					typeStr.Add("top");
				}
			}
			resNames = new List<string[]>();
			formulas = new List<string>();
			for (int i = 0; i < start.Count; i++){
				int[] x = GetContainedResidues(start[i], end[i], residuePos);
				int[] resPos = residuePos.SubArray(x);
				int[] resLen = residueLen.SubArray(x);
				resNames.Add(residueNames.SubArray(x));
				string form = formula.Substring(start[i], end[i] - start[i] + 1);
				if (resPos.Length > 0){
					for (int j = 0; j < resPos.Length; j++){
						resPos[j] -= start[i];
					}
					bool[] mask = new bool[form.Length];
					for (int j = 0; j < resPos.Length; j++){
						for (int k = 0; k < resLen[j]; k++){
							mask[resPos[j] + k] = true;
						}
					}
					StringBuilder sb = new StringBuilder();
					for (int j = 0; j < mask.Length; j++){
						if (!mask[j]){
							sb.Append(form[j]);
						}
					}
					form = sb.ToString();
				}
				formulas.Add(form);
			}
		}

		private static int[] GetContainedResidues(int start, int end, int[] residuePos){
			List<int> result = new List<int>();
			for (int i = 0; i < residuePos.Length; i++){
				int pos = residuePos[i];
				if (pos >= start && pos <= end){
					result.Add(i);
				}
			}
			return result.ToArray();
		}

		private static string ReplaceD(string formula){
			int x = GetDPosition(formula);
			if (x < 0){
				return formula;
			}
			return formula.Substring(0, x) + "Hx" + formula.Substring(x + 1);
		}

		private static int GetDPosition(string formula){
			int[] x = StringUtils.AllIndicesOf(formula, "D");
			List<int> result = new List<int>();
			foreach (int i in x){
				if (i == formula.Length - 1 || formula[i + 1] < 'a' || formula[i + 1] > 'z'){
					result.Add(i);
				}
			}
			if (result.Count > 1){
				throw new Exception("Multiple D positions.");
			}
			if (result.Count == 0){
				return -1;
			}
			return x[0];
		}

		private static int[] GetPos(string formula, int[] residuePos, int[] residueLen, string w){
			int[] x = StringUtils.AllIndicesOf(formula, w);
			if (x.Length == 0){
				return new int[0];
			}
			if (residuePos.Length == 0){
				return x;
			}
			bool[] mask = GetMask(formula.Length, residuePos, residueLen);
			List<int> result = new List<int>();
			foreach (int i in x){
				if (!mask[i]){
					result.Add(i);
				}
			}
			return result.ToArray();
		}

		private static bool[] GetMask(int length, int[] residuePos, int[] residueLen){
			bool[] result = new bool[length];
			for (int i = 0; i < residuePos.Length; i++){
				for (int j = 0; j < residueLen[i]; j++){
					result[residuePos[i] + j] = true;
				}
			}
			return result;
		}

		private static int[] GetResiduePos(string formula, out int[] residueLen, out string[] residueNames){
			List<int> lens = new List<int>();
			List<int> pos = new List<int>();
			List<string> names = new List<string>();
			string[] w1 = {"(R1)", "(R2)", "(R3)", "R1", "R2", "R3", "R", "X"};
			bool[] mask = new bool[formula.Length];
			foreach (string w in w1){
				foreach (int i in StringUtils.AllIndicesOf(formula, w)){
					if (mask[i]){
						continue;
					}
					if ((w.Equals("R") || w.Equals("X")) && i < formula.Length - 1){
						if (formula[i + 1] >= 'a' && formula[i + 1] <= 'z'){
							continue;
						}
					}
					pos.Add(i);
					lens.Add(w.Length);
					names.Add(w);
					for (int j = 0; j < w.Length; j++){
						mask[i + j] = true;
					}
				}
			}
			residueLen = lens.ToArray();
			residueNames = names.ToArray();
			return pos.ToArray();
		}

		private static int[] GetClosingPos(string formula, out int[] closingLen, out string[] closingNames,
			int[] residuePos, int[] residueLen){
			int[] x = GetPos(formula, residuePos, residueLen, ")");
			List<int> lens = new List<int>();
			List<int> pos = new List<int>();
			List<string> names = new List<string>();
			foreach (int i in x){
				if (i == formula.Length - 1){
					pos.Add(i);
					lens.Add(1);
					names.Add("");
				} else if (formula[i + 1] == 'n'){
					pos.Add(i);
					lens.Add(2);
					names.Add("n");
				} else if (formula[i + 1] == '('){
					pos.Add(i);
					lens.Add(1);
					names.Add("");
				} else if (formula.Substring(i + 1).StartsWith("mon")){
					pos.Add(i);
					lens.Add(4);
					names.Add("mon");
				} else if (formula[i + 1] == 'x'){
					pos.Add(i);
					lens.Add(2);
					names.Add("x");
				} else if (formula[i + 1] == 'y'){
					pos.Add(i);
					lens.Add(2);
					names.Add("y");
				} else if (formula[i + 1] == 'k'){
					pos.Add(i);
					lens.Add(2);
					names.Add("k");
				} else if (formula[i + 1] >= '1' && formula[i + 1] <= '9'){
					int lastPos = i + 1;
					while (lastPos + 1 < formula.Length &&
					       ((formula[lastPos + 1] >= '0' && formula[lastPos + 1] <= '9') ||
					        formula[lastPos + 1] == '-')){
						lastPos++;
					}
					pos.Add(i);
					lens.Add(1 + lastPos - i);
					names.Add(formula.Substring(i + 1, lastPos - i));
				} else if (formula.Substring(i + 1).StartsWith("m.") || formula.Substring(i + 1).StartsWith("m(")){
					pos.Add(i);
					lens.Add(2);
					names.Add("m");
				} else if (formula.Substring(i + 1).StartsWith("l.")){
					pos.Add(i);
					lens.Add(2);
					names.Add("l");
				} else if (formula.Substring(i + 1).StartsWith("ran.")){
					pos.Add(i);
					lens.Add(4);
					names.Add("ran");
				} else if (formula.Substring(i + 1).EndsWith("ran")){
					pos.Add(i);
					lens.Add(4);
					names.Add("ran");
				} else if (formula.Substring(i + 1).StartsWith(".")){
					pos.Add(i);
					lens.Add(1);
					names.Add("");
				} else if (formula.Substring(i + 1).StartsWith("p.")){
					pos.Add(i);
					lens.Add(2);
					names.Add("p");
				} else if (formula.Substring(i + 1).StartsWith("q.")){
					pos.Add(i);
					lens.Add(2);
					names.Add("q");
				} else if (formula.Substring(i + 1).StartsWith("kC")){
					pos.Add(i);
					lens.Add(2);
					names.Add("k");
				} else if (formula.Substring(i + 1)[1] == '-'){
					string s = formula.Substring(i + 1);
					s = s.Substring(0, s.IndexOf('.') + 1);
					pos.Add(i);
					lens.Add(s.Length);
					names.Add(s);
				} else{
					throw new Exception("Unrecognized closing element.");
				}
			}
			closingLen = lens.ToArray();
			closingNames = names.ToArray();
			return pos.ToArray();
		}

		private static void ParseChemicalData(string folderName, IDictionary<int, ChebiEntry> map){
			StreamReader chemReader = GetReader(Path.Combine(folderName, "chemical_data.tsv"));
			chemReader.ReadLine();
			string line;
			while ((line = chemReader.ReadLine()) != null){
				string[] w = line.Split('\t');
				string type = w[3];
				int compoundId = int.Parse(w[1]);
				string content = w[4];
				if (!map.ContainsKey(compoundId)){
					map.Add(compoundId, new ChebiEntry());
				}
				switch (type){
					case "FORMULA":
						if (!Parser.TryDouble(content, out _)){
							map[compoundId].Formula = content;
						}
						break;
					case "MASS":
						map[compoundId].Mass = Parser.Double(content);
						break;
					case "CHARGE":
						map[compoundId].Charge = int.Parse(content);
						break;
					case "MONOISOTOPIC MASS":
						break;
					default:
						throw new Exception("Unknown type: " + type);
				}
			}
		}
	}
}