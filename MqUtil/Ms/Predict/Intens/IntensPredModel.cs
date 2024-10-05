namespace MqUtil.Ms.Predict.Intens{
	public abstract class IntensPredModel : PredModel //<PredParams>
	{
		//protected string[] _AAs;
		protected string[] _massAnalyzer;
		protected string[] _fragMethod;
		protected int[] _charge;
		protected int _maxPeptideLength;
		protected string[] _alphabet;
		protected double[,] _identityMatrix;

		//protected IntensPredModel(PredParams param, byte[] model) : base(param, model) { }
		protected IntensPredModel(){
		}
		public string[] MassAnalyzerConfig(){
			return _massAnalyzer;
		}
		public string[] FragMethodConfig(){
			return _fragMethod;
		}
		public int[] ChargeConfig(){
			return _charge;
		}
		public int MaxPeptideConfig(){
			return _maxPeptideLength;
		}
		public string[] AlphabetConfig(){
			return _alphabet;
		}
		protected double[,] FillIdentityMatrix(int m){
			double[,] identityMatrix = new double[m, m];
			for (int i = 0; i < m; i++){
				for (int j = 0; j < m; j++){
					if (i == j){
						identityMatrix[i, j] = 1.0;
					} else{
						identityMatrix[i, j] = 0.0;
					}
				}
			}
			return identityMatrix;
		}
		protected double[] OneHotEncoding(string value, string[] alphabet){
			double[] featureVector = new double[_identityMatrix.GetLength(1)];
			for (int j = 0; j < _identityMatrix.GetLength(1); j++){
				featureVector[j] = _identityMatrix[(Array.IndexOf(alphabet, value)), j];
			}
			return featureVector;
		}
	}
}