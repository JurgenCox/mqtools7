using System.Runtime.Serialization;
using MqApi.Util;
namespace MqUtil.Table{
	public sealed class VirtualDataTable2 : TableModelImpl, ITable{
		public Func<int, object[]> GetRowData { private get; set; }
		private readonly int rowCount;
		private List<int> persistentColInds;
		private DataTable2 persistentTable;
		//transient
		private long rowInUse = -1;
		private object[] rowDataInUse;
		public VirtualDataTable2(string name, string description, int rowCount) : base(name, description){
			this.rowCount = rowCount;
		}

		private VirtualDataTable2(SerializationInfo info, StreamingContext context) : base(info, context){
			GetRowData = (Func<int, object[]>) info.GetValue("GetRowData", typeof (Func<int, object[]>));
			rowCount = info.GetInt32("rowCount");
			persistentColInds = (List<int>) info.GetValue("persistentColInds", typeof (List<int>));
			persistentTable = (DataTable2) info.GetValue("persistentTable", typeof (DataTable2));
		}
		public VirtualDataTable2(BinaryReader reader, Func<int, object[]> getRowData) : base(reader) {
			GetRowData = getRowData;
			rowCount = reader.ReadInt32();
			int[] x = FileUtils.ReadInt32Array(reader);
			if (x != null){
				persistentColInds = new List<int>(x);
			}
			bool isNull = reader.ReadBoolean();
			if (!isNull){
				persistentTable = new DataTable2(reader);
			}
		}
		public void Write(BinaryWriter writer){
			base.Write1(writer);
			writer.Write(rowCount);
			FileUtils.Write(persistentColInds, writer);
			bool isNull = persistentTable == null;
			writer.Write(isNull);
			if (!isNull){
				persistentTable.Write(writer);
			}
		}
		public void AddColumn(string colName, int width, ColumnType columnType, string description, bool persistent){
			AddColumn(colName, width, columnType, description);
			if (persistent){
				AddPersistentColumn(colName, width, columnType, description);
			}
		}

		private void AddPersistentColumn(string colName, int width, ColumnType columnType, string description){
			if (persistentTable == null){
				persistentTable = new DataTable2(Name, Description);
				persistentColInds = new List<int>();
			}
			persistentTable.AddColumn(colName, width, columnType, description);
			persistentColInds.Add(columnNames.Count - 1);
			persistentColInds.Sort();
		}

		public DataRow2 NewRow(){
			return new DataRow2(columnNames.Count, nameMapping);
		}

		public void FillPersistentData(){
			for (int i = 0; i < rowCount; i++){
				object[] rowData = GetRowData(i);
				DataRow2 row = persistentTable.NewRow();
				for (int j = 0; j < persistentColInds.Count; j++){
					row[j] = rowData[persistentColInds[j]];
				}
				persistentTable.AddRow(row);
			}
		}

		public override long RowCount => rowCount;

		public override object GetEntry(long row, int col){
			if (row >= RowCount || row < 0){
				return null;
			}
			if (rowInUse != row){
				rowDataInUse = GetRowDataImpl((int)row);
				rowInUse = row;
			}
			if (rowDataInUse == null){
				return null;
			}
			return col >= rowDataInUse.Length ? null : rowDataInUse[col];
		}

		public override void SetEntry(long row, int column, object value){
			if (persistentTable == null){
				throw new Exception("The table has no persistent columns.");
			}
			int ind = persistentColInds.BinarySearch(column);
			if (ind < 0){
				throw new Exception("The column is not persistent.");
			}
			persistentTable.SetEntry(row, ind, value);
		}

		private object[] GetRowDataImpl(int row){
			if (GetRowData == null){
				return null;
			}
			if (row < 0 || row >= rowCount){
				return null;
			}
			object[] result = GetRowData(row);
			if (persistentColInds != null){
				for (int i = 0; i < persistentColInds.Count; i++){
					result[persistentColInds[i]] = persistentTable.GetEntry(row, i);
				}
			}
			return result;
		}
	}
}