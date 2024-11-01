using MqApi.Num.Vector;
using MqApi.Param;

namespace MqUtil.Api{
	public interface IKernelFunction : ICloneable {
		bool UsesSquares{ get; }
		Parameters Parameters{ get; set; }
		double Evaluate(BaseVector xi, BaseVector xj, double xSquarei, double xSquarej);

		void Write(BinaryWriter writer);
		void Read(BinaryReader reader);
		KernelType GetKernelType();
		/// <summary>
		/// This is the name that e.g. appears in drop-down menus.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// The context help that will appear in tool tips etc. 
		/// </summary>
		string Description { get; }

	}
}