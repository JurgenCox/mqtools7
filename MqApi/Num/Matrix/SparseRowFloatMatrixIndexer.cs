using MqApi.Num.Vector;
using MqApi.Util;

namespace MqApi.Num.Matrix{
	public class SparseRowFloatMatrixIndexer : MatrixIndexer{
		private SparseFloatVectorDict[] vals;
		private int ncolumns;
		public SparseRowFloatMatrixIndexer(SparseFloatVectorDict[] vals, int ncolumns){
			this.vals = vals;
			this.ncolumns = ncolumns;
		}
		public SparseRowFloatMatrixIndexer()
		{
		}
		public SparseRowFloatMatrixIndexer(BinaryReader reader)
		{
			ncolumns = reader.ReadInt32();
			int n = reader.ReadInt32();
			vals = new SparseFloatVectorDict[n];
			for (int i = 0; i < n; i++)
			{
			  vals[i] = new SparseFloatVectorDict();
			  vals[i].Read(reader);
			}
		}
		public override void Write(BinaryWriter writer){
			writer.Write(ncolumns);
			writer.Write(vals.Length);
			foreach (SparseFloatVectorDict val in vals){
				val.Write(writer);
			}
		}
		public override void Init(int nrows, int ncolumns1){
			ncolumns = ncolumns1;
			vals = new SparseFloatVectorDict[nrows];
			for (int i = 0; i < nrows; i++){
				vals[i] = new SparseFloatVectorDict([], ncolumns);
			}
		}
		public override void Set(double[,] value){
			throw new Exception("Never get here.");
		}
	public override BaseVector GetRow(int row){
			return vals[row];
		}
		public override BaseVector GetColumn(int col){
			List<int> inds = [];
			List<float> x = [];
			for (int i = 0; i < vals.Length; i++){
				float w = (float) vals[i][col];
				if (w == 0){
					continue;
				}
				inds.Add(i);
				x.Add(w);
			}
			return new SparseFloatVector(inds.ToArray(), x.ToArray(), vals.Length);
		}
		public override bool IsInitialized(){
			return vals != null;
		}
		public override MatrixIndexer ExtractRows(IList<int> rows){
			return new SparseRowFloatMatrixIndexer{vals = ArrayUtils.SubArray(vals, rows), ncolumns = ncolumns};
		}
		public override void ExtractRowsInPlace(IList<int> rows){
			vals = ArrayUtils.SubArray(vals, rows);
		}
		public override MatrixIndexer ExtractColumns(IList<int> columns){
			throw new Exception("Never get here.");
		}
	public override void ExtractColumnsInPlace(IList<int> columns){
			for (int i = 0; i < vals.Length; i++){
				vals[i] = (SparseFloatVectorDict) vals[i].SubArray(columns);
			}
			ncolumns = columns.Count;
		}
		public override MatrixIndexer Transpose(){
			throw new Exception("Never get here.");
		}
	public override bool ContainsNaNOrInf(){
			foreach (SparseFloatVectorDict val in vals){
				if (val.ContainsNaNOrInf()){
					return true;
				}
			}
			return false;
		}
		public override bool IsNanOrInfRow(int row){
			return vals[row].IsNaNOrInf();
		}
		public override bool IsNanOrInfColumn(int column){
			for (int i = 0; i < RowCount; i++){
				float v = (float) vals[i][column];
				if (!float.IsNaN(v) && !float.IsInfinity(v)){
					return false;
				}
			}
			return true;
		}
		public override int RowCount => vals.Length;
		public override int ColumnCount => ncolumns;
		public override double this[int i, int j]{
			get => vals[i][j];
			set => vals[i][j] = value;
		}
		public override double Get(int i, int j){
			return vals[i][j];
		}
		public override void Set(int i, int j, double value){
			vals[i][j] = value;
		}
		public override void Dispose(){
			foreach (SparseFloatVectorDict val in vals){
				val.Dispose();
			}
			vals = null;
		}
		public override object Clone(){
			throw new Exception("Never get here.");
		}
	public void Add(int[] data, long[] indices, int nThreads)
		{

			ThreadDistributor td = new ThreadDistributor(nThreads, nThreads, a =>
			{
				for (int i = a; i < data.Length; i += nThreads)
				{
					Add(data[i], (int)indices[i], i);
				}
			});
			td.Start();
		}

		private void Add(int data, int column, int row)
		{
		  vals[row][column] = data;
		}
	}
}