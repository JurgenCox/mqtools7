using MqApi.Drawing;
using MqApi.Num;
using MqUtil.Base;
using MqUtil.Data;
using MqUtil.Ms.Enums;
using MqUtil.Ms.Raw;
using MqUtil.Ms.Utils;
using MqUtil.Num;
using MqUtil.Symbol;
using MqUtil.Util;
namespace MqUtil.Ms.Graph{
    public abstract class HeatMapData{
		protected static Color2 selectedColor = Color2.Red;

		protected ThreadSafeDictionary<int, WorkDispatcher>
			gridValues = new ThreadSafeDictionary<int, WorkDispatcher>();

		protected ThreadSafeDictionary<int, WorkDispatcher> gridValuesMulti =
			new ThreadSafeDictionary<int, WorkDispatcher>();

		protected WorkDispatcher massTraceValues;
		public bool showIsolationWindow = true;
		public Ms2Selection Ms2Selection{ get; set; }
		public bool ShowOnlySelectedMsms{ get; set; }
		public bool ShowMsms{ get; set; }
		protected readonly Random3 random = new Random3();
		public List<double> MassTracesMs{ get; set; }
		public List<Color2> MassTraceColorsMs{ get; set; }
		public PeakColorMode PeakColorModeMs{ get; set; }
		public ShowPeakMode ShowPeakModeMs{ get; set; }
		public List<double> MassTracesMsms{ get; set; }
		public List<Color2> MassTraceColorsMsms{ get; set; }
		public PeakColorMode PeakColorModeMsms{ get; set; }
		public ShowPeakMode ShowPeakModeMsms{ get; set; }
		public int PeakLineWidthMs{ get; set; }
		public int PeakLineWidthMsms{ get; set; }

		public object locker = new object();
		protected HeatMapData(){
			PeakLineWidthMs = 2;
			PeakLineWidthMsms = 2;
			MassTracesMs = new List<double>();
			MassTraceColorsMs = new List<Color2>();
			MassTracesMsms = new List<double>();
			MassTraceColorsMsms = new List<Color2>();
			Ms2Selection = new Ms2Selection();
			ShowOnlySelectedMsms = false;
			PeakColorModeMs = PeakColorMode.ByIsotopeCluster;
			ShowPeakModeMs = ShowPeakMode.None;
			PeakColorModeMsms = PeakColorMode.ByIsotopeCluster;
			ShowPeakModeMsms = ShowPeakMode.None;
		}

		public void Abort(){
			foreach (int key in gridValues.Keys){
				WorkDispatcher x = gridValues[key];
				x?.Abort();
			}
			gridValues = new ThreadSafeDictionary<int, WorkDispatcher>();
			foreach (int key in gridValuesMulti.Keys){
				WorkDispatcher x = gridValuesMulti[key];
				x?.Abort();
			}
			gridValuesMulti = new ThreadSafeDictionary<int, WorkDispatcher>();
		}

		public List<double> GetMassTraces(VisibleData visibleData){
			return visibleData == VisibleData.MsRt || visibleData == VisibleData.MsIm ? MassTracesMs : MassTracesMsms;
		}

		public List<Color2> GetMassTraceColors(VisibleData visibleData){
			return visibleData == VisibleData.MsRt || visibleData == VisibleData.MsIm
				? MassTraceColorsMs
				: MassTraceColorsMsms;
		}

		public PeakColorMode GetPeakColorMode(VisibleData visibleData){
			return visibleData == VisibleData.MsRt || visibleData == VisibleData.MsIm
				? PeakColorModeMs
				: PeakColorModeMsms;
		}

		public ShowPeakMode GetShowPeakMode(VisibleData visibleData){
			return visibleData == VisibleData.MsRt || visibleData == VisibleData.MsIm
				? ShowPeakModeMs
				: ShowPeakModeMsms;
		}

		public int GetPeakLineWidth(VisibleData visibleData){
			return visibleData == VisibleData.MsRt || visibleData == VisibleData.MsIm
				? PeakLineWidthMs
				: PeakLineWidthMsms;
		}

		public abstract string InitCombinedData(IList<string> fileNames);
		public abstract void AddRun(string path);

		public abstract string[] GetLayerInfos(int fileIndex);

