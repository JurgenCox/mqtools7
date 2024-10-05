using MqApi.Num;
using MqUtil.Mol;
using MqUtil.Ms.Analyzer;
using MqUtil.Ms.Annot;
using MqUtil.Ms.Fragment;
using MqUtil.Ms.Predict.Intens;
namespace MqUtil.Ms.Predict.Rank {
    public abstract class RankPredictionModel {
        protected IntensPredModel _ipModel;

        public abstract short[] GetRanks(FragmentationType fragType, int charge, string sequence, PeptideModificationState varMods,
            PeakAnnotation[] annotation, MsmsMassAnalyzer massAnalyzer);

        protected bool ValidInput(PredParams param) {
            if (!_ipModel.MassAnalyzerConfig().Contains(param.MassAnalyzer.Name))
                return false;
            if (!_ipModel.FragMethodConfig().Contains(param.FragType.Name))
                return false;
            if (!_ipModel.ChargeConfig().Contains(param.Charge))
                return false;
            if (_ipModel.MaxPeptideConfig() < param.Peptide.Sequence.Length)
                return false;
            if (!ValidInputSequence(param.Peptide.Sequence, _ipModel.AlphabetConfig()))
                return false;

            return true;
        }

        private bool ValidInputSequence(string sequence, string[] alphabet) {
            foreach (char aa in sequence) {
                if (!Array.Exists(alphabet, element => element == aa.ToString()))
                    return false;
            }
            return true;
        }

        protected short[] GetRank(Dictionary<PeakAnnotation, double> prediction, PeakAnnotation[] annotation) {
            Dictionary<PeakAnnotation, double> filteredPred = FilterPredictions(prediction, annotation);

            double[] intensities = new double[annotation.Length];
            int i = 0;
            foreach (PeakAnnotation anno in annotation) {
                intensities[i] = filteredPred[anno];
                i++;
            }
            return Rank(intensities);
        }

        protected Dictionary<PeakAnnotation, double> FilterPredictions(Dictionary<PeakAnnotation, double> prediction, PeakAnnotation[] annotations) {
            Dictionary<PeakAnnotation, double> filteredPred = new Dictionary<PeakAnnotation, double>();

            foreach (PeakAnnotation annotation in annotations) {
                if (prediction.ContainsKey(annotation)) {
                    filteredPred.Add(annotation, prediction[annotation]);
                }
                else {
                    filteredPred.Add(annotation, 0.0);
                }
            }

            return filteredPred;
        }

        protected static short[] Rank<T>(IList<T> data) where T : IComparable<T> {
            int n = data.Count;
            short[] rank = new short[n];
            int[] index = Order(data);
            for (short j = 0; j < n; j++) {
                rank[index[j]] = j;
            }
            return rank;
        }

        protected static int[] Order<T>(IList<T> x) where T : IComparable<T> {
            if (x == null) {
                return null;
            }
            int[] order = ArrayUtils.ConsecutiveInts(x.Count);
            const int low = 0;
            int high = order.Length - 1;
            int[] dummy = new int[order.Length];
            Array.Copy(order, dummy, order.Length);
            SortImpl(x, order, dummy, low, high);
            return order;
        }

        protected static void SortImpl<T>(IList<T> data, int[] orderDest, int[] orderSrc, int low, int high)
            where T : IComparable<T> {
            if (low >= high) {
                return;
            }
            int mid = low + ((high - low) >> 1);
            SortImpl(data, orderSrc, orderDest, low, mid);
            SortImpl(data, orderSrc, orderDest, mid + 1, high);
            if (data[orderSrc[mid]].CompareTo(data[orderSrc[mid + 1]]) > 0) {
                Array.Copy(orderSrc, low, orderDest, low, high - low + 1);
                return;
            }
            if (data[orderSrc[low]].CompareTo(data[orderSrc[high]]) <= 0) {
                int m = (high - low) % 2 == 0 ? mid : mid + 1;
                Array.Copy(orderSrc, low, orderDest, m, mid - low + 1);
                Array.Copy(orderSrc, mid + 1, orderDest, low, high - mid);
                return;
            }
            int tLow = low;
            int tHigh = mid + 1;
            for (int i = low; i <= high; i++) {
                if (tLow <= mid && (tHigh > high || data[orderSrc[tLow]].CompareTo(data[orderSrc[tHigh]]) > 0)) {
                    orderDest[i] = orderSrc[tLow++];
                }
                else {
                    orderDest[i] = orderSrc[tHigh++];
                }
            }
        }
    }
}