using MqUtil.Mol;
using MqUtil.Ms.Enums;
using MqUtil.Ms.Utils;
namespace MqUtil.Ms.Search {
    public class CrossGroupSearchParam {
        
        public  CrossLinker CrossLinker;
        public double SignaturePeakMassTolerance;
        public int CrosslinkMaxDiUnsaturated;
        public int CrosslinkMaxDiSaturated;
        public int CrosslinkMaxMonoUnsaturated;
        public int CrosslinkMaxMonoSaturated;
        public CrossLinkingType CrossLinkingType;
        public int MinPairedPepLenXl;
        public bool CrosslinkOnlyIntraProtein;
        public bool CrosslinkIntensityBasedPrecursor;
        public CrosslinkMode Mode;
        public int TopX; // Number of top intense peaks for intensity-based signature peak detection
        public LinkPatternResult LinkPatternResult { get; set; }
        public bool IsFirstCrosslink { get; set; }
        public int NumMissingPeaks = 1; // Number of missing peaks for signature peaks detection 
        public double MinMass = double.MinValue; // Minimum mass (accurately m/z value) to start checking topX intense peaks for signature peak detection
        public bool IsIdentified1 = false;
        public bool IsIdentified2 = false;
        public bool DoesIncludeHybridPrecDetermination = false;
        

        public CrossGroupSearchParam(CrossLinker crossLinker, double signaturePeakMassTolerance, 
            int crosslinkMaxMonoUnsaturated, int crosslinkMaxMonoSaturated, 
            int crosslinkMaxDiUnsaturated, int crosslinkMaxDiSaturated, CrossLinkingType crossLinkingType, 
            int minPairedPepLenXl, bool crosslinkOnlyIntraProtein, bool crosslinkIntensityBasedPrecursor, 
            CrosslinkMode mode, int topX, bool doesIncludeHybridPrecDetermination, 
            LinkPatternResult linkPatternResult, bool isFirstCrosslink) {
            CrossLinker = crossLinker;
            SignaturePeakMassTolerance = signaturePeakMassTolerance;
            CrosslinkMaxMonoUnsaturated = crosslinkMaxMonoUnsaturated;
            CrosslinkMaxMonoSaturated = crosslinkMaxMonoSaturated;
            CrosslinkMaxDiUnsaturated = crosslinkMaxDiUnsaturated;
            CrosslinkMaxDiSaturated = crosslinkMaxDiSaturated;
            CrossLinkingType = crossLinkingType;
            MinPairedPepLenXl = minPairedPepLenXl; //TODO Walter: check if minPepLenXl is sufficient
            CrosslinkOnlyIntraProtein = crosslinkOnlyIntraProtein;
            CrosslinkIntensityBasedPrecursor = crosslinkIntensityBasedPrecursor;
            Mode = mode;
            TopX = topX;
            LinkPatternResult = linkPatternResult;
            IsFirstCrosslink = isFirstCrosslink;
            DoesIncludeHybridPrecDetermination = doesIncludeHybridPrecDetermination;
        }

    }
}