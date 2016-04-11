@setlocal 
@set local=%~dp0

@pushd %WINDIR%\Microsoft.NET\Framework\v4.0.30319\
@goto build

:build
msbuild "%local%src\Chuye.Caching.sln" /t:Rebuild /P:Configuration=Release
@goto copy

:copy
robocopy "%local%src\Chuye.Caching.Memcached\bin\Release" "%local%release\Chuye.Caching.Memcached" /mir
robocopy "%local%src\Chuye.Caching.Redis\bin\Release" "%local%release\Chuye.Caching.Redis" /mir
@pack end

:pack
@pushd "%local%"
.nuget\NuGet.exe pack build\Chuye.Caching.nuspec -Prop Configuration=Release -OutputDirectory release
.nuget\NuGet.exe pack build\Chuye.Caching.Memcached.nuspec -Prop Configuration=Release -OutputDirectory release
.nuget\NuGet.exe pack build\Chuye.Caching.Redis.nuspec -Prop Configuration=Release -OutputDirectory release
@goto end

:end
@pushd %local%
@pause