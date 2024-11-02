﻿using MqApi.Num.Vector;
using MqUtil.Api;

namespace MqUtil.Num.Learning{
	[Serializable]
	public class GroupWiseClassifier : ClassificationModel{
		//This will not work for classifiers that are not serializable.
		private readonly ClassificationModel[] classifiers;

		public GroupWiseClassifier(ClassificationModel[] classifiers){
			this.classifiers = classifiers;
		}

		public override double[] PredictStrength(BaseVector x) {
			double[] result = new double[classifiers.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = classifiers[i].PredictStrength(x)[0];
			}
			return result;
		}

		public override void Write(string filePath){
			throw new NotImplementedException();
		}

		public override void Read(string filePath){
			throw new NotImplementedException();
		}
	}
}