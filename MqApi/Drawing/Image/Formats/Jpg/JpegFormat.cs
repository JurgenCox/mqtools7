namespace MqApi.Drawing.Image.Formats.Jpg{
	public class JpegFormat : IImageFormat{
		public IImageDecoder Decoder => new JpegDecoder();
		public IImageEncoder Encoder => new JpegEncoder();
	}
}