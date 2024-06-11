﻿using MqApi.Calc.Func;
using MqApi.Calc.Util;
using MqApi.Util;
namespace MqApi.Calc.F2{
	[Serializable]
	internal class Func2Ratio : Func2{
		internal override double NumEvaluateDouble(double x, double y){
			return x / y;
		}
		internal override ReturnType GetValueType(ReturnType returnType1, ReturnType returnType2){
			return ReturnType.Real;
		}
		internal override string ShortName => "ratio";
		internal override TreeNode Derivative(int index, TreeNode arg1, TreeNode arg2){
			if (arg1.DependsOnRealVar(index)){
				return arg2.DependsOnRealVar(index)
					? Ratio(Difference(Product(arg1.Derivative(index), arg2), Product(arg1, arg2.Derivative(index))),
						Square(arg2))
					: Ratio(arg1.Derivative(index), arg2);
			}
			return arg2.DependsOnRealVar(index)
				? Ratio(Minus(Product(arg1, arg2.Derivative(index))), Square(arg2))
				: zero;
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