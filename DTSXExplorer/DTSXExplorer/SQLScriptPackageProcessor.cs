using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DTSXExplorer
{
    internal class SQLScriptPackageProcessor : IPackageProcessor
    {
        public void Export(string packagePath, string destinationFile)
        {
            DTSXReader reader = new DTSXReader();
            var itemList = reader.Read(packagePath);

            using (StreamWriter sw = new StreamWriter(Path.Combine(destinationFile, "single-dtsx-data.sql")))
            {
                foreach (var item in itemList)
                {
                    sw.WriteLine($"insert into dtsx_info(dtsx_id,dtsx_name,item_id,item_type,field_id,field_name,value,linked_item_type)");
                    sw.WriteLine($"values(1,'{item.DTSXName}',{item.ItemId},'{item.ItemType}',{item.FieldId},'{item.FieldName}','{item.Value.Replace("'", "''").Replace("\n", "\r\n")}','{item.LinkedItemType}')");
                }
            }
        }

        public void ExportBatch(string packagePathsList, string destinationFolder)
        {
            using (StreamWriter sw = new StreamWriter(Path.Combine(destinationFolder, "dtsx-data.sql")))
            {
                string[] dtsxFiles = Directory.GetFiles(packagePathsList, "*.dtsx");
                for (int i = 0; i < dtsxFiles.Length; i++)
                {
                    DTSXReader reader = new DTSXReader();
                    var itemList = reader.Read(dtsxFiles[i]);

                    foreach (var item in itemList)
                    {
                        sw.WriteLine($"insert into dtsx_info(dtsx_id,dtsx_name,item_id,item_type,field_id,field_name,value,linked_item_type)");
                        sw.WriteLine($"values({i + 1},'{item.DTSXName}',{item.ItemId},'{item.ItemType}',{item.FieldId},'{item.FieldName}','{item.Value.Replace("'", "''").Replace("\n", "\r\n")}','{item.LinkedItemType}')");
                    }
                }
            }
        }
    }
}
