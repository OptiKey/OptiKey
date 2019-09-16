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
        [CustomAction]
        public static ActionResult CustomAction1(Session session)
        {

            // sending message to installation log
            session.Log("Begin CustomAction1");

            List<KeyValuePair<string, PointsSources>> ps = PointingAndSelectingViewModel.PointsSources;
            foreach (KeyValuePair<string, PointsSources> source in ps)
            {
                session.Log(source.Key + ": " + source.Value);
                session["TEST_TEXT"] = source.Key + ": " + source.Value;
            }

            // getting a property
            string Language = session["LANGUAGE_SELECTED"];

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

            // Construct property for combo data. 
            // Format is:
            // Property|Value1#Text1|Value2#Text2|Value3#Text3|...
            const string sep1 = "|";
            const string sep2 = "#";

            const string comboProp = "COMBO_EYE_TRACKER";
            String comboData = comboProp; // we'll append to this as we go
            foreach (KeyValuePair<string, PointsSources> tracker in trackers)
            {
                session.Log(tracker.Key + ": " + tracker.Value);

                // save the mapping from label to enum in an installer property
                string trackerLabel = tracker.Key;
                comboData += sep1 + trackerLabel + sep2 + trackerLabel;

                // store attached data in an installer property
                session["TRACKER_" + SanitisePropName(trackerLabel)] = "" + tracker.Value;

                if (trackerLabel.Contains("Mouse"))
                {
                    session[comboProp] = trackerLabel;
                }
            }

            // Set combobox data
            session["AI_COMBOBOX_DATA"] = comboData;

            // Invoke the action that populates with this data
            session.DoAction("PopulateComboBox");

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult PopulateLanguagesCombo(Session session)
        {
            session.Log("Begin PopulateLanguagesCombo");

            // Get list of available languages from WordsViewModel
            List<KeyValuePair<string, Languages>> languages = WordsViewModel.Languages;


            // Construct property for combo data. 
            // Format is
            // Property|Value1#Text1|Value2#Text2|Value3#Text3|...
            const string sep1 = "|";
            const string sep2 = "#";

            const string comboProp = "COMBO_LANGUAGE";
            String comboData = comboProp; // we'll append to this as we go
            foreach (KeyValuePair<string, Languages> language in languages)
            {
                session.Log(language.Key + ": " + language.Value);

                // key is full language description, e.g. "Dutch (Belgium) / Nederlands (België)" [may include unicode]
                // value is enum, e.g. DutchBelgium
                

                // save the mapping from label to enum in an installer property
                string lang_label = language.Key;
                comboData += sep1 + lang_label + sep2 + lang_label;

                // store attached data in an installer property
                string lang_name_en = lang_label.Split('/')[0];

                session["LANGUAGE_" + SanitisePropName(lang_name_en)] = "" + language.Value;

                if (lang_label.Contains("English") && lang_label.Contains("UK"))
                {
                    session[comboProp] = lang_label;
                }
            }

            // Set combobox data
            session["AI_COMBOBOX_DATA"] = comboData;

            // Invoke the action that populates with this data
            session.DoAction("PopulateComboBox");

            return ActionResult.Success;
        }


    }
}
