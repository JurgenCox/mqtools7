using MqApi.Num;
using MqApi.Util;
using MqUtil.Mol;

namespace MqUtil.Parse.Chebi{
	public class ChebiModel{
		private readonly ChebiEntry[] main;
		private readonly ChebiEntry[] series;
		private readonly double[] masses;
		private readonly int[][] indices;
		private readonly int[][] seriesCount1;
		private readonly int[][] seriesCount2;
		public string[] Compositions { get; }

		public ChebiModel(BinaryReader reader){
			int n = reader.ReadInt32();
			main = new ChebiEntry[n];
			for (int i = 0; i < n; i++) {
				main[i] = new ChebiEntry(reader);
			}
			n = reader.ReadInt32();
			series = new ChebiEntry[n];
			for (int i = 0; i < n; i++) {
				series[i] = new ChebiEntry(reader);
			}
			masses = FileUtils.ReadDoubleArray(reader);
			indices = FileUtils.Read2DInt32Array(reader);
			seriesCount1 = FileUtils.Read2DInt32Array(reader);
			seriesCount2 = FileUtils.Read2DInt32Array(reader);
			Compositions = FileUtils.ReadStringArray(reader);
		}
		public void Write(BinaryWriter writer){
			writer.Write(main.Length);
			foreach (ChebiEntry entry in main) {
				entry.Write(writer);
			}
			writer.Write(series.Length);
			foreach (ChebiEntry entry in series) {
				entry.Write(writer);
			}
			FileUtils.Write(masses, writer);
			FileUtils.Write(indices, writer);
			FileUtils.Write(seriesCount1, writer);
			FileUtils.Write(seriesCount2, writer);
			FileUtils.Write(Compositions, writer);
		}
		internal ChebiModel(ChebiEntry[] main, ChebiEntry[] series, double maxMass){
			this.main = main;
			this.series = series;
			List<double> masses1 = new List<double>();
			List<int> indices1 = new List<int>();
			List<int> seriesCount11 = new List<int>();
			List<int> seriesCount21 = new List<int>();
			for (int i = 0; i < main.Length; i++){
				double m = main[i].MonoisotopicMass - main[i].Charge*Molecule.massH;
				masses1.Add(m);
				indices1.Add(i);
				seriesCount11.Add(-1);
				seriesCount21.Add(-1);
			}
			for (int i = 0; i < series.Length; i++){
				if (series[i].SeriesFormula.Length == 1){
					AddSingleSeries(series[i], masses1, indices1, seriesCount11, seriesCount21, maxMass, i);
				} else{
					AddDoubleSeries(series[i], masses1, indices1, seriesCount11, seriesCount21, maxMass, i);
				}
			}
			int[] first;
			int[][] inds = Sort(masses1, indices1, seriesCount11, seriesCount21, out first);
			masses = ArrayUtils.SubArray(masses1, first);
			indices = Extract(indices1, inds);
			seriesCount1 = Extract(seriesCount11, inds);
			seriesCount2 = Extract(seriesCount21, inds);
			Compositions = new string[masses.Length];
			for (int i = 0; i < Compositions.Length; i++){
				Compositions[i] = GetComposition(GetEntriesAt(i)[0], seriesCount1[i][0], seriesCount2[i][0]);
			}
		}

		private int[][] Sort(List<double> masses1, List<int> indices1, List<int> seriesCount11, List<int> seriesCount21,
			out int[] first){
			string[] compositions1 = new string[masses1.Count];
			for (int i = 0; i < compositions1.Length; i++){
				ChebiEntry e = seriesCount11[i] >= 0 ? series[indices1[i]] : main[indices1[i]];
				compositions1[i] = GetComposition(e, seriesCount11[i], seriesCount21[i]);
			}
			Dictionary<string, Tuple<List<int>, double>> x = new Dictionary<string, Tuple<List<int>, double>>();
			for (int i = 0; i < masses1.Count; i++){
				if (!x.ContainsKey(compositions1[i])){
					x.Add(compositions1[i], new Tuple<List<int>, double>(new List<int>(), masses1[i]));
				}
				x[compositions1[i]].Item1.Add(i);
			}
			string[] keys = SortKeys(x);
			int[][] result = new int[keys.Length][];
			for (int i = 0; i < result.Length; i++){
				result[i] = x[keys[i]].Item1.ToArray();
			}
			first = new int[result.Length];
			for (int i = 0; i < result.Length; i++){
				first[i] = result[i][0];
			}
			return result;
		}

