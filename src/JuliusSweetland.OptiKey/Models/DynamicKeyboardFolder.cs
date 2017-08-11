using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using JuliusSweetland.OptiKey.Extensions;

namespace JuliusSweetland.OptiKey.Models
{
    struct KeyboardInfo
    {
        public string fullPath;
        public string keyboardName;
        public string symbolString;
        public bool isHidden; // default false

        public override string ToString()
        {
            string str = fullPath +  ", ";
            str += keyboardName + ", ";
            str += symbolString + ", ";
            str += isHidden + ", ";
            return str;
        }
    }

    class DynamicKeyboardFolder
    {

        #region Constants

        private const string ApplicationDataSubPath = @"JuliusSweetland\OptiKey\Keyboards\";
        private const string OriginalKeyboardsSubPath = @"Keyboards\";

        #endregion

        #region Private Members

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        public List<KeyboardInfo> keyboards;

        public DynamicKeyboardFolder()
        {
            // Find all possible xml files
            string filePath = GetUserKeyboardFolder();
            string[] fileArray = Directory.GetFiles(filePath, "*.xml");

            Log.InfoFormat("Found {0} keyboard files", fileArray.Length);

            // Read in keyboard name, symbol, hidden state from each file
            // TODO: ordering is undefined
            keyboards = new List<KeyboardInfo>();
            foreach (string fileName in fileArray)
            {
                string keyboardPath = Path.Combine(filePath, fileName);
                KeyboardInfo info = GetKeyboardInfo(keyboardPath);
                if (null != info.fullPath)
                {
                    if (!info.isHidden)
                    {
                        keyboards.Add(info);
                        Log.InfoFormat("Found keyboard file: {0}", info.fullPath);
                    }
                    else
                    {
                        Log.InfoFormat("Ignoring keyboard file: {0}", info.fullPath);
                    }
                }
            }
        }

        #region Private Methods

        private static string GetUserKeyboardFolder()
        {
            var applicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationDataSubPath);
            Directory.CreateDirectory(applicationDataPath); //Does nothing if already exists

            Log.DebugFormat("GetUserKeyboardFolder: {0}", applicationDataPath);

            return applicationDataPath;
        }

        // get name from XML if present
        private KeyboardInfo GetKeyboardInfo(string keyboardPath)
        {
            KeyboardInfo info = new KeyboardInfo();
            info.fullPath = keyboardPath;
            
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(XmlKeyboard));
                FileStream readStream = new FileStream(@keyboardPath, FileMode.Open);
                XmlKeyboard spec = (XmlKeyboard)serializer.Deserialize(readStream);
                readStream.Close();

                info.keyboardName = spec.Name.ToStringWithValidNewlines();
                info.symbolString = spec.Symbol;
                info.isHidden = spec.Hidden;

                // default to filename if no name given
                if (info.keyboardName.Length == 0)
                {
                    info.keyboardName = Path.GetFileName(keyboardPath);
                }      
            }
            catch (Exception e)
            {
                // replace info with default (no contents)
                info = new KeyboardInfo();
                Log.Error(e.ToString());
            }

            return info;
        }

        #endregion

    }
}
