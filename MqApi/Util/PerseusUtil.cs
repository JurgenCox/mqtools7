using MqApi.Calc;
using MqApi.Generic;
using MqApi.Matrix;
using MqApi.Num;
using MqApi.Num.Matrix;
namespace MqApi.Util{
	public static class PerseusUtil{
		public static void ReadMatrixFromFile(IMatrixData mdata, ProcessInfo processInfo, string filename,
			char separator){
			ReadMatrix(mdata, processInfo, () => FileUtils.GetReader(filename), filename, separator);
		}
		public static void ReadMatrix(IMatrixData mdata, ProcessInfo processInfo, Func<StreamReader> getReader,
			string name, char separator){
			ReadMatrixInformation(getReader, separator, out Dictionary<string, string[]> annotationRows,
				out string[] colNames, out int[] eInds, out int[] nInds, out int[] cInds, out int[] tInds,
				out int[] mInds, out List<Tuple<Relation[], int[], bool>> filters, out int nrows,
				out bool hasAdditionalMatrices);
			using (StreamReader reader = getReader()){
				LoadMatrixData(annotationRows, eInds, cInds, nInds, tInds, mInds, processInfo, colNames, mdata, reader,
					nrows, name, separator, false, hasAdditionalMatrices, filters, false, -1);
			}
		}
		public static void ReadMatrixInformation(Func<StreamReader> getReader, char separator,
			out Dictionary<string, string[]> annotationRows, out string[] colNames, out int[] eInds, out int[] nInds,
			out int[] cInds, out int[] tInds, out int[] mInds, out List<Tuple<Relation[], int[], bool>> filters,
			out int nrows, out bool hasAdditionalMatrices){
			annotationRows = new Dictionary<string, string[]>();
			using (StreamReader reader = getReader()){
				colNames = TabSep.GetColumnNames(reader, 0, StringUtils.commentPrefix,
					StringUtils.commentPrefixExceptions, annotationRows,
					separator);
			}
			string[] typeRow = annotationRows["Type"];
			ColumnIndices(typeRow, out eInds, out nInds, out cInds, out tInds, out mInds);
			using (StreamReader reader = getReader()){
				hasAdditionalMatrices = GetHasAddtlMatrices(reader, eInds, separator);
			}
			filters = new List<Tuple<Relation[], int[], bool>>();
			using (StreamReader reader = getReader()){
				nrows = GetRowCount(reader, filters, separator, hasAdditionalMatrices, false, -1);
			}
		}
		public static int GetRowCount(StreamReader reader, List<Tuple<Relation[], int[], bool>> filters, char separator,
			bool addtlMatrices, bool limit, int nlimit){
			reader.BaseStream.Seek(0, SeekOrigin.Begin);
			reader.ReadLine();
			int count = 0;
			string line;
			while ((line = reader.ReadLine()) != null){
				if (TabSep.IsCommentLine(line, StringUtils.commentPrefix, StringUtils.commentPrefixExceptions)){
					continue;
				}
				if (IsValidLine(line, separator, filters, addtlMatrices)){
					count++;
					if (limit && count >= nlimit){
						return count;
					}
				}
			}
			return count;
		}
		private static bool IsValidLine(string line, char separator, List<Tuple<Relation[], int[], bool>> filters,
			bool hasAddtlMatrices){
			if (filters == null || filters.Count == 0){
				return true;
			}
			string[] w = SplitLine(line, separator);
			foreach (Tuple<Relation[], int[], bool> filter in filters){
				if (!IsValidRowNumFilter(ToDoubles(w.SubArray(filter.Item2), hasAddtlMatrices), filter.Item1,
					    filter.Item3)){
					return false;
				}
			}
			return true;
		}
		public static bool IsValidRowNumFilter(double[] row, Relation[] relations, bool and){
			Dictionary<int, double> vars = new Dictionary<int, double>();
			for (int j = 0; j < row.Length; j++){
				vars.Add(j, row[j]);
			}
			bool[] results = new bool[relations.Length];
			for (int j = 0; j < relations.Length; j++){
				results[j] = relations[j].NumEvaluateDouble(vars);
			}
			return and ? ArrayUtils.And(results) : ArrayUtils.Or(results);
		}
		private static string[] SplitLine(string line, char separator){
			string[] tokens = line.Split(separator);
			string[] words = tokens.Aggregate((words: new List<string>(), word: string.Empty), (acc, token) => {
				(List<string> wordList, string word) = acc;
				bool hasUnbalancedQuotes = token.Count(x => x == '\"') % 2 != 0;
				if (string.IsNullOrEmpty(word) && hasUnbalancedQuotes){
					word = token;
				} else{
					if (string.IsNullOrEmpty(word)) // quotation not used
					{
						wordList.Add(token);
					} else if (hasUnbalancedQuotes) // end of quotation regardless of \" position
					{
						wordList.Add(String.Join(separator.ToString(), word, token));
						word = string.Empty;
					} else // token still quoted
					{
						word = string.Join(separator.ToString(), word, token);
					}
				}
				return (wordList, word);
			}, acc => {
				(List<string> wordList, string word) = acc;
				if (!string.IsNullOrEmpty(word)){
					throw new ArgumentException($"Line {line} contains unbalanced quotation marks.");
				}
				return wordList.ToArray();
			});
			return words;
		}
		private static bool IsValidLine(string line, char separator, List<Tuple<Relation[], int[], bool>> filters,
			out string[] split, bool hasAddtlMatrices){
			if (filters == null || filters.Count == 0){
				split = SplitLine(line, separator);
				return true;
			}
			split = SplitLine(line, separator);
			foreach (Tuple<Relation[], int[], bool> filter in filters){
				if (!IsValidRowNumFilter(ToDoubles(split.SubArray(filter.Item2), hasAddtlMatrices), filter.Item1,
					    filter.Item3)){
					return false;
				}
			}
			return true;
		}
		private static double[] ToDoubles(string[] s1, bool hasAddtlMatrices){
			double[] result = new double[s1.Length];
			for (int i = 0; i < s1.Length; i++){
				string s = StringUtils.RemoveWhitespace(s1[i]);
				if (hasAddtlMatrices){
					ParseExp(s, out double f, out bool _, out double _);
					result[i] = f;
				} else{
					bool success = Parser.TryDouble(s, out result[i]);
					if (!success){
						result[i] = double.NaN;
					}
				}
			}
			return result;
		}
		public static bool GetHasAddtlMatrices(TextReader reader, IList<int> expressionColIndices, char separator){
			if (expressionColIndices.Count == 0 || reader == null){
				return false;
			}
			int expressionColIndex = expressionColIndices[0];
			reader.ReadLine();
			string line;
			bool hasAddtl = false;
			while ((line = reader.ReadLine()) != null){
				if (TabSep.IsCommentLine(line, StringUtils.commentPrefix, StringUtils.commentPrefixExceptions)){
					continue;
				}
				string[] w = SplitLine(line, separator);
				if (expressionColIndex < w.Length){
					string s = StringUtils.RemoveWhitespace(w[expressionColIndex]);
					hasAddtl = s.Contains(";");
					break;
				}
			}
			reader.Close();
			return hasAddtl;
		}
		private static void ColumnIndices(string[] typeRow, out int[] eInds, out int[] nInds, out int[] cInds,
			out int[] tInds, out int[] mInds){
			List<int> _eInds = new List<int>();
			List<int> _nInds = new List<int>();
			List<int> _cInds = new List<int>();
			List<int> _tInds = new List<int>();
			List<int> _mInds = new List<int>();
			for (int i = 0; i < typeRow.Length; i++){
				switch (typeRow[i]){
					case "E":
						_eInds.Add(i);
						break;
					case "N":
						_nInds.Add(i);
						break;
					case "C":
						_cInds.Add(i);
						break;
					case "T":
						_tInds.Add(i);
						break;
					case "M":
						_mInds.Add(i);
						break;
				}
			}
			eInds = _eInds.ToArray();
			nInds = _nInds.ToArray();
			cInds = _cInds.ToArray();
			tInds = _tInds.ToArray();
			mInds = _mInds.ToArray();
		}
		private static void LoadMatrixData(IList<string> colNames, IList<string> colDescriptions,
			IList<int> mainColIndices, IList<int> catColIndices, IList<int> numColIndices, IList<int> textColIndices,
			IList<int> multiNumColIndices, string origin, IMatrixData matrixData, Action<int> progress,
			Action<string> status, char separator, TextReader reader, int nrows, bool shortenExpressionNames,
			List<Tuple<Relation[], int[], bool>> filters, bool addtlMatrices, bool limit, int nlimit){
			status("Reading data");
			LoadAllData(matrixData, colNames, mainColIndices, catColIndices, numColIndices, textColIndices,
				multiNumColIndices, reader, separator, nrows, filters, progress, addtlMatrices,
				out double[,] qualityValues, out bool[,] isImputedValues, out FloatMatrixIndexer mainValues, limit,
				nlimit);
			//AddColumnDescriptions(colDescriptions, catColIndices, numColIndices, textColIndices, multiNumColIndices, matrixData);
			//	AddMainColumnDescriptions(colDescriptions, mainColIndices, matrixData);
			matrixData.Name = origin;
			string[] columnNames = colNames.SubArray(mainColIndices);
			if (shortenExpressionNames){
				columnNames = StringUtils.RemoveCommonSubstrings(columnNames, true);
			}
			matrixData.ColumnNames = RemoveQuotes(columnNames);
			matrixData.Values = mainValues;
			if (addtlMatrices){
				matrixData.Quality.Set(qualityValues);
				matrixData.IsImputed.Set(isImputedValues);
			} else{
				//TODO
				matrixData.Quality = new FloatMatrixIndexer(0, mainValues.RowCount, mainValues.ColumnCount);
				matrixData.IsImputed = new BoolMatrixIndexer(false, mainValues.RowCount, mainValues.ColumnCount);
			}
			matrixData.Origin = origin;
			progress(0);
			status("");
		}
		private static List<string> RemoveQuotes(IEnumerable<string> names){
			List<string> result = new List<string>();
			foreach (string name in names){
				if (name.Length > 2 && name.StartsWith("\"") && name.EndsWith("\"")){
					result.Add(name.Substring(1, name.Length - 2));
				} else{
					result.Add(name);
				}
			}
			return result;
		}
		private static string RemoveQuotes(string name){
			if (name.Length > 2 && name.StartsWith("\"") && name.EndsWith("\"")){
				return name.Substring(1, name.Length - 2);
			}
			return name;
		}
		public static void LoadMatrixData(IDictionary<string, string[]> annotationRows, int[] mainColIndices,
			int[] catColIndices, int[] numColIndices, int[] textColIndices, int[] multiNumColIndices,
			ProcessInfo processInfo, IList<string> colNames, IMatrixData mdata, StreamReader reader, int nrows,
			string origin, char separator, bool shortenExpressionNames, bool hasAdditionalMatrices,
			List<Tuple<Relation[], int[], bool>> filters, bool limit, int nlimit){
			string[] colDescriptions = null;
			if (annotationRows.ContainsKey("Description")){
				colDescriptions = annotationRows["Description"];
				annotationRows.Remove("Description");
			}
			if (HasBadParameters(mainColIndices, catColIndices, numColIndices, textColIndices, multiNumColIndices,
				    processInfo, colNames)){
				return;
			}
			LoadMatrixData(colNames, colDescriptions, mainColIndices, catColIndices, numColIndices, textColIndices,
				multiNumColIndices, origin, mdata, processInfo.Progress, processInfo.Status, separator, reader, nrows,
				shortenExpressionNames, filters, hasAdditionalMatrices, limit, nlimit);
			AddAnnotationRows(mainColIndices, mdata, annotationRows);
		}
		private static void AddAnnotationRows(IList<int> mainColIndices, IDataWithAnnotationRows matrixData,
			IDictionary<string, string[]> annotationRows){
			SplitAnnotRows(annotationRows, out Dictionary<string, string[]> catAnnotatRows,
				out Dictionary<string, string[]> numAnnotatRows);
			foreach (string key in catAnnotatRows.Keys){
				string name = key;
				string[] svals = catAnnotatRows[key].SubArray(mainColIndices);
				string[][] cat = new string[svals.Length][];
				for (int i = 0; i < cat.Length; i++){
					string s = svals[i].Trim();
					cat[i] = s.Length > 0 ? s.Split(';') : new string[0];
					List<int> valids = new List<int>();
					for (int j = 0; j < cat[i].Length; j++){
						cat[i][j] = cat[i][j].Trim();
						if (cat[i][j].Length > 0){
							valids.Add(j);
						}
					}
					cat[i] = cat[i].SubArray(valids);
					Array.Sort(cat[i]);
				}
				matrixData.AddCategoryRow(name, name, cat);
			}
			foreach (string key in numAnnotatRows.Keys){
				string name = key;
				string[] svals = numAnnotatRows[key].SubArray(mainColIndices);
				double[] num = new double[svals.Length];
				for (int i = 0; i < num.Length; i++){
					string s = svals[i].Trim();
					if (!Parser.TryDouble(s, out num[i])){
						num[i] = double.NaN;
					}
				}
				matrixData.AddNumericRow(name, name, num);
			}
		}
		private static void SplitAnnotRows(IDictionary<string, string[]> annotRows,
			out Dictionary<string, string[]> catAnnotRows, out Dictionary<string, string[]> numAnnotRows){
			catAnnotRows = new Dictionary<string, string[]>();
			numAnnotRows = new Dictionary<string, string[]>();
			foreach (string name in annotRows.Keys){
				if (name.StartsWith("N:")){
					numAnnotRows.Add(name.Substring(2), annotRows[name]);
				} else if (name.StartsWith("C:")){
					catAnnotRows.Add(name.Substring(2), annotRows[name]);
				}
			}
		}
		/// <summary>
		/// Check for duplicate column selections
		/// </summary>
		private static bool HasBadParameters(int[] mainColIndices, int[] catColIndices, int[] numColIndices,
			int[] textColIndices, int[] multiNumColIndices, ProcessInfo processInfo, IList<string> colNames){
			int[] allInds = ArrayUtils.Concat(new[]{
				mainColIndices, catColIndices, numColIndices, textColIndices, multiNumColIndices
			});
			Array.Sort(allInds);
			for (int i = 0; i < allInds.Length - 1; i++){
				if (allInds[i + 1] == allInds[i]){
					processInfo.ErrString = "Column '" + colNames[allInds[i]] + "' has been selected multiple times";
					return true;
				}
			}
			string[] allColNames = colNames.SubArray(allInds);
			Array.Sort(allColNames);
			for (int i = 0; i < allColNames.Length - 1; i++){
				if (allColNames[i + 1].Equals(allColNames[i])){
					processInfo.ErrString = "Column name '" + allColNames[i] + "' occurs multiple times.";
					return true;
				}
			}
			return false;
		}
		public static void LoadAllData(IDataWithAnnotationColumns matrixData, IList<string> colNames,
			IList<int> mainColIndices, IList<int> catColIndices, IList<int> numColIndices, IList<int> textColIndices,
			IList<int> multiNumColIndices, TextReader reader, char separator, int nrows,
			List<Tuple<Relation[], int[], bool>> filters, Action<int> progress, bool addtlMatrices,
			out double[,] qualityValues, out bool[,] isImputedValues, out FloatMatrixIndexer mainValues, bool limit,
			int nlimit){
			InitializeAnnotationColumns(catColIndices, numColIndices, textColIndices, multiNumColIndices, nrows,
				out List<string[][]> categoryAnnotation, out List<double[]> numericAnnotation,
				out List<double[][]> multiNumericAnnotation, out List<string[]> stringAnnotation);
			mainValues = InitializeMainValues(mainColIndices, nrows, addtlMatrices, out qualityValues,
				out isImputedValues);
			reader.ReadLine();
			int count = 0;
			string line;
			while ((line = reader.ReadLine()) != null){
				if (SkipCommentOrInvalid(separator, filters, addtlMatrices, line, out string[] words)){
					continue;
				}
				progress(100 * (count + 1) / nrows);
				ReadMainColumns(mainColIndices, addtlMatrices, words, mainValues, count, isImputedValues,
					qualityValues);
				ReadAnnotationColumns(catColIndices, numColIndices, textColIndices, multiNumColIndices, words,
					numericAnnotation, count, multiNumericAnnotation, categoryAnnotation, stringAnnotation);
				count++;
				if (limit && count >= nlimit){
					break;
				}
			}
			reader.Close();
			string[] catColnames = colNames.SubArray(catColIndices);
			string[] numColnames = colNames.SubArray(numColIndices);
			string[] multiNumColnames = colNames.SubArray(multiNumColIndices);
			string[] textColnames = colNames.SubArray(textColIndices);
			matrixData.SetAnnotationColumns(RemoveQuotes(textColnames), stringAnnotation, RemoveQuotes(catColnames),
				categoryAnnotation, RemoveQuotes(numColnames), numericAnnotation, RemoveQuotes(multiNumColnames),
				multiNumericAnnotation);
		}
		private static void ReadMainColumns(IList<int> mainColIndices, bool addtlMatrices, string[] words,
			FloatMatrixIndexer mainValues, int count, bool[,] isImputedValues, double[,] qualityValues){
			for (int i = 0; i < mainColIndices.Count; i++){
				if (mainColIndices[i] >= words.Length){
					mainValues[count, i] = double.NaN;
				} else{
					string s = StringUtils.RemoveWhitespace(words[mainColIndices[i]]);
					if (addtlMatrices){
						ParseExp(s, out double mv, out isImputedValues[count, i], out qualityValues[count, i]);
						mainValues[count, i] = mv;
					} else{
						if (count < mainValues.RowCount){
							bool success = Parser.TryDouble(s, out double mv);
							mainValues[count, i] = mv;
							if (!success){
								mainValues[count, i] = double.NaN;
							}
						}
					}
				}
			}
		}
		private static bool SkipCommentOrInvalid(char separator, List<Tuple<Relation[], int[], bool>> filters,
			bool addtlMatrices, string line, out string[] w){
			w = new string[0];
			if (TabSep.IsCommentLine(line, StringUtils.commentPrefix, StringUtils.commentPrefixExceptions)){
				return true;
			}
			if (!IsValidLine(line, separator, filters, out w, addtlMatrices)){
				return true;
			}
			return false;
		}
		private static FloatMatrixIndexer InitializeMainValues(IList<int> mainColIndices, int nrows, bool addtlMatrices,
			out double[,] qualityValues, out bool[,] isImputedValues){
			FloatMatrixIndexer mainValues = new FloatMatrixIndexer();
			mainValues.Init(nrows, mainColIndices.Count);
			qualityValues = null;
			isImputedValues = null;
			if (addtlMatrices){
				qualityValues = new double[nrows, mainColIndices.Count];
				isImputedValues = new bool[nrows, mainColIndices.Count];
			}
			return mainValues;
		}
		private static void InitializeAnnotationColumns(IList<int> catColIndices, IList<int> numColIndices,
			IList<int> textColIndices, IList<int> multiNumColIndices, int nrows,
			out List<string[][]> categoryAnnotation, out List<double[]> numericAnnotation,
			out List<double[][]> multiNumericAnnotation, out List<string[]> stringAnnotation){
			categoryAnnotation = new List<string[][]>();
			for (int i = 0; i < catColIndices.Count; i++){
				categoryAnnotation.Add(new string[nrows][]);
			}
			numericAnnotation = new List<double[]>();
			for (int i = 0; i < numColIndices.Count; i++){
				numericAnnotation.Add(new double[nrows]);
			}
			multiNumericAnnotation = new List<double[][]>();
			for (int i = 0; i < multiNumColIndices.Count; i++){
				multiNumericAnnotation.Add(new double[nrows][]);
			}
			stringAnnotation = new List<string[]>();
			for (int i = 0; i < textColIndices.Count; i++){
				stringAnnotation.Add(new string[nrows]);
			}
		}
		private static void ReadAnnotationColumns(IList<int> catColIndices, IList<int> numColIndices,
			IList<int> textColIndices, IList<int> multiNumColIndices, string[] words, List<double[]> numericAnnotation,
			int count, List<double[][]> multiNumericAnnotation, List<string[][]> categoryAnnotation,
			List<string[]> stringAnnotation){
			for (int i = 0; i < numColIndices.Count; i++){
				if (numColIndices[i] >= words.Length){
					numericAnnotation[i][count] = double.NaN;
				} else{
					bool success = Parser.TryDouble(words[numColIndices[i]].Trim(), out double q);
					if (numericAnnotation[i].Length > count){
						numericAnnotation[i][count] = success ? q : double.NaN;
					}
				}
			}
			for (int i = 0; i < multiNumColIndices.Count; i++){
				if (multiNumColIndices[i] >= words.Length){
					multiNumericAnnotation[i][count] = new double[0];
				} else{
					string q = words[multiNumColIndices[i]].Trim();
					if (q.Length >= 2 && q[0] == '\"' && q[q.Length - 1] == '\"'){
						q = q.Substring(1, q.Length - 2);
					}
					if (q.Length >= 2 && q[0] == '\'' && q[q.Length - 1] == '\''){
						q = q.Substring(1, q.Length - 2);
					}
					string[] ww = q.Length == 0 ? new string[0] : q.Split(';');
					multiNumericAnnotation[i][count] = new double[ww.Length];
					for (int j = 0; j < ww.Length; j++){
						bool success = Parser.TryDouble(ww[j], out double q1);
						multiNumericAnnotation[i][count][j] = success ? q1 : double.NaN;
					}
				}
			}
			for (int i = 0; i < catColIndices.Count; i++){
				if (catColIndices[i] >= words.Length){
					categoryAnnotation[i][count] = new string[0];
				} else{
					string q = words[catColIndices[i]].Trim();
					if (q.Length >= 2 && q[0] == '\"' && q[q.Length - 1] == '\"'){
						q = q.Substring(1, q.Length - 2);
					}
					if (q.Length >= 2 && q[0] == '\'' && q[q.Length - 1] == '\''){
						q = q.Substring(1, q.Length - 2);
					}
					string[] ww = q.Length == 0 ? new string[0] : q.Split(';');
					List<int> valids = new List<int>();
					for (int j = 0; j < ww.Length; j++){
						ww[j] = ww[j].Trim();
						if (ww[j].Length > 0){
							valids.Add(j);
						}
					}
					ww = ww.SubArray(valids);
					Array.Sort(ww);
					if (categoryAnnotation[i].Length > count){
						categoryAnnotation[i][count] = ww;
					}
				}
			}
			for (int i = 0; i < textColIndices.Count; i++){
				if (textColIndices[i] >= words.Length){
					stringAnnotation[i][count] = "";
				} else{
					string q = words[textColIndices[i]].Trim();
					if (stringAnnotation[i].Length > count){
						stringAnnotation[i][count] = RemoveSplitWhitespace(RemoveQuotes(q));
					}
				}
			}
		}
		private static string RemoveSplitWhitespace(string s){
			if (!s.Contains(";")){
				return s.Trim();
			}
			string[] q = s.Split(';');
			for (int i = 0; i < q.Length; i++){
				q[i] = q[i].Trim();
			}
			return StringUtils.Concat(";", q);
		}
		private static void ParseExp(string s, out double expressionValue, out bool isImputedValue,
			out double qualityValue){
			string[] w = s.Split(';');
			expressionValue = double.NaN;
			isImputedValue = false;
			qualityValue = double.NaN;
			if (w.Length > 0){
				bool success = Parser.TryDouble(w[0], out expressionValue);
				if (!success){
					expressionValue = double.NaN;
				}
			}
			if (w.Length > 1){
				bool success = bool.TryParse(w[1], out isImputedValue);
				if (!success){
					isImputedValue = false;
				}
			}
			if (w.Length > 2){
				bool success = Parser.TryDouble(w[2], out qualityValue);
				if (!success){
					qualityValue = double.NaN;
				}
			}
		}
		/// <summary>
		/// Write data table to file with tab separation.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="filename"></param>
		public static void WriteDataWithAnnotationColumns(IDataWithAnnotationColumns data, string filename){
			using (StreamWriter writer = new StreamWriter(filename)){
				WriteDataWithAnnotationColumns(data, writer);
			}
		}
		/// <summary>
		/// Write data table to file with tab separation.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="filename"></param>
		public static void WriteDataWithAnnotationRows(IDataWithAnnotationRows data, string filename){
			using (StreamWriter writer = new StreamWriter(filename)){
				WriteDataWithAnnotationRows(data, writer);
			}
		}
		/// <summary>
		/// Write data table to stream with tab separation.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="writer"></param>
		public static void WriteDataWithAnnotationColumns(IDataWithAnnotationColumns data, StreamWriter writer){
			IEnumerable<string> columnNames = ColumnNames(data);
			writer.WriteLine(StringUtils.Concat("\t", columnNames));
			if (HasAnyDescription(data)){
				IEnumerable<string> columnDescriptions = ColumnDescriptions(data);
				writer.WriteLine("#!{Description}" + StringUtils.Concat("\t", columnDescriptions));
			}
			IEnumerable<string> columnTypes = ColumnTypes(data);
			writer.WriteLine("#!{Type}" + StringUtils.Concat("\t", columnTypes));
			IEnumerable<string> dataRows = DataAnnotationRows(data);
			foreach (string row in dataRows){
				writer.WriteLine(row);
			}
		}
		public static IEnumerable<string> ColumnTypes(IMatrixData data){
			List<string> words = new List<string>();
			for (int i = 0; i < data.ColumnCount; i++){
				words.Add("E");
			}
			return words.Concat(ColumnTypes((IDataWithAnnotationColumns) data));
		}
		private static IEnumerable<string> ColumnTypes(IDataWithAnnotationColumns data){
			List<string> words = new List<string>();
			for (int i = 0; i < data.CategoryColumnCount; i++){
				words.Add("C");
			}
			for (int i = 0; i < data.NumericColumnCount; i++){
				words.Add("N");
			}
			for (int i = 0; i < data.StringColumnCount; i++){
				words.Add("T");
			}
			for (int i = 0; i < data.MultiNumericColumnCount; i++){
				words.Add("M");
			}
			return words;
		}
		public static IEnumerable<string> ColumnNames(IDataWithAnnotationColumns data){
			List<string> words = new List<string>();
			for (int i = 0; i < data.CategoryColumnCount; i++){
				words.Add(data.CategoryColumnNames[i]);
			}
			for (int i = 0; i < data.NumericColumnCount; i++){
				words.Add(data.NumericColumnNames[i]);
			}
			for (int i = 0; i < data.StringColumnCount; i++){
				words.Add(data.StringColumnNames[i]);
			}
			for (int i = 0; i < data.MultiNumericColumnCount; i++){
				words.Add(data.MultiNumericColumnNames[i]);
			}
			return words;
		}
		private static IEnumerable<string> ColumnNames(IDataWithAnnotationRows data){
			List<string> words = new List<string>();
			for (int i = 0; i < data.CategoryRowCount; i++){
				words.Add(data.CategoryRowNames[i]);
			}
			for (int i = 0; i < data.NumericRowCount; i++){
				words.Add(data.NumericRowNames[i]);
			}
			for (int i = 0; i < data.StringRowCount; i++){
				words.Add(data.StringRowNames[i]);
			}
			for (int i = 0; i < data.MultiNumericRowCount; i++){
				words.Add(data.MultiNumericRowNames[i]);
			}
			return words;
		}
		/// <summary>
		/// True if any column description is set.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static bool HasAnyDescription(IMatrixData data){
			for (int i = 0; i < data.ColumnCount; i++){
				if (data.ColumnDescriptions[i] != null && data.ColumnDescriptions[i].Length > 0){
					return true;
				}
			}
			return HasAnyDescription((IDataWithAnnotationColumns) data);
		}
		private static bool HasAnyDescription(IDataWithAnnotationColumns data){
			for (int i = 0; i < data.CategoryColumnCount; i++){
				if (data.CategoryColumnDescriptions[i] != null && data.CategoryColumnDescriptions[i].Length > 0){
					return true;
				}
			}
			for (int i = 0; i < data.NumericColumnCount; i++){
				if (data.NumericColumnDescriptions[i] != null && data.NumericColumnDescriptions[i].Length > 0){
					return true;
				}
			}
			for (int i = 0; i < data.StringColumnCount; i++){
				if (data.StringColumnDescriptions[i] != null && data.StringColumnDescriptions[i].Length > 0){
					return true;
				}
			}
			for (int i = 0; i < data.MultiNumericColumnCount; i++){
				if (data.MultiNumericColumnDescriptions[i] != null &&
				    data.MultiNumericColumnDescriptions[i].Length > 0){
					return true;
				}
			}
			return false;
		}
		private static IEnumerable<string> DataAnnotationRows(IDataWithAnnotationColumns data){
			for (int i = 0; i < data.RowCount; i++){
				yield return StringUtils.Concat("\t", DataAnnotationRow(data, i));
			}
		}
		private static IEnumerable<string> DataAnnotationRows(IDataWithAnnotationRows data){
			for (int i = 0; i < data.ColumnCount; i++){
				yield return StringUtils.Concat("\t", DataAnnotationRow(data, i));
			}
		}
		private static IEnumerable<string> DataAnnotationRow(IDataWithAnnotationRows data, int j){
			List<string> words = new List<string>();
			for (int i = 0; i < data.CategoryRowCount; i++){
				string[] q = data.GetCategoryRowEntryAt(i, j) ?? new string[0];
				words.Add(q.Length > 0 ? StringUtils.Concat(";", q) : "");
			}
			for (int i = 0; i < data.NumericRowCount; i++){
				words.Add(data.NumericRows[i][j].ToString());
			}
			for (int i = 0; i < data.StringRowCount; i++){
				words.Add(data.StringRows[i][j] ?? string.Empty);
			}
			for (int i = 0; i < data.MultiNumericRowCount; i++){
				double[] q = data.MultiNumericRows[i][j];
				words.Add(q != null && q.Length > 0 ? StringUtils.Concat(";", q) : "");
			}
			return words;
		}
		public static IEnumerable<string> DataAnnotationRow(IDataWithAnnotationColumns data, int j){
			List<string> words = new List<string>();
			for (int i = 0; i < data.CategoryColumnCount; i++){
				string[] q = data.GetCategoryColumnEntryAt(i, j) ?? new string[0];
				words.Add(q.Length > 0 ? StringUtils.Concat(";", q) : "");
			}
			for (int i = 0; i < data.NumericColumnCount; i++){
				words.Add(data.NumericColumns[i][j].ToString());
			}
			for (int i = 0; i < data.StringColumnCount; i++){
				words.Add(data.StringColumns[i][j] ?? string.Empty);
			}
			for (int i = 0; i < data.MultiNumericColumnCount; i++){
				double[] q = data.MultiNumericColumns[i][j];
				words.Add(q != null && q.Length > 0 ? StringUtils.Concat(";", q) : "");
			}
			return words;
		}
		/// <summary>
		/// Write data table to stream with tab separation.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="writer"></param>
		public static void WriteDataWithAnnotationRows(IDataWithAnnotationRows data, StreamWriter writer){
			IEnumerable<string> columnNames = ColumnNames(data);
			writer.WriteLine(StringUtils.Concat("\t", columnNames));
			if (HasAnyDescription(data)){
				IEnumerable<string> columnDescriptions = ColumnDescriptions(data);
				writer.WriteLine("#!{Description}" + StringUtils.Concat("\t", columnDescriptions));
			}
			IEnumerable<string> columnTypes = ColumnTypes(data);
			writer.WriteLine("#!{Type}" + StringUtils.Concat("\t", columnTypes));
			IEnumerable<string> dataRows = DataAnnotationRows(data);
			foreach (string row in dataRows){
				writer.WriteLine(row);
			}
		}
		private static IEnumerable<string> ColumnTypes(IDataWithAnnotationRows data){
			List<string> words = new List<string>();
			for (int i = 0; i < data.CategoryRowCount; i++){
				words.Add("C");
			}
			for (int i = 0; i < data.NumericRowCount; i++){
				words.Add("N");
			}
			for (int i = 0; i < data.StringRowCount; i++){
				words.Add("T");
			}
			for (int i = 0; i < data.MultiNumericRowCount; i++){
				words.Add("M");
			}
			return words;
		}
		private static IEnumerable<string> ColumnDescriptions(IMatrixData data){
			List<string> words = new List<string>();
			for (int i = 0; i < data.ColumnCount; i++){
				words.Add(data.ColumnDescriptions[i] ?? "");
			}
			return words.Concat(ColumnDescriptions((IDataWithAnnotationColumns) data));
		}
		private static IEnumerable<string> ColumnDescriptions(IDataWithAnnotationColumns data){
			List<string> words = new List<string>();
			for (int i = 0; i < data.CategoryColumnCount; i++){
				words.Add(data.CategoryColumnDescriptions[i] ?? "");
			}
			for (int i = 0; i < data.NumericColumnCount; i++){
				words.Add(data.NumericColumnDescriptions[i] ?? "");
			}
			for (int i = 0; i < data.StringColumnCount; i++){
				words.Add(data.StringColumnDescriptions[i] ?? "");
			}
			for (int i = 0; i < data.MultiNumericColumnCount; i++){
				words.Add(data.MultiNumericColumnDescriptions[i] ?? "");
			}
			return words;
		}
		private static IEnumerable<string> ColumnDescriptions(IDataWithAnnotationRows data){
			List<string> words = new List<string>();
			for (int i = 0; i < data.CategoryRowCount; i++){
				words.Add(data.CategoryRowDescriptions[i] ?? "");
			}
			for (int i = 0; i < data.NumericRowCount; i++){
				words.Add(data.NumericRowDescriptions[i] ?? "");
			}
			for (int i = 0; i < data.StringRowCount; i++){
				words.Add(data.StringRowDescriptions[i] ?? "");
			}
			for (int i = 0; i < data.MultiNumericRowCount; i++){
				words.Add(data.MultiNumericRowDescriptions[i] ?? "");
			}
			return words;
		}
		private static IEnumerable<string> ColumnNames(IMatrixData data){
			List<string> words = new List<string>();
			for (int i = 0; i < data.ColumnCount; i++){
				words.Add(data.ColumnNames[i]);
			}
			return words.Concat(ColumnNames((IDataWithAnnotationColumns) data));
		}
		private static bool HasAnyDescription(IDataWithAnnotationRows data){
			for (int i = 0; i < data.CategoryRowCount; i++){
				if (data.CategoryRowDescriptions[i] != null && data.CategoryRowDescriptions[i].Length > 0){
					return true;
				}
			}
			for (int i = 0; i < data.NumericRowCount; i++){
				if (data.NumericRowDescriptions[i] != null && data.NumericRowDescriptions[i].Length > 0){
					return true;
				}
			}
			for (int i = 0; i < data.StringRowCount; i++){
				if (data.StringRowDescriptions[i] != null && data.StringRowDescriptions[i].Length > 0){
					return true;
				}
			}
			for (int i = 0; i < data.MultiNumericRowCount; i++){
				if (data.MultiNumericRowDescriptions[i] != null && data.MultiNumericRowDescriptions[i].Length > 0){
					return true;
				}
			}
			return false;
		}
	}
}