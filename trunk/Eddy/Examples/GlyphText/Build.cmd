if not exist Bin md Bin
xcopy ..\..\Bin\Firefly.*.dll Bin\ /Y
xcopy ..\..\Bin\Eddy.exe Bin\ /Y
xcopy ..\..\Bin\Eddy.*.dll Bin\ /Y
xcopy ..\..\Bin\Eddy.* Bin\ /E /Y
xcopy *.locproj Bin\ /Y
xcopy *.locplugin Bin\ /Y
