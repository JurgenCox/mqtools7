using MqUtil.Parse.Reactome.Data;

namespace MqUtil.Parse.Reactome{
	public class ReactomeModel{
		public readonly Dictionary<string, ReactomePathway> pathways = new Dictionary<string, ReactomePathway>();
		public readonly Dictionary<string, ReactomeComplex> complexes = new Dictionary<string, ReactomeComplex>();
		public readonly Dictionary<string, ReactomeProtein> proteins = new Dictionary<string, ReactomeProtein>();
		public readonly Dictionary<string, ReactomeBiochemicalReaction> biochemicalReactions =
			new Dictionary<string, ReactomeBiochemicalReaction>();
		public readonly Dictionary<string, ReactomeOpenControlledVocabulary> openControlledVocabulary =
			new Dictionary<string, ReactomeOpenControlledVocabulary>();
		public readonly Dictionary<string, ReactomeUnificationXref> unificationXrefs =
			new Dictionary<string, ReactomeUnificationXref>();
		public readonly Dictionary<string, ReactomePublicationXref> publicationXrefs =
			new Dictionary<string, ReactomePublicationXref>();
		public readonly Dictionary<string, ReactomeRelationshipXref> relationshipXrefs =
			new Dictionary<string, ReactomeRelationshipXref>();
		public readonly Dictionary<string, ReactomeSequenceSite> sequenceSites =
			new Dictionary<string, ReactomeSequenceSite>();
		public readonly Dictionary<string, ReactomeSequenceFeature> sequenceFeatures =
			new Dictionary<string, ReactomeSequenceFeature>();
		public readonly Dictionary<string, ReactomeSequenceInterval> sequenceIntervals =
			new Dictionary<string, ReactomeSequenceInterval>();
		public readonly Dictionary<string, ReactomeSequenceParticipant> sequenceParticipants =
			new Dictionary<string, ReactomeSequenceParticipant>();
		public readonly Dictionary<string, ReactomePhysicalEntityParticipant> physicalEntityParticipants =
			new Dictionary<string, ReactomePhysicalEntityParticipant>();
		public readonly Dictionary<string, ReactomePathwayStep> pathwaySteps = new Dictionary<string, ReactomePathwayStep>();
		public readonly Dictionary<string, ReactomeSmallMolecule> smallMolecules =
			new Dictionary<string, ReactomeSmallMolecule>();
		public readonly Dictionary<string, ReactomeCatalysis> catalyses = new Dictionary<string, ReactomeCatalysis>();
		public readonly Dictionary<string, ReactomePhysicalEntity> physicalEntities =
			new Dictionary<string, ReactomePhysicalEntity>();
		public readonly Dictionary<string, ReactomeControl> controls = new Dictionary<string, ReactomeControl>();
		public readonly Dictionary<string, ReactomeBioSource> bioSource = new Dictionary<string, ReactomeBioSource>();
		public readonly Dictionary<string, ReactomeModulation> modulation = new Dictionary<string, ReactomeModulation>();
		public readonly Dictionary<string, ReactomeDna> dna = new Dictionary<string, ReactomeDna>();
		public readonly Dictionary<string, ReactomeRna> rna = new Dictionary<string, ReactomeRna>();
		public bool IsEmpty{
			get{
				if (pathways.Count > 0){
					return false;
				}
				if (biochemicalReactions.Count > 0){
					return false;
				}
				if (proteins.Count > 0){
					return false;
				}
				if (sequenceFeatures.Count > 0){
					return false;
				}
				return true;
			}
		}

		public void Add(ReactomeItem item){
			if (item is ReactomePathway){
				pathways.Add(item.RdfId, (ReactomePathway) item);
				return;
			}
			if (item is ReactomeBiochemicalReaction){
				biochemicalReactions.Add(item.RdfId, (ReactomeBiochemicalReaction) item);
				return;
			}
			if (item is ReactomeComplex){
				complexes.Add(item.RdfId, (ReactomeComplex) item);
				return;
			}
			if (item is ReactomeProtein){
				proteins.Add(item.RdfId, (ReactomeProtein) item);
				return;
			}
			if (item is ReactomeOpenControlledVocabulary){
				openControlledVocabulary.Add(item.RdfId, (ReactomeOpenControlledVocabulary) item);
				return;
			}
			if (item is ReactomeUnificationXref){
				unificationXrefs.Add(item.RdfId, (ReactomeUnificationXref) item);
				return;
			}
			if (item is ReactomeRelationshipXref){
				relationshipXrefs.Add(item.RdfId, (ReactomeRelationshipXref) item);
				return;
			}
			if (item is ReactomePublicationXref){
				publicationXrefs.Add(item.RdfId, (ReactomePublicationXref) item);
				return;
			}
			if (item is ReactomeSequenceSite){
				sequenceSites.Add(item.RdfId, (ReactomeSequenceSite) item);
				return;
			}
			if (item is ReactomeSequenceParticipant){
				sequenceParticipants.Add(item.RdfId, (ReactomeSequenceParticipant) item);
				return;
			}
			if (item is ReactomeSequenceFeature){
				sequenceFeatures.Add(item.RdfId, (ReactomeSequenceFeature) item);
				return;
			}
			if (item is ReactomeSequenceInterval){
				sequenceIntervals.Add(item.RdfId, (ReactomeSequenceInterval) item);
				return;
			}
			if (item is ReactomePhysicalEntityParticipant){
				physicalEntityParticipants.Add(item.RdfId, (ReactomePhysicalEntityParticipant) item);
				return;
			}
			if (item is ReactomePathwayStep){
				pathwaySteps.Add(item.RdfId, (ReactomePathwayStep) item);
				return;
			}
			if (item is ReactomeSmallMolecule){
				smallMolecules.Add(item.RdfId, (ReactomeSmallMolecule) item);
				return;
			}
			if (item is ReactomeCatalysis){
				catalyses.Add(item.RdfId, (ReactomeCatalysis) item);
				return;
			}
			if (item is ReactomePhysicalEntity){
				physicalEntities.Add(item.RdfId, (ReactomePhysicalEntity) item);
				return;
			}
			if (item is ReactomeControl){
				controls.Add(item.RdfId, (ReactomeControl) item);
				return;
			}
			if (item is ReactomeBioSource){
				bioSource.Add(item.RdfId, (ReactomeBioSource) item);
				return;
			}
			if (item is ReactomeModulation){
				modulation.Add(item.RdfId, (ReactomeModulation) item);
				return;
			}
			if (item is ReactomeDna){
				dna.Add(item.RdfId, (ReactomeDna) item);
				return;
			}
			if (item is ReactomeRna){
				rna.Add(item.RdfId, (ReactomeRna) item);
				return;
			}
			throw new Exception("Never get here.");
		}
	}
}