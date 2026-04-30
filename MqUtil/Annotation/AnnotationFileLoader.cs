using System;
using System.Collections.Generic;
using System.IO;
namespace MqUtil.Annotation{
	public sealed class LoadedProteinAnnotation{
		public int Taxid;
		public string[] Ko = Array.Empty<string>();
		public string[] Cog = Array.Empty<string>();
		public string[] Ec = Array.Empty<string>();
	}

	public sealed class AnnotationFilePreview{
		public string[] Headers = Array.Empty<string>();
		public List<string[]> Rows = new List<string[]>();
	}

	public static class AnnotationFileLoader{
		private const char Bom = '﻿';

		public static AnnotationFilePreview Preview(string path, int nDataRows = 10){
			AnnotationFilePreview result = new AnnotationFilePreview();
			using StreamReader r = new StreamReader(path);
			bool headerRead = false;
			int rowsRead = 0;
			string line;
			bool firstNonEmpty = true;
			while ((line = r.ReadLine()) != null){
				if (firstNonEmpty && line.Length > 0 && line[0] == Bom){
					line = line.Substring(1);
				}
				if (string.IsNullOrWhiteSpace(line)){
					continue;
				}
				firstNonEmpty = false;
				if (!headerRead){
					string headerLine = line;
					if (headerLine.StartsWith("#")){
						headerLine = headerLine.Substring(1);
					}
					result.Headers = headerLine.Split('\t');
					headerRead = true;
					continue;
				}
				if (line.StartsWith("#")){
					continue;
				}
				result.Rows.Add(line.Split('\t'));
				rowsRead++;
				if (rowsRead >= nDataRows){
					break;
				}
			}
			return result;
		}

		public static Dictionary<string, LoadedProteinAnnotation> Load(AnnotationFileInfo info){
			Dictionary<string, LoadedProteinAnnotation> result = new Dictionary<string, LoadedProteinAnnotation>();
			if (string.IsNullOrEmpty(info.filePath) || !File.Exists(info.filePath)){
				return result;
			}
			using StreamReader r = new StreamReader(info.filePath);
			string[] headers = null;
			int idIdx = -1;
			int taxIdx = -1;
			int koIdx = -1;
			int cogIdx = -1;
			int ecIdx = -1;
			string seps = string.IsNullOrEmpty(info.valueSeparators)
				? AnnotationFileInfo.DefaultValueSeparators
				: info.valueSeparators;
			string line;
			bool firstNonEmpty = true;
			while ((line = r.ReadLine()) != null){
				if (firstNonEmpty && line.Length > 0 && line[0] == Bom){
					line = line.Substring(1);
				}
				if (string.IsNullOrWhiteSpace(line)){
					continue;
				}
				firstNonEmpty = false;
				if (headers == null){
					string headerLine = line;
					if (headerLine.StartsWith("#")){
						headerLine = headerLine.Substring(1);
					}
					headers = headerLine.Split('\t');
					idIdx = IndexOf(headers, info.identifierColumn);
					taxIdx = IndexOf(headers, info.taxonomyColumn);
					koIdx = IndexOf(headers, info.koColumn);
					cogIdx = IndexOf(headers, info.cogColumn);
					ecIdx = IndexOf(headers, info.ecColumn);
					continue;
				}
				if (line.StartsWith("#")){
					continue;
				}
				string[] cells = line.Split('\t');
				if (idIdx < 0 || idIdx >= cells.Length){
					continue;
				}
				string id = cells[idIdx]?.Trim();
				if (string.IsNullOrEmpty(id) || id == "-"){
					continue;
				}
				LoadedProteinAnnotation a = new LoadedProteinAnnotation();
				if (taxIdx >= 0 && taxIdx < cells.Length){
					string taxCell = cells[taxIdx]?.Trim();
					if (!string.IsNullOrEmpty(taxCell) && taxCell != "-"){
						int parsed;
						if (int.TryParse(taxCell, out parsed)){
							a.Taxid = parsed;
						}
					}
				}
				a.Ko = ExtractCell(cells, koIdx, seps);
				a.Cog = ExtractCell(cells, cogIdx, seps);
				a.Ec = ExtractCell(cells, ecIdx, seps);
				if (result.ContainsKey(id)){
					Console.WriteLine("AnnotationFileLoader: duplicate identifier '" + id + "' in " + info.filePath +
						" — last write wins.");
				}
				result[id] = a;
			}
			return result;
		}

		public static string[] SplitMulti(string s, string seps){
			if (string.IsNullOrEmpty(s)){
				return Array.Empty<string>();
			}
			char[] sepChars = string.IsNullOrEmpty(seps)
				? AnnotationFileInfo.DefaultValueSeparators.ToCharArray()
				: seps.ToCharArray();
			string[] parts = s.Split(sepChars);
			List<string> result = new List<string>(parts.Length);
			foreach (string p in parts){
				string t = p?.Trim();
				if (string.IsNullOrEmpty(t) || t == "-"){
					continue;
				}
				result.Add(t);
			}
			return result.ToArray();
		}

		private static string[] ExtractCell(string[] cells, int idx, string seps){
			if (idx < 0 || idx >= cells.Length){
				return Array.Empty<string>();
			}
			return SplitMulti(cells[idx], seps);
		}

		private static int IndexOf(string[] headers, string name){
			if (string.IsNullOrEmpty(name) || headers == null){
				return -1;
			}
			for (int i = 0; i < headers.Length; i++){
				if (string.Equals(headers[i], name, StringComparison.Ordinal)){
					return i;
				}
			}
			return -1;
		}
	}
}
