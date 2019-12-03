using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GEngine;

namespace Editor.Common
{
    class Global : SingletonObject<Global>
    {
        public string ResPath { get; set; }

        public string GetReferencePath()
        {
            return ResPath;
        }
    }
}
