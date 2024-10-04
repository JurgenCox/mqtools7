namespace MqUtil.Num{
	public class Random3{
		private readonly int[] values;
		private int c;
		private readonly object locker = new object();
		public Random3() : this(5000){}

		public Random3(int capacity){
			values = new int[capacity];
			Random r = new Random(7);
			for (int i = 0; i < capacity; i++){
				values[i] = r.Next();
			}
		}

		public int Next(){
			lock (locker){
				int v = values[c];
				c++;
				if (c == values.Length){
					c = 0;
				}
				return v;
			}
		}
	}
}