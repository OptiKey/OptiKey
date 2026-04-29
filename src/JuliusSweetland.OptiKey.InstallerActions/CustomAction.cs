using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Deployment.WindowsInstaller;

namespace JuliusSweetland.OptiKey.InstallerActions
{
    public class CustomActions
    {
        // Culture codes for all OptiKey-supported languages, derived from Languages.ToCultureInfo().
        // Update this list whenever a new language is added to the Languages enum.
        private static readonly string[] SupportedCultureCodes =
        {
            "ca-ES", "zh-CN", "zh-TW", "hr-HR", "cs-CZ", "da-DK",
            "nl-BE", "nl-NL", "en-CA", "en-GB", "en-US", "fi-FI",
            "fr-CA", "fr-FR", "ka-GE", "de-DE", "el-GR", "he-IL",
            "hi-IN", "hu-HU", "it-IT", "ja-JP", "ko-KR", "fa-IR",
            "pl-PL", "pt-PT", "ru-RU", "sr-Cyrl-RS", "sk-SK", "sl-SI",
            "es-ES", "tr-TR", "uk-UA", "ur-PK",
        };

        // Find the best matching OptiKey culture code for the given system culture.
        public static string GetDefaultLanguageCode(CultureInfo cultureInfo)
        {
            // Hard-coded defaults for language families with multiple regional variants.
            var countryDefaults = new Dictionary<string, string>
            {
                { "en", "en-GB" },
                { "fr", "fr-FR" },
                { "nl", "nl-NL" },
                { "zh", "zh-CN" },
            };

            // All supported cultures as split parts (e.g. "en-GB" → ["en", "GB"]).
            List<string[]> languages = SupportedCultureCodes.Select(c => c.Split('-')).ToList();

            string sysLanguageCode = cultureInfo.Name;
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
                    string sysLanguage = sysLanguageParts[0];
                    if (countryDefaults.ContainsKey(sysLanguage))
                        return countryDefaults[sysLanguage];
                    else
                        return String.Join("-", languages[0]);
            }
        }

        [CustomAction]
        public static ActionResult EyeTrackerComboSelected(Session session)
        {
            // The combo box Value is the PointsSources enum name (set at build time by
            // InstallerTranslations --patch-msi), or "OtherEyeTracker" for unlisted devices.
            string trackerValue = session["COMBO_EYE_TRACKER"];

            string closestCode = GetDefaultLanguageCode(CultureInfo.CurrentCulture);
            CultureInfo closestCulture = new CultureInfo(closestCode);

            string infoText = "";
            string infoTextEn = "";
            string enumForConfig = trackerValue;

            if (trackerValue == "OtherEyeTracker")
            {
                // Sentinel value used in the combo for "not listed" — show OTHER_TRACKER info
                // text but write MousePosition to config so OptiKey at least starts.
                enumForConfig = "MousePosition";
                infoText   = GetLocalised(InstallerStrings.OTHER_TRACKER, closestCulture);
                infoTextEn = GetLocalised(InstallerStrings.OTHER_TRACKER, new CultureInfo("en-GB"));
                if (infoText == infoTextEn) infoTextEn = "";
            }
            else if (!string.IsNullOrEmpty(trackerValue))
            {
                infoText   = GetPointsSourceDetails(trackerValue, closestCulture);
                infoTextEn = GetPointsSourceDetails(trackerValue, new CultureInfo("en-GB"));
                if (infoText == infoTextEn)
                    infoTextEn = "";
            }

            session["EYETRACKER_TEXT"]     = infoText;
            session["EYETRACKER_TEXT_EN"]  = infoTextEn;
            session["EYETRACKER_SELECTED"] = enumForConfig;

            if (trackerValue == "TouchScreenPosition")
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

        private static string GetLocalised(Dictionary<CultureInfo, string> dict, CultureInfo culture)
        {
            string value;
            return dict.TryGetValue(culture, out value) ? value.Replace("\\n", "\n") : "";
        }

        private static string GetPointsSourceDetails(string trackerValue, CultureInfo culture)
        {
            Dictionary<CultureInfo, string> dict;
            switch (trackerValue)
            {
                case "GazeTracker":      dict = InstallerStrings.GAZE_TRACKER_INFO;    break;
                case "IrisbondDuo":      dict = InstallerStrings.IRISBOND_DUO_INFO;    break;
                case "IrisbondHiru":     dict = InstallerStrings.IRISBOND_HIRU_INFO;   break;
                case "MousePosition":    dict = InstallerStrings.MOUSE_POSITION_INFO;  break;
                case "TobiiPcEyeGo":
                case "TobiiPcEyeGoPlus":
                case "TobiiPcEyeMini":
                case "TobiiX2_30":
                case "TobiiX2_60":       dict = InstallerStrings.TOBII_ASSISTIVE_INFO; break;
                default:                 return "";
            }
            return GetLocalised(dict, culture);
        }
    }
}
