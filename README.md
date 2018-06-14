# SalesforceSQLSchemaGenerator
Generates SQL script to match/mirror objects in the provided Salesforce org translating Salesforce types to Microsoft SQL Server data types.  Generated script can be placed into a single file or multiple files based on table/object name.  Script files can then be run against a MSSQL database to build tables.

Wish list/To-do items:
* Support for scripting global lookups
* Command line support
* Generate delta sync scripts