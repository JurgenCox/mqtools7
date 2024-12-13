using System.IO.Compression;
using System.Xml;
using MqApi.Util;

namespace MqUtil.Parse.Sbml{
	public class SbmlParser{
		private readonly string filename;
		private SbmlModel model;
		private int sbmlLevel;
		private int sbmlVersion;
		private SbmlSection section = SbmlSection.None;
		private SbmlNotes currentNotes;
		private bool inNotes;
		private bool inAnnotation;
		private SbmlSpeciesAnnotationType speciesAnnotationType = SbmlSpeciesAnnotationType.None;
		private bool inListOfReactants;
		private bool inMath;

		public SbmlParser(string filename){
			this.filename = filename;
		}

		public SbmlModel Parse(){
			if (filename.EndsWith("MODEL0912180000.xml")){
				return null;
			}
			Stream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
			Stream stream = filename.ToLower().EndsWith(".gz")
				? new GZipStream(fileStream, CompressionMode.Decompress)
				: fileStream;
			XmlTextReader reader = new XmlTextReader(new StreamReader(stream));
			while (reader.Read()){
				if (reader.IsEmptyElement && !reader.HasAttributes){
					continue;
				}
				switch (reader.NodeType){
					case XmlNodeType.Element:
						string name = reader.Name;
						int depth = reader.Depth;
						Dictionary<string, string> attributes = new Dictionary<string, string>();
						if (reader.HasAttributes){
							for (int i = 0; i < reader.AttributeCount; i++){
								reader.MoveToAttribute(i);
								attributes.Add(reader.Name, reader.Value);
							}
						}
						StartElement(name, attributes, depth);
						break;
					case XmlNodeType.EndElement:
						EndElement(reader.Name, reader.Depth);
						break;
					case XmlNodeType.Text:
						Characters(reader.Value);
						break;
				}
			}
			reader.Close();
			model.SbmlLevel = sbmlLevel;
			model.SbmlVersion = sbmlVersion;
			PostProcess();
			return model;
		}

		private static readonly string[] otherIdsInNotes ={
			"HEPATONET_1.0_ABBREVIATION:", "EHMN_ABBREVIATION:",
			"DRUGBANK_INDUCER:", "DRUGBANK_SUBSTRATE:", "DRUGBANK_UNKNOWN:", "DRUGBANK_ACTIVATOR:", "DRUGBANK_INHIBITOR:",
			"DRUGBANK_AGONIST:", "DRUGBANK_COFACTOR:", "DRUGBANK_PRODUCT_OF:", "DRUGBANK_OTHER_UNKNOWN:", "DRUGBANK_POTENTIATOR:",
			"DRUGBANK_LIGAND:", "DRUGBANK_OTHER:", "DRUGBANK_ANTAGONIST:", "DRUGBANK_BINDER:", "DRUGBANK_CONVERSION_INHIBITOR:",
			"DRUGBANK_POSITIVE_ALLOSTERIC_MODULATOR:", "BIGG:", "BIOPATH:", "BRENDA:", "CHEBI:", "KEGG:", "METACYC:", "MXNREF:",
			"SEED:", "SMILES:", "UPA:", "REACTOME:", "HMDB:", "LIPIDMAPS:"
		};

		private void PostProcess(){
			if (model.Species == null){
				model.Species = new List<SbmlSpecies>();
			}
			if (model.Reactions == null){
				model.Reactions = new List<SbmlReaction>();
			}
			foreach (SbmlSpecies species in model.Species){
				PostProcess(species);
			}
			model.Uniprot2SpeciesInd = CalcUniprot2SpeciesInd();
			model.Id2SpeciesInd = CalcId2SpeciesInd();
		}

		private Dictionary<string, int> CalcId2SpeciesInd(){
			Dictionary<string, int> result = new Dictionary<string, int>();
			for (int i = 0; i < model.Species.Count; i++){
				SbmlSpecies species = model.Species[i];
				result.Add(species.Id, i);
			}
			return result;
		}

		private Dictionary<string, List<int>> CalcUniprot2SpeciesInd(){
			Dictionary<string, List<int>> result = new Dictionary<string, List<int>>();
			for (int i = 0; i < model.Species.Count; i++){
				SbmlSpecies species = model.Species[i];
				foreach (string s in species.Uniprot){
					if (!result.ContainsKey(s)){
						result.Add(s, new List<int>());
					}
					result[s].Add(i);
				}
			}
			return result;
		}

		private static void PostProcess(SbmlSpecies species){
			if (species.Notes != null){
				foreach (string s in species.Notes.Text.ToString().Split(new[]{'\n'}, StringSplitOptions.RemoveEmptyEntries)){
					string q = s.Trim();
					if (string.IsNullOrEmpty(q)){
						continue;
					}
					if (q.StartsWith("FORMULA:")){
						species.Formula = q.Substring(8).Trim();
						continue;
					}
					if (q.StartsWith("CHARGE:")){
						//already set
						continue;
					}
					if (q.StartsWith("INCHI:")){
						species.Inchi = q.Substring(6).Trim();
						continue;
					}
					species.MiscIds = new Dictionary<string, string>();
					if (ChechForMiscIds(species.MiscIds, q)){
						continue;
					}
					if (q.Equals("biliganded basal state")){
						continue;
					}
					if (q.Equals("monoliganded intermediate")){
						continue;
					}
					if (q.Equals("monoliganded active state")){
						continue;
					}
					if (q.Equals("unkiganded active state")){
						continue;
					}
					if (q.Equals("unliganded basal state")){
						continue;
					}
					if (q.Equals("monoliganded basal state")){
						continue;
					}
					if (q.Equals("biliganded desensitised state")){
						continue;
					}
					if (q.Equals("fully desensitised state")){
						continue;
					}
					if (q.Equals("biliganded intermediate")){
						continue;
					}
					if (q.Equals("monoliganded desensitised state")){
						continue;
					}
					if (q.Equals("unliganted intermediate")){
						continue;
					}
					if (q.Equals("biliganted active state")){
						continue;
					}
					if (q.Equals("ligand")){
						continue;
					}
					if (q.Equals("lacI inhibitor")){
						continue;
					}
					//TODO
					continue;
					throw new Exception("Unrecognized string.");
				}
			}
			foreach (string s in species.Is){
                //forward slashes are for the sbmls with URLs
			    //colons are for the sbml with URNs
                if (s.Contains("/chebi/")){
					species.Chebi.Add(s.Substring(s.LastIndexOf('/') + 1));
					continue;
				}
			    if (s.Contains(":chebi:"))
			    {
			        species.Chebi.Add(s.Substring(s.LastIndexOf(':') + 1));
                    continue;
			    }
                if (s.Contains("/kegg.compound/")){
					species.KeggCompound.Add(s.Substring(s.LastIndexOf('/') + 1));
					continue;
				}
				if (s.Contains("/hmdb/")){
					species.Hmdb.Add(s.Substring(s.LastIndexOf('/') + 1));
					continue;
				}
				if (s.Contains("/pubchem.substance/")){
					species.PubchemSubstance.Add(s.Substring(s.LastIndexOf('/') + 1));
					continue;
				}
				if (s.Contains("/kegg.drug/")){
					species.KeggDrug.Add(s.Substring(s.LastIndexOf('/') + 1));
					continue;
				}
                if (s.Contains("/uniprot/")){
					species.Uniprot.Add(s.Substring(s.LastIndexOf('/') + 1));
					continue;
				}
			    if (s.Contains(":uniprot:"))
			    {
			        species.Uniprot.Add(s.Substring(s.LastIndexOf(':') + 1));
			        continue;
			    }
                if (s.Contains("/interpro/")){
					//TODO
					continue;
				}
				if (s.Contains("/obo.chebi/")){
					//TODO
					continue;
				}
				if (s.Contains("/bind/")){
					//TODO
					continue;
				}
				if (s.Contains("/go/")){
					//TODO
					continue;
				}
				if (s.Contains("/obo.go/")){
					//TODO
					continue;
				}
				if (s.Contains("/pato/")){
					//TODO
					continue;
				}
				if (s.Contains("/pirsf/")){
					//TODO
					continue;
				}
				if (s.Contains("/cas/")){
					//TODO
					continue;
				}
				if (s.Contains("/3dmet/")){
					//TODO
					continue;
				}
				if (s.Contains("/kegg.genes/")){
					//TODO
					continue;
				}
				if (s.Contains(":kegg.genes:")){
					//TODO
					continue;
				}
				if (s.Contains("/reactome/")){
					//TODO
					continue;
				}
				if (s.Contains(":reactome:")){
					//TODO
					continue;
				}
				if (s.Contains("/pubchem.compound/")){
					//TODO
					continue;
				}
				if (s.Contains("/chembl.compound/")){
					//TODO
					continue;
				}
				if (s.Contains("/fma/")){
					//TODO
					continue;
				}
				if (s.Contains("/ec-code/")){
					//TODO
					continue;
				}
				if (s.Contains("/tcdb/")){
					//TODO
					continue;
				}
				if (s.Contains("/cco/")){
					//TODO
					continue;
				}
				if (s.Contains("/omim/")){
					//TODO
					continue;
				}
				if (s.Contains("/lipidmaps/")){
					//TODO
					continue;
				}
				if (s.Contains(":obo.chebi:")){
					//TODO
					continue;
				}
				if (s.Contains(":kegg.compound:")){
					//TODO
					continue;
				}
				if (s.Contains("/taxonomy/")){
					//TODO
					continue;
				}
				if (s.Contains(":ensembl:")){
					//TODO
					continue;
				}
				if (s.Contains("/ensembl/")){
					//TODO
					continue;
				}
				if (s.Contains(":mirbase:")){
					//TODO
					continue;
				}
				if (s.Contains("/mirbase/")){
					//TODO
					continue;
				}
				if (s.Contains(":biomodels.sbo:")){
					//TODO
					continue;
				}
				if (s.Contains("/biomodels.sbo/")){
					//TODO
					continue;
				}
				if (s.Contains("/bto/")){
					//TODO
					continue;
				}
				if (s.Contains("/so/")){
					//TODO
					continue;
				}
				if (s.Contains("/pr/")){
					//TODO
					continue;
				}
				if (s.Contains("/sgd/")){
					//TODO
					continue;
				}
				if (s.Contains("/kegg.pathway/")){
					//TODO
					continue;
				}
				if (s.Contains(":kegg.pathway:")){
					//TODO
					continue;
				}
				if (s.Contains(":pr:")){
					//TODO
					continue;
				}
				if (s.Contains(":sgd:")){
					//TODO
					continue;
				}
				if (s.Contains("/biocyc/")){
					//TODO
					continue;
				}
				if (s.Contains(":biocyc:")){
					//TODO
					continue;
				}
				if (s.Contains("/ncbigene/")){
					//TODO
					continue;
				}
				if (s.Contains("/hgnc/")){
					//TODO
					continue;
				}
				if (s.Contains("/pdb-ccd/")){
					//TODO
					continue;
				}
				if (s.Contains("/kegg.glycan/")){
					//TODO
					continue;
				}
				if (s.Contains(":miriam:")){
					//TODO
					continue;
				}
				if (s.StartsWith("#metaid")){
					//TODO
					continue;
				}
				if (s.Contains("bigg.ucsd.edu")){
					//TODO
					continue;
				}
				if (s.Contains("/acinetocyc/"))
				{
					//TODO
					continue;
				}
				if (s.Contains("/refseq/"))
				{
					//TODO
					continue;
				}
				if (s.Contains("/ensembl.fungi/"))
				{
					//TODO
					continue;
				}
				if (s.Contains("/intact/"))
				{
					//TODO
					continue;
				}
				throw new Exception("Unrecognized string.");
			}
			foreach (string s in species.IsEncodedBy){
				if (s.Contains("/kegg.genes/")){
					species.KeggGenes.Add(s.Substring(s.LastIndexOf('/') + 1));
					continue;
				}
				if (s.Contains("/ensembl/")){
					//TODO
					continue;
				}
				if (s.Contains(":ensembl:")){
					//TODO
					continue;
				}
				if (s.Contains("/ncbigene/")){
					//TODO
					continue;
				}
				if (s.Contains("/ncbiprotein/")){
					//TODO
					continue;
				}
				if (s.Contains("/sgd/")){
					//TODO
					continue;
				}
				if (s.Contains("/pubchem.compound/")){
					//TODO
					continue;
				}
				if (s.Contains("/chebi/")){
					//TODO
					continue;
				}
				if (s.Contains("/ec-code/")){
					//TODO
					continue;
				}
			    if (s.Contains(":miriam:"))
			    {
			        //TODO
			        continue;
			    }
			    Console.WriteLine(s);
				throw new Exception("Unrecognized string.");
			}
			foreach (string s in species.HasPart){
				if (s.Contains("/uniprot/")){
					species.UniprotPart.Add(s.Substring(s.LastIndexOf('/') + 1));
					continue;
				}
			    if (s.Contains(":uniprot:"))
			    {
			        species.UniprotPart.Add(s.Substring(s.LastIndexOf(':') + 1));
			        continue;
			    }
			    if (s.Contains("/chebi/"))
			    {
			        //TODO
			        continue;
			    }
                if (s.Contains(":chebi:"))
			    {
			        //TODO
			        continue;
			    }
                if (s.Contains("/interpro/")){
					//TODO
					continue;
				}

				if (s.Contains("/kegg.compound/")){
					//TODO
					continue;
				}
				if (s.Contains("/obo.chebi/")){
					//TODO
					continue;
				}
				if (s.Contains("/go/")){
					//TODO
					continue;
				}
				if (s.Contains("/obo.go/")){
					//TODO
					continue;
				}
				if (s.Contains("/pirsf/")){
					//TODO
					continue;
				}
				if (s.Contains("/ncbigene/")){
					//TODO
					continue;
				}
				if (s.Contains("/psimod/")){
					//TODO
					continue;
				}
				if (s.Contains("/kegg.orthology/")){
					//TODO
					continue;
				}
				if (s.Contains("/obo.psi-mod/")){
					//TODO
					continue;
				}
				if (s.Contains("/pr/")){
					//TODO
					continue;
				}
				if (s.Contains("/pubchem.compound/")){
					//TODO
					continue;
				}
				if (s.Contains("/psimi/")){
					//TODO
					continue;
				}
				if (s.Contains("/cco/")){
					//TODO
					continue;
				}
				if (s.Contains("/kegg.genes/")){
					//TODO
					continue;
				}
				if (s.Contains("/chembl.compound/")){
					//TODO
					continue;
				}
				if (s.Contains("/ec-code/")){
					//TODO
					continue;
				}
				if (s.Contains("/insdc/")){
					//TODO
					continue;
				}
				if (s.Contains("/biomodels.sbo/")){
					//TODO
					continue;
				}
				if (s.Contains("/ensembl/")){
					//TODO
					continue;
				}
				if (s.Contains(":ensembl:")){
					//TODO
					continue;
				}
				if (s.Contains("/taxonomy/")){
					//TODO
					continue;
				}
				if (s.Contains("/cl/")){
					//TODO
					continue;
				}
				if (s.Contains("/fma/")){
					//TODO
					continue;
				}
				if (s.Contains("/so/")){
					//TODO
					continue;
				}
				if (s.Contains(":mirbase:")){
					//TODO
					continue;
				}
				if (s.Contains("/mirbase/")){
					//TODO
					continue;
				}
				if (s.Contains("/hmdb/")){
					//TODO
					continue;
				}
			    
				if (s.Contains(":go:")){
					//TODO
					continue;
				}
				continue;
				throw new Exception("Unrecognized string.");
			}
		}

