using System.Reflection;
using MqApi.Util;
namespace MqUtil.Ms.Raw {
	/// <summary>
	/// A collection of static fields and methods. The only public members are property RawFileTemplates and method GetExtensions().
	/// </summary>
	public static class RawFiles {
		/// <summary>
		/// Backing field for RawFileTemplates.
		/// </summary>
		private static RawFile[] rawFileTemplates;
		/// <summary>
		/// An array of RawFile objects, one for each "PLUGINRAW*.DLL" file.
		/// </summary>
		public static RawFile[] RawFileTemplates => rawFileTemplates ?? (rawFileTemplates = InitializeRawPlugins());
		/// <summary>
		/// Find all files in the executable path of the form "PLUGINRAW*.DLL" which implement the interface RawFile.
		/// Used for lazy initialization of the property RawFileTemplates.
		/// </summary>
		/// <returns>An array of uninitialized RawFile objects (templates). If one of them has Suffix ".raw", it is listed first.</returns>
		private static RawFile[] InitializeRawPlugins() {
			string[] pluginFiles = Directory.GetFiles(FileUtils.executablePath, "PluginRaw*.dll");
			List<RawFile> result = new List<RawFile>();
			foreach (string pluginFile in pluginFiles) {
				// Extract the file name, without preceding directory elements or terminating extention. 
				string a = pluginFile.Substring(
					pluginFile.LastIndexOf("" + Path.DirectorySeparatorChar, StringComparison.InvariantCulture) + 1,
					pluginFile.ToLower().IndexOf(".dll", StringComparison.InvariantCulture) -
					pluginFile.LastIndexOf("" + Path.DirectorySeparatorChar, StringComparison.InvariantCulture) - 1);
				Assembly ass = Assembly.Load(a);
				Type[] types = ass.GetTypes();
				foreach (Type t in types) {
					try {
						object o = Activator.CreateInstance(t);
						if (o is RawFile) {
							RawFile r = (RawFile) o;
							result.Add(r);
						}
					} catch (Exception) { }
				}
			}
			RawFile[] rf = result.ToArray();
			Reorder(rf);
			return rf;
		}
		/// <summary>
		/// Find the first RawFile type with Suffix ".raw" and move it to the front.
		/// </summary>
		private static void Reorder(RawFile[] keys) {
			if (keys.Length < 2) {
				return;
			}
			int ind = -1;
			for (int i = 0; i < keys.Length; i++) {
				if (keys[i].Suffix.Equals(".raw")) {
					ind = i;
					break;
				}
			}
			if (ind > 0) {
				(keys[ind], keys[0]) = (keys[0], keys[ind]);
			}
		}
		/// <summary>
		/// Find the suffixes of the RawFile types in RawFileTemplates.
		/// Used once in MQMetabolites.MaxQuantMLib.Data.RunM.Init
		/// and once in MQPeptides.MaxQuantPLib.Data.Run.Init.
		/// </summary>
		/// <returns>A string array with the suffixes.</returns>
		public static string[] GetExtensions() {
			string[] result = new string[RawFileTemplates.Length];
			for (int i = 0; i < result.Length; i++) {
				result[i] = RawFileTemplates[i].Suffix;
			}
			return result;
		}
	}
}