		public abstract bool SetSelectedLayer(int fileIndex, string layerName);
		public abstract bool IsInitialized();
		public abstract RawFile GetRawFile(string name);
		public abstract RawFileLayer GetRawFileLayer(int fileIndex, bool positive);
		public abstract double GetCalibratedRtFromRt(double time, int fileIndex, bool positive);
		public abstract double GetDecalibratedRt(double time, int fileIndex, bool positive);
		public abstract bool HasParams{ get; }
		public abstract BasicParams BasicParams{ get; }
		public abstract bool HasGeneNames{ get; }
		public abstract bool HasProteinNames{ get; }
		public abstract byte MaxMultiplicity{ get; }
		public abstract int TotalMaxCharge{ get; }
		public abstract EquatableArray<string>[] AllEnzymes{ get; }
		public abstract char[] AllAas{ get; }
		public abstract string[] AllVarMods{ get; }
		public abstract bool HasMsms{ get; }
		public abstract bool HasTims{ get; }
		public abstract bool IsCustomQuantification{ get; }
		public abstract int GetImsStep(int index);
        public abstract bool IsBoxCar(int index);
        public abstract bool IsDia(int index);
		public abstract bool IsFaims(int index);
        public abstract bool IsIms(int index);
		public abstract bool NeucodeInSilicoLowRes(int index);
		public abstract double NeucodeResolution(int index);
		public abstract bool NeucodeResolutionInMda(int index);
		public abstract string GetRawFileName(int i);
		public abstract bool IsProcessed1(int i, bool positive);
		public abstract string GetRawFilePath(int i);
		public abstract Tuple<double[], double[]> GetRtCalibrationFunction(int i, bool positive);
		public abstract bool MsmsFound(int fileIndex, int scanNumber);

		public abstract double[,] GetValues(int width, int height, double zoomXMin, double zoomXMax, double zoomYMin,
			double zoomYMax, int blur, bool recalMasses, bool recalTimes, bool readCentroids, int calcInd,
			int selectedFileIndex, bool positive, int iresolution, VisibleData visibleData, double imsIndMin,
			double imsIndMax, double resolution, double gridSpacing, int diaIndex, bool isDia, 
			int faimsIndex, bool isFaims);

		public abstract HeatmapPaintObjects GetPeaks(double zoomXMin, double zoomXMax, double zoomYMin, double zoomYMax,
			bool recalTimes, bool recalMasses, int fileIndex, int lineWidth, bool positive, int iresolution,
			VisibleData visibleData, double imsIndMin, double imsIndMax, double resolution, double gridSpacing, int diaInd, int faimsInd);

		public abstract double[,] GetValuesAt(int width, int height, double zoomXMin, double zoomXMax, double zoomYMin,
			double zoomYMax, int blur, bool recalMasses, bool recalTimes, bool readCentroids, int runIndex,
			bool positive, int iresolution, VisibleData visibleData, double imsIndMin, double imsIndMax,
			double resolution, double gridSpacing, int diaIndex, bool isDia, int faimsIndex, bool isFaims);

		public abstract double[,] GetChromatogramValues(int blur, bool recalTimes, bool readCentroids, double minTime,
			double maxTime, int bins, int selectedFileIndex, bool positive, VisibleData visibleData, double imsIndMin,
			double imsIndMax, double resolution, double gridSpacing, int diaWindowIndex);

		public int GetIndexFromRt(int fileIndex, bool positive, VisibleData visibleData, double rt){
			RawFileLayer layer = GetRawFileLayer(fileIndex, positive);
			return visibleData == VisibleData.MsRt ? layer.GetMs1IndexFromRt(rt) : layer.GetMs2IndexFromRt(rt);
		}


		public double[] GetMs1Rt(int fileIndex, bool positive){
			return GetRawFileLayer(fileIndex, positive).Ms1Rt;
		}

		public static int ModelToViewX(double m, int width, double xmin, double xmax){
			return (int) Math.Round((m - xmin) / (xmax - xmin) * width);
		}