		private static bool ChechForMiscIds(Dictionary<string, string> miscIds, string q){
			foreach (string s in otherIdsInNotes){
				if (q.StartsWith(s)){
					miscIds.Add(s, q.Substring(s.Length).Trim());
					return true;
				}
			}
			return false;
		}

		private void StartElementInNotes(string qName){
			switch (qName){
				case "body":
					return;
				case "div":
					return;
				case "p":
					return;
				case "a":
					return;
				case "i":
					return;
				case "br":
					return;
				case "em":
					return;
				case "h1":
					return;
				case "table":
					return;
				case "thead":
					return;
				case "tr":
					return;
				case "th":
					return;
				case "tbody":
					return;
				case "td":
					return;
				case "b":
					return;
				case "ul":
					return;
				case "li":
					return;
				case "sup":
					return;
				case "sub":
					return;
				case "strong":
					return;
				case "font":
					return;
				case "h3":
					return;
				case "ol":
					return;
				case "u":
					return;
				case "html":
					return;
				case "head":
					return;
				case "meta":
					return;
				case "span":
					return;
				case "hr":
					return;
				case "q":
					return;
				case "e":
					return;
				case "title":
					return;
				case "caption":
					return;
				case "small":
					return;
				case "pre":
					return;
				case "notes":
					return;
				case "code":
					return;
				case "html:p":
					return;
				case "html:body":
					return;
				case "tt":
					return;
				case "h2":
					return;
				case "xhtml:body":
					return;
				case "xhtml:p":
					return;
				case "html:span":
					return;
				default:
					throw new Exception("Unknown qName: " + qName);
			}
		}

		private void StartElementInMath(string qName){
			switch (qName){
				case "plus":
					return;
				case "minus":
					return;
				case "times":
					return;
				case "divide":
					return;
				case "power":
					return;
				case "apply":
					return;
				case "ci":
					return;
				case "gt":
					return;
				case "csymbol":
					return;
				case "cn":
					return;
				case "geq":
					return;
				case "leq":
					return;
				case "ln":
					return;
				case "piecewise":
					return;
				case "piece":
					return;
				case "lt":
					return;
				case "otherwise":
					return;
				case "sep":
					return;
				case "exp":
					return;
				case "lambda":
					return;
				case "bvar":
					return;
				case "ceiling":
					return;
				case "sin":
					return;
				case "pi":
					return;
				case "root":
					return;
				case "degree":
					return;
				case "and":
					return;
				case "eq":
					return;
				case "or":
					return;
				case "tanh":
					return;
				case "floor":
					return;
				case "cosh":
					return;
				case "abs":
					return;
				case "xor":
					return;
				case "log":
					return;
				case "logbase":
					return;
				default:
					throw new Exception("Unknown qName: " + qName);
			}
		}

		private void EndElementInMath(string qName){
			switch (qName){
				case "apply":
					return;
				case "times":
					return;
				case "ci":
					return;
				case "minus":
					return;
				case "gt":
					return;
				case "csymbol":
					return;
				case "cn":
					return;
				case "power":
					return;
				case "plus":
					return;
				case "divide":
					return;
				case "geq":
					return;
				case "leq":
					return;
				case "ln":
					return;
				case "piecewise":
					return;
				case "piece":
					return;
				case "lt":
					return;
				case "otherwise":
					return;
				case "sep":
					return;
				case "exp":
					return;
				case "lambda":
					return;
				case "bvar":
					return;
				case "ceiling":
					return;
				case "sin":
					return;
				case "pi":
					return;
				case "root":
					return;
				case "degree":
					return;
				case "and":
					return;
				case "eq":
					return;
				case "or":
					return;
				case "tanh":
					return;
				case "floor":
					return;
				case "cosh":
					return;
				case "abs":
					return;
				case "xor":
					return;
				case "log":
					return;
				case "logbase":
					return;
				default:
					throw new Exception("Unknown qName: " + qName);
			}
		}

		bool inListOfKeyValueData;

