using System.Reflection;
using MqApi.Util;
using MqUtil.Mol;
using MqUtil.Util;
namespace MqUtil.Base {
	public abstract class MaxQuantParamsWriter {
		public static string indent = "   ";
		protected bool verboseFasta;
		protected bool verboseIsobaricLabels;
		public abstract void Write(string filePath);

		private static object[] GetArray(InputParameter item, object o) {
			FieldInfo field = o.GetType().GetField(item.VariableName);
			object x;
			if (field != null) {
				x = field.GetValue(o);
			} else {
				PropertyInfo prop = o.GetType().GetProperty(item.VariableName);
				x = prop.GetValue(o);
			}
			try {
				return ((Array) x).Cast<object>().ToArray();
			} catch (Exception) {
				return new object[0];
			}
		}

		private static FastaFileInfo[] GetFastaFileInfoArray(InputParameter item, object o) {
			FieldInfo field = o.GetType().GetField(item.VariableName);
			object x;
			if (field != null) {
				x = field.GetValue(o);
			} else {
				PropertyInfo prop = o.GetType().GetProperty(item.VariableName);
				x = prop.GetValue(o);
			}
			try {
				return (FastaFileInfo[])x;
			} catch (Exception) {
				return new FastaFileInfo[0];
			}
		}

		private static IsobaricLabelInfo[] GetIsobaricLabelInfoArray(InputParameter item, object o) {
			FieldInfo field = o.GetType().GetField(item.VariableName);
			object x;
			if (field != null) {
				x = field.GetValue(o);
			} else {
				PropertyInfo prop = o.GetType().GetProperty(item.VariableName);
				x = prop.GetValue(o);
			}
			try {
				return (IsobaricLabelInfo[])x;
			} catch (Exception) {
				return new IsobaricLabelInfo[0];
			}
		}

		private static object GetScalar(InputParameter item, object o) {
			if (item.Type == typeof(int)) {
				FieldInfo field = o.GetType().GetField(item.VariableName);
				if (field != null) {
					return field.GetValue(o);
				}
				PropertyInfo prop = o.GetType().GetProperty(item.VariableName);
				return prop.GetValue(o);
			}
			if (item.Type == typeof(bool)) {
				FieldInfo field = o.GetType().GetField(item.VariableName);
				if (field != null) {
					return field.GetValue(o);
				}
				PropertyInfo prop = o.GetType().GetProperty(item.VariableName);
				return prop.GetValue(o);
			}
			if (item.Type == typeof(byte)) {
				FieldInfo field = o.GetType().GetField(item.VariableName);
				if (field != null) {
					return field.GetValue(o);
				}
				PropertyInfo prop = o.GetType().GetProperty(item.VariableName);
				return prop.GetValue(o);
			}
			if (item.Type == typeof(double)) {
				FieldInfo field = o.GetType().GetField(item.VariableName);
				if (field != null) {
					return field.GetValue(o);
				}
				PropertyInfo prop = o.GetType().GetProperty(item.VariableName);
				return prop.GetValue(o);
			}
			if (item.Type == typeof(string)) {
				FieldInfo field = o.GetType().GetField(item.VariableName);
				if (field != null) {
					return field.GetValue(o);
				}
				PropertyInfo prop = o.GetType().GetProperty(item.VariableName);
				return prop.GetValue(o);
			}
			throw new Exception("Unknown type: " + item.Type);
		}

		public static void Write(TextWriter writer, InputParameter val, object obj, string baseIndent, bool verboseFasta,
			bool verboseIsobaricLabels) {
			if (val.IsArray) {
				writer.WriteLine(baseIndent + "<" + val.Name + ">");
				if (verboseFasta && val.IsFastaFileInfo) {
					FastaFileInfo[] info = GetFastaFileInfoArray(val, obj) ?? new FastaFileInfo[0];
					if (val.Name == "fastaFiles"  && info.Length == 0) {
						//why?
						//WriteFastaFileInfo(writer, baseIndent + indent, val.ElementTypeName, new FastaFileInfo(""));
					} else {
						foreach (FastaFileInfo o1 in info) {
							WriteFastaFileInfo(writer, baseIndent + indent, val.ElementTypeName, o1);
						}
					}
				} else if (val.IsIsobaricLabelInfo) {
					IsobaricLabelInfo[] info = GetIsobaricLabelInfoArray(val, obj);
					if (verboseIsobaricLabels) {
						foreach (IsobaricLabelInfo o1 in info) {
							WriteIsobaricLabelInfo(writer, baseIndent + indent, val.ElementTypeName, o1);
						}
					} else {
						foreach (IsobaricLabelInfo o1 in info) {
							if (!string.IsNullOrEmpty(o1.internalLabel)) {
								WriteElement(writer, baseIndent + indent, "string", o1.internalLabel);
							}
							if (!string.IsNullOrEmpty(o1.terminalLabel)) {
								WriteElement(writer, baseIndent + indent, "string", o1.terminalLabel);
							}
						}
					}
				} else {
					foreach (object o1 in GetArray(val, obj)) {
						WriteElement(writer, baseIndent + indent, val.ElementTypeName, o1);
					}
				}
				writer.WriteLine(baseIndent + "</" + val.Name + ">");
			} else {
				WriteElement(writer, baseIndent, val.Name, GetScalar(val, obj));
			}
		}

		private static void WriteFastaFileInfo(TextWriter writer, string indent1, string name, FastaFileInfo ffi) {
			writer.WriteLine(indent1 + "<" + name + ">");
			foreach (InputParameter ip in ffi.vals) {
				WriteElement(writer, indent1 + indent, ip.Name, GetScalar(ip, ffi));
			}
			writer.WriteLine(indent1 + "</" + name + ">");
		}

		private static void WriteIsobaricLabelInfo(TextWriter writer, string indent1, string name, IsobaricLabelInfo ffi) {
			writer.WriteLine(indent1 + "<" + name + ">");
			foreach (InputParameter ip in ffi.vals) {
				WriteElement(writer, indent1 + indent, ip.Name, GetScalar(ip, ffi));
			}
			writer.WriteLine(indent1 + "</" + name + ">");
		}

		private static void WriteElement(TextWriter writer, string indent1, string name, object value) {
			writer.WriteLine(indent1 + "<" + name + ">" + (Parser.ToString(value) ?? "") + "</" + name + ">");
		}
	}
}