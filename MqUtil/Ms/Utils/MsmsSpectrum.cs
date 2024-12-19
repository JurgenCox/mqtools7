using MqApi.Num;
using MqApi.Util;
namespace MqUtil.Ms.Utils{
	public sealed class MsmsSpectrum : IDisposable{
		/// <summary>
		/// No deisotoping and top-x removal below this mass.
		/// </summary>
		public const double lowerMassCutoff = 250;
		public double[] masses;
		public float[] intensities;
		public int[] charges;
		public double[] origMasses = new double[0];
		public float[] origIntensities = new float[0];
		public int[] origCharges = new int[0];
		public int[] origToNew = new int[0];
		public string massAnalyzer; // One of FTMS, ITMS, TOF, or Unknown
		public string fragType; // One of CID, ETD, HCD, PQD, and Unknown
		public float tic;
		public MsmsSpectrum(double[] masses, float[] intensities, int[] charges, double[] origMasses,
			float[] origIntensities, int[] origCharges, int[] origToNew, string massAnalyzer, string fragType){
			this.masses = masses;
			this.intensities = intensities;
			this.charges = charges;
			this.origMasses = origMasses;
			this.origIntensities = origIntensities;
			this.origCharges = origCharges;
			this.origToNew = origToNew;
			this.massAnalyzer = massAnalyzer;
			this.fragType = fragType;
			tic = intensities.DefaultIfEmpty(0).Sum();
		}
		public MsmsSpectrum(double[] masses, float[] intensities, int[] charges, string massAnalyzer, string fragType){
			this.masses = masses;
			this.intensities = intensities;
			this.charges = charges;
			this.massAnalyzer = massAnalyzer;
			this.fragType = fragType;
			tic = intensities.DefaultIfEmpty(0).Sum();
		}
		public MsmsSpectrum(BinaryReader reader){
			tic = reader.ReadSingle();
			masses = FileUtils.ReadDoubleArray(reader);
			intensities = FileUtils.ReadFloatArray(reader);
			charges = FileUtils.ReadInt32Array(reader);
			origMasses = FileUtils.ReadDoubleArray(reader);
			origIntensities = FileUtils.ReadFloatArray(reader);
			origCharges = FileUtils.ReadInt32Array(reader);
			origToNew = FileUtils.ReadInt32Array(reader);
			massAnalyzer = reader.ReadString();
			fragType = reader.ReadString();
		}
		public void Write(BinaryWriter writer){
			writer.Write(tic);
			FileUtils.Write(masses, writer);
			FileUtils.Write(intensities, writer);
			FileUtils.Write(charges, writer);
			FileUtils.Write(origMasses, writer);
			FileUtils.Write(origIntensities, writer);
			FileUtils.Write(origCharges, writer);
			FileUtils.Write(origToNew, writer);
			writer.Write(massAnalyzer);
			writer.Write(fragType);
		}
		public MsmsSpectrum SortMasses2(){
			int[] o = masses.Order();
			return ExtractSpectrum(o);
		}

		public double[] Masses => masses;
		public float[] Intensities => intensities;
		public int[] Charges => charges;
		public int Count => masses?.Length ?? 0;
		public double MinMass => GetMass(0);
		public double MaxMass => GetMass(Count - 1);
		public int GetCeilIndex(double mass){
			return ArrayUtils.CeilIndex(masses, mass);
		}
		public int GetFloorIndex(double mass){
			return ArrayUtils.FloorIndex(masses, mass);
		}
		public int GetClosestIndex(double mass, bool outOfRangeIsInvalid){
			if (mass <= MinMass){
				return outOfRangeIsInvalid ? -1 : 0;
			}
			if (mass >= MaxMass){
				return outOfRangeIsInvalid ? -1 : Count - 1;
			}
			int index = Array.BinarySearch(masses, mass);
			if (index >= 0){
				return index;
			}
			index = -2 - index;
			if (Math.Abs(GetMass(index) - mass) < Math.Abs(GetMass(index + 1) - mass)){
				return index;
			}
			return index + 1;
		}
		public double GetMass(int index){
			return masses.Length > 0 ? masses[index] : double.NaN;
		}
		public double GetIntensity(int index){
			return intensities[index];
		}
		public void Dispose(){
			masses = null;
			intensities = null;
		}
		public MsmsSpectrum TopX(int topX, double window) {
			int[] index = TopXIndices(topX, window, lowerMassCutoff);
			return ExtractSpectrum(index);
		}
		public MsmsSpectrum ExtractSpectrum(int[] index) {
			return new MsmsSpectrum(masses.SubArray(index), intensities.SubArray(index), charges.SubArray(index),
				origMasses, origIntensities, origCharges, Transform(origToNew, index), massAnalyzer, fragType);
		}

		private static int[] Transform(int[] origToNew, int[] index) {
			Dictionary<int, int> m = ArrayUtils.InverseMap(index);
			int[] result = new int[origToNew.Length];
			for (int i = 0; i < origToNew.Length; i++) {
				if (m.ContainsKey(origToNew[i])) {
					result[i] = m[origToNew[i]];
				}else {
					result[i] = -1;
				}
			}
			return result;
		}

		private int[] TopXIndices(int topx, double window, double minThreshold){
			if (masses.Length == 0){
				return new int[0];
			}
			double w2 = window / 2.0;
			int pos = 0;
			int[] result = new int[masses.Length];
			double mmin = masses[0];
			double mmax = masses[masses.Length - 1];
			int length = Count;
			for (int i = 0; i < length; i++){
				double m = masses[i];
				if (m < minThreshold){
					result[pos++] = i;
					continue;
				}
				int lowInd = ArrayUtils.CeilIndex(masses, m - w2);
				int highInd = ArrayUtils.FloorIndex(masses, m + w2);
				int topx2 = topx;
				if (m - w2 < mmin){
					topx2 -= (int) Math.Round((mmin - m + w2) / window * topx);
				}
				if (m + w2 > mmax){
					topx2 -= (int) Math.Round((m + w2 - mmax) / window * topx);
				}
				double intens = intensities[i];
				int count = 0;
				for (int j = lowInd; j <= highInd; j++){
					if (j == i){
						continue;
					}
					if (intensities[j] > intens){
						count++;
					}
				}
				if (count < topx2){
					result[pos++] = i;
				}
			}
			Array.Resize(ref result, pos);
			return result;
		}
		public int GetMatchInd(double mz, double tol){
			if (masses.Length < 1){
				return -1;
			}
			int ind = GetClosestIndex(mz, false);
			if (Math.Abs(mz - masses[ind]) <= tol){
				return ind;
			}
			return -1;
		}
		public MsmsSpectrum RemoveAt(HashSet<int> removeInds){
			return ExtractSpectrum(ArrayUtils.Complement(removeInds, masses.Length));
		}
		/// <summary>
		/// Remove from the spectrum any peaks with m/z below the given value.
		/// </summary>
		/// <param name="mz"></param>
		public MsmsSpectrum RemoveBelow2(double mz){
			List<int> valids = new List<int>();
			for (int i = 0; i < masses.Length; i++){
				if (masses[i] >= mz){
					valids.Add(i);
				}
			}
			return ExtractSpectrum(valids.ToArray());
		}
		public MsmsSpectrum IntensityThreshold2(double intens){
			List<int> valids = new List<int>();
			for (int i = 0; i < masses.Length; i++){
				if (intensities[i] >= intens){
					valids.Add(i);
				}
			}
			return ExtractSpectrum(valids.ToArray());
		}
	}
}