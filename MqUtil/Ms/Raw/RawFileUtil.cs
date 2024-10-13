namespace MqUtil.Ms.Raw {
	public static class RawFileUtil {
		/// <summary>
		/// Given a path to raw data, return the RawFile template of the appropriate type
		/// (or null, in none is found) and an error code and message describing any errors.
		/// Error codes:
		///    0 = a unique match was found to a template that is fully installed;
		///    1 = the path does not exist;
		///    2 = no match was found;
		///    3 = a unique match was found, but the template is not fully installed.
		/// An exception is thrown if more than one match is found, whether or not any of
		/// them are installed. (It could be argued that a way should be found to keep going
		/// as long as possible; on the other hand, this is a programming error that should
		/// really be fixed.)
		/// </summary>
		/// <param name="path"></param>
		/// <returns>A Tuple consisting of a RawFile template matching the path (or else null),
		/// an int error code between 0 and 3, and a corresponding error message as a string.</returns>
		public static Tuple<RawFile, int> FindSuitableTemplate(string path) {
			RawFile suitableTemplate = null;
			int errCode;
			if (!File.Exists(path) && !Directory.Exists(path)) {
				errCode = 1;
			} else {
				errCode = 2;
				foreach (RawFile template in RawFiles.RawFileTemplates) {
					if (template.IsSuitableFile(path)) {
						if (suitableTemplate != null) {
							throw new Exception("Path " + path + " exists but the raw data format " +
												"is ambiguous. This is a bug.");
						}
						suitableTemplate = template;
						errCode = template.IsInstalled ? 0 : 3;
					}
				}
			}
			return new Tuple<RawFile, int>(suitableTemplate, errCode); 
		}
		/// <summary>
		/// Given the error code returned by FindSuitableTemplate, return a text message describing the error.
		/// </summary>
		/// <param name="errCode"></param>
		/// <returns></returns>
		public static string FindSuitableTemplateMsg(int errCode) {
			switch (errCode) {
				case 0: return "Path exists and matches a unique raw data format that is fully installed.";
				case 1: return "Path does not exist, neither as a file nor as a folder.";
				case 2: return "Path exists but does not match any known raw data format.";
				case 3:
					return "Path exists and matches a unique raw data format, " +
							"but a component required to read the raw data has not been installed.";
				default: throw new Exception("Argument to FindSuitableTemplateMsg has an unexpected value: " + errCode);
			}
		}
		/// <summary>
		/// Given a path to raw data, create a RawFile object of the appropriate type, initialized to the path.
		/// Null is returned if no RawFile type is found that matches the path (which could happen).
		/// If a RawFile object has already been constructed for the path, return that one.
		/// (In case FindSuitableTemplate returns null, Item2 of the Tuple returned is an error code that
		/// describes what went wrong. That information is used to create the message of the Exception thrown.)
		/// (Art was working on creating a version of this method that takes arguments that are used
		/// to restrict the scans used to those between a minTime and a maxTime.)
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static RawFile CreateRawFile(string path) {
			Tuple<RawFile, int> result = FindSuitableTemplate(path);
			RawFile template = result.Item1;
			int errCode = result.Item2;
			switch (errCode) {
				case 0:
					RawFile rawFile = (RawFile) Activator.CreateInstance(template.GetType());
					rawFile.Init(path);
					return rawFile;
				case 1:
				case 2:
					throw new Exception("Error creating a RawFile instance from " + path + ":\n" + FindSuitableTemplateMsg(errCode));
				case 3:
					throw new Exception("Error creating a RawFile instance from " + path + ":\n" + FindSuitableTemplateMsg(errCode) +
										"\n" + template.InstallMessage);
				default: throw new Exception("Unexpected value for errCode in CreateRawFile: " + errCode);
			}
		}
		public static int GetNumVoltages(string filename){
			RawFileLayer rawFile = RawFileUtil.CreateRawFile(filename).GetLayer(true);
			return rawFile.FaimsVoltageCount;
		}
		/// <summary>
		/// Examines the paths in a list to see if each has the form required by one of the
		/// RawFile templates, and if so, whether that RawFile type is fully installed.
		/// If any problems are found a message describing the problems found is returned.
		/// If no problem is found with any paths, then the empty string is returned.
		/// </summary>
		/// <param name="filenames"></param>
		/// <returns></returns>
		public static string AreSuitableFiles(IEnumerable<string> filenames) {
			string errMessages = "";
			foreach (string filename in filenames) {
				Tuple<RawFile, int> result = FindSuitableTemplate(filename);
				RawFile template = result.Item1;
				int errCode = result.Item2;
				if (errCode != 0) {
					errMessages += "A problem was encountered with the raw data source " + filename + ":\n";
					errMessages += FindSuitableTemplateMsg(errCode);
					if (errCode == 3) {
						errMessages += template.InstallMessage;
					}
					errMessages += "\n";
				}
			}
			return errMessages;
		}
		/// <summary>
		/// A list of names and suffixes of available file-based RawFile types, in a format suitable for
		/// use as a Filter in OpenFileDialog. (Folder-based types are excluded because it is difficult
		/// to make a dialog that allows the selection of both files and folders.)
		/// </summary>
		/// <returns>A string with description / filter pairs, all separated by pipe characters.</returns>
		public static string GetFileDialogFilter() {
			string result = "";
			bool first = true;
			foreach (RawFile rf in RawFiles.RawFileTemplates) {
				if (!rf.IsFolderBased) {
					string s = (first ? "" : "|") + rf.Name + " (*" + rf.Suffix + ")|*" + rf.Suffix + ";" + "*" + rf.Suffix.ToUpper();
					result += s;
					first = false;
				}
			}
			return result;
		}
	}
}