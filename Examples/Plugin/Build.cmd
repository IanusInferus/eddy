PATH %SystemRoot%\Microsoft.NET\Framework\v4.0.30319;%PATH%
if not exist Bin md Bin

cd Src
MSBuild /t:Rebuild /p:Configuration=Release
cd ..

xcopy ..\..\Bin\Firefly.*.dll Bin\ /Y
xcopy ..\..\Bin\Eddy.exe Bin\ /Y
xcopy ..\..\Bin\Eddy.*.dll Bin\ /Y
for /D %%r in (..\..\Bin\Eddy.*) do (
  xcopy "%%r" "Bin\%%~nxr\" /E /Y
)
pause
xcopy *.locproj Bin\ /Y
xcopy *.locplugin Bin\ /Y
