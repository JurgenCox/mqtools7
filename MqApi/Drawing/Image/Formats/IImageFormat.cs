namespace MqApi.Drawing.Image.Formats{
	public interface IImageFormat{
		IImageEncoder Encoder{ get; }
		IImageDecoder Decoder{ get; }
	}
}