using System.IO.Compression;
using System.Text;
using System.Xml;
using MqUtil.Mol;
using MqUtil.Parse.Reactome.Data;
using MqUtil.Parse.Reactome.Misc;

namespace MqUtil.Parse.Reactome{
	public class ReactomeBiopax2Parser{
		private StringBuilder buffer;
		private ReactomeItemType currentType = ReactomeItemType.Unknown;
		private ReactomeItem currentItem;
		private ReactomeModel model = new ReactomeModel();

		public ReactomeModel Parse(string filename) {
			Stream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
			Stream stream = filename.ToLower().EndsWith(".gz")
				? new GZipStream(fileStream, CompressionMode.Decompress) : fileStream;
			XmlTextReader reader = new XmlTextReader(new StreamReader(stream));
			while (reader.Read()){
				switch (reader.NodeType){
					case XmlNodeType.Element:
						string name = reader.Name;
						Dictionary<string, string> attributes = new Dictionary<string, string>();
						if (reader.HasAttributes){
							for (int i = 0; i < reader.AttributeCount; i++){
								reader.MoveToAttribute(i);
								attributes.Add(reader.Name, reader.Value);
							}
						}
						StartElement(name, attributes);
						break;
					case XmlNodeType.EndElement:
						EndElement(reader.Name);
						break;
					case XmlNodeType.Text:
						Characters(reader.Value, 0, reader.Value.Length);
						break;
				}
			}
			ReactomeModel model1 = model;
			model = new ReactomeModel();
			return model1;
		}

