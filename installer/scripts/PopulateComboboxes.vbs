' Populate eye tracker combo box from loaded properties
Function PopulateEyeTrackerCombo()
  ' This is specified in the combo element in the AIP dialog
  Const comboProp = "COMBO_EYE_TRACKER" 

  ' These two properties have been populated earlier via the Optikey DLL
  Const comboDataProp = "EYETRACKER_COMBO_DATA"
  Const comboDefaultProp = "EYETRACKER_COMBO_DEFAULT" 

  PopulateComboFromProperties(comboProp), comboDataProp, comboDefaultProp

  EyeTrackerSelected()
End  Function


' Populate languages combo box from loaded properties
Function PopulateLanguagesCombo()
  ' This is specified in the combo element in the AIP dialog
  Const comboProp = "COMBO_LANGUAGE" 

  ' These two properties have been populated earlier via the Optikey DLL
  Const comboDataProp = "LANGUAGE_COMBO_DATA"
  Const comboDefaultProp = "LANGUAGE_COMBO_DEFAULT" 

  PopulateComboFromProperties(comboProp), comboDataProp, comboDefaultProp
  
  Log ("SYSTEM_LANGUAGE"), Session.Property("SYSTEM_LANGUAGE")

  LanguageSelected()

End  Function


' Helper to populate a combobox from existing property and default value
Function PopulateComboFromProperties(comboProp, comboDataProp, comboDefaultProp)   
  
  ' Don't repopulate if this has already been run
  If Len(Session.Property(comboProp)) > 0 Then
    Exit Function
  End If

  AIComboData = comboProp & Session.Property(comboDataProp)

  ' This property will be used by the PopulateComboBox action
  Session.Property("AI_COMBOBOX_DATA") = AIComboData

  ' Directly set the current selection
  Session.Property(comboProp) = Session.Property(comboDefaultProp)
  
  ' Invoke the action to populate the combo box with cached data
  Session.DoAction("PopulateComboBox")  

End  Function




' helper method if you want to log something for debugging
Function Log(label, stuff)
  Session.Property("Log") = "" ' to force 'value changed'
  Session.Property("Log") = label + ":  " + stuff
End  Function


Function SanitisePropName(prop_name)
    prop_name = Replace(prop_name," ","")
    prop_name = Replace(prop_name,")","")
    prop_name = Replace(prop_name,"(","")

    ' Specific to this format: take only label before "/" [the english bit]'
    prop_name = Split(prop_name, "/")(0)

    SanitisePropName = prop_name
End  Function


' -----------------------------------------------------------------------------
' @info This function is executed when a new selection has been made in the 
'       UI language combo box   
' -----------------------------------------------------------------------------
Function LanguageSelected
  Const comboProp = "COMBO_LANGUAGE"  
  Const defaultFontFamily = "/Resources/Fonts/#Roboto"
  Const defaultFontStretch = "Condensed"
  Const defaultFontWeight = "Light"
  
  ' Get attached info from property
  selectedLanguage = Session.Property(comboProp)
  lang_enum = Session.Property("LANGUAGE_" + SanitisePropName(selectedLanguage))

  ' Store the  enum as a property: we'll need to use this for writing to XML
  Session.Property("KEYBOARD_LANGUAGE_SELECTED") = lang_enum

  ' Special case for chinese - all variants map to ChineseTraditionalTaiwan as the
  ' UI language
  If InStr(lang_enum, "Chinese") > 0 Then
    Session.Property("UI_LANGUAGE_SELECTED") = "ChineseTraditionalTaiwan"
  Else 
    Session.Property("UI_LANGUAGE_SELECTED") = lang_enum
  End If
  

  ' Set accompanying settings
  If StrComp(lang_enum, "PersianIran") = 0 Then
    Session.Property("SELECTED_FONTFAMILY") = "/Resources/Fonts/#Nafees Web Naskh"
    Session.Property("SELECTED_FONTSTRETCH") = "Normal"
    Session.Property("SELECTED_FONTWEIGHT") = "Regular"
  ElseIf StrComp(lang_enum, "UrduPakistan") = 0 Then 
    Session.Property("SELECTED_FONTFAMILY") = "/Resources/Fonts/#Nazli"
    Session.Property("SELECTED_FONTSTRETCH") = "Normal"
    Session.Property("SELECTED_FONTWEIGHT") = "Regular"
  Else
    Session.Property("SELECTED_FONTFAMILY") = defaultFontFamily
    Session.Property("SELECTED_FONTSTRETCH") = defaultFontStretch
    Session.Property("SELECTED_FONTWEIGHT") = defaultFontWeight
  End If    

End  Function



' -----------------------------------------------------------------------------
' @info Update UI and properties for new eye tracker
' -----------------------------------------------------------------------------
Function UpdateEyeTracker(tracker_enum, tracker_extra_info, tracker_extra_info_en)
  
  ' insert line feeds if present
  tracker_extra_info = Replace(tracker_extra_info,"\n",vbLf) 
  tracker_extra_info_en = Replace(tracker_extra_info_en,"\n",vbLf) 
  
  ' Update the text label displaying extra info
  Session.Property("EYETRACKER_TEXT") = tracker_extra_info
  Session.Property("EYETRACKER_TEXT_EN") = tracker_extra_info_en

  ' Store the eyetracker enum as a property: we'll need to use this for writing to XML
  Session.Property("EYETRACKER_SELECTED") = tracker_enum

  ' Coerce associated properties for touch input
  If StrComp(tracker_enum, "TouchScreenPosition") = 0 Then
    Session.Property("SELECTED_KEYSELECTIONTRIGGERSOURCE") = "TouchDownUps"
    Session.Property("SELECTED_POINTSELECTIONTRIGGERSOURCE") = "TouchDownUps"
    Session.Property("SELECTED_MINDWELLTIMEINMS") = "50"
    Session.Property("SELECTED_MULTIKEYSELECTIONTRIGGERSTOPSIGNAL") = "NextLow"
  Else
    Session.Property("SELECTED_KEYSELECTIONTRIGGERSOURCE") = "Fixations"
    Session.Property("SELECTED_POINTSELECTIONTRIGGERSOURCE") = "Fixations"
    Session.Property("SELECTED_MINDWELLTIMEINMS") = "250"
    Session.Property("SELECTED_MULTIKEYSELECTIONTRIGGERSTOPSIGNAL") = "NextHigh"
  End If  

End Function

' -----------------------------------------------------------------------------
' @info This function is executed when a new selection has been made in the 
'       eye tracker combo box   
' -----------------------------------------------------------------------------
Function EyeTrackerSelected
  Const comboProp = "COMBO_EYE_TRACKER"
  
  ' Get attached info from property
  selectedTracker = Session.Property(comboProp)
  tracker_enum = Session.Property("TRACKER_" + SanitisePropName(selectedTracker))
  tracker_info = Session.Property("TRACKERINFO_" + SanitisePropName(selectedTracker))  
  tracker_info_en = Session.Property("TRACKERINFO_EN_" + SanitisePropName(selectedTracker))  

  UpdateEyeTracker(tracker_enum), tracker_info, tracker_info_en

End Function

