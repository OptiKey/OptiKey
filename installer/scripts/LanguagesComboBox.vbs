
' helper method if you want to log something for debugging
Function Log(label, stuff)
  Session.Property("Log") = "" ' to force 'value changed'
  Session.Property("Log") = label + ":  " + stuff
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
  Session.Property("LANGUAGE_SELECTED") = lang_enum

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
