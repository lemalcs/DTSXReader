using System;
using System.Collections.Generic;
using System.IO;

namespace DTSXDumper
{
    /// <summary>
    /// Export DTSX files to SQL scripts.
    /// </summary>
    public class SQLScriptPackageProcessor : IPackageProcessor, IExporterObservable
    {
        /// <summary>
        /// The list of observers to notify to about current exported DTSX files.
        /// </summary>
        private List<IExporterObserver> observersList;

        /// <summary>
        /// The name of script files when reading multiple DTSX files.
        /// </summary>
        private const string SQL_SCRIPT_PACKAGE_LIST_NAME = "dtsx-data.sql";

        /// <summary>
        /// The name of script file when reading a single DTSX file.
        /// </summary>
        private const string SQL_SCRIPT_FILE_NAME = "single-dtsx-data.sql";

        /// <summary>
        /// SQL statement to create the table where DTSX content will be inserted.
        /// This script works for SQL Server database.
        /// </summary>
        private const string SQL_SCRIPT_TABLE_CREATION =
            @"/*
create table dtsx_info(
dtsx_id int,
dtsx_path nvarchar(2000),
dtsx_name varchar(200),
item_id int,
item_type varchar(200),
field_id int,
field_name varchar(200),
value varchar(max),
linked_item_type varchar(200)
)
*/";

        /// <summary>
        /// Export a DTSX file to a SQL script (SQL Server compatible).
        /// </summary>
        /// <param name="packagePath">The path of DTSX file.</param>
        /// <param name="destinationConnectionString">The path of folder where the script will be saved.</param>
        /// <returns>The number of DTSX files read.</returns>
        public int Export(string packagePath, string destinationConnectionString)
        {
            DTSXReader reader = new DTSXReader();
            List<DTSXItem> itemList = reader.Read(packagePath);

            // When destination folder is empty, the SQL script will
            // be saved in the same location as the executable (.exe)
            if (string.IsNullOrEmpty(destinationConnectionString))
                destinationConnectionString = Environment.CurrentDirectory;

            string scriptFilePath = Path.Combine(destinationConnectionString, SQL_SCRIPT_FILE_NAME);

            if (File.Exists(scriptFilePath))
                RenameExistingFile(scriptFilePath);

            int counter = 1;
            using (StreamWriter sw = new StreamWriter(scriptFilePath))
            {
                sw.WriteLine(SQL_SCRIPT_TABLE_CREATION);
                foreach (DTSXItem item in itemList)
                {
                    sw.WriteLine($"insert into dtsx_info(dtsx_id,dtsx_path,dtsx_name,item_id,item_type,field_id,field_name,value,linked_item_type)");
                    sw.WriteLine($"values({counter},'{Path.GetDirectoryName(packagePath).Replace("'", "''")}','{item.DTSXName.Replace("'", "''")}',{item.ItemId},'{item.ItemType}',{item.FieldId},'{item.FieldName}','{item.Value.Replace("'", "''").Replace("\n", "\r\n")}','{item.LinkedItemType}')");
                }
            }

            OnExportedDTSX(new ExportedDTSX(1, packagePath, scriptFilePath));

            return counter;
        }

