using MqApi.Util;
using MqUtil.Ms.Instrument;
using MqUtil.Ms.Utils;
namespace MqUtil.Ms.Raw{
	/// <summary>
	/// Contains data extracted from a raw data file. Implementations are vender dependent. Essentially 
	/// contains two RawFileLayer instances, one for positive ions and one for negative ions.
	/// </summary>
	public abstract class RawFile : IDisposable{
		/// <summary>
		/// Counter incremented when format used for index files is changed, to avoid using stale index files.
		/// </summary>
		private const int indexVersion = 31;
		/// <summary>
		/// Backing field to the Path property.
		/// </summary>
		private string path;
		/// <summary>
		/// Ensures that method PreInit is only called once.
		/// </summary>
		protected bool preInitialized; // initialized to default value, which is false
		/// <summary>
		/// The RawFileLayer for positive ions.
		/// </summary>
		private RawFileLayer posLayer;
		/// <summary>
		/// The RawFileLayer for negative ions.
		/// </summary>
		private RawFileLayer negLayer;
		protected RawFile(){
			posLayer = new RawFileLayer(this, true);
			negLayer = new RawFileLayer(this, false);
		}
		/// <summary>
		/// In the implementations, several methods start with the block if (!preInitialized){ PreInit(); }, 
		/// and this method normally ends with the statement preInitialized = true;
		/// </summary>
		protected abstract void PreInit();
		/// <summary>
		/// The case-insensitive suffix of the raw data files, e.g., .d, .mzxml, .wiff, or .raw.
		/// Please specify in lower case.
		/// </summary>
		public abstract string Suffix{ get; }
		/// <summary>
		/// True if the path to a raw data set should be a folder (directory); false if it should be a simple file.
		/// </summary>
		public abstract bool IsFolderBased{ get; }
		public abstract string Name{ get; }
		public abstract bool NeedsIsolationWindow{ get; }
		public abstract bool NeedsBackgroundSubtraction{ get; }
		/// <summary>
		/// Every raw data implementation has its own implementation of MsInstrument.
		/// </summary>
		public abstract MsInstrument DefaultInstrument{ get; }
		/// <summary>
		/// A self-contained raw file reader will simply return true, but some readers depend on external programs,
		/// and these will only return true if those programs are also installed. At this time, the most important 
		/// dependence of this kind is the dependence of Thermo on MSFileReader. This is only checked when new raw
		/// data sources are being loaded.
		/// </summary>
		public abstract bool IsInstalled{ get; }
		public abstract bool NeedsGrid{ get; }
		public abstract bool HasIms{ get; }
		/// <summary>
		/// The message to be returned if a required RawFile class is not completely installed.
		/// </summary>
		public abstract string InstallMessage{ get; }
		/// <summary>
		/// First scan in iteration over all scan numbers in InitFromRawFileImpl.
		/// </summary>
		public abstract int FirstScanNumber{ get; }
		/// <summary>
		/// Last scan in iteration over all scan numbers in InitFromRawFileImpl.
		/// </summary>
		public abstract int LastScanNumber{ get; }
		/// <summary>
		/// Sum of Ms1Count from the two layers. No usages found!
		/// </summary>
		public int Ms1Count => posLayer.Ms1Count + negLayer.Ms1Count;
		public virtual int PasefMsmsCount => 0;
		public virtual bool IsAstral => false;
		public virtual PasefFrameMsMsInfo GetPasefMsmsInfo(int index){
			return null;
		}
		public virtual int PasefPrecursorCount => 0;
		public virtual PasefPrecursorInfo GetPasefPrecursorInfo(int index){
			return null;
		}
		public double StartTime{
			get{
				if (negLayer.Ms1Count + negLayer.Ms2Count + negLayer.Ms3Count == 0) return posLayer.StartTime;
				if (posLayer.Ms1Count + posLayer.Ms2Count + posLayer.Ms3Count == 0) return negLayer.StartTime;
				return Math.Min(posLayer.StartTime, negLayer.StartTime);
			}
		}
		public double EndTime => Math.Max(posLayer.EndTime, negLayer.EndTime);
		public double Ms1MassMin => Math.Min(posLayer.Ms1MassMin, negLayer.Ms1MassMin);
		public double Ms1MassMax => Math.Max(posLayer.Ms1MassMax, negLayer.Ms1MassMax);
		public double Ms2MassMin => Math.Min(posLayer.Ms2MassMin, negLayer.Ms2MassMin);
		public double Ms2MassMax => Math.Max(posLayer.Ms2MassMax, negLayer.Ms2MassMax);
		public double MaxIntensity => Math.Max(posLayer.MaxIntensity, negLayer.MaxIntensity);
		public abstract void GetSpectrum(int scanNumberMin, int scanNumberMax, int imsIndexMin,
			int imsIndexMax, bool readCentroids, out double[] masses, out float[] intensities, double resolution,
			double gridSpacing, double mzMin, double mzMax, bool isMs1);
		public abstract IntSpectrum[] GetSpectrum(int scanNumberMin, int scanNumberMax, int[] imsIndexMin,
			int[] imsIndexMax, bool readCentroids);
		public abstract Spectrum GetSpectrum(IntSpectrum s, double resolution, double gridSpacing);
		protected internal void GetSpectrum(int scanNumber, bool readCentroids, out double[] masses,
			out float[] intensities, bool isMs1){
			GetSpectrum(scanNumber, scanNumber, 0, 0, readCentroids, out masses, out intensities, 0, double.NaN,
				double.NaN, double.NaN, isMs1);
		}
		/// <summary>
		/// 1/k0 is an alternative indexing for ion mobility.
		/// The regular <code>imsInds</code> refer to the ion mobility bin which is not transferable between runs.
		/// 1/k0 is however transferable between runs.
		/// </summary>
		protected internal abstract double[] Index2K0(int scanNumber, double[] imsInds);
		/// <summary>
		/// Default implementation does nothing more than to call PathIsValid, which examines three conditions:
		/// (1) the path exists, (2) it is a file or folder as expected by IsFolderBased, and (3) it ends with
		/// the string given in Suffix. When this is not sufficient to distinguish every vendor, this method
		/// must be overridden in the concrete implementations. In particular, raw data from both Agilent and
		/// Bruker is stored in folders with the suffix .d, so AgilentRawFile overrides this method to check for
		/// the existence of a sub-directory named AcqData, and BrukerRawFileSqlite checks for the existence of
		/// a file named analysis.baf. Used only by FindSuitableTemplate.
		/// </summary>
		/// <param name="path1"></param>
		/// <returns></returns>
		public virtual bool IsSuitableFile(string path1){
			return PathIsValid(path1);
		}
		/// <summary>
		/// Examines argument to determine if it has the characteristics expected of a valid raw data file or folder.
		/// true is returned if and only if three conditions are met: (1) the path exists, (2) it is a file or folder
		/// as expected by IsFolderBased, and (3) it ends with the string given in Suffix. Used only by IsSuitableFile,
		/// which may be overridden to remove ambiguity between some pairs of vendors.
		/// </summary>
		/// <param name="path1">path to be examined</param>
		/// <returns>true if the argument looks like a raw data file or folder of the right type, false otherwise</returns>
		public bool PathIsValid(string path1){
			FileSystemInfo info;
			if (IsFolderBased){
				info = new DirectoryInfo(path1);
			} else{
				info = new FileInfo(path1);
			}
			if (!info.Exists){
				return false;
			}
			if (!info.Extension.ToUpper().Equals(Suffix.ToUpper())){
				return false;
			}
			return true;
		}
		public virtual Dictionary<int, DiaWindowGroup> GetDiaWindowGroups(){
			return null;
		}
		/// <summary>
		/// Create a ScanInfo object with information on the scan with the given number.
		/// </summary>
		/// <param name="scanNumber"></param>
		/// <returns></returns>
		protected abstract ScanInfo GetInfoForScanNumber(int scanNumber);
		/// <summary>
		/// Path to the raw data file in proprietary format. Value is set immediately after creation 
		/// of the RawFile and is never modified. It is used in the RawFile* implementations of the
		/// abstract class RawFile.
		/// </summary>
		public string Path{
			get{
				if (path == null){
					throw new Exception("Attempt to get Path before it is set.");
				}
				return path;
			}
			set{
				if (path != null){
					throw new Exception("Attempt to set Path a second time.");
				}
				path = value;
			}
		}
		/// <summary>
		/// From the name of the raw data file of this RawFile, create the name of the corresponding 
		/// index file by changing the suffix to "index". (Immutable, since Path can only be set once.)
		/// </summary>
		/// <returns></returns>
		public string IndexFilename{
			get{
				if (path == null){
					throw new Exception("Attempt to get IndexFilename before Path is set.");
				}
				return System.IO.Path.ChangeExtension(path, "index");
			}
		}
		private readonly object locker = new object();
		public void Init(string path1){
			lock (locker){
				path = path1;
				if (File.Exists(IndexFilename) && IndexVersionIsCurrent()){
					try{
						ReadIndex();
					} catch (Exception){
						InitFromRawFile();
						WriteIndex();
					}
				} else{
					InitFromRawFile();
					WriteIndex();
				}
			}
		}
		/// <summary>
		/// Call InitFromRawFileImpl and, if appropriate, PreInit and InitMassGrid. 
		/// Only called from Init, which is only called from CreateRawFile.
		/// </summary>
		protected void InitFromRawFile(){
			if (!preInitialized){
				PreInit();
			}
			InitFromRawFileImpl();
			if (NeedsGrid){
				InitMassGrid();
			}
		}
		private void InitMassGrid(){
			Dictionary<int, double> mins = new Dictionary<int, double>();
			RawFileUtils.InitMassGrid(posLayer, mins);
			RawFileUtils.InitMassGrid(negLayer, mins);
			double[] values = RawFileUtils.MakeMonotone(mins, out int[] indices);
			RawFileUtils.Interpolate(ref indices, ref values);
		}
		/// <summary>
		/// Extract ScanInfo from this RawFile for each scan and add it to the posLayer or negLayer RawFileLayer of this RawFile.
		/// </summary>
		private void InitFromRawFileImpl(){
			InfoLists posInfoList = new InfoLists();
			InfoLists negInfoList = new InfoLists();
			double maximumIntensity = 0.0;
			bool hasFaims = false;
			HashSet<double> faimsVoltages = new HashSet<double>();
			for (int scanNum = FirstScanNumber; scanNum <= LastScanNumber; scanNum++){
				if (FirstScanNumber > LastScanNumber){
					break;
				}
				ScanInfo scanInfo = GetInfoForScanNumber(scanNum);
				if (scanInfo == null){
					continue;
				}
				if (scanInfo.faimsVoltageOn){
					hasFaims = true;
					faimsVoltages.Add(scanInfo.faimsCv);
				}
				if (scanInfo.positiveIonMode){
					posInfoList.Add(scanInfo, scanNum);
				} else{
					negInfoList.Add(scanInfo, scanNum);
				}
				maximumIntensity = Math.Max(maximumIntensity, scanInfo.basepeakIntensity);
			}
			double[] voltages = faimsVoltages.ToArray();
			Array.Sort(voltages);
			posLayer.SetData(posInfoList, maximumIntensity, hasFaims, voltages,
				GetVoltageInds(voltages, hasFaims, posInfoList.ms1Lists.faimsCv),
				GetVoltageInds(voltages, hasFaims, posInfoList.ms2Lists.faimsCv),
				GetVoltageInds(voltages, hasFaims, posInfoList.ms3Lists.faimsCv));
			negLayer.SetData(negInfoList, maximumIntensity, false, new double[0], new int[0][], new int[0][],
				new int[0][]);
		}
		public static int[] GetVoltageInds(double voltage, List<double> faimsCv){
			List<int> valids = new List<int>();
			for (int i = 0; i < faimsCv.Count; i++){
				if (faimsCv[i] == voltage){
					valids.Add(i);
				}
			}
			return valids.ToArray();
		}
		public static int[][] GetVoltageInds(double[] voltages, bool hasFaims, List<double> faimsCv){
			if (!hasFaims){
				return new int[0][];
			}
			int[][] result = new int[voltages.Length][];
			for (int i = 0; i < result.Length; i++){
				result[i] = GetVoltageInds(voltages[i], faimsCv);
			}
			return result;
		}
		/// <summary>
		/// Write indexVersion, Application.ProductVersion, posLayer, negLayer, and (if NeedsGrid) gridInfo to IndexFilename.
		/// Called only from Init, which is called only from CreateRawFile.
		/// The corresponding read method is ReadIndex.
		/// </summary>
		protected void WriteIndex(){
			BinaryWriter writer = null;
			try{
				writer = FileUtils.GetBinaryWriter(IndexFilename);
				writer.Write(indexVersion);
				writer.Write("1.0.0.0");
				posLayer.Write(writer);
				negLayer.Write(writer);
			} finally{
				writer?.Close();
			}
		}
		/// <summary>
		/// Whether the pre-existing index file has the format of the current version of the software.
		/// </summary>
		/// <returns></returns>
		protected bool IndexVersionIsCurrent(){
			BinaryReader reader = null;
			try{
				reader = FileUtils.GetBinaryReader(IndexFilename);
				int fileIndexVersion = reader.ReadInt32();
				reader.Close();
				Thread.Sleep(200);
				return fileIndexVersion == indexVersion;
			} catch (Exception){
				return false;
			} finally{
				reader?.Close();
			}
		}
		/// <summary>
		/// Set posLayer, negLayer, and (if NeedsGrid) gridInfo from IndexFilename.
		/// The work is done by RawFileLayer(reader, this) and MassGridInfo(reader).
		/// Called only from Init, which is called only from CreateRawFile.
		/// The corresponding write method is WriteIndex.
		/// </summary>
		protected void ReadIndex(){
			BinaryReader reader = null;
			try{
				reader = FileUtils.GetBinaryReader(IndexFilename);
				int indexVers =
					reader.ReadInt32(); // dummy var because we already know the answer from IndexVersionIsCurrent()
				string version = reader.ReadString(); // dummy var because the following if-block is commented out
				//if (!version.Equals(Application.ProductVersion)){
				//    throw new Exception("Wrong version");
				//}
				posLayer = new RawFileLayer(reader, this, true);
				negLayer = new RawFileLayer(reader, this, false);
			} finally{
				reader?.Close();
			}
		}
		public virtual bool CheckForRepeatingMs2Windows(double[] ms2IsolationMzMin){
			int ms2Count = ms2IsolationMzMin.Length;
			if (ms2Count == 0){
				return false;
			}
			double minValue = ms2IsolationMzMin[0];
			int repeatPos = -1;
			for (int i = 1; i < ms2Count; i++){
				double value = ms2IsolationMzMin[i];
				if (value == minValue){
					repeatPos = i;
					break;
				}
			}
			if (repeatPos == -1){
				return false;
			}
			if (ms2Count < 2 * repeatPos){
				return false;
			}
			for (int i = 0; i < repeatPos; i++){
				if (ms2IsolationMzMin[i] != ms2IsolationMzMin[i + repeatPos]){
					return false;
				}
			}
			return true;
		}
		public virtual int[][] GetDiaMs2Indices(double[] ms2IsolationMzMin, int[] ms2PrevMs1Index, int ms1Count,
			int lastMs2Index){
			int ms2Count = ms2IsolationMzMin.Length;
			if (ms2Count == 0){
				return new int[0][];
			}
			if (ms2IsolationMzMin.Length == 0){
				return new int[0][];
			}
			double d = ms2IsolationMzMin[0];
			int len = 1;
			while (len < ms2IsolationMzMin.Length && ms2IsolationMzMin[len] != d){
				len++;
			}
			int[][] result = new int[len][];
			int len2 = ms2Count / len;
			for (int i = 0; i < len; i++){
				result[i] = new int[len2];
				for (int j = 0; j < len2; j++){
					result[i][j] = i + len * j;
				}
			}
			return result;
		}
		public virtual void Dispose(){
			posLayer?.Dispose();
			posLayer = null;
			negLayer?.Dispose();
			negLayer = null;
		}
		public RawFileLayer GetPosLayer(){
			return posLayer;
		}
		public RawFileLayer GetLayer(bool positiveMode){
			return positiveMode ? posLayer : negLayer;
		}
	}
}