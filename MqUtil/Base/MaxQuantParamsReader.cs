using System.Reflection;
using MqApi.Num;
using MqApi.Util;
using MqUtil.Mol;
using MqUtil.Util;
namespace MqUtil.Base {
	public abstract class MaxQuantParamsReader {
		public abstract void Read(string filePath);

		public static void SetArray(string[] s, InputParameter item, object o) {
			SetDefaultValue(item, o);
			if (item.Type == typeof(string[])) {
				FieldInfo field = o.GetType().GetField(item.VariableName);
				if (field != null) {
					field.SetValue(o, s);
				} else {
					PropertyInfo prop = o.GetType().GetProperty(item.VariableName);
					prop.SetValue(o, s);
				}
			} else if (item.Type == typeof(FastaFileInfo[])) {
				FieldInfo field = o.GetType().GetField(item.VariableName);
				if (field != null) {
					FastaFileInfo[] v = new FastaFileInfo[s.Length];
					for (int i = 0; i < v.Length; i++) {
						v[i] = new FastaFileInfo(s[i]);
					}
					field.SetValue(o, v);
				} else {
					PropertyInfo prop = o.GetType().GetProperty(item.VariableName);
					FastaFileInfo[] v = new FastaFileInfo[s.Length];
					for (int i = 0; i < v.Length; i++) {
						v[i] = new FastaFileInfo(s[i]);
					}
					prop.SetValue(o, v);
				}
			} else if (item.Type == typeof(IsobaricLabelInfo[])) {
				FieldInfo field = o.GetType().GetField(item.VariableName);
				if (field != null) {
					IsobaricLabelInfo[] v = GetIsobaricLabelArray(s);
					field.SetValue(o, v);
				} else {
					PropertyInfo prop = o.GetType().GetProperty(item.VariableName);
					IsobaricLabelInfo[] v = GetIsobaricLabelArray(s);
					prop.SetValue(o, v);
				}
			} else if (item.Type == typeof(short[])) {
				FieldInfo field = o.GetType().GetField(item.VariableName);
				if (field != null) {
					short[] v = new short[s.Length];
					for (int i = 0; i < v.Length; i++) {
						v[i] = Parser.Short(s[i]);
					}
					field.SetValue(o, v);
				} else {
					PropertyInfo prop = o.GetType().GetProperty(item.VariableName);
					short[] v = new short[s.Length];
					for (int i = 0; i < v.Length; i++) {
						v[i] = Parser.Short(s[i]);
					}
					prop.SetValue(o, v);
				}
			} else if (item.Type == typeof(int[])) {
				FieldInfo field = o.GetType().GetField(item.VariableName);
				if (field != null) {
					int[] v = new int[s.Length];
					for (int i = 0; i < v.Length; i++) {
						v[i] = Parser.Int(s[i]);
					}
					field.SetValue(o, v);
				} else {
					PropertyInfo prop = o.GetType().GetProperty(item.VariableName);
					int[] v = new int[s.Length];
					for (int i = 0; i < v.Length; i++) {
						v[i] = Parser.Int(s[i]);
					}
					prop.SetValue(o, v);
				}
			} else if (item.Type == typeof(bool[])) {
				FieldInfo field = o.GetType().GetField(item.VariableName);
				if (field != null) {
					bool[] v = new bool[s.Length];
					for (int i = 0; i < v.Length; i++) {
						v[i] = Parser.Bool(s[i]);
					}
					field.SetValue(o, v);
				} else {
					PropertyInfo prop = o.GetType().GetProperty(item.VariableName);
					bool[] v = new bool[s.Length];
					for (int i = 0; i < v.Length; i++) {
						v[i] = Parser.Bool(s[i]);
					}
					prop.SetValue(o, v);
				}
			} else if (item.Type == typeof(string[][])) {
				//not supported.
			} else {
				throw new Exception("Unknown type: " + item.Type);
			}
		}

