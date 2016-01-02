cls

RD /S /Q packages
RD /S /Q paket-files

.paket\paket.exe install --force
xcopy /Y "paket-files\www.sqlite.org\sqlite3.exe" "src\db\sqlite3.exe"*
pause
