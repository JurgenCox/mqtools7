using System.Diagnostics;
using System.Text;
using MqApi.Util;
using MqUtil.Num;
namespace MqUtil.Util{
	public abstract class WorkDispatcher{
		private const int initialDelay = 6;
		private CancellationToken token;
		private Thread[] workThreads;
		private Process[] externalProcesses;
		private Stack<int> toBeProcessed;
		public string InfoFolder{ get; }
		public bool ProfilePerformance{ get; }
		public CalculationType CalculationType{ get; }
		public int MaxHeapSizeGb{ get; set; }
		public bool Permute{ get; set; }
		public bool WriteArgs{ get; set; }
		public int Nthreads{ get; }
		public Func<int> NTasks{ get; }
		protected WorkDispatcher(int nThreads, int nTasks, string infoFolder, CalculationType calculationType,
			bool profile) : this(nThreads, () => nTasks, infoFolder,
			calculationType, profile){
		}
		protected WorkDispatcher(int nThreads, Func<int> nTasks, string infoFolder, CalculationType calculationType,
			bool profile){
			Nthreads = nThreads;
			NTasks = nTasks;
			InfoFolder = infoFolder;
			ProfilePerformance = profile;
			if (!string.IsNullOrEmpty(infoFolder) && !Directory.Exists(infoFolder)){
				Directory.CreateDirectory(infoFolder);
			}
			CalculationType = calculationType;
		}
		private int nTasksCached = -1;
		private int NTasksCached{
			get{
				if (nTasksCached == -1){
					nTasksCached = NTasks();
				}
				return nTasksCached;
			}
		}
		public void Abort(){
			if (CalculationType == CalculationType.ExternalProcess && externalProcesses != null){
				foreach (Process process in externalProcesses){
					if (process != null && IsRunning(process)){
						try{
							process.Kill();
						} catch (Exception){
						}
					}
				}
			}
		}
		public void Start(CancellationToken token) {
			this.token = token;
			toBeProcessed = new Stack<int>();
			if (Permute){
				Random2 rand = new Random2(7);
				int[] p = rand.NextPermutation(NTasksCached);
				for (int index = NTasksCached - 1; index >= 0; index--){
					toBeProcessed.Push(p[index]);
				}
			} else{
				for (int index = NTasksCached - 1; index >= 0; index--){
					toBeProcessed.Push(index);
				}
			}
			workThreads = new Thread[Nthreads];
			externalProcesses = new Process[Nthreads];
			for (int i = 0; i < Nthreads; i++){
				workThreads[i] = new Thread(Work){Name = "Thread " + i + " of " + GetType().Name};
				workThreads[i].Start(i);
				Thread.Sleep(initialDelay);
			}
			while (true){
				Thread.Sleep(1000);
				bool busy = false;
				for (int i = 0; i < Nthreads; i++){
					if (workThreads[i].IsAlive){
						busy = true;
						break;
					}
				}
				if (!busy){
					break;
				}
			}
			string sleepTime = Environment.GetEnvironmentVariable("MQ_WORK_SLEEP");
			if (sleepTime != null){
				Thread.Sleep(int.Parse(sleepTime));
			}
		}
		public string GetMessagePrefix(){
			return MessagePrefix + " ";
		}
		public abstract void Calculation(string[] args, Responder responder);
		public virtual bool IsFallbackPosition => true;
		protected virtual string GetComment(int taskIndex){
			return "";
		}
		protected virtual string Executable => "MaxQuantTask.dll";
		protected abstract object[] GetArguments(int taskIndex);
		protected abstract int Id{ get; }
		protected abstract string MessagePrefix{ get; }
		protected abstract int SoftwareId{ get; }
		private void Work(object threadIndex){
			while (toBeProcessed.Count > 0){
				if (token.IsCancellationRequested){
					return;
				}
				int x;
				lock (this){
					if (toBeProcessed.Count > 0){
						x = toBeProcessed.Pop();
					} else{
						x = -1;
					}
				}
				if (x >= 0){
					DoWork(x, (int) threadIndex);
				}
			}
		}
		private void DoWork(int taskIndex, int threadIndex){
			switch (CalculationType){
				case CalculationType.ExternalProcess:
					ProcessSingleRunExternalProcess(taskIndex, threadIndex);
					break;
				case CalculationType.Thread:
					Calculation(GetStringArgs(taskIndex), new Responder());
					break;
			}
		}
		internal IList<string> GetCommandLineArgs(int taskIndex){
			string cmd = GetCommandFilename();
			string[] logArgs = GetLogArgs(taskIndex, taskIndex);
			string[] calcArguments = GetStringArgs(taskIndex);
			List<string> result = new List<string>{cmd};
			result.AddRange(logArgs);
			result.AddRange(calcArguments);
			return result;
		}
		protected Process GetProcess(IList<string> args){
			bool isUnix = FileUtils.IsUnix();
			string cmd = "./MaxQuantTask.dll";
			//string cmd = args[0];
			string argsStr = string.Join(" ", WrapArgs(args.Skip(1)));
			argsStr = cmd + " " + argsStr;
			if (WriteArgs){
				Console.WriteLine(argsStr);
			}
			ProcessStartInfo psi = new ProcessStartInfo("dotnet", argsStr);
			if (isUnix){
				psi.WorkingDirectory = Path.GetDirectoryName(args[0]).Substring(1);
				if (MaxHeapSizeGb > 0){
					psi.EnvironmentVariables["MONO_GC_PARAMS"] = "max-heap-size=" + MaxHeapSizeGb + "g";
				}
			} else{
				psi.WorkingDirectory = FileUtils.executablePath;
			}
			psi.EnvironmentVariables["PPID"] = Process.GetCurrentProcess().Id.ToString();
			psi.WindowStyle = ProcessWindowStyle.Hidden;
			psi.CreateNoWindow = true;
			psi.UseShellExecute = false;
			psi.RedirectStandardError = true;
			psi.RedirectStandardOutput = true;
			Process externalProcess = new Process{StartInfo = psi};
			externalProcess.OutputDataReceived += (sender, eventArgs) => { Console.WriteLine(eventArgs.Data); };
			externalProcess.ErrorDataReceived += (sender, eventArgs) => { Console.Error.WriteLine(eventArgs.Data); };
			return externalProcess;
		}
		private void ProcessSingleRunExternalProcess(int taskIndex, int threadIndex){
			IList<string> args = GetCommandLineArgs(taskIndex);
			Process externalProcess = GetProcess(args);
			externalProcesses[threadIndex] = externalProcess;
			externalProcesses[threadIndex].Start();
			int processid = externalProcesses[threadIndex].Id;
			externalProcesses[threadIndex].WaitForExit();
			string stdErr = externalProcess.StandardError.ReadToEnd();
			string stdOut = externalProcess.StandardOutput.ReadToEnd();
			int exitcode = externalProcesses[threadIndex].ExitCode;
			externalProcesses[threadIndex].Close();
			if (exitcode != 0){
				//throw new Exception("Exception during execution of external process: " + processid + " " + stdErr);
			}
		}
		private string GetName(int taskIndex){
			return GetFilename() + " (" + IntString(taskIndex + 1, NTasksCached) + "/" + NTasksCached + ")";
		}
		private string[] GetLogArgs(int taskIndex, int id){
			return new[]{
				InfoFolder, GetFilename(), id.ToString(), GetName(taskIndex), GetComment(taskIndex), "Process",
				$"\"{Id}\"", $"\"{SoftwareId}\""
			};
		}
		public string GetFilename(){
			return GetMessagePrefix().Trim().Replace("/", "").Replace("(", "_").Replace(")", "_").Replace(" ", "_");
		}
		public string GetCommandFilename(){
			return "\"" + FileUtils.executablePath + Path.DirectorySeparatorChar +
			       Executable + "\"";
		}
		private string[] GetStringArgs(int taskIndex){
			object[] o = GetArguments(taskIndex);
			string[] args = new string[o.Length];
			for (int i = 0; i < o.Length; i++){
				args[i] = $"{o[i]}";
			}
			return args;
		}
		private static string WrapArg(string arg){
			if (string.IsNullOrEmpty(arg)){
				return "\"\"";
			}
			if (!arg.StartsWith("\"") && !arg.EndsWith("\"")){
				return $"\"{arg}\"";
			}
			return arg;
		}
		public static string[] WrapArgs(IEnumerable<string> args){
			return args.Select(WrapArg).ToArray();
		}
		public static bool IsRunning(Process process){
			if (process == null) return false;
			try{
				Process.GetProcessById(process.Id);
			} catch (Exception){
				return false;
			}
			return true;
		}
		public static string IntString(int x, int n){
			int npos = (int) Math.Ceiling(Math.Log10(n));
			string result = "" + x;
			if (result.Length >= npos){
				return result;
			}
			return Repeat(npos - result.Length, "0") + result;
		}
		private static string Repeat(int n, string s){
			StringBuilder b = new StringBuilder();
			for (int i = 0; i < n; i++){
				b.Append(s);
			}
			return b.ToString();
		}
	}
}