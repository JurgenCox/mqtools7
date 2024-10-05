using MqUtil.Ms.Annot;
namespace MqUtil.Ms.Predict.Intens
{
    public interface IPredModel {
        Dictionary<PeakAnnotation, double> GetPrediction(PredParams param);
    }
}