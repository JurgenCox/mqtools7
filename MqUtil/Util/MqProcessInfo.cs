using System.Diagnostics;
using System.Text.RegularExpressions;
using MqApi.Util;
namespace MqUtil.Util{
	public class MqProcessInfo{
		public override string ToString() =>
			string.Join("\t", Parser.ToString(StartTime), StringUtils.GetTimeString(RunningTime),
				Finished ? "Done" : (Error ? "Error" : "Running"), Title, Description, ErrorMessage ?? string.Empty);
		private readonly DateTime endTime;
		public string ErrorMessage{ get; }
		public bool Finished{ get; }
		public bool Error{ get; }
		public string Title{ get; }
		public string Description{ get; }
		public string Comment{ get; }
		public DateTime StartTime{ get; }
		public string Id{ get; }
		public string UniqueIdentifier{ get; }
		public MqProcessInfo(string filepath, string commentPath){
			Id = "";
			Regex regex = new Regex("([A-Za-z0-9\\s]*)_*([0-9\\.]*).(started|finished|error).txt");
			UniqueIdentifier = Path.GetFileName(filepath);
			Match match = regex.Match(UniqueIdentifier);
			Finished = match.Groups[3].Value == "finished";
			Error = match.Groups[3].Value == "error";
			StreamReader reader = null;
			try{
				reader = new StreamReader(filepath);
				string line;
				while ((line = reader.ReadLine()) != null){
					string[] items = line.Split(new[]{"\t"}, StringSplitOptions.None);
					switch (items[0]){
						case "start":
							StartTime = DateTime.ParseExact(items[1].Trim(), FileUtils.dateFormat, null);
							break;
						case "title":
							Title = items[1].Trim();
							break;
						case "description":
							Description = items[1].Trim();
							break;
						case "end":
							endTime = DateTime.ParseExact(items[1].Trim(), FileUtils.dateFormat, null);
							break;
						case "error":
							ErrorMessage = items[1].Trim();
							break;
						case "id":
							Id = items[1].Trim();
							break;
					}
				}
			} catch (Exception){
			} finally{
				reader?.Close();
			}
			if (!string.IsNullOrEmpty(commentPath)){
				try{
					reader = new StreamReader(commentPath);
					string line = reader.ReadLine();
					if (!string.IsNullOrEmpty(line)){
						Comment = line;
					}
				} catch (Exception){
				} finally{
					reader?.Close();
				}
			}
		}
		public double RunningTime{
			get{
				TimeSpan dt = (Finished ? endTime : DateTime.Now).ToUniversalTime() - StartTime.ToUniversalTime();
				return dt.TotalMilliseconds;
			}
		}
		public static void StartLog(string infoFolder, string title, string description, DateTime starttime){
			if (string.IsNullOrEmpty(infoFolder)){
				return;
			}
			string id = "" + Process.GetCurrentProcess().Id;
			string filename = Responder.GetStatusFile(title, infoFolder) + ".started.txt";
			StreamWriter writer;
			try{
				writer = GetStreamWriter(filename);
			} catch (Exception){
				Thread.Sleep(5000);
				try{
					writer = GetStreamWriter(filename);
				} catch (Exception){
					return;
				}
			}
			try{
				if (writer != null){
					writer.WriteLine("id\t" + id);
					writer.WriteLine("start\t" + starttime.ToString(FileUtils.dateFormat));
					writer.WriteLine("title\t" + title);
					writer.WriteLine("description\t" + description);
					writer.Flush();
					writer.Close();
					writer = null;
				}
			} catch (Exception){
			} finally{
				writer?.Close();
			}
		}
		public static void EndLog(string infoFolder, string title, string description, DateTime starttime,
			DateTime endtime){
			if (string.IsNullOrEmpty(infoFolder)){
				return;
			}
			const string i = "0";
			string filename = Responder.GetStatusFile(title, infoFolder) + ".finished.txt";
			StreamWriter writer;
			try{
				writer = GetStreamWriter(filename);
			} catch (Exception){
				Thread.Sleep(5000);
				try{
					writer = GetStreamWriter(filename);
				} catch (Exception){
					return;
				}
			}
			try{
				if (writer != null){
					writer.WriteLine("id\t" + i);
					writer.WriteLine("start\t" + starttime.ToString(FileUtils.dateFormat));
					writer.WriteLine("title\t" + title);
					writer.WriteLine("description\t" + description);
					writer.WriteLine("end\t" + endtime.ToString(FileUtils.dateFormat));
					writer.Flush();
					writer.Close();
					writer = null;
				}
			} catch (Exception){
			} finally{
				writer?.Close();
			}
			DeleteStartedFile(infoFolder, title);
		}
		public static void ErrorLog(string infoFolder, string title, string description, DateTime startTime,
			DateTime endTime, string error){
			if (string.IsNullOrEmpty(infoFolder)){
				return;
			}
			const string i = "0";
			string filename = Responder.GetStatusFile(title, infoFolder) + ".error.txt";
			StreamWriter writer;
			try{
				writer = GetStreamWriter(filename);
			} catch (Exception){
				Thread.Sleep(5000);
				try{
					writer = GetStreamWriter(filename);
				} catch (Exception){
					try {
						writer = GetStreamWriter(filename);
					} catch (Exception) {
						Thread.Sleep(15000);
						try {
							writer = GetStreamWriter(filename);
						} catch (Exception) {
							return;
						}
					}
				}
			}
			try{
				if (writer != null){
					writer.WriteLine("id\t" + i);
					writer.WriteLine("start\t" + startTime.ToString(FileUtils.dateFormat));
					writer.WriteLine("title\t" + title);
					writer.WriteLine("description\t" + description);
					writer.WriteLine("error\t" + description + "_" + error);
					writer.WriteLine("end\t" + endTime.ToString(FileUtils.dateFormat));
					writer.Flush();
					writer.Close();
					writer = null;
				}
			} catch (Exception ex){
				Exception e = new Exception("Can not write ErrorLog to File " + filename, ex);
				throw e;
			} finally{
				writer?.Close();
			}
			DeleteStartedFile(infoFolder, title);
		}
		private static StreamWriter GetStreamWriter(string filename){
			if (filename == null){
				return null;
			}
			try{
				return new StreamWriter(filename, true);
			} catch (Exception){
				return null;
			}
		}
		private static void DeleteStartedFile(string infoFolder, string name){
			try{
				string started = Responder.GetStatusFile(name, infoFolder) + ".started.txt";
				FileUtils.DeleteFile(started);
			} catch (Exception){
				Thread.Sleep(5000);
				try{
					string started = Responder.GetStatusFile(name, infoFolder) + ".started.txt";
					FileUtils.DeleteFile(started);
				} catch (Exception){
				}
			}
		}
		public static string[] GetFiles(string[] files, string suffix) {
			List<string> result = new List<string>();
			foreach (string file in files) {
				if (file.EndsWith(suffix)) {
					result.Add(file);
				}
			}
			return result.ToArray();
		}
		public static Dictionary<string, MqProcessInfo> ReadAllProcessInfo(string infoFolder, bool showAllActivities,
			bool deleteFinishedPerformanceFiles) {
			Dictionary<string, MqProcessInfo> result = new Dictionary<string, MqProcessInfo>();
			if (infoFolder == null) return result;
			try{
				string[] allFiles = Directory.GetFiles(infoFolder);
				List<string> started = new List<string>(GetFiles(allFiles, ".started.txt"));
				string[] finished = GetFiles(allFiles, ".finished.txt");
				string[] error = GetFiles(allFiles, ".error.txt");
				string[] comment = GetFiles(allFiles, ".comment.txt");
				Dictionary<string, string> commentMap = new Dictionary<string, string>();
				foreach (string c in comment){
					commentMap.Add(c.Substring(0, c.Length - 12), c);
				}
				foreach (string t in finished){
					string filenameStarted = t.Replace(".finished.", ".started.");
					if (File.Exists(filenameStarted) && started.Contains(filenameStarted)){
						started.Remove(filenameStarted);
					}
					if (deleteFinishedPerformanceFiles){
						string filenameComment = t.Replace(".finished.", ".comment.");
						try{
							if (File.Exists(filenameStarted)) {
								File.Delete(filenameStarted);
							}
							if (File.Exists(filenameComment)) {
								File.Delete(filenameComment);
							}
							if (File.Exists(t)) {
								File.Delete(t);
							}
						} catch (Exception){}
					}
				}
				foreach (string t in error){
					string filename = t.Replace(".error.", ".started.");
					if (File.Exists(filename) && started.Contains(filename)){
						started.Remove(filename);
					}
				}
				foreach (string file in started){
					string com = null;
					string s = file.Substring(0, file.Length - 12);
					if (commentMap.ContainsKey(s)){
						com = commentMap[s];
					}
					MqProcessInfo pi = new MqProcessInfo(file, com);
					result.Add(pi.UniqueIdentifier, pi);
				}
				foreach (string file in error){
					MqProcessInfo pi = new MqProcessInfo(file, null);
					result.Add(pi.UniqueIdentifier, pi);
				}
				if (showAllActivities){
					foreach (string file in finished){
						MqProcessInfo pi = new MqProcessInfo(file, null);
						result.Add(pi.UniqueIdentifier, pi);
					}
				}
				return result;
			} catch{
				return result;
			}
		}
	}
}