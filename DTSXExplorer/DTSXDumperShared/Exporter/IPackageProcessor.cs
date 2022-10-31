namespace DTSXDumper
{
    /// <summary>
    /// Export the content of DTSX files to other formats.
    /// </summary>
    public interface IPackageProcessor
    {
        /// <summary>
        /// Export the content of a single DTSX file to a destination (for instance: file, database).
        /// </summary>
        /// <param name="packagePath">The path of DTSX file.</param>
        /// <param name="destinationConnectionString">The destination connection string where DTSX files will be saved.</param>
        /// <returns>The total number of read DTSX files.</returns>
        int Export(string packagePath, string destinationConnectionString);

        /// <summary>
        /// Export the content of multiple DTSX files to a destination (for instance: file, database).
        /// </summary>
        /// <param name="packagePathsList">The path of folder containing the DTSX files.</param>
        /// <param name="destinationConnectionString">The destination connection string where DTSX files will be saved</param>
        /// <returns>The total number of read DTSX files.</returns>
        int ExportToFiles(string packagePathsList, string destinationConnectionString);
    }
}
