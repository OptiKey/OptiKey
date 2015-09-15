using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.Reflection;

namespace JuliusSweetland.OptiKey.Services
{
    public class LocalisationService
    {
        #region private fields
        private static Dictionary<ResourceDictionary, LocalisationService> InstanceForResourcesDictionnary = new Dictionary<ResourceDictionary, LocalisationService>();
        private ResourceDictionary ResourceToLocalise = null;
        private string BaseLocFilesFolder = null;
        private string LocalisedFileFolder = null;
        private string FileBaseName = null;
        private string _CurrentLggPath = null; 
        private string _CurrentLanguage = null;
        #endregion

        #region public fields
        public string CurrentLggPath { get { return _CurrentLggPath; } }
        #endregion
            
        #region cosntructors
        /// <summary> 
        /// Constructor.
        /// Use static method GetInstanceForResource to retrieve an instance allready managing a resource.
        /// Thow exception if you try to have the same resource managed twice.
        /// </summary>
        /// <param name="nResourceToLocalize">Resource to localize. (App.Current.Resources / My.Application.Resources for the Application's resources) </param>
        /// <param name="nLanguage">Current language ("en", ...).</param>
        /// <param name="nBaseLocFilesFolder">Base Folder where the App is installed.  </param>
        /// <param name="nLocalisedFileFolder">SubFolder where the languages files are to be found. (expl : "Languages/") </param>
        /// <param name="nFileBaseName">Base name for all localized files. Expl : "InterfaceStrings" is the BaseFileName for "InterfaceStrings_en.xaml" </param>
        public LocalisationService(ResourceDictionary nResourceToLocalize, string nLanguage,
                                                                     string nBaseLocFilesFolder ,string nLocalisedFileFolder  , string nFileBaseName )
        {
           // prevent same resource from being handled by several instances of EZLocalize
            if (InstanceForResourcesDictionnary.ContainsKey(nResourceToLocalize))
                throw new Exception("LocalisationServices Error : this resource has allready an LocalisationServices instance attached. Use static method GetInstanceForResource to retrieve an instance.");

            InstanceForResourcesDictionnary.Add(nResourceToLocalize, this);
            ResourceToLocalise= nResourceToLocalize;
            _CurrentLanguage = nLanguage;

            if (BaseLocFilesFolder != null)            
                BaseLocFilesFolder = nBaseLocFilesFolder;            
            else            
                BaseLocFilesFolder = StandardApplicationLaunchPath();
            
            if (!((BaseLocFilesFolder.EndsWith("/") || (BaseLocFilesFolder.EndsWith("\\"))))) 
                BaseLocFilesFolder+="\\";

            LocalisedFileFolder = nLocalisedFileFolder;

            if (!((LocalisedFileFolder.EndsWith("/") || (LocalisedFileFolder.EndsWith("\\"))))) 
                LocalisedFileFolder += "\\";
            FileBaseName = nFileBaseName;
            _CurrentLggPath = BaseLocFilesFolder + LocalisedFileFolder;            
        }
        #endregion

        #region public methods
        /// <summary> Returns the instance of LocalisationService managing the resource, or null if no instance found. throw exception on null argument. </summary>
        public static LocalisationService GetInstanceForResource(ResourceDictionary ThisResource)
        {
          if (!InstanceForResourcesDictionnary.ContainsKey(ThisResource))  return null;
          LocalisationService FoundEZLocalize = null;
          InstanceForResourcesDictionnary.TryGetValue (ThisResource,out FoundEZLocalize);
          return FoundEZLocalize;
        }

        /// <summary>
        /// get all distinct files base name for the current folder.
        /// files must end by "_LL.xaml" where LL is the language.
        /// expl InterfaceStrings_en.xaml, InterfaceStrings_fr.xaml , foo_en.xaml, bar.xaml --> "InterfaceStrings", "foo"
        /// </summary>
        public List<string> getFileBaseNames(string WhichDir){
            List<string>  AllXamlFiles = Directory.EnumerateFiles(WhichDir, "*.xaml").ToList();
            List<string> returnedList = new List<string>() ;
            if (AllXamlFiles .Count== 0) return returnedList;
            foreach (string FileName in AllXamlFiles)
            {
                string ShortName = System.IO.Path.GetFileNameWithoutExtension(FileName);
                if (ShortName.LastIndexOf("_")<0)  continue;
                ShortName = ShortName.Substring(0,(ShortName.LastIndexOf("_") ));
                if (!returnedList.Contains(ShortName))  returnedList.Add(ShortName);
            }
            return returnedList;
        }
   
       /// <summary>
       /// URI path for translations strings files
       /// </summary>
       /// <param name="WhichDir"></param>
       /// <param name="Region"></param>
       /// <returns></returns>
       public string getURIFileBaseNames(string WhichDir, string Region)
       {
           List<string> AllXamlFiles = Directory.EnumerateFiles(WhichDir, "*.xaml").ToList();
           string result = "";
           if (AllXamlFiles.Count == 0) return result;
           foreach (string FileName in AllXamlFiles)
           {
               string ShortName = Path.GetFileNameWithoutExtension(FileName);
               if (ShortName.Contains(string.Format("{0}{1}", "_", Region)))
               {
                   result = string.Format("{0}{1}{2}", "/Resources/Localisation/", ShortName, ".xaml");
                   break;
               }
           }
           return result;
       }
         
        /// <summary>
        /// gives all languages available for this file.
        /// expl with files InterfaceStrings_en.xaml, InterfaceStrings_fr.xaml  : when called with "InterfaceStrings" it will return : "en", "fr"
        /// </summary>
        /// <param name="WhichDir"></param>
        /// <param name="FileBaseName"></param>
        /// <returns></returns>
         public List<string> LanguagesForFileBaseName(string WhichDir, string FileBaseName) 
         {
            List<string>  AllXamlFiles = Directory.EnumerateFiles(WhichDir, "*.xaml").ToList();
            List<string> MyLanguageForAFileBaseNames = new List<string>();
            //  loop through each xaml file
            foreach (string FileName in AllXamlFiles) 
            {
                string ShortName = System.IO.Path.GetFileNameWithoutExtension(FileName);
                int underscorePos = ShortName.LastIndexOf("_");
                if ((underscorePos < 0)) continue;
                string BaseName = ShortName.Substring(0, underscorePos);
                if ((BaseName.ToLower() != FileBaseName.ToLower())) continue;
                string Lgg = ShortName.Substring((underscorePos + 1));
                if (!MyLanguageForAFileBaseNames.Contains(Lgg))  MyLanguageForAFileBaseNames.Add(Lgg);
            }
            MyLanguageForAFileBaseNames.Sort();
            return MyLanguageForAFileBaseNames;
        }
        #endregion

        #region private methods
        private static string StandardApplicationLaunchPath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\";
        }
        #endregion

    }
}
