using System;
using System.Collections.Generic;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.UI.ViewModels.Management;

namespace InstallerTranslation
{
    class ProcessInstallerTranslations
    {
        // .NET's usual internationalisation architecture requires a satellite assembly, to keep 
        // resources for different cultures separate from the compiled executable. 
        // This makes it difficult to expose translated strings to AdvancedInstaller, since we 
        // can only attach a standalone DLL, and not all the associated resource folders.
        //
        // To get round this, we can extract strings from all cultures and hard-code them
        // into a class exposed to the InstallerActions DLL. 
        static void Main(string[] args)
        {
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter("InstallerStrings.cs"))
            {

                Console.WriteLine("Extracting internationalised strings...");

                // Get all Optikey languages
                List<KeyValuePair<string, Languages>> languages = WordsViewModel.Languages;

                // Query resources to create multilingual dicts for key installer strings
                Dictionary<Languages, string> all_ALIENWARE_17_INFO = new Dictionary<Languages, string>();
                Dictionary<Languages, string> all_GAZE_TRACKER_INFO = new Dictionary<Languages, string>();
                Dictionary<Languages, string> all_IRISBOND_DUO_INFO = new Dictionary<Languages, string>();
                Dictionary<Languages, string> all_IRISBOND_HIRU_INFO = new Dictionary<Languages, string>();
                Dictionary<Languages, string> all_MOUSE_POSITION_INFO = new Dictionary<Languages, string>();
                Dictionary<Languages, string> all_TOBII_EYEX_INFO = new Dictionary<Languages, string>();
                Dictionary<Languages, string> all_TOBII_ASSISTIVE_INFO = new Dictionary<Languages, string>();
                Dictionary<Languages, string> all_VI_MYGAZE_INFO = new Dictionary<Languages, string>();
                Dictionary<Languages, string> all_MOUSE_POSITION = new Dictionary<Languages, string>();

                foreach (KeyValuePair<string, Languages> entry in languages)
                {
                    Languages language = entry.Value;
                    JuliusSweetland.OptiKey.Properties.Resources.Culture = language.ToCultureInfo();

                    all_ALIENWARE_17_INFO.Add(language, JuliusSweetland.OptiKey.Properties.Resources.ALIENWARE_17_INFO);
                    all_GAZE_TRACKER_INFO.Add(language, JuliusSweetland.OptiKey.Properties.Resources.GAZE_TRACKER_INFO);
                    all_IRISBOND_DUO_INFO.Add(language, JuliusSweetland.OptiKey.Properties.Resources.IRISBOND_DUO_INFO);
                    all_IRISBOND_HIRU_INFO.Add(language, JuliusSweetland.OptiKey.Properties.Resources.IRISBOND_HIRU_INFO);
                    all_MOUSE_POSITION_INFO.Add(language, JuliusSweetland.OptiKey.Properties.Resources.MOUSE_POSITION_INFO);
                    all_TOBII_EYEX_INFO.Add(language, JuliusSweetland.OptiKey.Properties.Resources.TOBII_EYEX_INFO);
                    all_TOBII_ASSISTIVE_INFO.Add(language, JuliusSweetland.OptiKey.Properties.Resources.TOBII_ASSISTIVE_INFO);
                    all_VI_MYGAZE_INFO.Add(language, JuliusSweetland.OptiKey.Properties.Resources.VI_MYGAZE_INFO);
                    all_MOUSE_POSITION.Add(language, JuliusSweetland.OptiKey.Properties.Resources.MOUSE_POSITION);
                }

                // pre-amble
                file.WriteLine("// This is an auto-generated file, see InstallerTranslations console app");
                file.WriteLine("using System.Collections.Generic;");
                file.WriteLine("using System.Globalization;");
                file.WriteLine("using JuliusSweetland.OptiKey.Enums;");
                file.WriteLine("");
                file.WriteLine("namespace JuliusSweetland");
                file.WriteLine("{");
                file.WriteLine("\tpublic class InstallerStrings");
                file.WriteLine("\t{");

                // public member vars
                WriteDictInitialiser(file, all_ALIENWARE_17_INFO, "ALIENWARE_17_INFO");
                WriteDictInitialiser(file, all_GAZE_TRACKER_INFO, "GAZE_TRACKER_INFO");
                WriteDictInitialiser(file, all_IRISBOND_DUO_INFO, "IRISBOND_DUO_INFO");
                WriteDictInitialiser(file, all_IRISBOND_HIRU_INFO, "IRISBOND_HIRU_INFO");
                WriteDictInitialiser(file, all_MOUSE_POSITION_INFO, "MOUSE_POSITION_INFO");
                WriteDictInitialiser(file, all_TOBII_EYEX_INFO, "TOBII_EYEX_INFO");
                WriteDictInitialiser(file, all_TOBII_ASSISTIVE_INFO, "TOBII_ASSISTIVE_INFO");
                WriteDictInitialiser(file, all_VI_MYGAZE_INFO, "VI_MYGAZE_INFO");

                // post-amble
                file.WriteLine("\t}");
                file.WriteLine("}");

                Console.WriteLine("Strings extracted...");
            }
        }

        // Write a dict out to a class 
        static private void WriteDictInitialiser(System.IO.StreamWriter file,
            Dictionary<Languages, string> dict,
            string dictName)
        {
            string initString = "\t\tpublic static Dictionary<CultureInfo, string> {0} = new Dictionary<CultureInfo, string>()";
            string entryString = "\t\t\t{{ Languages.{0}.ToCultureInfo(), \"{1}\" }},";

            file.WriteLine(initString, dictName);
            file.WriteLine("\t\t{");
            foreach (KeyValuePair<Languages, string> entry in dict)
            {
                string value = entry.Value;
                value = value.Replace("\"", "\\\""); // escape any double quotes
                file.WriteLine(entryString, entry.Key, value);
            }
            file.WriteLine("\t\t};");
        }
    }
}
