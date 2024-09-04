using MqUtil.Drawing;
namespace MqUtil.Symbol{
	public class SymbolTypeHorizontalDash : SymbolType{
		public SymbolTypeHorizontalDash(int index) : base(index) {}
		public override string Name => "Horizontal dash";

		public override void GetPath(int size, out int[] pathX, out int[] pathY){
			int s2 = size/2;
			List<int> x = new List<int>();
			List<int> y = new List<int>();
			for (int i = -s2; i <= s2; i++){
				x.Add(i);
				y.Add(0);
			}
			pathX = x.ToArray();
			pathY = y.ToArray();
		}

		public override void Draw(int size, float x, float y, IGraphics g, Pen2 pen, Brush2 brush){
			int s2 = size/2;
			g.DrawLine(pen, x - s2, y, x + s2, y);
		}
	}
}