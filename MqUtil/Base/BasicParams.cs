using MqApi.Util;
using MqUtil.Util;
namespace MqUtil.Base {

	public abstract class BasicParams {
		public abstract bool CalcPeakProperties { get; }
		public abstract string SmtpHost { get; set; }
		public abstract string EmailFromAddress { get; set; }
		public abstract string EmailAddress { get; set; }
		public abstract bool SendEmail { get; set; }
		public abstract string TxtFolder { get; }
		public abstract string ProcessFolder { get; }
		public abstract string GlobalBaseFolder { get; set; }
		public abstract string CombinedFolder { get; }
		public abstract string LfqBayesFolder { get; }
		public abstract string Name { get; set; }
		public abstract int NumThreads { get; set; }
		public abstract bool[] Ptms { get; set; }
		public abstract short[] Fractions { get; set; }
		public abstract string[] Experiments { get; set; }
		public abstract string[] ReferenceChannel { get; set; }
		public abstract int[] ParamGroupIndices { get; set; }
		public abstract string[] FilePaths { get; set; }
		public abstract string MaxQuantVersion { get; set; }
		public abstract CalculationType CalculationType { get; }
		public abstract bool HasFractions { get; }
		public abstract bool HasExperiments { get; }
		public abstract int FileCount { get; }
		public abstract string[] AllExperimentValues { get; }
		public abstract int GetRawFileIndex(string rawFileName);
		public abstract string IndexFilePath(int fileIndex);
		public abstract string GetFileNameWithoutExtension(int fileId);
		public abstract HashSet<int> GetFileIndices(string exp);
		public abstract string ParameterFilename  {get;}
		public abstract CharacterEncoding Encoding { get; }
		public int GetIndexForGroup(int gind){
			for (int i = 0; i < ParamGroupIndices.Length; i++){
				if (ParamGroupIndices[i] == gind){
					return i;
				}
			}
			return -1;
		}
		public static Version ReadVersion(string filePath) {
			using (StreamReader reader = new StreamReader(filePath)) {
				string line;
				while ((line = reader.ReadLine()) != null) {
					line = StringUtils.RemoveWhitespace(line);
					try {
						if (line.StartsWith("<maxQuantVersion>")) {
							int i1 = line.IndexOf(">", StringComparison.InvariantCulture);
							int i2 = line.IndexOf("</", StringComparison.InvariantCulture);
							line = line.Substring(i1 + 1, i2 - i1 - 1);
							return new Version(line);
						}
					} catch (Exception) { }
				}
			}
			return null;
		}
	}
}