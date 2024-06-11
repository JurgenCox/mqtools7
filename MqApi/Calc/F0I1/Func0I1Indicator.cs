using MqApi.Calc.Func;
using MqApi.Calc.Util;
namespace MqApi.Calc.F0I1{
	internal abstract class Func0I1Indicator : Func0I1{
		internal override ReturnType ReturnType => ReturnType.Integer;
		internal override Topic Topic => Topic.UtilityFunctions;
	}
}