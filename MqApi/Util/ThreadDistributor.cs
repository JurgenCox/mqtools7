namespace MqApi.Util{
	public interface IThreadDistributor{
		void Start();
		void Abort();
	}
	public class ThreadDistributor : IThreadDistributor{
		protected readonly int nThreads;
		protected readonly int nTasks;
		protected Thread[] allWorkThreads;
		private CancellationTokenSource[] sources;
		protected Stack<int> toBeProcessed;
		private readonly Action<int, int> calculation;
		private readonly object locker = new object();
        public Action<double> ReportProgress { get; set; }
        public Action<string> Comment { get; set; }
		public int DelayMs{ get; set; }
		private int tasksDone;
		public ThreadDistributor(int nThreads, int nTasks, Action<int> calculation) : this(nThreads, nTasks,
			(itask, ithread) => calculation(itask)){
		}
		public ThreadDistributor(int nThreads, int nTasks, Action<int, int> calculation){
			this.nThreads = Math.Min(nThreads, nTasks);
			this.nTasks = nTasks;
			this.calculation = calculation;
		}
		public void Abort(){
			if (allWorkThreads != null){
				for (int i = 0; i < allWorkThreads.Length; i++){
					if (sources[i] != null){
						sources[i].Cancel();
					}
				}
			}
		}
		public void Start(){
			toBeProcessed = new Stack<int>();
			for (int index = nTasks - 1; index >= 0; index--){
				toBeProcessed.Push(index);
			}
			allWorkThreads = new Thread[nThreads];
			sources = new CancellationTokenSource[nThreads];
			for (int i = 0; i < nThreads; i++){
				allWorkThreads[i] = new Thread(Work);
				sources[i] = new CancellationTokenSource();
                allWorkThreads[i].Start(i);
				if (DelayMs > 0){
					Thread.Sleep(DelayMs);
				}
			}
			for (int i = 0; i < nThreads; i++){
				allWorkThreads[i].Join();
			}
		}
		private void Work(object ithread){
			ReportProgress?.Invoke(0);
			while (true){
				int x;
				lock (locker){
					if (toBeProcessed.Count == 0){
						break;
					}
					x = toBeProcessed.Pop();
				}
				int it = (int) ithread;
				if (sources != null){
					CancellationTokenSource source = sources[it];
					if (source != null){
						if (source.Token.IsCancellationRequested){
							return;
						}
					}
				}
                try {
                    calculation(x, it);
                }catch (Exception e){
					Comment?.Invoke(e.Message + "\n" + e.StackTrace);
                    foreach (CancellationTokenSource source in sources) {
                        source?.Cancel();
                    }
                    throw e;
                }
                lock (locker){
					tasksDone++;
					ReportProgress?.Invoke(tasksDone / (double) nTasks);
				}
			}
		}
	}
}