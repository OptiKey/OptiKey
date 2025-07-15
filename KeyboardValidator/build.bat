@echo off
echo Building KeyboardValidator...
"C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" KeyboardValidator.csproj /p:Configuration=Release /p:Platform=x64
if %errorlevel% neq 0 (
    echo Build failed!
    pause
    exit /b %errorlevel%
)
echo Build completed successfully!
pause