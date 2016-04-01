@setlocal 
@set local=%~dp0
@pushd %WINDIR%\Microsoft.NET\Framework\v4.0.30319\
@goto build

:build
msbuild "%local%src\ChuyeEventBus.sln" /t:Rebuild /P:Configuration=Release
@goto copy

:copy
robocopy "%local%src\ChuyeEventBus.Host\bin\Release" "%local%release\ChuyeEventBus" /mir
@goto end

:end
@pushd %local%
@pause