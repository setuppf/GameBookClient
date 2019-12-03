using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using GEngine;

namespace Editor
{
    class EnumHelper
    {
        private static readonly Dictionary<string, EnumDescription> dict_ = new Dictionary<string, EnumDescription>();

        public static EnumDescription GetEnumDescription(Type type)
        {
            if (dict_.TryGetValue(type.Name, out var result))
            {
                return result;
            }

            result = new EnumDescription();
            result.Init(type);
            dict_.Add(type.Name, result);
            return result;
        }
    }

    class EnumInfo
    {
        public string Display { get; set; }
        public int Value { get; set; }
    }

    class EnumDescription
    {
        public List<EnumInfo> Enums = new List<EnumInfo>();
        public void Init(Type type)
        {
            //遍历所有自定义属性
            foreach (FieldInfo fi in type.GetFields())
            {
                EditorEnumAttribute att = Attribute.GetCustomAttribute(fi, typeof(EditorEnumAttribute)) as EditorEnumAttribute;
                if (att == null)
                    continue;

                int value = (int)fi.GetValue(null);
                var info = new EnumInfo { Display = att.Name, Value = value };
                Enums.Add(info);
            }
        }
    }
}
