﻿using System.IO.Compression;
using System.Reflection;
using System.Security;
using System.Security.Cryptography;
using System.Text;
namespace MqApi.Util{
	public static class FileUtils{
		public static string dateFormat = "dd/MM/yyyy HH:mm:ss";

		//public static string fastaFilter = "Fasta file (*.fasta)|*.fasta;*.fas;*.faa;*.fa;*.fasta.gz;*.fas.gz;*.faa.gz;*.fa.gz";
		public static string fastaFilter = "Fasta file (*.fasta)|*.fasta;*.fas;*.faa;*.fa";
		public static string imageFilter =
			"All files|*.bmp;*.gif;*.jpg;*jif;*jpe;*jpeg;*.png|BMP Windows or OS/2 Bitmap (*.bmp)|*.bmp|" +
			"GIF Graphics Interchange Format (*.gif)|*.gif|" +
			"JPG JPEG (*.jpg,*jif,*jpe,*jpeg)|*.jpg;*jif;*jpe;*jpeg|PNG Portable Network Graphics (*.png)|*.png";
		private static readonly Random random = new Random();
		public static string executableFile =
			Assembly.GetEntryAssembly()?.Location ?? typeof(FileUtils).Assembly.Location;
		public static string executablePath = Path.GetDirectoryName(executableFile);
		public static string GetConfigPath(){
			return Path.Combine(executablePath, "conf");
		}
		public static string[] GetAllFilesWithSuffix(string folder, string[] suffixes, bool recursive){
			if (string.IsNullOrEmpty(folder)){
				return new string[0];
			}
			if (!Directory.Exists(folder)){
				return new string[0];
			}
			if (recursive){
				HashSet<string> result = new HashSet<string>();
				AddFilesWithSuffix(folder, suffixes, result);
				return result.ToArray();
			} else{
				HashSet<string> result = new HashSet<string>();
				foreach (string path in Directory.GetFiles(folder)){
					if (ValidPath(path, suffixes)){
						result.Add(path);
					}
				}
				return result.ToArray();
			}
		}
		private static void AddFilesWithSuffix(string folder, string[] suffixes, HashSet<string> result){
			foreach (string path in Directory.GetFiles(folder)){
				if (ValidPath(path, suffixes)){
					result.Add(path);
				}
			}
			foreach (string dir in Directory.GetDirectories(folder)){
				AddFilesWithSuffix(dir, suffixes, result);
			}
		}
		private static bool ValidPath(string path, IEnumerable<string> suffixes){
			foreach (string suffix in suffixes){
				if (path.EndsWith(suffix.ToUpper())){
					return true;
				}
				if (path.EndsWith(suffix.ToLower())){
					return true;
				}
			}
			return false;
		}
		public static string[] GetAllSuffixesInFolder(string folder, bool recursive){
			if (string.IsNullOrEmpty(folder)){
				return new string[0];
			}
			if (!Directory.Exists(folder)){
				return new string[0];
			}
			if (recursive){
				HashSet<string> result = new HashSet<string>();
				AddSuffixes(folder, result);
				return result.ToArray();
			} else{
				HashSet<string> result = new HashSet<string>();
				foreach (string path in Directory.GetFiles(folder)){
					if (!path.Contains('.')){
						continue;
					}
					int ind = path.LastIndexOf('.');
					string suffix = path.Substring(ind);
					result.Add(suffix);
				}
				return result.ToArray();
			}
		}
		private static void AddSuffixes(string folder, HashSet<string> result){
			foreach (string file in Directory.GetFiles(folder)){
				if (!file.Contains('.')){
					continue;
				}
				int ind = file.LastIndexOf('.');
				string suffix = file.Substring(ind);
				result.Add(suffix);
			}
			foreach (string dir in Directory.GetDirectories(folder)){
				AddSuffixes(dir, result);
			}
		}
		/// <summary>
		/// Creates a <code>BinaryReader</code> reading from the given file path.
		/// </summary>
		/// <param name="path">File to read from.</param>
		/// <returns>The <code>BinaryReader</code>.</returns>
		public static BinaryReader GetBinaryReader(string path){
			return new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
		}
		public static StreamReader GetReader(string filename, bool seekable = false){
			if (filename.ToLower().EndsWith(".gz")){
				Stream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
				Stream stream = new GZipStream(fileStream, CompressionMode.Decompress);
				if (seekable){
					MemoryStream memory = new MemoryStream();
					stream.CopyTo(memory);
					stream = memory;
				}
				return new StreamReader(stream);
			}
			return new StreamReader(filename);
		}
		/// <summary>
		/// Creates a <code>StreamReader</code> reading from the given text resource within this assembly.
		/// </summary>
		/// <param name="name">Name of the resource to read from.</param>
		/// <returns>The <code>StreamReader</code>.</returns>
		public static StreamReader GetResourceTextReader(string name){
			return new StreamReader(GetResourceStream(name));
		}
		/// <summary>
		/// Creates a <code>Stream</code> reading from the given text resource within this assembly.
		/// </summary>
		/// <param name="name">Name of the resource to read from.</param>
		/// <returns>The <code>StreamReader</code>.</returns>
		public static Stream GetResourceStream(string name){
			Assembly assembly = Assembly.GetExecutingAssembly();
			return assembly.GetManifestResourceStream(name);
		}
		/// <summary>
		/// Reads a string in a binary version which is purely ascii-encoded.
		/// </summary>
		public static string ReadString(BinaryReader reader){
			int len = reader.ReadInt32();
			byte[] x = new byte[len];
			for (int i = 0; i < len; i++){
				x[i] = reader.ReadByte();
			}
			char[] chars = new char[Encoding.ASCII.GetCharCount(x, 0, x.Length)];
			Encoding.ASCII.GetChars(x, 0, x.Length, chars, 0);
			return new string(chars);
		}
		/// <summary>
		/// Writes a string in a binary version which is purely ascii-encoded.
		/// </summary>
		public static void WriteString(string str, BinaryWriter writer){
			byte[] x = Encoding.ASCII.GetBytes(str);
			writer.Write(x.Length);
			foreach (byte t in x){
				writer.Write(t);
			}
		}
		/// <summary>
		/// Deletes file after checking for its existence.
		/// </summary>
		public static void DeleteFile(string filename){
			if (File.Exists(filename)){
				File.Delete(filename);
			}
		}
		public static Dictionary<string, int> ReadDictionaryStringInt(BinaryReader reader){
			int len = reader.ReadInt32();
			Dictionary<string, int> result = new Dictionary<string, int>();
			for (int i = 0; i < len; i++){
				string key = reader.ReadString();
				int value = reader.ReadInt32();
				result.Add(key, value);
			}
			return result;
		}
		public static Dictionary<string, string> ReadDictionaryStringString(BinaryReader reader){
			int len = reader.ReadInt32();
			Dictionary<string, string> result = new Dictionary<string, string>();
			for (int i = 0; i < len; i++){
				string key = reader.ReadString();
				string value = reader.ReadString();
				result.Add(key, value);
			}
			return result;
		}
		public static Dictionary<int, int> ReadDictionaryIntInt(BinaryReader reader){
			int len = reader.ReadInt32();
			Dictionary<int, int> result = new Dictionary<int, int>();
			for (int i = 0; i < len; i++){
				int key = reader.ReadInt32();
				int value = reader.ReadInt32();
				result.Add(key, value);
			}
			return result;
		}
		public static Dictionary<int, double> ReadDictionaryIntDouble(BinaryReader reader){
			int len = reader.ReadInt32();
			Dictionary<int, double> result = new Dictionary<int, double>();
			for (int i = 0; i < len; i++){
				int key = reader.ReadInt32();
				double value = reader.ReadDouble();
				result.Add(key, value);
			}
			return result;
		}
		public static Dictionary<int, float> ReadDictionaryIntFloat(BinaryReader reader){
			int len = reader.ReadInt32();
			Dictionary<int, float> result = new Dictionary<int, float>();
			for (int i = 0; i < len; i++){
				int key = reader.ReadInt32();
				float value = reader.ReadSingle();
				result.Add(key, value);
			}
			return result;
		}
		public static HashSet<int> ReadHashSetInt(BinaryReader reader){
			int len = reader.ReadInt32();
			HashSet<int> result = new HashSet<int>();
			for (int i = 0; i < len; i++){
				int key = reader.ReadInt32();
				result.Add(key);
			}
			return result;
		}
		public static HashSet<string> ReadHashSetString(BinaryReader reader){
			int len = reader.ReadInt32();
			HashSet<string> result = new HashSet<string>();
			for (int i = 0; i < len; i++){
				string key = reader.ReadString();
				result.Add(key);
			}
			return result;
		}
		/// <summary>
		/// Removes all files and folders recursively in the specified folder 
		/// and the specified folder itself.
		/// </summary>
		/// <param name="path">Path of the folder to be removed.</param>
		public static void Rmdir(string path){
			if (!Directory.Exists(path)){
				return;
			}
			string[] d = Directory.GetDirectories(path);
			foreach (string s in d){
				Rmdir(s);
			}
			while ((Directory.GetDirectories(path)).Length > 0){
				Thread.Sleep(1000);
			}
			string[] files1 = Directory.GetFiles(path);
			foreach (string f in files1){
				try{
					File.Delete(f);
				} catch (IOException){
				}
			}
			string[] files;
			while ((files = Directory.GetFiles(path)).Length > 0){
				string file = files.First();
				try{
					File.Delete(file);
				} catch (IOException){
					Thread.Sleep(100);
				}
			}
			Directory.Delete(path);
		}
		/// <summary>
		/// Creates a <code>BinaryWriter</code> writing to the given file path.
		/// </summary>
		/// <param name="path">File to write to.</param>
		/// <returns>The <code>BinaryWriter</code>.</returns>
		public static BinaryWriter GetBinaryWriter(string path){
			return new BinaryWriter(new FileStream(path, FileMode.Create, FileAccess.Write));
		}
		/// <summary>
		/// Tests whether the directory corresponding to the given path is writable.
		/// </summary>
		/// <param name="path">Path of the directory.</param>
		/// <returns><code>true</code> if the directory corresponding to the given path is writable.</returns>
		public static bool TestDirWritable(string path){
			if (Directory.Exists(path)){
				try{
					string filePath = Path.Combine(path, "AccesTest.tmp");
					StreamWriter accessTest = new StreamWriter(filePath);
					accessTest.Close();
					File.Delete(filePath);
				} catch{
					return false;
				}
				return true;
			}
			return false;
		}
		/// <summary>
		/// Remove all the files in the given path with the given suffix.
		/// </summary>
		/// <param name="path">The path to remove the files from.</param>
		/// <param name="suffix">The suffix of the files to remove.</param>
		public static void RemoveFiles(string path, string suffix){
			foreach (string file in Directory.GetFiles(path).Where(file => file.EndsWith(suffix))){
				File.Delete(file);
			}
		}
		public static string GetTempFolder(){
			try{
				string tempPath = Path.GetTempPath();
				if (Directory.Exists(tempPath)){
					return tempPath;
				}
			} catch (SecurityException){
			}
			return null;
		}
		/// <summary>
		/// Retrieves the filename of the given file path. When the extension is not required
		/// the parameter withExt should be set to false.
		/// </summary>
		/// <param name="filePath">The file path to retrieve the filename for.</param>
		/// <param name="withExt">Set to false when the extension is not required.</param>
		/// <returns></returns>
		public static string GetFileName(string filePath, bool withExt){
			string filename;
			string path = GetPath(filePath);
			if (!string.IsNullOrEmpty(path)){
				filename = filePath.Replace(GetPath(filePath), "");
				filename = filename.Substring(1);
			} else{
				filename = filePath;
			}
			if (filename.Contains(".") && !withExt){
				filename = filename.Substring(0, filename.LastIndexOf(".", StringComparison.InvariantCulture));
			}
			return filename;
		}
		/// <summary>
		/// Retrieves the path for the given absolute file path.
		/// </summary>
		/// <param name="filePath">The filename to retrieve the path for.</param>
		/// <returns>The path to the filename.</returns>
		public static string GetPath(string filePath){
			if (filePath.Contains("/")){
				return filePath.Substring(0, filePath.LastIndexOf("/", StringComparison.InvariantCulture));
			}
			return filePath.Contains(Path.DirectorySeparatorChar)
				? filePath.Substring(0,
					filePath.LastIndexOf("" + Path.DirectorySeparatorChar, StringComparison.InvariantCulture))
				: "";
		}
		public static string GetRandomFilename(){
			string tmpFolder = GetTempFolder();
			while (true){
				string filename = tmpFolder + Path.DirectorySeparatorChar + random.Next(0, int.MaxValue);
				if (!File.Exists(filename)){
					return filename;
				}
			}
		}
		public static string[] GetLines(int from, int to, string filename){
			StreamReader reader = new StreamReader(filename);
			string[] result = new string[to - from];
			string line;
			int lineNumber = 0;
			while (((line = reader.ReadLine()) != null) && lineNumber < to){
				if (lineNumber >= from){
					result[lineNumber - from] = line;
				}
				lineNumber++;
			}
			if (lineNumber <= from){
				return new string[0];
			}
			if (lineNumber < to){
				Array.Resize(ref result, lineNumber - from);
			}
			return result;
		}
		public static string GetNextAvailableFileName(string baseName, string extension){
			string fullName = baseName + extension;
			if (!File.Exists(fullName)){
				return fullName;
			}
			int count = 1;
			for (;;){
				string extendedName = baseName + "_" + count + extension;
				if (!File.Exists(extendedName)){
					return extendedName;
				}
				count++;
			}
		}
		/// <summary>
		/// Calculates the MD5 hash from the data stored in the given filename. The resulting byte
		/// array is automatically converted to string with the base64 algorithm, so it can be
		/// used for various string operations.
		/// </summary>
		/// <param name="filename">The file to retrieve the MD5 hash from.</param>
		/// <returns>The base64 encoded MD5 hash from the file.</returns>
		public static string GetMd5HashFromFile(string filename){
			FileStream s = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] v = md5.ComputeHash(s);
			s.Close();
			return Convert.ToBase64String(v);
		}
		public static void CopyFolder(string sourceFolder, string destFolder, bool replace){
			if (!Directory.Exists(destFolder)){
				DirectoryInfo info = Directory.CreateDirectory(destFolder);
				if (!info.Exists){
					throw new Exception("Could not create folder " + destFolder);
				}
			} else{
				DirectoryInfo sourceInfo = new DirectoryInfo(sourceFolder);
				DirectoryInfo destInfo = new DirectoryInfo(destFolder);
				if (!sourceInfo.Name.Equals(destInfo.Name)){
					destFolder = destFolder + Path.DirectorySeparatorChar + sourceInfo.Name;
				}
				if (!Directory.Exists(destFolder)){
					Directory.CreateDirectory(destFolder);
				}
			}
			string[] files = Directory.GetFiles(sourceFolder);
			foreach (string file in files){
				string dest = Path.Combine(destFolder, Path.GetFileName(file));
				File.Copy(file, dest, replace);
			}
			string[] folders = Directory.GetDirectories(sourceFolder);
			foreach (string folder in folders){
				string dest = Path.Combine(destFolder, Path.GetFileName(folder));
				CopyFolder(folder, dest, replace);
			}
		}
		public static void Move(string filePath, string destFolder){
			string name = Path.GetFileName(filePath);
			string destFilePath = Path.Combine(destFolder, name);
			if (File.Exists(destFilePath)){
				File.Delete(destFilePath);
			}
			FileInfo info = new FileInfo(filePath);
			info.MoveTo(destFilePath);
		}
		public static void Copy(string filePath, string destFolder){
			Copy(filePath, destFolder, false);
		}
		public static void Copy(string filePath, string destFolder, bool overwrite){
			string name = Path.GetFileName(filePath);
			string destFilePath = Path.Combine(destFolder, name);
			FileInfo info = new FileInfo(filePath);
			info.CopyTo(destFilePath, overwrite);
		}
		public static void MoveFolderOld(string srcFolder, string destFolder){
			if (!Directory.Exists(destFolder)){
				Directory.CreateDirectory(destFolder);
			}
			while (Directory.GetFiles(srcFolder).Length > 0){
				foreach (string file in Directory.GetFiles(srcFolder)){
					for (int i = 0; i < maxTries; i++){
						try{
							Move(file, destFolder);
							break;
						} catch (Exception){
							if (i == maxTries - 1){
								throw;
							}
							Thread.Sleep(50);
						}
					}
				}
			}
			while (Directory.GetDirectories(srcFolder).Length > 0){
				foreach (string dir in Directory.GetDirectories(srcFolder)){
					for (int i = 0; i < maxTries; i++){
						try{
							MoveFolderOld(dir, Path.Combine(destFolder, Path.GetFileName(dir)));
							break;
						} catch (Exception){
							if (i == maxTries - 1){
								throw;
							}
							Thread.Sleep(50);
						}
					}
				}
			}
			Directory.Delete(srcFolder);
		}
		private const int maxTries = 18000;
		public static void MoveFolder(string srcFolder, string destFolder){
			if (!Directory.Exists(destFolder)){
				Directory.CreateDirectory(destFolder);
			} else{
				DirectoryInfo sourceInfo = new DirectoryInfo(srcFolder);
				DirectoryInfo destInfo = new DirectoryInfo(destFolder);
				if (!sourceInfo.Name.Equals(destInfo.Name)){
					destFolder = destFolder + Path.DirectorySeparatorChar + sourceInfo.Name;
				}
				if (!Directory.Exists(destFolder)){
					Directory.CreateDirectory(destFolder);
				}
			}
			while (Directory.GetFiles(srcFolder).Length > 0){
				foreach (string file in Directory.GetFiles(srcFolder)){
					for (int i = 0; i < maxTries; i++){
						try{
							Move(file, destFolder);
							break;
						} catch (Exception){
							if (i == maxTries - 1){
								throw;
							}
							Thread.Sleep(50);
						}
					}
				}
			}
			while (Directory.GetDirectories(srcFolder).Length > 0){
				foreach (string dir in Directory.GetDirectories(srcFolder)){
					for (int i = 0; i < maxTries; i++){
						try{
							MoveFolderOld(dir, Path.Combine(destFolder, Path.GetFileName(dir)));
							break;
						} catch (Exception){
							if (i == maxTries - 1){
								throw;
							}
							Thread.Sleep(50);
						}
					}
				}
			}
			Directory.Delete(srcFolder);
		}
		public static bool IsNet45OrNewer(){
			// Class "ReflectionContext" exists from .NET 4.5 onwards.
			bool x = Type.GetType("System.Reflection.ReflectionContext", false) == null;
			return !x;
		}
		public static string BasicChecks(){
			return !IsNet45OrNewer() ? ".NET 4.5 framework is not installed." : null;
		}
		public static void Write(bool? x, BinaryWriter writer){
			bool nonNull = x != null;
			writer.Write(nonNull);
			if (nonNull){
				writer.Write(x.Value);
			}
		}
		public static bool? ReadNullableBoolean(BinaryReader reader){
			bool? result = null;
			bool nonNull = reader.ReadBoolean();
			if (nonNull){
				result = reader.ReadBoolean();
			}
			return result;
		}
		public static void Write(int? x, BinaryWriter writer){
			bool nonNull = x != null;
			writer.Write(nonNull);
			if (nonNull){
				writer.Write(x.Value);
			}
		}
		public static int? ReadNullableInt32(BinaryReader reader){
			int? result = null;
			bool nonNull = reader.ReadBoolean();
			if (nonNull){
				result = reader.ReadInt32();
			}
			return result;
		}
		public static void Write(double? x, BinaryWriter writer){
			bool nonNull = x != null;
			writer.Write(nonNull);
			if (nonNull){
				writer.Write(x.Value);
			}
		}
		public static double? ReadNullableDouble(BinaryReader reader){
			double? result = null;
			bool nonNull = reader.ReadBoolean();
			if (nonNull){
				result = reader.ReadDouble();
			}
			return result;
		}
		public static void Write(IList<double> x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			writer.Write(x.Count);
			foreach (double t in x){
				writer.Write(t);
			}
		}
		public static double[] ReadDoubleArray(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n = reader.ReadInt32();
			double[] result = new double[n];
			for (int i = 0; i < n; i++){
				result[i] = reader.ReadDouble();
			}
			return result;
		}
		public static List<double> ReadDoubleList(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n = reader.ReadInt32();
			List<double> result = new List<double>();
			for (int i = 0; i < n; i++){
				result.Add(reader.ReadDouble());
			}
			return result;
		}
		public static void Write(IList<float> x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			writer.Write(x.Count);
			foreach (float t in x){
				writer.Write(t);
			}
		}
		public static float[] ReadFloatArray(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n = reader.ReadInt32();
			float[] result = new float[n];
			for (int i = 0; i < n; i++){
				result[i] = reader.ReadSingle();
			}
			return result;
		}
		public static void Write(IList<int> x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			writer.Write(x.Count);
			foreach (int t in x){
				writer.Write(t);
			}
		}
		public static int[] ReadInt32Array(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n = reader.ReadInt32();
			int[] result = new int[n];
			for (int i = 0; i < n; i++){
				result[i] = reader.ReadInt32();
			}
			return result;
		}
		public static List<int> ReadInt32List(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n = reader.ReadInt32();
			List<int> result = new List<int>();
			for (int i = 0; i < n; i++){
				result.Add(reader.ReadInt32());
			}
			return result;
		}
		public static void Write(IList<uint> x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			writer.Write(x.Count);
			foreach (uint t in x){
				writer.Write(t);
			}
		}
		public static uint[] ReadUintArray(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n = reader.ReadInt32();
			uint[] result = new uint[n];
			for (int i = 0; i < n; i++){
				result[i] = reader.ReadUInt32();
			}
			return result;
		}
		public static void Write(IList<ushort> x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			writer.Write(x.Count);
			foreach (ushort t in x){
				writer.Write(t);
			}
		}
		public static ushort[] ReadUshortArray(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n = reader.ReadInt32();
			ushort[] result = new ushort[n];
			for (int i = 0; i < n; i++){
				result[i] = reader.ReadUInt16();
			}
			return result;
		}
		public static void Write(IList<short> x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			writer.Write(x.Count);
			foreach (short t in x){
				writer.Write(t);
			}
		}
		public static short[] ReadInt16Array(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n = reader.ReadInt32();
			short[] result = new short[n];
			for (int i = 0; i < n; i++){
				result[i] = reader.ReadInt16();
			}
			return result;
		}
		public static void Write(IList<string> x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			writer.Write(x.Count);
			foreach (string t in x){
				writer.Write(t);
			}
		}
		public static string[] ReadStringArray(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n = reader.ReadInt32();
			string[] result = new string[n];
			for (int i = 0; i < n; i++){
				result[i] = reader.ReadString();
			}
			return result;
		}
		public static void Write(IList<bool> x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			writer.Write(x.Count);
			foreach (bool t in x){
				writer.Write(t);
			}
		}
		public static bool[] ReadBooleanArray(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n = reader.ReadInt32();
			bool[] result = new bool[n];
			for (int i = 0; i < n; i++){
				result[i] = reader.ReadBoolean();
			}
			return result;
		}
		public static void Write(IList<byte> x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			writer.Write(x.Count);
			foreach (byte t in x){
				writer.Write(t);
			}
		}
		public static byte[] ReadByteArray(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n = reader.ReadInt32();
			byte[] result = new byte[n];
			for (int i = 0; i < n; i++){
				result[i] = reader.ReadByte();
			}
			return result;
		}
		public static void Write(IList<long> x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			writer.Write(x.Count);
			foreach (long t in x){
				writer.Write(t);
			}
		}
		public static long[] ReadInt64Array(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n = reader.ReadInt32();
			long[] result = new long[n];
			for (int i = 0; i < n; i++){
				result[i] = reader.ReadInt64();
			}
			return result;
		}
		public static void Write(IList<double[]> x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			writer.Write(x.Count);
			foreach (double[] t in x){
				Write(t, writer);
			}
		}
		public static void Write(IList<bool[]> x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			writer.Write(x.Count);
			foreach (bool[] t in x){
				Write(t, writer);
			}
		}
		public static double[][] Read2DDoubleArray(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n = reader.ReadInt32();
			double[][] result = new double[n][];
			for (int i = 0; i < n; i++){
				result[i] = ReadDoubleArray(reader);
			}
			return result;
		}
		public static void Write(double[,] x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			int n1 = x.GetLength(0);
			int n2 = x.GetLength(1);
			writer.Write(n1);
			writer.Write(n2);
			for (int i = 0; i < n1; i++){
				for (int j = 0; j < n2; j++){
					writer.Write(x[i, j]);
				}
			}
		}
		public static double[,] Read2DDoubleArray2(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n1 = reader.ReadInt32();
			int n2 = reader.ReadInt32();
			double[,] result = new double[n1, n2];
			for (int i = 0; i < n1; i++){
				for (int j = 0; j < n2; j++){
					result[i, j] = reader.ReadDouble();
				}
			}
			return result;
		}
		public static void Write(IList<string[]> x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			writer.Write(x.Count);
			foreach (string[] t in x){
				Write(t, writer);
			}
		}
		public static bool[][] Read2DBooleanArray(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n = reader.ReadInt32();
			bool[][] result = new bool[n][];
			for (int i = 0; i < n; i++){
				result[i] = ReadBooleanArray(reader);
			}
			return result;
		}
		public static void Write(bool[,] x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			int n1 = x.GetLength(0);
			int n2 = x.GetLength(1);
			writer.Write(n1);
			writer.Write(n2);
			for (int i = 0; i < n1; i++){
				for (int j = 0; j < n2; j++){
					writer.Write(x[i, j]);
				}
			}
		}
		public static bool[,] Read2DBooleanArray2(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n1 = reader.ReadInt32();
			int n2 = reader.ReadInt32();
			bool[,] result = new bool[n1, n2];
			for (int i = 0; i < n1; i++){
				for (int j = 0; j < n2; j++){
					result[i, j] = reader.ReadBoolean();
				}
			}
			return result;
		}
		public static string[][] Read2DStringArray(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n = reader.ReadInt32();
			string[][] result = new string[n][];
			for (int i = 0; i < n; i++){
				result[i] = ReadStringArray(reader);
			}
			return result;
		}
		public static void Write(string[,] x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			int n1 = x.GetLength(0);
			int n2 = x.GetLength(1);
			writer.Write(n1);
			writer.Write(n2);
			for (int i = 0; i < n1; i++){
				for (int j = 0; j < n2; j++){
					writer.Write(x[i, j]);
				}
			}
		}
		public static string[,] Read2DStringArray2(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n1 = reader.ReadInt32();
			int n2 = reader.ReadInt32();
			string[,] result = new string[n1, n2];
			for (int i = 0; i < n1; i++){
				for (int j = 0; j < n2; j++){
					result[i, j] = reader.ReadString();
				}
			}
			return result;
		}
		public static void Write(IList<int[]> x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			writer.Write(x.Count);
			foreach (int[] t in x){
				Write(t, writer);
			}
		}
		public static int[][] Read2DInt32Array(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n = reader.ReadInt32();
			int[][] result = new int[n][];
			for (int i = 0; i < n; i++){
				result[i] = ReadInt32Array(reader);
			}
			return result;
		}
		public static void Write(int[,] x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			int n1 = x.GetLength(0);
			int n2 = x.GetLength(1);
			writer.Write(n1);
			writer.Write(n2);
			for (int i = 0; i < n1; i++){
				for (int j = 0; j < n2; j++){
					writer.Write(x[i, j]);
				}
			}
		}
		public static int[,] Read2DIntArray2(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n1 = reader.ReadInt32();
			int n2 = reader.ReadInt32();
			int[,] result = new int[n1, n2];
			for (int i = 0; i < n1; i++){
				for (int j = 0; j < n2; j++){
					result[i, j] = reader.ReadInt32();
				}
			}
			return result;
		}
		public static void Write(IList<float[]> x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			writer.Write(x.Count);
			foreach (float[] t in x){
				Write(t, writer);
			}
		}
		public static float[][] Read2DFloatArray(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n = reader.ReadInt32();
			float[][] result = new float[n][];
			for (int i = 0; i < n; i++){
				result[i] = ReadFloatArray(reader);
			}
			return result;
		}
		public static void Write(float[,] x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			int n1 = x.GetLength(0);
			int n2 = x.GetLength(1);
			writer.Write(n1);
			writer.Write(n2);
			for (int i = 0; i < n1; i++){
				for (int j = 0; j < n2; j++){
					writer.Write(x[i, j]);
				}
			}
		}
		public static float[,] Read2DFloatArray2(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n1 = reader.ReadInt32();
			int n2 = reader.ReadInt32();
			float[,] result = new float[n1, n2];
			for (int i = 0; i < n1; i++){
				for (int j = 0; j < n2; j++){
					result[i, j] = reader.ReadSingle();
				}
			}
			return result;
		}
		public static void Write(ushort[,,] x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			int n1 = x.GetLength(0);
			int n2 = x.GetLength(1);
			int n3 = x.GetLength(2);
			writer.Write(n1);
			writer.Write(n2);
			writer.Write(n3);
			for (int i = 0; i < n1; i++){
				for (int j = 0; j < n2; j++){
					for (int k = 0; k < n3; k++){
						writer.Write(x[i, j, k]);
					}
				}
			}
		}
		public static ushort[,,] Read3DUshortArray(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n1 = reader.ReadInt32();
			int n2 = reader.ReadInt32();
			int n3 = reader.ReadInt32();
			ushort[,,] result = new ushort[n1, n2, n3];
			for (int i = 0; i < n1; i++){
				for (int j = 0; j < n2; j++){
					for (int k = 0; k < n3; k++){
						result[i, j, k] = reader.ReadUInt16();
					}
				}
			}
			return result;
		}
		public static void Write(float[,,] x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			int n1 = x.GetLength(0);
			int n2 = x.GetLength(1);
			int n3 = x.GetLength(2);
			writer.Write(n1);
			writer.Write(n2);
			writer.Write(n3);
			for (int i = 0; i < n1; i++){
				for (int j = 0; j < n2; j++){
					for (int k = 0; k < n3; k++){
						writer.Write(x[i, j, k]);
					}
				}
			}
		}
		public static float[,,] Read3DFloatArray(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n1 = reader.ReadInt32();
			int n2 = reader.ReadInt32();
			int n3 = reader.ReadInt32();
			float[,,] result = new float[n1, n2, n3];
			for (int i = 0; i < n1; i++){
				for (int j = 0; j < n2; j++){
					for (int k = 0; k < n3; k++){
						result[i, j, k] = reader.ReadSingle();
					}
				}
			}
			return result;
		}
		public static void Write(bool[,,] x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			int n1 = x.GetLength(0);
			int n2 = x.GetLength(1);
			int n3 = x.GetLength(2);
			writer.Write(n1);
			writer.Write(n2);
			writer.Write(n3);
			for (int i = 0; i < n1; i++){
				for (int j = 0; j < n2; j++){
					for (int k = 0; k < n3; k++){
						writer.Write(x[i, j, k]);
					}
				}
			}
		}
		public static bool[,,] Read3DBooleanArray(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n1 = reader.ReadInt32();
			int n2 = reader.ReadInt32();
			int n3 = reader.ReadInt32();
			bool[,,] result = new bool[n1, n2, n3];
			for (int i = 0; i < n1; i++){
				for (int j = 0; j < n2; j++){
					for (int k = 0; k < n3; k++){
						result[i, j, k] = reader.ReadBoolean();
					}
				}
			}
			return result;
		}
		public static void Write(double[,,] x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			int n1 = x.GetLength(0);
			int n2 = x.GetLength(1);
			int n3 = x.GetLength(2);
			writer.Write(n1);
			writer.Write(n2);
			writer.Write(n3);
			for (int i = 0; i < n1; i++){
				for (int j = 0; j < n2; j++){
					for (int k = 0; k < n3; k++){
						writer.Write(x[i, j, k]);
					}
				}
			}
		}
		public static double[,,] Read3DDoubleArray1(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n1 = reader.ReadInt32();
			int n2 = reader.ReadInt32();
			int n3 = reader.ReadInt32();
			double[,,] result = new double[n1, n2, n3];
			for (int i = 0; i < n1; i++){
				for (int j = 0; j < n2; j++){
					for (int k = 0; k < n3; k++){
						result[i, j, k] = reader.ReadDouble();
					}
				}
			}
			return result;
		}
		public static void Write(IList<double[,]> x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			writer.Write(x.Count);
			foreach (double[,] t in x){
				Write(t, writer);
			}
		}
		public static double[][,] Read3DDoubleArray2(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n = reader.ReadInt32();
			double[][,] result = new double[n][,];
			for (int i = 0; i < n; i++){
				result[i] = Read2DDoubleArray2(reader);
			}
			return result;
		}
		public static void Write(IList<int[][]> x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			writer.Write(x.Count);
			foreach (int[][] t in x){
				Write(t, writer);
			}
		}
		public static int[][][] Read3DIntArray2(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n = reader.ReadInt32();
			int[][][] result = new int[n][][];
			for (int i = 0; i < n; i++){
				result[i] = Read2DInt32Array(reader);
			}
			return result;
		}
		public static void Write(IList<float[,]> x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			writer.Write(x.Count);
			foreach (float[,] t in x){
				Write(t, writer);
			}
		}
		public static float[][,] Read3DFloatArray2(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n = reader.ReadInt32();
			float[][,] result = new float[n][,];
			for (int i = 0; i < n; i++){
				result[i] = Read2DFloatArray2(reader);
			}
			return result;
		}
		public static void Write(IList<double[][]> x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			writer.Write(x.Count);
			foreach (double[][] t in x){
				Write(t, writer);
			}
		}
		public static double[][][] Read3DDoubleArray(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n = reader.ReadInt32();
			double[][][] result = new double[n][][];
			for (int i = 0; i < n; i++){
				result[i] = Read2DDoubleArray(reader);
			}
			return result;
		}
		public static void Write(IList<float[,,]> x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			writer.Write(x.Count);
			foreach (float[,,] t in x){
				Write(t, writer);
			}
		}
		public static float[][,,] Read4DFloatArray2(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n = reader.ReadInt32();
			float[][,,] result = new float[n][,,];
			for (int i = 0; i < n; i++){
				result[i] = Read3DFloatArray(reader);
			}
			return result;
		}
		public static void Write(IList<bool[,,]> x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			writer.Write(x.Count);
			foreach (bool[,,] t in x){
				Write(t, writer);
			}
		}
		public static bool[][,,] Read4DBooleabArray2(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n = reader.ReadInt32();
			bool[][,,] result = new bool[n][,,];
			for (int i = 0; i < n; i++){
				result[i] = Read3DBooleanArray(reader);
			}
			return result;
		}
		public static void Write(float[,][,,] x, BinaryWriter writer){
			bool isNull = x == null;
			writer.Write(isNull);
			if (isNull){
				return;
			}
			int n0 = x.GetLength(0);
			int n1 = x.GetLength(1);
			writer.Write(n0);
			writer.Write(n1);
			for (int i = 0; i < n0; i++){
				for (int j = 0; j < n1; j++){
					Write(x[i, j], writer);
				}
			}
		}
		public static float[,][,,] Read5DFloatArray2(BinaryReader reader){
			bool isNull = reader.ReadBoolean();
			if (isNull){
				return null;
			}
			int n0 = reader.ReadInt32();
			int n1 = reader.ReadInt32();
			float[,][,,] result = new float[n0, n1][,,];
			for (int i0 = 0; i0 < n0; i0++){
				for (int i1 = 0; i1 < n1; i1++){
					result[i0, i1] = Read3DFloatArray(reader);
				}
			}
			return result;
		}
		public static void Write(IDictionary<string, int> x, BinaryWriter writer){
			writer.Write(x.Count);
			foreach (string key in x.Keys){
				writer.Write(key);
				writer.Write(x[key]);
			}
		}
		public static void Write(IDictionary<string, string> x, BinaryWriter writer){
			writer.Write(x.Count);
			foreach (string key in x.Keys){
				writer.Write(key);
				writer.Write(x[key]);
			}
		}
		public static void Write(IDictionary<int, int> x, BinaryWriter writer){
			writer.Write(x.Count);
			foreach (int key in x.Keys){
				writer.Write(key);
				writer.Write(x[key]);
			}
		}
		public static void Write(IDictionary<int, double> x, BinaryWriter writer){
			writer.Write(x.Count);
			foreach (int key in x.Keys){
				writer.Write(key);
				writer.Write(x[key]);
			}
		}
		public static void Write(IDictionary<int, float> x, BinaryWriter writer){
			writer.Write(x.Count);
			foreach (int key in x.Keys){
				writer.Write(key);
				writer.Write(x[key]);
			}
		}
		public static void Write(HashSet<int> x, BinaryWriter writer){
			writer.Write(x.Count);
			foreach (int key in x){
				writer.Write(key);
			}
		}
		public static void Write(HashSet<string> x, BinaryWriter writer){
			writer.Write(x.Count);
			foreach (string key in x){
				writer.Write(key);
			}
		}
		public static bool IsUnix(){
			OperatingSystem os = Environment.OSVersion;
			PlatformID pid = os.Platform;
			return pid == PlatformID.Unix || pid == PlatformID.MacOSX;
		}
		private static int BUFFER_SIZE = 64 * 1024; //64kB
		public static byte[] Compress(byte[] inputData){
			if (inputData == null)
				throw new ArgumentNullException($"Argument must not be null.");
			using (MemoryStream compressIntoMs = new MemoryStream()){
				using (BufferedStream gzs = new BufferedStream(new GZipStream(compressIntoMs, CompressionMode.Compress),
					       BUFFER_SIZE)){
					gzs.Write(inputData, 0, inputData.Length);
				}
				return compressIntoMs.ToArray();
			}
		}
		public static byte[] Decompress(byte[] inputData){
			if (inputData == null)
				throw new ArgumentNullException($"Argument must not be null.");
			using (MemoryStream compressedMs = new MemoryStream(inputData)){
				using (MemoryStream decompressedMs = new MemoryStream()){
					using (BufferedStream gzs =
					       new BufferedStream(new GZipStream(compressedMs, CompressionMode.Decompress), BUFFER_SIZE)){
						gzs.CopyTo(decompressedMs);
					}
					return decompressedMs.ToArray();
				}
			}
		}
	}
}