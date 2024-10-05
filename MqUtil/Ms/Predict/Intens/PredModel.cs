using MqUtil.Ms.Annot;
namespace MqUtil.Ms.Predict.Intens
{
    public abstract class PredModel : IPredModel {
        protected byte[] Model { get; set; }
        //protected T Param { get; set; }

        protected PredModel() {
        }
        
        //protected PredModel(T param, byte[] model) {
        //    Param = param;
        //    Model = model;
        //}

        public abstract Dictionary<PeakAnnotation, double> GetPrediction(PredParams param);
    }
}
