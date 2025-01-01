﻿using MqApi.Num.Vector;
using MqUtil.Api;

namespace MqUtil.Num.Learning{
	public class ClassificationOnSubFeatures : ClassificationModel{
		private readonly ClassificationModel classifier;
		private readonly int[] featureInds;

		public ClassificationOnSubFeatures(ClassificationModel classifier, int[] featureInds){
			this.classifier = classifier;
			this.featureInds = featureInds;
		}

		public override double[] PredictStrength(BaseVector x){
			return classifier.PredictStrength(x.SubArray(featureInds));
		}

		public override void Write(string filePath){
			throw new NotImplementedException();
		}

		public override void Read(string filePath){
			throw new NotImplementedException();
		}
	}
}