@echo off

set configuration=Mono
set platform=x64

echo ------------------------
echo [   Build SabreTools   ]
echo ------------------------
echo.

REM Set the configuration
:Configuration

echo What configuration do you want?
echo 1) Debug
echo 2) Release
echo 3) Mono [Default]

set /P confgnum="> "

if "%confignum%"=="1" ( set configuration=Debug )
if "%confignum%"=="2" ( set configuration=Release )
if "%confignum%"=="3" ( set configuration=Mono )
if "%confignum%"=="" ( set configuration=Mono )

REM Set the platform
:Platform

echo.
echo What platform do you want?
echo 1) AnyCPU
echo 2) x64 [Default]

set /P platnum="> "

if "%platnum%"=="1" ( set platform=AnyCPU )
if "%platnum%"=="2" ( set platform=x64 )
if "%platnum%"=="" ( set platform=x64 )

REM Now run the command
:Command

@echo on
"C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" SabreTools.sln /property:Configuration=%configuration% /property:Platform=%platform%

pause