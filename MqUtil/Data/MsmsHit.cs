using MqApi.Num;
using MqApi.Util;
using MqUtil.Mol;
using MqUtil.Ms.Analyzer;
using MqUtil.Ms.Annot;
using MqUtil.Ms.Enums;
using MqUtil.Ms.Search;
using MqUtil.Ms.Utils;

namespace MqUtil.Data{
	public class MsmsHit : IDisposable{
		public List<Ms3Hit> Ms3Hits{ get; set; }
		public PeakAnnotation[][] annotationArray;
		public double[] scoresArray;
		public int[] scoreCountsArray;
		public bool[] scoreFinishedArray;
		public PeptideModificationState[] TrueModificationArray{ get; set; }
		public PeptideModificationState[] labelModificationArray;
		public int[] MultipletIndexArray{ get; set; }
		public readonly int fragTypeIndex;
		private readonly int massAnalyzerIndex;
		private readonly QueryType type;
		public EtdSeriesType etdType;
		public EthcdSeriesType ethcdType;
		public Dictionary<ushort, double[]> modProbabilities; //only first
		public Dictionary<ushort, double[]> modScoreDiffs; //only first
		public double[] Intensities{ get; set; }
		public double[] MassDiffs{ get; set; }
		public double[] Masses{ get; set; }
		public int Nloss{ get; set; }
		public double IntensityCoverage { get; set; }
		public double UnfragmentedPrecursorIntensity { get; set; }
		public double UnfragmentedPrecursorFraction { get; set; }
		public double PeakCoverage{ get; set; }
		public int Id{ get; set; }
		public int EvidenceId{ get; set; }
		public double Pep{ get; set; }
		public double RetentionTime{ get; set; }
		public double DeltaScore{ get; set; }
		public double IntensityFractionWindow{ get; set; }
		public double IntensityFractionTotal{ get; set; }
		public double BasePeakFraction{ get; set; }
		public int PrecursorFullScanNumber{ get; set; }
		public double PrecursorIntensity{ get; set; }
		public double PrecursorApexFraction{ get; set; }
		public int PrecursorApexOffset{ get; set; }
		public double PrecursorApexOffsetTime{ get; set; }
		public int ScanNumber{ get; set; }
		public int Ms2Ind{ get; set; }
		public int ScanEventIndex{ get; set; }
		public int RawFileIndex{ get; set; }
		public double Mz{ get; set; }
		public double MonoisotopicMz{ get; set; }
		public ReporterResult ReporterResult{ get; set; }
		public IsotopeCalculationResult IsotopeCalculationResult{ get; set; }
		public AndromedaPeptide[] Peptides{ get; set; }
		public double[] IntensitiesChargeSplit { get; set; } = new double[0];
		public PeakAnnotation[] AnnotationChargeSplit { get; set; } = new PeakAnnotation[0];
		public double[] MassDiffsChargeSplit { get; set; } = new double[0];
		public double[] MassesChargeSplit { get; set; } = new double[0];
		public MsmsHit(AndromedaPeptide[] peptides, int scanNumber, int scanEventIndex, int fileIndex, QueryType type,
			int multipletIndex, double retentionTime, double mz, double monoisotopicMz, object fixedMods,
			byte multiplicity, string[][] labels, int fragTypeIndex, int massAnalyzerIndex,
			double intensityFractionWindow, double intensityFractionTotal, double basePeakFraction,
			int precursorFullScanNumber, double precursorIntensity, double precursorApexFraction,
			int precursorApexOffset, double precursorApexOffsetTime, int ms2Index, IsotopeCalculationResult icr){
			RetentionTime = retentionTime;
			Mz = mz;
			MonoisotopicMz = monoisotopicMz;
			Peptides = peptides;
			Ms3Hits = new List<Ms3Hit>();
			RawFileIndex = fileIndex;
			Ms2Ind = ms2Index;
			IntensityFractionWindow = intensityFractionWindow;
			IntensityFractionTotal = intensityFractionTotal;
			BasePeakFraction = basePeakFraction;
			PrecursorFullScanNumber = precursorFullScanNumber;
			PrecursorIntensity = precursorIntensity;
			PrecursorApexFraction = precursorApexFraction;
			PrecursorApexOffset = precursorApexOffset;
			PrecursorApexOffsetTime = precursorApexOffsetTime;
			ScanNumber = scanNumber;
			ScanEventIndex = scanEventIndex;
			this.fragTypeIndex = fragTypeIndex;
			this.massAnalyzerIndex = massAnalyzerIndex;
			this.type = type;
			Pep = peptides[0].Pep;
			IsotopeCalculationResult = icr;
			labelModificationArray = new PeptideModificationState[peptides.Length];
			TrueModificationArray = new PeptideModificationState[peptides.Length];
			MultipletIndexArray = new int[peptides.Length];
			scoreFinishedArray = new bool[peptides.Length];
			scoreCountsArray = new int[peptides.Length];
			annotationArray = new PeakAnnotation[peptides.Length][];
			scoresArray = new double[peptides.Length];
			for (int i = 0; i < peptides.Length; i++){
				labelModificationArray[i] = GetLabelModifications(peptides[i].Peptide1.Modifications, fixedMods,
					peptides[i].Peptide1.Sequence);
				TrueModificationArray[i] = peptides[i].Peptide1.Modifications.GetTrueModifications();
				MultipletIndexArray[i] = multipletIndex;
				if (multiplicity == 1){
					MultipletIndexArray[i] = 0;
				} else if (type != QueryType.Multiplet){
					AminoAcid[] allAas = MolUtil.CalcAllAas(labels);
					char[] allAaLetts = AminoAcids.GetSingleLetters(allAas);
					foreach (char t in allAaLetts){
						MultipletIndexArray[i] = PeptideModificationState.CalcLabelIndex(t, labelModificationArray[i],
							peptides[i].Peptide1.Sequence, multiplicity, labels);
						if (MultipletIndexArray[i] != -1){
							break;
						}
					}
					if (MultipletIndexArray[i] < 0){
						MultipletIndexArray[i] = PeptideModificationState.CalcLabelIndexNterm(labelModificationArray[i], multiplicity, labels);
					}
					if (MultipletIndexArray[i] < 0){
						MultipletIndexArray[i] = PeptideModificationState.CalcLabelIndexCterm(labelModificationArray[i], multiplicity, labels);
					}
				}
			}
		}

