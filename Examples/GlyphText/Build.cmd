if not exist Bin md Bin
xcopy ..\..\Bin\Firefly.*.dll Bin\ /Y
xcopy ..\..\Bin\Eddy.exe Bin\ /Y
xcopy ..\..\Bin\Eddy.*.dll Bin\ /Y
for /D %%r in (..\..\Bin\Eddy.*) do (
  xcopy "%%r" "Bin\%%~nxr\" /E /Y
)
pause
xcopy *.locproj Bin\ /Y
xcopy *.locplugin Bin\ /Y
