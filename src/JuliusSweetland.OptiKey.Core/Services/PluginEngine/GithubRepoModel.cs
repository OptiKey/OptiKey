using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuliusSweetland.OptiKey.Services.PluginEngine
{
    public enum PluginInstallStatus
    {
        NotInstalled = 0,
        UpgradeAvailable,
        UpToDate
    }

    public class ReleaseInfo
    {
        public string TagName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AssetsUrl { get; set; }
    }

    public class GithubRepoModel : BindableBase
    {
        // make these non mutable? would need a constructor since we can't use `init`
        public string Name { get; set; }
        public string Description { get; set; }
        public string HtmlUrl { get; set; }
        public string Owner { get; set; }
        public int Stars { get; set; } // stars
        public DateTime? QueriedAt { get; set; }
        public ReleaseInfo LatestRelease { get; set; }
        public List<ReleaseInfo> AllReleases { get; set; }

        // Mutable state
        public PluginInstallStatus InstallStatus { get; set; }
        public string InstalledReleaseName { get; set; }
        public string InstalledDllPath { get; set; }

        // Changeable property
        private int _downloadProgress = 0;
        public int DownloadProgress
        {
            get { return _downloadProgress; }
            set { SetProperty(ref _downloadProgress, value); }
        }

    }
}
