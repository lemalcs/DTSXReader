using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

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

        private ObservableCollection<string> scriptFilePaths;

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
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ScriptFilePaths.Add($"Id - [{((ExportedDTSX)e.UserState).Id}] - Input file: \"{((ExportedDTSX)e.UserState).DTSXSourcePath}\" - Output file: \"{((ExportedDTSX)e.UserState).OutputFilePath}\"");
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
            try
            {
                IPackageProcessor packageProcessor = new SQLScriptPackageProcessor();
                IsReading = true;

                ((SQLScriptPackageProcessor)packageProcessor).Subscribe(this);

                if (SingleFile)
                    e.Result = packageProcessor.Export(SourcePath, DestinationPath);
                else
                    e.Result = packageProcessor.ExportToFiles(SourcePath, DestinationPath);
            }
            catch (Exception)
            {
                throw;
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

                if (e.Result is string)
                {
                    ResultMessage = $"Data exported to file: {e.Result.ToString()}";
                }
                else if (e.Result is List<string>)
                {
                    if (((List<string>)e.Result).Count > 0)
                        ResultMessage = $"Data exported to folder: {DestinationPath}";
                    else
                        ResultMessage = $"No DTSX files found on path: {SourcePath}";
                }
            }
            ScriptFilePaths.Add($"End time: {DateTime.Now.ToLongTimeString()}");
        }

        public void OnDTSXExported(ExportedDTSX exportedDTSX)
        {
            worker.ReportProgress(0, exportedDTSX);
        }
    }
}