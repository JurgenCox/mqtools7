namespace MqApi.Drawing.Image.Formats.Bmp{
	public class BmpFormat : IImageFormat{
		public IImageDecoder Decoder => new BmpDecoder();
		public IImageEncoder Encoder => new BmpEncoder();
	}
}