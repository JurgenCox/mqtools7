using MqApi.Util;
using MqUtil.Mol;

namespace MqUtil.Parse.Psp{
    public class PspPtmSiteData{
        public string SiteGrpId { get; set; }
        public Tuple<SiteFunction, SiteFunctionQualifier>[] Func { get; set; }
        public Tuple<SiteProcess, SiteFunctionQualifier>[] Proc { get; set; }
        public Tuple<string, ProteinInteractionInfluence>[] ProtInt { get; set; }
        public Tuple<string, ProteinInteractionInfluence>[] OtherInt { get; set; }
        public string Notes { get; set; }
        public string[] Disease { get; set; }
        public string[] Alteration { get; set; }
        public string DiseaseNotes { get; set; }
        public string VarSiteSeq { get; set; }
        public string Ftid { get; set; }
        public string Dbsnp { get; set; }
        public char WtAa { get; set; }
        public int MutResPos { get; set; }
        public char VarAa { get; set; }
        public VariationType VarType { get; set; }
        public string[] VarDisease { get; set; }
        public string[] MimIds { get; set; }
        public VariationClass VarClass { get; set; }
        public List<PspPtmEraser> erasers = new List<PspPtmEraser>();
        public List<PspPtmWriter> writers = new List<PspPtmWriter>();
        public string FuncString{
            get{
                if (Func == null){
                    return "";
                }
                string[] result = new string[Func.Length];
                for (int i = 0; i < result.Length; i++){
                    result[i] = "" + Func[i].Item1 + "," + Func[i].Item2;
                }
                return StringUtils.Concat(";", result);
            }
        }
        public string ProcString{
            get{
                if (Proc == null){
                    return "";
                }
                string[] result = new string[Proc.Length];
                for (int i = 0; i < result.Length; i++){
                    result[i] = "" + Proc[i].Item1 + "," + Proc[i].Item2;
                }
                return StringUtils.Concat(";", result);
            }
        }
        public string ProtIntString{
            get{
                if (ProtInt == null){
                    return "";
                }
                string[] result = new string[ProtInt.Length];
                for (int i = 0; i < result.Length; i++){
                    result[i] = "" + ProtInt[i].Item1 + "," + ProtInt[i].Item2;
                }
                return StringUtils.Concat(";", result);
            }
        }
        public string OtherIntString{
            get{
                if (OtherInt == null){
                    return "";
                }
                string[] result = new string[OtherInt.Length];
                for (int i = 0; i < result.Length; i++){
                    result[i] = "" + OtherInt[i].Item1 + "," + OtherInt[i].Item2;
                }
                return StringUtils.Concat(";", result);
            }
        }
        public string WritersString{
            get{
                string[] result = new string[writers.Count];
                for (int i = 0; i < result.Length; i++){
                    result[i] = writers[i].ToString();
                }
                return StringUtils.Concat(";", result);
            }
        }
        public string ErasersString{
            get{
                string[] result = new string[erasers.Count];
                for (int i = 0; i < result.Length; i++){
                    result[i] = erasers[i].ToString();
                }
                return StringUtils.Concat(";", result);
            }
        }

        public PspPtmSiteData(string siteGrpId){
            SiteGrpId = siteGrpId;
        }

        public void AddRegulatoryInfo(Tuple<SiteFunction, SiteFunctionQualifier>[] func,
                                      Tuple<SiteProcess, SiteFunctionQualifier>[] proc,
                                      Tuple<string, ProteinInteractionInfluence>[] protInt,
                                      Tuple<string, ProteinInteractionInfluence>[] otherInt, string notes){
            Func = func;
            Proc = proc;
            ProtInt = protInt;
            OtherInt = otherInt;
            Notes = notes;
        }

        public void AddDiseaseAssociatedInfo(string[] disease, string[] alteration, string diseaseNotes){
            Disease = disease;
            Alteration = alteration;
            DiseaseNotes = diseaseNotes;
        }

        public void AddVariationInfo(string varSiteSeq, string ftid, string dbsnp, char wtAa, int mutResPos, char varAa,
                                     VariationType varType, string[] varDisease, string[] mimIds,
                                     VariationClass varClass){
            VarSiteSeq = varSiteSeq;
            Ftid = ftid;
            Dbsnp = dbsnp;
            WtAa = wtAa;
            MutResPos = mutResPos;
            VarAa = varAa;
            VarType = varType;
            VarDisease = varDisease;
            MimIds = mimIds;
            VarClass = varClass;
        }

        public void AddWriterInfo(string kinaseName, string kinaseAcc, bool inVivo, bool inVitro){
            writers.Add(new PspPtmWriter(kinaseAcc, kinaseName, inVivo, inVitro));
        }
    }
}