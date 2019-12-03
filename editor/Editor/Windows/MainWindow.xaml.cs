using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Editor.Common;
using MahApps.Metro.Controls;
using Microsoft.Win32;

namespace Editor.Windows
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly MainWindowViewModel _viewModel;

        public MainWindow()
        {
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;

            InitializeComponent();

            _viewModel.Grid = this.McDataGrid;

            // style
            _viewModel.Styles.Add(typeof(DataGridCheckBoxColumn).Name, (Style)this.Resources["MahApps.Styles.CheckBox.DataGrid"]);
            _viewModel.Styles.Add(typeof(DataGridTextColumn).Name, (Style)this.Resources["MahApps.Styles.TextBox.DataGrid.Editing"]);
        }

        private void SetResOnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Filter = "all files(*.*)|*.*" };
            if (openFileDialog.ShowDialog() == true)
            {
                Global.GetInstance().ResPath = Path.GetDirectoryName(openFileDialog.FileName);
                this.StatusBarRes.Text = Global.GetInstance().ResPath;
            }
        }
    }
}
