using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.Reflection;
using System.IO;
using JuliusSweetland.OptiKey.Contracts;

namespace JuliusSweetland.OptiKey.Services.PluginEngine
{
    // These methods run in non-standard AppDomains
    // MarshalByRefObject allows this to be used across AppDomains 
    public class AssemblyLoader : MarshalByRefObject
    {
        private Type TypeToLoad = typeof(IPointService);

        public Type LoadAssemblyAndFindType(string dllFilePath)
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(dllFilePath);
                var types = assembly.GetTypes();
                foreach (var t in types)
                {
                    var i = t.GetInterfaces();
                }
                return types.FirstOrDefault(t => t.GetInterfaces().Contains(TypeToLoad));
            }
            catch (Exception e)
            {
                // ?? can't log this
                int a = 1;
                return null;
            }
        }

        public bool LoadAssemblyAndConfirmType(string assemblyPath)
        {
            return LoadAssemblyAndFindType(assemblyPath) != null;
        }
    }

    /** This class handles 
     *  - testing DLL files in a separate AppDomain 
     *  - finding type info 
     *  - instantiating plugin classes
     */
    static class DllLoader
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Handling temporary domain 

        // AppDomain is reused, but can be reset when we need to release the lock on DLL files that
        // have been tested
        private static string DOMAIN_NAME = "EyeTrackerPluginsDomain";
        private static AppDomain temporaryDomain = null;

        public static void ResetTempDomain()
        {
            if (temporaryDomain != null)
            {
                AppDomain.Unload(temporaryDomain);
                temporaryDomain = null;
            }
        }

        

        private static AppDomain GetTempDomain()
        {
            DllLoader.ResetTempDomain();

            if (temporaryDomain == null)
            {
                var domainSetup = new AppDomainSetup { ApplicationBase = AppDomain.CurrentDomain.BaseDirectory };
                temporaryDomain = AppDomain.CreateDomain(DOMAIN_NAME, null, domainSetup);
                temporaryDomain.UnhandledException += (sender, args) =>
                    {
                        Log.Error("An UnhandledException has been encountered...", args.ExceptionObject as Exception);
                    };
            }
            return temporaryDomain;
        }

        #endregion

        #region Utility methods for loading files

        public static List<string> FindValidDlls(string basePath)
        {
            if (!Directory.Exists(basePath))
            {
                return new List<string>();
            }

            var dllFiles = Directory.GetFiles(basePath, "*.dll", SearchOption.AllDirectories);
            var validFiles = dllFiles.Where(file => DllLoader.IsValidPlugin(file)).ToList();

            // Reset lock on files
            DllLoader.ResetTempDomain();

            return validFiles;
        }

        public static bool IsValidPlugin(string dllFilePath)
        {
            Log.Debug($"Testing DLL file: { dllFilePath }");

            try
            {
                // Create the loader (a proxy) in the new AppDomain
                var assemblyLoader = (AssemblyLoader)DllLoader.GetTempDomain().CreateInstanceAndUnwrap(
                    typeof(AssemblyLoader).Assembly.FullName,
                    typeof(AssemblyLoader).FullName);

                bool available = assemblyLoader.LoadAssemblyAndConfirmType(dllFilePath);
                DllLoader.ResetTempDomain();
                return available;
            }
            catch (Exception ex)
            {
                Log.Debug($"Error trying to load {dllFilePath}: {ex}");
                return false;
            }
        }

        // Return PointService if available + error string if not
        public static Tuple<IPointService, string> TryInstantiatePointSource(string dllFilePath)
        {
            string baseFileName = Path.GetFileName(dllFilePath);

            string errorString = null;
            Type typeToLoad = null;
            IPointService dllService = null;

            // Try to identify the available implementation
            try
            {
                Assembly assembly = Assembly.LoadFrom(dllFilePath);
                typeToLoad = assembly.GetTypes().FirstOrDefault(t => t.GetInterfaces().Contains(typeof(IPointService)));
                if (typeToLoad == null)
                {
                    Log.Error($"No IPointService implementation found in {baseFileName}");
                }
            }
            catch (Exception e) {
                errorString = OptiKey.Properties.Resources.EYETRACKER_DLL_NOT_VALID;

                Log.Error($"Exception trying to load {baseFileName}");
                Log.Error(e);
            }

            // Instantiate it 
            if (typeToLoad != null)
            {
                try
                {
                    dllService = (IPointService)Activator.CreateInstance(typeToLoad);
                }
                catch (Exception e) {
                    errorString = OptiKey.Properties.Resources.EYETRACKER_DLL_INSTALLED_BUT_NOT_INSTANTIATED;

                    Log.Error($"Exception trying to instantiate { typeToLoad } from {baseFileName}");
                    Log.Error(e);
                }
            }
            return new Tuple<IPointService, string>(dllService, errorString);
        }

        #endregion

    }
}