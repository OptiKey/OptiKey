using System;
using System.Deployment.Application;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using JuliusSweetland.OptiKey.Native;
using JuliusSweetland.OptiKey.Native.Enums;
using JuliusSweetland.OptiKey.Native.Structs;
using Microsoft.Win32;
using JuliusSweetland.OptiKey.Properties;

namespace JuliusSweetland.OptiKey.Static
{
    public static class DiagnosticInfo
    {
        private const string uacRegistryKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System";
        private const string uacRegistryValue = "EnableLUA";

        private const uint STANDARD_RIGHTS_READ = 0x00020000;
        private const uint TOKEN_QUERY = 0x0008;
        private const uint TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);

        public static string AssemblyVersion
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public static string AssemblyFileVersion
        {
            get
            {
                var attribute = System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(
                    typeof (System.Reflection.AssemblyFileVersionAttribute), false);

                if (attribute.Any())
                {
                    return ((System.Reflection.AssemblyFileVersionAttribute)(System.Reflection.Assembly.GetExecutingAssembly().GetCustomAttributes(
                        typeof(System.Reflection.AssemblyFileVersionAttribute), false).First())).Version;
                }

                return null;
            }
        }

        public static bool IsApplicationNetworkDeployed
        {
            get
            {
                return ApplicationDeployment.IsNetworkDeployed;
            }
        }

        public static string DeploymentVersion
        {
            get
            {
                return ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
        }
    
        public static bool RunningAsAdministrator
        {
            get { return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator); }
        }

        public static string ProcessBitness
        {
            get { return Environment.Is64BitProcess ? "64-Bit" : "32-Bit"; }
        }

        public static string OperatingSystemBitness
        {
            get { return Environment.Is64BitOperatingSystem ? "64-Bit" : "32-Bit"; }
        }

        public static string OperatingSystemVersion
        {
            get
            {
                bool? isServerVersion = null;
                string isServerVersionException = null;
                try
                {
                    isServerVersion = IsServerVersion();
                }
                catch (Exception ex)
                {
                    isServerVersionException = string.Format("Error when querying if OS is server version: {0}", ex.Message);
                }

                var vs = Environment.OSVersion.Version;
                
                //http://msdn.microsoft.com/en-gb/library/windows/desktop/ms724832%28v=vs.85%29.aspx
                //N.B. If the manifest file does not specify compatibility with Windows 8.1/Windows 10/later versions
                //then the call to Environment.OSVersion.Version will lie and return 6.2 (Windows 8).
                //Workaround detailed here: http://msdn.microsoft.com/en-us/library/windows/desktop/dn302074.aspx
                switch (vs.Major)
                {
                    case 3:
                        return "Windows NT 3.51";

                    case 4:
                        return "Windows NT 4.0";

                    case 5:
                        if (vs.Minor == 0) return "Windows 2000";
                        if (vs.Minor == 1) return "Windows XP";
                        if (isServerVersion != null)
                        {
                            if (isServerVersion.Value)
                            {
                                if (PInvoke.GetSystemMetrics(89) == 0) return "Windows Server 2003";
                                return "Windows Server 2003 R2";
                            }
                            return "Windows XP";
                        }
                        break;

                    case 6:
                        if (vs.Minor == 0)
                        {
                            if (isServerVersion != null)
                            {
                                if (isServerVersion.Value)
                                {
                                    return "Windows Server 2008";
                                }
                                return "Windows Vista";
                            }
                        }
                        if (vs.Minor == 1)
                        {
                            if (isServerVersion != null)
                            {
                                if (isServerVersion.Value)
                                {
                                    return "Windows Server 2008 R2";
                                }
                                return "Windows 7";
                            }
                        }
                        if (vs.Minor == 2)
                        {
                            if (isServerVersion != null)
                            {
                                if (isServerVersion.Value)
                                {
                                    return "Windows Server 2012";
                                }
                                return "Windows 8";
                            }
                        }
                        if (vs.Minor == 3)
                        {
                            if (isServerVersion != null)
                            {
                                if (isServerVersion.Value)
                                {
                                    return "Windows Server 2012 R2";
                                }
                                return "Windows 8.1";
                            }
                        }
                        break;

                    case 10:
                        return "Windows 10";
                }

                return string.Format("OS v{0}.{1}{2}", vs.Major, vs.Minor,
                    isServerVersionException != null ? string.Format(" - {0}", isServerVersionException) : null);
            }
        }

        public static string OperatingSystemServicePack
        {
            get
            {
                var os = new OSVERSIONINFO { dwOSVersionInfoSize = Marshal.SizeOf(typeof(OSVERSIONINFO)) };
                PInvoke.GetVersionEx(ref os); 
                return string.IsNullOrEmpty(os.szCSDVersion) ? "No Service Pack" : os.szCSDVersion; 
            }
        }

        public static bool IsProcessElevated
        {
            get
            {
                if (IsUacEnabled)
                {
                    IntPtr tokenHandle;
                    if (!PInvoke.OpenProcessToken(Process.GetCurrentProcess().Handle, TOKEN_READ, out tokenHandle))
                    {
                        throw new ApplicationException("Could not get process token.  Win32 Error Code: " + Marshal.GetLastWin32Error());
                    }
                
                    try
                    {
                        var elevationResult = TOKEN_ELEVATION_TYPE.TokenElevationTypeDefault;
                        int elevationResultSize = Marshal.SizeOf((int) elevationResult);
                        IntPtr elevationTypePtr = Marshal.AllocHGlobal(elevationResultSize);
                        
                        try
                        {
                            uint returnedSize;
                            var success = PInvoke.GetTokenInformation(
                                tokenHandle, TOKEN_INFORMATION_CLASS.TokenElevationType, elevationTypePtr, 
                                (uint) elevationResultSize, out returnedSize);

                            if (success)
                            {
                                elevationResult = (TOKEN_ELEVATION_TYPE) Marshal.ReadInt32(elevationTypePtr);
                                bool isProcessAdmin = elevationResult == TOKEN_ELEVATION_TYPE.TokenElevationTypeFull;
                                return isProcessAdmin;
                            }
                            else
                            {
                                throw new ApplicationException(Resources.UNABLE_TO_DETERMINE_CURRENT_ELEVATION);
                            }
                        }
                        finally
                        {
                            if (elevationTypePtr != IntPtr.Zero)
                            {
                                Marshal.FreeHGlobal(elevationTypePtr);
                            }
                        }
                    }
                    finally
                    {
                        if (tokenHandle != IntPtr.Zero)
                        {
                            PInvoke.CloseHandle(tokenHandle);
                        }
                    }
                }
                else
                {
                    var identity = WindowsIdentity.GetCurrent();
                    var principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
        }
    
        private static bool IsServerVersion() 
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem")) 
            { 
                foreach (var managementObject in searcher.Get()) 
                { 
                    // ProductType will be one of: 
                    // 1: Workstation 
                    // 2: Domain Controller 
                    // 3: Server 
                    var productType = (uint)managementObject.GetPropertyValue("ProductType"); 
                    return productType != 1; 
                } 
            } 
            return false; 
        } 
    
        private static bool IsUacEnabled
        {
            get
            {
                var uacKey = Registry.LocalMachine.OpenSubKey(uacRegistryKey, false);
                return uacKey != null && uacKey.GetValue(uacRegistryValue).Equals(1);
            }
        }
    }
}
