using MqApi.Util;
namespace MqUtil.Table{
	public sealed class VirtualDataTable3 : TableModelImpl, ITable{
		public Func<int, int, object> GetCellData { private get; set; }
		private readonly int rowCount;
		private List<int> persistentColInds;
		private DataTable2 persistentTable;
		public VirtualDataTable3(string name, string description, int rowCount) : base(name, description){
			this.rowCount = rowCount;
		}
		public VirtualDataTable3(BinaryReader reader, Func<int, int, object> getCellData) : base(reader) {
			GetCellData = getCellData;
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
				DataRow2 row = persistentTable.NewRow();
				for (int j = 0; j < persistentColInds.Count; j++){
					row[j] = GetCellData(i,persistentColInds[j]);
				}
				persistentTable.AddRow(row);
			}
		}

		public override long RowCount => rowCount;

		public override object GetEntry(long row, int col){
			if (row >= RowCount || row < 0){
				return null;
			}
			return GetCellDataImpl((int)row, col);
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

		private object GetCellDataImpl(int row, int col){
			if (persistentColInds != null)
			{
				int a = persistentColInds.BinarySearch(col);
				if (a >= 0)
				{
					return persistentTable.GetEntry(row, a);
				}
			}

			if (GetCellData == null){
				return null;
			}
			if (row < 0 || row >= rowCount){
				return null;
			}
			return GetCellData(row, col);
		}
	}
}