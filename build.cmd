@echo Off
set config=%1
if "%config%" == "" (
   set config=debug
)

md artifacts
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild build\build.proj /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false