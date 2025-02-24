using MqApi.Util;
using MqUtil.Util;
namespace MqUtil.Base{
	public class MaxQuantWorkspace{
		private List<WorkDispatcher> jobs;
		public int numThreads;
		private StreamWriter runningTimesWriter;
		private string infoFolder;
		public EventHandler workDone;

		private bool InfoFolderValid => !string.IsNullOrEmpty(infoFolder) && Directory.Exists(infoFolder);

		public void SetJobs(List<WorkDispatcher> workDispatchers){
			jobs = workDispatchers;
		}

		/// <summary>
		/// Call the Start method on each WorkDispatcher in jobs List, writing runningTimes.txt as you go.
		/// </summary>
		public int Work(string infoFolder1, CancellationTokenSource tokenSource, bool stopWithError) {
			this.infoFolder = infoFolder1;
			if (InfoFolderValid){
				runningTimesWriter = new StreamWriter(RunningTimesFile(this.infoFolder));
				runningTimesWriter.WriteLine("Job\tRunning time [min]\tStart date\tStart time\tEnd date\tEnd time");
			}
			foreach (WorkDispatcher job in jobs){
				if (tokenSource.Token.IsCancellationRequested) {
					return 1;
				}
                Console.WriteLine(job.GetMessagePrefix());
				DateTime start = DateTime.Now;
				job.Start(tokenSource.Token);
                if (runningTimesWriter != null){
					try{
						DateTime end = DateTime.Now;
						runningTimesWriter.WriteLine(job.GetMessagePrefix() + "\t" + Parser.ToString((float)(end - start).TotalMinutes)  + "\t" +
						                             $"{start:dd/MM/yyyy}" + "\t" + $"{start:HH:mm:ss}" + "\t" +
						                             $"{end:dd/MM/yyyy}" + "\t" + $"{end:HH:mm:ss}");
						runningTimesWriter.Flush();
					} catch (Exception){ }
				}
				if (stopWithError){
					string[] allFiles = Directory.GetFiles(infoFolder);
					string[] errorFiles = MqProcessInfo.GetFiles(allFiles, ".error.txt");
					if (errorFiles.Length > 0) {
						tokenSource.Cancel();
						jobs = null;
						return 2;
					}
                }
				
            }
            runningTimesWriter?.Close();
			workDone?.Invoke(this,null);
			return 0;
		}

		private const string runningTimesFileName = "#runningTimes.txt";
		public static string RunningTimesFile(string infoFolder) => Path.Combine(infoFolder, runningTimesFileName);

		public void Abort(){
			if (jobs != null){
				foreach (WorkDispatcher job in jobs.Where(job => job != null)){
					job.Abort();
				}
			}
			if (runningTimesWriter != null){
				try{
					runningTimesWriter.Dispose();
				} catch (Exception){ }
				runningTimesWriter = null;
			}
		}
	}
}