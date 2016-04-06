REM Rename files with no renaming in mind
for /r "%~1" %%A in (*.xml *.dat) do (
	..\SingleGame "%%~A" -n
)