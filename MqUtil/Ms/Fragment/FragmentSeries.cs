using MqApi.Num;
using MqUtil.Mol;
using MqUtil.Ms.Annot;
using MqUtil.Ms.Annot.Ions;
using MqUtil.Ms.Search;
using MqUtil.Ms.Utils;
namespace MqUtil.Ms.Fragment{
	public static class FragmentSeries{
		public const double massMatchTol = 1.0e-10;
		private static readonly char[] waterLossAa ={'D', 'E', 'S', 'T'};
		private static readonly char[] nh3LossAa ={'K', 'N', 'Q', 'R'};
		private static readonly object locker = new object();
		public const int annotLen = 200;
		public static PeakAnnotation[] xSeriesAnnot;
		public static PeakAnnotation[] x2SeriesAnnot;
		public static PeakAnnotation[] xSeriesWater;
		public static PeakAnnotation[] xSeriesAmmonia;
		public static PeakAnnotation[] xSeriesLoss;
		public static PeakAnnotation[] xSeriesLoss2;
		public static PeakAnnotation[] ySeriesAnnot;
		public static PeakAnnotation[] y2SeriesAnnot;
		public static PeakAnnotation[] ySeriesWater;
		public static PeakAnnotation[] ySeriesAmmonia;
		public static PeakAnnotation[] ySeriesLoss;
		public static PeakAnnotation[] ySeriesLoss2;
		public static PeakAnnotation[] ySeriesLossWater;
		public static PeakAnnotation[] ySeriesLossAmmonia;
		public static PeakAnnotation[] zSeriesAnnot;
		public static PeakAnnotation[] z2SeriesAnnot;
		public static PeakAnnotation[] zSeriesWater;
		public static PeakAnnotation[] zSeriesAmmonia;
		public static PeakAnnotation[] zSeriesLoss;
		public static PeakAnnotation[] zSeriesLoss2;
		public static PeakAnnotation[] aSeriesAnnot;
		public static PeakAnnotation[] a2SeriesAnnot;
		public static PeakAnnotation[] aSeriesWater;
		public static PeakAnnotation[] aSeriesAmmonia;
		public static PeakAnnotation[] aSeriesLoss;
		public static PeakAnnotation[] aSeriesLoss2;
		public static PeakAnnotation[] bSeriesAnnot;
		public static PeakAnnotation[] b2SeriesAnnot;
		public static PeakAnnotation[] bSeriesWater;
		public static PeakAnnotation[] bSeriesAmmonia;
		public static PeakAnnotation[] bSeriesLoss;
		public static PeakAnnotation[] bSeriesLoss2;
		public static PeakAnnotation[] bSeriesLossWater;
		public static PeakAnnotation[] bSeriesLossAmmonia;
		public static PeakAnnotation[] cSeriesAnnot;
		public static PeakAnnotation[] c2SeriesAnnot;
		public static PeakAnnotation[] cSeriesWater;
		public static PeakAnnotation[] cSeriesAmmonia;
		public static PeakAnnotation[] cSeriesLoss;
		public static PeakAnnotation[] cSeriesLoss2;
		public static PeakAnnotation[] cmSeriesAnnot;
		public static PeakAnnotation[] cpSeriesAnnot;
		public static PeakAnnotation[] zDotSeriesAnnot;
		public static PeakAnnotation[] zPrimeSeriesAnnot;
		public static PeakAnnotation a2Annot;
		public static void CreateAnnot(){
			lock (locker){
				if (ySeriesAnnot != null){
                    return;
                }
                xSeriesAnnot = new PeakAnnotation[annotLen];
                x2SeriesAnnot = new PeakAnnotation[annotLen];
                xSeriesWater = new PeakAnnotation[annotLen];
                xSeriesAmmonia = new PeakAnnotation[annotLen];
                xSeriesLoss = new PeakAnnotation[annotLen];
                xSeriesLoss2 = new PeakAnnotation[annotLen];
                ySeriesAnnot = new PeakAnnotation[annotLen];
                y2SeriesAnnot = new PeakAnnotation[annotLen];
                ySeriesWater = new PeakAnnotation[annotLen];
                ySeriesAmmonia = new PeakAnnotation[annotLen];
                ySeriesLoss = new PeakAnnotation[annotLen];
                ySeriesLoss2 = new PeakAnnotation[annotLen];
				ySeriesLossWater = new PeakAnnotation[annotLen];
				ySeriesLossAmmonia = new PeakAnnotation[annotLen];
                zSeriesAnnot = new PeakAnnotation[annotLen];
                z2SeriesAnnot = new PeakAnnotation[annotLen];
                zSeriesWater = new PeakAnnotation[annotLen];
                zSeriesAmmonia = new PeakAnnotation[annotLen];
                zSeriesLoss = new PeakAnnotation[annotLen];
                zSeriesLoss2 = new PeakAnnotation[annotLen];
                aSeriesAnnot = new PeakAnnotation[annotLen];
                a2SeriesAnnot = new PeakAnnotation[annotLen];
                aSeriesWater = new PeakAnnotation[annotLen];
                aSeriesAmmonia = new PeakAnnotation[annotLen];
                aSeriesLoss = new PeakAnnotation[annotLen];
                aSeriesLoss2 = new PeakAnnotation[annotLen];
                bSeriesAnnot = new PeakAnnotation[annotLen];
                b2SeriesAnnot = new PeakAnnotation[annotLen];
                bSeriesWater = new PeakAnnotation[annotLen];
                bSeriesAmmonia = new PeakAnnotation[annotLen];
                bSeriesLoss = new PeakAnnotation[annotLen];
                bSeriesLoss2 = new PeakAnnotation[annotLen];
				bSeriesLossWater = new PeakAnnotation[annotLen];
				bSeriesLossAmmonia = new PeakAnnotation[annotLen];
                cSeriesAnnot = new PeakAnnotation[annotLen];
                c2SeriesAnnot = new PeakAnnotation[annotLen];
                cSeriesWater = new PeakAnnotation[annotLen];
                cSeriesAmmonia = new PeakAnnotation[annotLen];
                cSeriesLoss = new PeakAnnotation[annotLen];
                cSeriesLoss2 = new PeakAnnotation[annotLen];
                cmSeriesAnnot = new PeakAnnotation[annotLen];
                cpSeriesAnnot = new PeakAnnotation[annotLen];
                zDotSeriesAnnot = new PeakAnnotation[annotLen];
                zPrimeSeriesAnnot = new PeakAnnotation[annotLen];
				for (int i = 0; i < annotLen; i++){
                    xSeriesAnnot[i] = new MsmsPeakAnnotation(new XIon(), i + 1, 1, 0, 0);
                    x2SeriesAnnot[i] = new MsmsPeakAnnotation(new XIon(), i + 1, 2, 0, 0);
                    xSeriesWater[i] = new LossPeakAnnotation(xSeriesAnnot[i], MolUtil.waterLossLib);
                    xSeriesAmmonia[i] = new LossPeakAnnotation(xSeriesAnnot[i], MolUtil.ammoniaLossLib);
                    xSeriesLoss[i] = new MsmsPeakAnnotation(new XIon(), i + 1, 1, 0, 1);
                    xSeriesLoss2[i] = new MsmsPeakAnnotation(new XIon(), i + 1, 1, 0, 2);
                    ySeriesAnnot[i] = new MsmsPeakAnnotation(new YIon(), i + 1, 1, 0, 0);
                    y2SeriesAnnot[i] = new MsmsPeakAnnotation(new YIon(), i + 1, 2, 0, 0);
                    ySeriesWater[i] = new LossPeakAnnotation(ySeriesAnnot[i], MolUtil.waterLossLib);
                    ySeriesAmmonia[i] = new LossPeakAnnotation(ySeriesAnnot[i], MolUtil.ammoniaLossLib);
                    ySeriesLoss[i] = new MsmsPeakAnnotation(new YIon(), i + 1, 1, 0, 1);
                    ySeriesLoss2[i] = new MsmsPeakAnnotation(new YIon(), i + 1, 1, 0, 2);
					ySeriesLossWater[i] = new LossPeakAnnotation(ySeriesLoss[i], MolUtil.waterLossLib);
					ySeriesLossAmmonia[i] = new LossPeakAnnotation(ySeriesLoss[i], MolUtil.ammoniaLossLib);
                    zSeriesAnnot[i] = new MsmsPeakAnnotation(new ZIon(), i + 1, 1, 0, 0);
                    z2SeriesAnnot[i] = new MsmsPeakAnnotation(new ZIon(), i + 1, 2, 0, 0);
                    zSeriesWater[i] = new LossPeakAnnotation(zSeriesAnnot[i], MolUtil.waterLossLib);
                    zSeriesAmmonia[i] = new LossPeakAnnotation(zSeriesAnnot[i], MolUtil.ammoniaLossLib);
                    zSeriesLoss[i] = new MsmsPeakAnnotation(new ZIon(), i + 1, 1, 0, 1);
                    zSeriesLoss2[i] = new MsmsPeakAnnotation(new ZIon(), i + 1, 1, 0, 2);
                    aSeriesAnnot[i] = new MsmsPeakAnnotation(new AIon(), i + 1, 1, 0, 0);
                    a2SeriesAnnot[i] = new MsmsPeakAnnotation(new AIon(), i + 1, 2, 0, 0);
                    aSeriesWater[i] = new LossPeakAnnotation(aSeriesAnnot[i], MolUtil.waterLossLib);
                    aSeriesAmmonia[i] = new LossPeakAnnotation(aSeriesAnnot[i], MolUtil.ammoniaLossLib);
                    aSeriesLoss[i] = new MsmsPeakAnnotation(new AIon(), i + 1, 1, 0, 1);
                    aSeriesLoss2[i] = new MsmsPeakAnnotation(new AIon(), i + 1, 1, 0, 2);
                    bSeriesAnnot[i] = new MsmsPeakAnnotation(new BIon(), i + 1, 1, 0, 0);
                    b2SeriesAnnot[i] = new MsmsPeakAnnotation(new BIon(), i + 1, 2, 0, 0);
                    bSeriesWater[i] = new LossPeakAnnotation(bSeriesAnnot[i], MolUtil.waterLossLib);
                    bSeriesAmmonia[i] = new LossPeakAnnotation(bSeriesAnnot[i], MolUtil.ammoniaLossLib);
                    bSeriesLoss[i] = new MsmsPeakAnnotation(new BIon(), i + 1, 1, 0, 1);
                    bSeriesLoss2[i] = new MsmsPeakAnnotation(new BIon(), i + 1, 1, 0, 2);
					bSeriesLossWater[i] = new LossPeakAnnotation(bSeriesLoss[i], MolUtil.waterLossLib);
					bSeriesLossAmmonia[i] = new LossPeakAnnotation(bSeriesLoss[i], MolUtil.ammoniaLossLib);
                    cSeriesAnnot[i] = new MsmsPeakAnnotation(new CIon(), i + 1, 1, 0, 0);
                    c2SeriesAnnot[i] = new MsmsPeakAnnotation(new CIon(), i + 1, 2, 0, 0);
                    cSeriesWater[i] = new LossPeakAnnotation(cSeriesAnnot[i], MolUtil.waterLossLib);
                    cSeriesAmmonia[i] = new LossPeakAnnotation(cSeriesAnnot[i], MolUtil.ammoniaLossLib);
                    cSeriesLoss[i] = new MsmsPeakAnnotation(new CIon(), i + 1, 1, 0, 1);
                    cSeriesLoss2[i] = new MsmsPeakAnnotation(new CIon(), i + 1, 1, 0, 2);
                    cmSeriesAnnot[i] = new MsmsPeakAnnotation(new CmIon(), i + 1, 1, 0, 0);
                    cpSeriesAnnot[i] = new MsmsPeakAnnotation(new CpIon(), i + 1, 1, 0, 0);
                    zDotSeriesAnnot[i] = new MsmsPeakAnnotation(new ZDotIon(), i + 1, 1, 0, 0);
                    zPrimeSeriesAnnot[i] = new MsmsPeakAnnotation(new ZPrimeIon(), i + 1, 1, 0, 0);
                }
                a2Annot = new MsmsPeakAnnotation(new AIon(), 2, 1, 0, 0);
            }
        }
        public static double[] GetNtermMasses(string sequence, PeptideModificationInfo fixedMods,
			PeptideModificationState varMods){
			return GetNtermMasses(sequence, fixedMods, varMods, null);
		}
		public static double[] GetNtermMasses(string sequence, PeptideModificationInfo fixedMods,
			PeptideModificationState varMods, Dictionary<ushort, Modification2> specialMods){
			return GetNtermMasses(sequence, null, null, out int _, out int _, fixedMods, varMods, new double[0],
				new char[0], null, null, specialMods);
		}
		public static double[] GetNtermMasses(string sequence, int[] inds, double[] dm, out int fiveInd, out int oneInd,
			PeptideModificationInfo fixedMods, PeptideModificationState varMods, double[] fixedMasses,
			char[] fixedMassesAa, Modification fixedNtermMod, Modification fixedCtermMod,
			Dictionary<ushort, Modification2> specialMods){
			double m = Molecule.massH + Molecule.massProton + fixedMods.NtermModMass;
			if (varMods.NTermModification != ushort.MaxValue){
				Modification mod = Tables.ModificationList[varMods.NTermModification];
				m += mod.DeltaMass;
				if (fixedNtermMod != null){
					m -= fixedNtermMod.DeltaMass;
				}
			}
			int n = sequence.Length - 1;
			double[] masses = new double[n];
			ushort[] vms = varMods.Modifications;
			fiveInd = -1;
			oneInd = -1;
			for (int i = 0; i < n; i++){
				int bs = -1;
				if (inds != null){
					bs = Array.BinarySearch(inds, i);
					if (bs < 0){
						continue;
					}
				}
				if (i >= 5 && fiveInd == -1){
					fiveInd = inds != null ? bs : i;
				}
				if (i == 1 && oneInd == -1){
					oneInd = inds != null ? bs : i;
				}
				char s = sequence[i];
				m += AminoAcids.AaMonoMasses[s];
				double fm = fixedMods.ModMasses[i];
				m += fm;
				ushort vm = vms[i];
				if (vm != ushort.MaxValue){
					double newTerminusMass = 0;
					if (vm < Tables.ModificationList.Length){
						if (Tables.ModificationList[vm].ModificationType == ModificationType.SequenceBasedModifier){
							if (fixedNtermMod != null){
								newTerminusMass = fixedNtermMod.DeltaMass;
							} else{
								ushort nt = varMods.NTermModification;
								if (nt != ushort.MaxValue){
									Modification mx = Tables.ModificationList[nt];
									if (!Modification.IsStandardVarMod(mx.ModificationType)){
										newTerminusMass = mx.DeltaMass;
									}
								}
							}
						}
					}
					m += GetDeltaMass(vm, specialMods) + newTerminusMass;
				}
				if (fm != 0 && vm != ushort.MaxValue){
					if (!IsIsotopicMod(vm, specialMods)){
						int ind = Array.BinarySearch(fixedMassesAa, s);
						if (ind >= 0){
							m -= fixedMasses[ind];
						}
					}
				}
				masses[i] = m;
				if (inds != null){
					masses[i] += dm[bs];
					if (dm[bs] < 0){
						masses[i] = 0;
					}
				}
			}
			if (inds != null){
				masses = ArrayUtils.SubArray(masses, inds);
			}
			return masses;
		}
		public static double[] GetCtermMasses(string sequence, PeptideModificationInfo fixedMods,
			PeptideModificationState varMods){
			return GetCtermMasses(sequence, fixedMods, varMods, null);
		}
		public static double[] GetCtermMasses(string sequence, PeptideModificationInfo fixedMods,
			PeptideModificationState varMods, Dictionary<ushort, Modification2> specialMods){
			return GetCtermMasses(sequence, null, null, out int _, fixedMods, varMods, new double[0], new char[0], null,
				null, specialMods);
		}
		public static double[] GetCtermMasses(string sequence, int[] inds, double[] dm, out int fiveInd,
			PeptideModificationInfo fixedMods, PeptideModificationState varMods, double[] fixedMasses,
			char[] fixedMassesAa, Modification fixedNtermMod, Modification fixedCtermMod,
			Dictionary<ushort, Modification2> specialMods){
			double m = Molecule.massO + Molecule.massH + Molecule.massProton + fixedMods.CtermModMass;
			if (varMods.CTermModification != ushort.MaxValue){
				Modification mod = Tables.ModificationList[varMods.CTermModification];
				m += mod.DeltaMass;
				if (fixedCtermMod != null){
					m -= fixedCtermMod.DeltaMass;
				}
			}
			int n = sequence.Length - 1;
			double[] masses = new double[n];
			ushort[] vms = varMods.Modifications;
			fiveInd = -1;
			for (int i = 0; i < n; i++){
				int bs = -1;
				if (inds != null){
					bs = Array.BinarySearch(inds, i);
					if (bs < 0){
						continue;
					}
				}
				if (i >= 5 && fiveInd == -1){
					fiveInd = inds != null ? bs : i;
				}
				char s = sequence[n - i];
				m += AminoAcids.AaMonoMasses[s];
				double fm = fixedMods.ModMasses[n - i];
				m += fm;
				ushort vm = vms[n - i];
				if (vm != ushort.MaxValue){
					double newTerminusMass = 0;
					if (vm < Tables.ModificationList.Length){
						if (Tables.ModificationList[vm].ModificationType == ModificationType.SequenceBasedModifier){
							if (fixedNtermMod != null){
								newTerminusMass = fixedNtermMod.DeltaMass;
							} else{
								ushort nt = varMods.NTermModification;
								if (nt != ushort.MaxValue){
									Modification mx = Tables.ModificationList[nt];
									if (!Modification.IsStandardVarMod(mx.ModificationType)){
										newTerminusMass = mx.DeltaMass;
									}
								}
							}
						}
					}
					m += GetDeltaMass(vm, specialMods) + newTerminusMass;
				}
				if (fm != 0 && vm != ushort.MaxValue){
					if (!IsIsotopicMod(vm, specialMods)){
						int ind = Array.BinarySearch(fixedMassesAa, s);
						if (ind >= 0){
							m -= fixedMasses[ind];
						}
					}
				}
				masses[i] = m;
				if (inds != null){
					masses[i] += dm[bs];
					if (dm[bs] < 0){
						masses[i] = 0;
					}
				}
			}
			if (inds != null){
				masses = ArrayUtils.SubArray(masses, inds);
			}
			return masses;
		}
		public static double[] GetInternalMasses(string sequence, HashSet<char> aasContained, int minLen,
			PeptideModificationInfo fixedMods, PeptideModificationState varMods, double[] fixedMasses,
			char[] fixedMassesAa, Modification fixedNtermMod, Modification fixedCtermMod,
			Dictionary<ushort, Modification2> specialMods, out int[] pos1, out int[] pos2){
			int l = sequence.Length;
			bool requireAas = aasContained != null && aasContained.Count > 0;
			int toInd = l - 1 - minLen;
			int firstRequiredPos = -1;
			if (requireAas){
				bool[] reqiredAaPositions = new bool[l];
				for (int i = 0; i < reqiredAaPositions.Length; i++){
					reqiredAaPositions[i] = aasContained.Contains(sequence[i]);
				}
				int lastRequiredPos = -1;
				for (int i = 0; i < reqiredAaPositions.Length; i++){
					if (reqiredAaPositions[i]){
						firstRequiredPos = i;
						break;
					}
				}
				for (int i = reqiredAaPositions.Length - 1; i >= 0; i--){
					if (reqiredAaPositions[i]){
						lastRequiredPos = i;
						break;
					}
				}
				toInd = Math.Min(toInd, lastRequiredPos - 1);
			}
			double m0 = Molecule.massO + 2 * Molecule.massH + Molecule.massProton;
			double[] ms = new double[l - 2];
			ushort[] vms = varMods.Modifications;
			for (int i = 1; i < l - 1; i++){
				char s = sequence[i];
				double m = AminoAcids.AaMonoMasses[s];
				double fm = fixedMods.ModMasses[i];
				m += fm;
				ushort vm = vms[i];
				if (vm != ushort.MaxValue){
					double newTerminusMass = 0;
					if (vm < Tables.ModificationList.Length){
						if (Tables.ModificationList[vm].ModificationType == ModificationType.SequenceBasedModifier){
							if (fixedNtermMod != null){
								newTerminusMass = fixedNtermMod.DeltaMass;
							} else{
								ushort nt = varMods.NTermModification;
								if (nt != ushort.MaxValue){
									Modification mx = Tables.ModificationList[nt];
									if (!Modification.IsStandardVarMod(mx.ModificationType)){
										newTerminusMass = mx.DeltaMass;
									}
								}
							}
						}
					}
					m += GetDeltaMass(vm, specialMods) + newTerminusMass;
				}
				if (fm != 0 && vm != ushort.MaxValue){
					if (!IsIsotopicMod(vm, specialMods)){
						int ind = Array.BinarySearch(fixedMassesAa, s);
						if (ind >= 0){
							m -= fixedMasses[ind];
						}
					}
				}
				ms[i - 1] = m;
			}
			List<double> masses = new List<double>();
			List<int> inds1 = new List<int>();
			List<int> inds2 = new List<int>();
			for (int i1 = 0; i1 < toInd; i1++){
				int fromInd = i1 + minLen;
				fromInd = Math.Max(fromInd, firstRequiredPos);
				for (int i2 = fromInd; i2 < l - 1; i2++){
					string subSeq = sequence.Substring(i1 + 1, i2 - i1);
					if (!requireAas || Contains(subSeq, aasContained)){
						double mx = m0 + Sum(ms, i1, i2);
						masses.Add(mx);
						inds1.Add(i1);
						inds2.Add(i2);
					}
				}
			}
			int[] o = ArrayUtils.Order(masses);
			List<int> valids = new List<int>();
			if (masses.Count > 0){
				valids.Add(o[0]);
			}
			for (int i = 1; i < masses.Count; i++){
				if (Math.Abs(masses[o[i]] - masses[o[i - 1]]) > massMatchTol){
					valids.Add(o[i]);
				}
			}
			pos1 = ArrayUtils.SubArray(inds1, valids);
			pos2 = ArrayUtils.SubArray(inds2, valids);
			return ArrayUtils.SubArray(masses, valids);
		}
		private static double Sum(double[] ms, int i1, int i2){
			double sum = 0;
			for (int i = i1; i < i2; i++){
				sum += ms[i];
			}
			return sum;
		}
		private static bool Contains(string subSeq, HashSet<char> aasContained){
			foreach (char c in subSeq){
				if (aasContained.Contains(c)){
					return true;
				}
			}
			return false;
		}
		public static double GetDeltaMass(ushort vm, Dictionary<ushort, Modification2> specialMods){
			if (vm < Tables.ModificationList.Length){
				Modification mod = Tables.ModificationList[vm];
				return mod.DeltaMass;
			}
			if (specialMods.ContainsKey(vm)){
				return specialMods[vm].DeltaMass;
			}
			throw new Exception("Cannot find special modification.");
		}
		public static bool IsIsotopicMod(ushort vm, Dictionary<ushort, Modification2> specialMods){
			if (vm < Tables.ModificationList.Length){
				Modification mod = Tables.ModificationList[vm];
				return mod.IsIsotopicMod;
			}
			if (specialMods.ContainsKey(vm)){
				return specialMods[vm].IsIsotopicMod;
			}
			throw new Exception("Cannot find special modification.");
		}
		public static int GetDiagnosticMasses(string sequence, PeptideModificationState varMods, out double[] masses,
			out PeakAnnotation[] annot, bool sequenceBasedMod){
			int n = sequence.Length;
			List<double> masses1 = new List<double>();
			List<PeakAnnotation> annot1 = new List<PeakAnnotation>();
			ushort[] vms = varMods.Modifications;

			//TODO: not updated?
			for (int i = 0; i < n; i++){
				ushort vm = vms[i];
				if (vm != ushort.MaxValue){
					Modification mod = Tables.ModificationList[vm];
					char c = sequence[i];
					ModificationSite s = mod.GetSite(c);
					if (s.HasDiagnosticPeak){
						PeakAnnotation[] an = GetDiagnosticAnnotations(vm, sequence[i], s.diagnostic_collection);
						for (int j = 0; j < s.diagnostic_collection.Length; j++){
							double mz = s.diagnostic_collection[j].Mass + Molecule.massProton;
							masses1.Add(mz);
							annot1.Add(an[j]);
						}
					}
				}
			}
			annot = annot1.ToArray();
			masses = masses1.ToArray();
			return masses.Length;
		}
		public static void AddDiagnosticMassesNoAnnot(ref int pos, double[] masses, short[] dependents,
			bool calcNeighbors, short[] left, short[] right, string sequence, PeptideModificationState varMods,
			bool sequenceBasedMod){
			int n = sequence.Length;
			ushort[] vms = varMods.Modifications;
			HashSet<char> taken = new HashSet<char>();
			for (int i = 0; i < n; i++){
				ushort vm = vms[i];
				if (vm != ushort.MaxValue){
					if (vm >= Tables.ModificationList.Length){
						continue;
					}
					Modification mod = Tables.ModificationList[vm];
					char c = sequence[i];
					if (taken.Contains(c)){
						continue;
					}
					taken.Add(c);
					ModificationSite s = mod.GetSite(c);
					if (s.HasDiagnosticPeak){
						foreach (DiagnosticPeak t in s.diagnostic_collection){
							double mz = t.Mass + Molecule.massProton;
							masses[pos] = mz;
							dependents[pos] = -1;
							if (calcNeighbors){
								left[pos] = -1;
								right[pos] = -1;
							}
							pos++;
						}
					}
				}
			}
		}
        private static Dictionary<ushort, Dictionary<char, PeakAnnotation[]>> dpaCache;
		private static PeakAnnotation[] GetDiagnosticAnnotations(ushort modInd, char aa,
			IList<DiagnosticPeak> diagnostic){
			if (dpaCache == null){
				dpaCache = new Dictionary<ushort, Dictionary<char, PeakAnnotation[]>>();
			}
			if (!dpaCache.ContainsKey(modInd)){
				dpaCache.Add(modInd, new Dictionary<char, PeakAnnotation[]>());
			}
			if (!dpaCache[modInd].ContainsKey(aa)){
				PeakAnnotation[] annots = new PeakAnnotation[diagnostic.Count];
				for (int i = 0; i < annots.Length; i++){
					annots[i] = new DiagnosticPeakAnnotation(diagnostic[i], diagnostic[i].Mass + Molecule.massProton,
						modInd, aa);
				}
				dpaCache[modInd].Add(aa, annots);
			}
			return dpaCache[modInd][aa];
		}
		public static double[] GetNeutralLossBSeries(string sequence, PeptideModificationInfo fixedMods,
			PeptideModificationState varMods, out PeakAnnotation[] peakAnnot,
			Dictionary<ushort, Modification2> specialMods){
			double[] result = new double[sequence.Length - 1];
			ModificationSite site = null;
			int neutralLossIndex = -1;
			ushort[] vms = varMods.Modifications;
			for (int i = 0; i < sequence.Length - 1; i++){
				ushort vm = vms[i];
				if (vm != ushort.MaxValue){
					Modification mod = Tables.ModificationList[vm];
					ModificationSite index = mod.GetSite(sequence[i]);
					if (index != null && index.HasNeutralLoss){
						site = index;
						neutralLossIndex = i;
						break;
					}
				}
			}
			double[] b = GetBSeries(sequence, fixedMods, varMods, out peakAnnot, specialMods);
			peakAnnot = new MsmsPeakAnnotation[result.Length];
			if (neutralLossIndex != -1 && site != null){
				double loss = site.neutralloss_collection[0].DeltaMass;
				for (int i = neutralLossIndex; i < sequence.Length - 1; i++){
					result[i] = b[i] - loss;
					peakAnnot[i] = new MsmsPeakAnnotation(new BIon(), i + 1, 1, result[i], 1);
				}
			}
			return result;
		}
		public static double[] GetNeutralLossYSeries(string sequence, PeptideModificationInfo fixedMods,
			PeptideModificationState varMods, out PeakAnnotation[] peakAnnot,
			Dictionary<ushort, Modification2> specialMods){
			double[] result = new double[sequence.Length - 1];
			ModificationSite site = null;
			int neutralLossIndex = -1;
			ushort[] vms = varMods.Modifications;
			for (int i = sequence.Length - 1; i >= 1; i--){
				ushort vm = vms[i];
				if (vm != ushort.MaxValue){
					Modification mod = Tables.ModificationList[vm];
					ModificationSite index = mod.GetSite(sequence[i]);
					if (index != null && index.HasNeutralLoss){
						site = index;
						neutralLossIndex = sequence.Length - 1 - i;
						break;
					}
				}
			}
			double[] y = GetYSeries(sequence, fixedMods, varMods, out peakAnnot, specialMods);
			peakAnnot = new PeakAnnotation[result.Length];
			if (neutralLossIndex != -1 && site != null){
				double loss = site.neutralloss_collection[0].DeltaMass;
				for (int i = neutralLossIndex; i < sequence.Length - 1; i++){
					result[i] = y[i] - loss;
					peakAnnot[i] = new MsmsPeakAnnotation(new YIon(), i + 1, 1, result[i], 1);
				}
			}
			return result;
		}
		public static double[] GetNeutralLossXSeries(string sequence, PeptideModificationInfo fixedMods,
			PeptideModificationState varMods, out PeakAnnotation[] des, Dictionary<ushort, Modification2> specialMods){
			double[] result = new double[sequence.Length - 1];
			ModificationSite site = null;
			int neutralLossIndex = -1;
			ushort[] vms = varMods.Modifications;
			for (int i = result.Length - 1; i >= 0; i--){
				ushort vm = vms[i];
				if (vm != ushort.MaxValue){
					Modification mod = Tables.ModificationList[vm];
					ModificationSite index = mod.GetSite(sequence[i]);
					if (index != null && index.HasNeutralLoss){
						site = index;
						neutralLossIndex = sequence.Length - 1 - i;
						break;
					}
				}
			}
			double[] x = GetXSeries(sequence, fixedMods, varMods, out des, specialMods);
			des = new PeakAnnotation[result.Length];
			if (neutralLossIndex != -1 && site != null){
				double loss = site.neutralloss_collection[0].DeltaMass;
				for (int i = neutralLossIndex; i < sequence.Length - 1; i++){
					result[i] = x[i] - loss;
					des[i] = new MsmsPeakAnnotation(new XIon(), i + 1, 1, result[i], 1);
				}
			}
			return result;
		}
		public static double[] GetASeries(double[] ntermSeries, out PeakAnnotation[] des){
			double[] result = new double[ntermSeries.Length];
			des = new MsmsPeakAnnotation[ntermSeries.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = ntermSeries[i] + AminoAcids.aIonMassOffset;
				des[i] = new MsmsPeakAnnotation(new AIon(), i + 1, 1, result[i], 0);
			}
			return result;
		}
		public static double[] GetASeries(double[] ntermSeries){
			double[] result = new double[ntermSeries.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = ntermSeries[i] + AminoAcids.aIonMassOffset;
			}
			return result;
		}
		public static double[] GetASeries(string sequence, PeptideModificationInfo fixedMods,
			PeptideModificationState varMods, out PeakAnnotation[] des, Dictionary<ushort, Modification2> specialMods){
			return GetASeries(GetNtermMasses(sequence, fixedMods, varMods, specialMods), out des);
		}
		public static double[] GetBSeries(double[] ntermSeries, out PeakAnnotation[] des){
			double[] result = new double[ntermSeries.Length];
			des = new MsmsPeakAnnotation[ntermSeries.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = ntermSeries[i] + AminoAcids.bIonMassOffset;
				des[i] = new MsmsPeakAnnotation(new BIon(), i + 1, 1, result[i], 0);
			}
			return result;
		}
		public static double[] GetBSeries(IList<double> ntermSeries){
			double[] result = new double[ntermSeries.Count];
			for (int i = 0; i < result.Length; i++){
				result[i] = ntermSeries[i] + AminoAcids.bIonMassOffset;
			}
			return result;
		}
		public static double[] GetBSeries(string sequence, PeptideModificationInfo fixedMods,
			PeptideModificationState varMods, out PeakAnnotation[] des, Dictionary<ushort, Modification2> specialMods){
			return GetBSeries(GetNtermMasses(sequence, fixedMods, varMods, specialMods), out des);
		}
		public static double[] GetCSeries(IList<double> ntermSeries){
			double[] result = new double[ntermSeries.Count];
			for (int i = 0; i < result.Length; i++){
				result[i] = ntermSeries[i] + AminoAcids.cIonMassOffset;
			}
			return result;
		}
		public static double[] GetCmSeries(IList<double> ntermSeries){
			double[] result = new double[ntermSeries.Count];
			for (int i = 0; i < result.Length; i++){
				result[i] = ntermSeries[i] + AminoAcids.cIonMassOffset - Molecule.massH;
			}
			return result;
		}
		public static double[] GetCSeries(double[] ntermSeries, out PeakAnnotation[] des){
			double[] result = new double[ntermSeries.Length];
			des = new MsmsPeakAnnotation[ntermSeries.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = ntermSeries[i] + AminoAcids.cIonMassOffset;
				des[i] = new MsmsPeakAnnotation(new CIon(), i + 1, 1, result[i], 0);
			}
			return result;
		}
		public static double[] GetCpSeries(double[] ntermSeries, out PeakAnnotation[] des){
			double[] result = new double[ntermSeries.Length];
			des = new MsmsPeakAnnotation[ntermSeries.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = ntermSeries[i] + AminoAcids.cIonMassOffset + Molecule.massH;
				des[i] = new MsmsPeakAnnotation(new CpIon(), i + 1, 1, result[i], 0);
			}
			return result;
		}
		public static double[] GetCmSeries(double[] ntermSeries, out PeakAnnotation[] des){
			double[] result = new double[ntermSeries.Length];
			des = new MsmsPeakAnnotation[ntermSeries.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = ntermSeries[i] + AminoAcids.cIonMassOffset - Molecule.massH;
				des[i] = new MsmsPeakAnnotation(new CmIon(), i + 1, 1, result[i], 0);
			}
			return result;
		}
		public static double[] GetCSeries(string sequence, PeptideModificationInfo fixedMods,
			PeptideModificationState varMods, out PeakAnnotation[] des, Dictionary<ushort, Modification2> specialMods){
			return GetCSeries(GetNtermMasses(sequence, fixedMods, varMods, specialMods), out des);
		}
		public static double[] GetCpSeries(string sequence, PeptideModificationInfo fixedMods,
			PeptideModificationState varMods, out PeakAnnotation[] des, Dictionary<ushort, Modification2> specialMods){
			return GetCpSeries(GetNtermMasses(sequence, fixedMods, varMods, specialMods), out des);
		}
		public static double[] GetCmSeries(string sequence, PeptideModificationInfo fixedMods,
			PeptideModificationState varMods, out PeakAnnotation[] des, Dictionary<ushort, Modification2> specialMods){
			return GetCmSeries(GetNtermMasses(sequence, fixedMods, varMods, specialMods), out des);
		}
		private static double[] GetPhosphoXSeries(IList<double> y, string sequence, PeptideModificationState varMods,
			out PeakAnnotation[] descriptions, bool includeNeutralLoss){
			List<double> masses = new List<double>();
			List<MsmsPeakAnnotation> descr = new List<MsmsPeakAnnotation>();
			ushort[] vms = varMods.Modifications;
			for (int i = 0; i < y.Count; i++){
				int index = sequence.Length - 1 - i;
				ushort vm = vms[index];
				if (vm != ushort.MaxValue){
					if (Tables.ModificationList[vm].IsPhosphorylation){
						double x = y[i] + Molecule.massC + Molecule.massO;
						masses.Add(x);
						descr.Add(new MsmsPeakAnnotation(new XIon(), i + 1, 1, x, 0));
						if (sequence[index] != 'Y' && includeNeutralLoss){
							double xn = x - 97.976896;
							masses.Add(xn);
							descr.Add(new MsmsPeakAnnotation(new XIon(), i + 1, 1, xn, 1));
						}
					}
				}
			}
			descriptions = descr.ToArray();
			return masses.ToArray();
		}
		public static double[] GetPhosphoXSeries(string sequence, PeptideModificationInfo fixedMods,
			PeptideModificationState varMods, out PeakAnnotation[] descriptions, bool includeNeutralLoss,
			Dictionary<ushort, Modification2> specialMods){
			return GetPhosphoXSeries(GetYSeries(sequence, fixedMods, varMods, out descriptions, specialMods), sequence,
				varMods, out descriptions, includeNeutralLoss);
		}
		public static double[] GetXSeries(double[] ctermMasses, out PeakAnnotation[] des){
			double[] result = new double[ctermMasses.Length];
			des = new PeakAnnotation[ctermMasses.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = ctermMasses[i] + AminoAcids.xIonMassOffset;
				des[i] = new MsmsPeakAnnotation(new XIon(), i + 1, 1, result[i], 0);
			}
			return result;
		}
		public static double[] GetXSeries(double[] ctermMasses){
			double[] result = new double[ctermMasses.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = ctermMasses[i] + AminoAcids.xIonMassOffset;
			}
			return result;
		}
		public static double[] GetXSeries(string sequence, PeptideModificationInfo fixedMods,
			PeptideModificationState varMods, out PeakAnnotation[] des, Dictionary<ushort, Modification2> specialMods){
			return GetXSeries(GetCtermMasses(sequence, fixedMods, varMods, specialMods), out des);
		}
		public static double[] GetYSeries(double[] ctermMasses, out PeakAnnotation[] des){
			double[] result = new double[ctermMasses.Length];
			des = new MsmsPeakAnnotation[ctermMasses.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = ctermMasses[i] + AminoAcids.yIonMassOffset;
				des[i] = new MsmsPeakAnnotation(new YIon(), i + 1, 1, result[i], 0);
			}
			return result;
		}
		public static double[] GetYSeries(double[] ctermMasses){
			double[] result = new double[ctermMasses.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = ctermMasses[i] + AminoAcids.yIonMassOffset;
			}
			return result;
		}
		public static double[] GetYSeries(string sequence, PeptideModificationInfo fixedMods,
			PeptideModificationState varMods, out PeakAnnotation[] des, Dictionary<ushort, Modification2> specialMods){
			return GetYSeries(GetCtermMasses(sequence, fixedMods, varMods, specialMods), out des);
		}
		public static double[] GetZSeries(double[] ctermMasses, out PeakAnnotation[] des){
			double[] result = new double[ctermMasses.Length];
			des = new MsmsPeakAnnotation[ctermMasses.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = ctermMasses[i] + AminoAcids.zIonMassOffset;
				des[i] = new MsmsPeakAnnotation(new ZIon(), i + 1, 1, result[i], 0);
			}
			return result;
		}
		public static double[] GetZSeries(double[] ctermMasses){
			double[] result = new double[ctermMasses.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = ctermMasses[i] + AminoAcids.zIonMassOffset;
			}
			return result;
		}
		public static double[] GetZDotSeries(double[] ctermMasses, out PeakAnnotation[] des){
			double[] result = new double[ctermMasses.Length];
			des = new MsmsPeakAnnotation[ctermMasses.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = ctermMasses[i] + AminoAcids.zDotIonMassOffset;
				des[i] = new MsmsPeakAnnotation(new ZDotIon(), i + 1, 1, result[i], 0);
			}
			return result;
		}
		public static double[] GetZPrimeSeries(double[] ctermMasses, out PeakAnnotation[] des){
			double[] result = new double[ctermMasses.Length];
			des = new MsmsPeakAnnotation[ctermMasses.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = ctermMasses[i] + AminoAcids.zPrimeIonMassOffset;
				des[i] = new MsmsPeakAnnotation(new ZPrimeIon(), i + 1, 1, result[i], 0);
			}
			return result;
		}
		public static double[] GetZSeries(string sequence, PeptideModificationInfo fixedMods,
			PeptideModificationState varMods, out PeakAnnotation[] des, Dictionary<ushort, Modification2> specialMods){
			return GetZSeries(GetCtermMasses(sequence, fixedMods, varMods, specialMods), out des);
		}
		public static double[] GetZDotSeries(string sequence, PeptideModificationInfo fixedMods,
			PeptideModificationState varMods, out PeakAnnotation[] des, Dictionary<ushort, Modification2> specialMods){
			return GetZDotSeries(GetCtermMasses(sequence, fixedMods, varMods, specialMods), out des);
		}
		public static double[] GetZPrimeSeries(string sequence, PeptideModificationInfo fixedMods,
			PeptideModificationState varMods, out PeakAnnotation[] des, Dictionary<ushort, Modification2> specialMods){
			return GetZPrimeSeries(GetCtermMasses(sequence, fixedMods, varMods, specialMods), out des);
		}
		public static void AddNeutralLossCtermSeries(ref int pos, int[] inds, double[] masses,
            PeakAnnotation[] annotations, short[] dependents, bool calcNeighbors, short[] left, short[] right,
            string sequence, PeptideModificationState varMods, double[] ctermSeriesMasses, bool secondLoss,
            bool includeWater, int ctermWaterLossInd, bool includeAmmonia, int ctermNh3LossInd, bool dependentLosses,
			PeakAnnotation[] ctermSeriesLoss, PeakAnnotation[] ctermSeriesLossWater, PeakAnnotation[] ctermSeriesLossAmmonia,
			PeakAnnotation[] ctermSeriesLoss2, bool sequenceBasedMod){
            ModificationSite neutralLossSite1 = null;
            int neutralLossIndex1 = -1;
            ModificationSite neutralLossSite2 = null;
            int neutralLossIndex2 = -1;
            ModificationSite SBMsite = null;
            int SBMindex = -1;
            ushort[] vms = varMods.Modifications;
			for (int i = sequence.Length - 1; i >= 1; i--){
                ushort vm = vms[i];
				if (vm != ushort.MaxValue){
                    Modification mod = Tables.ModificationList[vm];
                    ModificationSite index = mod.GetSite(sequence[i]);
					if (index != null && index.HasNeutralLoss && sequenceBasedMod == false){
						if (neutralLossIndex1 == -1){
                            neutralLossSite1 = index;
                            neutralLossIndex1 = sequence.Length - 1 - i;
						} else{
                            neutralLossSite2 = index;
                            neutralLossIndex2 = sequence.Length - 1 - i;
                            break;
                        }
					} else if (sequenceBasedMod == true && index != null && index.HasNeutralLoss &&
					           mod.IsSequenceBasedModifier == true){
						if (SBMindex == -1){
                            SBMsite = index;
                            SBMindex = sequence.Length - 1 - i;
                            break;
                        }
                    }
                }
            }
			if (neutralLossIndex1 != -1 && neutralLossSite1 != null){
                double loss1 = neutralLossSite1.neutralloss_collection[0].DeltaMass;
                int start = ArrayUtils.CeilIndex(ctermSeriesMasses, loss1);
				if (start >= 0){
					if (inds != null){
                        start = inds[start];
                    }
                    start = Math.Max(neutralLossIndex1, start);
                    int indStart = pos;
					for (int i = 0; i < ctermSeriesMasses.Length; i++){
                        int ind = inds?[i] ?? i;
						if (ind >= start){
                            double m = ctermSeriesMasses[i] - loss1;
                            masses[pos] = m;
                            annotations[pos] = ind >= annotLen ? null : ctermSeriesLoss[ind];
                            dependents[pos] = -1;
							if (calcNeighbors){
								left[pos] = ind > start ? (short) (pos - 1) : (short) -1;
								right[pos] = i < ctermSeriesMasses.Length - 1 ? (short) (pos + 1) : (short) -1;
                            }
                            pos++;
                        }
                    }
					if (includeWater){
						for (int i = 0; i < ctermSeriesMasses.Length; i++){
                            int ind = inds?[i] ?? i;
							if (ind >= start && ctermWaterLossInd >= 0 && i >= ctermWaterLossInd){
                                double m = ctermSeriesMasses[i] - loss1;
                                masses[pos] = m - Molecule.massWater;
								annotations[pos] = ind >= annotLen ? null : ctermSeriesLossWater[ind];
                                //TODO
								dependents[pos] = (short) (dependentLosses ? indStart + i - start : -1);
								if (calcNeighbors){
									left[pos] = ind > start ? (short) (pos - 1) : (short) -1;
									right[pos] = i < ctermSeriesMasses.Length - 1 ? (short) (pos + 1) : (short) -1;
                                }
                                pos++;
                            }
                        }
                    }
					if (includeAmmonia){
						for (int i = 0; i < ctermSeriesMasses.Length; i++){
                            int ind = inds?[i] ?? i;
							if (ind >= start && ctermNh3LossInd >= 0 && i >= ctermNh3LossInd){
                                double m = ctermSeriesMasses[i] - loss1;
                                masses[pos] = m - Molecule.massAmmonia;
								annotations[pos] = ind >= annotLen ? null : ctermSeriesLossAmmonia[ind];
                                //TODO
								dependents[pos] = (short) (dependentLosses ? indStart + i - start : -1);
								if (calcNeighbors){
									left[pos] = ind > start ? (short) (pos - 1) : (short) -1;
									right[pos] = i < ctermSeriesMasses.Length - 1 ? (short) (pos + 1) : (short) -1;
                                }
                                pos++;
                            }
                        }
                    }
                }
            }
			if (secondLoss && neutralLossIndex2 != -1 && neutralLossSite2 != null){
                double loss1 = neutralLossSite1.neutralloss_collection[0].DeltaMass;
                double loss2 = neutralLossSite2.neutralloss_collection[0].DeltaMass;
                int start = ArrayUtils.CeilIndex(ctermSeriesMasses, loss1 + loss2);
				if (start >= 0){
					if (inds != null){
                        start = inds[start];
                    }
                    start = Math.Max(neutralLossIndex2, start);
					for (int i = 0; i < ctermSeriesMasses.Length; i++){
                        int ind = inds?[i] ?? i;
						if (ind >= start){
                            double m = ctermSeriesMasses[i] - loss1 - loss2;
							if (m > 0){
                                masses[pos] = m;
                                annotations[pos] = ind >= annotLen ? null : ctermSeriesLoss2[ind];
                                dependents[pos] = -1;
								if (calcNeighbors){
									left[pos] = ind > start ? (short) (pos - 1) : (short) -1;
									right[pos] = i < ctermSeriesMasses.Length - 1 ? (short) (pos + 1) : (short) -1;
                                }
                                pos++;
                            }
                        }
                    }
                }
            }
			if (SBMindex != -1 && SBMsite != null){
				for (int k = 0; k < SBMsite.neutralloss_collection.Length; k++){
                    double SBMloss = SBMsite.neutralloss_collection[k].DeltaMass;
                    int start = ArrayUtils.CeilIndex(ctermSeriesMasses, SBMloss);
					if (start >= 0){
						if (inds != null){
                            start = inds[start];
                        }
                        start = Math.Max(SBMindex, start);
                        int indStart = pos;
						for (int i = 0; i < ctermSeriesMasses.Length; i++){
                            int ind = inds?[i] ?? i;
							if (ind >= start){
                                double m = ctermSeriesMasses[i] - SBMloss;

                                string name = SBMsite.neutralloss_collection[k].ShortName; // to add a number for the specific p-ions
                                if (int.TryParse(name, out int pnr))
                                {
                                    annotations[pos] = new MsmsPeakAnnotation(new YIon(), ind + 1, 1, m, 0, pnr);
                                }
                                else // use number in list as annotation number
                                {
                                    int pnr2 = SBMsite.neutralloss_collection.Length - k;
                                    annotations[pos] = new MsmsPeakAnnotation(new YIon(), ind + 1, 1, m, 0, pnr2);
                                }
                                masses[pos] = m;
								annotations[pos] = ind >= annotLen ? null : ctermSeriesLoss[ind];
                                dependents[pos] = -1;
								if (calcNeighbors){
									left[pos] = ind > start ? (short) (pos - 1) : (short) -1;
									right[pos] = i < ctermSeriesMasses.Length - 1 ? (short) (pos + 1) : (short) -1;
                                }
                                pos++;
                            }
                        }
                    }
                }
            }
        }
        public static void AddNeutralLossCtermSeriesNoAnnot(ref int pos, int[] inds, double[] masses,
			short[] dependents, bool calcNeighbors, short[] left, short[] right, string sequence,
			PeptideModificationState varMods, double[] ctermSeriesMasses, bool secondLoss, bool includeWater,
			int ctermWaterLossInd, bool includeAmmonia, int ctermNh3LossInd, bool dependentLosses,
			bool sequenceBasedMod){
			ModificationSite neutralLossSite1 = null;
			int neutralLossIndex1 = -1;
			ModificationSite neutralLossSite2 = null;
			int neutralLossIndex2 = -1;
			ModificationSite SBMsite = null;
			int SBMindex = -1;
			ushort[] vms = varMods.Modifications;
			for (int i = sequence.Length - 1; i >= 1; i--){
				ushort vm = vms[i];
				if (vm != ushort.MaxValue){
					Modification mod = Tables.ModificationList[vm];
					ModificationSite index = mod.GetSite(sequence[i]);
					if (index != null && index.HasNeutralLoss && sequenceBasedMod == false){
						if (neutralLossIndex1 == -1){
							neutralLossSite1 = index;
							neutralLossIndex1 = sequence.Length - 1 - i;
						} else{
							neutralLossSite2 = index;
							neutralLossIndex2 = sequence.Length - 1 - i;
							break;
						}
					} else if (sequenceBasedMod == true && index != null && index.HasNeutralLoss &&
					           mod.IsSequenceBasedModifier == true){
						if (SBMindex == -1){
							SBMsite = index;
							SBMindex = sequence.Length - 1 - i;
							break;
						}
					}
				}
			}
			if (neutralLossIndex1 != -1 && neutralLossSite1 != null){
				double loss1 = neutralLossSite1.neutralloss_collection[0].DeltaMass;
				int start = ArrayUtils.CeilIndex(ctermSeriesMasses, loss1);
				if (start >= 0){
					if (inds != null){
						start = inds[start];
					}
					start = Math.Max(neutralLossIndex1, start);
					int indStart = pos;
					for (int i = 0; i < ctermSeriesMasses.Length; i++){
						int ind = inds?[i] ?? i;
						if (ind >= start){
							double m = ctermSeriesMasses[i] - loss1;
							masses[pos] = m;
							dependents[pos] = -1;
							if (calcNeighbors){
								left[pos] = ind > start ? (short) (pos - 1) : (short) -1;
								right[pos] = i < ctermSeriesMasses.Length - 1 ? (short) (pos + 1) : (short) -1;
							}
							pos++;
						}
					}
					if (includeWater){
						for (int i = 0; i < ctermSeriesMasses.Length; i++){
							int ind = inds?[i] ?? i;
							if (ind >= start && ctermWaterLossInd >= 0 && i >= ctermWaterLossInd){
								double m = ctermSeriesMasses[i] - loss1;
								masses[pos] = m - Molecule.massWater;
								dependents[pos] = (short) (dependentLosses ? indStart + i - start : -1);
								if (calcNeighbors){
									left[pos] = ind > start ? (short) (pos - 1) : (short) -1;
									right[pos] = i < ctermSeriesMasses.Length - 1 ? (short) (pos + 1) : (short) -1;
								}
								pos++;
							}
						}
					}
					if (includeAmmonia){
						for (int i = 0; i < ctermSeriesMasses.Length; i++){
							int ind = inds?[i] ?? i;
							if (ind >= start && ctermNh3LossInd >= 0 && i >= ctermNh3LossInd){
								double m = ctermSeriesMasses[i] - loss1;
								masses[pos] = m - Molecule.massAmmonia;
								dependents[pos] = (short) (dependentLosses ? indStart + i - start : -1);
								if (calcNeighbors){
									left[pos] = ind > start ? (short) (pos - 1) : (short) -1;
									right[pos] = i < ctermSeriesMasses.Length - 1 ? (short) (pos + 1) : (short) -1;
								}
								pos++;
							}
						}
					}
				}
			}
			if (secondLoss && neutralLossIndex2 != -1 && neutralLossSite2 != null){
				double loss1 = neutralLossSite1.neutralloss_collection[0].DeltaMass;
				double loss2 = neutralLossSite2.neutralloss_collection[0].DeltaMass;
				int start = ArrayUtils.CeilIndex(ctermSeriesMasses, loss1 + loss2);
				if (start >= 0){
					if (inds != null){
						start = inds[start];
					}
					start = Math.Max(neutralLossIndex2, start);
					for (int i = 0; i < ctermSeriesMasses.Length; i++){
						int ind = inds?[i] ?? i;
						if (ind >= start){
							double m = ctermSeriesMasses[i] - loss1 - loss2;
							if (m > 0){
								masses[pos] = m;
								dependents[pos] = -1;
								if (calcNeighbors){
									left[pos] = ind > start ? (short) (pos - 1) : (short) -1;
									right[pos] = i < ctermSeriesMasses.Length - 1 ? (short) (pos + 1) : (short) -1;
								}
								pos++;
							}
						}
					}
				}
			}
			if (SBMindex != -1 && SBMsite != null){
				for (int k = 0; k < SBMsite.neutralloss_collection.Length; k++){
					double SBMloss = SBMsite.neutralloss_collection[k].DeltaMass;
					int start = ArrayUtils.CeilIndex(ctermSeriesMasses, SBMloss);
					if (start >= 0){
						if (inds != null){
							start = inds[start];
						}
						start = Math.Max(SBMindex, start);
						int indStart = pos;
						for (int i = 0; i < ctermSeriesMasses.Length; i++){
							int ind = inds?[i] ?? i;
							if (ind >= start){
								double m = ctermSeriesMasses[i] - SBMloss;
								masses[pos] = m;
								dependents[pos] = -1;
								if (calcNeighbors){
									left[pos] = ind > start ? (short) (pos - 1) : (short) -1;
									right[pos] = i < ctermSeriesMasses.Length - 1 ? (short) (pos + 1) : (short) -1;
								}
								pos++;
							}
						}
					}
				}
			}
		}
		public static void AddNeutralLossNtermSeries(ref int pos, int[] inds, double[] masses,
			PeakAnnotation[] annotations, short[] dependents, bool calcNeighbors, short[] left, short[] right,
			string sequence, PeptideModificationState varMods, double[] ntermSeriesMasses, bool secondLoss,
			bool includeWater, int ntermWaterLossInd, bool includeAmmonia, int ntermNh3LossInd, bool dependentLosses,
			PeakAnnotation[] ntermSeriesLoss, PeakAnnotation[] ntermSeriesLossWater, PeakAnnotation[] ntermSeriesLossAmmonia,
			PeakAnnotation[] ntermSeriesLoss2, bool sequenceBasedMod){
			ModificationSite neutralLossSite1 = null;
			int neutralLossIndex1 = -1;
			ModificationSite neutralLossSite2 = null;
			int neutralLossIndex2 = -1;
			ushort[] vms = varMods.Modifications;
			ModificationSite SBMsite = null;
			int SBMindex = -1;
			for (int i = 0; i < sequence.Length - 1; i++){
				ushort vm = vms[i];
				if (vm != ushort.MaxValue){
					Modification mod = Tables.ModificationList[vm];
					ModificationSite index = mod.GetSite(sequence[i]);
					if (index != null && index.HasNeutralLoss && sequenceBasedMod == false){
						if (neutralLossIndex1 == -1){
							neutralLossSite1 = index;
							neutralLossIndex1 = i;
						} else{
							neutralLossSite2 = index;
							neutralLossIndex2 = i;
							break;
						}
					} else if (sequenceBasedMod == true && index != null && index.HasNeutralLoss &&
					           mod.IsSequenceBasedModifier == true){
						if (SBMindex == -1){
							SBMsite = index;
							SBMindex = i;
							break;
						}
					}
				}
			}
			if (neutralLossIndex1 != -1 && neutralLossSite1 != null){
				double loss1 = neutralLossSite1.neutralloss_collection[0].DeltaMass;
				int start = ArrayUtils.CeilIndex(ntermSeriesMasses, loss1);
				if (start >= 0){
					if (inds != null){
						start = inds[start];
					}
					start = Math.Max(neutralLossIndex1, start);
					int indStart = pos;
					for (int i = 0; i < ntermSeriesMasses.Length; i++){
						int ind = inds?[i] ?? i;
						if (ind >= start){
							double m = ntermSeriesMasses[i] - loss1;
							masses[pos] = m;
							annotations[pos] = ind >= annotLen ? null : ntermSeriesLoss[ind];
							dependents[pos] = -1;
							if (calcNeighbors){
								left[pos] = ind > start ? (short) (pos - 1) : (short) -1;
								right[pos] = i < ntermSeriesMasses.Length - 1 ? (short) (pos + 1) : (short) -1;
							}
							pos++;
						}
					}
					if (includeWater){
						for (int i = 0; i < ntermSeriesMasses.Length; i++){
							int ind = inds?[i] ?? i;
							if (ind >= start && ntermWaterLossInd >= 0 && i >= ntermWaterLossInd){
								double m = ntermSeriesMasses[i] - loss1;
								masses[pos] = m - Molecule.massWater;
								annotations[pos] = ind >= annotLen ? null : ntermSeriesLossWater[ind];
								//TODO
								dependents[pos] = (short) (dependentLosses ? indStart + i - start : -1);
								if (calcNeighbors){
									left[pos] = ind > start ? (short) (pos - 1) : (short) -1;
									right[pos] = i < ntermSeriesMasses.Length - 1 ? (short) (pos + 1) : (short) -1;
								}
								pos++;
							}
						}
					}
					if (includeAmmonia){
						for (int i = 0; i < ntermSeriesMasses.Length; i++){
							int ind = inds?[i] ?? i;
							if (ind >= start && ntermNh3LossInd >= 0 && i >= ntermNh3LossInd){
								double m = ntermSeriesMasses[i] - loss1;
								masses[pos] = m - Molecule.massAmmonia;
								annotations[pos] = ind >= annotLen ? null : ntermSeriesLossAmmonia[ind];
								//TODO
								dependents[pos] = (short) (dependentLosses ? indStart + i - start : -1);
								if (calcNeighbors){
									left[pos] = ind > start ? (short) (pos - 1) : (short) -1;
									right[pos] = i < ntermSeriesMasses.Length - 1 ? (short) (pos + 1) : (short) -1;
								}
								pos++;
							}
						}
					}
				}
			}
			if (secondLoss && neutralLossIndex2 != -1 && neutralLossSite2 != null){
				double loss1 = neutralLossSite1.neutralloss_collection[0].DeltaMass;
				double loss2 = neutralLossSite2.neutralloss_collection[0].DeltaMass;
				int start = ArrayUtils.CeilIndex(ntermSeriesMasses, loss1 + loss2);
				if (start >= 0){
					if (inds != null){
						start = inds[start];
					}
					start = Math.Max(neutralLossIndex2, start);
					for (int i = 0; i < ntermSeriesMasses.Length; i++){
						int ind = inds?[i] ?? i;
						if (ind >= start){
							double m = ntermSeriesMasses[i] - loss1 - loss2;
							if (m > 0){
								masses[pos] = m;
								annotations[pos] = ind >= annotLen ? null : ntermSeriesLoss2[ind];
								dependents[pos] = -1;
								if (calcNeighbors){
									left[pos] = ind > start ? (short) (pos - 1) : (short) -1;
									right[pos] = i < ntermSeriesMasses.Length - 1 ? (short) (pos + 1) : (short) -1;
								}
								pos++;
							}
						}
					}
				}
			}
			if (SBMindex != -1 && SBMsite != null){
				for (int k = 0; k < SBMsite.neutralloss_collection.Length; k++){
					double SBMloss = SBMsite.neutralloss_collection[k].DeltaMass;
					int start = ArrayUtils.CeilIndex(ntermSeriesMasses, SBMloss);
					if (start >= 0){
						if (inds != null){
							start = inds[start];
						}
						start = Math.Max(SBMindex, start);
						int indStart = pos;
						for (int i = 0; i < ntermSeriesMasses.Length; i++){
							int ind = inds?[i] ?? i;
							if (ind >= start){
								double m = ntermSeriesMasses[i] - SBMloss;
								masses[pos] = m;
                                string name = SBMsite.neutralloss_collection[k].ShortName;
                                if (int.TryParse(name, out int pnr))
                                {
                                    annotations[pos] = new MsmsPeakAnnotation(new BIon(), ind + 1, 1, m, 0, pnr);
                                }
                                else
                                {
                                    int pnr2 = SBMsite.neutralloss_collection.Length - k;
                                    annotations[pos] = new MsmsPeakAnnotation(new BIon(), ind + 1, 1, m, 0, pnr2);
                                }
                                dependents[pos] = -1;
								if (calcNeighbors){
									left[pos] = ind > start ? (short) (pos - 1) : (short) -1;
									right[pos] = i < ntermSeriesMasses.Length - 1 ? (short) (pos + 1) : (short) -1;
								}
								pos++;
							}
						}
					}
				}
			}
		}
		public static void AddNeutralLossNtermSeriesNoAnnot(ref int pos, int[] inds, double[] masses,
			short[] dependents, bool calcNeighbors, short[] left, short[] right, string sequence,
			PeptideModificationState varMods, double[] ntermSeriesMasses, bool secondLoss, bool includeWater,
			int ntermWaterLossInd, bool includeAmmonia, int ntermNh3LossInd, bool dependentLosses,
			bool sequenceBasedMod){
			ModificationSite neutralLossSite1 = null;
			int neutralLossIndex1 = -1;
			ModificationSite neutralLossSite2 = null;
			int neutralLossIndex2 = -1;
			ModificationSite SBMsite = null;
			int SBMindex = -1;
			ushort[] vms = varMods.Modifications;
			for (int i = 0; i < sequence.Length - 1; i++){
				ushort vm = vms[i];
				if (vm != ushort.MaxValue){
					Modification mod = Tables.ModificationList[vm];
					ModificationSite index = mod.GetSite(sequence[i]);
					if (index != null && index.HasNeutralLoss && sequenceBasedMod == false){
						if (neutralLossIndex1 == -1){
							neutralLossSite1 = index;
							neutralLossIndex1 = i;
						} else{
							neutralLossSite2 = index;
							neutralLossIndex2 = i;
							break;
						}
					} else if (sequenceBasedMod == true && index != null && index.HasNeutralLoss &&
					           mod.IsSequenceBasedModifier == true){
						if (SBMindex == -1){
							SBMsite = index;
							SBMindex = i;
							break;
						}
					}
				}
			}
			if (neutralLossIndex1 != -1 && neutralLossSite1 != null){
				double loss1 = neutralLossSite1.neutralloss_collection[0].DeltaMass;
				int start = ArrayUtils.CeilIndex(ntermSeriesMasses, loss1);
				if (start >= 0){
					if (inds != null){
						start = inds[start];
					}
					start = Math.Max(neutralLossIndex1, start);
					int indStart = pos;
					for (int i = 0; i < ntermSeriesMasses.Length; i++){
						int ind = inds?[i] ?? i;
						if (ind >= start){
							double m = ntermSeriesMasses[i] - loss1;
							masses[pos] = m;
							dependents[pos] = -1;
							if (calcNeighbors){
								left[pos] = ind > start ? (short) (pos - 1) : (short) -1;
								right[pos] = i < ntermSeriesMasses.Length - 1 ? (short) (pos + 1) : (short) -1;
							}
							pos++;
						}
					}
					if (includeWater){
						for (int i = 0; i < ntermSeriesMasses.Length; i++){
							int ind = inds?[i] ?? i;
							if (ind >= start && ntermWaterLossInd >= 0 && i >= ntermWaterLossInd){
								double m = ntermSeriesMasses[i] - loss1;
								masses[pos] = m - Molecule.massWater;
								dependents[pos] = (short) (dependentLosses ? indStart + i - start : -1);
								if (calcNeighbors){
									left[pos] = ind > start ? (short) (pos - 1) : (short) -1;
									right[pos] = i < ntermSeriesMasses.Length - 1 ? (short) (pos + 1) : (short) -1;
								}
								pos++;
							}
						}
					}
					if (includeAmmonia){
						for (int i = 0; i < ntermSeriesMasses.Length; i++){
							int ind = inds?[i] ?? i;
							if (ind >= start && ntermNh3LossInd >= 0 && i >= ntermNh3LossInd){
								double m = ntermSeriesMasses[i] - loss1;
								masses[pos] = m - Molecule.massAmmonia;
								dependents[pos] = (short) (dependentLosses ? indStart + i - start : -1);
								if (calcNeighbors){
									left[pos] = ind > start ? (short) (pos - 1) : (short) -1;
									right[pos] = i < ntermSeriesMasses.Length - 1 ? (short) (pos + 1) : (short) -1;
								}
								pos++;
							}
						}
					}
				}
			}
			if (secondLoss && neutralLossIndex2 != -1 && neutralLossSite2 != null){
				double loss1 = neutralLossSite1.neutralloss_collection[0].DeltaMass;
				double loss2 = neutralLossSite2.neutralloss_collection[0].DeltaMass;
				int start = ArrayUtils.CeilIndex(ntermSeriesMasses, loss1 + loss2);
				if (start >= 0){
					if (inds != null){
						start = inds[start];
					}
					start = Math.Max(neutralLossIndex2, start);
					for (int i = 0; i < ntermSeriesMasses.Length; i++){
						int ind = inds?[i] ?? i;
						if (ind >= start){
							double m = ntermSeriesMasses[i] - loss1 - loss2;
							if (m > 0){
								masses[pos] = m;
								dependents[pos] = -1;
								if (calcNeighbors){
									left[pos] = ind > start ? (short) (pos - 1) : (short) -1;
									right[pos] = i < ntermSeriesMasses.Length - 1 ? (short) (pos + 1) : (short) -1;
								}
								pos++;
							}
						}
					}
				}
			}
			if (SBMindex != -1 && SBMsite != null){
				for (int k = 0; k < SBMsite.neutralloss_collection.Length; k++){
					double SBMloss = SBMsite.neutralloss_collection[k].DeltaMass;
					int start = ArrayUtils.CeilIndex(ntermSeriesMasses, SBMloss);
					if (start >= 0){
						if (inds != null){
							start = inds[start];
						}
						start = Math.Max(SBMindex, start);
						int indStart = pos;
						for (int i = 0; i < ntermSeriesMasses.Length; i++){
							int ind = inds?[i] ?? i;
							if (ind >= start){
								double m = ntermSeriesMasses[i] - SBMloss;
								masses[pos] = m;
								dependents[pos] = -1;
								if (calcNeighbors){
									left[pos] = ind > start ? (short) (pos - 1) : (short) -1;
									right[pos] = i < ntermSeriesMasses.Length - 1 ? (short) (pos + 1) : (short) -1;
								}
								pos++;
							}
						}
					}
				}
			}
		}
		public static int GetNtermWaterLossInd(string sequence){
			for (int i = 0; i < sequence.Length; i++){
				if (Array.BinarySearch(waterLossAa, sequence[i]) >= 0){
					return i;
				}
			}
			return -1;
		}
		public static int GetCtermWaterLossInd(string sequence){
			for (int i = sequence.Length - 1; i >= 0; i--){
				if (Array.BinarySearch(waterLossAa, sequence[i]) >= 0){
					return sequence.Length - 1 - i;
				}
			}
			return -1;
		}
		public static int GetNtermNh3LossInd(string sequence){
			for (int i = 0; i < sequence.Length; i++){
				if (Array.BinarySearch(nh3LossAa, sequence[i]) >= 0){
					return i;
				}
			}
			return -1;
		}
		public static int GetCtermNh3LossInd(string sequence){
			for (int i = sequence.Length - 1; i >= 0; i--){
				if (Array.BinarySearch(nh3LossAa, sequence[i]) >= 0){
					return sequence.Length - 1 - i;
				}
			}
			return -1;
		}
	}
}