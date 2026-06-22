@echo off
pushd "%~dp0"

REM Find VS2022 vcvarsall.bat and set up x64 environment
set "VCVARS="
for /f "tokens=*" %%i in ('"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath 2^>nul') do (
    set "VCVARS=%%i\VC\Auxiliary\Build\vcvarsall.bat"
)
if not defined VCVARS (
    echo ERROR: Visual Studio not found
    goto :end
)
if not exist "%VCVARS%" (
    echo ERROR: vcvarsall.bat not found at %VCVARS%
    goto :end
)

call "%VCVARS%" x64 >nul 2>&1

if not exist "..\..\Assets\Plugins" mkdir "..\..\Assets\Plugins"

cl /LD /O2 /EHsc TransparentSwapChain.cpp /link dxgi.lib dcomp.lib user32.lib
if %ERRORLEVEL% NEQ 0 (
    echo Build FAILED
    goto :end
)

copy /Y TransparentSwapChain.dll "..\..\Assets\Plugins\" >nul
echo.
echo Build OK: Assets\Plugins\TransparentSwapChain.dll
echo Unity Inspector: select the DLL, check [x] Preloaded

:end
del /q TransparentSwapChain.obj TransparentSwapChain.lib TransparentSwapChain.exp TransparentSwapChain.dll 2>nul
popd
