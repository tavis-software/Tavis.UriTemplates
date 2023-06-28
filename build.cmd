@echo Off
set config=%1
if "%config%" == "" (
   set config=debug
)
md artifacts
dotnet build --configuration %config% --verbosity normal /m /v:M /fl /flp:LogFile=msbuild.log; /nr:false
dotnet pack --no-build --configuration %config% --output %cd%\artifacts