		public int Charge => (int) Math.Round(Peptides[0].Mass / Mz);

		private static PeptideModificationState GetLabelModifications(PeptideModificationState modIn, object labelMods,
			string sequence){
			PeptideModificationState result = modIn.GetFreshCopy(modIn.Length);
			if (modIn.NTermModification != ushort.MaxValue && !Modification.IsStandardVarMod(modIn.NTermModification)){
				result.NTermModification = modIn.NTermModification;
			}
			if (modIn.CTermModification != ushort.MaxValue && !Modification.IsStandardVarMod(modIn.CTermModification)){
				result.CTermModification = modIn.CTermModification;
			}
			for (int i = 0; i < modIn.Length; i++){
				ushort m = modIn.GetModificationAt(i);
				if (m != ushort.MaxValue && !Modification.IsStandardVarMod(m)){
					result.SetModificationAt(i, m);
				} else{
					result.SetModificationAt(i, ushort.MaxValue);
				}
			}
			if (labelMods is ushort[]){
				foreach (ushort labelMod in (ushort[]) labelMods){
					if (!Modification.IsStandardVarMod(labelMod)){
						AddLabelMod(labelMod, modIn, sequence, result);
					}
				}
			} else{
				foreach (string labelMod in (string[]) labelMods){
					if (Tables.Modifications.ContainsKey(labelMod)){
						Modification m = Tables.Modifications[labelMod];
						if (!Modification.IsStandardVarMod(m.Index)){
							AddLabelMod(m.Index, modIn, sequence, result);
						}
					} else{
						Modification2 m = new Modification2(labelMod);
						if (!Modification.IsStandardVarMod(m.Index)){
							AddLabelMod(m, modIn, sequence, result);
						}
					}
				}
			}
			return result;
		}

