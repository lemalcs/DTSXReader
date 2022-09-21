namespace DTSXExplorer
{
    /// <summary>
    /// Export the content of DTSX files to other formats.
    /// </summary>
    internal interface IPackageProcessor
    {
        /// <summary>
        /// Export the content of a single DTSX file to a single file.
        /// </summary>
        /// <param name="packagePath">The path of DTSX file.</param>
        /// <param name="destinationFile">The path of file containing the exported file.</param>
        void Export(string packagePath, string destinationFile);

        /// <summary>
        /// Export the content of multiple DTSX files to multiple files.
        /// </summary>
        /// <param name="packagePathsList">The path of folder containing the DTSX files.</param>
        /// <param name="destinationFolder">The path of folder where to store exported files.</param>
        void ExportToFiles(string packagePathsList, string destinationFolder);
    }
}
