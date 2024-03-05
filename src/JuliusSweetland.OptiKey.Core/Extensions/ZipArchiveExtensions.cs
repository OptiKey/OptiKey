using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JuliusSweetland.OptiKey.Extensions
{
    public static class ZipArchiveExtensions    
    {

        public static void ExtractToDirectory(this ZipArchive archive, string extractPath, bool overwrite)
        {
            if (!overwrite)
            {
                // standard implementation that doesn't allow overwriting
                archive.ExtractToDirectory(extractPath); 
            }
            else
            {
                foreach (ZipArchiveEntry file in archive.Entries)
                {
                    string completeFileName = Path.Combine(extractPath, file.FullName);
                    string directory = Path.GetDirectoryName(completeFileName);

                    // Don't allow extraction outside the destination dir
                    if (!completeFileName.StartsWith(extractPath, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new IOException("Trying to extract file outside of destination directory. See this link for more info: https://snyk.io/research/zip-slip-vulnerability");
                    }

                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    if (file.Name != "")
                        file.ExtractToFile(completeFileName, true);
                }
            }
        }
    }
}
