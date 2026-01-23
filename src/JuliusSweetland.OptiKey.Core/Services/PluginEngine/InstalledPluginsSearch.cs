using log4net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JuliusSweetland.OptiKey.Extensions;

namespace JuliusSweetland.OptiKey.Services.PluginEngine
{
    public class LocalPluginModel
    {
        public string FolderName { get; set; }
        public string FilePathDLL { get; set; }
        public string UserFacingName
        {
            get
            {
                return FolderName + Path.GetFileName(FilePathDLL);
            }
        }
    }

    public class InstalledPluginsSearch
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ObservableCollection<LocalPluginModel> _repositories = new ObservableCollection<LocalPluginModel>();
        public ObservableCollection<LocalPluginModel> Repositories
        {
            get { return _repositories; }
        }

        public InstalledPluginsSearch()
        {
            string folderName = EyeTrackerPluginEngine.GetTopLevelPluginDirectory();

            // Find all installed plugins
            List<string> dlls = DllLoader.FindValidDlls(folderName);
            foreach (string dll in dlls)
            {
                FileInfo file = new FileInfo(dll);                
                string relativePath = file.GetRelativePath(EyeTrackerPluginEngine.GetTopLevelPluginDirectory());
                
                Log.Debug($"{file.Name} from { relativePath }");

                _repositories.Add(new LocalPluginModel
                {
                    FolderName = relativePath,
                    FilePathDLL = file.FullName,
                });
                var f = Directory.GetDirectoryRoot(relativePath);
                var a = 1;
            }

            // Release any file locks
            DllLoader.ResetTempDomain();
        }

    }
}

