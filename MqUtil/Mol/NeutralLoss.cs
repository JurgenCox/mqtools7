﻿using System.Xml.Serialization;
namespace MqUtil.Mol{
	[Serializable, XmlRoot(ElementName = "neutralloss", IsNullable = false)]
	public class NeutralLoss{
		private string name;
		private string shortname;
		private double deltamass = double.NaN;
		private string composition = "";

		public NeutralLoss(){
			// Default Constructor for Serialization
		}

		public NeutralLoss(string shortname, string name){
			this.shortname = shortname;
			this.name = name;
		}

		[XmlAttribute("name")]
		public string Name { get => name;
			set => name = value;
		}
		[XmlAttribute("shortname")]
		public string ShortName { get => shortname;
			set => shortname = value;
		}
		[XmlIgnore]
		public double DeltaMass{
			get{
				if (double.IsNaN(deltamass)){
					ChemElements.DecodeComposition(composition, ChemElements.ElementDictionary, out int[] counts, out string[] comp, out double[] mono);
					deltamass = 0;
					for (int i = 0; i < mono.Length; i++){
						deltamass += mono[i]*counts[i];
					}
				}
				return deltamass;
			}
			set => deltamass = value;
		}
		[XmlAttribute("composition")]
		public string Composition { get => composition;
			set => composition = value;
		}
	}
}