		public static int ModelToViewY(double t, int height, double ymin, double ymax){
			int y = (int) Math.Round((t - ymin) / (ymax - ymin) * height);
			return height - y - 1;
		}
		public HeatmapPaintObjects GetMsms(double zoomXMin, double zoomXMax, double zoomYMin, double zoomYMax,
			bool recalTimes, int fileIndex, bool positive, double ms2PrecShift){
			RawFileLayer rfl = GetRawFileLayer(fileIndex, positive);
			HeatmapPaintObjects paintObjects = new HeatmapPaintObjects();
			if (!ShowMsms) return paintObjects;
			bool onlySelected = ShowOnlySelectedMsms;
			if (!IsProcessed1(fileIndex, positive)){
				if (!onlySelected){
					List<int> ind = rfl.GetMs2InRectangle(zoomXMin, zoomXMax, zoomYMin, zoomYMax, ms2PrecShift);
					paintObjects.AddPaintObjects(PaintMsms(ind, false, false, recalTimes, fileIndex, positive,
						ms2PrecShift));
				}
				List<int> sel = GetSelectionInRectangle(zoomXMin, zoomXMax, zoomYMin, zoomYMax, rfl, ms2PrecShift);
				paintObjects.AddPaintObjects(PaintMsms(sel, false, true, recalTimes, fileIndex, positive,
					ms2PrecShift));
			} else{
				bool canRead = true;
				int[] scanNumbers = rfl.Ms2ScanNumbers;
				List<int> identified = new List<int>();
				List<int> unidentified = new List<int>();
				if (!onlySelected){
					double tmin = zoomYMin;
					double tmax = zoomYMax;
					if (recalTimes){
						tmin = GetDecalibratedRt(tmin, fileIndex, positive);
						tmax = GetDecalibratedRt(tmax, fileIndex, positive);
					}
					List<int> ind = rfl.GetMs2InRectangle(zoomXMin, zoomXMax, tmin, tmax, ms2PrecShift);
					foreach (int index in ind){
						bool found = false;
						if (HasParams && canRead){
							try{
								if (MsmsFound(fileIndex, scanNumbers[index])){
									found = true;
								}
							} catch (ArithmeticException){
								canRead = false;
							}
						}
						if (found){
							identified.Add(index);
						} else{
							unidentified.Add(index);
						}
					}
					paintObjects.AddPaintObjects(PaintMsms(identified.ToArray(), true, false, recalTimes, fileIndex,
						positive, ms2PrecShift));
					paintObjects.AddPaintObjects(PaintMsms(unidentified.ToArray(), false, false, recalTimes, fileIndex,
						positive, ms2PrecShift));
					identified.Clear();
					unidentified.Clear();
				}
				double tmin1 = zoomYMin;
				double tmax1 = zoomYMax;
				if (recalTimes){
					tmin1 = GetDecalibratedRt(tmin1, fileIndex, positive);
					tmax1 = GetDecalibratedRt(tmax1, fileIndex, positive);
				}
				List<int> sel = GetSelectionInRectangle(zoomXMin, zoomXMax, tmin1, tmax1, rfl, ms2PrecShift);
				foreach (int index in sel){
					bool found = false;
					if (HasParams && canRead){
						try{
							if (MsmsFound(fileIndex, scanNumbers[index])){
								found = true;
							}
						} catch (Exception){
							canRead = false;
						}
					}
					if (found){
						identified.Add(index);
					} else{
						unidentified.Add(index);
					}
				}
				paintObjects.AddPaintObjects(PaintMsms(identified.ToArray(), true, true, recalTimes, fileIndex,
					positive, ms2PrecShift));
				paintObjects.AddPaintObjects(PaintMsms(unidentified.ToArray(), false, true, recalTimes, fileIndex,
					positive, ms2PrecShift));
			}
			return paintObjects;
		}

		public List<int> GetSelectionInRectangle(double minMass, double maxMass, double minRt, double maxRt,
			RawFileLayer rfl, double ms2PrecShift){
			List<int> sel = Ms2Selection.GetSelection();
			List<int> result = new List<int>();
			foreach (int p in sel){
				double t = rfl.GetMs2Time(p);
				if (t < minRt || t > maxRt){
					continue;
				}
				double ms = rfl.GetMs2ParentMz(p, ms2PrecShift);
				double mz = ms;
				if (mz < minMass || mz > maxMass){
					continue;
				}
				result.Add(p);
			}
			return result;
		}

