using System;
using System.Collections.Generic;
using System.Reflection;
using Editor.Common;
using GEngine;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Editor
{
    class ResourceHelper : SingletonObject<ResourceHelper>
    {
        private static readonly Dictionary<string, ResourceDescription> _tables = new Dictionary<string, ResourceDescription>();

        public static ResourceDescription GetReferenceHelper(ITableType tableType)
        {
            if (!_tables.TryGetValue(tableType.FileName, out var rh))
            {
                rh = new ResourceDescription(tableType);
                _tables.Add(tableType.FileName, rh);
            }
            return rh;
        }
    }

    class ResourceDescription
    {
        public readonly List<FieldDefine> Fields = new List<FieldDefine>();
        public readonly string ResourceName;

        public  ResourceDescription(ITableType tableType)
        {
            var type = tableType.TypeName;
            this.ResourceName = type.Name;

            //遍历所有自定义属性
            foreach (var one in type.GetProperties())
            {
                ObsoleteAttribute obsolete = Attribute.GetCustomAttribute(one, typeof(ObsoleteAttribute)) as ObsoleteAttribute;
                ColumnAttribute att = Attribute.GetCustomAttribute(one, typeof(ColumnAttribute)) as ColumnAttribute;
                if (att == null)
                    continue;

                FieldDefine rf = GetPropertyInfo(one, att, obsolete);
                if (rf == null)
                    continue;

                Fields.Add(rf);
            }
        }

        private FieldDefine GetPropertyInfo(PropertyInfo pi, ColumnAttribute att, ObsoleteAttribute obsolete)
        {
            FieldDefine rh = new FieldDefine
            {
                FieldName = pi.Name, ColumnName = att.Comment, Width = att.Width, Type = pi.PropertyType
            };

            if (!string.IsNullOrEmpty(att.StructType))
            {
                rh.Type = Type.GetType(att.StructType);
            }

            return rh;
        }
    }

    class FieldDefine
    {
        public string ColumnName { get; set; } // 编辑器中显示的列表

        public string FieldName { get; set; } // 在csv中的列名

        public int Width { get; set; }

        public Type Type { get; set; }
    }
}