		private static void AddLabelMod(Modification2 mod, PeptideModificationState modIn, string sequence,
			PeptideModificationState result){
			if (mod.IsInternal){
				for (int j = 0; j < mod.AaCount; j++){
					char c = mod.GetAaAt(j);
					for (int i = 0; i < modIn.Length; i++){
						if (sequence[i] == c){
							result.SetModificationAt(i, mod.Index);
						}
					}
				}
			} else{
				if (mod.IsNterminal){
					result.NTermModification = mod.Index;
				} else{
					result.CTermModification = mod.Index;
				}
			}
		}

		private static void AddLabelMod(ushort labelMod, PeptideModificationState modIn, string sequence,
			PeptideModificationState result){
			Modification mod = Tables.ModificationList[labelMod];
			if (mod.IsInternal){
				for (int j = 0; j < mod.AaCount; j++){
					char c = mod.GetAaAt(j);
					for (int i = 0; i < modIn.Length; i++){
						if (sequence[i] == c){
							result.SetModificationAt(i, mod.Index);
						}
					}
				}
			} else{
				if (mod.IsNterminal){
					result.NTermModification = mod.Index;
				} else{
					result.CTermModification = mod.Index;
				}
			}
		}

		public ReporterResult[] GetReporterResults(){
			ReporterResult[] ms3 = GetMs3ReporterResults();
			if (ms3.Length > 0){
				return ms3;
			}
			return ReporterResult != null ? new[]{ReporterResult} : new ReporterResult[0];
		}

		private ReporterResult[] GetMs3ReporterResults(){
			List<ReporterResult> result = new List<ReporterResult>();
			foreach (Ms3Hit hit in Ms3Hits){
				if (hit.ReporterResult != null){
					result.Add(hit.ReporterResult);
				}
			}
			return result.ToArray();
		}

		public double Score => scoresArray[0];
		public PeptideModificationState TrueModifications => TrueModificationArray[0];

		public int GetModCount(string mod){
			ushort us = Tables.Modifications[mod].Index;
			return TrueModificationArray[0].ProjectToCounts().GetModificationCount(us);
		}

