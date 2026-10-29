@echo off
setlocal
call "%~dp0build.cmd" android %*
exit /b %errorlevel%
