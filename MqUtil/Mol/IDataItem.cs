using MqUtil.Table;

namespace MqUtil.Mol{
	public interface IDataItem{
		DataTable2 CreateTable();
		void FillTable(IDataTable table, string species);
		string Header { get; }
		bool IsSpeciesSpecific { get; }
		IDataItem GetItemAt(int i, string species);
	}
}