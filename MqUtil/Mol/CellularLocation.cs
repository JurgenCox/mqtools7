using MqUtil.Table;

namespace MqUtil.Mol{
	public class CellularLocation : IDataItem{
		public string Name { get; set; }

		public DataTable2 CreateTable(){
			DataTable2 t = new DataTable2("Cellular location table", "");
			t.AddColumn("Name", 250, ColumnType.Text, "");
			return t;
		}

		public void FillTable(IDataTable t, string species){
			foreach (CellularLocation location in CellularLocations.AllLocations){
				DataRow2 row = t.NewRow();
				row["Name"] = location.Name;
				t.AddRow(row);
			}
		}

		public string Header => "Cellular locations";
		public bool IsSpeciesSpecific => false;

		public IDataItem GetItemAt(int i, string species){
			return CellularLocations.AllLocations[i];
		}
	}
}