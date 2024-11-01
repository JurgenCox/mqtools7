namespace MqUtil.Ms.Search {
	/// <summary>
	/// Low level andromeda score calculation.
	/// Caches score results for fast retrieval for integer k.
	/// </summary>
	public interface ICachedAndromedaScore {
		/// <summary>
		/// TopxWindow size used in the cache.
		/// </summary>
		double TopxWindow { get; }

		/// <summary>
		/// Calculate score. Should be backed by cache.
		/// </summary>
		double ScoreFunc(int n, int k, int topx);

		/// <summary>
		/// Calculate best score for all choices of <param name="k">k</param>.
		/// </summary>
		double ScoreFunc(int n, int[] k, double precursorMass, bool positioning);
	}
}