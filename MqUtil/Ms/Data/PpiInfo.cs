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
                string key_merged = key.Item1 + "_" + key.Item2;
                writer.Write(key_merged);
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
        public Dictionary<Tuple<string, string>, Dictionary<string, bool>>
            Contains(Dictionary<Tuple<string, string>, HashSet<Tuple<string, string>>> map)
        {
			Dictionary<Tuple<string, string>, Dictionary<string, bool>> result = 
                new Dictionary<Tuple<string, string>, Dictionary<string, bool>>();
            BinaryReader reader = FileUtils.GetBinaryReader(filePath);
            int n = reader.ReadInt32();
            for (int i = 0; i < n; i++)
            {
                string protIdMerged = reader.ReadString();
                string[] parts = protIdMerged.Split('_');
                Tuple<string, string> protId = new Tuple<string, string>(parts[0], parts[1]);
                string[] peptides1 = FileUtils.ReadStringArray(reader);
				string[] peptides2 = FileUtils.ReadStringArray(reader);
                if (map.ContainsKey(protId))
				{
					result.Add(protId, new Dictionary<string, bool>());
					Dictionary<string, bool> x = result[protId];
					HashSet<Tuple<string, string>> peptideSearch = map[protId];
					foreach (Tuple<string, string> s in peptideSearch)
					{
						foreach (string pep in new[] { s.Item1, s.Item2 }){
							bool contains = Array.BinarySearch(peptides1, s) >= 0||
											Array.BinarySearch(peptides2, s) >= 0;
                            x.Add(pep, contains);
                        }
					}
				}
				
            }
            reader.Close();
            return result;
        }
    }
}
