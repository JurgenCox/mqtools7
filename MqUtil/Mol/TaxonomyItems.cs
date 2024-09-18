﻿using System.IO.Compression;
using System.Reflection;
using MqApi.Util;
namespace MqUtil.Mol{
	public class TaxonomyItems{
		private static TaxonomyItems _taxonomyItems;

		public static Func<TaxonomyItems> GetTaxonomyItems =>
			() => _taxonomyItems ?? (_taxonomyItems = new TaxonomyItems());

		public TaxonomyItem[] taxonomyItems;
		public Dictionary<int, TaxonomyItem> taxId2Item;
		public Dictionary<string, TaxonomyItem> name2Item;

		public TaxonomyItems(){
			Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("MqUtil.nodes.dmp.gz");
			StreamReader reader = new StreamReader(new GZipStream(s, CompressionMode.Decompress));
			string line;
			List<TaxonomyItem> result = new List<TaxonomyItem>();
			Dictionary<TaxonomyRank, List<TaxonomyItem>> counts = new Dictionary<TaxonomyRank, List<TaxonomyItem>>();
			taxId2Item = new Dictionary<int, TaxonomyItem>();
			name2Item = new Dictionary<string, TaxonomyItem>();
			while ((line = reader.ReadLine()) != null){
				string[] w = line.Split(new[]{"\t|\t"}, StringSplitOptions.None);
				int taxId = Parser.Int(w[0]);
				int parentTaxId = Parser.Int(w[1]);
				TaxonomyRank rank = GetRank(w[2]);
				int divisionId = Parser.Int(w[4]);
				int geneticCodeId = Parser.Int(w[6]);
				int mitoGeneticCodeId = Parser.Int(w[8]);
				TaxonomyItem ti = new TaxonomyItem(taxId, parentTaxId, rank, divisionId, geneticCodeId,
					mitoGeneticCodeId);
				result.Add(ti);
				if (!counts.ContainsKey(rank)){
					counts.Add(rank, new List<TaxonomyItem>());
				}
				counts[rank].Add(ti);
				taxId2Item.Add(taxId, ti);
			}
			reader.Close();

			s = Assembly.GetExecutingAssembly().GetManifestResourceStream("MqUtil.names.dmp.gz");
			reader = new StreamReader(new GZipStream(s, CompressionMode.Decompress));
			while ((line = reader.ReadLine()) != null){
				string[] w = line.Split(new[]{"\t|\t"}, StringSplitOptions.None);
				int taxId = Parser.Int(w[0]);
				string name = w[1];
				TaxonomyNameType nameType = GetNameType(w[3].Substring(0, w[3].Length - 2));
				TaxonomyItem item = taxId2Item[taxId];
				item.AddName(name, nameType);
				string low = name.ToLower();
				if (!name2Item.ContainsKey(low)){
					name2Item.Add(low, item);
				}
			}
			taxonomyItems = result.ToArray();
		}

		public TaxonomyItem[] GetItemsOfRank(TaxonomyRank rank){
			List<TaxonomyItem> x = new List<TaxonomyItem>();
			foreach (TaxonomyItem ti in taxonomyItems){
				if (ti.Rank == rank){
					x.Add(ti);
				}
			}
			return x.ToArray();
		}

		private static TaxonomyNameType GetNameType(string s){
			switch (s){
				case "synonym":
					return TaxonomyNameType.Synonym;
				case "scientific name":
					return TaxonomyNameType.ScientificName;
				case "in-part":
					return TaxonomyNameType.InPart;
				case "blast name":
					return TaxonomyNameType.BlastName;
				case "genbank common name":
					return TaxonomyNameType.GenbankCommonName;
				case "equivalent name":
					return TaxonomyNameType.EquivalentName;
				case "type material":
					return TaxonomyNameType.TypeMaterial;
				case "includes":
					return TaxonomyNameType.Includes;
				case "authority":
					return TaxonomyNameType.Authority;
				case "misspelling":
					return TaxonomyNameType.Misspelling;
				case "genbank synonym":
					return TaxonomyNameType.GenbankSynonym;
				case "common name":
					return TaxonomyNameType.CommonName;
				case "misnomer":
					return TaxonomyNameType.Misnomer;
				case "anamorph":
					return TaxonomyNameType.Anamorph;
				case "genbank anamorph":
					return TaxonomyNameType.GenbankAnamorph;
				case "teleomorph":
					return TaxonomyNameType.Teleomorph;
				case "acronym":
					return TaxonomyNameType.Acronym;
				case "genbank acronym":
					return TaxonomyNameType.GenbankAcronym;
				default:
					throw new Exception("Unknown name type: " + s);
			}
		}

		private static TaxonomyRank GetRank(string s){
			switch (s){
				case "no rank":
					return TaxonomyRank.NoRank;
				case "superkingdom":
					return TaxonomyRank.Superkingdom;
				case "genus":
					return TaxonomyRank.Genus;
				case "species":
					return TaxonomyRank.Species;
				case "order":
					return TaxonomyRank.Order;
				case "family":
					return TaxonomyRank.Family;
				case "subspecies":
					return TaxonomyRank.Subspecies;
				case "subfamily":
					return TaxonomyRank.Subfamily;
				case "tribe":
					return TaxonomyRank.Tribe;
				case "phylum":
					return TaxonomyRank.Phylum;
				case "class":
					return TaxonomyRank.Class;
				case "forma":
					return TaxonomyRank.Forma;
				case "suborder":
					return TaxonomyRank.Suborder;
				case "superclass":
					return TaxonomyRank.Superclass;
				case "subclass":
					return TaxonomyRank.Subclass;
				case "varietas":
					return TaxonomyRank.Varietas;
				case "kingdom":
					return TaxonomyRank.Kingdom;
				case "superfamily":
					return TaxonomyRank.Superfamily;
				case "infraorder":
					return TaxonomyRank.Infraorder;
				case "subphylum":
					return TaxonomyRank.Subphylum;
				case "infraclass":
					return TaxonomyRank.Infraclass;
				case "superorder":
					return TaxonomyRank.Superorder;
				case "subgenus":
					return TaxonomyRank.Subgenus;
				case "parvorder":
					return TaxonomyRank.Parvorder;
				case "superphylum":
					return TaxonomyRank.Superphylum;
				case "species group":
					return TaxonomyRank.SpeciesGroup;
				case "species subgroup":
					return TaxonomyRank.SpeciesSubgroup;
				case "subtribe":
					return TaxonomyRank.Subtribe;
				case "subkingdom":
					return TaxonomyRank.Subkingdom;
				case "cohort":
					return TaxonomyRank.Cohort;
				default:
					throw new Exception("Unknown rank: " + s);
			}
		}

		public string GetTaxonomyIdOfRank(string taxonomyId, TaxonomyRank rank){
			if (!Parser.TryInt(taxonomyId, out int id)){
				return taxonomyId;
			}
			if (!taxId2Item.ContainsKey(id)){
				return taxonomyId;
			}
			TaxonomyItem item = taxId2Item[id];
			return "" + item.TaxId;
		}

		public static string GetScientificName(int id) {
			TaxonomyItems x = GetTaxonomyItems();
			if (x.taxId2Item.ContainsKey(id)) {
				string n = x.taxId2Item[id].GetScientificName();
				return n;
			}
			return "";
		}
		public static string[] GetScientificNames(int[] id) {
			string[] result = new string[id.Length];
			for (int i = 0; i < id.Length; i++){
				result[i] = GetScientificName(id[i]);
			}
			return result;
		}
	}
}