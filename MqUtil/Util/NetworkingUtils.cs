using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
namespace MqUtil.Util{
	public static class NetworkingUtils{
		public static string[] ReadHtmlPage(string uriString){
			WebRequest webReq = WebRequest.Create(uriString);
			WebResponse webRes = webReq.GetResponse();
			Stream stream = webRes.GetResponseStream();
			Encoding encode = Encoding.GetEncoding("utf-8");
			StreamReader reader = new StreamReader(stream, encode);
			List<string> result = new List<string>();
			string line;
			while ((line = reader.ReadLine()) != null){
				result.Add(line);
			}
			reader.Close();
			webRes.Close();
			return result.ToArray();
		}

		public static string[] GetFileListHtml(string uriString, string ftpUserId, string ftpPassword){
			List<string> result = new List<string>();
			FtpWebRequest reqFtp = (FtpWebRequest) WebRequest.Create(new Uri(uriString));
			reqFtp.UseBinary = false;
			reqFtp.Credentials = new NetworkCredential(ftpUserId, ftpPassword);
			reqFtp.Method = WebRequestMethods.Ftp.ListDirectory;
			WebResponse response;
			try{
				response = reqFtp.GetResponse();
			} catch (WebException){
				return new string[0];
			}
			StreamReader reader = new StreamReader(response.GetResponseStream());
			string line;
			while ((line = reader.ReadLine()) != null){
				result.Add(line);
			}
			reader.Close();
			response.Close();
			return result.ToArray();
		}

		public static string[] GetFileListHttp(string uriString){
			return ExtractFilenamesFromRealHtml(ReadHtmlPage(uriString));
		}

		public static string[] GetFileListFtp(string uriString, string ftpUserId, string ftpPassword){
			return ExtractFilenamesFromHtml(GetFileListHtml(uriString, ftpUserId, ftpPassword));
		}

		public static string[] GetFolderList(string uriString, string ftpUserId, string ftpPassword){
			return ExtractDirNamesFromHtml(GetFileListHtml(uriString, ftpUserId, ftpPassword));
		}

		public static void Download(string uriString, string destination, string fileName, string ftpUserId,
			string ftpPassword){
			try{
				if (File.Exists(destination + Path.DirectorySeparatorChar + fileName)){
					return;
				}
				FileStream outputStream =
					new FileStream(destination + Path.DirectorySeparatorChar + fileName, FileMode.Create);
				FtpWebRequest reqFtp = (FtpWebRequest) WebRequest.Create(new Uri(uriString + "/" + fileName));
				reqFtp.Method = WebRequestMethods.Ftp.DownloadFile;
				reqFtp.UseBinary = true;
				reqFtp.Credentials = new NetworkCredential(ftpUserId, ftpPassword);
				FtpWebResponse response = (FtpWebResponse) reqFtp.GetResponse();
				Stream ftpStream = response.GetResponseStream();
				const int bufferSize = 2048;
				int readCount;
				byte[] buffer = new byte[bufferSize];
				while ((readCount = ftpStream.Read(buffer, 0, bufferSize)) > 0){
					outputStream.Write(buffer, 0, readCount);
				}
				ftpStream.Close();
				outputStream.Close();
				response.Close();
			} catch (Exception){
				//MessageBox.Show(ex.Message);
			}
		}

		private static string[] ExtractFilenamesFromHtml(IEnumerable<string> list){
			List<string> result = new List<string>();
			foreach (string line in list){
				if (line.Contains("ALT=\"[FILE]\"")){
					string s = line.Substring(line.IndexOf("\"", StringComparison.Ordinal) + 1);
					s = s.Substring(0, s.IndexOf("\"", StringComparison.InvariantCulture));
					result.Add(s);
				}
			}
			return result.ToArray();
		}

		public static string[] ExtractFilenamesFromRealHtml(IEnumerable<string> list){
			List<string> result = new List<string>();
			bool infiles = false;
			foreach (string line in list){
				if (line.Contains("Parent Directory")){
					infiles = true;
					continue;
				}
				if (line.Contains("<tr><th colspan=\"5\"><hr></th></tr>")){
					infiles = false;
				}
				if (!infiles){
					continue;
				}
				int index = line.IndexOf("<a href=\"", StringComparison.InvariantCulture);
				string l = line.Substring(index + 9);
				l = l.Substring(0, l.IndexOf("\"", StringComparison.InvariantCulture));
				result.Add(l);
			}
			return result.ToArray();
		}

