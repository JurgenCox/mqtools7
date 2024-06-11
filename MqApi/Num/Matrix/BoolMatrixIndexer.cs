﻿using MqApi.Util;
namespace MqApi.Num.Matrix{
	[Serializable]
	public class BoolMatrixIndexer : IBoolMatrixIndexer{
		private bool[,] vals;
		private bool isConstant;
		private bool constVal;
		private int nrows;
		private int ncols;
		public BoolMatrixIndexer(bool[,] vals){
			this.vals = vals;
		}
		public BoolMatrixIndexer(bool val, int nrows, int ncols){
			isConstant = true;
			constVal = val;
			this.nrows = nrows;
			this.ncols = ncols;
		}
		public BoolMatrixIndexer(){
		}
		public BoolMatrixIndexer(BinaryReader reader){
			vals = FileUtils.Read2DBooleanArray2(reader);
			isConstant = reader.ReadBoolean();
			constVal = reader.ReadBoolean();
			nrows = reader.ReadInt32();
			ncols = reader.ReadInt32();
		}
		public void Write(BinaryWriter writer){
			FileUtils.Write(vals, writer);
			writer.Write(isConstant);
			writer.Write(constVal);
			writer.Write(nrows);
			writer.Write(ncols);
		}
		public void Init(int nrows1, int ncols1){
			isConstant = false;
			vals = new bool[nrows1, ncols1];
		}
		public IBoolMatrixIndexer Transpose(){
			if (isConstant){
				return new BoolMatrixIndexer(constVal, ncols, nrows);
			}
			return vals == null ? new BoolMatrixIndexer() : new BoolMatrixIndexer(ArrayUtils.Transpose(vals));
		}
		public void TransposeInPlace(){
			if (isConstant){
				(nrows, ncols) = (ncols, nrows);
				return;
			}
			vals = ArrayUtils.Transpose(vals);
		}
		public void Set(bool[,] value){
			isConstant = false;
			vals = value;
		}
		public bool[,] Get(){
			return vals;
		}
		public bool[] GetRow(int row){
			bool[] result = new bool[ColumnCount];
			for (int i = 0; i < result.Length; i++){
				result[i] = isConstant ? constVal : vals[row, i];
			}
			return result;
		}
		public bool[] GetColumn(int col){
			bool[] result = new bool[RowCount];
			for (int i = 0; i < result.Length; i++){
				result[i] = isConstant ? constVal : vals[i, col];
			}
			return result;
		}
		public bool IsInitialized(){
			return vals != null || isConstant;
		}
		public void ExtractRowsInPlace(int[] rows){
			if (isConstant){
				nrows = rows.Length;
				return;
			}
			if (vals != null){
				vals = ArrayUtils.ExtractRows(vals, rows);
			}
		}
		public void ExtractColumnsInPlace(int[] columns){
			if (isConstant){
				ncols = columns.Length;
				return;
			}
			if (vals != null){
				vals = ArrayUtils.ExtractColumns(vals, columns);
			}
		}
		public IBoolMatrixIndexer ExtractRows(int[] rows){
			if (isConstant){
				return new BoolMatrixIndexer(constVal, rows.Length, ncols);
			}
			return new BoolMatrixIndexer(ArrayUtils.ExtractRows(vals, rows));
		}
		public IBoolMatrixIndexer ExtractColumns(int[] columns){
			if (isConstant){
				return new BoolMatrixIndexer(constVal, nrows, columns.Length);
			}
			return new BoolMatrixIndexer(ArrayUtils.ExtractColumns(vals, columns));
		}
		public int RowCount => isConstant ? nrows : vals?.GetLength(0) ?? 0;
		public int ColumnCount => isConstant ? ncols : vals?.GetLength(1) ?? 0;
		public bool this[int i, int j]{
			get => (isConstant || vals == null) ? constVal : vals[i, j];
			set{
				if (isConstant && value != constVal){
					vals = new bool[nrows, ncols];
					if (constVal){
						for (int k = 0; k < nrows; k++){
							for (int l = 0; l < ncols; l++){
								vals[k, l] = constVal;
							}
						}
					}
					vals[i, j] = value;
					isConstant = false;
				}
				vals[i, j] = value;
			}
		}
		public void Dispose(){
			vals = null;
		}
		public object Clone(){
			if (isConstant){
				return new BoolMatrixIndexer(constVal, nrows, ncols);
			}
			return vals == null ? new BoolMatrixIndexer((bool[,]) null) : new BoolMatrixIndexer((bool[,]) vals.Clone());
		}
		public bool Equals(IBoolMatrixIndexer other){
			if (other == null){
				return false;
			}
			if (!IsInitialized() && !other.IsInitialized()){
				return true;
			}
			if (!other.IsInitialized()){
				return false;
			}
			if (isConstant){
				if (nrows != other.RowCount || ncols != other.ColumnCount){
					return false;
				}
				if (nrows == 0 || ncols == 0){
					return true;
				}
				return this[0, 0] == other[0, 0];
			}
			for (int i = 0; i < RowCount; i++){
				for (int j = 0; j < ColumnCount; j++){
					if (this[i, j] != other[i, j]){
						return false;
					}
				}
			}
			return true;
		}
	}
}