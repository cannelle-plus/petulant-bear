@echo off
if not exist "%CD%\src\db\temp" mkdir "%CD%\src\db\temp"
if not exist "%CD%\src\db\db" mkdir "%CD%\src\db\db"

del "%CD%\src\db\temp\script.sql"
copy /b "%CD%\src\db\scripts\0.0.1.0.sql"+"%CD%\src\db\scripts\0.0.2.0.sql" "%CD%\src\db\temp\script.sql"
echo.   >> "%CD%\src\db\temp\script.sql"
echo .exit >> "%CD%\src\db\temp\script.sql"



setlocal ENABLEDELAYEDEXPANSION
set word=\\
set str="%CD%\src\db\temp\script.sql"
set str=%str:\=!word!%
echo %str%



"%CD%\src\db\sqlite3.exe"  "%CD%\src\db\db\Bear2Bear.db" ".read ""%str%"""

echo ------------------------------------------------------
echo la base de donnees est prete dans le repertoire db...
echo ------------------------------------------------------