		private void StartElement(string qName, IDictionary<string, string> attrs){
			int ind = qName.IndexOf(':');
			if (ind < 0){
				throw new Exception("Name has no colon.");
			}
			string n1 = qName.Substring(0, ind);
			string n2 = qName.Substring(ind + 1);
			switch (n1){
				case "rdf":
					break;
				case "owl":
					break;
				case "bp":{
					switch (n2){
						case "biochemicalReaction":
							currentType = ReactomeItemType.BiochemicalReaction;
							currentItem = new ReactomeBiochemicalReaction{RdfId = attrs["rdf:ID"]};
							break;
						case "physicalEntityParticipant":
							currentType = ReactomeItemType.PhysicalEntityParticipant;
							currentItem = new ReactomePhysicalEntityParticipant{RdfId = attrs["rdf:ID"]};
							break;
						case "openControlledVocabulary":
							currentType = ReactomeItemType.OpenControlledVocabulary;
							currentItem = new ReactomeOpenControlledVocabulary{RdfId = attrs["rdf:ID"]};
							break;
						case "unificationXref":
							currentType = ReactomeItemType.UnificationXref;
							currentItem = new ReactomeUnificationXref{RdfId = attrs["rdf:ID"]};
							break;
						case "complex":
							currentType = ReactomeItemType.Complex;
							currentItem = new ReactomeComplex{RdfId = attrs["rdf:ID"]};
							break;
						case "sequenceParticipant":
							currentType = ReactomeItemType.SequenceParticipant;
							currentItem = new ReactomeSequenceParticipant{RdfId = attrs["rdf:ID"]};
							break;
						case "protein":
							currentType = ReactomeItemType.Protein;
							currentItem = new ReactomeProtein{RdfId = attrs["rdf:ID"]};
							break;
						case "sequenceInterval":
							currentType = ReactomeItemType.SequenceInterval;
							currentItem = new ReactomeSequenceInterval{RdfId = attrs["rdf:ID"]};
							break;
						case "sequenceFeature":
							currentType = ReactomeItemType.SequenceFeature;
							currentItem = new ReactomeSequenceFeature{RdfId = attrs["rdf:ID"]};
							break;
						case "smallMolecule":
							currentType = ReactomeItemType.SmallMolecule;
							currentItem = new ReactomeSmallMolecule{RdfId = attrs["rdf:ID"]};
							break;
						case "control":
							currentType = ReactomeItemType.Control;
							currentItem = new ReactomeControl{RdfId = attrs["rdf:ID"]};
							break;
						case "dataSource":
							currentType = ReactomeItemType.DataSource;
							break;
						case "bioSource":
							currentType = ReactomeItemType.BioSource;
							currentItem = new ReactomeBioSource{RdfId = attrs["rdf:ID"]};
							break;
						case "dna":
							currentType = ReactomeItemType.Dna;
							currentItem = new ReactomeDna{RdfId = attrs["rdf:ID"]};
							break;
						case "modulation":
							currentType = ReactomeItemType.Modulation;
							currentItem = new ReactomeModulation{RdfId = attrs["rdf:ID"]};
							break;
						case "rna":
							currentType = ReactomeItemType.Rna;
							currentItem = new ReactomeRna{RdfId = attrs["rdf:ID"]};
							break;
						case "sequenceSite":
							currentType = ReactomeItemType.SequenceSite;
							currentItem = new ReactomeSequenceSite{RdfId = attrs["rdf:ID"]};
							break;
						case "relationshipXref":
							currentType = ReactomeItemType.RelationshipXref;
							currentItem = new ReactomeRelationshipXref{RdfId = attrs["rdf:ID"]};
							break;
						case "publicationXref":
							currentType = ReactomeItemType.PublicationXref;
							currentItem = new ReactomePublicationXref{RdfId = attrs["rdf:ID"]};
							break;
						case "pathway":
							currentType = ReactomeItemType.Pathway;
							currentItem = new ReactomePathway{RdfId = attrs["rdf:ID"]};
							break;
						case "physicalEntity":
							currentType = ReactomeItemType.PhysicalEntity;
							currentItem = new ReactomePhysicalEntity{RdfId = attrs["rdf:ID"]};
							break;
						case "pathwayStep":
							currentType = ReactomeItemType.PathwayStep;
							currentItem = new ReactomePathwayStep{RdfId = attrs["rdf:ID"]};
							break;
						case "catalysis":
							currentType = ReactomeItemType.Catalysis;
							currentItem = new ReactomeCatalysis{RdfId = attrs["rdf:ID"]};
							break;
						case "LEFT":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.BiochemicalReaction:
									((ReactomeBiochemicalReaction) currentItem).Left.Add(attrs["rdf:resource"]);
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "RIGHT":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.BiochemicalReaction:
									((ReactomeBiochemicalReaction) currentItem).Right.Add(attrs["rdf:resource"]);
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "EC-NUMBER":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.BiochemicalReaction:
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "NAME":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.BiochemicalReaction:
								case ReactomeItemType.Complex:
								case ReactomeItemType.Protein:
								case ReactomeItemType.DataSource:
								case ReactomeItemType.BioSource:
								case ReactomeItemType.Pathway:
								case ReactomeItemType.SequenceFeature:
								case ReactomeItemType.PhysicalEntity:
								case ReactomeItemType.SmallMolecule:
								case ReactomeItemType.Control:
								case ReactomeItemType.Dna:
								case ReactomeItemType.Modulation:
								case ReactomeItemType.Rna:
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "SHORT-NAME":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.BiochemicalReaction:
								case ReactomeItemType.Complex:
								case ReactomeItemType.Protein:
								case ReactomeItemType.Pathway:
								case ReactomeItemType.PhysicalEntity:
								case ReactomeItemType.SmallMolecule:
								case ReactomeItemType.SequenceFeature:
								case ReactomeItemType.Control:
								case ReactomeItemType.Rna:
								case ReactomeItemType.Dna:
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "XREF":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.BiochemicalReaction:
								case ReactomeItemType.OpenControlledVocabulary:
								case ReactomeItemType.Complex:
								case ReactomeItemType.Protein:
								case ReactomeItemType.Catalysis:
								case ReactomeItemType.Pathway:
								case ReactomeItemType.SmallMolecule:
								case ReactomeItemType.PhysicalEntity:
								case ReactomeItemType.Control:
								case ReactomeItemType.Dna:
								case ReactomeItemType.SequenceFeature:
								case ReactomeItemType.Modulation:
								case ReactomeItemType.Rna:
								case ReactomeItemType.RelationshipXref:
								case ReactomeItemType.UnificationXref:
									((IXrefItem) currentItem).Xrefs.Add(attrs["rdf:resource"]);
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "COMMENT":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.BiochemicalReaction:
								case ReactomeItemType.PhysicalEntityParticipant:
								case ReactomeItemType.SequenceParticipant:
								case ReactomeItemType.Protein:
								case ReactomeItemType.DataSource:
								case ReactomeItemType.UnificationXref:
								case ReactomeItemType.Pathway:
								case ReactomeItemType.PhysicalEntity:
								case ReactomeItemType.SmallMolecule:
								case ReactomeItemType.Control:
								case ReactomeItemType.Rna:
								case ReactomeItemType.Dna:
								case ReactomeItemType.OpenControlledVocabulary:
								case ReactomeItemType.RelationshipXref:
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "DATA-SOURCE":
							break;
						case "PHYSICAL-ENTITY":
							((IParticipantItem) currentItem).PhysicalEntity = attrs["rdf:resource"];
							break;
						case "STOICHIOMETRIC-COEFFICIENT":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.PhysicalEntityParticipant:
									break;
								case ReactomeItemType.SequenceParticipant:
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "TERM":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.OpenControlledVocabulary:
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "DB":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.UnificationXref:
									break;
								case ReactomeItemType.RelationshipXref:
									break;
								case ReactomeItemType.PublicationXref:
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "ID":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.UnificationXref:
									break;
								case ReactomeItemType.RelationshipXref:
									break;
								case ReactomeItemType.PublicationXref:
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "COMPONENTS":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.Complex:
									((ReactomeComplex) currentItem).Components.Add(attrs["rdf:resource"]);
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "ORGANISM":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.Complex:
								case ReactomeItemType.Protein:
								case ReactomeItemType.Pathway:
								case ReactomeItemType.Dna:
								case ReactomeItemType.Rna:
									((IOrganismItem) currentItem).Organism = attrs["rdf:resource"];
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "CELLULAR-LOCATION":
							((IParticipantItem) currentItem).CellularLocation.Add(attrs["rdf:resource"]);
							break;
						case "SYNONYMS":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.Protein:
								case ReactomeItemType.SmallMolecule:
								case ReactomeItemType.BiochemicalReaction:
								case ReactomeItemType.PhysicalEntity:
								case ReactomeItemType.Pathway:
								case ReactomeItemType.Complex:
								case ReactomeItemType.Dna:
								case ReactomeItemType.Rna:
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "TAXON-XREF":
							((ReactomeBioSource) currentItem).TaxonXref = attrs["rdf:resource"];
							break;
						case "ID-VERSION":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.UnificationXref:
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "SEQUENCE-FEATURE-LIST":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.SequenceParticipant:
									((ReactomeSequenceParticipant) currentItem).SequenceFeatureList.Add(attrs["rdf:resource"]);
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "FEATURE-TYPE":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.SequenceFeature:
									((ReactomeSequenceFeature) currentItem).FeatureType = attrs["rdf:resource"];
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "FEATURE-LOCATION":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.SequenceFeature:
									((ReactomeSequenceFeature) currentItem).FeatureLocation = attrs["rdf:resource"];
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "SEQUENCE-INTERVAL-BEGIN":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.SequenceInterval:
									((ReactomeSequenceInterval) currentItem).SequenceIntervalBegin = attrs["rdf:resource"];
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "SEQUENCE-INTERVAL-END":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.SequenceInterval:
									((ReactomeSequenceInterval) currentItem).SequenceIntervalEnd = attrs["rdf:resource"];
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "SEQUENCE-POSITION":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.SequenceSite:
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "POSITION-STATUS":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.SequenceSite:
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "CONTROLLER":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.Catalysis:
								case ReactomeItemType.Control:
								case ReactomeItemType.Modulation:
									((IControlItem) currentItem).Controller = attrs["rdf:resource"];
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "CONTROLLED":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.Catalysis:
								case ReactomeItemType.Control:
								case ReactomeItemType.Modulation:
									((IControlItem) currentItem).Controlled = attrs["rdf:resource"];
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "DIRECTION":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.Catalysis:
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "CONTROL-TYPE":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.Catalysis:
								case ReactomeItemType.Control:
								case ReactomeItemType.Modulation:
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "RELATIONSHIP-TYPE":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.RelationshipXref:
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "YEAR":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.PublicationXref:
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "TITLE":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.PublicationXref:
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "AUTHORS":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.PublicationXref:
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "SOURCE":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.PublicationXref:
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "PATHWAY-COMPONENTS":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.Pathway:
									((ReactomePathway) currentItem).PathwayComponents.Add(attrs["rdf:resource"]);
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "STEP-INTERACTIONS":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.PathwayStep:
									((ReactomePathwayStep) currentItem).StepInteractions.Add(attrs["rdf:resource"]);
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "NEXT-STEP":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.PathwayStep:
									((ReactomePathwayStep) currentItem).NextSteps.Add(attrs["rdf:resource"]);
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "URL":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.PublicationXref:
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "EVIDENCE":
						case "evidence":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.Pathway:
									break;
								case ReactomeItemType.BiochemicalReaction:
									break;
								case ReactomeItemType.UnificationXref:
									break;
								case ReactomeItemType.RelationshipXref:
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "EVIDENCE-CODE":{
							buffer = new StringBuilder();
							switch (currentType){
								case ReactomeItemType.UnificationXref:
									break;
								case ReactomeItemType.RelationshipXref:
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						default:
							throw new Exception("Unknown suffix: " + n2);
					}
				}
					break;
				default:
					throw new Exception("Unknown prefix: " + n1);
			}
		}

		private void EndElement(string qName){
			int ind = qName.IndexOf(':');
			if (ind < 0){
				throw new Exception("Name has no colon.");
			}
			string n1 = qName.Substring(0, ind);
			string n2 = qName.Substring(ind + 1);
			switch (n1){
				case "rdf":
					break;
				case "owl":
					break;
				case "bp":{
					switch (n2){
						case "biochemicalReaction":
						case "physicalEntityParticipant":
						case "openControlledVocabulary":
						case "unificationXref":
						case "complex":
						case "sequenceParticipant":
						case "protein":
						case "sequenceInterval":
						case "sequenceFeature":
						case "smallMolecule":
						case "control":
						case "bioSource":
						case "dna":
						case "modulation":
						case "rna":
						case "sequenceSite":
						case "relationshipXref":
						case "publicationXref":
						case "pathway":
						case "physicalEntity":
						case "pathwayStep":
						case "catalysis":
							model.Add(currentItem);
							break;
						case "LEFT":
						case "RIGHT":
						case "XREF":
						case "DATA-SOURCE":
						case "ORGANISM":
						case "SEQUENCE-INTERVAL-BEGIN":
						case "SEQUENCE-INTERVAL-END":
						case "CONTROLLER":
						case "CONTROLLED":
						case "COMPONENTS":
						case "TAXON-XREF":
						case "PATHWAY-COMPONENTS":
						case "SEQUENCE-FEATURE-LIST":
						case "FEATURE-TYPE":
						case "FEATURE-LOCATION":
						case "STEP-INTERACTIONS":
						case "PHYSICAL-ENTITY":
						case "CELLULAR-LOCATION":
							break;
						case "SEQUENCE-POSITION":
							((ReactomeSequenceSite) currentItem).SequencePosition = int.Parse(buffer.ToString());
							break;
						case "EC-NUMBER":
							((ReactomeBiochemicalReaction) currentItem).EcNumber.Add(buffer.ToString());
							break;
						case "NAME":
							((INamedItem) currentItem).Name = buffer.ToString();
							break;
						case "SHORT-NAME":
							((IShortNamedItem) currentItem).ShortName = buffer.ToString();
							break;
						case "COMMENT":
							((ICommentableItem) currentItem).Comments.Add(buffer.ToString());
							break;
						case "SYNONYMS":
							((ISynonymsItem) currentItem).Synonyms.Add(buffer.ToString());
							break;
						case "CONTROL-TYPE":
							string s = buffer.ToString();
							ReactionControlType controlType;
							switch (s){
								case "ACTIVATION":
									controlType = ReactionControlType.Activation;
									break;
								case "INHIBITION":
									controlType = ReactionControlType.Inhibition;
									break;
								case "INHIBITION-COMPETITIVE":
									controlType = ReactionControlType.InhibitionCompetitive;
									break;
								case "INHIBITION-ALLOSTERIC":
									controlType = ReactionControlType.InhibitionAllosteric;
									break;
								case "ACTIVATION-ALLOSTERIC":
									controlType = ReactionControlType.ActivationAllosteric;
									break;
								case "INHIBITION-NONCOMPETITIVE":
									controlType = ReactionControlType.InhibitionNoncompetitive;
									break;
								default:
									throw new Exception("Unknown IControlItem type: " + s);
							}
							((IControlItem) currentItem).ControlType = controlType;
							break;
						case "DB":
							if (currentItem is ReactomePublicationXref){
								((ReactomePublicationXref)currentItem).Db = GetPublicationDb(buffer.ToString());
							} else if (currentItem is ReactomeRelationshipXref){
								((ReactomeRelationshipXref)currentItem).Db = GetRelationshipDb(buffer.ToString());
							} else {
								((ReactomeUnificationXref)currentItem).Db = GetUnificationDb(buffer.ToString());
							}
							break;
						case "ID":
							((IDbIdItem) currentItem).Id = buffer.ToString();
							break;
						case "YEAR":
							((ReactomePublicationXref) currentItem).Year = int.Parse(buffer.ToString());
							break;
						case "TITLE":
							((ReactomePublicationXref) currentItem).Title = buffer.ToString();
							break;
						case "AUTHORS":
							((ReactomePublicationXref) currentItem).Authors.Add(buffer.ToString());
							break;
						case "SOURCE":
							((ReactomePublicationXref) currentItem).Source = buffer.ToString();
							break;
						case "TERM":
							((ReactomeOpenControlledVocabulary) currentItem).Term = buffer.ToString();
							break;
						case "URL":
							((ReactomePublicationXref) currentItem).Url = buffer.ToString();
							break;
						case "DIRECTION":
							string s1 = buffer.ToString();
							switch (s1){
								case "PHYSIOL-LEFT-TO-RIGHT":
									break;
								default:
									throw new Exception("Unknown direction: " + s1);
							}
							((ReactomeCatalysis) currentItem).Direction = s1;
							break;
						case "ID-VERSION":
							((ReactomeUnificationXref) currentItem).IdVersion = buffer.ToString();
							break;
						case "POSITION-STATUS":
							((ReactomeSequenceSite) currentItem).PositionStatus = buffer.ToString();
							break;
						case "RELATIONSHIP-TYPE":
							((ReactomeRelationshipXref) currentItem).RelationshipType = buffer.ToString();
							break;
						case "NEXT-STEP":
							((ReactomePathwayStep) currentItem).NextSteps.Add(buffer.ToString());
							break;
						case "STOICHIOMETRIC-COEFFICIENT":
							((IParticipantItem) currentItem).StoichiometricCoefficient = int.Parse(buffer.ToString());
							break;
						case "EVIDENCE":
						case "evidence":{
							switch (currentType){
								case ReactomeItemType.Pathway:
									break;
								case ReactomeItemType.BiochemicalReaction:
									break;
								case ReactomeItemType.UnificationXref:
									break;
								case ReactomeItemType.RelationshipXref:
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "EVIDENCE-CODE":{
							switch (currentType){
								case ReactomeItemType.UnificationXref:
									break;
								case ReactomeItemType.RelationshipXref:
									break;
								default:
									throw new Exception("Unknown combination: " + qName + " " + currentType);
							}
						}
							break;
						case "dataSource":
							break;
						default:
							throw new Exception("Unknown suffix: " + n2);
					}
				}
					break;
				default:
					throw new Exception("Unknown prefix: " + n1);
			}
		}

		private static Database GetDb(string s) {
			switch (s) {
				default:
					throw new Exception("Unknown db: " + s);
			}
		}

		private static Database GetPublicationDb(string s) {
			switch (s) {
				case "Pubmed":
					return Database.Pubmed;
				case "ISBN":
					return Database.Isbn;
				default:
					throw new Exception("Unknown Publication db: " + s);
			}
		}

		private static Database GetUnificationDb(string s) {
			switch (s) {
				case "GO":
					return Database.Go;
				case "NCBI Taxonomy":
					return Database.NcbiTaxonomy;
				case "UniProt":
					return Database.UniProt;
				case "Reactome Database ID Release 47":
				case "Reactome":
					return Database.Reactome;
				case "Ensembl":
				case "ENSEMBL":
					return Database.Ensembl;
				case "ChEBI":
					return Database.CheBi;
				case "KEGG Glycan":
					return Database.KeggGlycan;
				case "PubChem Compound":
					return Database.PubChemCompound;
				case "NCBI_Protein":
					return Database.NcbiProtein;
				case "EMBL":
					return Database.Embl;
				case "NCBI Nucleotide":
					return Database.NcbiNucleotide;
				case "miRBase":
					return Database.MirBase;
				case "PRF":
					return Database.Prf;
				default:
					throw new Exception("Unknown Unification db: " + s);
			}
		}

		private static Database GetRelationshipDb(string s) {
			switch (s) {
				case "GO":
					return Database.Go;
				case "ChEBI":
					return Database.CheBi;
				default:
					throw new Exception("Unknown Relationship db: " + s);
			}
		}

		private void Characters(string buf, int offset, int len){
			buffer.Append(buf, offset, len);
		}

		public Dictionary<string, ReactomeModel> ParseFolder(string folder) {
			string[] files = Directory.GetFiles(folder);
			Dictionary<string, ReactomeModel> q = new Dictionary<string, ReactomeModel>();
			foreach (string file in files){
				if (file.ToLower().EndsWith(".owl")){
					ReactomeModel r = Parse(file);
					if (!r.IsEmpty){
						string p = Path.GetFileNameWithoutExtension(file);
						q.Add(p, r);
					}
				}
			}
			return q;
		}
	}
}