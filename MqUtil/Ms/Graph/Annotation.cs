using System.Text;
using MqApi.Drawing;
using MqApi.Util;
using MqUtil.Drawing;
using MqUtil.Ms.Annot;
using MqUtil.Ms.Annot.Ions;
using MqUtil.Ms.Enums;
using MqUtil.Num;
namespace MqUtil.Ms.Graph {
	public class Annotation : GraphicObject, IComparable {
		public double Mz { get; }
		public int PeakId { get; set; }
		public double Deviation { get; set; }
		public Unit DeviationUnit { get; set; }
		private int hash;
		private Color2 color = Color2.Empty;
		public bool Expert { get; set; }
		public Annotation(PeakAnnotation pa) : base(null) {
			Mz = double.NaN;
			PeakId = -1;
			Deviation = double.NaN;
			PeakAnnotation = pa ?? throw new Exception("PeakAnnotation cannot be null.");
			Mz = pa.Mz;
		}

		private string MzValue => DeviationUnit == Unit.Ppm
			? NumUtils.RoundSignificantDigits2(Mz, 7, 1000000)
			: Parser.ToString(Math.Round(Mz, 2));

		public int Charge => PeakAnnotation?.Charge ?? 0;
		public string RichText => PeakAnnotation.ToString2(false);
		public PeakAnnotation PeakAnnotation { get; }
		public Rectangle2 Location { get; set; } = Rectangle2.Empty;

		public Color2 Color {
			get {
				if (color.IsEmpty) {
					color = GetColor(this);
				}
				return color;
			}
			set => color = value;
		}

		public Annotation Copy() {
			return new Annotation(PeakAnnotation) {Pen = Pen, Location = Rectangle2.Empty, Expert = Expert};
		}

		public void GetText(out string text1, out string text2, out Font2 font1, out Font2 font2) {
			MainDesc(out text1, out font1);
			SubDesc(out text2, out font2);
		}

		public override int GetHashCode() {
			if (hash == 0) {
				hash = 31 * hash + PeakId;
				hash = 31 * hash + PeakAnnotation.GetHashCode();
				hash = 31 * hash + GetType().GetHashCode();
			}
			return hash;
		}

		public override bool Equals(object obj) {
			if (this == obj) {
				return true;
			}
			if (obj is Annotation) {
				Annotation other = (Annotation) obj;
				if (other.PeakId != PeakId) {
					return false;
				}
				if (!PeakAnnotation.Equals(other.PeakAnnotation)) {
					return false;
				}
				return GetType() == other.GetType();
			}
			return false;
		}

		public int CompareTo(object obj) {
			if (obj is Annotation) {
				return CompareTo((Annotation) obj);
			}
			throw new ArgumentException(obj + " is not the same type as this instance.");
		}

		private int CompareTo(Annotation a) {
			if (Equals(this, a)) {
				return 0;
			}
			if (a.PeakAnnotation.Score > PeakAnnotation.Score) {
				return -1;
			}
			if (a.PeakAnnotation.Score < PeakAnnotation.Score) {
				return 1;
			}
			if (!a.Expert && Expert) {
				return -1;
			}
			if (!Expert && a.Expert) {
				return 1;
			}
			return 0;
		}

		public void Draw(IGraphics g) {
			g.SmoothingMode = SmoothingMode2.AntiAlias;
			if (Pen == null) {
				Pen = new Pen2(Color);
			}
			Brush2 brush = new Brush2(Pen.Color);
			if (Location.X > 0 && Location.Y > 0) {
				StringBuilder desc = new StringBuilder();
				desc.Append("mass: " + MzValue);
				desc.Append("; deviation: " + NumUtils.RoundSignificantDigits2(Deviation, 4, 100) + " " +
				            DeviationUnit.ToString().ToLower());
				MainDesc(out string mainText, out Font2 mainFont1);
				SubDesc(out string subText, out Font2 subFont1);
				
                switch (PeakAnnotation.CurrentCrossType) {
					case 1:
						mainText =  mainText + 'α';
						break;
					case 2:
						mainText = mainText  + 'β';
						break;
				}

                switch (PeakAnnotation.CurrentCrossSpecificFragment) {
					case CrossLinkSpecificFragmentAnnotation.L:
                        mainText += 'L';
                        break;
                    case CrossLinkSpecificFragmentAnnotation.S:
                        mainText += 'S';
                        break;
                    case CrossLinkSpecificFragmentAnnotation.Pep:
                        mainText += "Pep";
                        break;
                    case CrossLinkSpecificFragmentAnnotation.ML:
                        mainText += "ML";
                        break;
				}

				Size2 s1 = mainText == null ? Size2.Empty : g.MeasureString(mainText, mainFont1);
				Size2 s2 = subText == null ? Size2.Empty : g.MeasureString(subText, subFont1);
				if (mainText != null) {
					Point2 pos = new Point2(Location.X + (Location.Width - s1.Width) * 0.5f, Location.Y);
					g.DrawString(mainText, mainFont1, brush, new Rectangle2(pos, s1));
					if (subText != null) {
						float de = s1.Height * 0.25f;
						//TODO: no references to specific implementations of IGraphics
						//#if (g is IPdfGraphics){
						//	de = -s1.Height*0.25f;
						//}
						pos = new Point2(Location.X + (Location.Width - s2.Width) * 0.5f, Location.Y + s1.Height - de);
						g.DrawString(subText, subFont1, brush, new Rectangle2(pos, s2));
					}
				} else {
					if (subText != null) {
						g.DrawString(subText, subFont1, brush, Location);
					}
				}
			}
		}

