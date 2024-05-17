mkdir "SetupFiles/PointSourceUtils"
copy "../src/JuliusSweetland.OptiKey.PointSourceUtils/PointSourceUtils/bin/x64/Release" "SetupFiles/PointSourceUtils/" /Y
del "SetupFiles/PointSourceUtils.zip"
"tools/7z" a -o:"\" "SetupFiles/PointSourceUtils.zip" "./SetupFiles/PointSourceUtils/*.*"
del "SetupFiles\PointSourceUtils\*" /Q
rmdir "SetupFiles/PointSourceUtils"
pause