		private static string[] ExtractDirNamesFromHtml(IEnumerable<string> list){
			List<string> result = new List<string>();
			foreach (string line in list){
				if (line.Contains("ALT=\"[DIR] \"")){
					string s = line.Substring(line.IndexOf("\"", StringComparison.InvariantCulture) + 1);
					s = s.Substring(0, s.IndexOf("\"", StringComparison.InvariantCulture) - 1);
					result.Add(s);
				}
			}
			return result.ToArray();
		}

		public static void DownloadEmbl(string destination){
			const string username = "anonymous";
			const string passwd = "cox@biochem.mpg.de";
			const string uriString = "ftp://ftp.ebi.ac.uk/pub/databases/embl/release";
			string[] fileList = GetFileListFtp(uriString, username, passwd);
			foreach (string filename in fileList){
				Download(uriString, destination, filename, username, passwd);
			}
		}

		public static void DownloadInParanoid(string destination){
			const string uriString = "http://inparanoid.sbc.su.se/download/current/tables_stats";
			string[] fileList = GetFileListHttp(uriString);
			WebClient client = new WebClient();
			foreach (string filename in fileList){
				client.DownloadFile(uriString + "/" + filename, destination + Path.DirectorySeparatorChar + filename);
			}
		}

		public static void DownloadPdbList(string listFile, string destination){
			StreamReader reader = new StreamReader(listFile);
			string line;
			while ((line = reader.ReadLine()) != null){
				string pdb = line.Substring(0, 4).ToLower();
				Console.WriteLine(pdb);
				DownloadOnePdbIfNonexistent(pdb, destination);
			}
		}

		public static void DownloadOnePdbIfNonexistent(string pdbName, string destination){
			if (File.Exists(destination + Path.DirectorySeparatorChar + "pdb" + pdbName.ToLower() + ".ent.gz")){
				return;
			}
			if (File.Exists(destination + Path.DirectorySeparatorChar + "pdb" + pdbName.ToLower() + ".ent")){
				return;
			}
			DownloadOnePdb(pdbName, destination);
		}

		public static void DownloadOnePdb(string pdbName, string destination){
			const string username = "anonymous";
			const string passwd = "cox@biochem.mpg.de";
			const string uriString = "ftp://ftp.wwpdb.org/pub/pdb/data/structures/all/pdb";
			string filename = "pdb" + pdbName.ToLower() + ".ent.gz";
			Download(uriString, destination, filename, username, passwd);
		}

		public static bool TryOpenUrl(string url){
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)){
				Process.Start(new ProcessStartInfo(url){UseShellExecute = true});
			} else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)){
				Process.Start("xdg-open", url);
			} else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)){
				Process.Start("open", url);
			} else{
				return false;
			}
			return true;
		}

		internal static string GetWindowsPath(string fileName){
			string path = null;
			for (int i = 0; i < 3; i++){
				try{
					switch (i){
						case 0:
							path = Environment.GetEnvironmentVariable("SystemRoot");
							break;
						case 1:
							path = Environment.GetEnvironmentVariable("windir");
							break;
						case 2:{
							string sysdir = Environment.GetFolderPath(Environment.SpecialFolder.System);
							path = Directory.GetParent(sysdir).FullName;
						}
							break;
					}
					if (path != null){
						path = Path.Combine(path, fileName);
						if (File.Exists(path)){
							return path;
						}
					}
				} catch{ }
			}
			// not found
			return null;
		}

		public static void SendEmail(string address, string subject, string body, string attachFile, string smtpHost,
			string emailFrom, string password){
			SmtpClient client = new SmtpClient{
				Port = 587,
				Host = smtpHost,
				EnableSsl = true,
				Timeout = 10000,
				DeliveryMethod = SmtpDeliveryMethod.Network,
				UseDefaultCredentials = false,
				Credentials = new NetworkCredential(emailFrom, password)
			};
			MailMessage message = new MailMessage();
			message.To.Add(address);
			message.Subject = subject;
			message.From = new MailAddress(emailFrom);
			message.Body = body;
			if (!string.IsNullOrEmpty(attachFile)){
				message.Attachments.Add(new Attachment(attachFile));
			}
			try{
				client.Send(message);
			} catch (Exception){ }
		}
	}
}