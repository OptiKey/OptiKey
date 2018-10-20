# Deployment Process

## Build, Package and Deploy

The following steps are to be followed when deploying a new version

1. Run all unit tests
2. Check that the `AssemblyVersion` in `AssemblyInfo` properties have already been incremented to the next release version number
3. Change `app.manifest` to include `uiAccess="true"` flag
4. Build in Release config
5. Run script 1 to clean the `src` directory, `bin` directory, copy files to `src` folder and sign the executable
6. Create installer in INNO
    1. Set version number
    2. Compile Inno script
7. Run script 3 to sign the installer
8. Test locally
9. If good then...
    1. Revert `app.manifest` back to `uiAccess="false"`
    2. Revert build from Release to Debug
    3. Commit source to GitHub (Debug mode)
10. Create release (tag) on GitHub including;
    1. Release notes
    2. Binary installer
11. Update the download link [on the wiki](https://github.com/JuliusSweetland/OptiKey/wiki/_Sidebar/_edit)

-----  

## INNO notes

.Net Framework dependencies (and auto download/install) taken from [http://www.codeproject.com/Articles/20868/NET-Framework-Installer-for-InnoSetup](http://www.codeproject.com/Articles/20868/NET-Framework-Installer-for-InnoSetup "Modular InnoSetup Dependency Installer (Code Project)")  
  A copy of the original scripts is included in the deployment folder in a file called `innodependencyinstaller.zip`

-----

## .Net Framework Version Notes

The application targets .Net 4.5 and so requires that the .Net Framework v4.5 (or later) is installed.

The .Net Framework v4.5 is supported on the following Windows versions (from [https://docs.microsoft.com/en-us/dotnet/framework/get-started/system-requirements](https://docs.microsoft.com/en-us/dotnet/framework/get-started/system-requirements ".Net Framework system requirements") )  

* Client operating systems from Windows Vista SP2 onwards
* Server operating systems from Windows Server 2008 SP2/Windows Server 2008 R2 SP1 onwards
* The above 2 requirements can be expressed as needing the following minimum Windows version numbers
  * Windows Vista, Service Pack 2: 6.0.6002
  * Windows Server 2008 SP2: 6.0.6002
  * Windows Server 2008 R2 SP1: 6.1.7601

-----

## Windows version numbers

The following resources help programatically identify the version of Windows which is currently executing

  [https://www.gaijin.at/en/infos/windows-version-numbers](https://www.gaijin.at/en/infos/windows-version-numbers "Windows version number reference")  
  [https://docs.microsoft.com/en-us/windows/desktop/Msi/operating-system-property-values](https://docs.microsoft.com/en-us/windows/desktop/Msi/operating-system-property-values "Operating System Property Values (MSDN)")  
  [https://www.techrepublic.com/blog/the-enterprise-cloud/a-guide-to-common-microsoft-software-versions/](https://www.techrepublic.com/blog/the-enterprise-cloud/a-guide-to-common-microsoft-software-versions/ "A guide to common Microsoft software versions")  

MSDN developer deployment guide  
[https://docs.microsoft.com/en-us/dotnet/framework/deployment/deployment-guide-for-developers](https://docs.microsoft.com/en-us/dotnet/framework/deployment/deployment-guide-for-developers ".NET Framework deployment guide for developers")
