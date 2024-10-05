using System.Xml.Serialization;
using MqApi.Util;
using MqUtil.Util;
namespace MqUtil.Ms.Annot{
	[Serializable, System.Diagnostics.DebuggerStepThrough, XmlType(AnonymousType = true),
	XmlRoot(ElementName = "AnnotationConfig", IsNullable = false)]
	public class AnnotationConfig{
		[XmlArray("NeutralLoss")]
		public NeutralLossLib[] NeutralLoss { get; set; }
		[XmlArray("ImmoniumIon")]
		public ImmoniumIon[] ImmoniumIon { get; set; }
		[XmlArray("SideChain")]
		public SideChain[] SideChain { get; set; }

		public void Write(string filename){
			XmlSerialization.SerializeObject(this, filename);
		}

		public static string Filename => Path.Combine(FileUtils.GetConfigPath(), "annotation_config.xml");

		public static AnnotationConfig Read(){
			if (!File.Exists(Filename)){
				throw new Exception("Could not find file: " + Filename);
			}
			return (AnnotationConfig) XmlSerialization.DeserializeObject(Filename, typeof (AnnotationConfig));
		}

		private static NeutralLossLib[] neutralLossArray;
		private static ImmoniumIon[] immoniumIonArray;
		private static SideChain[] sideChainArray;

		public static NeutralLossLib[] GetNeutralLossArray(){
			return neutralLossArray ?? (neutralLossArray = Read().NeutralLoss);
		}

		public static string[] GetNeutralLossFormulaArray(){
			return neutralLossArray?.Select(x => x.Formula).ToArray();
		}

		public static ImmoniumIon[] GetImmoniumArray(){
			return immoniumIonArray ?? (immoniumIonArray = Read().ImmoniumIon);
		}

		public static SideChain[] GetSideChainArray(){
			return sideChainArray ?? (sideChainArray = Read().SideChain);
		}
	}
}