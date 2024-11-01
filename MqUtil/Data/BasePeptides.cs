namespace MqUtil.Data{
	/// <summary>
	/// De-duplicating string dictionary with fast set and slow get operations.
	/// </summary>
	/// Multiple elements might be added but only unique strings are stored in memory.
	/// See MsLib.Test.Data.Peptide.BasePeptidesTest.
	public class BasePeptides{
		/// <summary>
		/// Keeps track of unique peptides.
		/// </summary>
		private readonly Dictionary<string, int> basePeptides;

		/// <summary>
		/// Keeps track of the indices at which unique peptides occur.
		/// </summary>
		private readonly Dictionary<int, int> map;

		public BasePeptides(){
			basePeptides = new Dictionary<string, int>();
			map = new Dictionary<int, int>();
		}

		public BasePeptides(BinaryReader reader) : this(){
			bool empty = reader.ReadBoolean();
			if (empty){
				return;
			}
			int n = reader.ReadInt32();
			for (int i = 0; i < n; i++){
				map.Add(reader.ReadInt32(), reader.ReadInt32());
			}
			n = reader.ReadInt32();
			for (int i = 0; i < n; i++){
				basePeptides.Add(reader.ReadString(), reader.ReadInt32());
			}
		}

		public string this[int index]{
			get{
				if (!map.TryGetValue(index, out int ind)){
					return null;
				}
				return basePeptides.FirstOrDefault(kv => kv.Value == ind).Key;
			}
			set{
				if (!basePeptides.TryGetValue(value, out int ind)){
					ind = basePeptides.Count;
					basePeptides.Add(value, ind);
				}
				map.Add(index, ind);
			}
		}

		public void Write(BinaryWriter writer){
			bool empty = map == null;
			writer.Write(empty);
			if (empty){
				return;
			}
			writer.Write(map.Count);
			foreach (KeyValuePair<int, int> pair in map){
				writer.Write(pair.Key);
				writer.Write(pair.Value);
			}
			writer.Write(basePeptides.Count);
			foreach (KeyValuePair<string, int> pair in basePeptides){
				writer.Write(pair.Key);
				writer.Write(pair.Value);
			}
		}
	}
}