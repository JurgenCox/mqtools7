using MqApi.Util;
using MqUtil.Mol;

namespace MqUtil.Parse.Chebi{
	public class ChebiEntry{
		public int CompoundId{ get; set; }
		public double MonoisotopicMass{ get; set; }
		public string Formula{ get; set; }
		public string[] ResNames{ get; set; }
		public double[] SeriesMonoisotopicMass{ get; set; }
		public string[] SeriesFormula{ get; set; }
		public string[][] SeriesResNames{ get; set; }
		public int Charge{ get; set; }
		public string Name{ get; set; }
		public string Source{ get; set; }
		public double Mass{ get; set; }
		public string Inchi{ get; set; }
		public Dictionary<SmallMoleculeDbAccessionType, List<string>> accessions =
			new Dictionary<SmallMoleculeDbAccessionType, List<string>>();
		public Dictionary<SmallMoleculeDbType, List<SmallMoleculeDbReference>> references =
			new Dictionary<SmallMoleculeDbType, List<SmallMoleculeDbReference>>();
		public ChebiEntry() {
		}
		public ChebiEntry(BinaryReader reader) {
			CompoundId = reader.ReadInt32();
			MonoisotopicMass = reader.ReadDouble();
			Formula = reader.ReadString();
			ResNames = FileUtils.ReadStringArray(reader);
			SeriesMonoisotopicMass = FileUtils.ReadDoubleArray(reader);
			SeriesFormula = FileUtils.ReadStringArray(reader);
			SeriesResNames = FileUtils.Read2DStringArray(reader);
			Charge = reader.ReadInt32();
			Name = reader.ReadString();
			Source = reader.ReadString();
			Mass = reader.ReadDouble();
			Inchi = reader.ReadString();
			accessions = new Dictionary<SmallMoleculeDbAccessionType, List<string>>();
			int n = reader.ReadInt32();
			for (int i = 0; i < n; i++) {
				SmallMoleculeDbAccessionType key = (SmallMoleculeDbAccessionType)reader.ReadInt32();
				List<string> val = new List<string>(FileUtils.ReadStringArray(reader));
				accessions.Add(key, val);
			}
			references = new Dictionary<SmallMoleculeDbType, List<SmallMoleculeDbReference>>();
			n = reader.ReadInt32();
			for (int i = 0; i < n; i++) {
				SmallMoleculeDbType key = (SmallMoleculeDbType)reader.ReadInt32();
				List<SmallMoleculeDbReference> val = new List<SmallMoleculeDbReference>();
				int m = reader.ReadInt32();
				for (int k = 0; k < m; k++){
					val.Add(new SmallMoleculeDbReference(reader));
				}
				references.Add(key, val);
			}
		}
		public void Write(BinaryWriter writer){
			writer.Write(CompoundId);
			writer.Write(MonoisotopicMass);
			writer.Write(Formula);
			FileUtils.Write(ResNames, writer);
			FileUtils.Write(SeriesMonoisotopicMass, writer);
			FileUtils.Write(SeriesFormula, writer);
			FileUtils.Write(SeriesResNames, writer);
			writer.Write(Charge);
			writer.Write(Name);
			writer.Write(Source);
			writer.Write(Mass);
			writer.Write(Inchi);
			writer.Write(accessions.Count);
			foreach (KeyValuePair<SmallMoleculeDbAccessionType, List<string>> pair in accessions){
				writer.Write((int)pair.Key);
				FileUtils.Write(pair.Value, writer);
			}
			writer.Write(references.Count);
			foreach (KeyValuePair<SmallMoleculeDbType, List<SmallMoleculeDbReference>> pair in references){
				writer.Write((int)pair.Key);
				List<SmallMoleculeDbReference> val = pair.Value;
				writer.Write(val.Count);
				foreach (SmallMoleculeDbReference w in val){
					w.Write(writer);
				}
			}
		}
	}
}