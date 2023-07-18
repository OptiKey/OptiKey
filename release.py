#! python

from subprocess import call

import subprocess
import fileinput
import re
import os
import sys
import getopt
import shutil

origPath = os.getcwd();

def safeProcess( cmd ):
    "Run command, return boolean for success"
    print(cmd);
    try:
        out = subprocess.check_output(cmd, shell=True)
        print(out.decode("utf-8").replace("\\\n", "\n"))
        return True;
    except subprocess.CalledProcessError as e:                                                                                                   
        print("Status : FAIL", e.returncode)
        for line in e.output.decode('utf-8').split('\n'):
            print(line)
        return False;
        
def safeExit():
    print ("Exiting...")
    os.chdir(origPath)
    sys.exit(1)
#    safeProcess("git reset --hard head")

def get_version(filename):
    pattern = re.compile("\[assembly:\s*AssemblyVersion\(\"(\d*.\d*.\d*)\"\)\]")

    for line in fileinput.input(filename):
        if re.search(pattern, line): 
            version = pattern.search(line).groups()[0]
            return version
    return None;  

def search_directories(root_dir, target_dirs):
    found_dirs = []
    for root, dirs, files in os.walk(root_dir):
        for dir in dirs:
            if dir in target_dirs:
                path = os.path.join(root, dir)
                found_dirs.append(path)
    return found_dirs

def delete_directories(dirs):
    for dir in dirs:
        try:
            shutil.rmtree(dir)
            print(f"Deleted: {dir}", flush=True)

        except Exception as e:
            print(f"Error while deleting directory {dir}. {e}")

def delete_build_dirs():
    directories_to_delete = search_directories('.', ['bin', 'obj'])

    if directories_to_delete:
        print("The following directories will be deleted:")
        for dir in directories_to_delete:
            print(dir)

        confirm = input("\nAre you sure you want to delete these directories? (y/n): ")
        if confirm.lower() == 'y':
            print("Deleting directories...")
            delete_directories(directories_to_delete)
        else:
            print("Operation cancelled.")
            exit(1)
    else:
        print("No build directories found to delete.")
                         
  
# Don't continue if working copy is dirty
if not safeProcess('git diff-index --quiet HEAD --'):
    print( "Cannot build, git working copy dirty")
    safeExit()

# Delete all bin and obj directories to be sure it's fully clean
delete_build_dirs()

# Temporarily change the uiAccess flag in the app manifest
orig_file = "src\\JuliusSweetland.OptiKey.Core\\app.manifest"
deploy_file = "src\\JuliusSweetland.OptiKey.Core\\app.manifest.deploy"
shutil.copy2(deploy_file, orig_file)

# Build all projects
# FYI if you're running this directly in git bash, you need to escape the forward slashes in the options (e.g. //Rebuild)
clean = 'devenv.com OptiKeyDeployment.sln /Clean "Release|x86"'
if not safeProcess(clean):
    print("Error cleaning project")
    safeExit()

build = 'devenv.com OptiKeyDeployment.sln /Rebuild "Release|x86"'
if not safeProcess(build):
    print("Error building project")
    safeExit()

# Build installers
installers = ["OptiKeyPro.aip", "OptiKeyMouse.aip", "OptiKeySymbol.aip", "OptiKeyChat.aip"]
installers = [os.path.join("installer", file) for file in installers]
for installer_file in installers:    
    if not safeProcess("AdvancedInstaller.com /rebuild {}".format(installer_file)):
        print("Failed building installer "+ installer_file)
        safeExit()

# Discard local changes to InstallerStrings (these are a build artefact)
if not safeProcess("git checkout src/JuliusSweetland.OptiKey.InstallerActions/InstallerStrings.cs"):
    print("Error checking out InstallerStrings.cs")
    safeExit()

# Discard changes to app.manifest
if not safeProcess("git checkout {}".format(orig_file)):
    print("Error checking out {}".format(orig_file))
    safeExit()

# Tag code by version
# TODO?


