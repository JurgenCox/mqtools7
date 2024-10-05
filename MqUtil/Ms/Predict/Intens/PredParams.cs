using MqUtil.Ms.Analyzer;
using MqUtil.Ms.Fragment;
namespace MqUtil.Ms.Predict.Intens {
    public class PredParams {
        public PeptideInstance Peptide { get; set; }
        public FragmentationType FragType { get; set; }
        public int Charge { get; set; }
        public MsmsMassAnalyzer MassAnalyzer { get; set; }

        public string print() {
            return "Peptide: " + Peptide.Sequence + ", Fragmentation: " + FragType.Name+ ", Charge: " + Charge + ", MassAnalyzer: " + MassAnalyzer.Name;
        }
    }
}
