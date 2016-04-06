REM Rename files with root directory in mind
set rootdir=C:\

for /r "%~1" %%A in (*.xml *.dat) do (
	..\SingleGame "%%~A" -r=%rootdir%
)