		public HeatmapPaintObjects PaintMsms(IEnumerable<int> ind, bool identified, bool selected, bool recalTimes,
			int fileIndex, bool positive, double ms2PrecShift){
			SymbolType symbolType = identified ? SymbolType.square : SymbolType.cross;
			int symbolSize = 7;
			Color2 symbolColor = identified ? Color2.Magenta : Color2.Blue;
			HeatmapPaintObjects paintObjects = new HeatmapPaintObjects();
			RawFileLayer rf = GetRawFileLayer(fileIndex, positive);
			foreach (int index in ind){
				double t = rf.GetMs2Time(index);
				int ms1Ind = rf.GetMs1IndexFromRt(t);
				if (recalTimes) t = GetCalibratedRtFromRt(t, fileIndex, positive);
				double ms = rf.GetMs2ParentMz(index, ms2PrecShift);
				double isoMin = rf.GetMs2IsolationMin(index);
				double isoMax = rf.GetMs2IsolationMax(index);
				double[] d = rf.GetMs1TimeSpan(ms1Ind);
				double t1 = d[0];
				double t2 = d[1];
				if (recalTimes){
					t1 = GetCalibratedRtFromRt(t1, fileIndex, positive);
					t2 = GetCalibratedRtFromRt(t2, fileIndex, positive);
				}
				if (d.Length == 2) t = Math.Min(t1, t2) + Math.Abs(t2 - t1) / 2;
				if (selected){
					symbolColor = selectedColor;
					if (showIsolationWindow && !double.IsNaN(isoMin))
						paintObjects.AddRectangle(symbolColor.Value, isoMin, Math.Min(t1, t2),
							Math.Abs(isoMin - isoMax), Math.Abs(t1 - t2));
				}
				paintObjects.AddSymbol(symbolType, symbolSize, symbolColor.Value, ms, t);
			}
			return paintObjects;
		}

		public void SetShowPeakMode(ShowPeakMode showPeakMode, VisibleData visibleData){
			if (visibleData == VisibleData.MsRt){
				ShowPeakModeMs = showPeakMode;
			} else{
				ShowPeakModeMsms = showPeakMode;
			}
		}

		public void SetPeakColorMode(PeakColorMode peakColorMode, VisibleData visibleData){
			if (visibleData == VisibleData.MsRt){
				PeakColorModeMs = peakColorMode;
			} else{
				PeakColorModeMsms = peakColorMode;
			}
		}

		public void SetPeakLineWidth(int newVal, VisibleData visibleData){
			if (visibleData == VisibleData.MsRt){
				PeakLineWidthMs = newVal;
			} else{
				PeakLineWidthMsms = newVal;
			}
		}

		public double[][] GetMs1SpectrumOnGrid(int fileIndex, bool positive, int index, bool readCentroids, double min,
			double max, int count, double imsIndexMin, double imsIndexMax, double resolution, double gridSpacing){
			return GetRawFileLayer(fileIndex, positive).GetMs1SpectrumOnGrid(index, readCentroids, min, max, count,
				(int) Math.Round(imsIndexMin), (int) Math.Round(imsIndexMax), resolution, gridSpacing);
		}

		public List<int> GetMs2InRectangle(int index, bool positive, double minMz, double maxMz, double minRt,
			double maxRt, double ms2PrecShift){
			return GetRawFileLayer(index, positive).GetMs2InRectangle(minMz, maxMz, minRt, maxRt, ms2PrecShift);
		}

		public double[][] GetTicOnGrid(int index, bool positive, double min, double max, int count){
			return GetRawFileLayer(index, positive).GetTicOnGrid(min, max, count);
		}

		public int GetMs1Count(int index, bool positive){
			return GetRawFileLayer(index, positive).Ms1Count;
		}

		public virtual int GetMs2Count(int index, bool positive){
			return GetRawFileLayer(index, positive).Ms2Count;
		}

		public bool[] GetMs1SelectionOnGrid(int index, bool positive, double minRt, double rtStep, int rtCount,
			int selectedMs1Index){
			return GetRawFileLayer(index, positive).GetMs1SelectionOnGrid(minRt, rtStep, rtCount, selectedMs1Index);
		}

		public bool[] GetMs2SelectionOnGrid(int index, bool positive, double minRt, double rtStep, int rtCount,
			int selectedMs2Index){
			return GetRawFileLayer(index, positive).GetMs2SelectionOnGrid(minRt, rtStep, rtCount, selectedMs2Index);
		}

		public double GetMs1Rt(int index, bool positive, int ind){
			return GetRawFileLayer(index, positive).Ms1Rt[ind];
		}

		public double GetMs2Rt(int index, bool positive, int ind){
			return GetRawFileLayer(index, positive).Ms2Rt[ind];
		}

		public virtual int GetMs2ScanNumber(int index, bool positive, int ind){
			return GetRawFileLayer(index, positive).Ms2ScanNumbers[ind];
		}

		public virtual int GetMs1IndexFromRt(int index, bool positive, double rt){
			return GetRawFileLayer(index, positive).GetMs1IndexFromRt(rt);
		}

		public double GetMs2Mz(int index, bool positive, int ind){
			return GetRawFileLayer(index, positive).Ms2Mz[ind];
		}

