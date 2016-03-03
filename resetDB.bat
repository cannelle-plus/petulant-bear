rmdir /s/q "%CD%\data"

del "%CD%\src\db\db\bear2bear.db"

copy "%CD%\src\db\db\bear2bearProd.db" "%CD%\src\db\db\bear2bear.db"

"%CD%\src\db\buildDB.bat"