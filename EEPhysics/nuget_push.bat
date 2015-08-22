@echo on
setlocal
for /f "" %%i in ('dir /B | findstr EEPhysics\.[0-9]*\.[0-9]\.[0-9]*\.[0-9]*\.nupkg') do set nuget_temp_file=%%i
ECHO %nuget_temp_file%>NUL
endlocal