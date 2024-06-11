namespace MqApi.Drawing.Image.Formats.Png{
	internal sealed class PngColorTypeInformation{
		public PngColorTypeInformation(int scanlineFactor, int[] supportedBitDepths,
			Func<byte[], byte[], IColorReader> scanlineReaderFactory){
			ChannelsPerColor = scanlineFactor;
			ScanlineReaderFactory = scanlineReaderFactory;
			SupportedBitDepths = supportedBitDepths;
		}
		public int[] SupportedBitDepths{ get; private set; }
		public Func<byte[], byte[], IColorReader> ScanlineReaderFactory{ get; }
		public int ChannelsPerColor{ get; private set; }
		public IColorReader CreateColorReader(byte[] palette, byte[] paletteAlpha){
			return ScanlineReaderFactory(palette, paletteAlpha);
		}
	}
}