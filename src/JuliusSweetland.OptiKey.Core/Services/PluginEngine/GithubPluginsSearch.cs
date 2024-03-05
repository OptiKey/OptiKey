using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Prism.Mvvm;
using System.Net.Http;
using log4net;
using System.Reflection;

namespace JuliusSweetland.OptiKey.Services.PluginEngine
{
    // This class queries github to find any repos advertising support for Optikey
    // (tag = "optikey-plugins")
    // Due to a small public rate limit on the github API, the results are cached in
    // a file between instantiations, and on construction we attempt to refresh but
    // fall back to the previous results if we hit a limit.
    public class GithubPluginsSearch : BindableBase
    {
        // static instance means we can trigger the class to be instantiated and loaded at startup, but 
        // don't need to pass around the object.
        private static readonly Lazy<GithubPluginsSearch> instance =
            new Lazy<GithubPluginsSearch>(() => new GithubPluginsSearch());
        public static GithubPluginsSearch Instance { get { return instance.Value; } }

        #region Private members

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly GitHubClient _github;
        private const string topic = "optikey-plugin";
        private Dictionary<string, GithubRepoModel> _cachedRepositories;

        #endregion

        #region Public properties

        private Dictionary<string, GithubRepoModel> _availableRepositories;
        public Dictionary<string, GithubRepoModel> Repositories
        {
            get { return _availableRepositories; }
            set { SetProperty(ref _availableRepositories, value); }
        }

        private bool _isLoaded = false;
        public bool IsLoaded
        {
            get { return _isLoaded; }
            set
            {
                _isLoaded = true;
                if (_isLoaded)
                {
                    Loaded?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler Loaded;

        #endregion

        public GithubPluginsSearch()
        {
            string token = null; 
            if (!String.IsNullOrEmpty(token))
            {
                _github = new GitHubClient(new ProductHeaderValue("OptiKey"))
                {
                    Credentials = new Credentials(token)
                };
            }
            else
            {
                _github = new GitHubClient(new ProductHeaderValue("OptiKey"));
            }        

            _cachedRepositories = DeserializeRepositoriesFromFile() ?? new Dictionary<string, GithubRepoModel>();
            _availableRepositories = new Dictionary<string, GithubRepoModel>();
        }

        public async Task TryLoad(int attempts, int delaySeconds)
        {
            for (int i =0; i < attempts; i++)
            {
                try {
                    await this.Load();
                    break;
                }
                catch (HttpRequestException)
                {
                    // We get this if no network connection
                    Log.Error("Unable to connect to GitHub API to search for eye tracker plugins");
                }
                catch (Exception otherException)
                {
                    Log.Error("Exception trying to query github API for eyetracker plugins");
                    Log.Error($"{otherException.GetType()}: {otherException}");                    
                }

                // Wait and try again later
                await Task.Delay(1000 * delaySeconds);
            }
        }

        public async Task Load()
        {           
            // Initially load up the repos from previous cache - we'll update any if we find new info
            foreach (var item in _cachedRepositories)
            {
                var repo = item.Value;
                if (repo.AllReleases.Count > 0)
                {
                    Repositories[item.Key] = repo;
                }
            }

            var searchRequest = new SearchRepositoriesRequest($"topic:{topic}")
            {
                Page = 1,
                PerPage = 100
            };
            
            var result = await _github.Search.SearchRepo(searchRequest);

            // First query any new repos, then 
            // existing repos - most stale first.
            foreach (var item in result.Items.OrderBy(item =>
                _availableRepositories.ContainsKey(PluginUtils.GetRepoKey(item.Owner.Login, item.Name)) ?
                _availableRepositories[PluginUtils.GetRepoKey(item.Owner.Login, item.Name)].QueriedAt :
                DateTime.MinValue))
            {
                string key = PluginUtils.GetRepoKey(item.Owner.Login, item.Name);
                try
                {
                    var releases = await _github.Repository.Release.GetAll(item.Owner.Login, item.Name);

                    // Filter for prereleases and order them by creation date                   
                    var orderedPreReleases = releases.Where(r => !r.Prerelease)
                                                     .OrderBy(r => r.CreatedAt)
                                                     .ToList();

                    // Find any ZIP assets - these are the only releases we care about
                    List<ReleaseInfo> allReleases = new List<ReleaseInfo>();
                    foreach (var release in orderedPreReleases)
                    {
                        ReleaseInfo info = ReleaseInfoFromGH(release);
                        if (info != null)
                            allReleases.Add(info);
                    }


                    ReleaseInfo latestRelease = null;
                    if (allReleases.Count > 0)
                    {
                        // Separately query the "Latest" release - this has particular semantic meaning outside of datetimes
                        var latest = await _github.Repository.Release.GetLatest(item.Owner.Login, item.Name);
                        latestRelease = ReleaseInfoFromGH(latest);
                    }

                    // Make a repo data structure
                    var repository = new GithubRepoModel
                    {
                        Name = item.Name,
                        Description = item.Description,
                        HtmlUrl = item.HtmlUrl,
                        Owner = item.Owner.Login,
                        Stars = item.StargazersCount,
                        AllReleases = allReleases,
                        LatestRelease = latestRelease,
                        QueriedAt = DateTime.Now
                    };

                    // Only include in our user-facing list if there's a release to download
                    if (repository.AllReleases.Count > 0)
                    {
                        _availableRepositories[key] = repository;
                    }

                    // Include in our cache regardless.
                    // This means that in the event there are lots of repos with the right tag
                    // but without releases/ZIPs then we don't waste API quota on re-querying 
                    // them until after we've looked for new ones. 
                    _cachedRepositories[key] = repository;

                }
                catch (RateLimitExceededException)
                {
                    Log.Info("Rate limit exceeded while querying github for plugins");
                }
            }

            SerializeRepositoriesToFile();

            IsLoaded = true;
        }        

        #region Private methods

        private ReleaseInfo ReleaseInfoFromGH(Release release)
        {
            ReleaseInfo info = null;

            var zipAsset = release.Assets.Where(asset => asset.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (zipAsset != null)
            {
                info = new ReleaseInfo
                {
                    TagName = release.TagName,
                    CreatedAt = zipAsset.CreatedAt.DateTime,
                    AssetsUrl = zipAsset.BrowserDownloadUrl
                };
            }

            return info;
        }

        private void SerializeRepositoriesToFile()
        {
            var jsonString = JsonConvert.SerializeObject(_cachedRepositories, Formatting.Indented);
            File.WriteAllText(GetRepositoryInfoFileName(), jsonString);
        }

        private Dictionary<string, GithubRepoModel> DeserializeRepositoriesFromFile()
        {
            string filePath = GetRepositoryInfoFileName();
            if (File.Exists(filePath))
            {
                var jsonString = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<Dictionary<string, GithubRepoModel>>(jsonString);
            }
            else
            {
                return new Dictionary<string, GithubRepoModel>();
            }
        }

        private string GetRepositoryInfoFileName()
        {
            string ApplicationDataSubPath = @"OptiKey\OptiKey";
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationDataSubPath);
            return Path.Combine(appDataPath, "repositories.json");
        }

        #endregion
    }
}
