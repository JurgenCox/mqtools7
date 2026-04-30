using System;
using System.Collections.Generic;
using MqUtil.Util;
namespace MqUtil.Annotation{
	[Serializable]
	public class AnnotationFileInfo : IComparable<AnnotationFileInfo>{
		public const string DefaultValueSeparators = ";,";

		public readonly List<InputParameter> vals = new List<InputParameter>{
			new InputParameter<string>("filePath", "filePath"),
			new InputParameter<string>("identifierColumn", "identifierColumn"),
			new InputParameter<string>("taxonomyColumn", "taxonomyColumn"),
			new InputParameter<string>("koColumn", "koColumn"),
			new InputParameter<string>("cogColumn", "cogColumn"),
			new InputParameter<string>("ecColumn", "ecColumn"),
			new InputParameter<string>("valueSeparators", "valueSeparators", DefaultValueSeparators)
		};

		public readonly Dictionary<string, InputParameter> map;
		public string filePath;
		public string identifierColumn;
		public string taxonomyColumn;
		public string koColumn;
		public string cogColumn;
		public string ecColumn;
		public string valueSeparators;

		public AnnotationFileInfo() : this("", "", "", "", "", "", DefaultValueSeparators){ }

		public AnnotationFileInfo(string filePath) : this(filePath, "", "", "", "", "", DefaultValueSeparators){ }

		public AnnotationFileInfo(string filePath, string identifierColumn, string taxonomyColumn, string koColumn,
			string cogColumn, string ecColumn, string valueSeparators){
			map = new Dictionary<string, InputParameter>();
			foreach (InputParameter val in vals){
				map.Add(val.Name, val);
			}
			this.filePath = filePath;
			this.identifierColumn = identifierColumn;
			this.taxonomyColumn = taxonomyColumn;
			this.koColumn = koColumn;
			this.cogColumn = cogColumn;
			this.ecColumn = ecColumn;
			this.valueSeparators = string.IsNullOrEmpty(valueSeparators) ? DefaultValueSeparators : valueSeparators;
		}

		public AnnotationFileInfo(string[] s) : this(){
			filePath = s[0];
			identifierColumn = s.Length > 1 ? s[1] : "";
			taxonomyColumn = s.Length > 2 ? s[2] : "";
			koColumn = s.Length > 3 ? s[3] : "";
			cogColumn = s.Length > 4 ? s[4] : "";
			ecColumn = s.Length > 5 ? s[5] : "";
			string sep = s.Length > 6 ? s[6] : "";
			valueSeparators = string.IsNullOrEmpty(sep) ? DefaultValueSeparators : sep;
		}

		public string[] ToStringArray(){
			return new[]{
				filePath, identifierColumn, taxonomyColumn, koColumn, cogColumn, ecColumn, valueSeparators
			};
		}

		public static string[] GetFilePath(AnnotationFileInfo[] files){
			string[] result = new string[files.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = files[i].filePath;
			}
			return result;
		}

		public static string[] GetIdentifierColumn(AnnotationFileInfo[] files){
			string[] result = new string[files.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = files[i].identifierColumn;
			}
			return result;
		}

		public static string[] GetTaxonomyColumn(AnnotationFileInfo[] files){
			string[] result = new string[files.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = files[i].taxonomyColumn;
			}
			return result;
		}

		public static string[] GetKoColumn(AnnotationFileInfo[] files){
			string[] result = new string[files.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = files[i].koColumn;
			}
			return result;
		}

		public static string[] GetCogColumn(AnnotationFileInfo[] files){
			string[] result = new string[files.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = files[i].cogColumn;
			}
			return result;
		}

		public static string[] GetEcColumn(AnnotationFileInfo[] files){
			string[] result = new string[files.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = files[i].ecColumn;
			}
			return result;
		}

		public static string[] GetValueSeparators(AnnotationFileInfo[] files){
			string[] result = new string[files.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = files[i].valueSeparators;
			}
			return result;
		}

		public static string[][] AdaptFiles(AnnotationFileInfo[] files){
			string[][] result = new string[files.Length][];
			for (int i = 0; i < result.Length; i++){
				result[i] = files[i].ToStringArray();
			}
			return result;
		}

		public static AnnotationFileInfo[] AdaptFiles(string[][] files){
			AnnotationFileInfo[] result = new AnnotationFileInfo[files.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = new AnnotationFileInfo(files[i]);
			}
			return result;
		}

		public int CompareTo(AnnotationFileInfo other){
			return string.Compare(filePath, other.filePath, StringComparison.InvariantCulture);
		}

		public override string ToString(){
			return filePath;
		}
	}
}
