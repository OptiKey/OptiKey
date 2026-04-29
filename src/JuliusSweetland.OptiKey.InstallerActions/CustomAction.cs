using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.UI.ViewModels.Management;
using Microsoft.Deployment.WindowsInstaller;

namespace JuliusSweetland.OptiKey.InstallerActions
{
    public class CustomActions
    {
        // Find best match in Optikey for a particular culture
        public static string GetDefaultLanguageCode(CultureInfo cultureInfo)
        {
            // Some hard-coded defaults for language flavours to use if language matches but country doesn't
            // (these are the languages we have multi-country support for)
            Dictionary<string, string> countryDefaults = new Dictionary<string, string>
            {
                { "en", "en-GB" },
                { "fr", "fr-FR" },
                { "nl", "nl-NL" },
                { "zh", "zh-CN" },
            };

            // Get list of available languages to choose from, as language tag in parts (e.g. en-GB gives ['en', 'GB'])
            List<KeyValuePair<string, Languages>> languagePairs = WordsViewModel.KeyboardLanguages;
            List<string[]> languages = (from kvp in languagePairs select kvp.Value.ToCultureInfo().Name.Split('-')).ToList();

            string sysLanguageCode = cultureInfo.Name; // (e.g. en-GB)
            string[] sysLanguageParts = sysLanguageCode.Split('-');

            List<string[]> matchingLanguages = languages;
            int idx = 0;
            while (matchingLanguages.Count > 0 && idx < sysLanguageParts.Length)
            {
                Predicate<string[]> matches = (parts) => (parts.Length > idx) && parts[idx].Equals(sysLanguageParts[idx]);
                matchingLanguages = new List<string[]>(languages.FindAll(matches));
                if (idx == 0 || matchingLanguages.Count > 0)
                    languages = matchingLanguages;
                idx++;
            }

            switch (languages.Count)
            {
                case 0:
                    return "en-GB";
                case 1:
                    return String.Join("-", languages[0]);
                default:
                    string sysCountry = sysLanguageParts[0];
                    if (countryDefaults.ContainsKey(sysCountry))
                        return countryDefaults[sysCountry];
                    else
                        return String.Join("-", languages[0]);
            }
        }

        [CustomAction]
        public static ActionResult EyeTrackerComboSelected(Session session)
        {
            // The combo box Value is now the enum string directly (set at build time by
            // InstallerTranslations --patch-msi), so no runtime label-to-enum lookup is needed.
            string trackerEnum = session["COMBO_EYE_TRACKER"];

            string closestCode = GetDefaultLanguageCode(CultureInfo.CurrentCulture);
            CultureInfo closestCulture = new CultureInfo(closestCode);

            string infoText = "";
            string infoTextEn = "";
            string enumForConfig = trackerEnum;

            if (trackerEnum == "OtherEyeTracker")
            {
                // Sentinel value used in the combo for "not listed" — show OTHER_TRACKER info
                // text but write MousePosition to config so OptiKey at least starts.
                enumForConfig = PointsSources.MousePosition.ToString();
                infoText   = InstallerStrings.OTHER_TRACKER.GetValueOrDefault(closestCulture, "").Replace("\\n", "\n");
                infoTextEn = InstallerStrings.OTHER_TRACKER.GetValueOrDefault(new CultureInfo("en-GB"), "").Replace("\\n", "\n");
                if (infoText == infoTextEn) infoTextEn = "";
            }
            else
            {
                PointsSources pointSource;
                if (Enum.TryParse(trackerEnum, out pointSource))
                {
                    infoText   = GetPointsSourceDetails(pointSource, closestCulture).Replace("\\n", "\n");
                    infoTextEn = GetPointsSourceDetails(pointSource, new CultureInfo("en-GB")).Replace("\\n", "\n");
                    if (infoText == infoTextEn)
                        infoTextEn = "";
                }
            }

            session["EYETRACKER_TEXT"]     = infoText;
            session["EYETRACKER_TEXT_EN"]  = infoTextEn;
            session["EYETRACKER_SELECTED"] = enumForConfig;

            if (trackerEnum == "TouchScreenPosition")
            {
                session["SELECTED_KEYSELECTIONTRIGGERSOURCE"]          = "TouchDownUps";
                session["SELECTED_POINTSELECTIONTRIGGERSOURCE"]        = "TouchDownUps";
                session["SELECTED_MINDWELLTIME"]                       = "00:00:00.0500000";
                session["SELECTED_MULTIKEYSELECTIONTRIGGERSTOPSIGNAL"] = "NextLow";
            }
            else
            {
                session["SELECTED_KEYSELECTIONTRIGGERSOURCE"]          = "Fixations";
                session["SELECTED_POINTSELECTIONTRIGGERSOURCE"]        = "Fixations";
                session["SELECTED_MINDWELLTIME"]                       = "00:00:00.2500000";
                session["SELECTED_MULTIKEYSELECTIONTRIGGERSTOPSIGNAL"] = "NextHigh";
            }
            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult LanguageComboSelected(Session session)
        {
            const string defaultFontFamily  = "/Resources/Fonts/#Roboto";
            const string defaultFontStretch = "Condensed";
            const string defaultFontWeight  = "Light";

            // The combo box Value is now the enum string directly (set at build time by
            // InstallerTranslations --patch-msi), so no runtime label-to-enum lookup is needed.
            string langEnum = session["COMBO_LANGUAGE"];

            session["KEYBOARD_LANGUAGE_SELECTED"] = langEnum;
            session["UI_LANGUAGE_SELECTED"] = langEnum.Contains("Chinese")
                ? "ChineseTraditionalTaiwan"
                : langEnum;

            if (langEnum == "PersianIran")
            {
                session["SELECTED_FONTFAMILY"]  = "/Resources/Fonts/#Nafees Web Naskh";
                session["SELECTED_FONTSTRETCH"] = "Normal";
                session["SELECTED_FONTWEIGHT"]  = "Regular";
            }
            else if (langEnum == "UrduPakistan")
            {
                session["SELECTED_FONTFAMILY"]  = "/Resources/Fonts/#Nazli";
                session["SELECTED_FONTSTRETCH"] = "Normal";
                session["SELECTED_FONTWEIGHT"]  = "Regular";
            }
            else
            {
                session["SELECTED_FONTFAMILY"]  = defaultFontFamily;
                session["SELECTED_FONTSTRETCH"] = defaultFontStretch;
                session["SELECTED_FONTWEIGHT"]  = defaultFontWeight;
            }
            return ActionResult.Success;
        }

        private static string GetPointsSourceDetails(PointsSources pointSource, CultureInfo culture)
        {
            try
            {
                switch (pointSource)
                {
                    case PointsSources.GazeTracker:    return InstallerStrings.GAZE_TRACKER_INFO[culture];
                    case PointsSources.IrisbondDuo:    return InstallerStrings.IRISBOND_DUO_INFO[culture];
                    case PointsSources.IrisbondHiru:   return InstallerStrings.IRISBOND_HIRU_INFO[culture];
                    case PointsSources.MousePosition:  return InstallerStrings.MOUSE_POSITION_INFO[culture];
                    case PointsSources.TobiiPcEyeGo:
                    case PointsSources.TobiiPcEyeGoPlus:
                    case PointsSources.TobiiPcEyeMini:
                    case PointsSources.TobiiX2_30:
                    case PointsSources.TobiiX2_60:     return InstallerStrings.TOBII_ASSISTIVE_INFO[culture];
                    default:                            return "";
                }
            }
            catch (KeyNotFoundException)
            {
                return "";
            }
        }
    }
}
