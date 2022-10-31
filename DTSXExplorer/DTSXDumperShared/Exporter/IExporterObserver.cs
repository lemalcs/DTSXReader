namespace DTSXDumper
{
    /// <summary>
    /// Details about exported DTSX files.
    /// </summary>
    public class ExportedDTSX
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
        public string OutputLocation { get; private set; }

        public ExportedDTSX(int id, string dTSXSourcePath, string outputLocation)
        {
            Id = id;
            DTSXSourcePath = dTSXSourcePath;
            OutputLocation = outputLocation;
        }
    }

    /// <summary>
    /// Receives notifications about exported DTSX files.
    /// </summary>
    public interface IExporterObserver
    {
        /// <summary>
        /// Notifies about exported DTSX file.
        /// </summary>
        /// <param name="exportedDTSX">Details about exported file.</param>
        void OnDTSXExported(ExportedDTSX exportedDTSX);
    }
}
