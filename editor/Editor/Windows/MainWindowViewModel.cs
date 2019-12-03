using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Editor.Common;
using GEngine;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Editor.Windows
{
    public class TablesMenuData
    {
        public string Name { get; set; }
        public ITableType TableType { get; set; }

        public Brush ColorBrush { get; set; }
    }

    public class MainWindowViewModel : ViewModelBase, IDataErrorInfo, IDisposable
    {
        public string this[string columnName]
        {
            get { return null; }
        }

        public string Error => string.Empty;

        public ICommand GenreDropDownMenuItemCommand { get; }
        public ICommand ArtistsDropDownCommand { get; }

        public List<TablesMenuData> Resources { get; set; }

        public DataGrid Grid;
        private List<Reference> _gridData = new List<Reference>();

        public Dictionary<string, Style> Styles = new Dictionary<string, Style>();

        public MainWindowViewModel()
        {
            LoadResourceMenus();

            this.GenreDropDownMenuItemCommand = new SimpleCommand(
                o => true,
                this.LoadTable
                );

            this.ArtistsDropDownCommand = new SimpleCommand(o => false);
        }

        private async void LoadTable(object obj)
        {
            if (string.IsNullOrEmpty(Global.GetInstance().ResPath))
            {
                await ((MetroWindow)Application.Current.MainWindow).ShowMessageAsync("Error", $"资源路径不对：{Global.GetInstance().ResPath}");
                return;
            }

            ITableType typeObj = obj as ITableType;
            if (typeObj == null)
                return;

            var resDes = ResourceHelper.GetReferenceHelper(typeObj);
            foreach (var field in resDes.Fields)
            {
                if (field.Type.IsEnum)
                {
                    var column = new DataGridComboBoxColumn
                    {
                        Header = field.ColumnName,
                        Width = field.Width,
                        SelectedValueBinding = new Binding(field.FieldName),
                    };

                    var enumDesc = EnumHelper.GetEnumDescription(field.Type);
                    column.ItemsSource = enumDesc.Enums;
                    column.SelectedValuePath = "Value";
                    column.DisplayMemberPath = "Display";

                    Grid.Columns.Add(column);
                }
                else if (field.Type == typeof(bool))
                {
                    var column = new DataGridCheckBoxColumn()
                    {
                        Header = field.ColumnName,
                        Width = field.Width,
                        Binding = new Binding(field.FieldName),
                    };

                    if (Styles.TryGetValue(typeof(DataGridCheckBoxColumn).Name, out var style))
                    {
                        column.ElementStyle = style;
                        column.EditingElementStyle = style;
                    }

                    Grid.Columns.Add(column);
                }
                else
                {
                    var column = new DataGridTextColumn
                    {
                        Header = field.ColumnName,
                        Width = field.Width,
                        Binding = new Binding(field.FieldName),
                    };

                    if (Styles.TryGetValue(typeof(DataGridTextColumn).Name, out var style))
                        column.EditingElementStyle = style;

                    Grid.Columns.Add(column);
                }

            }

            if (!ResourceAll.IsInstance())
            {
                ResourceAll.GetInstance().Init();
            }

            IReferenceMgr resMgr = null;
            if (typeObj.TypeName == typeof(ResourceWorld))
            {
                resMgr = ResourceAll.GetInstance().MapMgr;
            }

            if (resMgr == null)
                return;

            _gridData = resMgr.GetAll();
            Grid.ItemsSource = _gridData;
            Grid.Visibility = Visibility.Visible;
        }

        private void LoadResourceMenus()
        {
            Resources = new List<TablesMenuData>();
            foreach (var table in TableTypeDefine.Tables)
            {
                var tableObj = new TablesMenuData()
                {
                    Name = table.DesName,
                    TableType = table,
                    ColorBrush = table.ColorBrush,
                };
                
                Resources.Add(tableObj);
            }
        }

        public void Dispose()
        {
        }
    }
}
