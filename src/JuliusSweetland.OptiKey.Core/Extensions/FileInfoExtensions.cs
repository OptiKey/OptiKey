// Copyright (c) 2024 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.IO;

namespace JuliusSweetland.OptiKey.Extensions
{
    public static class FileInfoExtensions
    {

        public static string GetRelativePath(this FileInfo f, string basePath)
        {
            string fullPath = Path.GetDirectoryName(f.FullName) + Path.DirectorySeparatorChar;

            Uri baseUri = new Uri(basePath + Path.DirectorySeparatorChar);
            Uri fullUri = new Uri(fullPath);
            string relativePath = Uri.UnescapeDataString(baseUri.MakeRelativeUri(fullUri).ToString());

            // Represent the current directory if empty
            if (string.IsNullOrEmpty(relativePath) ||
                relativePath == Path.DirectorySeparatorChar.ToString())
                relativePath = "." + Path.DirectorySeparatorChar;

            // Uris use "/" in all cases. Replace with DirectorySeparatorChar
            relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);

            return relativePath;
        }


        public static bool IsLocked(this FileInfo f)
        {
            FileStream fs = null;
            try
            {
                fs = File.Open(f.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                return false;
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                fs?.Close(); 
            }
        }
    }
}