		private static IsobaricLabelInfo[] GetIsobaricLabelArray(string[] x) {
			string[][] v = GetIsoLabelModsTable(x);
			IsobaricLabelInfo[] result = new IsobaricLabelInfo[v.Length];
			for (int i = 0; i < result.Length; i++) {
				result[i] = v[i].Length == 8 ? (IsobaricLabelInfo) new IsobaricLabelInfoSimple(v[i]) : new IsobaricLabelInfoComplex(v[i]);
			}
			return result;
		}

		private static string[][] GetIsoLabelModsTable(string[] x){
			int d = GetDivisor(x);
			int n = x.Length / d;
			string[][] result = new string[n][];
			for (int i = 0; i < result.Length; i++) {
				result[i] = x.SubArray(d * i, d * (i + 1));
			}
			return result;
		}
		private static int GetDivisor(string[] x){
			if (x.Length <= 8) {
				return 8;
			}
			bool success = Parser.TryDouble(x[6], out _);
			return success ? 12 : 8;
		}

		private static void SetDefaultValue(InputParameter item, object o) {
			FieldInfo field = o.GetType().GetField(item.VariableName);
			if (field != null) {
				field.SetValue(o, item.DefaultValue);
			} else {
				PropertyInfo prop = o.GetType().GetProperty(item.VariableName);
				prop.SetValue(o, item.DefaultValue);
			}
		}

		public static void SetScalar(string s, InputParameter item, object o) {
			SetDefaultValue(item, o);
			if (s == null) {
				FieldInfo field = o.GetType().GetField(item.VariableName);
				if (field != null) {
					field.SetValue(o, item.DefaultValue);
				} else {
					PropertyInfo prop = o.GetType().GetProperty(item.VariableName);
					prop.SetValue(o, item.DefaultValue);
				}
			} else if (item.Type == typeof(int)) {
				FieldInfo field = o.GetType().GetField(item.VariableName);
				if (field != null) {
					field.SetValue(o, Parser.Int(s));
				} else {
					PropertyInfo prop = o.GetType().GetProperty(item.VariableName);
					prop.SetValue(o, Parser.Int(s));
				}
			} else if (item.Type == typeof(bool)) {
				FieldInfo field = o.GetType().GetField(item.VariableName);
				if (field != null) {
					field.SetValue(o, Parser.Bool(s));
				} else {
					PropertyInfo prop = o.GetType().GetProperty(item.VariableName);
					prop.SetValue(o, Parser.Bool(s));
				}
			} else if (item.Type == typeof(byte)) {
				FieldInfo field = o.GetType().GetField(item.VariableName);
				if (field != null) {
					field.SetValue(o, Parser.Byte(s));
				} else {
					PropertyInfo prop = o.GetType().GetProperty(item.VariableName);
					prop.SetValue(o, Parser.Byte(s));
				}
			} else if (item.Type == typeof(double)) {
				FieldInfo field = o.GetType().GetField(item.VariableName);
				double d;
				try {
					d = Parser.Double(s);
				} catch (OverflowException) {
					d = s.StartsWith("-") ? double.MinValue : double.MaxValue;
				} catch (FormatException) {
					d = double.NaN;
				}
				d = Math.Min(d, 0.999 * double.MaxValue);
				d = Math.Max(d, 0.999 * double.MinValue);
				if (field != null) {
					field.SetValue(o, d);
				} else {
					PropertyInfo prop = o.GetType().GetProperty(item.VariableName);
					prop.SetValue(o, d);
				}
			} else if (item.Type == typeof(string)) {
				FieldInfo field = o.GetType().GetField(item.VariableName);
				if (field != null) {
					field.SetValue(o, s);
				} else {
					PropertyInfo prop = o.GetType().GetProperty(item.VariableName);
					prop.SetValue(o, s);
				}
			} else {
				throw new Exception("Unknown type: " + item.Type);
			}
		}
	}
}