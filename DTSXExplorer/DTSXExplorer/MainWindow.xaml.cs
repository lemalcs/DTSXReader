using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace DTSXExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BackgroundWorker worker;
        ReaderViewModel viewModel;
        public MainWindow()
        {
            InitializeComponent();
            worker = new BackgroundWorker();
            viewModel = new ReaderViewModel();
            DataContext = viewModel;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog().Value)
            {
                viewModel.SourcePath = openFileDialog.FileName;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog();
            openFileDialog.IsFolderPicker=true;
            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
                viewModel.DestinationPath = openFileDialog.FileName;
        }
    }
}
