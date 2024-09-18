using MqApi.Util;
using MqUtil.Table;
namespace MqUtil.Ms{
	public class DataTableWriterSerializer : IDataTable{
		private readonly StreamWriter writer;
		private bool first;
		private bool firstReally;
		private const string separator = "\t";
		private DataRow2 rowModel;
		private readonly BinaryWriter writer2;
		private readonly List<long> filePos = new List<long>();
		private readonly List<ColumnType> columnTypes = new List<ColumnType>();
		private readonly List<string> columnNames = new List<string>();
		private readonly List<string> columnDescriptions = new List<string>();
		private readonly bool verboseColumnHeaders;
		private readonly Dictionary<string, int> nameMapping = new Dictionary<string, int>();
		private readonly bool writeBinary;
		private readonly string filePathSer;

		public DataTableWriterSerializer(string filePathTxt, string filePathSer, bool appendTxt, bool appendSer,
			bool verboseColumnHeaders, bool noHeader, CharacterEncoding encoding) {
			writer = new StreamWriter(filePathTxt, appendTxt, StringUtils.GetEncoding(encoding));
			writeBinary = !string.IsNullOrEmpty(filePathSer);
			if (writeBinary) {
					writer2 = new BinaryWriter(new FileStream(filePathSer, appendSer ? FileMode.Append : FileMode.Create, 
						FileAccess.Write));
			}
			first = !appendTxt && !noHeader;
			firstReally = !appendTxt;
			this.verboseColumnHeaders = verboseColumnHeaders;
			this.filePathSer = filePathSer;
		}

		public DataTableWriterSerializer(string filePathTxt, string filePathSer, bool verboseColumnHeaders,
			CharacterEncoding encoding) : this(
			filePathTxt, filePathSer, false, false, verboseColumnHeaders, false, encoding){ }

		public string Description{ get; set; }

		public void AddColumn(string colName, int width, ColumnType columnType){
			AddColumn(colName, width, columnType, "");
		}

		public void AddColumn(string colName, int width, ColumnType columnType, string description){
			if (nameMapping.ContainsKey(colName)){
				throw new Exception("Column '" + colName + "' has already been added.");
			}
			nameMapping.Add(colName, columnNames.Count);
			columnTypes.Add(columnType);
			columnDescriptions.Add(description);
			columnNames.Add(colName);
		}

		public DataRow2 NewRow(){
			if (rowModel == null){
				rowModel = new DataRow2(columnNames.Count, nameMapping);
			}
			for (int i = 0; i < rowModel.ItemArray.Length; i++){
				rowModel[i] = DBNull.Value;
			}
			return rowModel;
		}

		public void AddRow(DataRow2 row){
			if (firstReally){
				WriteColumnTypes();
				firstReally = false;
			}
			if (first){
				WriteHeader();
				first = false;
			}
			WriteRow(row);
		}
		private void WriteColumnTypes() {
			BinaryWriter w = FileUtils.GetBinaryWriter(filePathSer + "x");
			w.Write(columnTypes.Count);
			foreach (ColumnType c in columnTypes) {
				w.Write((int)c);
			}
			w.Close();
		}
		private void WriteRow(DataRow2 row){
			string[] values = new string[columnNames.Count];
			for (int i = 0; i < values.Length; i++){
				if (row[i] != null){
					values[i] = Parser.ToString(row[i]);
				} else{
					values[i] = "";
				}
			}
			writer.WriteLine(StringUtils.Concat(separator, values));
			if (writeBinary){
				object[] valuesObj = new object[columnDescriptions.Count];
				for (int i = 0; i < valuesObj.Length; i++){
					valuesObj[i] = row[i];
				}
				writer2.Flush();
				filePos.Add(writer2.BaseStream.Position);
				WriteRow(writer2, valuesObj);	
			}
		}
		private void WriteRow(BinaryWriter writer2, object[] valuesObj){
			for (int i = 0; i < valuesObj.Length; i++){
				TableUtils.WriteElement(writer2, valuesObj[i], columnTypes[i]);
			}
		}
		private void WriteHeader(){
			string[] columnNames2 = new string[columnDescriptions.Count];
			for (int i = 0; i < columnDescriptions.Count; i++){
				columnNames2[i] = columnNames[i];
			}
			writer.WriteLine(StringUtils.Concat(separator, columnNames2));
			if (verboseColumnHeaders){
				string[] columnDesc = new string[columnDescriptions.Count];
				for (int i = 0; i < columnDescriptions.Count; i++){
					columnDesc[i] = columnDescriptions[i];
				}
				writer.WriteLine("#!{Description}" + StringUtils.Concat(separator, columnDesc));
				string[] columnType = new string[columnDescriptions.Count];
				for (int i = 0; i < columnDescriptions.Count; i++){
					columnType[i] = TableUtils.ColumnTypeToString(columnTypes[i]);
				}
				writer.WriteLine("#!{Type}" + StringUtils.Concat(separator, columnType));
			}
		}

		public void Clear(){ }

		public void Close(){
			writer.Close();
			if (writeBinary){
				writer2?.Close();
			}
		}

		public long[] GetFilePos(){
			return filePos.ToArray();
		}
		public static object[] ReadRow(BinaryReader reader, ColumnType[] colTypes){
			object[] result = new object[colTypes.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = TableUtils.ReadElement(reader, colTypes[i]);
			}
			return result;
		}
	}
}