namespace DTSXExplorer
{
    /// <summary>
    /// Types of destinations where to export DTSX files.
    /// </summary>
    internal enum ExportDestination
    {
        /// <summary>
        /// Export to the file system (file or folder).
        /// </summary>
        FileSystem = 1,

        /// <summary>
        /// Export to a SQL Server database.
        /// </summary>
        SQLServer = 2
    }
}
