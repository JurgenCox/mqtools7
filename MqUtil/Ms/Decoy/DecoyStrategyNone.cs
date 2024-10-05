namespace MqUtil.Ms.Decoy{
	public class DecoyStrategyNone : DecoyStrategy{
		public DecoyStrategyNone() : base(""){
		}
		public override string ProcessProtein(string protSeq, bool isCodon){
			throw new System.NotImplementedException();
		}
		public override string ProcessVariation(string mutaions, string protSeq, bool isCodon){
			throw new System.NotImplementedException();
		}
		public override string ProcessPeptide(string pepSeq){
			throw new System.NotImplementedException();
		}
		public override DecoyMode DecoyMode{ get; }
		public override int GetHashCode(){
			unchecked{
				return ((specialAas != null ? MqUtil.Util.HashCode.GetDeterministicHashCode(specialAas) : 3) * 397 +
				        868);
			}
		}
	}
}