		private void StartElementInAnnotation(string qName, IDictionary<string, string> attrs){
			if (inListOfKeyValueData){
				//TODO
				return;
			}
			if (qName.Equals("listOfKeyValueData")){
				inListOfKeyValueData = true;
				return;
			}
			switch (section){
				case SbmlSection.None:
					switch (qName){
						case "rdf:RDF":
							return;
						case "rdf:Description":
							return;
						case "dc:creator":
							return;
						case "rdf:Bag":
							return;
						case "rdf:li":
							return;
						case "vCard:N":
							return;
						case "vCard:Family":
							return;
						case "vCard:Given":
							return;
						case "vCard:EMAIL":
							return;
						case "vCard:ORG":
							return;
						case "vCard:Orgname":
							return;
						case "dcterms:created":
							return;
						case "dcterms:W3CDTF":
							return;
						case "dcterms:modified":
							return;
						case "bqbiol:occursIn":
							return;
						case "bqmodel:is":
							return;
						case "bqmodel:isDerivedFrom":
							return;
						case "bqbiol:isDescribedBy":
							return;
						case "bqmodel:isDescribedBy":
							return;
						case "bqbiol:isVersionOf":
							return;
						case "bqbiol:hasTaxon":
							return;
						case "bqbiol:isHomologTo":
							return;
						case "bqbiol:hasVersion":
							return;
						case "bqbiol:is":
							return;
						case "bqbiol:isPartOf":
							return;
						case "bqbiol:hasProperty":
							return;
						case "bqbiol:hasPart":
							return;
						case "jd:header":
							return;
						case "jd:VersionHeader":
							return;
						case "jd:ModelHeader":
							return;
						case "jd:display":
							return;
						case "jd:SBMLGraphicsHeader":
							return;
						case "jd:notes":
							return;
						case "jd:note":
							return;
						case "doqcs:timestamp":
							return;
						case "doqcs:accesstype":
							return;
						case "doqcs:transcriber":
							return;
						case "doqcs:developer":
							return;
						case "doqcs:species":
							return;
						case "doqcs:tissue":
							return;
						case "doqcs:cellcompartment":
							return;
						case "doqcs:methodology":
							return;
						case "doqcs:model_implementation":
							return;
						case "doqcs:model_validation":
							return;
						case "VCellInfo":
							return;
						case "BioModel":
							return;
						case "SimulationSpec":
							return;
						case "Simulation":
							return;
						case "celldesigner:modelVersion":
							return;
						case "celldesigner:modelDisplay":
							return;
						case "celldesigner:listOfIncludedSpecies":
							return;
						case "celldesigner:species":
							return;
						case "celldesigner:notes":
							return;
						case "html":
							return;
						case "body":
							return;
						case "celldesigner:annotation":
							return;
						case "celldesigner:complexSpecies":
							return;
						case "celldesigner:speciesIdentity":
							return;
						case "celldesigner:class":
							return;
						case "celldesigner:proteinReference":
							return;
						case "celldesigner:name":
							return;
						case "celldesigner:listOfCompartmentAliases":
							return;
						case "celldesigner:compartmentAlias":
							return;
						case "celldesigner:bounds":
							return;
						case "celldesigner:namePoint":
							return;
						case "celldesigner:doubleLine":
							return;
						case "celldesigner:paint":
							return;
						case "celldesigner:listOfComplexSpeciesAliases":
							return;
						case "celldesigner:complexSpeciesAlias":
							return;
						case "celldesigner:activity":
							return;
						case "celldesigner:view":
							return;
						case "celldesigner:backupSize":
							return;
						case "celldesigner:backupView":
							return;
						case "celldesigner:usualView":
							return;
						case "celldesigner:innerPosition":
							return;
						case "celldesigner:boxSize":
							return;
						case "celldesigner:singleLine":
							return;
						case "celldesigner:briefView":
							return;
						case "celldesigner:listOfSpeciesAliases":
							return;
						case "celldesigner:speciesAlias":
							return;
						case "celldesigner:listOfGroups":
							return;
						case "celldesigner:listOfProteins":
							return;
						case "celldesigner:protein":
							return;
						case "celldesigner:listOfGenes":
							return;
						case "celldesigner:listOfRNAs":
							return;
						case "celldesigner:listOfAntisenseRNAs":
							return;
						case "celldesigner:listOfLayers":
							return;
						case "celldesigner:listOfBlockDiagrams":
							return;
						case "celldesigner:point":
							return;
						case "celldesigner:gene":
							return;
						case "celldesigner:RNA":
							return;
						case "celldesigner:geneReference":
							return;
						case "COPASI":
							return;
						case "CopasiMT:isPartOf":
							return;
						case "celldesigner:extension":
							return;
						case "celldesigner:font":
							return;
						case "celldesigner:info":
							return;
						case "dcterms:creator":
							return;
						case "jd2:JDesignerLayout":
							return;
						case "jd2:header":
							return;
						case "jd2:VersionHeader":
							return;
						case "jd2:ModelHeader":
							return;
						case "jd2:TimeCourseDetails":
							return;
						case "jd2:JDGraphicsHeader":
							return;
						case "jd2:listOfCompartments":
							return;
						case "jd2:compartment":
							return;
						case "jd2:boundingBox":
							return;
						case "jd2:membraneStyle":
							return;
						case "jd2:interiorStyle":
							return;
						case "jd2:text":
							return;
						case "jd2:position":
							return;
						case "jd2:font":
							return;
						case "jd2:listOfSpecies":
							return;
						case "jd2:species":
							return;
						case "jd2:complex":
							return;
						case "jd2:subunit":
							return;
						case "jd2:color":
							return;
						case "jd2:edgeStyle":
							return;
						case "jd2:listOfReactions":
							return;
						case "jd2:reaction":
							return;
						case "jd2:listOfReactants":
							return;
						case "jd2:speciesReference":
							return;
						case "jd2:listOfProducts":
							return;
						case "jd2:listOfModifierEdges":
							return;
						case "jd2:kineticLaw":
							return;
						case "jd2:rateEquation":
							return;
						case "jd2:listOfSymbols":
							return;
						case "jd2:parameter":
							return;
						case "jd2:display":
							return;
						case "jd2:lineType":
							return;
						case "jd2:edge":
							return;
						case "jd2:pt":
							return;
						case "jd2:modifierEdge":
							return;
						case "jd2:destinationReaction":
							return;
						case "bqbiol:encodes":
							return;
						case "listoflayouts":
							return;
						case "layout":
							return;
						case "container":
							return;
						case "clone":
							return;
						case "neighbour":
							return;
						case "celldesigner:state":
							return;
						case "celldesigner:homodimer":
							return;
						case "celldesigner:listOfModifications":
							return;
						case "celldesigner:modification":
							return;
						case "celldesigner:listOfModificationResidues":
							return;
						case "celldesigner:modificationResidue":
							return;
						case "dcterms:bibliographicCitation":
							return;
						case "CopasiMT:isDescribedBy":
							return;
						case "celldesigner:blockDiagram":
							return;
						case "celldesigner:canvas":
							return;
						case "celldesigner:block":
							return;
						case "celldesigner:halo":
							return;
						case "celldesigner:listOfResiduesInBlockDiagram":
							return;
						case "celldesigner:listOfExternalNamesForResidue":
							return;
						case "celldesigner:listOfBindingSitesInBlockDiagram":
							return;
						case "celldesigner:listOfEffectSitesInBlockDiagram":
							return;
						case "celldesigner:effectSiteInBlockDiagram":
							return;
						case "celldesigner:effectInBlockDiagram":
							return;
						case "celldesigner:effectTargetInBlockDiagram":
							return;
						case "celldesigner:listOfInternalOperatorsInBlockDiagram":
							return;
						case "celldesigner:listOfInternalLinksInBlockDiagram":
							return;
						case "pathwaydocument":
							return;
						case "pathway":
							return;
						case "participantlist":
							return;
						case "participant":
							return;
						case "smallmolecule":
							return;
						case "compartment":
							return;
						case "synonymlist":
							return;
						case "synonym":
							return;
						case "datasource":
							return;
						case "database":
							return;
						case "databaselist":
							return;
						case "interactionlist":
							return;
						case "interaction":
							return;
						case "participantuselist":
							return;
						case "participantuse":
							return;
						case "presentationlist":
							return;
						case "presentation":
							return;
						case "participantstatelist":
							return;
						case "participantstate":
							return;
						case "interactionstatelist":
							return;
						case "interactionstate":
							return;
						case "experimentlist":
							return;
						case "experiment":
							return;
						case "celldesigner:structuralState":
							return;
						case "celldesigner:layer":
							return;
						case "CopasiMT:is":
							return;
						case "CopasiMT:isVersionOf":
							return;
						case "CopasiMT:occursIn":
							return;
						case "dcterms:description":
							return;
						case "CopasiMT:encodes":
							return;
						case "plasmo:dbinfo":
							return;
						case "jd2:listOfParameterSets":
							return;
						case "jd2:parameterSet":
							return;
						case "jd2:notes":
							return;
						case "jd2:NameValue":
							return;
						case "jd2:listOfParameters":
							return;
						case "jd2:otherDisplayObjects":
							return;
						case "jd2:textObject":
							return;
						case "jd2:squareObject":
							return;
						case "jd2:shapeProperties":
							return;
						case "jd2:circleObject":
							return;
						case "gxl":
							return;
						case "graph":
							return;
						case "node":
							return;
						case "parameter":
							return;
						case "value":
							return;
						case "exp":
							return;
						case "nodevisualsetting":
							return;
						case "rect":
							return;
						case "ellipse":
							return;
						case "celldesigner:rnaReference":
							return;
						case "celldesigner:listOfTexts":
							return;
						case "celldesigner:layerSpeciesAlias":
							return;
						case "celldesigner:layerNotes":
							return;
						case "vcell:VCellInfo":
							return;
						case "vcell:VCMLSpecific":
							return;
						case "vcell:BioModel":
							return;
						case "vcell:SimulationSpec":
							return;
						case "VersionHeader":
							return;
						case "ModelHeader":
							return;
						case "SBMLGraphicsHeader":
							return;
						case "note":
							return;
						case "celldesigner:listOfInnerAliases":
							return;
						case "celldesigner:innerAlias":
							return;
						case "celldesigner:group":
							return;
						case "jd2:edgeCenter":
							return;
						case "flux:listOfAnalysisTypes":
							return;
						case "flux:analysisType":
							return;
						case "flux:listOfAnalyses":
							return;
						case "flux:analysis":
							return;
						default:
							throw new Exception("Unknown qName: " + qName);
					}
				case SbmlSection.Toplevel:
					switch (qName){
						case "mathsbml:AuthorConfiguration":
							return;
						case "mathsbml:date":
							return;
						case "mathsbml:MachineDomain":
							return;
						case "mathsbml:MachineID":
							return;
						case "mathsbml:MachineName":
							return;
						case "mathsbml:MachineType":
							return;
						case "mathsbml:MathematicaVersion":
							return;
						case "mathsbml:OperatingSystem":
							return;
						case "mathsbml:ProcessorType":
							return;
						case "mathsbml:ProductInformation":
							return;
						case "mathsbml:SoftwareVersion":
							return;
						case "mathsbml:System":
							return;
						case "mathsbml:UserName":
							return;
						case "rdf:RDF":
							return;
						case "rdf:Description":
							return;
						case "dc:creator":
							return;
						case "rdf:Bag":
							return;
						case "rdf:li":
							return;
						case "vCard:N":
							return;
						case "vCard:Family":
							return;
						case "vCard:Given":
							return;
						case "vCard:EMAIL":
							return;
						case "vCard:ORG":
							return;
						case "vCard:Orgname":
							return;
						case "dcterms:created":
							return;
						case "dcterms:W3CDTF":
							return;
						case "dcterms:modified":
							return;
						case "bqmodel:is":
							return;
						case "bqmodel:isDescribedBy":
							return;
						case "bqmodel:isDerivedFrom":
							return;
						case "bqbiol:isVersionOf":
							return;
						case "bqbiol:hasTaxon":
							return;
						case "bqbiol:isHomologTo":
							return;
						case "jd:header":
							return;
						case "jd:VersionHeader":
							return;
						case "jd:ModelHeader":
							return;
						case "jd:display":
							return;
						case "jd:SBMLGraphicsHeader":
							return;
						case "jd:notes":
							return;
						case "jd:note":
							return;
						case "SimBiology":
							return;
						case "Version":
							return;
						case "bqbiol:hasProperty":
							return;
						case "bqbiol:occursIn":
							return;
						case "jigcell:blankline":
							return;
						default:
							throw new Exception("Unknown qName: " + qName);
					}
				case SbmlSection.ListOfCompartments:
					switch (qName){
						case "rdf:RDF":
							return;
						case "rdf:Description":
							return;
						case "bqbiol:is":
							return;
						case "rdf:Bag":
							return;
						case "rdf:li":
							return;
						case "jd:display":
							return;
						case "jd:boundingBox":
							return;
						case "bqbiol:isVersionOf":
							return;
						case "bqmodel:is":
							return;
						case "celldesigner:name":
							return;
						case "bqbiol:isPartOf":
							return;
						case "bqbiol:hasPart":
							return;
						case "bqbiol:occursIn":
							return;
						case "COPASI":
							return;
						case "dcterms:created":
							return;
						case "dcterms:W3CDTF":
							return;
						case "celldesigner:extension":
							return;
						case "CopasiMT:isVersionOf":
							return;
						case "CopasiMT:is":
							return;
						case "SimBiology":
							return;
						case "Unit":
							return;
						case "CopasiMT:isPartOf":
							return;
						case "MesoRD:difference":
							return;
						case "MesoRD:translation":
							return;
						case "MesoRD:cylinder":
							return;
						case "MesoRD:union":
							return;
						case "MesoRD:sphere":
							return;
						case "MesoRD:compartment":
							return;
						case "boundingBox":
							return;
						default:
							throw new Exception("Unknown qName: " + qName);
					}
				case SbmlSection.ListOfSpecies:
					switch (qName){
						case "rdf:RDF":
							return;
						case "rdf:Description":
							return;
						case "rdf:Bag":
							return;
						case "rdf:li":
							switch (speciesAnnotationType){
								case SbmlSpeciesAnnotationType.Is:
									model.Species.Last().Is.Add(attrs["rdf:resource"]);
									return;
								case SbmlSpeciesAnnotationType.HasPart:
									model.Species.Last().HasPart.Add(attrs["rdf:resource"]);
									return;
								case SbmlSpeciesAnnotationType.IsEncodedBy:
									model.Species.Last().IsEncodedBy.Add(attrs["rdf:resource"]);
									return;
								case SbmlSpeciesAnnotationType.IsVersionOf:
									//TODO
									return;
								case SbmlSpeciesAnnotationType.HasVersion:
									//TODO
									return;
								case SbmlSpeciesAnnotationType.HasProperty:
									//TODO
									return;
								case SbmlSpeciesAnnotationType.Encodes:
									//TODO
									return;
								case SbmlSpeciesAnnotationType.IsDescribedBy:
									//TODO
									return;
								case SbmlSpeciesAnnotationType.IsPartOf:
									//TODO
									return;
								case SbmlSpeciesAnnotationType.IsHomologTo:
									//TODO
									return;
								case SbmlSpeciesAnnotationType.OccursIn:
									//TODO
									return;
								case SbmlSpeciesAnnotationType.IsPropertyOf:
									//TODO
									return;
								case SbmlSpeciesAnnotationType.VCellInfo:
									//TODO
									return;
								case SbmlSpeciesAnnotationType.UnknownQualifier:
									//TODO
									return;
								case SbmlSpeciesAnnotationType.None:
									if (attrs.Count == 0){
										return;
									} else{
										throw new Exception("Unknown species annotation.");
									}
								default:
									throw new Exception("Unknown species annotation.");
							}
						case "bqbiol:is":
							speciesAnnotationType = SbmlSpeciesAnnotationType.Is;
							return;
						case "CopasiMT:isEncodedBy":
						case "bqbiol:isEncodedBy":
							speciesAnnotationType = SbmlSpeciesAnnotationType.IsEncodedBy;
							return;
						case "CopasiMT:haspart":
						case "CopasiMT:hasPart":
						case "bqbiol:hasPart":
							speciesAnnotationType = SbmlSpeciesAnnotationType.HasPart;
							return;
						case "CopasiMT:isVersionOf":
						case "bqbiol:isVersionOf":
							speciesAnnotationType = SbmlSpeciesAnnotationType.IsVersionOf;
							return;
						case "CopasiMT:hasVersion":
						case "bqbiol:hasVersion":
							speciesAnnotationType = SbmlSpeciesAnnotationType.HasVersion;
							return;
						case "bqbiol:hasProperty":
							speciesAnnotationType = SbmlSpeciesAnnotationType.HasProperty;
							return;
						case "CopasiMT:encodes":
						case "bqbiol:encodes":
							speciesAnnotationType = SbmlSpeciesAnnotationType.Encodes;
							return;
						case "Compound":
						case "bqmodel:is":
						case "CopasiMT:is":
							speciesAnnotationType = SbmlSpeciesAnnotationType.Is;
							return;
						case "CopasiMT:isDescribedBy":
						case "bqbiol:isDescribedBy":
						case "bqmodel:isDescribedBy":
							speciesAnnotationType = SbmlSpeciesAnnotationType.IsDescribedBy;
							return;
						case "CopasiMT:isPartOf":
						case "bqbiol:isPartOf":
							speciesAnnotationType = SbmlSpeciesAnnotationType.IsPartOf;
							return;
						case "bqbiol:isHomologTo":
							speciesAnnotationType = SbmlSpeciesAnnotationType.IsHomologTo;
							return;
						case "bqbiol:isPropertyOf":
							speciesAnnotationType = SbmlSpeciesAnnotationType.IsPropertyOf;
							return;
						case "bqbiol:occursIn":
							speciesAnnotationType = SbmlSpeciesAnnotationType.OccursIn;
							return;
						case "VCellInfo":
							//needed?
							speciesAnnotationType = SbmlSpeciesAnnotationType.VCellInfo;
							return;
						case "bqbiol:unknownQualifier":
							speciesAnnotationType = SbmlSpeciesAnnotationType.UnknownQualifier;
							return;
						case "jd:display":
							return;
						case "jd:font":
							return;
						case "jd:listOfShadows":
							return;
						case "jd:shadow":
							return;
						case "celldesigner:positionToCompartment":
							return;
						case "celldesigner:speciesIdentity":
							return;
						case "celldesigner:class":
							return;
						case "celldesigner:proteinReference":
							return;
						case "celldesigner:name":
							return;
						case "celldesigner:geneReference":
							return;
						case "celldesigner:rnaReference":
							return;
						case "celldesigner:listOfCatalyzedReactions":
							return;
						case "celldesigner:catalyzed":
							return;
						case "COPASI":
							return;
						case "celldesigner:extension":
							return;
						case "dcterms:created":
							return;
						case "dcterms:W3CDTF":
							return;
						case "CopasiMT:isHomologTo":
							return;
						case "celldesigner:state":
							return;
						case "celldesigner:listOfModifications":
							return;
						case "celldesigner:modification":
							return;
						case "celldesigner:homodimer":
							return;
						case "dcterms:bibliographicCitation":
							return;
						case "dcterms:creator":
							return;
						case "vCard:N":
							return;
						case "vCard:Family":
							return;
						case "vCard:Given":
							return;
						case "vCard:ORG":
							return;
						case "vCard:Orgname":
							return;
						case "celldesigner:listOfStructuralStates":
							return;
						case "celldesigner:structuralState":
							return;
						case "SimBiology":
							return;
						case "Unit":
							return;
						case "dcterms:description":
							return;
						case "species":
							return;
						case "in:inchi":
							return;
						case "celldesigner:hypothetical":
							return;
						case "MesoRD:diffusion":
							return;
						case "vcell:VCellInfo":
							return;
						case "vcell:VCMLSpecific":
							return;
						case "vcell:Compound":
							return;
						case "notes":
							inNotes = true;
							currentNotes = new SbmlNotes();
							return;
						case "recon2":
							return;
						case "drugbank:unknown":
							return;
						case "drugbank:inhibitor":
							return;
						case "drugbank:cofactor":
							return;
						case "drugbank:substrate":
							return;
						case "drugbank:product_of":
							return;
						case "drugbank:inducer":
							return;
						case "drugbank:activator":
							return;
						case "drugbank:agonist":
							return;
						case "drugbank:ligand":
							return;
						case "drugbank:conversion_inhibitor":
							return;
						case "drugbank:positive_allosteric_modulator":
							return;
						case "drugbank:potentiator":
							return;
						case "drugbank:other_unknown":
							return;
						case "drugbank:antagonist":
							return;
						case "drugbank:other":
						case "drugbank:Other":
							return;
						case "drugbank:binder":
							return;
						case "font":
							return;
						case "celldesigner:heterodimerIdentity":
							return;
						case "celldesigner:listOfHeterodimerEntries":
							return;
						case "celldesigner:heterodimerEntry":
							return;
						default:
							throw new Exception("Unknown qName: " + qName);
					}
				case SbmlSection.ListOfReactions:
					switch (qName){
						case "rdf:RDF":
							return;
						case "rdf:Description":
							return;
						case "rdf:Bag":
							return;
						case "rdf:li":
							return;
						case "bqbiol:is":
							return;
						case "bqbiol:isVersionOf":
							return;
						case "bqbiol:isDescribedBy":
							return;
						case "bqbiol:hasVersion":
							return;
						case "bqbiol:hasPart":
							return;
						case "bqbiol:isHomologTo":
							return;
						case "jd:arcSeg":
							return;
						case "jd:pt":
							return;
						case "jd:shadowRef":
							return;
						case "bqbiol:isPartOf":
							return;
						case "bqmodel:isDescribedBy":
							return;
						case "bqmodel:is":
							return;
						case "jd:builtIn":
							return;
						case "jd:listOfSymbols":
							return;
						case "parameter":
							return;
						case "VCellInfo":
							return;
						case "SimpleReaction":
							return;
						case "ReactionRate":
							return;
						case "jigcell:ratelaw":
							return;
						case "FluxStep":
							return;
						case "celldesigner:reactionType":
							return;
						case "celldesigner:baseReactants":
							return;
						case "celldesigner:baseReactant":
							return;
						case "celldesigner:linkAnchor":
							return;
						case "celldesigner:baseProducts":
							return;
						case "celldesigner:baseProduct":
							return;
						case "celldesigner:connectScheme":
							return;
						case "celldesigner:listOfLineDirection":
							return;
						case "celldesigner:lineDirection":
							return;
						case "celldesigner:editPoints":
							return;
						case "celldesigner:line":
							return;
						case "celldesigner:alias":
							return;
						case "celldesigner:listOfModification":
							return;
						case "celldesigner:modification":
							return;
						case "celldesigner:linkTarget":
							return;
						case "celldesigner:listOfProductLinks":
							return;
						case "celldesigner:productLink":
							return;
						case "celldesigner:selectedFunction":
							return;
						case "celldesigner:extension":
							return;
						case "celldesigner:name":
							return;
						case "celldesigner:listOfReactantLinks":
							return;
						case "celldesigner:reactantLink":
							return;
						case "bqbiol:hasProperty":
							return;
						case "COPASI":
							return;
						case "dcterms:created":
							return;
						case "dcterms:W3CDTF":
							return;
						case "CopasiMT:isVersionOf":
							return;
						case "CopasiMT:isPartOf":
							return;
						case "dcterms:bibliographicCitation":
							return;
						case "CopasiMT:isDescribedBy":
							return;
						case "CopasiMT:is":
							return;
						case "dcterms:description":
							return;
						case "CopasiMT:hasVersion":
							return;
						case "BooleanLaws":
							return;
						case "vcell:VCellInfo":
							return;
						case "vcell:VCMLSpecific":
							return;
						case "vcell:SimpleReaction":
							return;
						case "vcell:ReactionRate":
							return;
						case "CopasiMT:occursIn":
							return;
						case "bqbiol:occursIn":
							return;
						case "coreco:annotation":
							return;
						case "coreco:ec":
							return;
						case "coreco:balanced":
							return;
						case "coreco:posterior":
							return;
						case "coreco:cost":
							return;
						case "coreco:level":
							return;
						case "coreco:naivep":
							return;
						case "coreco:btscore":
							return;
						case "coreco:gtscore":
							return;
						case "coreco:bscore":
							return;
						case "coreco:bseq1":
							return;
						case "coreco:bseq2":
							return;
						case "coreco:gscore":
							return;
						case "coreco:gseq1":
							return;
						case "coreco:gseq2":
							return;
						case "CopasiMT:isEncodedBy":
							return;
						case "bqbiol:isEncodedBy":
							return;
						case "CopasiMT:isHomologTo":
							return;
						case "pt":
							return;
						case "celldesigner:offset":
							return;
						case "flux:limit":
							return;
						default:
							throw new Exception("Unknown qName: " + qName);
					}
				case SbmlSection.ListOfParameters:
					switch (qName){
						case "rdf:RDF":
							return;
						case "rdf:Description":
							return;
						case "bqbiol:isVersionOf":
							return;
						case "rdf:Bag":
							return;
						case "rdf:li":
							return;
						case "bqbiol:hasPart":
							return;
						case "bqbiol:hasVersion":
							return;
						case "bqbiol:is":
							return;
						case "bqmodel:is":
							return;
						case "bqbiol:hasProperty":
							return;
						case "COPASI":
							return;
						case "dcterms:created":
							return;
						case "dcterms:W3CDTF":
							return;
						case "bqbiol:isPartOf":
							return;
						case "initialValue":
							return;
						case "CopasiMT:hasPart":
							return;
						case "bqmodel:isDescribedBy":
							return;
						case "dcterms:bibliographicCitation":
							return;
						case "CopasiMT:isDescribedBy":
							return;
						case "bqbiol:isDescribedBy":
							return;
						default:
							throw new Exception("Unknown qName: " + qName);
					}
				case SbmlSection.ListOfEvents:
					switch (qName){
						case "rdf:RDF":
							return;
						case "rdf:Description":
							return;
						case "bqbiol:is":
							return;
						case "rdf:Bag":
							return;
						case "rdf:li":
							return;
						case "bqbiol:isHomologTo":
							return;
						case "bqbiol:isVersionOf":
							return;
						case "bqbiol:hasVersion":
							return;
						case "bqbiol:isPartOf":
							return;
						case "bqmodel:is":
							return;
						case "COPASI":
							return;
						case "dcterms:created":
							return;
						case "dcterms:W3CDTF":
							return;
						default:
							throw new Exception("Unknown qName: " + qName);
					}
				case SbmlSection.ListOfRules:
					switch (qName){
						case "rdf:RDF":
							return;
						case "rdf:Description":
							return;
						case "bqbiol:isVersionOf":
							return;
						case "rdf:Bag":
							return;
						case "rdf:li":
							return;
						case "bqbiol:isPartOf":
							return;
						case "jigcell:species":
							return;
						case "bqbiol:hasPart":
							return;
						case "bqbiol:hasVersion":
							return;
						case "jigcell:conservationlaw":
							return;
						default:
							throw new Exception("Unknown qName: " + qName);
					}
				case SbmlSection.ListOfFunctionDefinitions:
					switch (qName){
						case "jigcell:ratelaw":
							return;
						case "COPASI":
							return;
						case "rdf:RDF":
							return;
						case "rdf:Description":
							return;
						case "dcterms:created":
							return;
						case "dcterms:W3CDTF":
							return;
						case "dcterms:modified":
							return;
						case "function":
							return;
						default:
							throw new Exception("Unknown qName: " + qName);
					}
				case SbmlSection.ListOfSpeciesTypes:
					switch (qName){
						case "rdf:RDF":
							return;
						case "rdf:Description":
							return;
						case "bqbiol:is":
							return;
						case "rdf:Bag":
							return;
						case "rdf:li":
							return;
						case "bqbiol:isDescribedBy":
							return;
						case "bqbiol:hasPart":
							return;
						case "bqbiol:isEncodedBy":
							return;
						case "bqbiol:isHomologTo":
							return;
						case "bqbiol:isVersionOf":
							return;
						default:
							throw new Exception("Unknown qName: " + qName);
					}
				default:
					throw new Exception("Unknown section");
			}
		}

