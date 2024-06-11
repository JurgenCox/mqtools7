﻿using MqApi.Calc.Func;
using MqApi.Calc.Util;
using MqApi.Num;
using MqApi.Util;
namespace MqApi.Calc.FN{
	[Serializable]
	internal class FuncNMin : FuncN{
		internal override double NumEvaluateDouble(double[] x){
			return ArrayUtils.Min(x);
		}
		internal override ReturnType GetValueType(ReturnType[] types){
			foreach (ReturnType type in types){
				if (type == ReturnType.Real){
					return ReturnType.Real;
				}
			}
			return ReturnType.Integer;
		}
		internal override string ShortName => "Min";
		internal override TreeNode Derivative(int index, TreeNode[] args){
			throw new NotImplementedException();
		}
		internal override TreeNode NthDerivative(int realVar, int intVar, TreeNode[] args){
			throw new NotImplementedException();
		}
		internal override TreeNode IndefiniteIntegral(int index, TreeNode[] args){
			throw new NotImplementedException();
		}
		internal override string Name => "";
		internal override string Description => "";
		internal override DocumentType DescriptionType => DocumentType.PlainText;
		internal override Topic Topic => Topic.Unknown;
	}
}