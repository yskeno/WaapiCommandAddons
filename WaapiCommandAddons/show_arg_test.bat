@echo off
setlocal enabledelayedexpansion

echo ----- %0 -----
echo.

if "%~1"=="" (
    echo arg is not found.
    echo.
    pause
    exit 
)

set num=0
for %%i in (%*) do (
    set /a num = num + 1
    echo arg!num!:
    echo %%i
)

echo.
echo -----   end of args   -----
pause
exit