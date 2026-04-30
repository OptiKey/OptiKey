using System;
using System.Linq;
using System.Collections.Generic;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.UI.ViewModels.Management;
using System.Globalization;
using Microsoft.Deployment.WindowsInstaller;

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
        //
        // This tool also patches combo box data (languages and eye trackers) directly into a
        // built MSI, so that the heavy InstallerActions DLL no longer needs to populate them
        // at install runtime.
        //
        // Usage:
        //   InstallerTranslations.exe                         -- generates InstallerStrings.cs only
        //   InstallerTranslations.exe --patch-msi <msi-path> -- also patches combo boxes into MSI

        static void Main(string[] args)
        {
            string msiPath = null;
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "--patch-msi")
                    msiPath = args[i + 1];
            }

            Console.WriteLine("Extracting internationalised strings...");
            GenerateInstallerStrings();
            Console.WriteLine("Strings extracted.");

            if (msiPath != null)
            {
                Console.WriteLine($"Patching combo boxes in: {msiPath}");
                PatchMsiComboBoxes(msiPath);
                Console.WriteLine("MSI patched successfully.");
            }
        }

        static void GenerateInstallerStrings()
        {
            using (var file = new System.IO.StreamWriter("InstallerStrings.cs"))
            {
                // Create multilingual dicts for key installer strings
                var all_ALIENWARE_17_INFO    = new Dictionary<Languages, string>();
                var all_GAZE_TRACKER_INFO    = new Dictionary<Languages, string>();
                var all_IRISBOND_DUO_INFO    = new Dictionary<Languages, string>();
                var all_IRISBOND_HIRU_INFO   = new Dictionary<Languages, string>();
                var all_MOUSE_POSITION_INFO  = new Dictionary<Languages, string>();
                var all_TOBII_EYEX_INFO      = new Dictionary<Languages, string>();
                var all_TOBII_ASSISTIVE_INFO = new Dictionary<Languages, string>();
                var all_OTHER_TRACKER        = new Dictionary<Languages, string>();

                var languages = Enum.GetValues(typeof(Languages)).Cast<Languages>().ToList();
                var translatedCultures = new HashSet<CultureInfo>();

                foreach (Languages language in languages)
                {
                    var culture = language.ToCultureInfo();
                    JuliusSweetland.OptiKey.Properties.Resources.Culture = culture;

                    if (!translatedCultures.Contains(culture))
                    {
                        all_ALIENWARE_17_INFO.Add(language,    JuliusSweetland.OptiKey.Properties.Resources.ALIENWARE_17_INFO);
                        all_GAZE_TRACKER_INFO.Add(language,    JuliusSweetland.OptiKey.Properties.Resources.GAZE_TRACKER_INFO);
                        all_IRISBOND_DUO_INFO.Add(language,    JuliusSweetland.OptiKey.Properties.Resources.IRISBOND_DUO_INFO);
                        all_IRISBOND_HIRU_INFO.Add(language,   JuliusSweetland.OptiKey.Properties.Resources.IRISBOND_HIRU_INFO);
                        all_MOUSE_POSITION_INFO.Add(language,  JuliusSweetland.OptiKey.Properties.Resources.MOUSE_POSITION_INFO);
                        all_TOBII_EYEX_INFO.Add(language,      JuliusSweetland.OptiKey.Properties.Resources.TOBII_EYEX_INFO);
                        all_TOBII_ASSISTIVE_INFO.Add(language,  JuliusSweetland.OptiKey.Properties.Resources.TOBII_ASSISTIVE_INFO);
                        all_OTHER_TRACKER.Add(language,        JuliusSweetland.OptiKey.Properties.Resources.OTHER_TRACKER);

                        translatedCultures.Add(culture);
                    }
                }

                file.WriteLine("// This is an auto-generated file, see InstallerTranslations console app");
                file.WriteLine("using System.Collections.Generic;");
                file.WriteLine("using System.Globalization;");
                file.WriteLine("");
                file.WriteLine("namespace JuliusSweetland");
                file.WriteLine("{");
                file.WriteLine("\tpublic class InstallerStrings");
                file.WriteLine("\t{");

                WriteDictInitialiser(file, all_ALIENWARE_17_INFO,    "ALIENWARE_17_INFO");
                WriteDictInitialiser(file, all_GAZE_TRACKER_INFO,    "GAZE_TRACKER_INFO");
                WriteDictInitialiser(file, all_IRISBOND_DUO_INFO,    "IRISBOND_DUO_INFO");
                WriteDictInitialiser(file, all_IRISBOND_HIRU_INFO,   "IRISBOND_HIRU_INFO");
                WriteDictInitialiser(file, all_MOUSE_POSITION_INFO,  "MOUSE_POSITION_INFO");
                WriteDictInitialiser(file, all_TOBII_EYEX_INFO,      "TOBII_EYEX_INFO");
                WriteDictInitialiser(file, all_TOBII_ASSISTIVE_INFO, "TOBII_ASSISTIVE_INFO");
                WriteDictInitialiser(file, all_OTHER_TRACKER,        "OTHER_TRACKER");

                file.WriteLine("\t}");
                file.WriteLine("}");
            }
        }

        static void PatchMsiComboBoxes(string msiPath)
        {
            // Use English labels — these are what the user sees in the installer UI.
            // BundledPointsSources and KeyboardLanguages both read from Resources, so
            // we set culture to en-GB before calling them.
            JuliusSweetland.OptiKey.Properties.Resources.Culture = new CultureInfo("en-GB");

            var languages = WordsViewModel.KeyboardLanguages
                .Select(kvp => (Text: kvp.Key, Value: kvp.Value.ToString()))
                .ToList();

            var eyeTrackers = PointingAndSelectingViewModel.BundledPointsSources
                .Select(kvp => (Text: kvp.Key, Value: kvp.Value.ToString()))
                .ToList();
            // "Other" entry: user has an unlisted tracker. We use a distinct sentinel value
            // "OtherEyeTracker" (not a real enum) so CustomAction.cs can show the correct
            // OTHER_TRACKER info text, then map it to MousePosition when writing config.
            eyeTrackers.Add(("Other eye tracker (not listed)", "OtherEyeTracker"));

            string defaultTracker = eyeTrackers[0].Value;
            string defaultTrackerText = GetEnglishTrackerText(defaultTracker);

            using (var db = new Database(msiPath, DatabaseOpenMode.Transact))
            {
                PatchComboItems(db, "COMBO_LANGUAGE",    languages);
                PatchComboItems(db, "COMBO_EYE_TRACKER", eyeTrackers);

                // Set defaults so combos show a selection when dialogs first open.
                PatchProperty(db, "COMBO_LANGUAGE",    languages[0].Value);
                PatchProperty(db, "COMBO_EYE_TRACKER", defaultTracker);

                // Pre-set the info text for the default eye tracker so it is visible immediately
                // when the dialog opens, without needing any custom action to run first.
                PatchProperty(db, "EYETRACKER_TEXT",    defaultTrackerText);
                PatchProperty(db, "EYETRACKER_TEXT_EN", "");

                // Replace managed combo-onChange CA with fast Type-51 property CAs + fixed refresh.
                // Managed CA is moved to Next so it only runs once (no AppDomain per selection).
                PatchEyeTrackerDialog(db, eyeTrackers);
                PatchLanguageDialogAction(db);

                db.Commit();
            }
        }

        static string GetEnglishTrackerText(string trackerEnum)
        {
            // Resources.Culture is already set to en-GB by the caller.
            var r = JuliusSweetland.OptiKey.Properties.Resources.ResourceManager;
            string key;
            switch (trackerEnum)
            {
                case "GazeTracker":      key = "GAZE_TRACKER_INFO";    break;
                case "IrisbondDuo":      key = "IRISBOND_DUO_INFO";    break;
                case "IrisbondHiru":     key = "IRISBOND_HIRU_INFO";   break;
                case "MousePosition":    key = "MOUSE_POSITION_INFO";  break;
                case "OtherEyeTracker":  key = "OTHER_TRACKER";        break;
                case "TobiiPcEyeGo":
                case "TobiiPcEyeGoPlus":
                case "TobiiPcEyeMini":
                case "TobiiX2_30":
                case "TobiiX2_60":       key = "TOBII_ASSISTIVE_INFO"; break;
                default:                 return "";
            }
            return r.GetString(key, new CultureInfo("en-GB")) ?? "";
        }

        static void PatchEyeTrackerDialog(Database db, List<(string Text, string Value)> eyeTrackers)
        {
            const string dialog        = "EyeTracker";
            const string combo         = "MyComboBox";
            const string managedAction = "EyeTrackerComboSelected_3C91A0DD__1_2797DD9D_AE59_4839_8B66_70AB5FC1B8_7";
            const string textProp      = "EYETRACKER_TEXT";
            const string condition     = "AI_INSTALL";

            // Remove the managed DoAction and ALL [AiRefreshDlg] events from the combo.
            // The original [AiRefreshDlg] condition is "AI_INSTALL AND AI_BOOTSTRAPPER" which
            // never fires for a plain MSI install. We'll re-add it with just AI_INSTALL below.
            using (var del = db.OpenView(
                "SELECT `Dialog_`, `Control_`, `Event`, `Argument`, `Condition`, `Ordering` " +
                "FROM `ControlEvent` WHERE `Dialog_` = ? AND `Control_` = ?"))
            {
                del.Execute(new Record(dialog, combo));
                Record row;
                while ((row = del.Fetch()) != null)
                {
                    string evt = row.GetString(3);
                    string arg = row.GetString(4);
                    if ((evt == "DoAction" && arg == managedAction) || evt == "[AiRefreshDlg]")
                        del.Modify(ViewModifyMode.Delete, row);
                }
            }

            // Build a list of (CA name, info text, tracker value) for each tracker that has text.
            var textCAs = new List<(string Name, string InfoText, string TrackerValue)>();
            foreach (var tracker in eyeTrackers)
            {
                string text = GetEnglishTrackerText(tracker.Value);
                if (!string.IsNullOrEmpty(text))
                    textCAs.Add(("SetEyeTrackerText_" + tracker.Value, text, tracker.Value));
            }

            // Idempotent: remove any previously-patched CAs and their ControlEvents.
            foreach (var ca in textCAs)
            {
                using (var del = db.OpenView(
                    "SELECT `Action`, `Type`, `Source`, `Target` FROM `CustomAction` WHERE `Action` = ?"))
                {
                    del.Execute(new Record(ca.Name));
                    Record row;
                    while ((row = del.Fetch()) != null)
                        del.Modify(ViewModifyMode.Delete, row);
                }
                using (var del = db.OpenView(
                    "SELECT `Dialog_`, `Control_`, `Event`, `Argument`, `Condition`, `Ordering` " +
                    "FROM `ControlEvent` WHERE `Dialog_` = ? AND `Control_` = ? AND `Event` = 'DoAction' AND `Argument` = ?"))
                {
                    del.Execute(new Record(dialog, combo, ca.Name));
                    Record row;
                    while ((row = del.Fetch()) != null)
                        del.Modify(ViewModifyMode.Delete, row);
                }
            }

            // Add Type-51 CAs to the CustomAction table.
            // Type 51 sets a property to a fixed string value — no DLL loading, instantaneous.
            using (var ins = db.OpenView("SELECT `Action`, `Type`, `Source`, `Target` FROM `CustomAction`"))
            {
                ins.Execute();
                foreach (var ca in textCAs)
                {
                    var record = new Record(4);
                    record.SetString(1, ca.Name);
                    record.SetInteger(2, 51);
                    record.SetString(3, textProp);
                    record.SetString(4, ca.InfoText);
                    ins.Modify(ViewModifyMode.Insert, record);
                }
            }

            // Wire each Type-51 CA to the combo onChange via ControlEvents.
            // Then add [AiRefreshDlg] with AI_INSTALL (no AI_BOOTSTRAPPER) so the text redraws.
            using (var ins = db.OpenView(
                "SELECT `Dialog_`, `Control_`, `Event`, `Argument`, `Condition`, `Ordering` FROM `ControlEvent`"))
            {
                ins.Execute();
                int ordering = 10;
                foreach (var ca in textCAs)
                {
                    string cond = condition + " AND COMBO_EYE_TRACKER=\"" + ca.TrackerValue + "\"";
                    var record = new Record(6);
                    record.SetString(1, dialog);
                    record.SetString(2, combo);
                    record.SetString(3, "DoAction");
                    record.SetString(4, ca.Name);
                    record.SetString(5, cond);
                    record.SetInteger(6, ordering++);
                    ins.Modify(ViewModifyMode.Insert, record);
                }

                // [AiRefreshDlg] fires after the property is set, causing the text label to redraw.
                var refresh = new Record(6);
                refresh.SetString(1, dialog);
                refresh.SetString(2, combo);
                refresh.SetString(3, "[AiRefreshDlg]");
                refresh.SetString(4, "0");
                refresh.SetString(5, condition);
                refresh.SetInteger(6, ordering);
                ins.Modify(ViewModifyMode.Insert, refresh);
            }

            // Add the managed CA on the Next button so it runs once on navigation.
            // This sets EYETRACKER_SELECTED and the trigger settings needed for the install.
            using (var del = db.OpenView(
                "SELECT `Dialog_`, `Control_`, `Event`, `Argument`, `Condition`, `Ordering` " +
                "FROM `ControlEvent` WHERE `Dialog_` = ? AND `Control_` = ? AND `Event` = 'DoAction' AND `Argument` = ?"))
            {
                del.Execute(new Record(dialog, "Next", managedAction));
                Record row;
                while ((row = del.Fetch()) != null)
                    del.Modify(ViewModifyMode.Delete, row);
            }
            using (var ins = db.OpenView(
                "SELECT `Dialog_`, `Control_`, `Event`, `Argument`, `Condition`, `Ordering` FROM `ControlEvent`"))
            {
                ins.Execute();
                var record = new Record(6);
                record.SetString(1, dialog);
                record.SetString(2, "Next");
                record.SetString(3, "DoAction");
                record.SetString(4, managedAction);
                record.SetString(5, condition);
                record.SetInteger(6, 1);
                ins.Modify(ViewModifyMode.Insert, record);
            }
        }

        static void PatchLanguageDialogAction(Database db)
        {
            const string dialog        = "Languages";
            const string combo         = "ComboBox_Language";
            const string managedAction = "LanguageSelected_1AF024D1_2FE7_45A3_28831AEE_9A35_4BCF_8D01_A1F260618E_3";
            const string condition     = "AI_INSTALL";

            // Move the CA from combo onChange to Next. The CA only needs to run once;
            // there is no dynamic text area in the Languages dialog to update.
            using (var del = db.OpenView(
                "SELECT `Dialog_`, `Control_`, `Event`, `Argument`, `Condition`, `Ordering` " +
                "FROM `ControlEvent` WHERE `Dialog_` = ? AND `Control_` = ? AND `Event` = 'DoAction' AND `Argument` = ?"))
            {
                del.Execute(new Record(dialog, combo, managedAction));
                Record row;
                while ((row = del.Fetch()) != null)
                    del.Modify(ViewModifyMode.Delete, row);
            }
            using (var del = db.OpenView(
                "SELECT `Dialog_`, `Control_`, `Event`, `Argument`, `Condition`, `Ordering` " +
                "FROM `ControlEvent` WHERE `Dialog_` = ? AND `Control_` = ? AND `Event` = 'DoAction' AND `Argument` = ?"))
            {
                del.Execute(new Record(dialog, "Next", managedAction));
                Record row;
                while ((row = del.Fetch()) != null)
                    del.Modify(ViewModifyMode.Delete, row);
            }
            using (var ins = db.OpenView(
                "SELECT `Dialog_`, `Control_`, `Event`, `Argument`, `Condition`, `Ordering` FROM `ControlEvent`"))
            {
                ins.Execute();
                var record = new Record(6);
                record.SetString(1, dialog);
                record.SetString(2, "Next");
                record.SetString(3, "DoAction");
                record.SetString(4, managedAction);
                record.SetString(5, condition);
                record.SetInteger(6, 1);
                ins.Modify(ViewModifyMode.Insert, record);
            }
        }

        static void PatchProperty(Database db, string property, string value)
        {
            // MSI rejects Property rows with empty-string values (treated as "not set").
            // Delete any existing row; only re-insert if the value is non-empty.
            using (var del = db.OpenView("SELECT `Property`, `Value` FROM `Property` WHERE `Property` = ?"))
            {
                del.Execute(new Record(property));
                Record row;
                while ((row = del.Fetch()) != null)
                    del.Modify(ViewModifyMode.Delete, row);
            }
            if (string.IsNullOrEmpty(value)) return;
            using (var ins = db.OpenView("SELECT `Property`, `Value` FROM `Property`"))
            {
                ins.Execute();
                var record = new Record(2);
                record.SetString(1, property);
                record.SetString(2, value);
                ins.Modify(ViewModifyMode.Insert, record);
            }
        }

        static void PatchComboItems(Database db, string property,
            List<(string Text, string Value)> items)
        {
            // AI doesn't generate the ComboBox table if no items are defined in the .aip.
            // Create it if absent, otherwise delete existing rows for this property.
            if (!db.Tables.Contains("ComboBox"))
            {
                db.Execute(
                    "CREATE TABLE `ComboBox` (`Property` CHAR(72) NOT NULL, `Order` SHORT NOT NULL, " +
                    "`Value` CHAR(64) NOT NULL, `Text` CHAR(64) PRIMARY KEY `Property`, `Order`)");
            }
            else
            {
                // Delete via SELECT view + Modify(Delete) — more reliable than DELETE SQL in MSI.
                using (var del = db.OpenView("SELECT `Property`, `Order`, `Value`, `Text` FROM `ComboBox` WHERE `Property` = ?"))
                {
                    del.Execute(new Record(property));
                    Record row;
                    while ((row = del.Fetch()) != null)
                        del.Modify(ViewModifyMode.Delete, row);
                }
            }

            // Insert via SELECT view + Modify(Insert) — MSI SQL INSERT VALUES is not supported.
            using (var ins = db.OpenView("SELECT `Property`, `Order`, `Value`, `Text` FROM `ComboBox`"))
            {
                ins.Execute();
                int order = 1;
                foreach (var item in items)
                {
                    var record = new Record(4);
                    record.SetString(1, property);
                    record.SetInteger(2, order++);
                    record.SetString(3, item.Value);
                    record.SetString(4, item.Text);
                    ins.Modify(ViewModifyMode.Insert, record);
                }
            }
        }

        static private void WriteDictInitialiser(System.IO.StreamWriter file,
            Dictionary<Languages, string> dict, string dictName)
        {
            string initString = "\t\tpublic static Dictionary<CultureInfo, string> {0} = new Dictionary<CultureInfo, string>()";
            // Use the culture name string directly so InstallerStrings.cs has no dependency
            // on the Languages enum or any OptiKey Core types.
            string entryString = "\t\t\t{{ new CultureInfo(\"{0}\"), \"{1}\" }},";

            file.WriteLine(initString, dictName);
            file.WriteLine("\t\t{");
            foreach (KeyValuePair<Languages, string> entry in dict)
            {
                string value = entry.Value;
                value = value.Replace("\"", "\\\""); // escape any double quotes
                value = value.Replace("\r\n", "\\r\\n"); // escape newline
                file.WriteLine(entryString, entry.Key.ToCultureInfo().Name, value);
            }
            file.WriteLine("\t\t};");
        }
    }
}
