@echo off
setlocal
call "%~dp0build.cmd" mac %*
exit /b %errorlevel%
