# DTSXReader
An helper to read tons of DTSX files without using Visual Studio. 

> A DTSX is file used by [SQL Server Integration Services](https://docs.microsoft.com/en-us/sql/integration-services/sql-server-integration-services?view=sql-server-ver16) which is a platform to build ETL(Extract Transform Load) processes. 

## What do we want?
A DTSX file can be opened using Visual Studio which is great IDE for debugging and developing purposes but it's often laggy along the way. This behavior is not a major issue if used for a couple of DTSX files but when dealing with tens, hundreds or even thousands of DTSXs, Visual Studio is not a suitable option if your only intention is to read their content.

Hence there is a need to get any information like flow, tasks, connections, variables of DTSXs but at scale.

## Getting started
This helper generates scripts for SQL Server that insert the content of DTSX files to a table.

You can get the scripts following these steps:

1. Select the **Type of processing** that indicates whether to load a single DTSX file or multiple DTSX files.
    > When selecting **Multiple DTSX files** option, this application reads the files in current folder and the files located in its descendant folders recursively. 
2. In **Source path** field, enter the full path of the DTSX file when **Single DTSX file** option is selected otherwise enter path of the folder where the files are located.
3. In **Destination path** field, enter the path of the folder where the scripts will be saved.
    > Each output script file will contain all the DTSXs found in the current folder
4. Click **Start reading** and wait for the scripts to be generated.

Before executing a script you may need to create the target table if it doesn't exist yet. The definition of the table is placed in a comment at the beginning of each script.

If you need to execute the scripts for other relational databases that support SQL language then you may need to remove the `begin tran` and `commit tran` statements.

## Examples of queries

Once a DTSX file is loaded to the table you can run queries to gather any king of information.

**Get sql queries from SQL Tasks**

```sql
select dtsx_id,dtsx_name,value as sql_query
from dtsx_info
where
item_type='SQLTask:SqlTaskData'
and field_name='SQLTask:SqlStatementSource'
```


**Get comments**

```sql
select dtsx_id,dtsx_name,value as comments 
from dtsx_info
where
item_type='AnnotationLayout'
and field_name='Text'
```


**Get variables**

```sql
select dtsx_id,dtsx_name,value as variable_name
from dtsx_info 
where
item_type='DTS:Variable'
and field_name='DTS:ObjectName'
```


**Get email addresses**

```sql
-- FROM field
select distinct dtsx_id,dtsx_name,value as FROM_email_address
from dtsx_info 
where
item_type='SendMailTask:SendMailTaskData'
and field_name='SendMailTask:From'


-- TO field
select distinct dtsx_id,dtsx_name,value as TO_email_address
from dtsx_info 
where
item_type='SendMailTask:SendMailTaskData'
and field_name='SendMailTask:To'


-- BCC field
select distinct dtsx_id,dtsx_name,value as BBC_email_address
from dtsx_info 
where
item_type='SendMailTask:SendMailTaskData'
and field_name='SendMailTask:BCC'
```


**Get connection strings**

```sql
select d.dtsx_id,d.dtsx_name,string.value as connection_string
from dtsx_info d join dtsx_info conn
on d.dtsx_id=conn.dtsx_id
and d.item_id=conn.item_id
join dtsx_info string
on d.dtsx_id=string.dtsx_id
and string.item_id=conn.value
where
d.item_type ='DTS:Property'
and d.field_name='DTS:Name'
and d.value='ConnectionString'
and conn.field_name='_child_'
and string.field_name='value'
```

To retrieve more information such as data flow tasks, creator name, script tasks; you should open the dtsx file with a text editor and read the inner XML in order to build a query that fits your needs.
