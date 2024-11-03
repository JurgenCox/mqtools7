using MqApi.Util;

namespace MqUtil.Mol{
	public static class CellularLocations{
		private static List<CellularLocation> locations;
		public static List<CellularLocation> AllLocations => locations ?? (locations = Init());

		private static List<CellularLocation> Init(){
			string folder = FileUtils.GetConfigPath() + Path.DirectorySeparatorChar+"data"+ Path.DirectorySeparatorChar;
			string file = folder + "cellularLocations.txt";
			StreamReader reader = FileUtils.GetReader(file);
			reader.ReadLine();
			string line;
			List<CellularLocation> result = new List<CellularLocation>();
			while ((line = reader.ReadLine()) != null){
				string[] w = line.Split('\t');
				if (w.Length < 1){
					continue;
				}
				CellularLocation pr = new CellularLocation{Name = w[0]};
				result.Add(pr);
			}
			return result;
		}
	}
}