using MqApi.Num;
using MqUtil.Mol;
namespace MqUtil.Ms.Search{
	public class PeptideModificationInfo{
		public double NtermModMass{ get; set; }
		public double CtermModMass{ get; set; }
		public double[] ModMasses{ get; }
		public PeptideModificationInfo(Modification2[] mods, string sequence, bool isNterm, bool isCterm){
			ModMasses = new double[sequence.Length];
			foreach (Modification2 mod in mods){
				ApplyFixedModification(mod, sequence, isNterm, isCterm);
			}
		}
		public PeptideModificationInfo(string sequence, IDictionary<char, ushort> fmods, Modification ntermMod,
			Modification ctermMod){
			PeptideModificationState state = PeptideModificationState.FillFixedModifications(sequence, fmods);
			if (ntermMod != null){
				state.NTermModification = ntermMod.Index;
			}
			if (ctermMod != null){
				state.CTermModification = ctermMod.Index;
			}
			if (state.CTermModification != ushort.MaxValue){
				Modification mod = Tables.ModificationList[state.CTermModification];
				CtermModMass = mod.DeltaMass;
			}
			if (state.NTermModification != ushort.MaxValue){
				Modification mod = Tables.ModificationList[state.NTermModification];
				NtermModMass = mod.DeltaMass;
			}
			ModMasses = new double[state.Length];
			for (int i = 0; i < ModMasses.Length; i++){
				if (state.Modifications[i] != ushort.MaxValue){
					Modification mod = Tables.ModificationList[state.Modifications[i]];
					ModMasses[i] = mod.DeltaMass;
				}
			}
		}
		public PeptideModificationInfo(PeptideModificationState state){
			if (state.CTermModification != ushort.MaxValue){
				Modification mod = Tables.ModificationList[state.CTermModification];
				CtermModMass = mod.DeltaMass;
			}
			if (state.NTermModification != ushort.MaxValue){
				Modification mod = Tables.ModificationList[state.NTermModification];
				NtermModMass = mod.DeltaMass;
			}
			ModMasses = new double[state.Length];
			for (int i = 0; i < ModMasses.Length; i++){
				if (state.Modifications[i] != ushort.MaxValue){
					Modification mod = Tables.ModificationList[state.Modifications[i]];
					ModMasses[i] = mod.DeltaMass;
				}
			}
		}
		private void ApplyFixedModification(Modification2 mod, string sequence, bool isNterm, bool isCterm){
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
						if (ModMasses[j] != 0){
							throw new Exception("Conflicting fixed modifications.");
						}
						ModMasses[j] = mod.DeltaMass;
					}
				}
			}
			if (pos == ModificationPosition.anyNterm){
				if (NtermModMass != 0){
					throw new Exception("Conflicting fixed modifications.");
				}
				NtermModMass = mod.DeltaMass;
			}
			if (pos == ModificationPosition.anyCterm){
				if (CtermModMass != 0){
					throw new Exception("Conflicting fixed modifications.");
				}
				CtermModMass = mod.DeltaMass;
			}
			if (pos == ModificationPosition.proteinNterm && isNterm){
				if (NtermModMass != 0){
					throw new Exception("Conflicting fixed modifications.");
				}
				NtermModMass = mod.DeltaMass;
			}
			if (pos == ModificationPosition.proteinCterm && isCterm){
				if (CtermModMass != 0){
					throw new Exception("Conflicting fixed modifications.");
				}
				CtermModMass = mod.DeltaMass;
			}
		}
		public double GetDeltaMass(){
			return NtermModMass + CtermModMass + ArrayUtils.Sum(ModMasses);
		}
	}
}