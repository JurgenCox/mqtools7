using MqApi.Param;
using MqUtil.Api;
using MqUtil.Num.Svm;

namespace MqUtil.Num.ClassificationRank{
	public static class ClassificationFeatureRankingMethods{
		private static readonly ClassificationFeatureRankingMethod[] allMethods = InitRankingMethods();

		private static ClassificationFeatureRankingMethod[] InitRankingMethods(){
			return new ClassificationFeatureRankingMethod[]{
				new AnovaFeatureRanking(), new LinearSvmFeatureRanking(), new LinearSvmRfeFeatureRanking(), new GolubFeatureRanking()
			};
		}

		public static string[] GetAllNames(){
			string[] result = new string[allMethods.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = allMethods[i].Name;
			}
			return result;
		}

		public static Parameters[] GetAllSubParameters(IGroupDataProvider data){
			Parameters[] result = new Parameters[allMethods.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = allMethods[i].GetParameters(data);
			}
			return result;
		}

		public static ClassificationFeatureRankingMethod Get(int index){
			return allMethods[index];
		}

		public static ClassificationFeatureRankingMethod GetByName(string name){
			foreach (ClassificationFeatureRankingMethod method in allMethods.Where(method => method.Name.Equals(name))){
				return method;
			}
			throw new Exception("Unknown type: " + name);
		}
	}
}