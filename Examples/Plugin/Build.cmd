PATH %windir%\Microsoft.NET\Framework\v4.0.30319;%PATH%
if not exist Bin md Bin

cd Src
MSBuild /t:Rebuild /p:Configuration=Release
cd ..

copy ..\..\Bin\Firefly.Core.dll Bin\
copy ..\..\Bin\Firefly.GUI.dll Bin\
copy ..\..\Bin\Firefly.Project.dll Bin\
copy ..\..\Bin\Eddy.exe Bin\
copy ..\..\Bin\Eddy.*.dll Bin\
copy *.locproj Bin\
copy *.locplugin Bin\
