using MqApi.Param;
using MqUtil.Api;

namespace MqUtil.Num.RegressionRank{
	public static class RegressionFeatureRankingMethods{
		private static readonly RegressionFeatureRankingMethod[] allMethods = InitRankingMethods();
		private static RegressionFeatureRankingMethod[] InitRankingMethods(){
			return new RegressionFeatureRankingMethod[]{
				new AbsCorrelationFeatureRanking(), new PositiveCorrelationFeatureRanking(), new NegativeCorrelationFeatureRanking(),
				new AbsRankCorrelationFeatureRanking(), new PositiveRankCorrelationFeatureRanking(), new NegativeRankCorrelationFeatureRanking()
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

		public static RegressionFeatureRankingMethod Get(int index) { return allMethods[index]; }

		public static RegressionFeatureRankingMethod GetByName(string name){
			foreach (RegressionFeatureRankingMethod method in allMethods.Where(method => method.Name.Equals(name))){
				return method;
			}
			throw new Exception("Unknown type: " + name);
		}
	}
}