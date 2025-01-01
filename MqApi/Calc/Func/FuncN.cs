using MqApi.Calc.Util;
namespace MqApi.Calc.Func{
	internal abstract class FuncN : GenericFunc{
		internal abstract double NumEvaluateDouble(double[] x);
		internal abstract ReturnType GetValueType(ReturnType[] types);
	}
}