using MqApi.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace MqUtil.Ms.Data
{
    public class PpiInfo
    {
        private readonly HashSet<Tuple<string, string>> keys = new HashSet<Tuple<string, string>>();
        private readonly string filePath;
        private BinaryWriter writer;
        public PpiInfo(string combinedFolder)
        {
            filePath = Path.Combine(combinedFolder, "ppiInfoFile");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            writer = FileUtils.GetBinaryWriter(filePath);
            writer.Write(0);
        }
        public void Add(Tuple<string, string> key, string[] value1, string[] value2)
        {
            if (!keys.Contains(key))
            {
                keys.Add(key);
				writer.Write(key.Item1);
                writer.Write(key.Item2);
                FileUtils.Write(value1, writer);
				FileUtils.Write(value2, writer);
            }
        }
        public void Finish()
        {
            writer.BaseStream.Seek(0, SeekOrigin.Begin);
            writer.Write(keys.Count);
            writer.Close();
            writer = null;
        }
        public bool ContainsKey(Tuple<string, string> key)
        {
            return keys.Contains(key);
        }
        public Dictionary<Tuple<string, string>, Dictionary<Tuple<string, string>, bool>>
            Contains(Dictionary<Tuple<string, string>, HashSet<Tuple<string, string>>> map)
        {
			Dictionary<Tuple<string, string>, Dictionary<Tuple<string, string>, bool>> result = 
                new Dictionary<Tuple<string, string>, Dictionary<Tuple<string, string>, bool>>();
            BinaryReader reader = FileUtils.GetBinaryReader(filePath);
            int n = reader.ReadInt32();
            for (int i = 0; i < n; i++)
            {
                string protId1 = reader.ReadString();
				string protId2 = reader.ReadString();
                Tuple<string, string> protIds = new Tuple<string, string>(protId1, protId2);
				Tuple<string, string> protIds2 = new Tuple<string, string>(protId1, protId2);
                string[] peptides1 = FileUtils.ReadStringArray(reader);
				string[] peptides2 = FileUtils.ReadStringArray(reader);
                if (!map.ContainsKey(protIds)){
					if(!map.ContainsKey(protIds2)){
						continue;
					}
					protIds = protIds2;
					(peptides1, peptides2) = (peptides2, peptides1);
                }
                result.Add(protIds, new Dictionary<Tuple<string, string>, bool>());
                Dictionary<Tuple<string, string>, bool> x = result[protIds];
                HashSet<Tuple<string, string>> peptideSearch = map[protIds];
                foreach (Tuple<string, string> s in peptideSearch)
                {
                    bool contains1 = Array.BinarySearch(peptides1, s.Item1) >= 0;
                    bool contains2 = Array.BinarySearch(peptides2, s.Item2) >= 0;
                    x.TryAdd(s, contains1&&contains2);
                    
                }

            }
            reader.Close();
            return result;
        }
    }
}
