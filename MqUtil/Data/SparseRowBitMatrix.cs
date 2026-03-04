namespace MqUtil.Data
{
	public class SparseRowBitMatrix : SparseBitMatrix
	{
		private readonly SortedSet<int>[] data;
		public SparseRowBitMatrix(int nrows, int ncols) : base(nrows, ncols)
		{
			data = new SortedSet<int>[nrows];
		}
		public override bool Get(int row, int col)
		{
			return data[row] != null && data[row].Contains(col);
		}
		public override void Set(int row, int col, bool val)
		{
			if (data[row] == null)
			{
				data[row] = new SortedSet<int>();
			}
			if (val)
			{
				data[row].Add(col);
			}
			else
			{
				data[row].Remove(col);
			}
		}
		public override void SetRowFalse(int row)
		{
			data[row] = null;
		}
		public override void SetColumnFalse(int col)
		{
			for (int i = 0; i < RowCount; i++)
			{
				if (data[i] != null)
				{
					data[i].Remove(col);
				}
			}
		}
	}
}