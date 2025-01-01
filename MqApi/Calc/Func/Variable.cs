﻿using MqApi.Calc.Except;
using MqApi.Calc.Util;
using MqApi.Util;
namespace MqApi.Calc.Func{
	internal class Variable : GenericFunc{
		internal int id;
		internal ReturnType type;
		internal override string ShortName => "var";
		internal sealed override TreeNode Derivative(int index, TreeNode[] arguments){
			return type == ReturnType.Integer || id != index ? zero : one;
		}
		internal override TreeNode NthDerivative(int realVar, int intVar, TreeNode[] args){
			if (type == ReturnType.Integer || id != realVar){
				return zero;
			}
			throw new CannotCalculateNthDerivativeException();
		}
		internal override TreeNode IndefiniteIntegral(int index, TreeNode[] args){
			throw new NotImplementedException();
		}
		internal override string Encode(){
			return ShortName + sep + Parser.ToString(id) + sep + Parser.ToString((int) type);
		}
		internal override string Name => "var";
		internal override string Description => "";
		internal override DocumentType DescriptionType => DocumentType.PlainText;
		internal override Topic Topic => Topic.Unknown;
	}
}