		public int[] GetScanIndices(int index, bool positive, int length, VisibleData visibleData, double zIndMin,
			double zIndMax, bool horizontal, double ymin, double ymax, double resolution, double gridSpacing){
			RawFileLayer layer = GetRawFileLayer(index, positive);
			RawLayer rfl;
			if (visibleData == VisibleData.MsRt){
				if (zIndMin < 0 && zIndMax < 0){
					rfl = new RawLayerMs1(layer);
				} else{
					rfl = new RawLayerMs1TimsWindowSummedFrames(layer, (int) Math.Round(zIndMin),
						(int) Math.Round(zIndMax), resolution, gridSpacing, double.NaN, double.NaN);
				}
			} else{
				rfl = new RawLayerMs2(layer);
			}
			int[] result = new int[length];
			for (int i = 0; i < result.Length; i++){
				double t = ViewToModel(i, length, horizontal, ymin, ymax);
				result[i] = rfl.GetIndexFromRt(t);
			}
			return result;
		}

		private static double ViewToModel(int m, int length, bool horizontal, double ymin, double ymax){
			if (!horizontal){
				m = length - 1 - m;
			}
			return ymin + m * (ymax - ymin) / length;
		}

		public Tuple<double, double, double, double, double, double, double> GetRanges(int fileIndex){
			lock (locker){
				RawFile rf = GetRawFile(fileIndex);
				Tuple<double, double, double, double, double, double, double> result =
					new Tuple<double, double, double, double, double, double, double>(rf.Ms1MassMin, rf.Ms1MassMax,
						rf.Ms2MassMin, rf.Ms2MassMax, rf.StartTime, rf.EndTime, rf.MaxIntensity);
				return result;
			}
		}

		public abstract RawFile GetRawFile(int fileIndex);

		public bool HasIms(int fileIndex){
			return GetRawFile(fileIndex).HasIms;
		}

		public int GetMs1MaxNumIms(int fileIndex, bool positive){
			return GetRawFileLayer(fileIndex, positive).Ms1MaxNumIms;
		}

		public Tuple<int[], int[], double[], int[][]> GetFrameInfo(int fileIndex, bool positive){
			RawFileLayer rfl = GetRawFileLayer(fileIndex, positive);
			RawLayerMs1TimsWindowSummedFrames.Prepare(rfl.Ms1ScanNumbers, rfl.Ms1Rt, out int[] frameIndMin,
				out int[] frameIndMax, out double[] rts);
			int[][] ms2FrameInds = GetMs2FrameInds(rfl, frameIndMin);
			return new Tuple<int[], int[], double[], int[][]>(frameIndMin, frameIndMax, rts, ms2FrameInds);
		}

		private static int[][] GetMs2FrameInds(RawFileLayer rawFile, int[] frameIndMin){
			List<int>[] result = new List<int>[frameIndMin.Length];
			for (int i = 0; i < result.Length; i++){
				result[i] = new List<int>();
			}
			for (int i = 0; i < rawFile.Ms2Count; i++){
				int ms1Ind = rawFile.GetPreviousMs1IndForMs2Ind(i);
				int x = ArrayUtils.FloorIndex(frameIndMin, ms1Ind);
				if (x < 0){
					continue;
				}
				result[x].Add(i);
			}
			int[][] r = new int[result.Length][];
			for (int i = 0; i < r.Length; i++){
				r[i] = result[i].ToArray();
			}
			return r;
		}

		public abstract SpectrumInfo ProvideMs2Data(int fileIndex, bool positive, int selected, bool isFaims, int faimsIndex);
		public abstract void ClearTables();
		public abstract void ReadFileInfo();
		public abstract string[] GetInternalModifications();

		public void SetMassTraces(VisibleData msRt, IList<double> mzTraces){
			GetMassTraces(msRt).Clear();
			GetMassTraceColors(msRt).Clear();
			GetMassTraces(msRt).AddRange(mzTraces);
			for (int c = 0; c < mzTraces.Count; c++){
				GetMassTraceColors(msRt).Add(Color2.GetPredefinedColor(c));
			}
		}

		public abstract void LoadCombinedFolder(string combinedFolder);
		public abstract (double mzMin, double mzMax)[] GetDiaIndices(int index, bool positiveMode);
		public abstract double[] GetFaimsVoltages(int index, bool positiveMode);
		public abstract bool CheckIfDia(int index, bool positiveMode);
		public abstract bool CheckIfFaims(int index, bool positiveMode);
	}
}