		public MsmsHit(BinaryReader reader){
			Ms3Hits = new List<Ms3Hit>();
			RawFileIndex = reader.ReadInt32();
			ScanNumber = reader.ReadInt32();
			Ms2Ind = reader.ReadInt32();
			ScanEventIndex = reader.ReadInt32();
			MultipletIndexArray = FileUtils.ReadInt32Array(reader);
			fragTypeIndex = reader.ReadInt32();
			massAnalyzerIndex = reader.ReadInt32();
			byte x = reader.ReadByte();
			switch (x){
				case 0:
					type = QueryType.Multiplet;
					break;
				case 1:
					type = QueryType.Isotope;
					break;
				case 2:
					type = QueryType.Peak;
					break;
			}
			x = reader.ReadByte();
			switch (x){
				case 0:
					etdType = EtdSeriesType.Restricted;
					break;
				case 1:
					etdType = EtdSeriesType.RestrictedPlusY;
					break;
				case 2:
					etdType = EtdSeriesType.RestrictedPlusYb;
					break;
				case 3:
					etdType = EtdSeriesType.Complete;
					break;
				case 4:
					etdType = EtdSeriesType.CompletePlusY;
					break;
				case 5:
					etdType = EtdSeriesType.CompletePlusYb;
					break;
				case 6:
					etdType = EtdSeriesType.Unknown;
					break;
			}
			x = reader.ReadByte();
			switch (x){
				case 0:
					ethcdType = EthcdSeriesType.Etd;
					break;
				case 1:
					ethcdType = EthcdSeriesType.Cid;
					break;
				case 2:
					ethcdType = EthcdSeriesType.Unknown;
					break;
			}
			Pep = reader.ReadDouble();
			DeltaScore = reader.ReadDouble();
			IntensityFractionWindow = reader.ReadDouble();
			IntensityFractionTotal = reader.ReadDouble();
			BasePeakFraction = reader.ReadDouble();
			PrecursorFullScanNumber = reader.ReadInt32();
			PrecursorIntensity = reader.ReadDouble();
			PrecursorApexFraction = reader.ReadDouble();
			PrecursorApexOffset = reader.ReadInt32();
			PrecursorApexOffsetTime = reader.ReadDouble();
			int n = reader.ReadInt32();
			labelModificationArray = new PeptideModificationState[n];
			TrueModificationArray = new PeptideModificationState[n];
			for (int i = 0; i < n; i++){
				labelModificationArray[i] = PeptideModificationState.Read(reader);
				TrueModificationArray[i] = PeptideModificationState.Read(reader);
			}
			RetentionTime = reader.ReadDouble();
			Id = reader.ReadInt32();
			Mz = reader.ReadDouble();
			MonoisotopicMz = reader.ReadDouble();
			EvidenceId = reader.ReadInt32();
			DeltaScore = reader.ReadDouble();
			scoreCountsArray = FileUtils.ReadInt32Array(reader);
			scoreFinishedArray = FileUtils.ReadBooleanArray(reader);
			Nloss = reader.ReadInt32();
			IntensityCoverage = reader.ReadDouble();
			UnfragmentedPrecursorIntensity = reader.ReadDouble();
			UnfragmentedPrecursorFraction = reader.ReadDouble();
			PeakCoverage = reader.ReadDouble();
			int len = reader.ReadInt32();
			Peptides = new AndromedaPeptide[len];
			for (int i = 0; i < len; i++){
				Peptides[i] = new AndromedaPeptide(reader);
			}
			scoresArray = FileUtils.ReadDoubleArray(reader);
			n = reader.ReadInt32();
			annotationArray = new PeakAnnotation[n][];
			for (int i = 0; i < n; i++){
				int len1 = reader.ReadInt32();
				annotationArray[i] = new PeakAnnotation[len1];
				for (int j = 0; j < len1; j++){
					int annotType = reader.ReadInt32();
					annotationArray[i][j] = ReadAnnotation(annotType, reader);
				}
			}
			len = reader.ReadInt32();
			Intensities = new double[len];
			MassDiffs = new double[len];
			Masses = new double[len];
			for (int i = 0; i < len; i++){
				Intensities[i] = reader.ReadDouble();
				MassDiffs[i] = reader.ReadDouble();
				Masses[i] = reader.ReadDouble();
			}
			modProbabilities = new Dictionary<ushort, double[]>();
			len = reader.ReadInt32();
			ushort[] keys = new ushort[len];
			for (int i = 0; i < keys.Length; i++){
				keys[i] = reader.ReadUInt16();
			}
			foreach (ushort key in keys){
				len = reader.ReadInt32();
				double[] value = new double[len];
				for (int i = 0; i < value.Length; i++){
					value[i] = reader.ReadDouble();
				}
				modProbabilities.Add(key, value);
			}
			modScoreDiffs = new Dictionary<ushort, double[]>();
			len = reader.ReadInt32();
			keys = new ushort[len];
			for (int i = 0; i < keys.Length; i++){
				keys[i] = reader.ReadUInt16();
			}
			foreach (ushort key in keys){
				len = reader.ReadInt32();
				double[] value = new double[len];
				for (int i = 0; i < value.Length; i++){
					value[i] = reader.ReadDouble();
				}
				modScoreDiffs.Add(key, value);
			}
			bool hasReporter = reader.ReadBoolean();
			if (hasReporter){
				ReporterResult = new ReporterResult(reader);
			}
			bool hasIsoCalc = reader.ReadBoolean();
			if (hasIsoCalc){
				IsotopeCalculationResult = new IsotopeCalculationResult(reader);
			}
			len = reader.ReadInt32();
			for (int i = 0; i < len; i++){
				Ms3Hits.Add(new Ms3Hit(reader));
			}
			IntensitiesChargeSplit = FileUtils.ReadDoubleArray(reader);
			len = reader.ReadInt32();
			AnnotationChargeSplit = new PeakAnnotation[len];
			for (int i = 0; i < len; i++) {
				int annotType = reader.ReadInt32();
				AnnotationChargeSplit[i] = ReadAnnotation(annotType, reader);
			}
			MassDiffsChargeSplit = FileUtils.ReadDoubleArray(reader);
			MassesChargeSplit = FileUtils.ReadDoubleArray(reader);
		}

