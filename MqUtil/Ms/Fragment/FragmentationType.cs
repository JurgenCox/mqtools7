using MqUtil.Mol;
using MqUtil.Ms.Analyzer;
using MqUtil.Ms.Annot;
using MqUtil.Ms.Predict.Rank;
using MqUtil.Ms.Search;
namespace MqUtil.Ms.Fragment{
	public abstract class FragmentationType{
		protected FragmentationType(int index){
			Index = index;
		}
		public int Index{ get; }
		public abstract string Name{ get; }
		public virtual bool UseIntensityPredictionDefault => false;
		public virtual bool UseSequenceBasedModifierDefault => false;
		public virtual bool InternalFragmentsDefault => false;
		public virtual double InternalFragmentWeightDefault => 1.0;
		public virtual string InternalFragmentAasDefault => "KRH";
		public abstract IonSeriesType[] IonSeries{ get; }

		//TODO seq
		public abstract double[] GetQueryMasses(ScoreArgs args, RankPredictionModel ipm, string sequence,
			int[] ntermInds, int[] ctermInds, double[] ntermMass, double[] ctermMass, PeptideModificationInfo fixedMods,
			PeptideModificationState varMods, FragmentationType fragType, int charge, bool calcNeighbors,
			bool includeNeutralLoss, bool secondLoss, bool sequenceBasedMod, double[] fixedMasses,
			char[] fixedMassesAas,
			Modification fixedNtermMod, Modification fixedCtermMod, Dictionary<ushort, Modification2> specialMods,
			out PeakAnnotation[] annotation, out short[] dependent, out short[] ranks, out short[] left,
			out short[] right, MsmsMassAnalyzer ma);
		/// <summary>
		/// Get theoretical peaks
		/// </summary>
		public int GetQueryMassesNoAnnot(ScoreArgs args, RankPredictionModel ipm, string sequence,
			PeptideModificationInfo fixedMods, PeptideModificationState varMods, FragmentationType fragType, int charge,
			bool calcNeighbors, bool includeNeutralLoss, bool secondLoss, bool sequenceBasedMod, double[] fixedMasses,
			char[] fixedMassesAas, Modification fixedNtermMod, Modification fixedCtermMod,
			Dictionary<ushort, Modification2> specialMods, out double[] masses, out short[] dependent,
			out short[] ranks,
			out short[] left, out short[] right, MsmsMassAnalyzer ma){
			return GetQueryMassesNoAnnot(args, ipm, sequence, null, null, null, null,
				fixedMods, varMods, fragType, charge, calcNeighbors, includeNeutralLoss, secondLoss, sequenceBasedMod,
				fixedMasses, fixedMassesAas, fixedNtermMod, fixedCtermMod, specialMods, out masses, out dependent,
				out ranks,
				out left, out right, ma);
		}
		/// <summary>
		/// Get theoretical peaks
		/// </summary>
		public virtual int GetQueryMassesNoAnnot(ScoreArgs args, RankPredictionModel ipm, string sequence,
			int[] ntermInds, int[] ctermInds, double[] ntermMass, double[] ctermMass, PeptideModificationInfo fixedMods,
			PeptideModificationState varMods, FragmentationType fragType, int charge, bool calcNeighbors,
			bool includeNeutralLoss, bool secondLoss, bool sequenceBasedMod, double[] fixedMasses,
			char[] fixedMassesAas,
			Modification fixedNtermMod, Modification fixedCtermMod, Dictionary<ushort, Modification2> specialMods,
			out double[] masses, out short[] dependent, out short[] ranks, out short[] left, out short[] right,
			MsmsMassAnalyzer ma){
			masses = GetQueryMasses(args, ipm, sequence, ntermInds, ctermInds, ntermMass, ctermMass, fixedMods, varMods,
				fragType, charge, calcNeighbors, includeNeutralLoss, secondLoss, sequenceBasedMod, fixedMasses,
				fixedMassesAas,
				fixedNtermMod, fixedCtermMod, specialMods, out PeakAnnotation[] _, out dependent, out ranks, out left,
				out right, ma);
			return masses.Length;
		}
		public override string ToString(){
			return Name;
		}
	}
}