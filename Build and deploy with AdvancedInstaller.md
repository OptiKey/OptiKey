# Build pre-requisites
- AdvancedInstaller (license required)
	- Once installed run Advanced Installer
	- Since some dialogs are shared between all installers, you might need to tell AdvancedInstaller to look locally for these files. Go to Settings -> Repository Manager -> top level Settings -> Change repository path. Set it to <OptikeyRoot>/installers
	- Apply and close Advanced Installer
- WiX Toolset (https://wixtoolset.org/releases/)
- Visual Studio (e.g. 2019 community edition)

# To build installers:
- Open OptikeyDeployment.sln in VS
- Ensure <OptikeyRoot>\src\JuliusSweetland.OptiKey.Core\Properties\AssemblyInfo.cs has the correct version number
- Commit and push all changes to GitHub (so that the release tag lines up with the correct version of the codebase)
- Ensure <OptikeyRoot>\src\JuliusSweetland.OptiKey.Core\app.manifest has uiAccess="true"
- Build entire solution in Release
- Manually sign all required (or all 4) exe files
- Open all four AIP projects e.g. <OptikeyRoot>/installers/OptikeyMouse.aip
- Manually set the installer version to match the AssemblyInfo.cs version number
- Build all in AdvancedInstaller
- Setup files will be built in <OptikeyRoot>/installers/SetupFiles/
- Manually sign all installer exe files

# Ignoring generate code changes
When you build the deployment solution you'll get some changes in git that you'll want to ignore with: 
`git update-index --skip-worktree src/JuliusSweetland.OptiKey.InstallerActions/InstallerStrings.cs`

# Tests
There are a few tests in the Deployment solution. It would be worth hooking them up to CI at some point. I took them out of the main unit test project, since I wanted to keep all the extra deployment-related projects in the separate solution.