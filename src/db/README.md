db-wookie
=========

setup
-----
install first http://system.data.sqlite.org/blobs/1.0.93.0/sqlite-netFx451-setup-bundle-x86-2013-1.0.93.0.exe

run recreateDrawTeamsWithTestSample.bat

basic instructions 
------------------

--open the database

sqlite3.exe drawTeams.db 

--command shell sqlLite3 :

|instructions                 | comments                |
|-----------------------------|-------------------------|
|.tables                      | list all tables         |
|select *from authentication; | do a basic select       |
|.quit                        | exit the command shell  |

batch
-----
recreateDrawTeams.bat deletes the current db if any and then creates an empty drawTeams database.
recreateDrawTeamsWithTestSample.bat deletes the current db if any and then creates drawTeams database inserting sample values.

authenticationScript.sql contains all sql statement relative to authentication to create the structure of drawTeams database.
authenticationScript.tests.sql contains all sql statement relative to authentication and insert test values of drawTeamdatabase.