		private static PeakAnnotation ReadAnnotation(int type, BinaryReader reader){
			if (type == 0){
				return new MsmsPeakAnnotation(reader);
			}
			if (type == 1){
				return new LossPeakAnnotation(reader);
			}
			if (type == 2){
				return new DiagnosticPeakAnnotation(reader);
			}
			throw new Exception("Never get here. " + type);
		}

		private static int GetAnnotType(PeakAnnotation p){
			if (p is MsmsPeakAnnotation){
				return 0;
			}
			if (p is LossPeakAnnotation){
				return 1;
			}
			if (p is DiagnosticPeakAnnotation){
				return 2;
			}
			throw new Exception("Never get here.");
		}

		public long Combinatorics => scoreCountsArray[0];
		public PeakAnnotation[] Annotation => annotationArray[0];

		public double ScoreDiff{
			get{
				if (modScoreDiffs.Count == 0){
					return double.NaN;
				}
				double result = 0;
				foreach (double[] modScoreDiff in modScoreDiffs.Values){
					double max = ArrayUtils.Max(modScoreDiff);
					if (max > result){
						result = max;
					}
				}
				return result;
			}
		}

		public double LocalizationProb{
			get{
				if (modProbabilities.Count == 0){
					return double.NaN;
				}
				double result = 0;
				foreach (double[] a in modProbabilities.Values){
					double max = ArrayUtils.Max(a);
					if (max > result){
						result = max;
					}
				}
				return result;
			}
		}

		public EtdSeriesType EtdType => etdType;



		public MsmsMassAnalyzer GetMassAnalyzer(){
			return MsmsMassAnalyzer.AllMassAnalyzers[massAnalyzerIndex];
		}

