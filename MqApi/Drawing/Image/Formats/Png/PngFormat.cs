namespace MqApi.Drawing.Image.Formats.Png{
	public class PngFormat : IImageFormat{
		public IImageDecoder Decoder => new PngDecoder();
		public IImageEncoder Encoder => new PngEncoder();
	}
}