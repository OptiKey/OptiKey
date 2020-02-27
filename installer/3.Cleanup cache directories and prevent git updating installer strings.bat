rd /s /q "OptiKeyChat-cache"
rd /s /q "OptiKeyMouse-cache"
rd /s /q "OptiKeyPro-cache"
rd /s /q "OptiKeySymbol-cache"

del /q "*.back.aip"

REM git update-index --skip-worktree ../src/JuliusSweetland.OptiKey.InstallerActions/InstallerStrings.cs
git checkout -- src/JuliusSweetland.OptiKey.InstallerActions/InstallerStrings.cs
pause