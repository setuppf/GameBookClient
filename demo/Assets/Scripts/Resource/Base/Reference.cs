using System.Collections.Generic;

namespace GEngine
{
    abstract class Reference
    {
        protected List<string> _values = new List<string>();
        protected Dictionary<string, int> _head;
        protected int _id = 0;

        public static string InValid = "-1";


        public void AttackHead(Dictionary<string, int> head)
        {
            this._head = head;
        }

        public abstract void LoadAfter();

        public int GetId()
        {
            return _id;
        }

        public bool LoadProperty(string line)
        {
            string[] properties = CvsAnalysis.GetInstance().GetProperty(line);

            if (properties.Length != _head.Count)
            {
#if !Editor
                GameLogger.GetInstance( ).Output( "!!!!! load file is error." );
#endif
                return false;
            }

            foreach (var one in properties)
            {
                _values.Add(one);
            }

            _id = int.Parse(_values[0]);

            return true;
        }

        public bool GetBool(string name)
        {
            name = name.ToLower();

            if (!_head.ContainsKey(name))
            {
#if !Editor
                GameLogger.GetInstance( ).Output( "!!!!! GetBool is error. name:" + name );
#endif
                return false;
            }

            return int.Parse(_values[_head[name]]) >= 1;
        }

        public int GetInt(string name)
        {
            name = name.ToLower();
            if (!_head.ContainsKey(name))
            {
#if !Editor
                GameLogger.GetInstance( ).Output( "!!!!! GetInt is error. name:" + name );
#endif
                return 0;
            }

            return int.Parse(_values[_head[name]]);
        }

        public string GetString(string name)
        {
            name = name.ToLower();
            if (!_head.ContainsKey(name))
            {
#if !Editor
                GameLogger.GetInstance( ).Output( "!!!!! GetInt is error. name:" + name );
#endif
                return "";
            }

            return _values[_head[name]];
        }
    }

}