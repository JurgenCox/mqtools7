using MqApi.Calc.Except;
using MqApi.Calc.Func;
using MqApi.Calc.Util;
using MqApi.Util;
namespace MqApi.Calc.F1{
	[Serializable]
	internal class Func1Minus : Func1{
		internal override double NumEvaluateDouble(double x){
			return -x;
		}
		internal override Tuple<double, double> NumEvaluateComplexDouble(double re, double im){
			return new Tuple<double, double>(-re, -im);
		}
		internal override bool HasComplexArgument => true;
		internal override ReturnType GetValueType(ReturnType returnType){
			return returnType;
		}
		internal override TreeNode RealPart(TreeNode re, TreeNode im){
			return Minus(re);
		}
		internal override TreeNode ImaginaryPart(TreeNode re, TreeNode im){
			return Minus(im);
		}
		internal override TreeNode OuterDerivative(TreeNode arg){
			return minusOne;
		}
		internal override TreeNode IndefiniteIntegral(TreeNode x){
			throw new CannotCalculateIndefiniteIntegralException();
		}
		internal override TreeNode OuterNthDerivative(TreeNode x, TreeNode n){
			throw new CannotCalculateNthDerivativeException();
		}
		internal override TreeNode DomainMin => negInfinity;
		internal override TreeNode DomainMax => posInfinity;
		internal override string ShortName => "minus";
		internal override string Name => "";
		internal override string Description => "";
		internal override DocumentType DescriptionType => DocumentType.PlainText;
		internal override Topic Topic => Topic.Unknown;
	}
}