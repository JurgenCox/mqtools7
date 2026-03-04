namespace MqUtil.Data
{
	public class SparseColumnBitMatrix : SparseBitMatrix
	{
		private readonly SortedSet<int>[] data;
		public SparseColumnBitMatrix(int nrows, int ncols) : base(nrows, ncols)
		{
			data = new SortedSet<int>[ncols];
		}
		public override bool Get(int row, int col)
		{
			return data[col] != null && data[col].Contains(row);
		}
		public override void Set(int row, int col, bool val)
		{
			if (data[col] == null)
			{
				data[col] = new SortedSet<int>();
			}
			if (val)
			{
				data[col].Add(row);
			}
			else
			{
				data[col].Remove(row);
			}
		}
		public override void SetColumnFalse(int col)
		{
			data[col] = null;
		}
		public override void SetRowFalse(int row)
		{
			for (int i = 0; i < ColumnCount; i++)
			{
				if (data[i] != null)
				{
					data[i].Remove(row);
				}
			}
		}
	}
}
