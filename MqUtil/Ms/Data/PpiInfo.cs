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
        public void Add(Tuple<string, string> key, Tuple<string, string>[] value)
        {
            if (!keys.Contains(key))
            {
                keys.Add(key);
                string key_merged = key.Item1 + "_" + key.Item2;
                writer.Write(key_merged);
                string[] value_merged = new string[value.Length];
                for(int i = 0; i < value.Length; i++)
                {
                    value_merged[i] = value[i].Item1 + "_" + value[i].Item2;
                }
                FileUtils.Write(value_merged, writer);
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
                string protIdMerged = reader.ReadString();
                string[] parts = protIdMerged.Split('_');
                Tuple<string, string> protId = new Tuple<string, string>(parts[0], parts[1]);
                string[] peptidesMerged = FileUtils.ReadStringArray(reader);
                Tuple<string, string>[] peptides = new Tuple<string, string>[peptidesMerged.Length];
                for (int j =0; j < peptidesMerged.Length; j++)
                {
                    string[] peptideParts = peptidesMerged[j].Split('_');
                    peptides[j] = new Tuple<string, string>(peptideParts[0], peptideParts[1]);
                }
                if (map.ContainsKey(protId))
                {
                    result.Add(protId, new Dictionary<Tuple<string, string>, bool>());
                    Dictionary<Tuple<string, string>, bool> x = result[protId];
                    HashSet<Tuple<string, string>> peptideSearch = map[protId];
                    foreach (Tuple<string, string> s in peptideSearch)
                    {
                        bool contains = Array.BinarySearch(peptides, s) >= 0;
                        x.Add(s, contains);
                    }
                }
            }
            reader.Close();
            return result;
        }
    }
}
