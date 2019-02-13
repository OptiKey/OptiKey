# The `UiAccess` Flag

## Purpose

The `UiAccess` flag (basically) allows OptiKey to stay on top of every window, including the start (menu) interface in Windows 8.  
This is obviously required for an on screen keyboard.  
  
The flag is specified in the application manifest (`app.manifest` in the root of the project) on the `requestedExecutionLevel` key.

WHY IS IT SET TO FALSE IN THE OptiKey PROJECT? This is because Visual Studio has trouble debugging the application when it is on.  

For `UiAccess` flag to work, there are 4 (actually I think 3) requirements (detailed below), which are difficult to do during a standard debugging run. I, therefore, leave it off and turn it on during deployment of OptiKey.

MSDN article: [http://msdn.microsoft.com/en-us/library/windows/desktop/ee671610(v=vs.85).aspx]()

## Requirements to work

1. Elevated privileges - Click2Speak does **NOT** need elevated privileges. My tests also suggest that this is not required.
2. Signed assembly (by a trusted certificate) - use the `Signing` tab on project properties within Visual Studio to sign manifests (deployment and application manifests) AND assembly
3. "Secure location" - Click2Speak appears to install a shill in the ClickOnce location and the rest of the files in Program Files
4. `UiAccess="True"` flag - this prevents debugging so set in Release mode Post-Build/Pre-Publish only

Refer to "[Certificates and signing notes.md]()" for steps to create a test certifiicate and use it to sign the `OptiKey.exe` binary.  

## Workaround

You can get around the "Secure location" requirement and run the `OptiKey.exe` that's in the build output folder (and that's been signed) with UIAccess enabled by configuring local/group policy.  

To do this, refer to:  
[https://docs.microsoft.com/en-us/windows/security/threat-protection/security-policy-settings/user-account-control-only-elevate-uiaccess-applications-that-are-installed-in-secure-locations]()
