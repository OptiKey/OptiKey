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

        public static string GetDefaultLanguage(string sysLanguageAndCountry)
        {
            // Some hard-coded defaults for language flavours to use if language matches but country doesn't
            // (these are the languages we have multi-country support for)
            Dictionary<string, string> countryDefaults = new Dictionary<string, string>
            {
                { "English", "UK" },
                { "French", "France" },
                { "Dutch", "Netherlands" }
            };

            // Get list of available languages to choose from
            List<KeyValuePair<string, Languages>> languagePairs = WordsViewModel.Languages;
            List<string> languages = (from kvp in languagePairs select kvp.Key).ToList();
            
            // Fall-back is always English if we don't find better match
            string defaultLanguage = "English (UK)";

            if (String.IsNullOrEmpty(sysLanguageAndCountry))
            {
                return defaultLanguage;
            }
            else
            {
                // Split system language into language and (optional) country(-ies)
                char[] delimiterChars = { ' ', '(', ')', ',' };
                string[] parts = sysLanguageAndCountry.Split(delimiterChars, System.StringSplitOptions.RemoveEmptyEntries);
                string sysLanguage = parts[0];
                var sysCountries = parts.ToList();
                sysCountries.RemoveAt(0);
                
                // Look for language match(es)
                Predicate<string> containsLanguage = s => s.Contains(sysLanguage);
                List<string> matchingLanguages = new List<string>(languages.FindAll(containsLanguage));

                if (matchingLanguages.Count == 0)
                {
                    return defaultLanguage;
                }
                else if (matchingLanguages.Count == 1)
                {
                    return matchingLanguages[0];
                }
                else
                {
                    // Fallback to first entry if we can't do better
                    defaultLanguage = matchingLanguages[0];

                    // Check for matching country
                    Predicate<string> containsCountry = s => sysCountries.Any(s.Contains);
                    string matchingCountry = languages.Find(containsCountry);

                    if (!String.IsNullOrEmpty(matchingCountry))
                    {
                        return matchingCountry;
                    }
                    else
                    {
                        // Try to find 'best match' in our defaults
                        if (countryDefaults.ContainsKey(sysLanguage))
                        {
                            var defaultCountry = countryDefaults[sysLanguage];
                            Predicate<string> containsDefaultCountry = s => s.Contains(defaultCountry);
                            string matchingDefaultCountry = languages.Find(containsDefaultCountry);
                            if (!String.IsNullOrEmpty(matchingDefaultCountry))
                            {
                                return matchingDefaultCountry;
                            }
                        }
                        return defaultLanguage;
                    }
                }
            }
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
            foreach (KeyValuePair<string, Languages> language in languages)
            {
                string languageLabel = language.Key; // includes "english description (location) / native description (location)"
                string languageEnum = language.Value.ToString();

                // add to combobox prop
                comboData = AppendItemToComboData(comboData, languageLabel);

                // save the mapping from label to enum in an installer property
                string languageLabelEnglish = languageLabel.Split('/')[0];
                session["LANGUAGE_" + SanitisePropName(languageLabelEnglish)] = languageEnum;
            }

            // Try to match default language to system language
            string cultureName = CultureInfo.CurrentCulture.EnglishName;
            string defaultLanguage = GetDefaultLanguage(cultureName);
            session.Log("System language is "+ cultureName);

            // Set combobox data
            session["LANGUAGE_COMBO_DATA"] = comboData;
            session["LANGUAGE_COMBO_DEFAULT"] = defaultLanguage;

            return ActionResult.Success;
        }


    }
}
