﻿using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using MqApi.Util;
namespace MqUtil.Table{
	/// <summary>
	/// Partial implementation of <code>ITableModel</code>, implementing all functionality that is shared 
	/// between the full implementations of <code>ITableModel</code>.
	/// </summary>
	public abstract class TableModelImpl : ITableModel{
		public string Name { get; set; }
		public string Description { get; set; }
		protected readonly List<string> columnNames = new List<string>();
		private readonly List<int> columnWidths = new List<int>();
		protected readonly List<ColumnType> columnTypes = new List<ColumnType>();
		private readonly List<string> columnDescriptions = new List<string>();
		private readonly List<string> annotationRowNames = new List<string>();
		private readonly List<string> annotationRowDescriptions = new List<string>();
		private readonly Collection<DataAnnotationRow> annotationRows = new Collection<DataAnnotationRow>();
		protected readonly Dictionary<string, int> nameMapping = new Dictionary<string, int>();

		protected TableModelImpl(string name, string description){
			Name = name;
			Description = description;
		}

		protected TableModelImpl(SerializationInfo info, StreamingContext ctxt){
			Name = info.GetString("Name");
			Description = info.GetString("Description");
			columnNames = (List<string>) info.GetValue("columnNames", typeof (List<string>));
			columnWidths = (List<int>) info.GetValue("columnWidths", typeof (List<int>));
			columnTypes = (List<ColumnType>) info.GetValue("columnTypes", typeof (List<ColumnType>));
			columnDescriptions = (List<string>) info.GetValue("columnDescriptions", typeof (List<string>));
			annotationRowNames = (List<string>) info.GetValue("annotationRowNames", typeof (List<string>));
			annotationRowDescriptions = (List<string>) info.GetValue("annotationRowDescriptions", typeof (List<string>));
			annotationRows =
				(Collection<DataAnnotationRow>) info.GetValue("annotationRows", typeof (Collection<DataAnnotationRow>));
			nameMapping = (Dictionary<string, int>) info.GetValue("nameMapping", typeof (Dictionary<string, int>));
		}
		protected TableModelImpl(BinaryReader reader) {
			Name = reader.ReadString();
			Description = reader.ReadString();
			columnNames = new List<string>(FileUtils.ReadStringArray(reader));
			columnWidths = new List<int>(FileUtils.ReadInt32Array(reader));
			columnTypes = new List<ColumnType>();
			int n = reader.ReadInt32();
			for (int i = 0; i < n; i++) {
				columnTypes.Add((ColumnType)reader.ReadInt32());
			}
			columnDescriptions = new List<string>(FileUtils.ReadStringArray(reader));
			annotationRowNames = new List<string>(FileUtils.ReadStringArray(reader));
			annotationRowDescriptions = new List<string>(FileUtils.ReadStringArray(reader));
			annotationRows = new Collection<DataAnnotationRow>();
			n = reader.ReadInt32();
			for (int i = 0; i < n; i++) {
				annotationRows.Add(new DataAnnotationRow(reader, columnTypes));
			}
			nameMapping = new Dictionary<string, int>();
			n = reader.ReadInt32();
			for (int i = 0; i < n; i++) {
				string key = reader.ReadString();
				int value = reader.ReadInt32();
				nameMapping.Add(key, value);
			}
		}
		protected void Write1(BinaryWriter writer) {
			writer.Write(Name);
			writer.Write(Description);
			FileUtils.Write(columnNames, writer);
			FileUtils.Write(columnWidths, writer);
			writer.Write(columnTypes.Count);
			foreach (ColumnType type in columnTypes){
				writer.Write((int)type);				
			}
			FileUtils.Write(columnDescriptions, writer);
			FileUtils.Write(annotationRowNames, writer);
			FileUtils.Write(annotationRowDescriptions, writer);
			writer.Write(annotationRows.Count);
			foreach (DataAnnotationRow row in annotationRows) {
				row.Write(writer, columnTypes);
			}
			writer.Write(nameMapping.Count);
			foreach (KeyValuePair<string, int> pair in nameMapping) {
				writer.Write(pair.Key);
				writer.Write(pair.Value);
			}
		}

		public int ColumnCount => columnNames.Count;

		public int GetColumnWidth(int col){
			return columnWidths[col];
		}

		public string GetColumnName(int col){
			return columnNames[col];
		}

        public bool IsColumnEditable(int column){
			return false;
		}

		public string GetColumnDescription(int col){
			return columnDescriptions[col];
		}

		public ColumnType GetColumnType(int col){
			return col < 0 ? ColumnType.Text : columnTypes[col];
		}

		public int GetColumnIndex(string colName){
			return nameMapping.ContainsKey(colName) ? nameMapping[colName] : -1;
		}

		public object GetEntry(long row, string colname){
			int colInd = GetColumnIndex(colname);
			return colInd < 0 ? null : GetEntry(row, colInd);
		}

		public void SetEntry(int row, string colname, object value){
			int colInd = GetColumnIndex(colname);
			if (colInd < 0){
				return;
			}
			SetEntry(row, colInd, value);
		}

		public abstract long RowCount { get; }
		public abstract object GetEntry(long row, int column);
		public abstract void SetEntry(long row, int column, object value);

		public DataAnnotationRow NewAnnotationRow(){
			return new DataAnnotationRow(columnNames.Count, nameMapping);
		}

		public void AddAnnotationRow(DataAnnotationRow row, string name, string description){
			annotationRows.Add(row);
			annotationRowNames.Add(name);
			annotationRowDescriptions.Add(description);
		}

		public int AnnotationRowsCount => annotationRowNames.Count;

		public string GetAnnotationRowName(int index){
			return annotationRowNames[index];
		}

		public string GetAnnotationRowDescription(int index){
			return annotationRowDescriptions[index];
		}

		public object GetAnnotationRowValue(int index, int column){
			return annotationRows[index][column];
		}

		public object GetAnnotationRowValue(int index, string colname){
			return GetAnnotationRowValue(index, GetColumnIndex(colname));
		}

		public void AddColumn(string colName, int width, ColumnType columnType) {
			AddColumn(colName, width, columnType, null);
		}

		public void AddColumn(string colName, int width, ColumnType columnType, string description){
			if (nameMapping.ContainsKey(colName)){
				return;
			}
			nameMapping.Add(colName, columnNames.Count);
			columnNames.Add(colName);
			columnWidths.Add(width);
			columnTypes.Add(columnType);
			columnDescriptions.Add(description);
		}
	}
}