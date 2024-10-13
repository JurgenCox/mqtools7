using System.Diagnostics;
using MqApi.Util;
namespace MqUtil.Util {
	public static class Utils {
		public static bool XgboostDllRunsFine() {
			ProcessStartInfo psi = new ProcessStartInfo(Path.Combine(FileUtils.executablePath, "CheckDll"));
			psi.EnvironmentVariables["PPID"] = Process.GetCurrentProcess().Id.ToString();
			psi.WorkingDirectory = FileUtils.executablePath;
			psi.WindowStyle = ProcessWindowStyle.Hidden;
			psi.CreateNoWindow = true;
			psi.UseShellExecute = false;
			psi.RedirectStandardError = true;
			psi.RedirectStandardOutput = true;
			Process externalProcess = new Process { StartInfo = psi };
			externalProcess.Start();
			externalProcess.WaitForExit();
			int exitcode = externalProcess.ExitCode;
			return exitcode == 0;
		}
		public static string GetXgboostMessage() {
			return "Visual C++ Redistributable needs to be installed. Please visit " +
			       "https://support.microsoft.com/en-us/help/2977003/the-latest-supported-visual-c-downloads " +
			       "and install vc_redist.x64.exe";
		}
	}
}
