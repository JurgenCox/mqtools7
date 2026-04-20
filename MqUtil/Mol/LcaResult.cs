namespace MqUtil.Mol{
	public readonly struct LcaResult{
		public readonly int Taxid;
		public readonly int[] UnknownTaxids;

		public LcaResult(int taxid, int[] unknownTaxids){
			Taxid = taxid;
			UnknownTaxids = unknownTaxids;
		}
	}
}
