using System;
using System.Collections.Generic;

#if NET40
using System.Data.SqlClient;
#else
using Microsoft.Data.SqlClient;
#endif

using System.IO;

namespace DTSXDumper
{
    /// <summary>
    /// Export DTSX files to a SQL Server database.
    /// </summary>
    public class SQLServerPackageProcessor : IPackageProcessor, IExporterObservable
    {
        /// <summary>
        /// The list of observers to notify to about current exported DTSX files.
        /// </summary>
        private List<IExporterObserver> observersList;

        /// <summary>
        /// Export a DTSX file to a SQL Server database.
        /// </summary>
        /// <param name="packagePath">The path of DTSX file.</param>
        /// <param name="destinationConnectionString">The connection string for SQL Server database.</param>
        /// <returns>The number of DTSX files read.</returns>
        public int Export(string packagePath, string destinationConnectionString)
        {
            DTSXReader reader = new DTSXReader();
            List<DTSXItem> itemList = reader.Read(packagePath);

            int counter = 1;
            using (SqlConnection sqlConnection = new SqlConnection(destinationConnectionString))
            {
                SqlTransaction sqlTransaction = null;
                try
                {
                    sqlConnection.Open();
                    sqlTransaction = sqlConnection.BeginTransaction();

                    // Insert data to destination table within a transaction
                    foreach (DTSXItem item in itemList)
                    {
                        SqlCommand sqlCommand = new SqlCommand();
                        sqlCommand.Connection = sqlConnection;
                        sqlCommand.Transaction = sqlTransaction;
                        sqlCommand.CommandText = $@"insert into dtsx_info(dtsx_id,dtsx_path,dtsx_name,item_id,item_type,field_id,field_name,value,linked_item_type)
values(@dtsx_id,@dtsx_path,@dtsx_name,@item_id,@item_type,@field_id,@field_name,@value,@linked_item_type)";

                        sqlCommand.Parameters.AddWithValue("@dtsx_id", counter);
                        sqlCommand.Parameters.AddWithValue("@dtsx_path", Path.GetDirectoryName(packagePath));
                        sqlCommand.Parameters.AddWithValue("@dtsx_name", item.DTSXName);
                        sqlCommand.Parameters.AddWithValue("@item_id", item.ItemId);
                        sqlCommand.Parameters.AddWithValue("@item_type", item.ItemType);
                        sqlCommand.Parameters.AddWithValue("@field_id", item.FieldId);
                        sqlCommand.Parameters.AddWithValue("@field_name", item.FieldName);
                        sqlCommand.Parameters.AddWithValue("@value", item.Value);
                        sqlCommand.Parameters.AddWithValue("@linked_item_type", item.LinkedItemType == null ? string.Empty : item.LinkedItemType);

                        sqlCommand.ExecuteNonQuery();
                    }

                    sqlTransaction.Commit();

                    // Fire an event when finished exporting a DTSX file
                    // Do not send the output location because the connection string to a database
                    // may have sensitive information.
                    OnExportedDTSX(new ExportedDTSX(1, packagePath, null));
                }
                catch (Exception)
                {
                    if (sqlTransaction != null)
                        sqlTransaction.Rollback();

                    throw;
                }
            }

            return counter;
        }

        /// <summary>
        /// Export a set of DTSX files to a SQL Server database.
        /// </summary>
        /// <param name="packagePath">The path of directory where DTSX files are located.</param>
        /// <param name="destinationConnectionString">The connection string for SQL Server database.</param>
        /// <param name="readFiles">The current number of read DTSX files.</param>
        /// <returns>The number of DTSX files read.</returns>
        public int ExportToFiles(string packagePathsList, string destinationConnectionString, int readFiles)
        {
            string[] dtsxFiles = Directory.GetFiles(packagePathsList, "*.dtsx");

            // This variable is used to give to every DTSX file
            // a unique number.
            int counter = readFiles;

            if (dtsxFiles != null && dtsxFiles.Length > 0)
            {
                using (SqlConnection sqlConnection = new SqlConnection(destinationConnectionString))
                {
                    counter++;

                    for (int i = 0; i < dtsxFiles.Length; i++)
                    {
                        DTSXReader reader = new DTSXReader();
                        List<DTSXItem> itemList = reader.Read(dtsxFiles[i]);

                        SqlTransaction sqlTransaction = null;
                        try
                        {
                            sqlConnection.Open();
                            sqlTransaction = sqlConnection.BeginTransaction();

                            // Insert data to destination table within a transaction
                            foreach (DTSXItem item in itemList)
                            {
                                SqlCommand sqlCommand = new SqlCommand();
                                sqlCommand.Connection = sqlConnection;
                                sqlCommand.Transaction = sqlTransaction;
                                sqlCommand.CommandText = $@"insert into dtsx_info(dtsx_id,dtsx_path,dtsx_name,item_id,item_type,field_id,field_name,value,linked_item_type)
values(@dtsx_id,@dtsx_path,@dtsx_name,@item_id,@item_type,@field_id,@field_name,@value,@linked_item_type)";

                                sqlCommand.Parameters.AddWithValue("@dtsx_id", counter);
                                sqlCommand.Parameters.AddWithValue("@dtsx_path", Path.GetDirectoryName(dtsxFiles[i]));
                                sqlCommand.Parameters.AddWithValue("@dtsx_name", item.DTSXName);
                                sqlCommand.Parameters.AddWithValue("@item_id", item.ItemId);
                                sqlCommand.Parameters.AddWithValue("@item_type", item.ItemType);
                                sqlCommand.Parameters.AddWithValue("@field_id", item.FieldId);
                                sqlCommand.Parameters.AddWithValue("@field_name", item.FieldName);
                                sqlCommand.Parameters.AddWithValue("@value", item.Value);
                                sqlCommand.Parameters.AddWithValue("@linked_item_type", item.LinkedItemType == null ? string.Empty : item.LinkedItemType);

                                sqlCommand.ExecuteNonQuery();
                            }

                            sqlTransaction.Commit();

                            // Fire an event when finished exporting a DTSX file
                            // Do not send the output location because the connection string to a database
                            // may have sensitive information.
                            OnExportedDTSX(new ExportedDTSX(counter, dtsxFiles[i], null));

                            counter++;
                        }
                        catch (Exception)
                        {
                            if (sqlTransaction != null)
                                sqlTransaction.Rollback();

                            throw;
                        }
                        finally
                        {
                            sqlConnection.Close();
                        }
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
        /// Export a set of DTSX files to a SQL Server database.
        /// </summary>
        /// <param name="packagePath">The path of directory where DTSX files are located.</param>
        /// <param name="destinationConnectionString">The connection string for SQL Server database.</param>
        /// <returns>The number of DTSX files read.</returns>
        public int ExportToFiles(string packagePathsList, string destinationConnectionString)
        {
            return ExportToFiles(packagePathsList, destinationConnectionString, 0);
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

#region IDisposable members

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
