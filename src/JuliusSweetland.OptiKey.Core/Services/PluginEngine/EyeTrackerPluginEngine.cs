using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using Prism.Commands;
using System.Windows;
using JuliusSweetland.OptiKey.Extensions;
using System.IO.Compression;
using Prism.Mvvm;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.UI.Windows;

namespace JuliusSweetland.OptiKey.Services.PluginEngine
{
    // This class is a ViewModel that contains the logic for presenting available
    // eye tracker plugins to the user. 
    public partial class EyeTrackerPluginEngine : BindableBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private string _topic;
        private InstalledPluginsSearch installedPlugins;
        private GithubPluginsSearch githubPlugins = GithubPluginsSearch.Instance;

        public DelegateCommand<GithubRepoModel> InstallCommand { get; private set; }
        public DelegateCommand<GithubRepoModel> UninstallCommand { get; private set; }
        public DelegateCommand<LocalPluginModel> OpenDirectoryCommand { get; private set; }

        //fixme: add IsSearching (or Loading?) property hooked up to GH plugins search

        #region Constructor

        public EyeTrackerPluginEngine()
        {
            // Delegates for button handling            
            InstallCommand = new DelegateCommand<GithubRepoModel>(Install);
            UninstallCommand = new DelegateCommand<GithubRepoModel>(Uninstall);
            OpenDirectoryCommand = new DelegateCommand<LocalPluginModel>(OpenDirectory);

            // Initialise lists and listeners
            locallyInstalledPlugins = new ObservableCollection<LocalPluginModel>();
            githubPluginsInstalled = new ObservableCollection<GithubRepoModel>();
            githubPluginsAvailable = new ObservableCollection<GithubRepoModel>();

            if (!githubPlugins.IsLoaded)
            {
                githubPlugins.Loaded += (s, e) => 
                {
                    Refresh();
                    RaisePropertyChanged("IsSearching");
                };
            }
            
            Refresh();
        }

        private void GithubPlugins_Loaded(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Public properties

        public bool IsSearching {
            get { return !githubPlugins.IsLoaded; }
        }

        public GithubRepoModel MostRecentlyInstalledDll
        {
            get; set;
        }

        public List<KeyValuePair<string, string>> AllSourcesAvailable
        {
            get
            {
                var allRepos = new List<KeyValuePair<string, string>>();

                // Add entries from Repositories
                foreach (var repo in Repositories.Values)
                {
                    if (repo.InstallStatus != PluginInstallStatus.NotInstalled)
                    {
                        var repoKey = PluginUtils.GetRepoKey(repo);
                        allRepos.Add(new KeyValuePair<string, string>(repoKey, repo.InstalledDllPath));
                    }
                }

                // Add entries from LocalPlugins
                foreach (var plugin in LocallyInstalledPlugins)
                {
                    var fileName = Path.GetFileNameWithoutExtension(plugin.FilePathDLL);
                    allRepos.Add(new KeyValuePair<string, string>(plugin.UserFacingName, plugin.FilePathDLL));
                }

                return allRepos;
            }
        }

        public Dictionary<string, GithubRepoModel> Repositories
        {
            get { return githubPlugins.Repositories; }
        }

        public ObservableCollection<LocalPluginModel> LocalPlugins
        {
            get { return installedPlugins.Repositories; }
        }

        public bool HasLocalPlugins
        {
            get { return locallyInstalledPlugins.Count > 0; }
        }

        public bool HasGithubPluginsInstalled
        {
            get { return githubPluginsInstalled.Count > 0; }
        }

        public bool HasGithubPluginsAvailable
        {
            get { return githubPluginsAvailable.Count > 0; }
        }

        ObservableCollection<LocalPluginModel> locallyInstalledPlugins;
        public ObservableCollection<LocalPluginModel> LocallyInstalledPlugins
        {
            get { return locallyInstalledPlugins; }
            set { SetProperty(ref locallyInstalledPlugins, value); }
        }

        ObservableCollection<GithubRepoModel> githubPluginsInstalled;
        public ObservableCollection<GithubRepoModel> GithubPluginsInstalled
        {
            get { return githubPluginsInstalled; }
            set { SetProperty(ref githubPluginsInstalled, value); }
        }

        ObservableCollection<GithubRepoModel> githubPluginsAvailable;
        public ObservableCollection<GithubRepoModel> GithubPluginsAvailable
        {
            get { return githubPluginsAvailable; }
            set { SetProperty(ref githubPluginsAvailable, value); }
        }

        #endregion

        public static string GetTopLevelPluginDirectory()
        {
            string ApplicationDataSubPath = @"OptiKey\OptiKey";

            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                ApplicationDataSubPath,
                                "EyeTrackerPlugins");
        }

