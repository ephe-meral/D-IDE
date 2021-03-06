@echo off
del ".\application_build.log"
del ".\installer_build.log"

if "%PROGRAMFILES(X86)%"=="" goto :x86
goto :x64
:x86
	if "%DEVENV%"=="" set DEVENV="%PROGRAMFILES%\Microsoft Visual Studio 10.0\Common7\IDE\devenv.exe"  /out ".\application_build.log" /rebuild release
	if "%MAKENSIS%"=="" set MAKENSIS=%PROGRAMFILES%\NSIS
	goto done
:x64
	if "%DEVENV%"=="" set DEVENV="%PROGRAMFILES(X86)%\Microsoft Visual Studio 10.0\Common7\IDE\devenv.exe"  /out ".\application_build.log" /rebuild release
	if "%MAKENSIS%"=="" set MAKENSIS=%PROGRAMFILES(X86)%\NSIS
:done

echo Build updater...
%DEVENV% "..\D-IDE.Updater\D-IDE.Updater.sln"

echo Make web installer...
"%MAKENSIS%\makensis.exe" ".\d-ide.web-install.nsi" > ".\installer_build.log"

set DEVENV=
set MAKENSIS=

echo Done