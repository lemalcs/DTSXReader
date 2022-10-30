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

        private void btn_Set_Connection_Click(object sender, RoutedEventArgs e)
        {
            // Show dialog to configure connection to a SQL Server database.
            SQLServerConnectionWindow connectionWindow = new SQLServerConnectionWindow();
            connectionWindow.Owner = Application.Current.MainWindow;
            SQLServerAuthenticationViewModel authenticationViewModel = new SQLServerAuthenticationViewModel();
            connectionWindow.DataContext = authenticationViewModel;

            if (connectionWindow.ShowDialog().Value)
            {
                viewModel.ConnectionProperties = authenticationViewModel;
            }
        }

        private void hyperlink_Click_View_Table_Definition(object sender, RoutedEventArgs e)
        {
            // Show the dialog to view the SQL script of destination table for SQL Server.
            TableDefinitionWindow definitionWindow = new TableDefinitionWindow();
            definitionWindow.Owner = Application.Current.MainWindow;
            definitionWindow.ShowDialog();
        }
    }
}
