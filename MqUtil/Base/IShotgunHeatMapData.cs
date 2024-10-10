using MqUtil.Parse.Uniprot;
namespace MqUtil.Base{
	public interface IShotgunHeatMapData{
		object[] GetProteinGroupTableRow(long row);
		object[] GetMsScansTableRow(long row);
		object[] GetMsmsTableRow(long row);
		object[] GetSummaryTableRow(long row);
		object[] GetMzRangeTableRow(long row);
		object[] GetAllPeptideTableRow(long row);
		object[] GetAllPeptideTimsTableRow(long row);
		object[] GetEvidenceTableRow(long row);
		object[] GetLibraryMatchTableRow(long row);
		object[] GetMatchedFeaturesTableRow(long row);
		object[] GetModifiedPeptideTableRow(long row);
		object[] GetMs3ScansTableRow(long row);
		object[] GetMsmsScansTableRow(long row);
		object[] GetPasefMsmsScansTableRow(long row);
		object[] GetAccumulatedMsmsScansTableRow(long row);
		object[] GetPeptideTableRow(long row);
		object[] GetParametersTableRow(long row);
		long GetProteinGroupTableCount();
		long GetMzRangeTableCount();
		long GetAllPeptideTableCount();
		long GetAllPeptideTimsTableCount();
		long GetEvidenceTableCount();
		long GetLibraryMatchTableCount();
		long GetMatchedFeaturesTableCount();
		long GetModifiedPeptideTableCount();
		long GetMs3ScansTableCount();
		long GetMsmsScansTableCount();
		long GetPasefMsmsScansTableCount();
		long GetAccumulatedMsmsScansTableCount();
		long GetPeptideTableCount();
		long GetParametersTableCount();
		long GetSummaryTableCount();
		long GetMsmsTableCount();
		long GetMsScansTableCount();
		object[] GetSiteTableRow(long row, string modName, int imod);
		long GetSiteTableCount(int imod);
		IList<double> CalcShotgunMzTracesForEvidence(int evidenceId, int selectedFileIndex, bool positive);
		string GetPeptideSequenceAt(int pepId);
		bool PeptideIsUniqueGroup(int pepId);
		bool PeptideIsUniqueProtein(int pepId);
		BasicParams GetMqParams();
		FeatureType[] GetAllFeatureTypes();
	}
}