        /* Go through the list of available plugins on GitHub, and the DLLs that have been
         * found on the file system, and pair them up to consolidate list of "installed" and "available"
         * plugins
         */
        public void Refresh()
        {
            // Find locally available ones            
            installedPlugins = new InstalledPluginsSearch();

            // Clear previous cache
            locallyInstalledPlugins.Clear();
            githubPluginsInstalled.Clear();
            githubPluginsAvailable.Clear();

            // Reset the install state of github plugins
            foreach (GithubRepoModel githubRepo in githubPlugins.Repositories.Values)
            {
                githubRepo.InstallStatus = PluginInstallStatus.NotInstalled;
            }

            // Figure out which of the files found locally correspond to github repos       
            foreach (LocalPluginModel localRepo in installedPlugins.Repositories)
            {
                //var parts = 
                string[] pathParts = localRepo.FolderName.Split(Path.DirectorySeparatorChar);
                bool foundRepo = false;
                if (pathParts.Length >= 2)
                {
                    string key = PluginUtils.GetRepoKey(pathParts[0], pathParts[1]);
                    if (githubPlugins.Repositories.ContainsKey(key))
                    {
                        var githubRepo = githubPlugins.Repositories[key];
                        githubRepo.InstalledDllPath = Path.Combine(GetTopLevelPluginDirectory(),
                                                                   localRepo.FolderName,
                                                                   localRepo.FilePathDLL);

                        // We may get here multiple times, for multiple version subfolders.
                        // So we need to update our belief of installed status as we go along
                        if (githubRepo.InstallStatus != PluginInstallStatus.UpToDate)
                        {
                            // Is it the latest?
                            if (pathParts.Length >= 3 && pathParts[2] == githubRepo.LatestRelease.TagName)
                            {
                                githubRepo.InstallStatus = PluginInstallStatus.UpToDate;
                                githubRepo.InstalledReleaseName = githubRepo.LatestRelease.TagName;
                            }
                            else if (githubRepo.InstallStatus != PluginInstallStatus.UpToDate &&
                                pathParts.Length >= 3)
                            {
                                githubRepo.InstallStatus = PluginInstallStatus.UpgradeAvailable;

                                // see if this is a more up to date version that whatever we've already found
                                var tag = pathParts[2];
                                int indexThisTag = githubRepo.AllReleases.FindIndex(release => release.TagName == tag);
                                int indexExistingTag = githubRepo.AllReleases.FindIndex(release => release.TagName == githubRepo.InstalledReleaseName);

                                if (indexExistingTag == -1 ||
                                    (indexThisTag != -1 && indexThisTag < indexExistingTag))
                                {
                                    githubRepo.InstalledReleaseName = tag;
                                }
                            }
                            else
                            {
                                githubRepo.InstalledReleaseName = "unknown";
                            }
                        }

                        foundRepo = true;
                    }
                }
                if (!foundRepo)
                {
                    locallyInstalledPlugins.Add(localRepo);
                }
            }

            // Create list of unique local plugins (having consolidated multiple versions of same github rep[)

            // Figure out which of the github repos are installed or not
            foreach (GithubRepoModel githubRepo in githubPlugins.Repositories.Values)
            {
                switch (githubRepo.InstallStatus)
                {
                    case PluginInstallStatus.UpToDate:
                    case PluginInstallStatus.UpgradeAvailable:
                        githubPluginsInstalled.Add(githubRepo);
                        break;
                    case PluginInstallStatus.NotInstalled:
                        githubPluginsAvailable.Add(githubRepo);
                        break;
                }
            }

            RaisePropertyChanged("HasGithubPluginsAvailable");
            RaisePropertyChanged("HasGithubPluginsInstalled");
            RaisePropertyChanged("HasLocalPlugins");

            // Release any file locks
            DllLoader.ResetTempDomain();
        }

        #region UI-driven commands and associated utils

        private void OpenDirectory(LocalPluginModel plugin)
        {
            //var folderName = plugin.FolderName.Replace('/', Path.DirectorySeparatorChar); //fixme: can we do away with this? choose key better
            var path = Path.Combine(GetTopLevelPluginDirectory(), plugin.FolderName);

            Process.Start(new ProcessStartInfo()
            {
                FileName = "explorer.exe",
                Arguments = path,
                UseShellExecute = true
            });
        }

        private string GetInstallFolderName(GithubRepoModel repo, bool includeVersionTag = true)
        {
            List<string> paths = new List<string>
            {
                GetTopLevelPluginDirectory(),
                repo.Owner,
                repo.Name
            };

            if (includeVersionTag)
                paths.Add(repo.LatestRelease.TagName);

            return Path.Combine(paths.ToArray());
        }

        private void EnsureExists(string folderName)
        {
            if (!Directory.Exists(folderName))
            {
                Directory.CreateDirectory(folderName);
            }
        }

        private static void ShowMessage(string errorMessage)
        {
            MessageBox.Show(errorMessage,
                "Error downloading plugin",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Asterisk);
        }

