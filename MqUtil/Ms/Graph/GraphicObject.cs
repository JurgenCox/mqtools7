using MqUtil.Drawing;
namespace MqUtil.Ms.Graph{
	public abstract class GraphicObject{
		public Pen2 Pen { get; set; }
		protected GraphicObject(Pen2 pen){
			Pen = pen;
		}
		public GraphicsPath2 Path { get; } = new GraphicsPath2();
	}
}