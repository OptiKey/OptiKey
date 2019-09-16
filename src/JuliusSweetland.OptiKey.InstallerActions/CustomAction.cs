using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Text;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.UI.ViewModels.Management;
using Microsoft.Deployment.WindowsInstaller;

namespace InstallerActions
{
    public class CustomActions
    {

        private static string AppendItemToComboData(string combodata, string item)
        {
            const string sep1 = "|";
            const string sep2 = "#";
            combodata += sep1 + item + sep2 + item;
            return combodata;
        }

        [CustomAction]
        public static ActionResult LoadOptikeyProperties(Session session)
        {
            // Load Optikey installer options into installer properties
            // Calling this DLL is quite expensive (slow) because it depends on all of Optikey,
            // so this one action gets everything out the way in one go and caches in installer
            // properties
            
            session.Log("Begin LoadOptikeyProperties");

            PopulateEyetrackersCombo(session);
            PopulateLanguagesCombo(session);

            return ActionResult.Success;
        }

        public static string SanitisePropName(string prop_name)
        {
            prop_name = prop_name.Replace(" ", "");
            prop_name = prop_name.Replace(")", "");
            prop_name = prop_name.Replace("(", "");
            return prop_name;
        }


        [CustomAction]
        public static ActionResult PopulateEyetrackersCombo(Session session)
        {
            session.Log("Begin PopulateEyetrackersCombo");

            // Get list of available eyetrackers from PointingAndSelectingViewModel
            List<KeyValuePair<string, PointsSources>> trackers = PointingAndSelectingViewModel.PointsSources;

            string comboData = ""; // we'll append to this as we go
            string defaultTracker = "";
            foreach (KeyValuePair<string, PointsSources> tracker in trackers)
            {
                string trackerLabel = tracker.Key;
                string trackerEnum = tracker.Value.ToString();

                // add to combobox prop
                comboData = AppendItemToComboData(comboData, trackerLabel);

                // save the mapping from label to enum in an installer property
                session["TRACKER_" + SanitisePropName(trackerLabel)] = trackerEnum;

                if (trackerLabel.Contains("Mouse"))
                {
                    defaultTracker = trackerLabel;
                }
            }

            // Set combobox data
            session["EYETRACKER_COMBO_DATA"] = comboData;
            session["EYETRACKER_COMBO_DEFAULT"] = defaultTracker;

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult PopulateLanguagesCombo(Session session)
        {
            session.Log("Begin PopulateLanguagesCombo");

            // Get list of available languages from WordsViewModel
            List<KeyValuePair<string, Languages>> languages = WordsViewModel.Languages;

            // Construct property for combo data. 
            string comboData = ""; // we'll append to this as we go
            string defaultLanguage = "";
            foreach (KeyValuePair<string, Languages> language in languages)
            {
                string languageLabel = language.Key; // includes "english description (location) / native description (location)"
                string languageEnum = language.Value.ToString();

                // add to combobox prop
                comboData = AppendItemToComboData(comboData, languageLabel);

                // save the mapping from label to enum in an installer property
                string languageLabelEnglish = languageLabel.Split('/')[0];
                session["LANGUAGE_" + SanitisePropName(languageLabelEnglish)] = languageEnum;

                if (languageLabel.Contains("English") && languageLabel.Contains("UK"))
                {
                    defaultLanguage = languageLabel;
                }
            }

            // Set combobox data
            session["LANGUAGE_COMBO_DATA"] = comboData;
            session["LANGUAGE_COMBO_DEFAULT"] = defaultLanguage;

            return ActionResult.Success;
        }


    }
}
