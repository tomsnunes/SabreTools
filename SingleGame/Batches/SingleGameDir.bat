REM Rename files
for /r "%~1" %%A in (*.xml *.dat) do (
	..\SingleGame "%%~A"
)