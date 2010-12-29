@for /d %%a in (*) do @(
  @if exist %%a\obj @(
    @rd %%a\obj /S /Q
  )
)
@cd..
@for /D %%r in (Bin;Bin\*) do @(
  @pushd "%%r"
  @if errorlevel 1 goto exit
  @for %%a in (*.exe;*.dll) do @(
    @del %%~na.pdb /F /Q
    @del %%~na.xml /F /Q
  )
  @del *.vshost.exe /F /Q
  @del *.manifest /F /Q
  @del *.CodeAnalysisLog.xml /F /Q
  @del *.lastcodeanalysissucceeded /F /Q
  @del Test.* /F /S /Q
  @popd
)
@cd Src
@del *.cache /F /Q

:exit
@pause
