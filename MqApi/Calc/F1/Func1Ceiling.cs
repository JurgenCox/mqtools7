﻿using MqApi.Calc.Except;
using MqApi.Calc.Func;
using MqApi.Calc.Util;
using MqApi.Util;
namespace MqApi.Calc.F1{
	internal class Func1Ceiling : Func1{
		internal override double NumEvaluateDouble(double x){
			return Math.Ceiling(x);
		}
		internal override Tuple<double, double> NumEvaluateComplexDouble(double re, double im){
			return null;
		}
		internal override bool HasComplexArgument => false;
		internal override ReturnType GetValueType(ReturnType returnType){
			return ReturnType.Integer;
		}
		internal override TreeNode RealPart(TreeNode re, TreeNode im){
			throw new CannotCalculateRealPartException();
		}
		internal override TreeNode ImaginaryPart(TreeNode re, TreeNode im){
			throw new CannotCalculateImaginaryPartException();
		}
		internal override TreeNode OuterDerivative(TreeNode arg){
			return DiracComb(arg);
		}
		internal override TreeNode IndefiniteIntegral(TreeNode x){
			throw new CannotCalculateIndefiniteIntegralException();
		}
		internal override TreeNode OuterNthDerivative(TreeNode x, TreeNode n){
			throw new CannotCalculateNthDerivativeException();
		}
		internal override TreeNode DomainMin => negInfinity;
		internal override TreeNode DomainMax => posInfinity;
		internal override string ShortName => "ceil";
		internal override string Name => "";
		internal override string Description => "";
		internal override DocumentType DescriptionType => DocumentType.PlainText;
		internal override Topic Topic => Topic.Unknown;
	}
}