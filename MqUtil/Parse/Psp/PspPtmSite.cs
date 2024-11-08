using MqUtil.Mol;

namespace MqUtil.Parse.Psp{
    public class PspPtmSite{
        public string Accession { get; set; }
        public char Residue { get; set; }
        public int Pos { get; set; }
        public string[] Domains { get; set; }
        public string SeqWindow { get; set; }
        public readonly Dictionary<PtmType, PspPtmSiteData> ptmTypes = new Dictionary<PtmType, PspPtmSiteData>();

        public PspPtmSite(string accession, char residue, int pos, string[] domains, string seqWindow){
            Accession = accession;
            Residue = residue;
            Pos = pos;
            Domains = domains;
            SeqWindow = seqWindow;
        }

        public void Add(PtmType ptmType, string siteGrpId){
            if (!ptmTypes.ContainsKey(ptmType)){
                ptmTypes.Add(ptmType, new PspPtmSiteData(siteGrpId));
            }
        }

        public void AddRegulatoryInfo(PtmType modtype, Tuple<SiteFunction, SiteFunctionQualifier>[] func,
                                      Tuple<SiteProcess, SiteFunctionQualifier>[] proc,
                                      Tuple<string, ProteinInteractionInfluence>[] protInt,
                                      Tuple<string, ProteinInteractionInfluence>[] otherInt, string notes){
            ptmTypes[modtype].AddRegulatoryInfo(func, proc, protInt, otherInt, notes);
        }

        public void AddDiseaseAssociatedInfo(PtmType modtype, string[] disease, string[] alteration, string notes){
            ptmTypes[modtype].AddDiseaseAssociatedInfo(disease, alteration, notes);
        }

        public void AddVariationInfo(PtmType modtype, string varSiteSeq, string ftid, string dbsnp, char wtAa,
                                     int mutResPos, char varAa, VariationType varType, string[] disease, string[] mimIds,
                                     VariationClass varClass){
            ptmTypes[modtype].AddVariationInfo(varSiteSeq, ftid, dbsnp, wtAa, mutResPos, varAa, varType, disease, mimIds,
                                               varClass);
        }

        public void AddWriterEraserInfo(PtmType ptmType, string siteGrpId, string kinaseName, string kinaseAcc,
                                        bool inVivo, bool inVitro){
            if (!ptmTypes.ContainsKey(ptmType)){
                ptmTypes.Add(ptmType, new PspPtmSiteData(siteGrpId));
            }
            ptmTypes[ptmType].AddWriterInfo(kinaseName, kinaseAcc, inVivo, inVitro);
        }
    }
}