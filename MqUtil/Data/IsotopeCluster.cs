using MqApi.Util;
namespace MqUtil.Data{
	public class IsotopeCluster{
		public int Charge { get; set; }
		public int[] Members { get; set; }
		public double IsotopeCorrelation { get; set; }
		public int IsotopePatternStart { get; set; }
		public double MassError { get; set; }
		public double Intensity { get; set; }
		public double RetentionTime { get; set; }
		public double RetentionTimeMin { get; set; }
		public double RetentionTimeMax { get; set; }
		public int MultipletIndex { get; set; }
		public short MultiplexWhich{ get; set; }
		public IsotopeCluster(int[] members, int charge){
			MultipletIndex = -1;
			MultiplexWhich = -1;
			Members = members;
			Charge = charge;
		}
		public IsotopeCluster(BinaryReader reader){
			Members = FileUtils.ReadInt32Array(reader);
			Charge = reader.ReadInt32();
			MultipletIndex = reader.ReadInt32();
			MultiplexWhich = reader.ReadInt16();
			IsotopePatternStart = reader.ReadInt32();
			MassError = reader.ReadDouble();
			IsotopeCorrelation = reader.ReadDouble();
			RetentionTime = reader.ReadDouble();
			Intensity = reader.ReadDouble();
			RetentionTimeMin = reader.ReadDouble();
			RetentionTimeMax = reader.ReadDouble();
		}
		public virtual void Write(BinaryWriter writer){
			FileUtils.Write(Members, writer);
			writer.Write(Charge);
			writer.Write(MultipletIndex);
			writer.Write(MultiplexWhich);
			writer.Write(IsotopePatternStart);
			writer.Write(MassError);
			writer.Write(IsotopeCorrelation);
			writer.Write(RetentionTime);
			writer.Write(Intensity);
			writer.Write(RetentionTimeMin);
			writer.Write(RetentionTimeMax);
		}
		/// <summary>
		/// The number of members in this cluster.
		/// </summary>
		public int Count => Members.Length;
		public int[] GetStandardMembers(double mass){
			return GetStandardMembers(mass, out int _);
		}
		/// <summary>
		/// Standard members are the 2 or 3 members with an index between startInd-IsotopePatternStart and 2-IsotopePatternStart, 
		/// where startInd is 0, except for high masses, when it is 1.
		/// </summary>
		/// <param name="mass"></param>
		/// <param name="offset"></param>
		/// <returns></returns>
		public int[] GetStandardMembers(double mass, out int offset){
			int startInd = mass > 2800 ? 1 : 0;
			List<int> result = new List<int>();
			int smallestInd = -1;
			for (int memberInd = 0; memberInd < Members.Length; memberInd++){
				int k = memberInd + IsotopePatternStart;
				if (k >= startInd && k < 3){
					if (smallestInd == -1){
						smallestInd = k;
					}
					result.Add(Members[memberInd]);
				}
			}
			offset = smallestInd - startInd;
			return result.ToArray();
		}
		public double RetentionTimeLength => RetentionTimeMax - RetentionTimeMin;
		public void Dispose(){
			Members = null;
		}
		public double GetMassError(AutocorrFitfunc factor, double m1){
			if (double.IsNaN(m1)){
				return double.NaN;
			}
			if (double.IsNaN(MassError)){
				return double.NaN;
			}
			return factor.Interpolate(MassError/m1*1e6)*MassError;
		}
	}
}