		public void Write(BinaryWriter writer){
			if (writer == null){
				return;
			}
			writer.Write(RawFileIndex);
			writer.Write(ScanNumber);
			writer.Write(Ms2Ind);
			writer.Write(ScanEventIndex);
			FileUtils.Write(MultipletIndexArray, writer);
			writer.Write(fragTypeIndex);
			writer.Write(massAnalyzerIndex);
			switch (type){
				case QueryType.Multiplet:
					writer.Write((byte) 0);
					break;
				case QueryType.Isotope:
					writer.Write((byte) 1);
					break;
				case QueryType.Peak:
					writer.Write((byte) 2);
					break;
			}
			switch (etdType){
				case EtdSeriesType.Restricted:
					writer.Write((byte) 0);
					break;
				case EtdSeriesType.RestrictedPlusY:
					writer.Write((byte) 1);
					break;
				case EtdSeriesType.RestrictedPlusYb:
					writer.Write((byte) 2);
					break;
				case EtdSeriesType.Complete:
					writer.Write((byte) 3);
					break;
				case EtdSeriesType.CompletePlusY:
					writer.Write((byte) 4);
					break;
				case EtdSeriesType.CompletePlusYb:
					writer.Write((byte) 5);
					break;
				case EtdSeriesType.Unknown:
					writer.Write((byte) 6);
					break;
			}
			switch (ethcdType){
				case EthcdSeriesType.Etd:
					writer.Write((byte) 0);
					break;
				case EthcdSeriesType.Cid:
					writer.Write((byte) 1);
					break;
				case EthcdSeriesType.Unknown:
					writer.Write((byte) 2);
					break;
			}
			writer.Write(Pep);
			writer.Write(DeltaScore);
			writer.Write(IntensityFractionWindow);
			writer.Write(IntensityFractionTotal);
			writer.Write(BasePeakFraction);
			writer.Write(PrecursorFullScanNumber);
			writer.Write(PrecursorIntensity);
			writer.Write(PrecursorApexFraction);
			writer.Write(PrecursorApexOffset);
			writer.Write(PrecursorApexOffsetTime);
			if (labelModificationArray == null){
				labelModificationArray = new PeptideModificationState[0];
			}
			writer.Write(labelModificationArray.Length);
			for (int index = 0; index < labelModificationArray.Length; index++){
				labelModificationArray[index].Write(writer);
				TrueModificationArray[index].Write(writer);
			}
			writer.Write(RetentionTime);
			writer.Write(Id);
			writer.Write(Mz);
			writer.Write(MonoisotopicMz);
			writer.Write(EvidenceId);
			writer.Write(DeltaScore);
			FileUtils.Write(scoreCountsArray, writer);
			FileUtils.Write(scoreFinishedArray, writer);
			writer.Write(Nloss);
			writer.Write(IntensityCoverage);
			writer.Write(UnfragmentedPrecursorIntensity);
			writer.Write(UnfragmentedPrecursorFraction);
			writer.Write(PeakCoverage);
			if (Peptides == null){
				Peptides = new AndromedaPeptide[0];
			}
			writer.Write(Peptides.Length);
			foreach (AndromedaPeptide t in Peptides){
				t.Write(writer);
			}
			FileUtils.Write(scoresArray, writer);
			if (annotationArray == null){
				annotationArray = new PeakAnnotation[0][];
			}
			writer.Write(annotationArray.Length);
			foreach (PeakAnnotation[] t in annotationArray){
				if (t == null){
					writer.Write(0);
				} else{
					writer.Write(t.Length);
					foreach (PeakAnnotation t1 in t) {
						int annotType = GetAnnotType(t1);
						writer.Write(annotType);
						t1.Write(writer);
					}
				}
			}
			if (Intensities == null){
				Intensities = new double[0];
			}
			writer.Write(Intensities.Length);
			for (int i = 0; i < Intensities.Length; i++){
				writer.Write(Intensities[i]);
				writer.Write(MassDiffs[i]);
				writer.Write(Masses[i]);
			}
			if (modProbabilities == null){
				modProbabilities = new Dictionary<ushort, double[]>();
			}
			ushort[] keys = modProbabilities.Keys.ToArray();
			writer.Write(keys.Length);
			foreach (ushort t in keys){
				writer.Write(t);
			}
			foreach (ushort key in keys){
				double[] value = modProbabilities[key];
				if (value == null){
					writer.Write(0);
				} else{
					writer.Write(value.Length);
					foreach (double t in value) {
						writer.Write(t);
					}
				}
			}
			if (modScoreDiffs == null){
				modScoreDiffs = new Dictionary<ushort, double[]>();
			}
			keys = modScoreDiffs.Keys.ToArray();
			writer.Write(keys.Length);
			foreach (ushort t in keys){
				writer.Write(t);
			}
			foreach (ushort key in keys){
				double[] value = modScoreDiffs[key];
				if (value == null) {
					writer.Write(0);
				} else {
					writer.Write(value.Length);
					foreach (double t in value){
						writer.Write(t);
					}
				}
			}
			bool hasReporter = ReporterResult != null;
			writer.Write(hasReporter);
			if (hasReporter){
				ReporterResult.Write(writer);
			}
			bool hasIsoCalc = IsotopeCalculationResult != null;
			writer.Write(hasIsoCalc);
			if (hasIsoCalc){
				IsotopeCalculationResult.Write(writer);
			}
			if (Ms3Hits == null){
				Ms3Hits = new List<Ms3Hit>();
			}
			writer.Write(Ms3Hits.Count);
			foreach (Ms3Hit hit in Ms3Hits){
				hit.Write(writer);
			}
			FileUtils.Write(IntensitiesChargeSplit, writer);
			writer.Write(AnnotationChargeSplit.Length);
			foreach (PeakAnnotation t1 in AnnotationChargeSplit) {
				int annotType = GetAnnotType(t1);
				writer.Write(annotType);
				t1.Write(writer);
			}
			FileUtils.Write(MassDiffsChargeSplit, writer);
			FileUtils.Write(MassesChargeSplit, writer);
		}

		public PeptideModificationState GetFixedAndLabelModifications(string[] fixedModifications, string sequence){
			return GetFixedAndLabelModifications(fixedModifications, sequence, labelModificationArray[0]);
		}

		public static PeptideModificationState GetFixedAndLabelModifications(string[] fixedModifications,
			string sequence, PeptideModificationState labelMods){
			PeptideModificationState result =
				labelMods != null ? labelMods.Clone() : new PeptideModificationState(sequence.Length);
			Modification[] fixedMods = Tables.ToModifications(fixedModifications);
			ApplyFixedModifications(result, fixedMods, sequence, true, true);
			return result;
		}

