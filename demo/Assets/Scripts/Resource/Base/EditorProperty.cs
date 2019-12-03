using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GEngine
{

    [System.AttributeUsage(System.AttributeTargets.Property)]
    public partial class ColumnAttribute : System.Attribute
    {

        public string Comment;      // 注释
        public int Width;
        public string StructType;

        public ColumnAttribute(string comment, int width)
        {
            this.Comment = comment;
            this.Width = width;
            this.StructType = string.Empty;
        }

        public ColumnAttribute(string comment, int width, string structType)
        {
            this.Comment = comment;
            this.Width = width;
            this.StructType = structType;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class EditorEnumAttribute : System.Attribute
    {
        public EditorEnumAttribute(string name)
            : this(0, name, String.Empty)
        {
        }

        public EditorEnumAttribute(int id, string name)
            : this(id, name, String.Empty)
        {
        }

        public EditorEnumAttribute(string name, string display)
            : this(0, name, display)
        {
        }

        public EditorEnumAttribute(int id, string name, string display)
        {
            this.id = id;
            this.name = name;
            this.display = display;
        }

        public string Name { get { return name; } }
        public string Comment { get { return name; } }
        public string Display { get { return display; } }
        public int ID { get { return id; } }

        private int id;
        private string name;
        private string display;      //客户端用于显示的自定义字符串
    }

}
