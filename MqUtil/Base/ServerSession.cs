using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using MqApi.Num;
using MqApi.Util;
using MqUtil.Ms.Graph;
using MqUtil.Ms.Raw;
using MqUtil.Table;
using MqUtil.Util;
namespace MqUtil.Base{
	public class ServerSession{
		public const int updateSpeedMs = 1000;
		private readonly string title;
		private readonly DataTable2 processTable;
		public readonly DataTable2 samplesTable;
		public readonly DataTable2 fileTable;
		private readonly MaxQuantWorkspace workspace;
		public HeatMapData HeatMapData{ get; internal set; }
        public CancellationTokenSource cancelThreadSource;
        private CancellationTokenSource workThreadSource;
		private CancellationTokenSource updateThreadSource;
		private Thread workThread;
		private Thread updateThread;
		private string infoFolder;
		public string ExperimentColumn { get; set; }
		public string FractionsColumn { get; set; }
		public string ChannelColumn { get; set; }
		public string ChannelIndexColumn { get; set; }
		public string SampleNameColumn { get; set; }
		public string RawFilesColumn { get; set; }
        public bool IsDone{ get; set; }
		public int FinishCode{ get; set; }
		public string Message{ get; set; }
		public ServerSession(IWorkflowModel wmodel){
			workspace = new MaxQuantWorkspace();
			string extension = wmodel.GetNameExtension();
			if (!string.IsNullOrEmpty(extension)){
				title = Loc.MaxQuant + " - " + extension;
			} else{
				title = Loc.MaxQuant;
			}
			processTable = CreateProcessTable();
			samplesTable = CreateSamplesTable();
			fileTable = CreateFileTable(wmodel.HasFractions, wmodel.HasPtm, wmodel.HasCommonChannel);
			HeatMapData = wmodel.CreateHeatMapData();
			ExperimentColumn = "Experiment";
			FractionsColumn = "Fractions";
            ChannelColumn = "Channel";
            ChannelIndexColumn = "Channel index";
			SampleNameColumn = "Sample name";
			RawFilesColumn = "Raw files";
        }
		public DataTable2 GetProcessTable(){
			return processTable;
		}
		public DataTable2 GetSamplesTable(){
			return samplesTable;
		}
		public DataTable2 GetFileTable(){
			return fileTable;
		}
		public string GetTitle(){
			return title;
		}
		/// <summary>
		/// Reads the "File" column from the fileTableView property.
		/// </summary>
		/// <returns>The file names listed.</returns>
		public string[] GetFilenames(){
			int colInd = fileTable.GetColumnIndex("File");
			string[] names = new string[fileTable.RowCount];
			for (int i = 0; i < names.Length; i++){
				names[i] = (string) fileTable.GetEntry(i, colInd);
			}
			return names;
		}
		/// <summary>
		/// True iff all files in the TableModel are marked as existing.
		/// Simply negates the method HasNonexistentFiles. Used only by SelectTab.
		/// </summary>
		/// <returns></returns>
		public bool CheckIfFilesExist(){
			return !HasNonexistentFiles();
		}
		public string[] GetFilenames(out string combinedFolder){
			string[] names = new string[fileTable.RowCount];
			for (int i = 0; i < names.Length; i++){
				names[i] = (string) fileTable.GetRow(i)["File"];
			}
			string name = names[0];
			combinedFolder = name.Substring(0,
				                 name.LastIndexOf("" + Path.DirectorySeparatorChar,
					                 StringComparison.InvariantCulture)) +
			                 Path.DirectorySeparatorChar + "combined";
			return names;
		}
		public string[] GetSampleTableColumn(int col){
			string[] vals = new string[samplesTable.RowCount];
			for (int i = 0; i < vals.Length; i++){
				vals[i] = samplesTable.GetEntry(i, col).ToString() ?? "";
			}
			return vals;
		}
        public void NoFractions(bool hasFractions){
			int fileCol = fileTable.GetColumnIndex("File");
			int expCol = fileTable.GetColumnIndex("Experiment");
			int fracCol = fileTable.GetColumnIndex("Fraction");
			string[] files = new string[fileTable.RowCount];
			for (int i = 0; i < fileTable.RowCount; i++){
				string path = (string) fileTable.GetEntry(i, fileCol);
				files[i] = Path.GetFileName(path);
			}
			files = StringUtils.RemoveCommonSubstrings(files, true);
			for (int i = 0; i < fileTable.RowCount; i++){
				fileTable.SetEntry(i, expCol, files[i]);
				if (hasFractions){
					fileTable.SetEntry(i, fracCol, null);
				}
			}
		}
		public string RunAnalysis(int numThreads, int[] jobInds, IWorkflowModel wmodel, string fileName,
			string password, CancellationTokenSource tokenSource, bool deleteFinishedPerformanceFiles, 
            CancellationTokenSource cancelSourcePerformance) {
			string x = StartAnalysis(wmodel);
			if (x != null){
				return x;
			}
			infoFolder = wmodel.BasicParams.ProcessFolder;
			if (Directory.Exists(infoFolder)){
				try{
					FileUtils.Rmdir(infoFolder);
				} catch (Exception){
					return $"Cannot delete folder {infoFolder}. Please make sure no other processes are accessing it.";
				}
			}
			if (!FileUtils.IsUnix()){
				try {
					MqUtil.Util.Utils.XgboostDllRunsFine();
				}
				catch {
					return MqUtil.Util.Utils.GetXgboostMessage();


				}
			}
			if (!string.IsNullOrEmpty(infoFolder)){
				Directory.CreateDirectory(infoFolder);
			}
			wmodel.WriteParameterFile(wmodel.BasicParams.ParameterFilename);
			if (!Directory.Exists(wmodel.BasicParams.CombinedFolder)){
				Directory.CreateDirectory(wmodel.BasicParams.CombinedFolder);
			}
			List<WorkDispatcher> jobs = wmodel.GetAllJobs(numThreads, wmodel.BasicParams.ParameterFilename);
			if (jobInds != null){
				jobs = jobs.SubList(jobInds);
			}
			workThreadSource = tokenSource;
			workThread = new Thread(() => DoWork(jobs, numThreads, wmodel, fileName, password, 
				workThreadSource, deleteFinishedPerformanceFiles, cancelSourcePerformance, true, infoFolder));
			workThread.Start();
			return null;
		}
		public string ReadFromFile(string filename, bool hasFractions){
			try{
				bool hasName = TabSep.HasColumn("Name", filename, '\t');
				if (!hasName){
					return "A 'Name' column is required in the experimental design file.";
				}
			} catch (IOException){
				return "Cannot open file " + filename + ". Is it open in another program, e.g. Excel?";
			}
			bool hasCommonChannel = TabSep.HasColumn("Reference channels", filename, '\t');
			bool hasFraction = TabSep.HasColumn("Fraction", filename, '\t') && hasFractions;
			bool hasExperiment = TabSep.HasColumn("Experiment", filename, '\t');
			bool hasPtm = TabSep.HasColumn("PTM", filename, '\t');
			string[] names = TabSep.GetColumn("Name", filename, '\t');
			string[] experiments = null;
			string[] commonChannels = null;
			if (hasExperiment){
				experiments = TabSep.GetColumn("Experiment", filename, '\t');
				for (int i = 0; i < experiments.Length; i++){
					experiments[i] = experiments[i].Trim();
				}
			}
			if (hasCommonChannel){
				string[] cc = TabSep.GetColumn("Reference channels", filename, '\t');
				commonChannels = new string[cc.Length];
				for (int i = 0; i < commonChannels.Length; i++){
					commonChannels[i] = cc[i];
				}
			}
			short[] fractions = null;
			if (hasFraction){
				string[] f = TabSep.GetColumn("Fraction", filename, '\t');
				fractions = new short[f.Length];
				for (int i = 0; i < f.Length; i++){
					bool success = short.TryParse(f[i], out fractions[i]);
					if (!success){
						fractions[i] = short.MaxValue;
					}
				}
			}
			bool[] ptm = null;
			if (hasPtm){
				string[] f = TabSep.GetColumn("PTM", filename, '\t');
				ptm = new bool[f.Length];
				for (int i = 0; i < f.Length; i++){
					ptm[i] = ParsePtm(f[i]);
				}
			}
			Dictionary<string, int> nameToRow = CreateNameToRowMap();
			for (int i = 0; i < names.Length; i++){
				if (!nameToRow.ContainsKey(names[i])){
					continue;
				}
				int row = nameToRow[names[i]];
				DataRow2 dr = fileTable.GetRow(row);
				if (hasExperiment){
					dr["Experiment"] = experiments[i];
				}
				if (hasFraction){
					if (fractions[i] == short.MaxValue){
						dr["Fraction"] = null;
					} else{
						dr["Fraction"] = fractions[i];
					}
				}
				if (hasPtm){
					dr["PTM"] = ptm[i];
				}
				if (hasCommonChannel){
					dr["Reference channels"] = commonChannels[i];
				}
			}
			return null;
		}
		public static int[] ParseChannels(string s){
			if (string.IsNullOrWhiteSpace(s)){
				return new int[0];
			}
			s = s.Trim();
			if (s.Contains(",")){
				string[] x = s.Split(',');
				return ParseChannels(x);
			}
			if (s.Contains(";")){
				string[] x = s.Split(';');
				return ParseChannels(x);
			}
			return ParseChannels(new[]{s});
		}
		private static int[] ParseChannels(string[] s){
			int[] result = new int[s.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = int.Parse(s[i]) - 1;
			}
			return result;
		}
		private static bool ParsePtm(string s){
			if (string.IsNullOrEmpty(s)){
				return false;
			}
			if (s.Equals("+")){
				return true;
			}
			bool success = bool.TryParse(s, out bool ptm);
			return success && ptm;
		}
		public void ChangeFolder(int[] sel, string path, IWorkflowModel wmodel){
			foreach (int i in sel){
				string name = (string) fileTable.GetRow(i)["File"];
				string newFile = Path.Combine(path, Path.GetFileName(name));
				DataRow2 row = fileTable.GetRow(i);
				row["File"] = newFile;
				bool exists = File.Exists(newFile) || Directory.Exists(newFile);
				row["Exists"] = exists;
				row["Size"] = File.Exists(newFile) ? StringUtils.GetFileSizeString(newFile) : "";
				row["Data format"] = exists ? RawFileUtil.FindSuitableTemplate(newFile).Item1.Name : "";
			}
			ChangeFolderInMqpar(sel, path, wmodel);
		}
		public void ChangeFolderInMqpar(int[] sel, string path, IWorkflowModel wmodel){
			string[] filePaths = wmodel.BasicParams.FilePaths;
			if (filePaths.Length == 0) return;
			foreach (int i in sel){
				filePaths[i] = Path.Combine(path, Path.GetFileName(filePaths[i]));
			}
			string mqparFile = Path.Combine(wmodel.BasicParams.GlobalBaseFolder, "mqpar.xml");
			wmodel.WriteParameterFile(mqparFile);
		}
		public void StopIt(bool deleteFinishedPerformanceFiles) {
			workThreadSource?.Cancel();
			workspace?.Abort();
			StopPerformanceTable();
			KillProcesses(deleteFinishedPerformanceFiles);
		}
		private string StartAnalysis(IWorkflowModel wmodel){
			//string err = FileUtils.BasicChecks();
			//if (!string.IsNullOrEmpty(err)){
			//	return err;
			//}
			if (!HasFiles){
				return "Please specify some raw files.";
			}
			string errStr = wmodel.AreSuitableFiles(GetFilenames());
			if (!string.IsNullOrEmpty(errStr)){
				//return errStr;
			}
			// If the previous test is working properly, then this test should never be true.
			if (HasNonexistentFiles()){
				//return "Some of the specified raw files do not exist.";
			}
			string[] x = CheckForDuplicatedFilenames();
			if (x != null){
				return "Raw file name appears twice: " + x[1] + " " + x[2];
			}
			string folder = BaseFolder;
			try{
				Random random = new Random((int) DateTime.Now.Ticks);
				string foldername = Path.Combine(folder, "test_" + random.Next());
				while (File.Exists(foldername)){
					foldername = Path.Combine(folder, "test_" + random.Next());
				}
				Directory.CreateDirectory(foldername);
				Thread.Sleep(100);
				FileUtils.Rmdir(foldername);
			} catch (Exception){
				return $"Cannot write into folder '{folder}' presumably due to lack of permission.";
			}
			return null;
		}
		private void KillProcesses(bool deleteFinishedPerformanceFiles) {
			try{
				Dictionary<string, MqProcessInfo> procs =
					MqProcessInfo.ReadAllProcessInfo(infoFolder, false, deleteFinishedPerformanceFiles);
				foreach (MqProcessInfo pi in procs.Values){
					if (!pi.Finished){
						try{
							Process p = Process.GetProcessById(int.Parse(pi.Id));
							p.Kill();
						} catch (Exception){
						}
					}
				}
			} catch (Exception){
			}
		}
		private Dictionary<string, int> CreateNameToRowMap(){
			Dictionary<string, int> nameToRow = new Dictionary<string, int>();
			for (int i = 0; i < fileTable.RowCount; i++){
				DataRow2 dr = fileTable.GetRow(i);
				string name = (string) dr["File"];
				nameToRow.Add(Path.GetFileNameWithoutExtension(name), i);
				string n2 = Path.GetFileName(name);
				if (!nameToRow.ContainsKey(n2)){
					nameToRow.Add(n2, i);
				}
			}
			return nameToRow;
		}
		public Dictionary<string, Tuple<List<string>, List<string>>> CreateExperimentToFractionAndFileMap(){
			Dictionary<string, Tuple<List<string>, List<string>>> experimentToFractionAndRowMap 
				= new Dictionary<string, Tuple<List<string>, List<string>>>();
            for (int i = 0; i < fileTable.RowCount; i++){
				DataRow2 dr = fileTable.GetRow(i);
				string experiment = (string)dr["Experiment"];
				string fraction = dr["Fraction"].ToString();
				if (fraction == null){
					fraction = short.MaxValue.ToString();
	            }
				string rawFile = (string)dr["File"];
                if (!experimentToFractionAndRowMap.ContainsKey(experiment)){
	                Tuple<List<string>, List<string>> value = new Tuple<List<string>, List<string>>(
		                new List<string> { fraction },
		                new List<string> { rawFile }
	                );

					experimentToFractionAndRowMap.Add(experiment, value);
				} else{
	                Tuple<List<string>, List<string>> value = experimentToFractionAndRowMap[experiment];
					value.Item1.Add(fraction);
					value.Item2.Add(rawFile);
					experimentToFractionAndRowMap[experiment] = value;

                }
			}
			return experimentToFractionAndRowMap;
		}
		internal bool ShowAllActivities{ get; set; }
		private void UpdateStep(bool deleteFinishedPerformanceFiles) {
			if (infoFolder == null){
				return;
			}
			if (!Directory.Exists(infoFolder)){
				return;
			}
			try{
				Dictionary<string, MqProcessInfo> procs =
					MqProcessInfo.ReadAllProcessInfo(infoFolder, ShowAllActivities,
						deleteFinishedPerformanceFiles);
				processTable.Clear();
				string[] pids = processTable.GetStringValuesInColumn(0);
				for (int i = 0; i < pids.Length; i++){
					if (procs.ContainsKey(pids[i])){
						DataRow2 row1 = processTable.GetRow(i);
						UpdateRow(row1, procs[pids[i]]);
						procs.Remove(pids[i]);
					}
				}
				foreach (MqProcessInfo pi in procs.Values){
					DataRow2 row = processTable.NewRow();
					bool valid = UpdateRow(row, pi);
					if (valid){
						processTable.AddRow(row);
					}
				}
				try{
					//client.InvalidateProcessTable();
				} catch (Exception){
				}
			} catch (Exception){
				return;
			}
			try{
				//client.InvalidateProcessTable();
			} catch (Exception){
				// do not throw an exception if invalidate failed.
			}
		}
		private void UpdateLoop(bool deleteFinishedPerformanceFiles) {
			while (!cancelThreadSource.Token.IsCancellationRequested)
            {
				if (updateThreadSource != null && updateThreadSource.Token.IsCancellationRequested){
					return;
				}
				Thread.Sleep(updateSpeedMs);
				UpdateStep(deleteFinishedPerformanceFiles);
			}
			// ReSharper disable FunctionNeverReturns
		}
		// ReSharper restore FunctionNeverReturns
		private void StartPerformanceTable(bool deleteFinishedPerformanceFiles, CancellationTokenSource cancelSourcePerformance) {
			StopPerformanceTable();
            cancelThreadSource = cancelSourcePerformance;
            updateThreadSource = new CancellationTokenSource();
            updateThread = new Thread(() => UpdateLoop(deleteFinishedPerformanceFiles));
			updateThread.Start();
		}
		public void DoWork(List<WorkDispatcher> jobs, int numThreads, IWorkflowModel wmodel, string fileName,
			string password, CancellationTokenSource tokenSource, bool deleteFinishedPerformanceFiles, CancellationTokenSource cancelSourcePerformance,
			bool stopWithError, string infoFolder) {
			StartPerformanceTable(deleteFinishedPerformanceFiles, cancelSourcePerformance);
			try{
				workspace.SetJobs(jobs);
				workspace.numThreads = numThreads;
				string err = wmodel.ValidateParams(out bool fatal);
				if (err != null){
					Message = err;
					if (fatal){
						workspace.Abort();
						return;
					}
				}
				FinishCode = workspace.Work(infoFolder, tokenSource, stopWithError);
				if (FinishCode != 0){
					cancelThreadSource.Cancel();
                    return;
				}
				if (wmodel.BasicParams.SendEmail && ValidEmail(wmodel.BasicParams.EmailAddress) &&
				    ValidEmail(wmodel.BasicParams.EmailFromAddress)){
					string summFile = Path.Combine(wmodel.BasicParams.TxtFolder, "summary.txt");
					if (!File.Exists(summFile)){
						summFile = null;
					}
					NetworkingUtils.SendEmail(wmodel.BasicParams.EmailAddress,
						"MaxQuant job " + fileName + " is finished.",
						"Dear MaxQuant user,\n\nThank you for running MaxQuant. Your MaxQuant run " + fileName +
						" finished successfully. Attached is the summary file. Have a nice day.", summFile,
						wmodel.BasicParams.SmtpHost, wmodel.BasicParams.EmailFromAddress, password);
				}
				IsDone = true;
				cancelThreadSource.Cancel();
            } catch (ThreadAbortException){
			}
		}
		/// <returns>If row is valid.</returns>
		private static bool UpdateRow(DataRow2 row, MqProcessInfo pinfo){
			row["Start time"] = Parser.ToString(pinfo.StartTime);
			row["Running time"] = StringUtils.GetTimeString(pinfo.RunningTime);
			row["Status"] = pinfo.Finished ? "Done" : (pinfo.Error ? "Error" : "Running");
			row["Title"] = pinfo.Title;
			row["Description"] = pinfo.Description;
			row["Comment"] = pinfo.Comment;
			row["PID"] = pinfo.Id;
			if (pinfo.ErrorMessage != null){
				row["Description"] = pinfo.ErrorMessage;
			}
			return !string.IsNullOrEmpty(pinfo.Title);
		}
		private void StopPerformanceTable(){
			if (updateThreadSource != null){
				updateThreadSource.Cancel();
				//updateThreadSource = null;
			}
		}
		private string BaseFolder{
			get{
				if (fileTable.RowCount > 0){
					string x = (string) fileTable.GetRow(0)["File"];
					return x.Substring(0,
						x.LastIndexOf("" + Path.DirectorySeparatorChar, StringComparison.InvariantCulture));
				}
				return null;
			}
		}
		/// <summary>
		/// Compares the file names (not the full paths!) in the TableModel with each other.
		/// Returns null if they are all unique. As soon as two file names are found to be
		/// identical, that name and both full paths are returned. Used only by StartButton_OnClick.
		/// </summary>
		/// <returns></returns>
		private string[] CheckForDuplicatedFilenames(){
			string[] paths = GetFilenames();
			Dictionary<string, string> map = new Dictionary<string, string>();
			foreach (string path in paths){
				string name = Path.GetFileName(path);
				if (map.ContainsKey(name)){
					return new[]{name, map[name], path};
				}
				map.Add(name, path);
			}
			return null;
		}
		/// <summary>
		/// Checks whether any of the files listed have been marked as not existing (by the AddRawFile method).
		/// This can occur, for example, if a MaxQuant session is stored in an mqpar.xml file, one of the files 
		/// is deleted or moved to another location, and then a new session is started from that mqpar.xml file.
		/// Used by StartButton_OnClick and CheckIfFilesExist (which is only used by SelectTab).
		/// </summary>
		/// <returns></returns>
		private bool HasNonexistentFiles(){
			int colInd = fileTable.GetColumnIndex("Exists");
			for (int i = 0; i < fileTable.RowCount; i++){
				bool exists = (bool) fileTable.GetEntry(i, colInd);
				if (!exists){
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// Returns true if the table model lists at least one file.
		/// </summary>
		private bool HasFiles => fileTable.RowCount > 0;
		public bool HasParams => HeatMapData.HasParams;
		public BasicParams BasicParams => HeatMapData.BasicParams;
		public void ClearRawFileTable(){
			List<DataRow2> rows = new List<DataRow2>();
			for (int rowid = 0; rowid < fileTable.RowCount; rowid++){
				rows.Add(fileTable.GetRow(rowid));
			}
			foreach (DataRow2 row in rows){
				fileTable.RemoveRow(row);
			}
		}
		/// <summary>
		/// Add a raw data file name and related information to the TableModel. Used by:
		///    private bool AddRawFiles(string[] fileNames, string exp)
		///    private void LoadParameters(string filename)
		/// This is the end of the line, that is, the method that actually adds a row to the TableModel
		/// for a single raw data path (unless a row with that name already exists). "File", 
		/// "Parameter group", "Experiment", and "Fraction" (if applicable) are taken directly from 
		/// the arguments. "Exists" and "Size" are the results of queries to the file system.
		/// "Data format" is determined from a call to RawFileUtil.FindSuitableTemplate(path).
		/// </summary>
		/// <param name="path">Path to the raw data</param>
		/// <param name="group"></param>
		/// <param name="exp">Normally empty, but when folders are added recursively, the experiment name is
		/// derived from the folder hierarchy.</param>
		/// <param name="frac">Only used if the WorkflowModel implementation has fractions (as most do).</param>
		/// <param name="hasFractions"></param>
		/// <param name="ptm"></param>
		/// <param name="hasPtm"></param>
		/// <param name="commonChannel"></param>
		/// <param name="hasCommonChannel"></param>
		public void AddRawFile(string path, int group, string exp, short frac, bool hasFractions, bool ptm, bool hasPtm,
			string commonChannel, bool hasCommonChannel){
			for (int i = 0; i < fileTable.RowCount; ++i){
				if (fileTable.GetRow(i)["File"].Equals(path)){
					return;
				}
			}
			DataRow2 row = fileTable.NewRow();
			row["File"] = path;
			bool exists = File.Exists(path) || Directory.Exists(path);
			row["Exists"] = exists;
			row["Size"] = File.Exists(path) ? StringUtils.GetFileSizeString(path) : "";
			Tuple<RawFile, int> x = RawFileUtil.FindSuitableTemplate(path);
			row["Data format"] = exists ? x.Item1.Name : "";
			row["Parameter group"] = "Group " + group;
			if (hasFractions){
				if (frac == short.MaxValue){
					row["Fraction"] = null;
				} else{
					row["Fraction"] = frac;
				}
			}
			if (hasPtm){
				row["PTM"] = ptm;
			}
			if (hasCommonChannel){
				row["Reference channels"] = commonChannel;
			}
			row["Experiment"] = exp;
			fileTable.AddRow(row);
		}
		private static bool ValidEmail(string emailAddress){
			return emailAddress.Contains("@") && emailAddress.Contains(".");
		}
		private static DataTable2 CreateProcessTable(){
			DataTable2 pt = new DataTable2("Processes", "");
			pt.AddColumn("Running time", 60, ColumnType.Text, "Total running time of the process.");
			pt.AddColumn("Status", 60, ColumnType.Text, "The status of the process (running/finished/error)");
			pt.AddColumn("Title", 230, ColumnType.Text, "The name of the process.");
			pt.AddColumn("Description", 210, ColumnType.Text, "Description of the input for the process.");
			pt.AddColumn("Comment", 180, ColumnType.Text, "Comments that are sent from the running process.");
			pt.AddColumn("PID", 50, ColumnType.Text, "Id of the corresponding system process.");
			pt.AddColumn("Start time", 200, ColumnType.Text, "Time at which job started.");
			return pt;
		}
		private static DataTable2 CreateSamplesTable(){
			DataTable2 pt = new DataTable2("Samples", "");
			pt.AddColumn("Experiment", 90, ColumnType.Text,
				"Experiment names as specified in the 'Raw files' table.");
			pt.AddColumn("factor value[group]", 180, ColumnType.Text, "");
            pt.AddColumn("Fractions", 180, ColumnType.Text, "Fractions corresponding to the experiment");
			pt.AddColumn("Channel", 180, ColumnType.Text, "Channel name from labeling.");
            pt.AddColumn("Sample name", 180, ColumnType.Text, "User-specifiable name of the sample.");
            pt.AddColumn("Organism part", 90, ColumnType.Text, 
	            "The part of organism’s anatomy or substance arising from an organism from which the biomaterial was derived, (e.g., liver)");
            pt.AddColumn("Cell type", 90, ColumnType.Text,
	            "A cell type is a distinct morphological or functional form of cell. Examples are epithelial, glial etc.");
            pt.AddColumn("Disease", 90, ColumnType.Text, "The disease under study in the Sample.");
            pt.AddColumn("Biological Replicate", 180, ColumnType.Text, "Parallel measurements of biologically distinct samples");
            pt.AddColumn("Technical Replicate", 180, ColumnType.Text, "Repeated measurements of the same sample");
            return pt;
		}
        private static DataTable2 CreateFileTable(bool hasFractions, bool hasPtm, bool hasCommonChannel){
			DataTable2 ft = new DataTable2("File table",
				"Table containing all raw data sources that are being processed together.");
			ft.AddColumn("File", 355, ColumnType.Text, "Path to the raw data source, either a file or a folder.");
			ft.AddColumn("Exists", 50, ColumnType.Boolean,
				"The path as it is stored in the mqpar.xml file might not be correct, e.g. after moving a complete " +
				"project to another folder or disk. Here is indicated which of the specified " +
				"paths do not point to an existing file or folder.");
			ft.AddColumn("Size", 68, ColumnType.Text, "If the raw data is file-based, the size of the file.");
			ft.AddColumn("Data format", 80, ColumnType.Text, "The vendor-specific format of this raw data source.");
			ft.AddColumn("Parameter group", 110, ColumnType.Text,
				"The parameter group this raw file is associated with. 'Parameter group' is a concept which allows you to " +
				"specify different parameter settings to different subsets of raw files. For each parameter group you " +
				"use in this table there will appear a new parameter group page in the 'Group-specific parameters' " +
				"section. To assign a parameter group to selected raw files in the table use the \"Set parameter group\" button.");
			ft.AddColumn("Experiment", 100, ColumnType.Text,
				"The experiment name is initially empty unless raw data sources were entered recursively, in which case the " +
				"initial experiment is constructed from the path of the folder containing the raw data source relative to the " +
				"top level folder. To assign an experiment to selected raw files in the table use the \"Set experiment\" button.");
			if (hasFractions){
				ft.AddColumn("Fraction", 60, ColumnType.Integer,
					"To assign a fraction to selected raw files in the table use the \"Set fractions\" button.");
			}
			if (hasPtm){
				ft.AddColumn("PTM", 50, ColumnType.Boolean,
					"This column indicates whether the sample was subjected to an enrichment for a PTM. " +
					"In that case, also unmodified peptides from this run will not be used for protein quantification.");
			}
			if (hasCommonChannel){
				ft.AddColumn("Reference channels", 130, ColumnType.Text,
					"Channels in reporter-based quantification that carry a common sample used for normalization." +
					" To assign a common channel to selected raw files in the table use the \"Set reference channels\" button.");
			}
			return ft;
		}
		public string InitCombinedData(IList<string> fileNames){
			return HeatMapData.InitCombinedData(fileNames);
		}
		public void ClearTables(){
			HeatMapData.ClearTables();
		}
		public object[] GetShotgunMatchedFeaturesTableRow(long row){
			return ((IShotgunHeatMapData) HeatMapData).GetMatchedFeaturesTableRow(row);
		}
		public object[] GetShotgunPasefMsmsScansTableRow(long row){
			return ((IShotgunHeatMapData) HeatMapData).GetPasefMsmsScansTableRow(row);
		}
		public object[] GetShotgunAccumulatedMsmsScansTableRow(long row){
			return ((IShotgunHeatMapData) HeatMapData).GetAccumulatedMsmsScansTableRow(row);
		}
		public long GetShotgunMatchedFeaturesTableCount(){
			return ((IShotgunHeatMapData) HeatMapData).GetMatchedFeaturesTableCount();
		}
		public long GetShotgunPasefMsmsScansTableCount(){
			return ((IShotgunHeatMapData) HeatMapData).GetPasefMsmsScansTableCount();
		}
		public long GetShotgunAccumulatedMsmsScansTableCount(){
			return ((IShotgunHeatMapData) HeatMapData).GetAccumulatedMsmsScansTableCount();
		}
		public void SetShowAllActivities(bool showAllActivities){
			ShowAllActivities = showAllActivities;
		}
		public string[] GetExperimentColumn(){
			string[] result = new string[fileTable.RowCount];
			for (int i = 0; i < result.Length; i++){
				result[i] = (string) fileTable.GetEntry(i, 5);
			}
			return result;
		}
		public int[] GetParameterGroupColumn(){
			int[] result = new int[fileTable.RowCount];
			for (int i = 0; i < result.Length; i++){
				string s = (string) fileTable.GetEntry(i, 4);
				result[i] = Parser.Int(s.Substring(6));
			}
			return result;
		}
	}
}