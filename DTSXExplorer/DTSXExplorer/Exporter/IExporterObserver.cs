namespace DTSXExplorer
{
    internal class ExportedDTSX
    {
        /// <summary>
        /// The ID assigned to a DTSX file.
        /// </summary>
        public int Id { get; private set; }
        /// <summary>
        /// The path of source DTSX file.
        /// </summary>
        public string DTSXSourcePath { get; private set; }

        /// <summary>
        /// The path of exported file which contains the content of DTSX files.
        /// </summary>
        public string OutputFilePath { get; private set; }

        public ExportedDTSX(int id, string dTSXSourcePath, string outputFilePath)
        {
            Id = id;
            DTSXSourcePath = dTSXSourcePath;
            OutputFilePath = outputFilePath;
        }
    }

    internal interface IExporterObserver
    {
        /// <summary>
        /// Notifies about exported DTSX file.
        /// </summary>
        /// <param name="exportedDTSX">Details about exported file.</param>
        void OnDTSXExported(ExportedDTSX exportedDTSX);
    }
}
