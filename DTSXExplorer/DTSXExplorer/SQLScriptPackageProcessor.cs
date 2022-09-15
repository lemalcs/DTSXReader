using System.IO;

namespace DTSXExplorer
{
    internal class SQLScriptPackageProcessor : IPackageProcessor
    {
        private int counter = 1;

        private const string SQL_SCRIPT_PACKAGE_LIST_NAME = "dtsx-data.sql";

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

        public void Export(string packagePath, string destinationFile)
        {
            DTSXReader reader = new DTSXReader();
            var itemList = reader.Read(packagePath);

            using (StreamWriter sw = new StreamWriter(Path.Combine(destinationFile, "single-dtsx-data.sql")))
            {
                sw.WriteLine(SQL_SCRIPT_TABLE_CREATION);
                foreach (var item in itemList)
                {
                    sw.WriteLine($"insert into dtsx_info(dtsx_id,dtsx_path,dtsx_name,item_id,item_type,field_id,field_name,value,linked_item_type)");
                    sw.WriteLine($"values({counter},'{Path.GetDirectoryName(packagePath).Replace("'", "''")}','{item.DTSXName.Replace("'", "''")}',{item.ItemId},'{item.ItemType}',{item.FieldId},'{item.FieldName}','{item.Value.Replace("'", "''").Replace("\n", "\r\n")}','{item.LinkedItemType}')");
                }
            }
        }

        public void ExportBatch(string packagePathsList, string destinationFolder)
        {
            if (counter == 1)
            {
                File.Delete(Path.Combine(destinationFolder, SQL_SCRIPT_PACKAGE_LIST_NAME));
            }

            using (StreamWriter sw = new StreamWriter(Path.Combine(destinationFolder, SQL_SCRIPT_PACKAGE_LIST_NAME), true))
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
                    ExportBatch(path, destinationFolder);
                }
            }
        }

        public void ExportPerFile(string packagePathsList, string destinationFolder)
        {
            if (counter == 1)
            {
                File.Delete(Path.Combine(destinationFolder, $"{counter.ToString()}_{SQL_SCRIPT_PACKAGE_LIST_NAME}"));
            }

            using (StreamWriter sw = new StreamWriter(Path.Combine(destinationFolder, $"{counter.ToString()}_{SQL_SCRIPT_PACKAGE_LIST_NAME}")))
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
                    ExportPerFile(path, destinationFolder);
                }
            }
        }
    }
}
