using MqApi.Generic;
using MqApi.Util;
namespace MqApi.Document{
	public interface IDocumentData : IDataWithAnnotationRows{
		List<string> Text{ get; set; }
		List<string> Header{ get; set; }
		List<DocumentType> Type{ get; set; }
		void AddTextBlock(string text);
		void AddTextBlock(string text, string header);
		void AddTextBlock(string text, string header, DocumentType type);
	}
}