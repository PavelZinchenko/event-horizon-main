@echo off
setlocal
call "%~dp0build.cmd" linux %*
exit /b %errorlevel%
