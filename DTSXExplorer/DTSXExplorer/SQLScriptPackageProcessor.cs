using System;
using System.Collections.Generic;
using System.IO;

namespace DTSXExplorer
{
    internal class SQLScriptPackageProcessor : IPackageProcessor
    {
        /// <summary>
        /// The current number of read DTSX files.
        /// </summary>
        private int counter = 1;

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
CREATE TABLE DTSX_INFO(
DTSX_ID INT,
DTSX_PATH NVARCHAR(2000),
DTSX_NAME VARCHAR(200),
ITEM_ID INT,
ITEM_TYPE VARCHAR(200),
FIELD_ID INT,
FIELD_NAME VARCHAR(200),
VALUE VARCHAR(MAX),
LINKED_ITEM_TYPE VARCHAR(200)
)
*/";

        public string Export(string packagePath, string destinationFile)
        {
            DTSXReader reader = new DTSXReader();
            var itemList = reader.Read(packagePath);

            string scriptFilePath = Path.Combine(destinationFile, SQL_SCRIPT_FILE_NAME);

            if (File.Exists(scriptFilePath))
                RenameExistingFile(scriptFilePath);

            using (StreamWriter sw = new StreamWriter(scriptFilePath))
            {
                sw.WriteLine(SQL_SCRIPT_TABLE_CREATION);
                foreach (var item in itemList)
                {
                    sw.WriteLine($"insert into dtsx_info(dtsx_id,dtsx_path,dtsx_name,item_id,item_type,field_id,field_name,value,linked_item_type)");
                    sw.WriteLine($"values({counter},'{Path.GetDirectoryName(packagePath).Replace("'", "''")}','{item.DTSXName.Replace("'", "''")}',{item.ItemId},'{item.ItemType}',{item.FieldId},'{item.FieldName}','{item.Value.Replace("'", "''").Replace("\n", "\r\n")}','{item.LinkedItemType}')");
                }
            }

            return scriptFilePath;
        }

        public List<string> ExportToFiles(string packagePathsList, string destinationFolder)
        {
            string scriptFilePath = Path.Combine(destinationFolder, $"{counter.ToString()}_{SQL_SCRIPT_PACKAGE_LIST_NAME}");

            if (counter == 1)
            {
                if (File.Exists(scriptFilePath))
                    RenameExistingFile(scriptFilePath);
            }

            List<string> outputScripFiles = new List<string> { scriptFilePath };

            using (StreamWriter sw = new StreamWriter(scriptFilePath))
            {
                sw.WriteLine(SQL_SCRIPT_TABLE_CREATION);

                string[] dtsxFiles = Directory.GetFiles(packagePathsList, "*.dtsx");
                for (int i = 0; i < dtsxFiles.Length; i++)
                {
                    DTSXReader reader = new DTSXReader();
                    var itemList = reader.Read(dtsxFiles[i]);
                    sw.WriteLine("begin tran");
                    foreach (var item in itemList)
                    {
                        sw.WriteLine($"insert into dtsx_info(dtsx_id,dtsx_path,dtsx_name,item_id,item_type,field_id,field_name,value,linked_item_type)");
                        sw.WriteLine($"values({counter},'{Path.GetDirectoryName(dtsxFiles[i]).Replace("'", "''")}','{item.DTSXName.Replace("'", "''")}',{item.ItemId},'{item.ItemType}',{item.FieldId},'{item.FieldName}','{item.Value.Replace("'", "''").Replace("\n", "\r\n")}','{item.LinkedItemType}')");
                    }
                    sw.WriteLine("commit tran");
                    counter++;
                }
            }

            string[] childDirectories = Directory.GetDirectories(packagePathsList);
            if (childDirectories.Length > 0)
            {
                foreach (string path in childDirectories)
                {
                    List<string> scriptsList = ExportToFiles(path, destinationFolder);
                    foreach (string script in scriptsList)
                    {
                        if (!outputScripFiles.Contains(script))
                        {
                            outputScripFiles.Add(script);
                        }
                    }
                }
            }

            return outputScripFiles;
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
    }
}
