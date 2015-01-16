* Why I disabled the Visual Studio Hosting Process (VSHostingProcess):
Debugging the application with the hosting process enabled seems to prevent the <supportedOS> settings in my app.manifest from being detected.
It also results in duplicate user settings files being created as this is based on the application's name (among other things).
This leads to incorrect OS Version information when calling Environment.OSVersion.Version (Windows 8.1 and beyond are reported as v6.2 == Windows 8).
http://stackoverflow.com/questions/27947672/windows-8-1-version-returned-as-6-2-win-8-0-when-not-elevated-but-6-3-win-8/27980291#27980291

* Why I attempted to re-write the InputSimulator:
I was an idiot and was using the Ctrl key as my trigger button. This was, predictably, resulting in published keystrokes having a Ctrl modifier.
