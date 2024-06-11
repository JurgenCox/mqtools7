using MqApi.Util;
namespace MqApi.Generic{
	public class ProcessInfo{
		private readonly int numThreads;
		public Settings Settings{ get; }
		public Action<string> Status{ get; }
		public Action<int> Progress{ get; }
		public CancellationToken Token{ get; }
		public string ErrString{ get; set; }
		public List<ThreadDistributor> threadDistributors = new List<ThreadDistributor>();
		private readonly List<Tuple<Thread, CancellationTokenSource>> registeredThreads =
			new List<Tuple<Thread, CancellationTokenSource>>();
		public ProcessInfo(Settings settings, Action<string> status, Action<int> progress, int numThreads,
			CancellationToken token){
			Settings = settings;
			Status = status;
			Progress = progress;
			this.numThreads = numThreads;
			Token = token;
		}
		public int NumThreads => Math.Min(numThreads, Settings.Nthreads);
		public void Abort(){
			foreach (
				ThreadDistributor threadDistributor in threadDistributors.Where(threadDistributor =>
					threadDistributor != null)){
				threadDistributor.Abort();
			}
			if (registeredThreads != null){
				for (int i = 0; i < registeredThreads.Count; i++){
					Tuple<Thread, CancellationTokenSource> t = registeredThreads[i];
					if (t != null){
						t.Item2.Cancel();
					}
				}
			}
		}
		public void RegisterThread(Thread t, CancellationTokenSource source){
			registeredThreads.Add(new Tuple<Thread, CancellationTokenSource>(t, source));
		}
		public void ClearRegisteredThreads(){
			registeredThreads.Clear();
		}
	}
}