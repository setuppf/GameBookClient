using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using GEngine;

namespace Editor.Common
{
    public class ITableType
    {
        public string FileName;
        public string DesName;
        public Type TypeName;
        public Brush ColorBrush;
    }

    public class TableType<T> : ITableType
    {
        public TableType(string name, string fileName, Brush color)
        {
            FileName = fileName;
            DesName = name;
            TypeName = typeof(T);
            ColorBrush = color;
        }
    }

    public static class TableTypeDefine
    {
        // <表名,显示名>
        public static List<ITableType> Tables = new List<ITableType>();

        static TableTypeDefine()
        {
            Tables.Add(new TableType<ResourceWorld>("地图", "World", Brushes.BlueViolet));
        }
    }
}
