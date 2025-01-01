using System.Numerics;
using MqApi.Calc.Func;
using MqApi.Calc.Util;
using MqApi.Util;
namespace MqApi.Calc.Const{
	internal class ConstInteger : Constant{
		private readonly BigInteger number;
		internal ConstInteger(string text){
			number = BigInteger.Parse(text);
		}
		internal override string ShortName => "constInt";
		internal override double NumEvaluateDouble => Parser.Double(number.ToString());
		internal override ReturnType ReturnType => ReturnType.Integer;
		internal override string Name => "";
		internal override string Description => "";
		internal override DocumentType DescriptionType => DocumentType.PlainText;
		internal override Topic Topic => Topic.Unknown;
		internal override string Encode(){
			return ShortName + sep + number.ToString();
		}
	}
}