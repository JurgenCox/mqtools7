﻿using MqApi.Calc.Func;
using MqApi.Calc.Util;
using MqApi.Num;
using MqApi.Util;
namespace MqApi.Calc.F0I2{
	internal class Func0I2BinomCoeff : Func0I2{
		internal override string ShortName => "bico";
		internal override string Name => "binomial coefficient";
		internal override string Description => "";
		internal override DocumentType DescriptionType => DocumentType.PlainText;
		internal override Topic Topic => Topic.Combinatorics;
		internal override double NumEvaluateDouble(long n1, long n2){
			return Factorial.BinomialCoeff(n1, n2);
		}
		internal override ReturnType ReturnType => ReturnType.Integer;
	}
}