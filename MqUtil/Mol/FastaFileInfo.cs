using System;
using System.Collections.Generic;
using System.IO;
using MqApi.Util;
using MqUtil.Util;
namespace MqUtil.Mol{
	public class FastaFileInfo : IComparable<FastaFileInfo>{
		public readonly List<InputParameter> vals = new List<InputParameter>{
			new InputParameter<string>("fastaFilePath", "fastaFilePath"),
			new InputParameter<string>("identifierParseRule", "identifierParseRule"),
			new InputParameter<string>("descriptionParseRule", "descriptionParseRule"),
			new InputParameter<string>("taxonomyParseRule", "taxonomyParseRule"),
			new InputParameter<string>("variationParseRule", "variationParseRule"),
			new InputParameter<string>("modificationParseRule", "modificationParseRule"),
			new InputParameter<string>("taxonomyId", "taxonomyId"),
			new InputParameter<string>("koParseRule", "koParseRule"),
			new InputParameter<string>("cogParseRule", "cogParseRule"),
			new InputParameter<string>("ecParseRule", "ecParseRule")
		};

		public readonly Dictionary<string, InputParameter> map;
		public FastaFileInfo() : this("", "", "", "", "", "", "", "", "", ""){ }
		public string fastaFilePath;
		public string identifierParseRule;
		public string descriptionParseRule;
		public string taxonomyParseRule;
		public string taxonomyId;
		public string variationParseRule;
		public string modificationParseRule;
		public string koParseRule;
		public string cogParseRule;
		public string ecParseRule;

		public FastaFileInfo(string fastaFilePath) : this(fastaFilePath, Tables.GetIdentifierParseRule(fastaFilePath),
			Tables.GetDescriptionParseRule(fastaFilePath), Tables.GetTaxonomyParseRule(fastaFilePath),
			Tables.GetTaxonomyId(fastaFilePath), Tables.GetVariationParseRule(fastaFilePath),
			Tables.GetModificationParseRule(fastaFilePath), Tables.GetKoParseRule(fastaFilePath),
			Tables.GetCogParseRule(fastaFilePath), Tables.GetEcParseRule(fastaFilePath)){ }

		public FastaFileInfo(string fastaFilePath, string identifierParseRule, string descriptionParseRule,
			string taxonomyParseRule, string taxonomyId, string variationParseRule, string modificationParseRule,
			string koParseRule = "", string cogParseRule = "", string ecParseRule = ""){
			map = new Dictionary<string, InputParameter>();
			foreach (InputParameter val in vals){
				map.Add(val.Name, val);
			}
			this.fastaFilePath = fastaFilePath;
			this.identifierParseRule = identifierParseRule;
			this.descriptionParseRule = descriptionParseRule;
			this.taxonomyParseRule = taxonomyParseRule;
			this.variationParseRule = variationParseRule;
			this.modificationParseRule = modificationParseRule;
			this.taxonomyId = taxonomyId;
			this.koParseRule = koParseRule;
			this.cogParseRule = cogParseRule;
			this.ecParseRule = ecParseRule;
		}

		public FastaFileInfo(string[] s) : this(){
			fastaFilePath = s[0];
			identifierParseRule = s[1];
			descriptionParseRule = s[2];
			taxonomyParseRule = s[3];
			taxonomyId = s[4];
			variationParseRule = s[5];
			modificationParseRule = s[6];
			if (s.Length > 7) koParseRule = s[7];
			if (s.Length > 8) cogParseRule = s[8];
			if (s.Length > 9) ecParseRule = s[9];
		}

		public string[] ToStringArray(){
			return new[]{
				fastaFilePath, identifierParseRule, descriptionParseRule, taxonomyParseRule, taxonomyId,
				variationParseRule, modificationParseRule, koParseRule, cogParseRule, ecParseRule
			};
		}

		public static string[] GetFastaFilePath(FastaFileInfo[] filePaths){
			string[] result = new string[filePaths.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = filePaths[i].fastaFilePath;
			}
			return result;
		}

		public static string[] GetIdentifierParseRule(FastaFileInfo[] filePaths){
			string[] result = new string[filePaths.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = filePaths[i].identifierParseRule;
			}
			return result;
		}

		public static string[] GetDescriptionParseRule(FastaFileInfo[] filePaths){
			string[] result = new string[filePaths.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = filePaths[i].descriptionParseRule;
			}
			return result;
		}

		public static string[] GetTaxonomyParseRule(FastaFileInfo[] filePaths){
			string[] result = new string[filePaths.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = filePaths[i].taxonomyParseRule;
			}
			return result;
		}

		public static string[] GetTaxonomyId(FastaFileInfo[] filePaths){
			string[] result = new string[filePaths.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = filePaths[i].taxonomyId;
			}
			return result;
		}

		public static string[] GetVariationParseRule(FastaFileInfo[] filePaths){
			string[] result = new string[filePaths.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = filePaths[i].variationParseRule;
			}
			return result;
		}

		public static string[] GetModificationParseRule(FastaFileInfo[] filePaths){
			string[] result = new string[filePaths.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = filePaths[i].modificationParseRule;
			}
			return result;
		}

		public static string[] GetKoParseRule(FastaFileInfo[] filePaths){
			string[] result = new string[filePaths.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = filePaths[i].koParseRule;
			}
			return result;
		}

		public static string[] GetCogParseRule(FastaFileInfo[] filePaths){
			string[] result = new string[filePaths.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = filePaths[i].cogParseRule;
			}
			return result;
		}

		public static string[] GetEcParseRule(FastaFileInfo[] filePaths){
			string[] result = new string[filePaths.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = filePaths[i].ecParseRule;
			}
			return result;
		}

		public static string[][] AdaptFastaFiles(FastaFileInfo[] mqparFastaFiles){
			string[][] result = new string[mqparFastaFiles.Length][];
			for (int i = 0; i < result.Length; i++){
				result[i] = mqparFastaFiles[i].ToStringArray();
			}
			return result;
		}

		public static FastaFileInfo[] AdaptFastaFiles(string[][] mqparFastaFiles){
			FastaFileInfo[] result = new FastaFileInfo[mqparFastaFiles.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = new FastaFileInfo(mqparFastaFiles[i]);
			}
			return result;
		}

		public int CompareTo(FastaFileInfo other){
			return String.Compare(fastaFilePath, other.fastaFilePath, StringComparison.InvariantCulture);
		}

		public override string ToString(){
			return fastaFilePath;
		}
		public static string GetContaminantFilePath() {
			return Path.Combine(FileUtils.GetConfigPath(), "contaminants.fasta");
		}

		public static string GetContaminantParseRule() {
			return ">([^ ]*)";
		}

		public static FastaFileInfo GetContaminantFastaFile() {
			return new FastaFileInfo(GetContaminantFilePath(), GetContaminantParseRule(), "", "", "", "", "-1");
		}


	}
}