		private void EndElementInAnnotation(string qName){
			if (qName.Equals("listOfKeyValueData")){
				inListOfKeyValueData = false;
				//TODO
				return;
			}
			switch (section){
				case SbmlSection.None:
					switch (qName){
						case "rdf:RDF":
							return;
						case "rdf:Description":
							return;
						case "dc:creator":
							return;
						case "rdf:Bag":
							return;
						case "rdf:li":
							return;
						case "vCard:N":
							return;
						case "vCard:Family":
							return;
						case "vCard:Given":
							return;
						case "vCard:EMAIL":
							return;
						case "vCard:ORG":
							return;
						case "vCard:Orgname":
							return;
						case "dcterms:created":
							return;
						case "dcterms:W3CDTF":
							return;
						case "dcterms:modified":
							return;
						case "bqbiol:occursIn":
							return;
						case "bqmodel:is":
							return;
						case "bqmodel:isDerivedFrom":
							return;
						case "bqbiol:isDescribedBy":
							return;
						case "bqmodel:isDescribedBy":
							return;
						case "bqbiol:isVersionOf":
							return;
						case "bqbiol:hasTaxon":
							return;
						case "bqbiol:isHomologTo":
							return;
						case "bqbiol:hasVersion":
							return;
						case "bqbiol:is":
							return;
						case "bqbiol:isPartOf":
							return;
						case "bqbiol:hasProperty":
							return;
						case "bqbiol:hasPart":
							return;
						case "jd:header":
							return;
						case "jd:VersionHeader":
							return;
						case "jd:ModelHeader":
							return;
						case "jd:display":
							return;
						case "jd:SBMLGraphicsHeader":
							return;
						case "jd:notes":
							return;
						case "jd:note":
							return;
						case "doqcs:timestamp":
							return;
						case "doqcs:accesstype":
							return;
						case "doqcs:transcriber":
							return;
						case "doqcs:developer":
							return;
						case "doqcs:species":
							return;
						case "doqcs:tissue":
							return;
						case "doqcs:cellcompartment":
							return;
						case "doqcs:methodology":
							return;
						case "doqcs:model_implementation":
							return;
						case "doqcs:model_validation":
							return;
						case "VCellInfo":
							return;
						case "BioModel":
							return;
						case "SimulationSpec":
							return;
						case "Simulation":
							return;
						case "celldesigner:modelVersion":
							return;
						case "celldesigner:modelDisplay":
							return;
						case "celldesigner:listOfIncludedSpecies":
							return;
						case "celldesigner:species":
							return;
						case "celldesigner:notes":
							return;
						case "html":
							return;
						case "body":
							return;
						case "celldesigner:annotation":
							return;
						case "celldesigner:complexSpecies":
							return;
						case "celldesigner:speciesIdentity":
							return;
						case "celldesigner:class":
							return;
						case "celldesigner:proteinReference":
							return;
						case "celldesigner:name":
							return;
						case "celldesigner:listOfCompartmentAliases":
							return;
						case "celldesigner:compartmentAlias":
							return;
						case "celldesigner:bounds":
							return;
						case "celldesigner:namePoint":
							return;
						case "celldesigner:doubleLine":
							return;
						case "celldesigner:paint":
							return;
						case "celldesigner:listOfComplexSpeciesAliases":
							return;
						case "celldesigner:complexSpeciesAlias":
							return;
						case "celldesigner:activity":
							return;
						case "celldesigner:view":
							return;
						case "celldesigner:backupSize":
							return;
						case "celldesigner:backupView":
							return;
						case "celldesigner:usualView":
							return;
						case "celldesigner:innerPosition":
							return;
						case "celldesigner:boxSize":
							return;
						case "celldesigner:singleLine":
							return;
						case "celldesigner:briefView":
							return;
						case "celldesigner:listOfSpeciesAliases":
							return;
						case "celldesigner:speciesAlias":
							return;
						case "celldesigner:listOfGroups":
							return;
						case "celldesigner:listOfProteins":
							return;
						case "celldesigner:protein":
							return;
						case "celldesigner:listOfGenes":
							return;
						case "celldesigner:listOfRNAs":
							return;
						case "celldesigner:listOfAntisenseRNAs":
							return;
						case "celldesigner:listOfLayers":
							return;
						case "celldesigner:listOfBlockDiagrams":
							return;
						case "celldesigner:point":
							return;
						case "celldesigner:gene":
							return;
						case "celldesigner:RNA":
							return;
						case "celldesigner:geneReference":
							return;
						case "COPASI":
							return;
						case "CopasiMT:isPartOf":
							return;
						case "celldesigner:extension":
							return;
						case "celldesigner:font":
							return;
						case "celldesigner:info":
							return;
						case "dcterms:creator":
							return;
						case "jd2:JDesignerLayout":
							return;
						case "jd2:header":
							return;
						case "jd2:VersionHeader":
							return;
						case "jd2:ModelHeader":
							return;
						case "jd2:TimeCourseDetails":
							return;
						case "jd2:JDGraphicsHeader":
							return;
						case "jd2:listOfCompartments":
							return;
						case "jd2:compartment":
							return;
						case "jd2:boundingBox":
							return;
						case "jd2:membraneStyle":
							return;
						case "jd2:interiorStyle":
							return;
						case "jd2:text":
							return;
						case "jd2:position":
							return;
						case "jd2:font":
							return;
						case "jd2:listOfSpecies":
							return;
						case "jd2:species":
							return;
						case "jd2:complex":
							return;
						case "jd2:subunit":
							return;
						case "jd2:color":
							return;
						case "jd2:edgeStyle":
							return;
						case "jd2:listOfReactions":
							return;
						case "jd2:reaction":
							return;
						case "jd2:listOfReactants":
							return;
						case "jd2:speciesReference":
							return;
						case "jd2:listOfProducts":
							return;
						case "jd2:listOfModifierEdges":
							return;
						case "jd2:kineticLaw":
							return;
						case "jd2:rateEquation":
							return;
						case "jd2:listOfSymbols":
							return;
						case "jd2:parameter":
							return;
						case "jd2:display":
							return;
						case "jd2:lineType":
							return;
						case "jd2:edge":
							return;
						case "jd2:pt":
							return;
						case "jd2:modifierEdge":
							return;
						case "jd2:destinationReaction":
							return;
						case "bqbiol:encodes":
							return;
						case "listoflayouts":
							return;
						case "layout":
							return;
						case "container":
							return;
						case "clone":
							return;
						case "neighbour":
							return;
						case "celldesigner:state":
							return;
						case "celldesigner:homodimer":
							return;
						case "celldesigner:listOfModifications":
							return;
						case "celldesigner:modification":
							return;
						case "celldesigner:listOfModificationResidues":
							return;
						case "celldesigner:modificationResidue":
							return;
						case "dcterms:bibliographicCitation":
							return;
						case "CopasiMT:isDescribedBy":
							return;
						case "celldesigner:blockDiagram":
							return;
						case "celldesigner:canvas":
							return;
						case "celldesigner:block":
							return;
						case "celldesigner:halo":
							return;
						case "celldesigner:listOfResiduesInBlockDiagram":
							return;
						case "celldesigner:listOfExternalNamesForResidue":
							return;
						case "celldesigner:listOfBindingSitesInBlockDiagram":
							return;
						case "celldesigner:listOfEffectSitesInBlockDiagram":
							return;
						case "celldesigner:effectSiteInBlockDiagram":
							return;
						case "celldesigner:effectInBlockDiagram":
							return;
						case "celldesigner:effectTargetInBlockDiagram":
							return;
						case "celldesigner:listOfInternalOperatorsInBlockDiagram":
							return;
						case "celldesigner:listOfInternalLinksInBlockDiagram":
							return;
						case "pathwaydocument":
							return;
						case "pathway":
							return;
						case "participantlist":
							return;
						case "participant":
							return;
						case "smallmolecule":
							return;
						case "compartment":
							return;
						case "synonymlist":
							return;
						case "synonym":
							return;
						case "datasource":
							return;
						case "database":
							return;
						case "databaselist":
							return;
						case "interactionlist":
							return;
						case "interaction":
							return;
						case "participantuselist":
							return;
						case "participantuse":
							return;
						case "presentationlist":
							return;
						case "presentation":
							return;
						case "participantstatelist":
							return;
						case "participantstate":
							return;
						case "interactionstatelist":
							return;
						case "interactionstate":
							return;
						case "experimentlist":
							return;
						case "experiment":
							return;
						case "celldesigner:structuralState":
							return;
						case "celldesigner:layer":
							return;
						case "CopasiMT:is":
							return;
						case "CopasiMT:isVersionOf":
							return;
						case "CopasiMT:occursIn":
							return;
						case "dcterms:description":
							return;
						case "CopasiMT:encodes":
							return;
						case "plasmo:dbinfo":
							return;
						case "jd2:listOfParameterSets":
							return;
						case "jd2:parameterSet":
							return;
						case "jd2:notes":
							return;
						case "jd2:NameValue":
							return;
						case "jd2:listOfParameters":
							return;
						case "jd2:otherDisplayObjects":
							return;
						case "jd2:textObject":
							return;
						case "jd2:squareObject":
							return;
						case "jd2:shapeProperties":
							return;
						case "jd2:circleObject":
							return;
						case "gxl":
							return;
						case "graph":
							return;
						case "node":
							return;
						case "parameter":
							return;
						case "value":
							return;
						case "exp":
							return;
						case "nodevisualsetting":
							return;
						case "rect":
							return;
						case "ellipse":
							return;
						case "celldesigner:rnaReference":
							return;
						case "celldesigner:listOfTexts":
							return;
						case "celldesigner:layerSpeciesAlias":
							return;
						case "celldesigner:layerNotes":
							return;
						case "vcell:VCellInfo":
							return;
						case "vcell:VCMLSpecific":
							return;
						case "vcell:BioModel":
							return;
						case "vcell:SimulationSpec":
							return;
						case "VersionHeader":
							return;
						case "ModelHeader":
							return;
						case "SBMLGraphicsHeader":
							return;
						case "note":
							return;
						case "celldesigner:listOfInnerAliases":
							return;
						case "celldesigner:innerAlias":
							return;
						case "celldesigner:group":
							return;
						case "jd2:edgeCenter":
							return;
						case "flux:listOfAnalysisTypes":
							return;
						case "flux:analysisType":
							return;
						case "flux:listOfAnalyses":
							return;
						case "flux:analysis":
							return;
						default:
							return;
					}
				case SbmlSection.Toplevel:
					switch (qName){
						case "mathsbml:AuthorConfiguration":
							return;
						case "mathsbml:date":
							return;
						case "mathsbml:MachineDomain":
							return;
						case "mathsbml:MachineID":
							return;
						case "mathsbml:MachineName":
							return;
						case "mathsbml:MachineType":
							return;
						case "mathsbml:MathematicaVersion":
							return;
						case "mathsbml:OperatingSystem":
							return;
						case "mathsbml:ProcessorType":
							return;
						case "mathsbml:ProductInformation":
							return;
						case "mathsbml:SoftwareVersion":
							return;
						case "mathsbml:System":
							return;
						case "mathsbml:UserName":
							return;
						case "rdf:RDF":
							return;
						case "rdf:Description":
							return;
						case "dc:creator":
							return;
						case "rdf:Bag":
							return;
						case "rdf:li":
							return;
						case "vCard:N":
							return;
						case "vCard:Family":
							return;
						case "vCard:Given":
							return;
						case "vCard:EMAIL":
							return;
						case "vCard:ORG":
							return;
						case "vCard:Orgname":
							return;
						case "dcterms:created":
							return;
						case "dcterms:W3CDTF":
							return;
						case "dcterms:modified":
							return;
						case "bqmodel:is":
							return;
						case "bqmodel:isDescribedBy":
							return;
						case "bqmodel:isDerivedFrom":
							return;
						case "bqbiol:isVersionOf":
							return;
						case "bqbiol:hasTaxon":
							return;
						case "bqbiol:isHomologTo":
							return;
						case "jd:header":
							return;
						case "jd:VersionHeader":
							return;
						case "jd:ModelHeader":
							return;
						case "jd:display":
							return;
						case "jd:SBMLGraphicsHeader":
							return;
						case "jd:notes":
							return;
						case "jd:note":
							return;
						case "SimBiology":
							return;
						case "Version":
							return;
						case "bqbiol:hasProperty":
							return;
						case "bqbiol:occursIn":
							return;
						case "jigcell:blankline":
							return;

						default:
							throw new Exception("Unknown qName: " + qName);
					}
				case SbmlSection.ListOfCompartments:
					switch (qName){
						case "rdf:RDF":
							return;
						case "rdf:Description":
							return;
						case "bqbiol:is":
							return;
						case "rdf:Bag":
							return;
						case "rdf:li":
							return;
						case "jd:display":
							return;
						case "jd:boundingBox":
							return;
						case "bqbiol:isVersionOf":
							return;
						case "bqmodel:is":
							return;
						case "celldesigner:name":
							return;
						case "bqbiol:isPartOf":
							return;
						case "bqbiol:hasPart":
							return;
						case "bqbiol:occursIn":
							return;
						case "COPASI":
							return;
						case "dcterms:created":
							return;
						case "dcterms:W3CDTF":
							return;
						case "celldesigner:extension":
							return;
						case "CopasiMT:isVersionOf":
							return;
						case "CopasiMT:is":
							return;
						case "SimBiology":
							return;
						case "Unit":
							return;
						case "CopasiMT:isPartOf":
							return;
						case "MesoRD:difference":
							return;
						case "MesoRD:translation":
							return;
						case "MesoRD:cylinder":
							return;
						case "MesoRD:union":
							return;
						case "MesoRD:sphere":
							return;
						case "MesoRD:compartment":
							return;
						case "boundingBox":
							return;
						default:
							throw new Exception("Unknown qName: " + qName);
					}
				case SbmlSection.ListOfSpecies:
					switch (qName){
						case "rdf:RDF":
							return;
						case "rdf:Description":
							return;
						case "rdf:Bag":
							return;
						case "rdf:li":
							return;
						case "bqbiol:is":
							speciesAnnotationType = SbmlSpeciesAnnotationType.None;
							return;
						case "bqbiol:isEncodedBy":
							speciesAnnotationType = SbmlSpeciesAnnotationType.None;
							return;
						case "bqbiol:hasPart":
							speciesAnnotationType = SbmlSpeciesAnnotationType.None;
							return;
						case "CopasiMT:isVersionOf":
						case "bqbiol:isVersionOf":
							speciesAnnotationType = SbmlSpeciesAnnotationType.None;
							return;
						case "bqbiol:hasVersion":
							speciesAnnotationType = SbmlSpeciesAnnotationType.None;
							return;
						case "bqbiol:hasProperty":
							speciesAnnotationType = SbmlSpeciesAnnotationType.None;
							return;
						case "bqbiol:encodes":
							speciesAnnotationType = SbmlSpeciesAnnotationType.None;
							return;
						case "jd:display":
							return;
						case "jd:font":
							return;
						case "jd:listOfShadows":
							return;
						case "jd:shadow":
							return;
						case "Compound":
						case "bqmodel:is":
						case "CopasiMT:is":
							speciesAnnotationType = SbmlSpeciesAnnotationType.None;
							return;
						case "bqbiol:isDescribedBy":
						case "bqmodel:isDescribedBy":
							speciesAnnotationType = SbmlSpeciesAnnotationType.None;
							return;
						case "bqbiol:isPartOf":
							speciesAnnotationType = SbmlSpeciesAnnotationType.None;
							return;
						case "bqbiol:isHomologTo":
							speciesAnnotationType = SbmlSpeciesAnnotationType.None;
							return;
						case "VCellInfo":
							speciesAnnotationType = SbmlSpeciesAnnotationType.None;
							return;
						case "celldesigner:positionToCompartment":
							return;
						case "celldesigner:speciesIdentity":
							return;
						case "celldesigner:class":
							return;
						case "celldesigner:proteinReference":
							return;
						case "celldesigner:name":
							return;
						case "celldesigner:geneReference":
							return;
						case "celldesigner:rnaReference":
							return;
						case "celldesigner:listOfCatalyzedReactions":
							return;
						case "celldesigner:catalyzed":
							return;
						case "bqbiol:isPropertyOf":
							speciesAnnotationType = SbmlSpeciesAnnotationType.None;
							return;
						case "bqbiol:occursIn":
							speciesAnnotationType = SbmlSpeciesAnnotationType.None;
							return;
						case "COPASI":
							return;
						case "CopasiMT:encodes":
							speciesAnnotationType = SbmlSpeciesAnnotationType.None;
							return;
						case "celldesigner:extension":
							return;
						case "dcterms:created":
							return;
						case "dcterms:W3CDTF":
							return;
						case "CopasiMT:isHomologTo":
							speciesAnnotationType = SbmlSpeciesAnnotationType.None;
							return;
						case "celldesigner:state":
							return;
						case "celldesigner:listOfModifications":
							return;
						case "celldesigner:modification":
							return;
						case "celldesigner:homodimer":
							return;
						case "dcterms:bibliographicCitation":
							return;
						case "CopasiMT:isDescribedBy":
							speciesAnnotationType = SbmlSpeciesAnnotationType.None;
							return;
						case "dcterms:creator":
							return;
						case "vCard:N":
							return;
						case "vCard:Family":
							return;
						case "vCard:Given":
							return;
						case "vCard:ORG":
							return;
						case "vCard:Orgname":
							return;
						case "celldesigner:listOfStructuralStates":
							return;
						case "celldesigner:structuralState":
							return;
						case "CopasiMT:hasPart":
							speciesAnnotationType = SbmlSpeciesAnnotationType.None;
							return;
						case "CopasiMT:isPartOf":
							speciesAnnotationType = SbmlSpeciesAnnotationType.None;
							return;
						case "CopasiMT:hasVersion":
							speciesAnnotationType = SbmlSpeciesAnnotationType.None;
							return;
						case "CopasiMT:haspart":
							speciesAnnotationType = SbmlSpeciesAnnotationType.None;
							return;
						case "SimBiology":
							return;
						case "Unit":
							return;
						case "dcterms:description":
							return;
						case "CopasiMT:isEncodedBy":
							speciesAnnotationType = SbmlSpeciesAnnotationType.None;
							return;
						case "in:inchi":
							return;
						case "celldesigner:hypothetical":
							return;
						case "MesoRD:diffusion":
							return;
						case "vcell:VCellInfo":
							return;
						case "vcell:VCMLSpecific":
							return;
						case "vcell:Compound":
							return;
						case "p":
						case "body":
							//strange
							return;
						case "recon2":
							return;
						case "drugbank:unknown":
							return;
						case "drugbank:inhibitor":
							return;
						case "drugbank:cofactor":
							return;
						case "drugbank:substrate":
							return;
						case "drugbank:product_of":
							return;
						case "drugbank:inducer":
							return;
						case "drugbank:activator":
							return;
						case "drugbank:agonist":
							return;
						case "drugbank:ligand":
							return;
						case "drugbank:conversion_inhibitor":
							return;
						case "drugbank:positive_allosteric_modulator":
							return;
						case "drugbank:potentiator":
							return;
						case "drugbank:other_unknown":
							return;
						case "drugbank:antagonist":
							return;
						case "drugbank:other":
						case "drugbank:Other":
							return;
						case "bqbiol:unknownQualifier":
							speciesAnnotationType = SbmlSpeciesAnnotationType.None;
							return;
						case "drugbank:binder":
							return;
						case "font":
							return;
						case "celldesigner:heterodimerIdentity":
							return;
						case "celldesigner:listOfHeterodimerEntries":
							return;
						case "celldesigner:heterodimerEntry":
							return;
						default:
							throw new Exception("Unknown qName: " + qName);
					}
				case SbmlSection.ListOfReactions:
					switch (qName){
						case "rdf:RDF":
							return;
						case "rdf:Description":
							return;
						case "bqbiol:isVersionOf":
							return;
						case "rdf:Bag":
							return;
						case "rdf:li":
							return;
						case "bqbiol:isDescribedBy":
							return;
						case "bqbiol:is":
							return;
						case "bqbiol:hasVersion":
							return;
						case "bqbiol:hasPart":
							return;
						case "bqbiol:isHomologTo":
							return;
						case "jd:arcSeg":
							return;
						case "jd:pt":
							return;
						case "jd:shadowRef":
							return;
						case "bqbiol:isPartOf":
							return;
						case "bqmodel:isDescribedBy":
							return;
						case "bqmodel:is":
							return;
						case "jd:builtIn":
							return;
						case "jd:listOfSymbols":
							return;
						case "parameter":
							return;
						case "VCellInfo":
							return;
						case "SimpleReaction":
							return;
						case "ReactionRate":
							return;
						case "jigcell:ratelaw":
							return;
						case "FluxStep":
							return;
						case "celldesigner:reactionType":
							return;
						case "celldesigner:baseReactants":
							return;
						case "celldesigner:baseReactant":
							return;
						case "celldesigner:linkAnchor":
							return;
						case "celldesigner:baseProducts":
							return;
						case "celldesigner:baseProduct":
							return;
						case "celldesigner:connectScheme":
							return;
						case "celldesigner:listOfLineDirection":
							return;
						case "celldesigner:lineDirection":
							return;
						case "celldesigner:editPoints":
							return;
						case "celldesigner:line":
							return;
						case "celldesigner:alias":
							return;
						case "celldesigner:listOfModification":
							return;
						case "celldesigner:modification":
							return;
						case "celldesigner:linkTarget":
							return;
						case "celldesigner:listOfProductLinks":
							return;
						case "celldesigner:productLink":
							return;
						case "celldesigner:selectedFunction":
							return;
						case "celldesigner:extension":
							return;
						case "celldesigner:name":
							return;
						case "celldesigner:listOfReactantLinks":
							return;
						case "celldesigner:reactantLink":
							return;
						case "bqbiol:hasProperty":
							return;
						case "COPASI":
							return;
						case "dcterms:created":
							return;
						case "dcterms:W3CDTF":
							return;
						case "CopasiMT:isVersionOf":
							return;
						case "CopasiMT:isPartOf":
							return;
						case "dcterms:bibliographicCitation":
							return;
						case "CopasiMT:isDescribedBy":
							return;
						case "CopasiMT:is":
							return;
						case "dcterms:description":
							return;
						case "CopasiMT:hasVersion":
							return;
						case "BooleanLaws":
							return;
						case "vcell:VCellInfo":
							return;
						case "vcell:VCMLSpecific":
							return;
						case "vcell:SimpleReaction":
							return;
						case "vcell:ReactionRate":
							return;
						case "CopasiMT:occursIn":
							return;
						case "bqbiol:occursIn":
							return;
						case "coreco:annotation":
							return;
						case "coreco:ec":
							return;
						case "coreco:balanced":
							return;
						case "coreco:posterior":
							return;
						case "coreco:cost":
							return;
						case "coreco:level":
							return;
						case "coreco:naivep":
							return;
						case "coreco:btscore":
							return;
						case "coreco:gtscore":
							return;
						case "coreco:bscore":
							return;
						case "coreco:bseq1":
							return;
						case "coreco:bseq2":
							return;
						case "coreco:gscore":
							return;
						case "coreco:gseq1":
							return;
						case "coreco:gseq2":
							return;
						case "CopasiMT:isEncodedBy":
							return;
						case "bqbiol:isEncodedBy":
							return;
						case "CopasiMT:isHomologTo":
							return;
						case "pt":
							return;
						case "celldesigner:offset":
							return;
						case "flux:limit":
							return;
						default:
							throw new Exception("Unknown qName: " + qName);
					}
				case SbmlSection.ListOfParameters:
					switch (qName){
						case "rdf:RDF":
							return;
						case "rdf:Description":
							return;
						case "bqbiol:isVersionOf":
							return;
						case "rdf:Bag":
							return;
						case "rdf:li":
							return;
						case "bqbiol:hasPart":
							return;
						case "bqbiol:hasVersion":
							return;
						case "bqbiol:is":
							return;
						case "bqmodel:is":
							return;
						case "bqbiol:hasProperty":
							return;
						case "COPASI":
							return;
						case "dcterms:created":
							return;
						case "dcterms:W3CDTF":
							return;
						case "bqbiol:isPartOf":
							return;
						case "initialValue":
							return;
						case "CopasiMT:hasPart":
							return;
						case "bqmodel:isDescribedBy":
							return;
						case "dcterms:bibliographicCitation":
							return;
						case "CopasiMT:isDescribedBy":
							return;
						case "bqbiol:isDescribedBy":
							return;
						default:
							throw new Exception("Unknown qName: " + qName);
					}
				case SbmlSection.ListOfEvents:
					switch (qName){
						case "rdf:RDF":
							return;
						case "rdf:Description":
							return;
						case "bqbiol:is":
							return;
						case "rdf:Bag":
							return;
						case "rdf:li":
							return;
						case "bqbiol:isHomologTo":
							return;
						case "bqbiol:isVersionOf":
							return;
						case "bqbiol:hasVersion":
							return;
						case "bqbiol:isPartOf":
							return;
						case "bqmodel:is":
							return;
						case "COPASI":
							return;
						case "dcterms:created":
							return;
						case "dcterms:W3CDTF":
							return;
						default:
							throw new Exception("Unknown qName: " + qName);
					}
				case SbmlSection.ListOfRules:
					switch (qName){
						case "rdf:RDF":
							return;
						case "rdf:Description":
							return;
						case "bqbiol:isVersionOf":
							return;
						case "rdf:Bag":
							return;
						case "rdf:li":
							return;
						case "bqbiol:isPartOf":
							return;
						case "jigcell:species":
							return;
						case "bqbiol:hasPart":
							return;
						case "bqbiol:hasVersion":
							return;
						case "jigcell:conservationlaw":
							return;
						default:
							throw new Exception("Unknown qName: " + qName);
					}
				case SbmlSection.ListOfFunctionDefinitions:
					switch (qName){
						case "jigcell:ratelaw":
							return;
						case "COPASI":
							return;
						case "rdf:RDF":
							return;
						case "rdf:Description":
							return;
						case "dcterms:created":
							return;
						case "dcterms:W3CDTF":
							return;
						case "dcterms:modified":
							return;
						case "function":
							return;
						default:
							throw new Exception("Unknown qName: " + qName);
					}
				case SbmlSection.ListOfSpeciesTypes:
					switch (qName){
						case "rdf:RDF":
							return;
						case "rdf:Description":
							return;
						case "bqbiol:is":
							return;
						case "rdf:Bag":
							return;
						case "rdf:li":
							return;
						case "bqbiol:isDescribedBy":
							return;
						case "bqbiol:hasPart":
							return;
						case "bqbiol:isEncodedBy":
							return;
						case "bqbiol:isHomologTo":
							return;
						case "bqbiol:isVersionOf":
							return;
						default:
							throw new Exception("Unknown qName: " + qName);
					}
				default:
					throw new Exception("Unknown section");
			}
		}

