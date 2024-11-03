﻿using MqApi.Num;
using MqApi.Num.Vector;
using MqApi.Param;
using MqUtil.Api;
using MqUtil.Num.Kernel;
using MqUtil.Num.Svm.Impl;

namespace MqUtil.Num.Svm {
	public class LinearSvmFeatureRanking : ClassificationFeatureRankingMethod {
		public override Parameters GetParameters(IGroupDataProvider data) {
			return new Parameters(new Parameter[] {new DoubleParam("C", 100) {Help = SvmClassification.cHelp}});
		}

		public override string Name => "SVM";
		public override string Description => "";
		public override float DisplayRank => 0;
		public override bool IsActive => true;

		public override int[] Rank(BaseVector[] x, int[][] y, int ngroups, Parameters param, IGroupDataProvider data,
			int nthreads, Action<double> reportProgress) {
			SvmParameter sp = new SvmParameter {
				kernelFunction = new LinearKernelFunction(),
				svmType = SvmType.CSvc,
				c = param.GetParam<double>("C").Value
			};
			bool[] invert;
			SvmProblem[] problems = CreateProblems(x, y, ngroups, out invert);
			int[][] rankedSets = new int[problems.Length][];
			for (int i = 0; i < problems.Length; ++i) {
				rankedSets[i] = RankBinary(problems[i], sp);
			}
			return CombineRankedFeaturesLists(rankedSets);
		}

		private static int[] RankBinary(SvmProblem prob, SvmParameter param) {
			int nfeatures = prob.x[0].Length;
			double[] criteria = ComputeRankingCriteria(SvmMain.SvmTrain(prob, param).ComputeBinaryClassifierWeights(nfeatures));
			int[] order = ArrayUtils.Order(criteria);
			Array.Reverse(order);
			return order;
		}

		private static double[] ComputeRankingCriteria(IList<double> weights) {
			double[] rankCriteria = new double[weights.Count];
			for (int i = 0; i < weights.Count; i++) {
				rankCriteria[i] = weights[i] * weights[i];
			}
			return rankCriteria;
		}

		private static int[] CombineRankedFeaturesLists(IList<int[]> featuresLists) {
			List<int> ranked = new List<int>();
			for (int j = 0; j < featuresLists[0].Length; ++j) {
				foreach (int[] t in featuresLists) {
					if (!ranked.Contains(t[j])) {
						ranked.Add(t[j]);
					}
				}
			}
			return ranked.ToArray();
		}

		private static SvmProblem[] CreateProblems(BaseVector[] x, IList<int[]> y, int ngroups, out bool[] invert) {
			if (ngroups == 2) {
				invert = new bool[1];
				return new[] {CreateProblem(x, y, 0, out invert[0])};
			}
			SvmProblem[] result = new SvmProblem[ngroups];
			invert = new bool[ngroups];
			for (int i = 0; i < ngroups; i++) {
				result[i] = CreateProblem(x, y, i, out invert[i]);
			}
			return result;
		}

		private static SvmProblem CreateProblem(BaseVector[] x, IList<int[]> y, int index, out bool invert) {
			double[] y1 = new double[y.Count];
			for (int i = 0; i < y.Count; i++) {
				if (Array.BinarySearch(y[i], index) >= 0) {
					y1[i] = 1;
				} else {
					y1[i] = 0;
				}
			}
			invert = Array.BinarySearch(y[0], index) < 0;
			return new SvmProblem(x, y1);
		}
	}
}