using MqApi.Param;
namespace MqUtil.Ms{
	[Serializable]
	public abstract class LcmsRunType{
		public abstract Parameters GetParameters();
		public abstract string Name{ get; }
		public abstract bool UsesLibrary{ get; }
		public abstract bool IsTims{ get; }
		public abstract bool IsFaims{ get; }
		public abstract bool IsDia{ get; }
		public abstract bool IsReporterQuant{ get; }
		public abstract bool IsReporterQuantMs2{ get; }
		public abstract bool IsReporterQuantMs3{ get; }
		public abstract bool IsBoxcar{ get; }
		protected bool Equals(LcmsRunType other){
			return Name.Equals(other.Name);
		}
		public override int GetHashCode(){
			return Name.GetHashCode();
		}
		public override bool Equals(object obj){
			return obj is LcmsRunType && Equals((LcmsRunType) obj);
		}
	}
}