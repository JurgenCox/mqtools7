using MqApi.Util;
namespace MqUtil.Util{
	public class ThreadDistributorTriangularMatrix{
		private readonly ThreadDistributor td;

		public ThreadDistributorTriangularMatrix(int nThreads, int n, Action<int, int> calculation){
			int nTasks = n*(n - 1)/2;
			nThreads = Math.Min(nThreads, nTasks);
			td = new ThreadDistributor(nThreads, nTasks, i =>{
				GetIndices(i, out var j, out var k);
				calculation(j, k);
			});
		}

		public Action<string> Comment {
			get {
				return td?.Comment;
			}
			set {
				td.Comment = value;
			}
		}

		private static void GetIndices(int i, out int j, out int k){
			j = (int) (0.5 + Math.Sqrt(0.25 + 2*i) + 1e-6);
			k = i - j*(j - 1)/2;
		}

		public void Abort(){
			td.Abort();
		}

		public void Start(){
			td.Start();
		}
	}
}