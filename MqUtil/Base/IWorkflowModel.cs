using MqApi.Param;
using MqUtil.Ms.Enums;
using MqUtil.Ms.Graph;
using MqUtil.Util;
namespace MqUtil.Base{
	public interface IWorkflowModel{
		List<WorkDispatcher> GetAllJobs(int nThreads, string mqparFile);
		/// <summary>
		/// Set field mqpar to the values read from the file named in the argument and return true.
		/// If the file cannot be read, make no change in mqpar and return false.
		/// </summary>
		bool ReadParameterFile(string mqparFile, out Dictionary<int, Parameters> gpar);
		void WriteParameterFile(string mqparFile);
		Parameters GetGlobalParameters();
		Parameters GetGroupParameters();
		BasicParams BasicParams { get; }
		void InitParameters();
		void UpdateParameters(Parameters param);
		void SetParametersInGui(Parameters param);
		BasicGroupParams GetGroupParams(Parameters parameters);
		void SetGroupParams(BasicGroupParams[] groupParams);
		void SetGroupParamsInParameters(int groupIndex, Parameters parameters);
		string ValidateParams(out bool fatal);
		/// <summary>
		/// True for all implementations except WorkflowModelRna (false) and WorkflowModelI (never assigned).
		/// </summary>
		bool HasFractions { get; }
		bool HasPtm { get; }
		bool HasCommonChannel { get; }
		bool HasViewer { get; }
		bool HasConfig { get; }
		bool HasPeptideMasses { get; }
		bool HasSmallMoleculeMasses { get; }
		/// <summary>
		/// Returns a Filter for OpenFileDialog. Used only by MaxQuantMainWindow.OpenFiles.
		/// </summary>
		/// <returns>A list of names and suffixes of available RawFile types, in a format suitable
		/// for use as a Filter in OpenFileDialog.</returns>
		string GetFileDialogFilter();
		string GetNameExtension();
		/// <summary>
		/// Examines the paths in a list to see if each has the form required by one of the
		/// RawFile templates, and if so, whether that RawFile type is fully installed.
		/// A message describing the first problem found is returned. If no problem is found,
		/// then null is returned.
		/// </summary>
		/// <param name="paths"></param>
		/// <returns></returns>
		string AreSuitableFiles(IEnumerable<string> paths);
		MsDataType DataType { get; }
		Version Version { get; }
		HeatMapData CreateHeatMapData();
	}
}