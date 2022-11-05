# DTSXReader
An helper to read tons of DTSX files without using Visual Studio. 

> A DTSX is file used by [SQL Server Integration Services](https://docs.microsoft.com/en-us/sql/integration-services/sql-server-integration-services?view=sql-server-ver16) which is a platform to build ETL(Extract Transform Load) processes. 

## What do we want?
A DTSX file can be opened using Visual Studio which is great IDE for debugging and developing purposes but it's often laggy along the way. This behavior is not a major issue if used for a couple of DTSX files but when dealing with tens, hundreds or even thousands of DTSXs, Visual Studio is not a suitable option if your only intention is to read their content.

Hence there is a need to get any information like flow, tasks, connections, variables of DTSXs but at scale.

## Getting started
This helper dumps the content of DTSX files to the following destinations:
- Scripts (SQL Server compatible)
- SQL Server database

**SQL Scripts**

You can get the scripts following these steps:

1. Select the **Type of processing** that indicates whether to load a single DTSX file or multiple DTSX files.
    > When selecting **Multiple DTSX files** option, this application reads the files in current folder and the files located in its descendant folders recursively. 
2. In **Source path** field, enter the full path of the DTSX file when **Single DTSX file** option is selected otherwise enter path of the folder where the files are located.
3. In **Export to** section choose **File system** option and in the field **Destination folder** enter the path of the folder where the scripts will be saved in.
    > Each output script file will contain all the DTSXs found in the current folder
4. Click **Start reading** and wait for the scripts to be generated.

Before executing a script you may need to create the target table if it doesn't exist yet. The definition of the table is placed in a comment at the beginning of each script.

If you need to execute the scripts for other relational databases that support SQL language then you may need to remove the `begin tran` and `commit tran` statements.

Here is the table definition where the content of DTSXs will be inserted:
```sql
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
```


**SQL Server**

DTSX files can be dumped straight into a table in a SQL Server database. In order to do this, after you had selected the source DTSX file or files, follow these steps:

1. In the **Export to** section choose **SQL Server** option and click **Set connection**.
2. In the **SQL Server Connection** dialog, enter the server name, database name and credentials for the SQL Server instance, then click **OK**.
3. Hit the **Start reading** button to start dumping DTSX files.

> You need to have the table created beforehand in the database, find the definition of the table in the *SQL Script* section.



## Examples of queries

Once a DTSX file is loaded to the table you can run queries to gather any kind of information.

**A. Get sql queries from SQL Tasks**

```sql
select dtsx_id,dtsx_name,value as sql_query
from dtsx_info
where
item_type='SQLTask:SqlTaskData'
and field_name='SQLTask:SqlStatementSource'
```


**B. Get comments**

This query works for Integration Services 2012 and later:
```sql
select dtsx_id,dtsx_name,value as comments 
from dtsx_info
where
item_type='AnnotationLayout'
and field_name='Text'
```


**C. Get variables**

This query works for Integration Services 2005 and Integration Services 2008:

```sql
select dt.dtsx_id,dt.dtsx_name,actualname.value as variable_name
from dtsx_info dt
join dtsx_info childdetail
on dt.dtsx_id=childdetail.dtsx_id
and childdetail.item_id=dt.value
join dtsx_info text
on dt.dtsx_id=text.dtsx_id
and text.item_id=childdetail.item_id
join dtsx_info actualname
on dt.dtsx_id=actualname.dtsx_id
and actualname.item_id=text.value
where
dt.item_type='DTS:Variable'
and dt.field_name='_child_'
and childdetail.field_name='DTS:Name'
and childDetail.value='ObjectName'
and text.field_name='_child_'
and actualname.field_name='value'
```

This query works for Integration Services 2012 and later:
```sql
select dtsx_id,dtsx_name,value as variable_name
from dtsx_info 
where
item_type='DTS:Variable'
and field_name='DTS:ObjectName'
```


**D. Get email addresses**

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


**E. Get connection strings**

This query works for Integration Services 2005 and Integration Services 2008:
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


This query works for Integration Services 2012 and later.
```sql
select
refId.dtsx_id,
refId.dtsx_name,
refId.value as dts_refId,
ssId.value as dts_dtsid,
connString.value as connection_string,
conntype.value as connection_type
from 
dtsx_info refId join dtsx_info d 
on refId.dtsx_id=d.dtsx_id
and refId.item_id=d.item_id
join dtsx_info connChild
on d.dtsx_id=connChild.dtsx_id
and d.value=connChild.item_id
join dtsx_info connString
on connChild.dtsx_id=connString.dtsx_id
and connString.item_id=connChild.value
join dtsx_info ssId
on refId.dtsx_id=ssId.dtsx_id
and refId.item_id=ssId.item_id
join dtsx_info conntype
on ssId.dtsx_id=conntype.dtsx_id
and ssId.item_id=conntype.item_id
where
d.field_name='_child_'
and connChild.field_name='_child_'
and connString.field_name='DTS:ConnectionString'
and refId.field_name='DTS:refId'
and ssId.field_name='DTS:DTSID'
and conntype.field_name='DTS:CreationName'
```

**How to get more about content of a DTSX?**

To retrieve more information such as data flow tasks, creator name, script tasks; you can open the DTSX file with a text editor and read the inner XML in order to write a query that fits your needs, the queries listed above can give you guidelines of how to write them.

## DTSXDumper library

The library DTSXDumper can be used to read and export DTSX files from other type of applications such as: command line tools, web services or even other DTSX files.

You can review the DTSXExplorer project (WPF desktop application) as an example of how to use this library.

There are two libraries available as nuget packages:
- [DTSXDumper](https://www.nuget.org/packages/DTSXDumper) which targets .NET 3.1 and later 
- [DTSXDumper.Net40](https://www.nuget.org/packages/DTSXDumper.Net40) which targets .Net Framework 4.0 and later.