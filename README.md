# Project Introduction
A tool for gathering and saving structure data into database from HTML/Text on HTTP by Regex.

![UI](https://images.gitee.com/uploads/images/2019/0621/170427_c9d9fa01_27856.png "20190621170416.png")

# Database Config
* Database type:OleDb(Excel or Access),MsSql,MySql,PgSql,SQLite;
* Connection String:connection string of each kind of databases in C#;
* Table Name:name of data table.

# Gathering Config
* Website URL:a URL for application;
* Detail Page Base URL:no dynamic parameters and containing detail data;
* PageIndex Parameter Name:page index parameter name;
* PageIndex Parameter Position:PageIndex Parameter in GET or POST;
* Data Index:first row data index;
* GET:fixed parameters in URL;
* POST:fixed parameters in HTTP request body;
* Primary Key:field name which can identify every row uniquely;
* Header Regex:fetching all field names via this;
* Attachment Header:the else field names formatted as source field index(form 0)-filed name(seperated by semicolon);
* PageIndex Regex:fetching max page via this;
* Data Regex:fetching data list via this;
* Attachment Regex:fetching detail data via this.
