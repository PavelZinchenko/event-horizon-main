@echo off
setlocal
powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0build-database-editor.ps1" %*
exit /b %errorlevel%