		private static readonly Font2 mainFont = new Font2("Lucida Sans Unicode", 10f, FontStyle2.Bold);
		private static readonly Font2 subFont = new Font2("Lucida Sans Unicode", 5f, FontStyle2.Regular);

		private void MainDesc(out string text, out Font2 font) {
			font = mainFont;
			text = RichText;
		}

		private void SubDesc(out string text, out Font2 font) {
			font = subFont;
			text = MzValue;
		}

		private static readonly Dictionary<string, Color2> defaultColors = GetDefaultColor();

		private static Dictionary<string, Color2> GetDefaultColor() {
			return new Dictionary<string, Color2> {
				{IonType.A.ToString(), Color2.SkyBlue},
				{IonType.B.ToString(), Color2.Blue},
				{IonType.C.ToString(), Color2.Turquoise},
				{IonType.X.ToString(), Color2.Green},
				{IonType.Y.ToString(), Color2.Red},
				{IonType.Z.ToString(), Color2.Red},
				{IonType.parent.ToString(), Color2.Aquamarine},
				{LossPeakAnnotation.type, Color2.Orange},
				{ImmoniumPeakAnnotation.type, Color2.MediumSeaGreen},
				{SideChainPeakAnnotation.type, Color2.LightSeaGreen},
				{InternalPeakAnnotation.type, Color2.BlueViolet},
				{DiagnosticPeakAnnotation.type, Color2.Pink}
			};
		}

		public static readonly Dictionary<string, Color2> ColorsCrossAlpha = GetColorCrossAlpha();
		private static Dictionary<string, Color2> GetColorCrossAlpha() {
			return new Dictionary<string, Color2> {
				{IonType.A.ToString(), Color2.Blue},
				{IonType.B.ToString(), Color2.Blue},
				{IonType.C.ToString(), Color2.Blue},
				{IonType.X.ToString(), Color2.Red},
				{IonType.Y.ToString(), Color2.Red},
				{IonType.Z.ToString(), Color2.Red},
				{IonType.parent.ToString(), Color2.Orange},
				{LossPeakAnnotation.type, Color2.Orange},
				{ImmoniumPeakAnnotation.type, Color2.Orange},
				{SideChainPeakAnnotation.type, Color2.Orange},
				{InternalPeakAnnotation.type, Color2.Orange},
				{DiagnosticPeakAnnotation.type, Color2.Orange}
			};
		}

		public static readonly Dictionary<string, Color2> ColorsCrossBeta = GetColorCrossBeta();
		private static Dictionary<string, Color2> GetColorCrossBeta() {
			return new Dictionary<string, Color2> {
				{IonType.A.ToString(), Color2.Green},
				{IonType.B.ToString(), Color2.Green},
				{IonType.C.ToString(), Color2.Green},
				{IonType.X.ToString(), Color2.DarkViolet},
				{IonType.Y.ToString(), Color2.DarkViolet},
				{IonType.Z.ToString(), Color2.DarkViolet},
				{IonType.parent.ToString(), Color2.Magenta},
				{LossPeakAnnotation.type, Color2.Magenta},
				{ImmoniumPeakAnnotation.type, Color2.Magenta},
				{SideChainPeakAnnotation.type, Color2.Magenta},
				{InternalPeakAnnotation.type, Color2.Magenta},
				{DiagnosticPeakAnnotation.type, Color2.Magenta}
			};
		}

		public static Color2 GetColor(Annotation a) {
			string type = GetType(a.PeakAnnotation);
			return !defaultColors.ContainsKey(type) ? Color2.Black : defaultColors[type];
		}

		public static Color2 GetColorCrossAlphaPeptide(Annotation a) {
			string type = GetType(a.PeakAnnotation);
			return !ColorsCrossAlpha.ContainsKey(type) ? Color2.Black : ColorsCrossAlpha[type];
		}

		public static Color2 GetColorCrossBetaPeptide(Annotation a) {
			string type = GetType(a.PeakAnnotation);
			return !ColorsCrossBeta.ContainsKey(type) ? Color2.Black : ColorsCrossBeta[type];
		}

		private static string GetType(PeakAnnotation pa) {
			if (pa is MsmsPeakAnnotation) {
				return pa.IonType.ToString();
			}
			if (pa is ImmoniumPeakAnnotation) {
				return ImmoniumPeakAnnotation.type;
			}
			if (pa is SideChainPeakAnnotation) {
				return SideChainPeakAnnotation.type;
			}
			if (pa is DiagnosticPeakAnnotation) {
				return DiagnosticPeakAnnotation.type;
			}
			if (pa is InternalPeakAnnotation) {
				return InternalPeakAnnotation.type;
			}
			if (pa is LossPeakAnnotation) {
				return LossPeakAnnotation.type;
			}
			return null;
		}
	}
}