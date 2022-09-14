using System;
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
            worker.RunWorkerAsync();
            ResultMessage = "Reading...";
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                IPackageProcessor packageProcessor = new SQLScriptPackageProcessor();

                if (SingleFile)
                    packageProcessor.Export(SourcePath, DestinationPath);
                else
                    packageProcessor.ExportPerFile(SourcePath, DestinationPath);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                ResultMessage = e.Error.Message;
            else
                ResultMessage = $"Data exported to {DestinationPath}";
        }
    }
}