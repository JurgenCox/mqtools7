using MqApi.Calc.Func;
using MqApi.Calc.Util;
using MqApi.Util;
namespace MqApi.Calc.Const{
	internal class ConstEuler : Constant{
		private const double value = 0.5772156649015328606065120900824024310421;
		internal override double NumEvaluateDouble => value;
		internal override string ShortName => "euler";
		internal override ReturnType ReturnType => ReturnType.Real;
		internal override string Name => "Euler Mascheroni constant (gamma)";
		internal override string Description => "";
		internal override DocumentType DescriptionType => DocumentType.PlainText;
		internal override Topic Topic => Topic.Unknown;
	}
}