		public static void ApplyFixedModifications(PeptideModificationState state, Modification[] mods, string sequence,
			bool isNterm, bool isCterm){
			foreach (Modification mod in mods){
				ApplyFixedModification(state, mod, sequence, isNterm, isCterm);
			}
		}

		public static void ApplyFixedModification(PeptideModificationState state, Modification mod, string sequence,
			bool isNterm, bool isCterm){
			ModificationPosition pos = mod.Position;
			for (int i = 0; i < mod.AaCount; i++){
				for (int j = 0; j < sequence.Length; j++){
					if ((pos == ModificationPosition.notNterm || pos == ModificationPosition.notTerm) && j == 0){
						continue;
					}
					if ((pos == ModificationPosition.notCterm || pos == ModificationPosition.notTerm) &&
					    j == sequence.Length - 1){
						continue;
					}
					if (sequence[j] == mod.GetAaAt(i)){
						if (state.GetModificationAt(j) != ushort.MaxValue){
							throw new Exception("Conflicting fixed modifications.");
						}
						state.SetModificationAt(j, mod.Index);
					}
				}
			}
			if (pos == ModificationPosition.anyNterm){
				if (state.NTermModification != ushort.MaxValue){
					throw new Exception("Conflicting fixed modifications.");
				}
				state.NTermModification = mod.Index;
			}
			if (pos == ModificationPosition.anyCterm){
				if (state.CTermModification != ushort.MaxValue){
					throw new Exception("Conflicting fixed modifications.");
				}
				state.CTermModification = mod.Index;
			}
			if (pos == ModificationPosition.proteinNterm && isNterm){
				if (state.NTermModification != ushort.MaxValue){
					throw new Exception("Conflicting fixed modifications.");
				}
				state.NTermModification = mod.Index;
			}
			if (pos == ModificationPosition.proteinCterm && isCterm){
				if (state.CTermModification != ushort.MaxValue){
					throw new Exception("Conflicting fixed modifications.");
				}
				state.CTermModification = mod.Index;
			}
		}
		public double GetPositionalProbability(ushort mod, int pos){
			return GetPositionalProbability(mod, pos, modProbabilities);
		}

		public static double GetPositionalProbability(ushort mod, int pos, Dictionary<ushort, double[]> modProbabilities) {
			if (!modProbabilities.ContainsKey(mod)) {
				return 0;
			}
			double[] f = modProbabilities[mod];
			if (pos < 0 || pos >= f.Length) {
				return 0;
			}
			return f[pos];
		}

		public double GetScoreDiff(ushort mod, int pos){
			if (!modScoreDiffs.ContainsKey(mod)){
				return double.NaN;
			}
			double[] f = modScoreDiffs[mod];
			if (pos < 0 || pos >= f.Length){
				return double.NaN;
			}
			return f[pos];
		}

		public string GetMultipletStateString(){
			return "" + MultipletIndexArray[0];
		}

		public void Dispose(){
			Intensities = null;
			MassDiffs = null;
			Masses = null;
			if (modProbabilities != null){
				modProbabilities.Clear();
				modProbabilities = null;
			}
			if (modScoreDiffs != null){
				modScoreDiffs.Clear();
				modScoreDiffs = null;
			}
		}

		public bool HasDiagnosticPeak(ushort modType, char aa){
			foreach (PeakAnnotation pa in annotationArray[0]){
				DiagnosticPeakAnnotation dpa = pa as DiagnosticPeakAnnotation;
				if (dpa?.ModInd == modType && dpa.Aa == aa){
					return true;
				}
			}
			return false;
		}

		public bool HasIsobaricLabels(){
			return labelModificationArray[0].HasIsobaricLabels();
		}

		public int[] GetMs3ScanNumbers(){
			if (Ms3Hits == null){
				return new int[0];
			}
			int[] result = new int[Ms3Hits.Count];
			for (int i = 0; i < result.Length; i++){
				result[i] = Ms3Hits[i].ScanNumber;
			}
			return result;
		}
	}
}