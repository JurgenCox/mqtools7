namespace MqApi.Num.Vector
{
	public class SparseFloatVectorDict : BaseVector
	{
		internal Dictionary<int, float> map;

		/// <summary>
		/// Total length of the vector.
		/// </summary>
		private int length;

		public SparseFloatVectorDict(IList<float> values)
		{
			Dictionary<int, float> newMap = new Dictionary<int, float>();
			for (int i = 0; i < values.Count; i++)
			{
				if (values[i] != 0)
				{
					newMap.Add(i, values[i]);
				}
			}

			map = newMap;
			length = values.Count;
		}

		public SparseFloatVectorDict()
		{
		}

		public SparseFloatVectorDict(Dictionary<int, float> map, int length)
		{
			this.map = map;
			this.length = length;
		}

		public override BaseVector Minus(BaseVector other)
		{
			throw new Exception("Never get here.");
		}

		public override BaseVector Plus(BaseVector other)
		{
			throw new Exception("Never get here.");
		}

		public override int Length => length;

		public override BaseVector Mult(double d)
		{
			throw new Exception("Never get here.");
		}

		public override BaseVector Copy()
		{
			throw new Exception("Never get here.");
		}

		public override BaseVector SubArray(IList<int> inds)
		{
			throw new Exception("Never get here.");
		}

		public override IEnumerator<double> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		public override void Read(BinaryReader reader)
		{
			length = reader.ReadInt32();
			int n = reader.ReadInt32();
			map = new Dictionary<int, float>();
			for (int i = 0; i < n; i++)
			{
				int key = reader.ReadInt32();
				float value = reader.ReadSingle();
				map.Add(key, value);
			}
		}

		public override void Write(BinaryWriter writer)
		{
			writer.Write(length);
			writer.Write(map.Count);
			foreach (KeyValuePair<int, float> pair in map)
			{
				writer.Write(pair.Key);
				writer.Write(pair.Value);
			}
		}

		public override VectorType GetVectorType()
		{
			return VectorType.SparseFloat;
		}

		public override bool ContainsNaNOrInf()
		{
			return false;
		}

		public override double[] Unpack()
		{
			return ArrayUtils.ToDoubles(this);
		}

		public override void Dispose()
		{
			map = null;
		}

		public override double this[int i]
		{
			get
			{
				if (map.ContainsKey(i))
				{
					return map[i];
				}

				return 0;
			}
			set { map[i] = (float)value; }
		}

		public override double Dot(BaseVector y)
		{
			throw new Exception("Never get here.");
		}

		public override double SumSquaredDiffs(BaseVector y)
		{
			throw new Exception("Never get here.");
		}

		public override bool IsNaNOrInf()
		{
			return false;
		}
	}
}