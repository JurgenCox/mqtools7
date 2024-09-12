namespace MqUtil.Base {
	public interface IFileIndexProvider {
		HashSet<int> GetFileIndices(string exp);
		HashSet<int> GetFileIndicesNoPtm(string exp);
	}
}
