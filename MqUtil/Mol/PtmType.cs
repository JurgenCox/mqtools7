using MqUtil.Table;

namespace MqUtil.Mol{
	public class PtmType : IDataItem{
		public string PspName { get; set; }
		public string Residues { get; set; }

		public DataTable2 CreateTable(){
			DataTable2 t = new DataTable2("PTM type table", "");
			t.AddColumn("PTM type", 120, ColumnType.Text, "");
			t.AddColumn("Amino acids", 120, ColumnType.Text, "");
			return t;
		}

		public void FillTable(IDataTable t, string species){
			foreach (PtmType r in PtmTypes.allTypes){
				DataRow2 row = t.NewRow();
				row["PTM type"] = r.PspName.ToLower();
				row["Amino acids"] = r.Residues;
				t.AddRow(row);
			}
		}

		public string Header => "PTM types";
		public bool IsSpeciesSpecific => false;

		public IDataItem GetItemAt(int i, string species){
			return PtmTypes.allTypes[i];
		}
	}
}