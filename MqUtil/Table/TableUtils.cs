using MqApi.Drawing;
using MqApi.Num;
using MqApi.Util;
using MqUtil.Drawing;
using MqUtil.Num;
using MqUtil.Symbol;
namespace MqUtil.Table {
	public static class TableUtils {
		public static void WriteElement(BinaryWriter writer, object o, ColumnType columnType) {
			switch (columnType) {
				case ColumnType.Boolean:
					if (o is DBNull) {
						writer.Write(false);
					} else if (o is string) {
						string x = (string)o;
						bool success = Parser.TryBool(x, out bool y);
						if (!success) {
							y = false;
						}
						writer.Write(y);
					} else {
						writer.Write((bool)o);
					}
					break;
				case ColumnType.Text:
					if (o == null) {
						writer.Write("");
					} else if (o is DBNull) {
						writer.Write("");
					} else if (o is char) {
						writer.Write("" + (char)o);
					} else if (o is int) {
						writer.Write("" + (int)o);
					} else if (o is string) {
						writer.Write((string)o);
					} else{
						writer.Write(o.ToString());
					}
					break;
				case ColumnType.Integer:
					if (o == null || o is DBNull) {
						writer.Write(int.MaxValue);
					} else if (o is UInt16) {
						UInt16 x = (UInt16)o;
						int y = x;
						writer.Write(y);
					} else if (o is UInt32) {
						UInt32 x = (UInt32)o;
						int y = (int)x;
						writer.Write(y);
					} else if (o is Int16) {
						Int16 x = (Int16)o;
						int y = x;
						writer.Write(y);
					} else if (o is byte) {
						byte x = (byte)o;
						int y = x;
						writer.Write(y);
					} else if (o is UInt64) {
						ulong x = (ulong)o;
						int y = (int)x;
						writer.Write(y);
					} else if (o is Int64) {
						long x = (long)o;
						int y = (int)x;
						writer.Write(y);
					} else if (o is double) {
						double x = (double)o;
						int y = (int)Math.Round(x);
						writer.Write(y);
					} else if (o is float) {
						float x = (float)o;
						int y = (int)Math.Round(x);
						writer.Write(y);
					} else if (o is string) {
						string x = (string)o;
						bool success = Parser.TryInt(x, out int y);
						if (!success){
							y = 0;
						}
						writer.Write(y);
					} else {
						writer.Write((int)o);
					}
					break;
				case ColumnType.Numeric:
					if (o == null || o is DBNull) {
						writer.Write(double.NaN);
					} else if (o is int) {
						int x = (int)o;
						double d = x;
						writer.Write(d);
					} else if (o is float) {
						float x = (float)o;
						double d = x;
						writer.Write(d);
					} else if (o is UInt16) {
						UInt16 x = (UInt16)o;
						double y = x;
						writer.Write(y);
					} else if (o is UInt32) {
						UInt32 x = (UInt32)o;
						double y = (double)x;
						writer.Write(y);
					} else if (o is Int16) {
						Int16 x = (Int16)o;
						double y = x;
						writer.Write(y);
					} else if (o is byte) {
						byte x = (byte)o;
						double y = x;
						writer.Write(y);
					} else if (o is UInt64) {
						ulong x = (ulong)o;
						double y = (int)x;
						writer.Write(y);
					} else if (o is Int64) {
						long x = (long)o;
						double y = (int)x;
						writer.Write(y);
					} else if (o is string) {
						string x = (string)o;
						bool success = Parser.TryDouble(x, out double y);
						if (!success) {
							y = double.NaN;
						}
						writer.Write(y);
					} else {
						writer.Write((double)o);
					}
					break;
				case ColumnType.Categorical:
					if (o == null || o is DBNull) {
						writer.Write("");
					} else if (o is Boolean) {
						bool x = (bool)o;
						string y = Parser.ToString(x);
						writer.Write(y);
					} else if (o is char) {
						char x = (char)o;
						string y = "" + x;
						writer.Write(y);
					} else {
						writer.Write((string)o);
					}
					break;
				case ColumnType.MultiNumeric:
					writer.Write((string)o);
					break;
				case ColumnType.MultiInteger:
					if (o is int[]) {
						int[] x = (int[])o;
						string s = StringUtils.Concat(";", x);
						writer.Write(s);
					} else {
						writer.Write((string)o);
					}
					break;
				case ColumnType.DateTime:
					writer.Write(((DateTime)o).Ticks);
					break;
				case ColumnType.Color:
					writer.Write(((Color2)o).Value);
					break;
				case ColumnType.DashStyle:
					writer.Write((int)o);
					break;
				case ColumnType.SymbolType:
					int w = (int)o;
					writer.Write(w);
					break;
				default:
					throw new Exception("Never get here.");
			}
		}
		public static object ReadElement(BinaryReader reader, ColumnType columnType) {
			switch (columnType) {
				case ColumnType.Boolean:
					return reader.ReadBoolean();
				case ColumnType.Text:
					return reader.ReadString();
				case ColumnType.Integer:
					return reader.ReadInt32();
				case ColumnType.Numeric:
					return reader.ReadDouble();
				case ColumnType.Categorical:
					return reader.ReadString();
				case ColumnType.MultiNumeric:
					return reader.ReadString();
				case ColumnType.MultiInteger:
					return reader.ReadString();
				case ColumnType.DateTime:
					return new DateTime(reader.ReadInt64());
				case ColumnType.Color:
					return Color2.FromArgb(reader.ReadInt32());
				case ColumnType.DashStyle:
					return reader.ReadInt32();
				case ColumnType.SymbolType:
					return reader.ReadInt32();
				default:
					throw new Exception("Never get here.");
			}
		}
		//TODO
		public static RenderTableCell GetCellRenderer(ColumnType type){
			switch (type){
				case ColumnType.SymbolType:
					return (g1, selected, o, width, x1, y1) => {
						IGraphics g = g1;
						int index = (int)o;
						if (index < 0) {
							return;
						}
						SymbolType stype = SymbolType.allSymbols[index];
						if (selected) {
							stype.Draw(13, x1 + 9, y1 + 11, g, Pens2.White, Brushes2.White);
						} else {
							stype.Draw(13, x1 + 9, y1 + 11, g, Pens2.Red, Brushes2.Red);
						}
					};
				default:
					return null;
			}
		}
		public static string ColumnTypeToString(ColumnType ct) {
			switch (ct) {
				case ColumnType.Boolean:
					return "C";
				case ColumnType.Categorical:
					return "C";
				case ColumnType.Color:
					return "C";
				case ColumnType.DateTime:
					return "T";
				case ColumnType.DashStyle:
					return "C";
				case ColumnType.Integer:
					return "N";
				case ColumnType.SymbolType:
					return "C";
				case ColumnType.MultiInteger:
					return "M";
				case ColumnType.MultiNumeric:
					return "M";
				case ColumnType.Numeric:
					return "N";
				case ColumnType.Text:
					return "T";
				default:
					return "T";
			}
		}
		public const int ratioSigDigits = 5;
		public static void SetDouble(DataRow2 row, string name, double value, int digits) {
			row[name] = NumUtils.RoundSignificantDigits(value, digits);
		}
		public static void CollapseFiles(LongList<long> allFilePos, string txtFolder, string serFolder, int nfiles,
			string txtFile, string serFile)
		{
			string bigTxtFilename = Path.Combine(txtFolder, txtFile);
			string binFilename = Path.Combine(serFolder, serFile);
			if (File.Exists(bigTxtFilename))
			{
				File.Delete(bigTxtFilename);
			}
			if (File.Exists(binFilename))
			{
				File.Delete(binFilename);
			}
			if (File.Exists(binFilename + "_0x") && !File.Exists(binFilename + "x"))
			{
				File.Move(binFilename + "_0x", binFilename + "x");
			}
			Delete000Files(txtFolder);
			long lengthTxt = new FileInfo(bigTxtFilename + "_0").Length;
			File.Move(bigTxtFilename + "_0", bigTxtFilename);
			long length = new FileInfo(binFilename + "_0").Length;
			File.Move(binFilename + "_0", binFilename);
			string filenameTxt = bigTxtFilename + "_0";
			long[] filePos = ReadFilePos(filenameTxt + "Pos");
			allFilePos.AddRange(filePos);
			bool firstTime = true;
			string origFilename = bigTxtFilename;
			for (int fileIndex = 1; fileIndex < nfiles; fileIndex++)
			{
				filenameTxt = origFilename + "_" + fileIndex;
				long newTxtLen = new FileInfo(filenameTxt).Length;
				filePos = ReadFilePos(filenameTxt + "Pos");
				for (int i = 0; i < filePos.Length; i++)
				{
					filePos[i] += length;
				}
				allFilePos.AddRange(filePos);
				if (lengthTxt + newTxtLen > maxTxtFileSize)
				{
					if (firstTime)
					{
						int x = bigTxtFilename.LastIndexOf('.');
						string s1 = bigTxtFilename.Substring(0, x);
						string s2 = bigTxtFilename.Substring(x);
						File.Move(bigTxtFilename, s1 + "00000" + s2);
					}
					bigTxtFilename = GetNextFilename(bigTxtFilename);
					File.Move(filenameTxt, bigTxtFilename);
					lengthTxt = 0;
					firstTime = false;
				}
				else
				{
					Append(bigTxtFilename, filenameTxt);
				}
				length += AppendBin(binFilename, binFilename + "_" + fileIndex);
				lengthTxt += newTxtLen;
			}
		}
		private const long maxTxtFileSize = 50L * 1024L * 1024L * 1024L;
		private static void Delete000Files(string txtFolder)
		{
			foreach (string file in Directory.GetFiles(txtFolder))
			{
				int x = file.LastIndexOf('.');
				if (x < 0)
				{
					continue;
				}
				if (x < 3)
				{
					continue;
				}
				string s1 = file.Substring(x - 3, 3);
				if (StringUtils.AreDigits(s1))
				{
					File.Delete(file);
				}
			}
		}
		private static string GetNextFilename(string s)
		{
			int x = s.LastIndexOf('.');
			string s1 = s.Substring(0, x);
			string s2 = s.Substring(x);
			char c = s1[s1.Length - 1];
			if (c < '0' || x > '9')
			{
				return s1 + "00001" + s2;
			}
			int w = int.Parse(s1.Substring(s1.Length - 5));
			return s1.Substring(0, s1.Length - 5) + StringUtils.IntToStringLeadingZeroes(w + 1, 5) + s2;
		}
		private static long[] ReadFilePos(string filename)
		{
			BinaryReader reader = FileUtils.GetBinaryReader(filename);
			long[] filePos = FileUtils.ReadInt64Array(reader);
			reader.Close();
			File.Delete(filename);
			return filePos;
		}

		public static void Append(string bigFile, string smallFile)
		{
			StreamWriter writer = new StreamWriter(bigFile, true);
			StreamReader reader = new StreamReader(smallFile);
			string line;
			while ((line = reader.ReadLine()) != null)
			{
				writer.WriteLine(line);
			}
			reader.Close();
			writer.Close();
			File.Delete(smallFile);
		}
		private static long AppendBin(string bigFile, string smallFile)
		{
			Stream extra = new FileStream(smallFile, FileMode.Open, FileAccess.Read);
			long len = extra.Length;
			byte[] buffer = new byte[32 * 1024];
			Stream original = new FileStream(bigFile, FileMode.Append);
			try
			{
				int blockSize;
				while ((blockSize = extra.Read(buffer, 0, buffer.Length)) > 0)
				{
					original.Write(buffer, 0, blockSize);
				}
			}
			catch (Exception)
			{
			}
			finally
			{
				original.Close();
			}
			extra.Close();
			File.Delete(smallFile);
			return len;
		}

	}
}
