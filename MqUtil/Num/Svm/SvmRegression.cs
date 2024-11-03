﻿using MqApi.Num.Vector;
using MqApi.Param;
using MqUtil.Api;
using MqUtil.Num.Kernel;
using MqUtil.Num.Svm.Impl;
using MqUtil.Util;

namespace MqUtil.Num.Svm{
	public class SvmRegression : RegressionMethod{
		public override RegressionModel Train(BaseVector[] x, int[] nominal, double[] y, Parameters param, int nthreads,
			Responder responder){
			x = ClassificationMethod.ToOneHotEncoding(x, nominal);
			ParameterWithSubParams<int> kernelParam = param.GetParamWithSubParams<int>("Kernel");
			SvmParameter sp = new SvmParameter{
				kernelFunction = KernelFunctions.GetKernelFunction(kernelParam.Value, kernelParam.GetSubParameters()),
				svmType = SvmType.EpsilonSvr,
				c = param.GetParam<double>("C").Value
			};
			SvmModel model = SvmMain.SvmTrain(new SvmProblem(x, y), sp);
			return new SvmRegressionModel(model);
		}

		public override Parameters Parameters =>
			new Parameters(KernelFunctions.GetKernelParameters(),
				new DoubleParam("C", 100){Help = SvmClassification.cHelp});

		public override string Name => "Support vector machine";
		public override string Description => "";
		public override float DisplayRank => 0;
		public override bool IsActive => true;
	}
}