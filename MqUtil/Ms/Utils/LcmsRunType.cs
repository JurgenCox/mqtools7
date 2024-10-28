using MqApi.Param;
namespace MqUtil.Ms.Utils{
	public abstract class LcmsRunType{
		public abstract Parameters GetParameters();
		public abstract string Name{ get; }
		public abstract bool UsesLibrary{ get; }
		public abstract bool IsTims{ get; }
		public abstract bool IsFaims{ get; }
		public abstract bool IsDia{ get; }
		public bool IsReporterQuant => IsReporterQuantMs2 || IsReporterQuantMs3;
		public abstract bool IsReporterQuantMs2{ get; }
		public abstract bool IsReporterQuantMs3{ get; }
		public abstract bool IsBoxcar{ get; }
		protected bool Equals(LcmsRunType other){
			return Name.Equals(other.Name);
		}
		public override int GetHashCode(){
			return Name.GetHashCode();
		}
		public override bool Equals(object obj){
			return obj is LcmsRunType && Equals((LcmsRunType) obj);
		}
		public static Parameter CreateMultiplicityParam(string[] labels, bool hasManyMs1Labels)
		{
			string[] multiplicities = hasManyMs1Labels
				? new[] { "1", "2", "3", "4", "5", "6" }
				: new[] { "1", "2", "3" };
			const string multiplicityHelp =
				"Specify here the number of MS1 labels that are quantified against each other. If no labeling is used set this value to" +
				" 1. As an example, for the case of SILAC labeling with lys0/arg0 as light and lys8/arg10 as heavy proteins you have" +
				" to select the value 2 here. Please specify in the boxes below the actual labeling that has been used.";
			SingleChoiceWithSubParams multiplicityParam1 = new SingleChoiceWithSubParams("Multiplicity")
			{
				Help = multiplicityHelp,
				ParamNameWidth = 90,
				TotalWidth = 585,
				Value = 0,
				Values = multiplicities,
				SubParams = new[]{
					new Parameters(new Parameter[]{
						new Ms1LabelParam("Labels", new[]{new int[0]}){
							Multiplicity = 1, Values = labels, Help = labelsHelpText
						}
					}),
					new Parameters(CreateMaxLabeledAasParam(),
						new Ms1LabelParam("Labels", new[]{new int[0], new int[0]}){
							Multiplicity = 2, Values = labels, Help = labelsHelpText
						}),
					new Parameters(CreateMaxLabeledAasParam(),
						new Ms1LabelParam("Labels", new[]{new int[0], new int[0], new int[0]}){
							Multiplicity = 3, Values = labels, Help = labelsHelpText
						}),
					new Parameters(CreateMaxLabeledAasParam(),
						new Ms1LabelParam("Labels", new[]{new int[0], new int[0], new int[0], new int[0]}){
							Multiplicity = 4, Values = labels, Help = labelsHelpText
						}),
					new Parameters(CreateMaxLabeledAasParam(),
						new Ms1LabelParam("Labels", new[]{new int[0], new int[0], new int[0], new int[0], new int[0]}){
							Multiplicity = 5, Values = labels, Help = labelsHelpText
						}),
					new Parameters(CreateMaxLabeledAasParam(),
						new Ms1LabelParam("Labels",
							new[]{new int[0], new int[0], new int[0], new int[0], new int[0], new int[0]}){
							Multiplicity = 6, Values = labels, Help = labelsHelpText
						})
				}
			};
			return multiplicityParam1;
		}
		private const string labelsHelpText = "Specify here the MS1 labels used for each channel.";
		public static IntParam CreateMaxLabeledAasParam()
		{
			return new IntParam("Max. labeled AAs", 3)
			{
				Help = "The maximal number of labeled amino acids that a peptide can have. If the peptide" +
				       " has more labeled amino acids than the number specified here, it will not be recognized as a pair or triplet."
			};
		}

	}
}