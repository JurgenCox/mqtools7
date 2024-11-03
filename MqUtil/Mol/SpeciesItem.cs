using MqApi.Util;
using MqUtil.Table;

namespace MqUtil.Mol{
	public class SpeciesItem : IDataItem{
		public string Name { get; set; }
		public int TaxonomyId { get; set; }
		public string CommonName { get; set; }
		public string PspName { get; set; }
		public string ReactomeName { get; set; }
		public EnsemblDivision EnsemblDivision { get; set; }
		public string EnsemblSubspecies { get; set; }
		public int[] SubnodeIds { get; set; }
		public string Superkingdom { get; set; }
		public string Kingdom { get; set; }
		public string Subkingdom { get; set; }
		public string Superphylum { get; set; }
		public string Phylum { get; set; }
		public string Subphylum { get; set; }
		public string Superclass { get; set; }
		public string Class { get; set; }
		public string Subclass { get; set; }
		public string Infraclass { get; set; }
		public string Superorder { get; set; }
		public string Order { get; set; }
		public string Suborder { get; set; }
		public string Infraorder { get; set; }
		public string Parvorder { get; set; }
		public string Superfamily { get; set; }
		public string Family { get; set; }
		public string Subfamily { get; set; }
		public string Tribe { get; set; }
		public string Subtribe { get; set; }
		public string Genus { get; set; }
		public string Subgenus { get; set; }
		public string SpeciesGroup { get; set; }
		public string SpeciesSubgroup { get; set; }

		public DataTable2 CreateTable(){
			DataTable2 t = new DataTable2("Species table", "");
			t.AddColumn("Name", 170, ColumnType.Text);
			t.AddColumn("Taxonomy ID", 69, ColumnType.Integer);
			t.AddColumn("Common name", 110, ColumnType.Text);
			t.AddColumn("PSP name", 60, ColumnType.Text);
			t.AddColumn("ReactomeName", 60, ColumnType.Text);
			t.AddColumn("EnsemblDivision", 60, ColumnType.Text);
			t.AddColumn("EnsemblSubspecies", 60, ColumnType.Text);
			t.AddColumn("SubnodeIds", 60, ColumnType.Text);
			t.AddColumn("Superkingdom", 60, ColumnType.Text);
			t.AddColumn("Kingdom", 60, ColumnType.Text);
			t.AddColumn("Subkingdom", 60, ColumnType.Text);
			t.AddColumn("Superphylum", 60, ColumnType.Text);
			t.AddColumn("Phylum", 60, ColumnType.Text);
			t.AddColumn("Subphylum", 60, ColumnType.Text);
			t.AddColumn("Superclass", 60, ColumnType.Text);
			t.AddColumn("Class", 60, ColumnType.Text);
			t.AddColumn("Subclass", 60, ColumnType.Text);
			t.AddColumn("Infraclass", 60, ColumnType.Text);
			t.AddColumn("Superorder", 60, ColumnType.Text);
			t.AddColumn("Order", 60, ColumnType.Text);
			t.AddColumn("Suborder", 60, ColumnType.Text);
			t.AddColumn("Infraorder", 60, ColumnType.Text);
			t.AddColumn("Parvorder", 60, ColumnType.Text);
			t.AddColumn("Superfamily", 60, ColumnType.Text);
			t.AddColumn("Family", 60, ColumnType.Text);
			t.AddColumn("Subfamily", 60, ColumnType.Text);
			t.AddColumn("Tribe", 60, ColumnType.Text);
			t.AddColumn("Subtribe", 60, ColumnType.Text);
			t.AddColumn("Genus", 60, ColumnType.Text);
			t.AddColumn("Subgenus", 60, ColumnType.Text);
			t.AddColumn("SpeciesGroup", 60, ColumnType.Text);
			t.AddColumn("SpeciesSubgroup", 60, ColumnType.Text);
			return t;
		}

		public void FillTable(IDataTable t, string species){
			SpeciesItem[] refs = SpeciesItems.Species.ToArray();
			foreach (SpeciesItem r in refs){
				DataRow2 row = t.NewRow();
				row["Name"] = r.Name;
				row["Taxonomy ID"] = r.TaxonomyId;
				row["Common name"] = r.CommonName;
				row["PSP name"] = r.PspName;
				row["ReactomeName"] = r.ReactomeName;
				row["EnsemblDivision"] = r.EnsemblDivision == EnsemblDivision.NotAvalilable ? "" : "" + r.EnsemblDivision;
				row["EnsemblSubspecies"] = r.EnsemblSubspecies;
				row["SubnodeIds"] = StringUtils.Concat(";", r.SubnodeIds);
				row["Superkingdom"] = r.Superkingdom;
				row["Kingdom"] = r.Kingdom;
				row["Subkingdom"] = r.Subkingdom;
				row["Superphylum"] = r.Superphylum;
				row["Phylum"] = r.Phylum;
				row["Subphylum"] = r.Subphylum;
				row["Superclass"] = r.Superclass;
				row["Class"] = r.Class;
				row["Subclass"] = r.Subclass;
				row["Infraclass"] = r.Infraclass;
				row["Superorder"] = r.Superorder;
				row["Order"] = r.Order;
				row["Suborder"] = r.Suborder;
				row["Infraorder"] = r.Infraorder;
				row["Parvorder"] = r.Parvorder;
				row["Superfamily"] = r.Superfamily;
				row["Family"] = r.Family;
				row["Subfamily"] = r.Subfamily;
				row["Tribe"] = r.Tribe;
				row["Subtribe"] = r.Subtribe;
				row["Genus"] = r.Genus;
				row["Subgenus"] = r.Subgenus;
				row["SpeciesGroup"] = r.SpeciesGroup;
				row["SpeciesSubgroup"] = r.SpeciesSubgroup;
				t.AddRow(row);
			}
		}

		public string Header => "Species";

		public bool IsSpeciesSpecific => false;

		public IDataItem GetItemAt(int i, string species){
			return SpeciesItems.Species[i];
		}
	}
}