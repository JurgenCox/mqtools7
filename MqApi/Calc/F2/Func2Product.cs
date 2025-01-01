﻿using MqApi.Calc.Func;
using MqApi.Calc.Util;
using MqApi.Util;
namespace MqApi.Calc.F2{
	internal class Func2Product : Func2{
		internal override double NumEvaluateDouble(double x, double y){
			return x * y;
		}
		internal override ReturnType GetValueType(ReturnType returnType1, ReturnType returnType2){
			if (returnType1 == ReturnType.Integer && returnType2 == ReturnType.Integer){
				return ReturnType.Integer;
			}
			return ReturnType.Real;
		}
		internal override string ShortName => "times";
		internal override TreeNode Derivative(int index, TreeNode arg1, TreeNode arg2){
			if (arg1.DependsOnRealVar(index)){
				return arg2.DependsOnRealVar(index)
					? Sum(Product(arg1.Derivative(index), arg2), Product(arg1, arg2.Derivative(index)))
					: Product(arg1.Derivative(index), arg2);
			}
			return arg2.DependsOnRealVar(index) ? Product(arg1, arg2.Derivative(index)) : zero;
		}
		internal override TreeNode NthDerivative(int realVar, int intVar, TreeNode arg1, TreeNode arg2){
			throw new NotImplementedException();
		}
		internal override string Name => "";
		internal override string Description => "";
		internal override DocumentType DescriptionType => DocumentType.PlainText;
		internal override Topic Topic => Topic.Unknown;
	}
}