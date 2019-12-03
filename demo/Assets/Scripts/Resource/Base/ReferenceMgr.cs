using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GEngine
{
    abstract class IReferenceMgr
    {
        public abstract List<Reference> GetAll();
    }

    class ReferenceMgr<T> : IReferenceMgr where T : Reference, new()
    {
        protected readonly Dictionary<int, T> _maps = new Dictionary<int, T>();
        protected readonly Dictionary<string, int> _head = new Dictionary<string, int>();

        public void LoadFromFile(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var result = new byte[fs.Length];
                fs.Read(result, 0, result.Length);
                LoadFromMemory(new MemoryStream(result));
            }
        }

        private bool LoadHead(string line)
        {
            line = line.ToLower();
            string[] properties = line.Split('\t', ';', ',');
            for (int i = 0; i < properties.Length; i++)
            {
                _head.Add(properties[i], i);
            }

            if (string.Compare(properties[0], "ID", StringComparison.OrdinalIgnoreCase) != 0)
                return false;

            return true;
        }

        public void LoadFromMemory(MemoryStream ms)
        {
            _maps.Clear();

            bool isLoadedHead = false;
            StreamReader sr = new StreamReader(ms, Encoding.UTF8);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line == null)
                    break;

                if (line.Length == 0)
                    continue;

                if (!isLoadedHead)
                {
                    LoadHead(line);
                    isLoadedHead = true;
                    continue;
                }

                T obj = new T();
                obj.AttackHead(_head);
                if (!obj.LoadProperty(line))
                {
#if !Editor
                    GameLogger.GetInstance().Output($"!!!!! LoadProperty is error. line:{line}");
#endif
                    continue;
                }
                obj.LoadAfter();
                _maps.Add(obj.GetId(), obj);
            }

            OnAfterReload();
        }

        protected virtual void OnAfterReload() { }

        public T GetReference(int id)
        {
            if (!_maps.ContainsKey(id))
                return null;

            return _maps[id];
        }

        public override List<Reference> GetAll()
        {
            var rs = new List<Reference>();
            rs.AddRange(_maps.Values);
            return rs;
        }
    }

}