namespace MqApi.Drawing.Image{
	public interface IPixelAccessor : IDisposable{
		Color2 this[int x, int y]{ get; set; }
		int Width{ get; }
		int Height{ get; }
	}
}