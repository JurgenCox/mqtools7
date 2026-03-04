namespace MqUtil.Data{
	public abstract class SparseBitMatrix{

		protected SparseBitMatrix(int nrows, int ncols){
			RowCount = nrows;
			ColumnCount = ncols;
		}
		public int RowCount{ get; }
		public int ColumnCount{ get; }
		public abstract void Set(int row, int col, bool val);
		public abstract bool Get(int row, int col);
		public abstract void SetRowFalse(int row);
		public abstract void SetColumnFalse(int col);
	}
}