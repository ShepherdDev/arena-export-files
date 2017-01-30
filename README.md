## Information

Arena stores some files in `file_folder` and `file_folder_document` tables.
Use this program to get these files extracted from the database and placed
into the file system. Due to file system limitations some folder names or
file names may be modified. For example, `:` is an illegal character in a
Windows path so it, and any other illegal characters, will be replaced with
`_` (underscore).

## Usage

.\ExportArenaFiles.exe [ExportPath] [Server] [Database]

Example:

`.\ExportArenaFiles.exe C:\Export SQLSVR-08\SQLEXPRESS ArenaDb`

Default options:
* ExportPath = .\
* Server = localhost
* Database = ArenaDB

Once it has finished exporting it will wait for you to press a key
before closing.