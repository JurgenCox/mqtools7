using MqUtil.Util;
namespace MqUtil.Base{
	public class ExecutionParams{
		public int Nthreads{ get; set; }
		public string[] Filenames{ get; set; }
		public string InfoFolder{ get; set; }
		public string CombinedFolder{ get; set; }
		
		public string GlobalBaseFolder{ get; set; }
		public string ParameterFile{ get; set; }
		public bool ProfilePerformance{ get; set; }

		public CalculationType CalculationType{ get; set; }
		public int Nfiles => Filenames.Length;
	}
}