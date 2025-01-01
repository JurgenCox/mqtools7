using MqApi.Calc.Func;
using MqApi.Calc.Util;
using MqApi.Util;
namespace MqApi.Calc.Const{
	internal class ConstE : Constant{
		internal override double NumEvaluateDouble => Math.E;
		internal override string ShortName => "e";
		internal override ReturnType ReturnType => ReturnType.Real;
		internal override string Name => "euler number";
		internal override string Description => "";
		internal override DocumentType DescriptionType => DocumentType.PlainText;
		internal override Topic Topic => Topic.Unknown;
	}
}