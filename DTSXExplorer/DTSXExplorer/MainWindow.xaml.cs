using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.ComponentModel;
using System.Windows;

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
            openFileDialog.IsFolderPicker = true;
            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
                viewModel.DestinationPath = openFileDialog.FileName;
        }
    }
}
