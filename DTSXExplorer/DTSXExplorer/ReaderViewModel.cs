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
    internal class ReaderViewModel : BaseModel
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
                if(isReading != value)
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
                if(scriptFilePaths != value)
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
            ReadCommand = new RelayCommand(ReadPackage);
            SingleFile = true;
        }

        /// <summary>
        /// Read the DTSX files.
        /// </summary>
		private void ReadPackage()
        {
            if(ScriptFilePaths != null)
                ScriptFilePaths.Clear();

            worker.RunWorkerAsync();
            ResultMessage = "Reading...";
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                IPackageProcessor packageProcessor = new SQLScriptPackageProcessor();
                IsReading = true;
               
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
                    ScriptFilePaths.Add($"Output file: {e.Result.ToString()}");
                    ResultMessage = $"Data exported to file: {e.Result.ToString()}";
                }
                else
                {
                    List<string> filePaths = (List<string>)e.Result;
                    foreach (string filePath in filePaths)
                    {
                        ScriptFilePaths.Add($"Output file: {filePath}");
                    }
                    ResultMessage = $"Data exported to folder: {DestinationPath}";
                }
            }
        }
    }
}