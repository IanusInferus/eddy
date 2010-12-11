PATH %SystemRoot%\Microsoft.NET\Framework\v4.0.30319;%PATH%

MSBuild /t:Rebuild /p:Configuration=Release /p:Platform="Any CPU"
MSBuild /t:Build /p:Configuration=Release /p:Platform=x86

copy Doc\Readme.*.txt ..\Bin\
copy Doc\UpdateLog.*.txt ..\Bin\
copy Doc\License.*.txt ..\Bin\

pause
