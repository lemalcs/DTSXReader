using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;

namespace DTSXExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ReaderViewModel viewModel;
        public MainWindow()
        {
            InitializeComponent();
            viewModel = new ReaderViewModel();
            DataContext = viewModel;
        }

        private void btn_Browse_Source_Path_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.MultipleFiles)
            {
                viewModel.SourcePath = OpenFolderPicker();
            }
            else
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog().Value)
                {
                    viewModel.SourcePath = openFileDialog.FileName;
                }
            }
        }

        private void btn_Browse_Destination_Path_Click(object sender, RoutedEventArgs e)
        {
            viewModel.DestinationPath = OpenFolderPicker() ?? viewModel.DestinationPath;
        }

        /// <summary>
        /// Opens a dialog to pick a folder. 
        /// </summary>
        /// <returns>The path of select folder.</returns>
        private string OpenFolderPicker()
        {
            CommonOpenFileDialog openFileDialog = new CommonOpenFileDialog();
            openFileDialog.IsFolderPicker = true;
            if (openFileDialog.ShowDialog() == CommonFileDialogResult.Ok)
                return openFileDialog.FileName;
            return null;
        }
    }
}