        /// <summary>
        /// Export a set of DTSX files to SQL scripts (SQL Server compatible).
        /// </summary>
        /// <param name="packagePath">The path of DTSX files.</param>
        /// <param name="destinationConnectionString">The path of folder where the scripts will be saved.</param>
        /// <param name="readFiles">The current number of read DTSX files.</param>
        /// <returns>The total number of DTSX files read.</returns>
        public int ExportToFiles(string packagePathsList, string destinationConnectionString, int readFiles)
        {
            string[] dtsxFiles = Directory.GetFiles(packagePathsList, "*.dtsx");

            // This variable is used to give to every DTSX file
            // a unique number.
            int counter = readFiles;

            if (dtsxFiles != null && dtsxFiles.Length > 0)
            {
                counter++;

                // When destination folder is empty, the SQL script will
                // be saved in the same location as the executable (.exe)
                if (string.IsNullOrEmpty(destinationConnectionString))
                    destinationConnectionString = Environment.CurrentDirectory;

                string scriptFilePath = Path.Combine(destinationConnectionString, $"{counter}_{SQL_SCRIPT_PACKAGE_LIST_NAME}");

                if (File.Exists(scriptFilePath))
                    RenameExistingFile(scriptFilePath);

                using (StreamWriter sw = new StreamWriter(scriptFilePath))
                {
                    sw.WriteLine(SQL_SCRIPT_TABLE_CREATION);

                    for (int i = 0; i < dtsxFiles.Length; i++)
                    {
                        DTSXReader reader = new DTSXReader();
                        List<DTSXItem> itemList = reader.Read(dtsxFiles[i]);
                        sw.WriteLine("begin tran");
                        foreach (DTSXItem item in itemList)
                        {
                            sw.WriteLine($"insert into dtsx_info(dtsx_id,dtsx_path,dtsx_name,item_id,item_type,field_id,field_name,value,linked_item_type)");
                            sw.WriteLine($"values({counter},'{Path.GetDirectoryName(dtsxFiles[i]).Replace("'", "''")}','{item.DTSXName.Replace("'", "''")}',{item.ItemId},'{item.ItemType}',{item.FieldId},'{item.FieldName}','{item.Value.Replace("'", "''").Replace("\n", "\r\n")}','{item.LinkedItemType}')");
                        }
                        sw.WriteLine("commit tran");

                        OnExportedDTSX(new ExportedDTSX(counter, dtsxFiles[i], scriptFilePath));
                        counter++;
                    }

                    // Decrease the number of read files that was increased in above loop,
                    // since no file was processed after last cycle.
                    counter--;
                }
            }

            // Read DTSX files from subdirectories
            string[] childDirectories = Directory.GetDirectories(packagePathsList);
            if (childDirectories.Length > 0)
            {
                foreach (string path in childDirectories)
                {
                    counter = ExportToFiles(path, destinationConnectionString, counter);
                }
            }

            return counter;
        }

        /// <summary>
        /// Export a set of DTSX files to SQL scripts (SQL Server compatible).
        /// </summary>
        /// <param name="packagePath">The path of DTSX files.</param>
        /// <param name="destinationConnectionString">The path of folder where the scripts will be saved.</param>
        /// <returns>The total number of DTSX files read.</returns>
        public int ExportToFiles(string packagePathsList, string destinationConnectionString)
        {
            return ExportToFiles(packagePathsList, destinationConnectionString, 0);
        }

        /// <summary>
        /// Rename an existing file adding a number at the end of its name but before the extension.
        /// </summary>
        /// <param name="fileName">The file path to rename to.</param>
        /// <returns>The path of renamed file.</returns>
        private string RenameExistingFile(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    string newFileName = fileName;
                    for (int counter = 0; File.Exists(newFileName);)
                    {
                        counter++;
                        newFileName = Path.Combine(
                            Path.GetDirectoryName(fileName),//Fill path of file
                            string.Format("{0} ({1}){2}",
                                            Path.GetFileNameWithoutExtension(fileName), //Name of file
                                            counter,//Counter for new file name
                                            Path.GetExtension(fileName))//Extension of file
                            );
                    }
                    File.Move(fileName, newFileName);
                    return newFileName;
                }
                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region IExporterObservable members

        /// <summary>
        /// Subscribes an observer in order to send notification about processed DTSX files.
        /// </summary>
        /// <param name="observer">A <see cref="IExporterObserver"/> object</param>
        public void Subscribe(IExporterObserver observer)
        {
            if (observersList == null)
                observersList = new List<IExporterObserver>();

            if (observer == null)
                throw new ArgumentNullException($"{nameof(observer)} parameter cannot be null.");

            observersList.Add(observer);
        }

        /// <summary>
        /// Unsubscribes an observer from receiving notifications about processed DTSX files.
        /// </summary>
        /// <param name="observer">A <see cref="IExporterObserver"/> object</param>
        public void UnSubscribe(IExporterObserver observer)
        {
            if (observersList != null)
                observersList.Remove(observer);
        }

        #endregion

        /// <summary>
        /// Fires an event when a DTSX file was exported to a destination.
        /// </summary>
        /// <param name="exportedDTSX">The details of exported DTSX file.</param>
        private void OnExportedDTSX(ExportedDTSX exportedDTSX)
        {
            foreach (IExporterObserver observer in observersList)
            {
                observer.OnDTSXExported(exportedDTSX);
            }
        }

        #region IDisposible Members

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    observersList.Clear();
                    observersList = null;
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
