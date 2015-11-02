@echo off
if not exist "%CD%\src\db\temp" mkdir "%CD%\src\db\temp"
if not exist "%CD%\src\db\db" mkdir "%CD%\src\db\db"
del "%CD%\src\db\db\Bear2Bear.db"
del "%CD%\src\db\temp\script.temp"
copy /b "%CD%\src\db\scripts\authenticationScript.sql" "%CD%\src\db\temp\script.temp"
echo.   >> "%CD%\src\db\temp\script.temp"
echo .exit >> "%CD%\src\db\temp\script.temp"
if  exist "%CD%\src\db\db\Bear2Bear.db"	 del "%CD%\src\db\db\Bear2Bear.db"		
"%CD%\src\db\sqlite3.exe"  "%CD%\src\db\db\Bear2Bear.db" < "%CD%\src\db\temp\script.temp"	
echo ------------------------------------------------------
echo la base de donnees est prete dans le repertoire db...
echo ------------------------------------------------------