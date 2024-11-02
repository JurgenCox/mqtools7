using MqApi.Num.Vector;
using MqUtil.Api;

namespace MqUtil.Num.Learning{
	[Serializable]
	public class RegressionOnSubFeatures : RegressionModel{
		private readonly RegressionModel regressionModel;
		private readonly int[] featureInds;

		public RegressionOnSubFeatures(RegressionModel regressionModel, int[] featureInds){
			this.regressionModel = regressionModel;
			this.featureInds = featureInds;
		}

		public override double Predict(BaseVector x){
			return regressionModel.Predict(x.SubArray(featureInds));
		}

		public override void Write(string filePath){
			throw new NotImplementedException();
		}

		public override void Read(string filePath){
			throw new NotImplementedException();
		}
	}
}