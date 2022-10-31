using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using DTSXDumper;

namespace DTSXExplorer
{
    /// <summary>
    /// The view model for DTSX reader main window.
    /// </summary>
    internal class ReaderViewModel : BaseModel, IExporterObserver
    {
        #region Properties

        /// <summary>
        /// The path of DTSX file or the path of folder containing DTSX files.
        /// </summary>
		private string sourcePath;

        /// <summary>
        /// The path of folder where exported file will be written.
        /// </summary>
		private string destinationPath;

        /// <summary>
        /// Read files with a background process.
        /// </summary>
        private BackgroundWorker worker = null;

        /// <summary>
        /// The description of current activity being performed.
        /// </summary>
		private string resultMessage;

        /// <summary>
        /// Indicates whether to process a single DTSX file.
        /// </summary>
		private bool singleFile;

        /// <summary>
        /// Indicates whether to process multiple DTSX files.
        /// </summary>
		private bool multipleFiles;

        /// <summary>
        /// Indicates whether there is a currently reading operation.
        /// </summary>
        private bool isReading;

        /// <summary>
        /// Destination type for DTSX files.
        /// </summary>
        private ExportDestination destinationType;

        private ObservableCollection<string> scriptFilePaths;

        /// <summary>
        /// The connection properties used to connect to a SQL Server instance.
        /// </summary>
        private SQLServerAuthenticationViewModel connectionProperties;

        public string SourcePath
        {
            get => sourcePath;
            set
            {
                if (sourcePath != value)
                {
                    sourcePath = value;
                    OnPropertyChanged(nameof(SourcePath));
                }
            }
        }
        public string DestinationPath
        {
            get => destinationPath;
            set
            {
                if (destinationPath != value)
                {
                    destinationPath = value;
                    OnPropertyChanged(nameof(DestinationPath));
                }
            }
        }

        public string ResultMessage
        {
            get => resultMessage;
            set
            {
                if (resultMessage != value)
                {
                    resultMessage = value;
                    OnPropertyChanged(nameof(ResultMessage));
                }
            }
        }

        public bool SingleFile
        {
            get => singleFile;
            set
            {
                if (singleFile != value)
                {
                    singleFile = value;
                    OnPropertyChanged(nameof(SingleFile));
                }
            }
        }
        public bool MultipleFiles
        {
            get => multipleFiles;
            set
            {
                if (multipleFiles != value)
                {
                    multipleFiles = value;
                    OnPropertyChanged(nameof(MultipleFiles));
                }
            }
        }
        public bool IsReading
        {
            get => isReading;
            set
            {
                if (isReading != value)
                {
                    isReading = value;
                    OnPropertyChanged(nameof(IsReading));
                };
            }
        }

        public ObservableCollection<string> ScriptFilePaths
        {
            get => scriptFilePaths;
            private set
            {
                if (scriptFilePaths != value)
                {
                    scriptFilePaths = value;
                    OnPropertyChanged(nameof(ScriptFilePaths));
                }
            }
        }

        public ExportDestination DestinationType
        {
            get => destinationType;
            set
            {
                if (destinationType != value)
                {
                    destinationType = value;
                    OnPropertyChanged(nameof(DestinationType));

                    // Clear the destination field when destination type is changed
                    // in order to set a new value
                    DestinationPath = string.Empty;
                    ConnectionProperties = null;
                }
            }
        }
        public SQLServerAuthenticationViewModel ConnectionProperties
        {
            get => connectionProperties;
            set
            {
                if (connectionProperties != value)
                {
                    connectionProperties = value;
                    OnPropertyChanged(nameof(ConnectionProperties));

                    // Do not show user credentials on DestinationPath field
                    if (connectionProperties != null)
                        DestinationPath = $"Server={ConnectionProperties.ServerName}; Database={ConnectionProperties.DatabaseName}";
                }
            }
        }

        #endregion

        #region Commands
        public ICommand ReadCommand { get; }

        #endregion

        public ReaderViewModel()
        {
            worker = new BackgroundWorker();
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            worker.ProgressChanged += Worker_ProgressChanged;
            ReadCommand = new RelayCommand(ReadPackage);
            SingleFile = true;
            DestinationType = ExportDestination.FileSystem;
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Show information about the current processed DTSX file
            if (DestinationType == ExportDestination.FileSystem)
                ScriptFilePaths.Add($"Id - [{((ExportedDTSX)e.UserState).Id}] - Input file: \"{((ExportedDTSX)e.UserState).DTSXSourcePath}\" - Output file: \"{((ExportedDTSX)e.UserState).OutputLocation}\"");
            else
                ScriptFilePaths.Add($"Id - [{((ExportedDTSX)e.UserState).Id}] - Input file: \"{((ExportedDTSX)e.UserState).DTSXSourcePath}\" - Output: \"{DestinationPath}\"");
        }

        /// <summary>
        /// Read the DTSX files.
        /// </summary>
        private void ReadPackage()
        {
            if (ScriptFilePaths != null)
                ScriptFilePaths.Clear();
            else
                ScriptFilePaths = new ObservableCollection<string>();

            ScriptFilePaths.Add($"Start time: {DateTime.Now.ToLongTimeString()}");
            worker.WorkerReportsProgress = true;
            worker.RunWorkerAsync();
            ResultMessage = "Reading...";
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            IPackageProcessor packageProcessor = null;
            try
            {
                if (DestinationType == ExportDestination.FileSystem)
                    packageProcessor = new SQLScriptPackageProcessor();
                else
                    packageProcessor = new SQLServerPackageProcessor();

                IsReading = true;

                ((IExporterObservable)packageProcessor).Subscribe(this);

                // Set the connection string for destination
                string connectionString = null;
                if (DestinationType == ExportDestination.FileSystem)
                    connectionString = DestinationPath;
                else if (ConnectionProperties != null)
                    connectionString = ConnectionProperties.GetConnectionString();

                // Start to export DTSX files
                if (SingleFile)
                    e.Result = packageProcessor.Export(SourcePath, connectionString);
                else
                    e.Result = packageProcessor.ExportToFiles(SourcePath, connectionString);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (packageProcessor != null)
                    ((IExporterObservable)packageProcessor).UnSubscribe(this);
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsReading = false;

            if (e.Error != null)
            {
                ResultMessage = e.Error.Message;
            }
            else
            {
                if (ScriptFilePaths == null)
                    ScriptFilePaths = new ObservableCollection<string>();

                int readFiles = (int)e.Result;

                if (readFiles > 0)
                {
                    ResultMessage = DestinationType == ExportDestination.FileSystem ?
                                    $"Data exported to: {DestinationPath}" : $"Data exported to: {DestinationPath}";
                }
                else
                {
                    ResultMessage = $"No DTSX files found on path: {SourcePath}";
                }
            }
            ScriptFilePaths.Add($"End time: {DateTime.Now.ToLongTimeString()}");
        }

        public void OnDTSXExported(ExportedDTSX exportedDTSX)
        {
            worker.ReportProgress(0, exportedDTSX);

            // Set destination path to the directory of the exported files
            // when there is no destination path configured
            if (DestinationType == ExportDestination.FileSystem && string.IsNullOrEmpty(DestinationPath))
            {
                DestinationPath = Path.GetDirectoryName(exportedDTSX.OutputLocation);
            }

        }
    }
}