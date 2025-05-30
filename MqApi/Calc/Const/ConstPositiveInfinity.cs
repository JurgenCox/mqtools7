﻿using MqApi.Calc.Func;
using MqApi.Calc.Util;
using MqApi.Util;
namespace MqApi.Calc.Const{
	internal class ConstPositiveInfinity : Constant{
		internal override double NumEvaluateDouble => double.PositiveInfinity;
		internal override string ShortName => "+inf";
		internal override ReturnType ReturnType => ReturnType.Real;
		internal override string Name => "";
		internal override string Description => "";
		internal override DocumentType DescriptionType => DocumentType.PlainText;
		internal override Topic Topic => Topic.Unknown;
	}
}