		private void StartElement(string qName, IDictionary<string, string> attrs, int depth){
			if (inNotes){
				StartElementInNotes(qName);
				return;
			}
			if (inMath){
				StartElementInMath(qName);
				return;
			}
			if (inAnnotation){
				StartElementInAnnotation(qName, attrs);
				return;
			}
			switch (depth){
				case 0:
					switch (qName){
						case "sbml":
							sbmlLevel = int.Parse(attrs["level"]);
							sbmlVersion = int.Parse(attrs["version"]);
							return;
						default:
							throw new Exception("Unknown qName");
					}
				case 1:
					switch (qName){
						case "model":
							model = new SbmlModel{
								Id = attrs.ContainsKey("id") ? attrs["id"] : "",
								Name = attrs.ContainsKey("name") ? attrs["name"] : ""
							};
							return;
						case "annotation":
							section = SbmlSection.Toplevel;
							inAnnotation = true;
							return;
						case "notes":
							inNotes = true;
							currentNotes = new SbmlNotes();
							//TODO
							//model.Notes = currentNotes;
							return;
						default:
							throw new Exception("Unknown qName");
					}
				case 2:
					switch (qName){
						case "notes":
							inNotes = true;
							currentNotes = new SbmlNotes();
							model.Notes = currentNotes;
							return;
						case "annotation":
							inAnnotation = true;
							return;
						case "listOfUnitDefinitions":
							section = SbmlSection.ListOfUnitDefinitions;
							model.UnitDefinitions = new List<SbmlUnitDefinition>();
							return;
						case "listOfCompartments":
							section = SbmlSection.ListOfCompartments;
							model.Compartments = new List<SbmlCompartment>();
							return;
						case "listOfSpecies":
							section = SbmlSection.ListOfSpecies;
							model.Species = new List<SbmlSpecies>();
							return;
						case "listOfReactions":
							section = SbmlSection.ListOfReactions;
							model.Reactions = new List<SbmlReaction>();
							return;
						case "listOfParameters":
							section = SbmlSection.ListOfParameters;
							model.Parameters = new List<SbmlParameter>();
							return;
						case "listOfEvents":
							section = SbmlSection.ListOfEvents;
							model.Events = new List<SbmlEvent>();
							return;
						case "listOfRules":
							section = SbmlSection.ListOfRules;
							model.Rules = new List<SbmlRule>();
							return;
						case "listOfFunctionDefinitions":
							section = SbmlSection.ListOfFunctionDefinitions;
							model.FunctionDefinitions = new List<SbmlFunctionDefinition>();
							return;
						case "listOfInitialAssignments":
							section = SbmlSection.ListOfInitialAssignments;
							model.InitialAssignments = new List<SbmlInitialAssignment>();
							return;
						case "listOfSpeciesTypes":
							section = SbmlSection.ListOfSpeciesTypes;
							model.SpeciesTypes = new List<SbmlSpeciesType>();
							return;
						case "qual:listOfQualitativeSpecies":
							section = SbmlSection.ListOfQualitativeSpecies;
							model.QualitativeSpecies = new List<SbmlQualitativeSpecies>();
							return;
						case "qual:listOfTransitions":
							section = SbmlSection.ListOfTransitions;
							model.Transitions = new List<SbmlTransition>();
							return;
						case "listOfConstraints":
							section = SbmlSection.ListOfConstraints;
							model.Constraints = new List<SbmlConstraint>();
							return;
						case "fbc:listOfFluxBounds":
							section = SbmlSection.ListOfFluxBounds;
							model.FluxBounds = new List<SbmlFluxBound>();
							return;
						case "fbc:listOfObjectives":
							section = SbmlSection.ListOfObjectives;
							model.Objectives = new List<SbmlObjective>();
							return;
						default:
							throw new Exception("Unknown qName: " + qName);
					}
				case 3:
					switch (section){
						case SbmlSection.ListOfUnitDefinitions:
							switch (qName){
								case "unitDefinition":
									model.UnitDefinitions.Add(new SbmlUnitDefinition{Id = attrs["id"]});
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfCompartments:
							switch (qName){
								case "compartment":
									model.Compartments.Add(new SbmlCompartment{
										Id = attrs["id"],
										Name = attrs.ContainsKey("name") ? attrs["name"] : "",
										Constant = !attrs.ContainsKey("constant") || bool.Parse(attrs["constant"]),
										SpatialDimensions = attrs.ContainsKey("spatialDimensions") ? int.Parse(attrs["spatialDimensions"]) : 0,
										SboTerm = attrs.ContainsKey("sboTerm") ? attrs["sboTerm"] : "",
										Outside = attrs.ContainsKey("outside") ? attrs["outside"] : "",
										Size = attrs.ContainsKey("size") ? Parser.Double(attrs["size"]) : double.NaN
									});
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfSpecies:
							switch (qName){
								case "species":
									model.Species.Add(new SbmlSpecies{
										Id = attrs["id"],
										Name = attrs.ContainsKey("name") ? attrs["name"] : "",
										InitialConcentration =
											attrs.ContainsKey("initialConcentration") ? Parser.Double(attrs["initialConcentration"]) : double.NaN,
										Constant = !attrs.ContainsKey("constant") || bool.Parse(attrs["constant"]),
										Charge = attrs.ContainsKey("charge") ? int.Parse(attrs["charge"]) : 0,
										HasOnlySubstanceUnits =
											!attrs.ContainsKey("hasOnlySubstanceUnits") || bool.Parse(attrs["hasOnlySubstanceUnits"]),
										BoundaryCondition = attrs.ContainsKey("boundaryCondition") && bool.Parse(attrs["boundaryCondition"]),
										SboTerm = attrs.ContainsKey("sboTerm") ? attrs["sboTerm"] : "",
										Compartment = attrs["compartment"]
									});
									return;
								case "notes":
									inNotes = true;
									currentNotes = new SbmlNotes();
									//strange
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfReactions:
							switch (qName){
								case "reaction":
									model.Reactions.Add(new SbmlReaction{
										Id = attrs["id"],
										Name = attrs.ContainsKey("name") ? attrs["name"] : "",
										Reversible = attrs.ContainsKey("reversible") && bool.Parse(attrs["reversible"]),
										SboTerm = attrs.ContainsKey("sboTerm") ? attrs["sboTerm"] : ""
									});
									return;
								case "notes":
									inNotes = true;
									currentNotes = new SbmlNotes();
									//strange
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfParameters:
							switch (qName){
								case "parameter":
									model.Parameters.Add(new SbmlParameter{
										Id = attrs["id"],
										Name = attrs.ContainsKey("name") ? attrs["name"] : "",
										Constant = attrs.ContainsKey("constant") && bool.Parse(attrs["constant"]),
										SboTerm = attrs.ContainsKey("sboTerm") ? attrs["sboTerm"] : "",
										Value = attrs.ContainsKey("value") ? Parser.Double(attrs["value"]) : double.NaN
									});
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfEvents:
							switch (qName){
								case "event":
									model.Events.Add(new SbmlEvent{
										Id = attrs.ContainsKey("id") ? attrs["id"] : "",
										Name = attrs.ContainsKey("name") ? attrs["name"] : "",
									});
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfRules:
							switch (qName){
								case "assignmentRule":
									model.Rules.Add(new SbmlAssignmentRule{Variable = attrs["variable"]});
									return;
								case "rateRule":
									model.Rules.Add(new SbmlRateRule{Variable = attrs["variable"]});
									return;
								case "algebraicRule":
									model.Rules.Add(new SbmlAlgebraicRule());
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfFunctionDefinitions:
							switch (qName){
								case "functionDefinition":
									model.FunctionDefinitions.Add(new SbmlFunctionDefinition{Id = attrs["id"]});
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfInitialAssignments:
							switch (qName){
								case "initialAssignment":
									model.InitialAssignments.Add(new SbmlInitialAssignment{Symbol = attrs["symbol"]});
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfSpeciesTypes:
							switch (qName){
								case "speciesType":
									//TODO
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfTransitions:
							switch (qName){
								case "qual:transition":
									//TODO
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfQualitativeSpecies:
							switch (qName){
								case "qual:qualitativeSpecies":
									//TODO
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfConstraints:
							switch (qName){
								case "constraint":
									//TODO
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfFluxBounds:
							switch (qName){
								case "fbc:fluxBound":
									//TODO
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfObjectives:
							switch (qName){
								case "fbc:objective":
									//TODO
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						default:
							throw new Exception("Unknown section");
					}
				case 4:
					switch (section){
						case SbmlSection.ListOfUnitDefinitions:
							switch (qName){
								case "notes":
									inNotes = true;
									currentNotes = new SbmlNotes();
									model.UnitDefinitions.Last().Notes = currentNotes;
									return;
								case "listOfUnits":
									model.UnitDefinitions.Last().Units = new List<SbmlUnit>();
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfCompartments:
							switch (qName){
								case "notes":
									inNotes = true;
									currentNotes = new SbmlNotes();
									model.Compartments.Last().Notes = currentNotes;
									return;
								case "annotation":
									inAnnotation = true;
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfSpecies:
							switch (qName){
								case "notes":
									inNotes = true;
									currentNotes = new SbmlNotes();
									model.Species.Last().Notes = currentNotes;
									return;
								case "annotation":
									inAnnotation = true;
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfReactions:
							switch (qName){
								case "notes":
									inNotes = true;
									currentNotes = new SbmlNotes();
									model.Reactions.Last().Notes = currentNotes;
									return;
								case "annotation":
									inAnnotation = true;
									return;
								case "listOfReactants":
									inListOfReactants = true;
									model.Reactions.Last().Reactants = new List<string>();
									return;
								case "listOfProducts":
									model.Reactions.Last().Products = new List<string>();
									return;
								case "kineticLaw":
									model.Reactions.Last().KineticLaw = new SbmlKineticLaw();
									return;
								case "listOfModifiers":
									model.Reactions.Last().Modifiers = new List<string>();
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfParameters:
							switch (qName){
								case "notes":
									inNotes = true;
									currentNotes = new SbmlNotes();
									model.Parameters.Last().Notes = currentNotes;
									return;
								case "annotation":
									inAnnotation = true;
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfEvents:
							switch (qName){
								case "annotation":
									inAnnotation = true;
									return;
								case "notes":
									inNotes = true;
									currentNotes = new SbmlNotes();
									model.Events.Last().Notes = currentNotes;
									return;
								case "trigger":
									//TODO
									return;
								case "listOfEventAssignments":
									//TODO
									return;
								case "delay":
									//TODO
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfRules:
							switch (qName){
								case "annotation":
									inAnnotation = true;
									return;
								case "notes":
									inNotes = true;
									currentNotes = new SbmlNotes();
									model.Rules.Last().Notes = currentNotes;
									return;
								case "math":
									inMath = true;
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfFunctionDefinitions:
							switch (qName){
								case "annotation":
									inAnnotation = true;
									return;
								case "notes":
									inNotes = true;
									currentNotes = new SbmlNotes();
									model.FunctionDefinitions.Last().Notes = currentNotes;
									return;
								case "math":
									inMath = true;
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfInitialAssignments:
							switch (qName){
								case "math":
									inMath = true;
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfTransitions:
							switch (qName){
								case "qual:listOfInputs":
									return;
								case "qual:listOfOutputs":
									return;
								case "qual:listOfFunctionTerms":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfSpeciesTypes:
							switch (qName){
								case "annotation":
									inAnnotation = true;
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfConstraints:
							switch (qName){
								case "math":
									inMath = true;
									return;
								case "message":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfObjectives:
							switch (qName){
								case "fbc:listOfFluxObjectives":
									//TODO
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						default:
							throw new Exception("Unknown section");
					}
				case 5:
					switch (section){
						case SbmlSection.ListOfUnitDefinitions:
							switch (qName){
								case "unit":
									model.UnitDefinitions.Last().Units.Add(new SbmlUnit{
										Scale = attrs.ContainsKey("scale") ? Parser.Double(attrs["scale"]) : double.NaN,
										Exponent = attrs.ContainsKey("exponent") ? int.Parse(attrs["exponent"]) : 0,
										Multiplier = attrs.ContainsKey("multiplier") ? Parser.Double(attrs["multiplier"]) : double.NaN,
										Kind = attrs["kind"]
									});
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfReactions:
							switch (qName){
								case "annotation":
									inAnnotation = true;
									return;
								case "speciesReference":
									if (inListOfReactants){
										model.Reactions.Last().Reactants.Add(attrs["species"]);
									} else{
										model.Reactions.Last().Products.Add(attrs["species"]);
									}
									return;
								case "math":
									inMath = true;
									return;
								case "listOfParameters":
									model.Reactions.Last().KineticLaw.Parameters = new List<SbmlParameter>();
									return;
								case "modifierSpeciesReference":
									model.Reactions.Last().Modifiers.Add(attrs["species"]);
									return;
								case "notes":
									inNotes = true;
									currentNotes = new SbmlNotes();
									model.Reactions.Last().KineticLaw.Notes = currentNotes;
									return;
								case "listOfLocalParameters":
									//TODO
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfParameters:
							switch (qName){
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfEvents:
							switch (qName){
								case "math":
									inMath = true;
									return;
								case "eventAssignment":
									model.Events.Last().EventAssignment = new SbmlEventAssignment{Variable = attrs["variable"]};
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfRules:
							switch (qName){
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfTransitions:
							switch (qName){
								case "qual:defaultTerm":
									return;
								case "qual:functionTerm":
									return;
								case "qual:input":
									return;
								case "qual:output":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfConstraints:
							switch (qName){
								case "p":
									//TODO: could be any html
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfObjectives:
							switch (qName){
								case "fbc:fluxObjective":
									//TODO
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						default:
							throw new Exception("Unknown section");
					}
				case 6:
					switch (section){
						case SbmlSection.ListOfReactions:
							switch (qName){
								case "parameter":
									//TODO
									return;
								case "localParameter":
									//TODO
									return;
								case "annotation":
									//TODO
									inAnnotation = true;
									return;
								case "stoichiometryMath":
									//TODO
									return;
								case "notes":
									inNotes = true;
									currentNotes = new SbmlNotes();
									//TODO
									//model.Events.Last().EventAssignment.Notes = currentNotes;
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfParameters:
							switch (qName){
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfEvents:
							switch (qName){
								case "math":
									inMath = true;
									return;
								case "notes":
									inNotes = true;
									currentNotes = new SbmlNotes();
									model.Events.Last().EventAssignment.Notes = currentNotes;
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfRules:
							switch (qName){
								case "math":
									inMath = true;
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfTransitions:
							switch (qName){
								case "math":
									inMath = true;
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						default:
							throw new Exception("Unknown section");
					}
				case 7:
					switch (section){
						case SbmlSection.ListOfReactions:
							switch (qName){
								case "notes":
									inNotes = true;
									currentNotes = new SbmlNotes();
									//TODO
									//model.Reactions.Last().EventAssignment.Notes = currentNotes;
									return;
								case "math":
									inMath = true;
									return;
								case "annotation":
									inAnnotation = true;
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						default:
							throw new Exception("Unknown section");
					}
			}
		}

		private void EndElement(string qName, int depth){
			switch (qName){
				case "notes":
					inNotes = false;
					return;
				case "annotation":
					inAnnotation = false;
					return;
				case "math":
					inMath = false;
					return;
			}
			if (inAnnotation){
				EndElementInAnnotation(qName);
				return;
			}
			if (inMath){
				EndElementInMath(qName);
				return;
			}
			if (inNotes){
				return;
			}
			switch (depth){
				case 0:
					switch (qName){
						case "sbml":
							return;
						default:
							throw new Exception("Unknown qName: " + qName);
					}
				case 1:
					switch (qName){
						case "model":
							return;
						default:
							throw new Exception("Unknown qName: " + qName);
					}
				case 2:
					section = SbmlSection.None;
					switch (qName){
						case "listOfUnitDefinitions":
							return;
						case "listOfCompartments":
							return;
						case "listOfSpecies":
							return;
						case "listOfReactions":
							return;
						case "listOfParameters":
							return;
						case "listOfEvents":
							return;
						case "listOfRules":
							return;
						case "listOfFunctionDefinitions":
							return;
						case "listOfInitialAssignments":
							return;
						case "listOfSpeciesTypes":
							return;
						case "qual:listOfQualitativeSpecies":
							return;
						case "qual:listOfTransitions":
							return;
						case "listOfConstraints":
							return;
						case "fbc:listOfFluxBounds":
							return;
						case "fbc:listOfObjectives":
							return;
						default:
							throw new Exception("Unknown qName: " + qName);
					}
				case 3:
					switch (section){
						case SbmlSection.None:
							return;
						case SbmlSection.ListOfUnitDefinitions:
							switch (qName){
								case "unitDefinition":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfCompartments:
							switch (qName){
								case "compartment":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfSpecies:
							switch (qName){
								case "notes":
									//strange
									return;
								case "species":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfReactions:
							switch (qName){
								case "reaction":
									return;
								case "notes":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfParameters:
							switch (qName){
								case "parameter":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfEvents:
							switch (qName){
								case "event":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfRules:
							switch (qName){
								case "assignmentRule":
									return;
								case "rateRule":
									return;
								case "algebraicRule":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfFunctionDefinitions:
							switch (qName){
								case "functionDefinition":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfInitialAssignments:
							switch (qName){
								case "initialAssignment":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfSpeciesTypes:
							switch (qName){
								case "speciesType":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfTransitions:
							switch (qName){
								case "qual:transition":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfQualitativeSpecies:
							switch (qName){
								case "qual:qualitativeSpecies":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfConstraints:
							switch (qName){
								case "constraint":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfFluxBounds:
							switch (qName){
								case "fbc:fluxBound":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfObjectives:
							switch (qName){
								case "fbc:objective":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						default:
							throw new Exception("Unknown section");
					}
				case 4:
					switch (section){
						case SbmlSection.None:
							return;
						case SbmlSection.ListOfUnitDefinitions:
							switch (qName){
								case "listOfUnits":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfCompartments:
							switch (qName){
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfSpecies:
							switch (qName){
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfReactions:
							switch (qName){
								case "listOfReactants":
									inListOfReactants = false;
									return;
								case "listOfProducts":
									return;
								case "kineticLaw":
									return;
								case "listOfModifiers":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfParameters:
							switch (qName){
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfEvents:
							switch (qName){
								case "trigger":
									return;
								case "listOfEventAssignments":
									return;
								case "delay":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfRules:
							switch (qName){
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfFunctionDefinitions:
							switch (qName){
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfTransitions:
							switch (qName){
								case "qual:listOfInputs":
									return;
								case "qual:listOfOutputs":
									return;
								case "qual:listOfFunctionTerms":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfSpeciesTypes:
							switch (qName){
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfConstraints:
							switch (qName){
								case "message":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfObjectives:
							switch (qName){
								case "fbc:listOfFluxObjectives":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						default:
							throw new Exception("Unknown section");
					}
				case 5:
					switch (section){
						case SbmlSection.ListOfUnitDefinitions:
							switch (qName){
								case "unit":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfReactions:
							switch (qName){
								case "speciesReference":
									return;
								case "listOfParameters":
									return;
								case "listOfLocalParameters":
									return;
								case "modifierSpeciesReference":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfParameters:
							switch (qName){
								case "body":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfEvents:
							switch (qName){
								case "eventAssignment":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfRules:
							switch (qName){
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfTransitions:
							switch (qName){
								case "qual:defaultTerm":
									return;
								case "qual:functionTerm":
									return;
								case "qual:input":
									return;
								case "qual:output":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfConstraints:
							switch (qName){
								case "p":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfObjectives:
							switch (qName){
								case "fbc:fluxObjective":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						default:
							throw new Exception("Unknown section");
					}
				case 6:
					switch (section){
						case SbmlSection.ListOfReactions:
							switch (qName){
								case "parameter":
									return;
								case "localParameter":
									return;
								case "stoichiometryMath":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfParameters:
							switch (qName){
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfEvents:
							switch (qName){
								case "eventAssignment":
									return;
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfRules:
							switch (qName){
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						case SbmlSection.ListOfTransitions:
							switch (qName){
								default:
									throw new Exception("Unknown qName: " + qName);
							}
						default:
							throw new Exception("Unknown section");
					}
				case 7:
					switch (section){
						default:
							throw new Exception("Unknown section");
					}
			}
		}

		private void Characters(string val){
			if (inAnnotation){
				return;
			}
			if (inNotes){
				currentNotes.Text.Append(val + "\n");
				return;
			}
			//TODO
			//throw new Exception("Unknown text");
		}
	}
}