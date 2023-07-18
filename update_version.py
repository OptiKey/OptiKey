#! python

from subprocess import call

import subprocess
import fileinput
import re
import os
import sys
import getopt

if len(sys.argv) < 2:
    print("usage: ./update_version.py --major")
    print("or:    ./update_version.py --minor")
    print("or:    ./update_version.py --revision")
    sys.exit(1)

origPath = os.getcwd();
version_level = sys.argv[1];
valid_version_levels = ['--major', '--minor', '--revision']
if version_level not in valid_version_levels:
    print("Version level not recognised, must be one of")
    print(valid_version_levels)
    sys.exit(1)
    
if version_level in  ['--major', '--minor']:
    print('major and minor version levels not yet implemented')
    print('WARNING: for version levels > revision, you might need to change extra GUIDs in the installer')    

def safeProcess( cmd ):
    "Run command, return boolean for success"
    print(cmd);
    try:
        out = subprocess.check_output(cmd, shell=True)
        print(out.decode("utf-8").replace("\\\n", "\n"))
        return True;
    except subprocess.CalledProcessError as e:                                                                                                   
        print("Status : FAIL", e.returncode, e.output)
        return False;
        
def safeExit():
    print ("Exiting...")
    os.chdir(origPath)
    sys.exit(1)
    print("To reset state, you probably want to run: ")
    print("git reset --hard head")
#    safeProcess("git reset --hard head")

def update_assembly_version(filename):
    pattern = re.compile("\[assembly:\s*AssemblyVersion\(\"(\d*).(\d*).(\d*)\"\)\]")
    
    new_version = None
    for line in fileinput.input(filename, inplace=True):
        if re.search(pattern, line): 
            major = int(pattern.search(line).groups()[0])
            minor = int(pattern.search(line).groups()[1])
            revision = int(pattern.search(line).groups()[2])
            
            if version_level == "--major":
                major += 1
                minor = 0
                revision = 0
            elif version_level == "--minor":
                minor += 1
                revision = 0
            elif version_level == "--revision":
                revision += 1
            new_version =  "{}.{}.{}".format(major, minor, revision)
            line = re.sub(pattern, "[assembly: AssemblyVersion(\"{}\")]".format(new_version), line);        
        print(line.rstrip('\n'))
    if not new_version:
        raise Exception("Could not find version in file {}".format(filename))
    return new_version

def update_installer_version(filename, new_version):
    # AI has commandline tools for updating version, inc GUID
    if not safeProcess("AdvancedInstaller.com /edit {} /SetVersion {}".format(filename, new_version)):
        print("Failed updating version of AI project")
        safeExit()   
        
# Don't continue if working copy is dirty
if not safeProcess('git diff-index --quiet HEAD --'):
    print( "Cannot continue, git working copy dirty")
    safeExit()
    
# Update the source files that OptiKey uses
assemblies =["Core", "Chat", "Pro", "Mouse", "Symbol"]
version_files = [os.path.join("src", "JuliusSweetland.OptiKey." + assembly, "Properties", "AssemblyInfo.cs") for assembly in assemblies]
for version_file in version_files:    
    new_version = update_assembly_version(version_file)

# Update the version reported in the AI installer projects
installers = ["OptiKeyPro.aip", "OptiKeyMouse.aip", "OptiKeySymbol.aip", "OptiKeyChat.aip"]
installers = [os.path.join("installer", file) for file in installers]
for installer_file in installers:
    update_installer_version(installer_file, new_version)

# Commit changes
for version_file in version_files:
    safeProcess("git add {}".format(version_file))

for installer_file in installers:
    safeProcess("git add {}".format(installer_file))

safeProcess('git commit -m "Update version number to ' + new_version + '"')
    

