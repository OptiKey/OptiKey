using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuliusSweetland.OptiKey.Services.PluginEngine
{
    static class PluginUtils
    {
        // Get single unique string that combines owner and repo
        // We'll use this as a key in dictionaries        
        public static string GetRepoKey(string owner, string repoName)
        {
            return $"{owner}{Path.DirectorySeparatorChar}{repoName}";
        }

        public static string GetRepoKey(GithubRepoModel repo)
        {
            return GetRepoKey(repo.Owner, repo.Name);
        }        
    }
}
