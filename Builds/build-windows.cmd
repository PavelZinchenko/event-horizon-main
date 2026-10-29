@echo off
setlocal
call "%~dp0build.cmd" windows %*
exit /b %errorlevel%
