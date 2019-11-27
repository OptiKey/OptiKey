"tools\signtool.exe" sign /n "Open Source Developer, Julius Sweetland" /t http://time.certum.pl /fd sha256 /v "..\src\JuliusSweetland.OptiKey.Chat\bin\Release\OptikeyChat.exe"
pause

"tools\signtool.exe" sign /n "Open Source Developer, Julius Sweetland" /t http://time.certum.pl /fd sha256 /v "..\src\JuliusSweetland.OptiKey.Mouse\bin\Release\OptikeyMouse.exe"
pause

"tools\signtool.exe" sign /n "Open Source Developer, Julius Sweetland" /t http://time.certum.pl /fd sha256 /v "..\src\JuliusSweetland.OptiKey.Pro\bin\Release\OptikeyPro.exe"
pause

"tools\signtool.exe" sign /n "Open Source Developer, Julius Sweetland" /t http://time.certum.pl /fd sha256 /v "..\src\JuliusSweetland.OptiKey.Symbol\bin\Release\OptikeySymbol.exe"
pause