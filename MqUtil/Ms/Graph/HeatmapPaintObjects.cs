using MqUtil.Symbol;
namespace MqUtil.Ms.Graph{
	public class HeatmapPaintObjects{
		public List<Symbol> Symbols{ get; set; } = new List<Symbol>();
		public List<Rectangle> Rectangles{ get; set; } = new List<Rectangle>();
		public List<Line> Lines{ get; set; } = new List<Line>();
		public void AddSymbol(SymbolType symbolType, int symbolSize, int argb, double x, double y) =>
			Symbols.Add(new Symbol(symbolType, symbolSize, argb, x, y));
		public void AddRectangle(int argb, double x, double y, double wid, double height) =>
			Rectangles.Add(new Rectangle(argb, x, y, wid, height));
		public void AddLine(int argb, double x1, double y1, double x2, double y2, bool dots, int width1)
			=> Lines.Add(new Line(argb, x1, y1, x2, y2, dots, width1));
		public void AddPaintObjects(HeatmapPaintObjects otherObject){
			Symbols.AddRange(otherObject.Symbols);
			Rectangles.AddRange(otherObject.Rectangles);
			Lines.AddRange(otherObject.Lines);
		}
		public class Symbol{
			public readonly SymbolType symbolType;
			public readonly int symbolSize;
			public readonly int argb;
			public readonly double x;
			public readonly double y;
			public Symbol(SymbolType symbolType, int symbolSize, int argb, double x, double y){
				this.symbolType = symbolType;
				this.symbolSize = symbolSize;
				this.argb = argb;
				this.x = x;
				this.y = y;
			}
		}
		public class Rectangle{
			public readonly int argb;
			public readonly double x;
			public readonly double y;
			public readonly double wid;
			public readonly double height;
			public Rectangle(int argb, double x, double y, double wid, double height){
				this.argb = argb;
				this.x = x;
				this.y = y;
				this.wid = wid;
				this.height = height;
			}
		}
		public class Line{
			public readonly int argb;
			public readonly double x1;
			public readonly double y1;
			public readonly double x2;
			public readonly double y2;
			public readonly bool dots;
			public readonly int width1;
			public Line(int argb, double x1, double y1, double x2, double y2, bool dots, int width1){
				this.argb = argb;
				this.x1 = x1;
				this.y1 = y1;
				this.x2 = x2;
				this.y2 = y2;
				this.dots = dots;
				this.width1 = width1;
			}
		}
	}
}