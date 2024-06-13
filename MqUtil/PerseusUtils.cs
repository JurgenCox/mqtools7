using System.Text;
using MqApi.Calc;
using MqApi.Generic;
using MqApi.Matrix;
using MqApi.Network;
using MqApi.Num;
using MqApi.Num.Matrix;
using MqApi.Param;
using MqApi.Util;
namespace MqUtil{
	public static class PerseusUtils{
		public static void LoadMatrixData(IDictionary<string, string[]> annotationRows, int[] mainColIndices,
			int[] catColIndices, int[] numColIndices, int[] textColIndices, int[] multiNumColIndices,
			ProcessInfo processInfo, IList<string> colNames, IMatrixData mdata, StreamReader reader,
			StreamReader auxReader, int nrows, string origin, char separator, bool shortenExpressionNames,
			List<Tuple<Relation[], int[], bool>> filters){
			bool hasAdditionalMatrices = PerseusUtil.GetHasAddtlMatrices(auxReader, mainColIndices, separator);
			PerseusUtil.LoadMatrixData(annotationRows, mainColIndices, catColIndices, numColIndices, textColIndices,
				multiNumColIndices, processInfo, colNames, mdata, reader, nrows, origin, separator,
				shortenExpressionNames, hasAdditionalMatrices, filters, false, -1);
		}
		public static void LoadDataWithAnnotationColumns(IDataWithAnnotationColumns matrixData, IList<string> colNames,
			IList<int> catColIndices, IList<int> numColIndices, IList<int> textColIndices,
			IList<int> multiNumColIndices, Action<int> progress, char separator, TextReader reader, int nrows,
			List<Tuple<Relation[], int[], bool>> filters){
			PerseusUtil.LoadAllData(matrixData, colNames, new int[0], catColIndices, numColIndices, textColIndices,
				multiNumColIndices, reader, separator, nrows, filters, progress, false, out double[,] _, out bool[,] _,
				out FloatMatrixIndexer _, false, -1);
		}
		/// <summary>
		/// Search the annotation folder for annotations.
		/// </summary>
		/// <param name="baseNames">The name of the base identifier from which the mapping will be performed. For example Uniprot, ENSG</param>
		/// <param name="files"></param>
		/// <returns>A list of annotations for each file. For example <code>{{"Chromosome", "Orientation"},{"KEGG name", "Pfam"}}</code></returns>
		public static string[][] GetAvailableAnnots(out string[] baseNames, out string[] files){
			return GetAvailableAnnots(out baseNames, out AnnotType[][] _, out files, out List<string> _);
		}
		/// <summary>
		/// Search the annotation folder for annotations.
		/// </summary>
		/// <param name="baseNames">The name of the base identifier from which the mapping will be performed. For example UniProt, ENSG</param>
		/// <param name="files"></param>
		/// <param name="badFiles">List of files which could not be processed</param>
		/// <returns>A list of annotations for each file. For example <code>{{"Chromosome", "Orientation"},{"KEGG name", "Pfam"}}</code></returns>
		public static string[][] GetAvailableAnnots(out string[] baseNames, out string[] files,
			out List<string> badFiles){
			return GetAvailableAnnots(out baseNames, out AnnotType[][] _, out files, out badFiles);
		}
		/// <summary>
		/// Search the annotation folder for annotations.
		/// </summary>
		/// <param name="baseNames">The name of the base identifier from which the mapping will be performed. For example UniProt, ENSG</param>
		/// <param name="types"><see cref="AnnotType"/></param>
		/// <param name="files"></param>
		/// <returns>A list of annotations for each file. For example <code>{{"Chromosome", "Orientation"},{"KEGG name", "Pfam"}}</code></returns>
		public static string[][]
			GetAvailableAnnots(out string[] baseNames, out AnnotType[][] types, out string[] files){
			return GetAvailableAnnots(out baseNames, out types, out files, out List<string> _);
		}
		/// <summary>
		/// Search the annotation folder for annotations.
		/// </summary>
		/// <param name="baseNames">The name of the base identifier from which the mapping will be performed. For example UniProt, ENSG</param>
		/// <param name="types"><see cref="AnnotType"/></param>
		/// <param name="files"></param>
		/// <param name="badFiles">List of files which could not be processed</param>
		/// <returns>A list of annotations for each file. For example <code>{{"Chromosome", "Orientation"},{"KEGG name", "Pfam"}}</code></returns>
		public static string[][] GetAvailableAnnots(out string[] baseNames, out AnnotType[][] types, out string[] files,
			out List<string> badFiles){
			List<string> filesList = GetAnnotFiles().ToList();
			badFiles = new List<string>();
			List<string> baseNamesList = new List<string>();
			List<AnnotType[]> typesList = new List<AnnotType[]>();
			List<string[]> annotationNames = new List<string[]>();
			foreach (string file in filesList){
				try{
					string[] name = GetAvailableAnnots(file, out string baseName, out AnnotType[] type);
					if (name == null){
						continue;
					}
					annotationNames.Add(name);
					baseNamesList.Add(baseName);
					typesList.Add(type);
				} catch (Exception){
					badFiles.Add(file);
				}
			}
			foreach (string badFile in badFiles){
				filesList.Remove(badFile);
			}
			files = filesList.ToArray();
			baseNames = baseNamesList.ToArray();
			types = typesList.ToArray();
			return annotationNames.ToArray();
		}
		private static string[] GetAvailableAnnots(string file, out string baseName, out AnnotType[] types){
			StreamReader reader = FileUtils.GetReader(file);
			string line = reader.ReadLine();
			if (line == null){
				baseName = "";
				types = new AnnotType[0];
				return new string[0];
			}
			string[] header = line.Split('\t');
			line = reader.ReadLine();
			string[] desc = line.Split('\t');
			reader.Close();
			baseName = header[0];
			string[] result = header.SubArray(1, header.Length);
			types = new AnnotType[desc.Length - 1];
			for (int i = 0; i < types.Length; i++){
				types[i] = FromString1(desc[i + 1]);
			}
			return result;
		}
		private static AnnotType FromString1(string s){
			switch (s){
				case "Text": return AnnotType.Text;
				case "Categorical": return AnnotType.Categorical;
				case "Numerical": return AnnotType.Numerical;
				default: return AnnotType.Categorical;
			}
		}
		public static string[] GetAnnotFiles(){
			string folder = Path.Combine(FileUtils.executablePath, "conf", "annotations");
			if (!Directory.Exists(folder)){
				return new string[0];
			}
			string[] files = Directory.GetFiles(folder);
			List<string> result = new List<string>();
			foreach (string file in files){
				string fileLow = file.ToLower();
				if (fileLow.EndsWith(".txt.gz") || fileLow.EndsWith(".txt")){
					result.Add(file);
				}
			}
			return result.ToArray();
		}
		/// <summary>
		/// Create numeric filter parameters for each selection, consisting of a column selection, a relations and a combination parameter.
		/// </summary>
		/// <param name="selection"></param>
		/// <returns></returns>
		public static Parameter[] GetNumFilterParams(string[] selection){
			return new[]{
				GetColumnSelectionParameter(selection), GetRelationsParameter(),
				new SingleChoiceParam("Combine through", 0){Values = new[]{"intersection", "union"}}
			};
		}
		private static Parameter GetColumnSelectionParameter(string[] selection){
			const int maxCols = 5;
			string[] values = new string[maxCols];
			Parameters[] subParams = new Parameters[maxCols];
			for (int i = 1; i <= maxCols; i++){
				values[i - 1] = "" + i;
				Parameter[] px = new Parameter[i];
				for (int j = 0; j < i; j++){
					px[j] = new SingleChoiceParam(GetVariableName(j), j){Values = selection};
				}
				Parameters p = new Parameters(px);
				subParams[i - 1] = p;
			}
			return new SingleChoiceWithSubParams("Number of columns", 0){
				Values = values, SubParams = subParams, ParamNameWidth = 120, TotalWidth = 800
			};
		}
		private static string GetVariableName(int i){
			const string x = "xyzabc";
			return "" + x[i];
		}
		private static Parameter GetRelationsParameter(){
			const int maxCols = 5;
			string[] values = new string[maxCols];
			Parameters[] subParams = new Parameters[maxCols];
			for (int i = 1; i <= maxCols; i++){
				values[i - 1] = "" + i;
				Parameter[] px = new Parameter[i];
				for (int j = 0; j < i; j++){
					px[j] = new StringParam("Relation " + (j + 1));
				}
				Parameters p = new Parameters(px);
				subParams[i - 1] = p;
			}
			return new SingleChoiceWithSubParams("Number of relations", 0){
				Values = values, SubParams = subParams, ParamNameWidth = 120, TotalWidth = 800
			};
		}
		public static Relation[] GetRelationsNumFilter(Parameters param, out string errString, out int[] colInds,
			out bool and){
			errString = null;
			if (param == null){
				colInds = new int[0];
				and = false;
				return null;
			}
			and = param.GetParam<int>("Combine through").Value == 0;
			colInds = GetColIndsNumFilter(param, out string[] realVariableNames);
			if (colInds == null || colInds.Length == 0){
				errString = "Please specify at least one column.";
				return null;
			}
			Relation[] relations = GetRelations(param, realVariableNames);
			foreach (Relation relation in relations){
				if (relation == null){
					errString = "Could not parse relations";
					return null;
				}
			}
			return relations;
		}
		private static Relation[] GetRelations(Parameters parameters, string[] realVariableNames){
			ParameterWithSubParams<int> sp = parameters.GetParamWithSubParams<int>("Number of relations");
			int nrel = sp.Value + 1;
			List<Relation> result = new List<Relation>();
			Parameters param = sp.GetSubParameters();
			for (int j = 0; j < nrel; j++){
				string rel = param.GetParam<string>("Relation " + (j + 1)).Value;
				if (rel.StartsWith(">") || rel.StartsWith("<") || rel.StartsWith("=")){
					rel = "x" + rel;
				}
				Relation r = Relation.CreateFromString(rel, realVariableNames, new string[0], out string _);
				result.Add(r);
			}
			return result.ToArray();
		}
		private static int[] GetColIndsNumFilter(Parameters parameters, out string[] realVariableNames){
			ParameterWithSubParams<int> sp = parameters.GetParamWithSubParams<int>("Number of columns");
			int ncols = sp.Value + 1;
			int[] result = new int[ncols];
			realVariableNames = new string[ncols];
			Parameters param = sp.GetSubParameters();
			for (int j = 0; j < ncols; j++){
				realVariableNames[j] = GetVariableName(j);
				result[j] = param.GetParam<int>(realVariableNames[j]).Value;
			}
			return result;
		}
		public static int GetRowCount(StreamReader reader, StreamReader auxReader, int[] mainColIndices,
			List<Tuple<Relation[], int[], bool>> filters, char separator){
			return PerseusUtil.GetRowCount(reader, filters, separator,
				auxReader != null && PerseusUtil.GetHasAddtlMatrices(auxReader, mainColIndices, separator), false, -1);
		}
		public static void AddFilter(List<Tuple<Relation[], int[], bool>> filters, Parameters p, int[] inds,
			out string errString){
			Relation[] relations = GetRelationsNumFilter(p, out errString, out int[] colInds, out bool and);
			if (errString != null){
				return;
			}
			colInds = inds.SubArray(colInds);
			if (relations != null){
				filters.Add(new Tuple<Relation[], int[], bool>(relations, colInds, and));
			}
		}
		/// <summary>
		/// Write matrix to stream with tab separation.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="writer"></param>
		/// <param name="addtlMatrices"></param>
		public static void WriteMatrix(IMatrixData data, StreamWriter writer, bool addtlMatrices = false){
			IEnumerable<string> columnNames = PerseusUtil.GetColumnNames(data);
			writer.WriteLine(StringUtils.Concat("\t", columnNames));
			IEnumerable<string> columnTypes = PerseusUtil.ColumnTypes(data);
			writer.WriteLine("#!{Type}" + StringUtils.Concat("\t", columnTypes));
			IEnumerable<string> numAnnotRows = NumericalAnnotationRows(data);
			foreach (string row in numAnnotRows){
				writer.WriteLine(row);
			}
			IEnumerable<string> catAnnotRows = CategoricalAnnotationRows(data);
			foreach (string row in catAnnotRows){
				writer.WriteLine(row);
			}
			WriteDataRows(data, addtlMatrices, writer);
		}
		public static void WriteMatrixNoAnnotation(IMatrixData data, string filename, bool addtlMatrices = false) {
            using (StreamWriter writer = new StreamWriter(filename, false, Encoding.UTF8)) {
				IEnumerable<string> columnNames = PerseusUtil.GetColumnNames(data);
				writer.WriteLine(StringUtils.Concat("\t", columnNames));
				for (int j = 0; j < data.RowCount; j++) {
					List<string> words = new List<string>();
					for (int i = 0; i < data.ColumnCount; i++) {
						string s1 = Parser.ToString(data.Values.Get(j, i));
						words.Add(s1);
					}
					IEnumerable<string> row =
						words.Concat(PerseusUtil.DataAnnotationRow((IDataWithAnnotationColumns)data, j));
					writer.WriteLine(StringUtils.Concat("\t", row));
				}
				WriteDataRows(data, addtlMatrices, writer);
			}
		}
        /// <summary>
        /// Write matrix to file with tab separation
        /// </summary>
        /// <param name="data"></param>
        /// <param name="filename"></param>
        /// <param name="addtlMatrices">if true numbers are converted to triples <code>value;imputed;quality</code></param>
        public static void WriteMatrixToFile(IMatrixData data, string filename, bool addtlMatrices = false){
			using (StreamWriter writer = new StreamWriter(filename, false, Encoding.UTF8)){
				WriteMatrix(data, writer, addtlMatrices);
			}
		}
		private static void WriteDataRows(IMatrixData data, bool addtlMatrices, TextWriter writer){
			for (int j = 0; j < data.RowCount; j++){
				List<string> words = new List<string>();
				for (int i = 0; i < data.ColumnCount; i++){
					string s1 = Parser.ToString(data.Values.Get(j, i));
					if (addtlMatrices){
						s1 += ";" + data.IsImputed[j, i] + ";" + Parser.ToString(data.Quality.Get(j, i));
					}
					words.Add(s1);
				}
				IEnumerable<string> row =
					words.Concat(PerseusUtil.DataAnnotationRow((IDataWithAnnotationColumns) data, j));
				writer.WriteLine(StringUtils.Concat("\t", row));
			}
		}
		private static IEnumerable<string> CategoricalAnnotationRows(IDataWithAnnotationRows data){
			List<string> rows = new List<string>();
			for (int i = 0; i < data.CategoryRowCount; i++){
				List<string> words = new List<string>();
				for (int j = 0; j < data.ColumnCount; j++){
					string[] s = data.GetCategoryRowAt(i)[j];
					words.Add(s.Length == 0 ? "" : StringUtils.Concat(";", s));
				}
				IEnumerable<string> row = words.Concat(AnnotationRowPadding((IDataWithAnnotationColumns) data));
				rows.Add("#!{C:" + data.CategoryRowNames[i] + "}" + StringUtils.Concat("\t", row));
			}
			return rows;
		}
		private static IEnumerable<string> NumericalAnnotationRows(IDataWithAnnotationRows data){
			List<string> rows = new List<string>();
			for (int i = 0; i < data.NumericRowCount; i++){
				List<string> words = new List<string>();
				for (int j = 0; j < data.ColumnCount; j++){
					words.Add(Parser.ToString(data.NumericRows[i][j]));
				}
				IEnumerable<string> row = words.Concat(AnnotationRowPadding((IDataWithAnnotationColumns) data));
				rows.Add("#!{N:" + data.NumericRowNames[i] + "}" + StringUtils.Concat("\t", row));
			}
			return rows;
		}
		private static IEnumerable<string> AnnotationRowPadding(IDataWithAnnotationColumns data){
			List<string> words = new List<string>();
			for (int j = 0; j < data.CategoryColumnCount; j++){
				words.Add("");
			}
			for (int j = 0; j < data.NumericColumnCount; j++){
				words.Add("");
			}
			for (int j = 0; j < data.StringColumnCount; j++){
				words.Add("");
			}
			for (int j = 0; j < data.MultiNumericColumnCount; j++){
				words.Add("");
			}
			return words;
		}
		public static void ReadDataWithAnnotationColumns(IDataWithAnnotationColumns data, ProcessInfo processInfo,
			Func<StreamReader> getReader, string name, char separator){
			PerseusUtil.ReadMatrixInformation(getReader, separator, out Dictionary<string, string[]> annotationRows,
				out string[] colNames, out int[] eInds, out int[] nInds, out int[] cInds, out int[] tInds,
				out int[] mInds, out List<Tuple<Relation[], int[], bool>> filters, out int nrows,
				out bool hasAdditionalMatrices);
			using (StreamReader reader = getReader()){
				LoadDataWithAnnotationColumns(data, colNames, cInds, nInds, tInds, mInds, processInfo.Progress,
					separator, reader, nrows, filters);
			}
		}
		public static void ReadMatrixFromFile(IMatrixData mdata, ProcessInfo processInfo, string filename, int[] eInds,
			int[] nInds, int[] cInds, int[] tInds, int[] mInds, Parameters[] mainFilterParameters,
			Parameters[] numericalFilterParameters, bool shortenExpressionColumnNames, bool limit, int nlimit){
			if (!File.Exists(filename)){
				processInfo.ErrString = "File '" + filename + "' does not exist.";
				return;
			}
			string ftl = filename.ToLower();
			bool csv = ftl.EndsWith(".csv") || ftl.EndsWith(".csv.gz");
			char separator = csv ? ',' : '\t';
			string[] colNames;
			Dictionary<string, string[]> annotationRows = new Dictionary<string, string[]>();
			try{
				colNames = TabSep.GetColumnNames(filename, StringUtils.commentPrefix,
					StringUtils.commentPrefixExceptions, annotationRows,
					separator);
			} catch (Exception){
				processInfo.ErrString = "Could not open the file '" + filename +
				                        "'. It is probably opened in another program.";
				return;
			}
			string origin = filename;
			List<Tuple<Relation[], int[], bool>> filters = new List<Tuple<Relation[], int[], bool>>();
			string errString;
			foreach (Parameters p in mainFilterParameters){
				AddFilter(filters, p, eInds, out errString);
				if (errString != null){
					processInfo.ErrString = errString;
					return;
				}
			}
			foreach (Parameters p in numericalFilterParameters){
				AddFilter(filters, p, nInds, out errString);
				if (errString != null){
					processInfo.ErrString = errString;
					return;
				}
			}
			int nrows;
			bool hasAdditionalMatrices;
			using (StreamReader reader = FileUtils.GetReader(filename)){
				hasAdditionalMatrices = PerseusUtil.GetHasAddtlMatrices(reader, eInds, separator);
			}
			using (StreamReader reader = FileUtils.GetReader(filename)){
				nrows = PerseusUtil.GetRowCount(reader, filters, separator, hasAdditionalMatrices, limit, nlimit);
			}
			using (StreamReader reader = FileUtils.GetReader(filename)){
				PerseusUtil.LoadMatrixData(annotationRows, eInds, cInds, nInds, tInds,
					mInds, processInfo, colNames, mdata, reader, nrows, origin, separator,
					shortenExpressionColumnNames, hasAdditionalMatrices, filters, limit, nlimit);
			}
			GC.Collect();
		}
		/// <summary>
		/// Create minimally initialized <see cref="IMatrixData"/>.
		/// </summary>
		public static IMatrixData CreateMatrixData(IMatrixData mdata, double[,] values, List<string> columnNames){
			mdata.Values.Set(values);
			mdata.ColumnNames = columnNames;
			BoolMatrixIndexer imputed = new BoolMatrixIndexer();
			imputed.Init(mdata.RowCount, mdata.ColumnCount);
			mdata.IsImputed = imputed;
			return mdata;
		}
		/// <summary>
		/// Create index map for mapping between two tables.
		/// leftIndexMap contains a mapping of row indices between the two tables
		/// <code>unmatchedRightIndices</code> contains a list of unmapped 
		/// </summary>
		public static (int[][] leftIndexMap, int[] unmatchedRightIndices) GetIndexMap(
			IDataWithAnnotationColumns leftData, IDataWithAnnotationColumns rightData, (int left, int right) first,
			(int left, int right)? second, bool ignoreCase){
			Dictionary<string, List<int>> idToRows;
			string[][] matchCol;
			if (second.HasValue){
				idToRows = MapIdToRow(rightData, first.right, second.Value.right, ignoreCase);
				matchCol = GetColumnPair(leftData, first.left, second.Value.left);
			} else{
				idToRows = MapIdToRow(rightData, first.right, ignoreCase);
				matchCol = GetColumnSplitBySemicolon(leftData, first.left);
			}
			HashSet<int> unmatchedIndices = new HashSet<int>(Enumerable.Range(0, rightData.RowCount));
			int[][] indexMap = new int[matchCol.Length][];
			for (int i = 0; i < matchCol.Length; i++){
				int[] indices = GetIndices(matchCol[i], ignoreCase, idToRows);
				indexMap[i] = indices;
				foreach (int index in indices){
					unmatchedIndices.Remove(index);
				}
			}
			return (indexMap, unmatchedIndices.OrderBy(i => i).ToArray());
		}
		public static string[][] GetColumnSplitBySemicolon(IDataWithAnnotationColumns mdata, int matchingColumn){
			string[] matchingColumn2;
			if (matchingColumn < mdata.StringColumnCount){
				matchingColumn2 = mdata.StringColumns[matchingColumn];
			} else{
				matchingColumn2 = mdata.NumericColumns[matchingColumn - mdata.StringColumnCount]
					.Select(Convert.ToString).ToArray();
			}
			string[][] w = new string[matchingColumn2.Length][];
			for (int i = 0; i < matchingColumn2.Length; i++){
				string r = matchingColumn2[i].Trim();
				w[i] = r.Length == 0 ? new string[0] : r.Split(';');
				w[i] = ArrayUtils.UniqueValues(w[i]);
			}
			return w;
		}
		/// <summary>
		/// Create a mapping from id to row index from the specified <see cref="idColumn"/>.
		/// </summary>
		public static Dictionary<string, List<int>> MapIdToRow(IDataWithAnnotationColumns data, int idColumn,
			bool ignoreCase){
			string[][] splitIds = GetColumnSplitBySemicolon(data, idColumn);
			Dictionary<string, List<int>> idToRow = new Dictionary<string, List<int>>();
			for (int i = 0; i < splitIds.Length; i++){
				foreach (string s in splitIds[i]){
					string id = s;
					if (ignoreCase){
						id = id.ToLower();
					}
					if (idToRow.TryGetValue(id, out List<int> rows)){
						rows.Add(i);
					} else{
						idToRow.Add(id, new List<int>{i});
					}
				}
			}
			return idToRow;
		}
		/// <summary>
		/// Create a mapping from id1 and id2 with a magic <see cref="separator"/> to row index from the specified id columns.
		/// </summary>
		private static Dictionary<string, List<int>> MapIdToRow(IDataWithAnnotationColumns data, int idColumn1,
			int idColumn2, bool ignoreCase){
			string[][] splitIds1 = GetColumnSplitBySemicolon(data, idColumn1);
			string[][] splitIds2 = GetColumnSplitBySemicolon(data, idColumn2);
			Dictionary<string, List<int>> idToRows = new Dictionary<string, List<int>>();
			for (int i = 0; i < splitIds1.Length; i++){
				foreach (string s1 in splitIds1[i]){
					foreach (string s2 in splitIds2[i]){
						string id = s1 + separator + s2;
						if (ignoreCase){
							id = id.ToLower();
						}
						if (idToRows.TryGetValue(id, out List<int> rows)){
							rows.Add(i);
						} else{
							idToRows.Add(id, new List<int>{i});
						}
					}
				}
			}
			return idToRows;
		}
		public static int[] GetIndices(string[] m, bool ignoreCase, Dictionary<string, List<int>> idToRows){
			List<int> matchingRows = new List<int>();
			foreach (string s in m){
				string id = s;
				if (ignoreCase){
					id = id.ToLower();
				}
				if (idToRows.TryGetValue(id, out List<int> rows)){
					matchingRows.AddRange(rows);
				}
			}
			int[] indices = ArrayUtils.UniqueValues(matchingRows.ToArray());
			return indices;
		}
		private static string[][] GetColumnPair(IDataWithAnnotationColumns mdata1, int matchingColumnInMatrix1,
			int additionalColumnInMatrix1){
			string[][] matchCol = GetColumnSplitBySemicolon(mdata1, matchingColumnInMatrix1);
			string[][] matchColAddtl = GetColumnSplitBySemicolon(mdata1, additionalColumnInMatrix1);
			string[][] result = new string[matchCol.Length][];
			for (int i = 0; i < result.Length; i++){
				result[i] = Combine(matchCol[i], matchColAddtl[i]);
			}
			return result;
		}
		private static string[] Combine(IEnumerable<string> s1, ICollection<string> s2){
			List<string> result = new List<string>();
			foreach (string t1 in s1){
				foreach (string t2 in s2){
					result.Add(t1 + separator + t2);
				}
			}
			result.Sort();
			return result.ToArray();
		}
		private const string separator = "!§$%";
		/// <summary>
		/// Perform matching between tables and return unmatched indices.
		/// </summary>
		public static IEnumerable<(int[][] indexMap, int[] unmappedRightIndices)> MatchDataWithAnnotationRows(
			((int[] copy, int combine) main, int[] text, (int[] copy, int combine) numeric, int[] category) annotation,
			ICollection<IDataWithAnnotationColumns> tables, IMatrixData mdata,
			((int m1, int m2) first, (int m1, int m2)? second, bool outer, bool ignoreCase) matching){
			int LocalColumnIndex(int global, IDataWithAnnotationColumns edgeTable){
				string[] commonStringColumns = NetworkUtils.Intersect(tables, table => table.StringColumnNames);
				string[] commonNumericColumns = NetworkUtils.Intersect(tables, table => table.NumericColumnNames);
				return global < commonStringColumns.Length
					? edgeTable.StringColumnNames.FindIndex(col => col.Equals(commonStringColumns[global]))
					: edgeTable.StringColumnCount + edgeTable.NumericColumnNames.FindIndex(col =>
						col.Equals(commonNumericColumns[global - commonStringColumns.Length]));
			}
			foreach (IDataWithAnnotationColumns table in tables){
				int first = LocalColumnIndex(matching.first.m1, table);
				matching.first.m1 = first;
				if (matching.second.HasValue){
					int second = LocalColumnIndex(matching.second.Value.m1, table);
					matching.second = (second, matching.second.Value.m2);
				}
				(int[][] indexMatch, int[] unmatchedRightIndices) = PerseusUtils.GetIndexMap(table, mdata,
					matching.first, matching.second, matching.ignoreCase);
				yield return (indexMatch, unmatchedRightIndices);
			}
		}
	}
}