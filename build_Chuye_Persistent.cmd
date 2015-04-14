@setlocal 
@set local=%~dp0
@pushd %WINDIR%\Microsoft.NET\Framework\v4.0.30319\
@goto build


:build
msbuild %local%src\Chuye.Persistent.sln /t:Rebuild /P:Configuration=Release
@goto copy

:copy
robocopy %local%src\Chuye.Persistent\bin\Release %local%release /e
robocopy %local%src\Chuye.Persistent.NH\bin\Release %local%release /e
robocopy %local%src\Chuye.Persistent.Mongo\bin\Release %local%release /e


:end
@pushd %local%
@pause