namespace MqApi.Drawing.Image.Formats{
	public interface IImageEncoder{
		int Quality{ get; set; }
		string MimeType{ get; }
		string Extension{ get; }
		bool IsSupportedFileExtension(string extension);
		void Encode(ImageBase image, Stream stream);
	}
}