        private void Uninstall(GithubRepoModel repo)
        {
            // We will uninstall all versions
            var folderName = GetInstallFolderName(repo, false);

            if (Directory.Exists(folderName))
            {
                // Check if deleteable (it might not be, if loaded by current AppDomain)
                if (new FileInfo(repo.InstalledDllPath).IsLocked())
                {
                    MessageBox.Show(Resources.UNABLE_UNINSTALL,
                    Resources.FILES_LOCKED,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                }
                else
                {
                    // Confirm the uninstallation                
                    var result = MessageBox.Show($"{Resources.ASK_CONFIRM_UNINSTALL}\n\n{folderName}",
                        Resources.CONFIRM_UNINSTALL,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        // Release the AppDomain that has been used for loading DLL files, otherwise we find
                        // files are locked
                        DllLoader.ResetTempDomain();

                        // Recursively delete all files and subdirectories
                        DeleteDirectory(folderName);

                        // Make sure not in "most recently installed"
                        if (MostRecentlyInstalledDll != null &&
                            MostRecentlyInstalledDll.InstalledDllPath == repo.InstalledDllPath)
                            MostRecentlyInstalledDll = null;

                        // Remove parent directory if it was left empty
                        var parentDirectory = System.IO.Directory.GetParent(folderName).FullName;
                        if (!Directory.EnumerateFileSystemEntries(parentDirectory).Any())
                        {
                            DeleteDirectory(parentDirectory);
                        }

                        Refresh();
                    }
                }
            }
        }

        private static void DeleteDirectory(string targetDir)
        {
            try
            {
                Directory.Delete(targetDir, true);
            }
            catch (Exception e)
            {
                ShowMessage($"{Resources.ERROR_UNINSTALLING_PLUGIN}\n{e}");

                Log.Error($"Error deleting directory: {targetDir}");
                Log.Error(e);
            }
        }        

        private void Install(GithubRepoModel repo)
        {
           
            var folderName = GetInstallFolderName(repo);

            EnsureExists(folderName);

            // The payload is a ZIP file
            // so we'll download to TMP and then extract into APPDATA

            var uri = new Uri(repo.LatestRelease.AssetsUrl);
            string sourceFilePath = System.IO.Path.GetFileName(uri.LocalPath);
            string tempFilePath = Path.Combine(System.IO.Path.GetTempPath(), sourceFilePath);
            string destFilePath = Path.GetFileNameWithoutExtension(Path.Combine(folderName, sourceFilePath));

            // We'll download the content asynchronously with a progress bar
            WebClient client = new WebClient();
            client.DownloadProgressChanged += (s, ev) =>
            {
                repo.DownloadProgress = Math.Max(ev.ProgressPercentage, 90); // save some 'progress' for unzipping etc
            };

            client.DownloadFileCompleted += (s, ev) =>
            {
                repo.DownloadProgress = 95;

                // Extract the ZIP file
                try
                {
                    if (ev.Error == null && !ev.Cancelled)
                    {
                        using (ZipArchive archive = ZipFile.Open(tempFilePath, ZipArchiveMode.Read, Encoding.UTF8))
                        {
                            archive.ExtractToDirectory(folderName, true);
                            Log.Debug($"Downloaded plugin and extracted to {folderName}");
                            var result = TestNewInstall(folderName, repo);
                            var success = result.Item1;
                            var filename = result.Item2;

                            // If fully successfully, we can coerce this choice later
                            if (success)
                            {
                                repo.InstalledDllPath = filename;
                                MostRecentlyInstalledDll = repo;
                            }
                            else
                            {
                                MostRecentlyInstalledDll = null;
                            }
                        }
                    }
                    else
                    {
                        ShowMessage(ev.Error.Message);
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage(ex.Message);
                }
                finally
                {
                    try
                    {
                        File.Delete(tempFilePath);
                    }
                    catch
                    {
                        Log.Error($"Unable to delete temporary file {tempFilePath}");
                    }
                    repo.DownloadProgress = 100;
                    Refresh();
                }
            };

            try
            {
                client.DownloadFileAsync(uri, tempFilePath);
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message);
            }
            finally
            {
                Refresh();
            }
        }
        
        private Tuple<bool, string> TestNewInstall(string folderName, GithubRepoModel repo)
        {
            bool success = false;
            string dllFilename = null;

            // Is there an appropriate DLL available?
            List<string> dlls = DllLoader.FindValidDlls(folderName);            
            if (dlls.Count > 0)
            {
                dllFilename = dlls[0];
            }
            else
            {
                Log.Info($"No valid DLL file was found in {folderName}");
                ShowMessage(Resources.NO_VALID_DLL_FOUND);
                return new Tuple<bool, string>(false, null);
            }

            // Can we instantiate it okay?            
            DllLoader.ResetTempDomain(); // release lock so we can load same file in main appdomain

            var dllServiceAndErrorString = DllLoader.TryInstantiatePointSource(dlls[0]);
            var dllService = dllServiceAndErrorString.Item1;
            var errorMsg = dllServiceAndErrorString.Item2;
            
            //fixme: if we could instantiate in temp domain, would be better, otherwise it stays locked
            // but when we actually use it, it will be in main AppDomain, so might not be appropriate test

            if (dllService == null) {
                Log.Info($"Couldn't instantiate pointsource from {dlls[0]}");
                ShowMessage(errorMsg);
                success = false;
            }
            else
            {
                // Dispose it properly - we are not using it now
                if (dllService is IDisposable)
                {
                    var disposable = (IDisposable)dllService;
                    disposable.Dispose();
                }
                success = true;
            }
            return new Tuple<bool, string>(success, dllFilename);
        }

        #endregion

    }

}
