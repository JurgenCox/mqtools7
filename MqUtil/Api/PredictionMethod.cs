using MqApi.Param;

namespace MqUtil.Api{
	public abstract class PredictionMethod{
		public abstract string Name{ get; }
		public abstract string Description{ get; }
		public abstract float DisplayRank{ get; }
		public abstract bool IsActive{ get; }

		/// <summary>
		/// Gets the <code>Parameters</code> object which is to be filled with the user-defined values.
		/// </summary>
		public abstract Parameters Parameters{ get; }

		public static string aas = "ARNDCEQGHILKMFPSTWYV";
	}
}