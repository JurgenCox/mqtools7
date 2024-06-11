namespace MqApi.Drawing.Image.Formats{
	public interface IImageDecoder{
		int HeaderSize{ get; }
		bool IsSupportedFileExtension(string extension);
		bool IsSupportedFileFormat(byte[] header);
		void Decode(Image2 image, Stream stream);
	}
}