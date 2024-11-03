using MqApi.Num.Vector;
using MqApi.Util;
using MqUtil.Api;
using MqUtil.Num.Svm.Impl;

namespace MqUtil.Num.Svm{
	[Serializable]
	public class SvmRegressionModel : RegressionModel{
		private SvmModel model;

		public SvmRegressionModel(SvmModel model){
			this.model = model;
		}

		public SvmRegressionModel(){ }

		public override double Predict(BaseVector x){
			return SvmMain.SvmPredict(model, x);
		}

		public override void Read(string filePath){
			BinaryReader reader = FileUtils.GetBinaryReader(filePath);
			model = new SvmModel(reader);
			reader.Close();
		}

		public override void Write(string filePath){
			BinaryWriter writer = FileUtils.GetBinaryWriter(filePath);
			model.Write(writer);
			writer.Close();
		}
	}
}