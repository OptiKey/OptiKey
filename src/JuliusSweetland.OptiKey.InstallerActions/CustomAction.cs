using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.UI.ViewModels.Management;
using Microsoft.Deployment.WindowsInstaller;

namespace JuliusSweetland.OptiKey.InstallerActions
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

        // Find best match in Optikey for a particular culture
        public static string GetDefaultLanguageCode(CultureInfo cultureInfo)
        {

            // Some hard-coded defaults for language flavours to use if language matches but country doesn't
            // (these are the languages we have multi-country support for)
            Dictionary<string, string> countryDefaults = new Dictionary<string, string>
            {
                { "en", "en-GB" },
                { "fr", "fr-FR" },
                { "nl", "nl-NL" }
            };

            // Get list of available languages to choose from, as language tag in parts (e.g. en-GB gives ['en', 'GB'])
            List<KeyValuePair<string, Languages>> languagePairs = WordsViewModel.Languages;
            List<string[]> languages = (from kvp in languagePairs select kvp.Value.ToCultureInfo().Name.Split('-')).ToList();

            string sysLanguageCode = cultureInfo.Name; //  (e.g. en-GB)
            string[] sysLanguageParts = sysLanguageCode.Split('-');

            // We'll remove non-matching languages at increasing specificity
            // e.g. language first, then country and any other specifiers
            List<string[]> matchingLanguages = languages;
            int idx = 0;
            while (matchingLanguages.Count > 0 && idx < sysLanguageParts.Length)
            {
                Predicate<string[]> matches = (parts) => (parts.Length > idx) && parts[idx].Equals(sysLanguageParts[idx]);
                matchingLanguages = new List<string[]>(languages.FindAll(matches));
                if (idx == 0 || matchingLanguages.Count > 0) // after idx == 0, previous pass is acceptable if nothing matches current pass
                    languages = matchingLanguages;
                idx++;
            }

            // Now languages list contains zero, one or more viable options. 
            switch (languages.Count)
            {
                case 0:
                    // Fall-back is English if nothing matches at all
                    return "en-GB";
                case 1:
                    return String.Join("-", languages[0]);
                default:
                    //Ambiguous match
                    string sysCountry = sysLanguageParts[0];
                    if (countryDefaults.ContainsKey(sysCountry))
                    {
                        // We've hardcoded a preference for this language
                        return countryDefaults[sysCountry];
                    }
                    else
                    {
                        // Still ambiguous, just take first match
                        return String.Join("-", languages[0]);
                    }
            }
        }


        [CustomAction]
        public static ActionResult PopulateEyetrackersCombo(Session session)
        {
            session.Log("Begin PopulateEyetrackersCombo");

            string closestLanguageCode = GetDefaultLanguageCode(CultureInfo.CurrentCulture);
            CultureInfo closestCulture = new CultureInfo(closestLanguageCode);

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

                // also the extended info, translated and original text 
                string details = GetPointsSourceDetails(tracker.Value, closestCulture);
                string details_english = GetPointsSourceDetails(tracker.Value, new CultureInfo("en-GB"));
                if (details == details_english)
                {
                    details_english = "";
                }
                else
                {
                    details_english = "Automatically translated from original text:\n" + details_english;
                }
            
                session["TRACKERINFO_" + SanitisePropName(trackerLabel)] = details;
                session["TRACKERINFO_EN_" + SanitisePropName(trackerLabel)] = details_english;

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

            // Try to match default language to system language
            string defaultLanguageCode = GetDefaultLanguageCode(CultureInfo.CurrentCulture);
            string defaultLanguage = "";

            // Construct property for combo data. 
            string comboData = ""; // we'll append to this as we go
            foreach (KeyValuePair<string, Languages> language in languages)
            {
                string languageLabel = language.Key; // includes "english description (location) / native description (location)"
                string languageEnum = language.Value.ToString();

                // add to combobox prop
                comboData = AppendItemToComboData(comboData, languageLabel);

                // save the mapping from label to enum in an installer property
                string languageLabelEnglish = languageLabel.Split('/')[0];
                session["LANGUAGE_" + SanitisePropName(languageLabelEnglish)] = languageEnum;

                // is default?
                if (language.Value.ToCultureInfo().Name.Equals(defaultLanguageCode))
                {
                    defaultLanguage = languageLabel;
                }
            }

            // Set combobox data
            session["LANGUAGE_COMBO_DATA"] = comboData;
            session["LANGUAGE_COMBO_DEFAULT"] = defaultLanguage;

            return ActionResult.Success;
        }

        private static string GetPointsSourceDetails(PointsSources pointSource, CultureInfo culture)
        {
            switch (pointSource)
            {
                // TODO check for culture not in dict, default empty?
                case PointsSources.Alienware17: return InstallerStrings.ALIENWARE_17_INFO[culture];
                case PointsSources.GazeTracker: return InstallerStrings.GAZE_TRACKER_INFO[culture];
                case PointsSources.IrisbondDuo: return InstallerStrings.IRISBOND_DUO_INFO[culture];
                case PointsSources.MousePosition: return InstallerStrings.MOUSE_POSITION_INFO[culture];
                case PointsSources.TobiiEyeX: return InstallerStrings.TOBII_EYEX_INFO[culture];
                case PointsSources.TobiiEyeTracker4C: return InstallerStrings.TOBII_EYEX_INFO[culture];
                case PointsSources.TobiiEyeTracker5: return InstallerStrings.TOBII_EYEX_INFO[culture];
                case PointsSources.TobiiRex: return InstallerStrings.TOBII_ASSISTIVE_INFO[culture];
                case PointsSources.TobiiPcEyeGo: return InstallerStrings.TOBII_ASSISTIVE_INFO[culture];
                case PointsSources.TobiiPcEyeGoPlus: return InstallerStrings.TOBII_ASSISTIVE_INFO[culture];
                case PointsSources.TobiiPcEyeMini: return InstallerStrings.TOBII_ASSISTIVE_INFO[culture];
                case PointsSources.TobiiX2_30: return InstallerStrings.TOBII_ASSISTIVE_INFO[culture];
                case PointsSources.TobiiX2_60: return InstallerStrings.TOBII_ASSISTIVE_INFO[culture];
                case PointsSources.VisualInteractionMyGaze: return InstallerStrings.VI_MYGAZE_INFO[culture];
                default: return "";
            }

        }


    }
}
