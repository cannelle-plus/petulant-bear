cls

@RD /S /Q packages
@RD /S /Q paket-files

.paket\paket.exe install --force
pause
