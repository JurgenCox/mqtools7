namespace MqApi.Generic{
	public interface IMergeWithMatrix : IActivityWithHeading{
		string HelpOutput{ get; }
		string[] HelpSupplTables{ get; }
		int NumSupplTables{ get; }
		string[] HelpDocuments{ get; }
		int NumDocuments{ get; }
	}
}