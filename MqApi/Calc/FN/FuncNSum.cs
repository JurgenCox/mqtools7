﻿using MqApi.Calc.Func;
using MqApi.Calc.Util;
using MqApi.Num;
using MqApi.Util;
namespace MqApi.Calc.FN{
	internal class FuncNSum : FuncN{
		internal override double NumEvaluateDouble(double[] x){
			return ArrayUtils.Sum(x);
		}
		internal override ReturnType GetValueType(ReturnType[] types){
			foreach (ReturnType type in types){
				if (type == ReturnType.Real){
					return ReturnType.Real;
				}
			}
			return ReturnType.Integer;
		}
		internal override TreeNode Derivative(int index, TreeNode[] args){
			List<TreeNode> result = new List<TreeNode>();
			foreach (TreeNode node in args){
				if (node.DependsOnRealVar(index)){
					result.Add(node.Derivative(index));
				}
			}
			if (result.Count == 0){
				return zero;
			}
			if (result.Count == 1){
				return result[0];
			}
			return result.Count == 2 ? Sum(result[0], result[1]) : Sum(result.ToArray());
		}
		internal override TreeNode NthDerivative(int realVar, int intVar, TreeNode[] args){
			throw new NotImplementedException();
		}
		internal override TreeNode IndefiniteIntegral(int index, TreeNode[] args){
			throw new NotImplementedException();
		}
		internal override string ShortName => "sum";
		internal override string Name => "Sum";
		internal override string Description => "";
		internal override DocumentType DescriptionType => DocumentType.PlainText;
		internal override Topic Topic => Topic.Unknown;
	}
}