		private static string[] SortKeys(Dictionary<string, Tuple<List<int>, double>> x){
			string[] keys = x.Keys.ToArray();
			double[] m = new double[keys.Length];
			for (int i = 0; i < keys.Length; i++){
				m[i] = x[keys[i]].Item2;
			}
			int[] o = ArrayUtils.Order(m);
			return ArrayUtils.SubArray(keys, o);
		}

		private static string GetComposition(ChebiEntry e, int seriesCount1, int seriesCount2){
			int missingH = 0;
			Molecule m = new Molecule(e.Formula);
			if (e.Charge > 0){
				int n = e.Charge;
				int hc = m.GetHCount();
				if (n > hc){
					missingH = n - hc;
					n = hc;
				}
				m = Molecule.Subtract(m, new Molecule("H" + n));
			}
			if (e.Charge < 0){
				m = Molecule.Sum(m, new Molecule("H" + Math.Abs(e.Charge)));
			}
			if (e.SeriesFormula.Length > 0){
				for (int j = 0; j < seriesCount1; j++){
					m = Molecule.Sum(m, new Molecule(e.SeriesFormula[0]));
				}
			}
			if (e.SeriesFormula.Length > 1){
				for (int j = 0; j < seriesCount2; j++){
					m = Molecule.Sum(m, new Molecule(e.SeriesFormula[1]));
				}
			}
			string comp = m.GetEmpiricalFormula(false);
			if (missingH > 0){
				comp += "-H";
				if (missingH > 1){
					comp += "" + missingH;
				}
			}
			return comp;
		}

		public ChebiEntry[] GetEntriesAt(int ind){
			int[] inds = indices[ind];
			ChebiEntry[] result = new ChebiEntry[inds.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = seriesCount1[ind][i] >= 0 ? series[inds[i]] : main[inds[i]];
			}
			return result;
		}

		private static T[][] Extract<T>(IList<T> array, int[][] inds){
			T[][] result = new T[inds.Length][];
			for (int i = 0; i < inds.Length; i++){
				result[i] = ArrayUtils.SubArray(array, inds[i]);
			}
			return result;
		}

		private static void AddSingleSeries(ChebiEntry e, List<double> masses1, List<int> indices1, List<int> seriesCount11,
			List<int> seriesCount21, double maxMass, int ind){
			for (int i = 1; i < int.MaxValue; i++){
				double m = e.MonoisotopicMass - e.Charge*Molecule.massH + i*e.SeriesMonoisotopicMass[0];
				if (m > maxMass){
					break;
				}
				masses1.Add(m);
				indices1.Add(ind);
				seriesCount11.Add(i);
				seriesCount21.Add(-1);
			}
		}

		private static void AddDoubleSeries(ChebiEntry e, List<double> masses1, List<int> indices1, List<int> seriesCount11,
			List<int> seriesCount21, double maxMass, int ind){
			for (int i = 0; i < int.MaxValue; i++){
				for (int j = 0; j < int.MaxValue; j++){
					if (i == 0 && j == 0){
						continue;
					}
					double m = e.MonoisotopicMass - e.Charge*Molecule.massH + i*e.SeriesMonoisotopicMass[0] +
								j*e.SeriesMonoisotopicMass[1];
					if (m > maxMass){
						break;
					}
					masses1.Add(m);
					indices1.Add(ind);
					seriesCount11.Add(i);
					seriesCount21.Add(-1);
				}
				double m1 = e.MonoisotopicMass - e.Charge*Molecule.massH + i*e.SeriesMonoisotopicMass[0];
				if (m1 > maxMass){
					break;
				}
			}
		}
	}
}