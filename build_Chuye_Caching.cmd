@setlocal 
@set local=%~dp0
@pushd %WINDIR%\Microsoft.NET\Framework\v4.0.30319\
@goto build

:build
msbuild "%local%src\Chuye.Caching.sln" /t:Rebuild /P:Configuration=Release
@goto copy

:copy
robocopy "%local%src\Chuye.Caching\bin\Release" "%local%release" /e
robocopy "%local%src\Chuye.Caching.Memcached\bin\Release" "%local%release" /e
@goto end

:end
@pushd %local%
@pause