' -----------------------------------------------------------------------------
' @info Attached to UI action, fills combo box
' -----------------------------------------------------------------------------
Function PopulateUiLanguagesCombo()
  Const comboProp = "COMBO_UI_LANGUAGE"
  Const selectedProp = "UI_LANGUAGE_SELECTED"
  Const orderStart = 10
  Const orderDelta = 5

  ' This file contains the list of languages + enums 
  strFile = Session.Property("TempFolder") & "languages.txt"
  PopulateLanguageComboFromFile(strFile), comboProp, orderStart, orderDelta, selectedProp
End  Function

' -----------------------------------------------------------------------------
' @info Attached to UI action, fills combo box
' -----------------------------------------------------------------------------
Function PopulateTypingLanguagesCombo()
  Const comboProp = "COMBO_TYPING_LANGUAGE"
  Const selectedProp = "TYPING_LANGUAGE_SELECTED"
  Const orderStart = 10
  Const orderDelta = 5

  ' This file contains the list of languages + enums 
  strFile = Session.Property("TempFolder") & "languages.txt"
  PopulateLanguageComboFromFile(strFile), comboProp, orderStart, orderDelta, selectedProp
End  Function

' -----------------------------------------------------------------------------
' @info Reads the specified file line by line
'       and constructs the value for the AI_COMBOBOX_DATA Property.
'       After this it calls the predefined AI Custom Action "PopulateComboBox"
'       which will retrieve and parse the AI_COMBOBOX_DATA Property and do the
'       actual insertion.
' -----------------------------------------------------------------------------
Function PopulateLanguageComboFromFile(filespec, comboProp, orderStart, orderDelta, selectedProp)

  ' TODO: extract method 90%-duped with tracker combo
  Dim fso, strLine, idx
  Const ForReading = 1
  Const SEP_1 = "#"
  Const SEP_2 = "|"

  Set fso = CreateObject("Scripting.FileSystemObject")  
  If Not fso.FileExists(filespec) Then
    Exit Function
  End If

  Set srcFile  = fso.OpenTextFile(filespec, ForReading, False)
  
  ' This will be used to specify the combobox control and data that will 
  ' be inserted in the control.
  ' The format is:
  ' ComboProp#OrderStart#OrderDelta|Value1#Text1|Value2#Text2|...
  ' OrderStart and OrderDelta are optional, as well as the Text for each 
  ' item. If the text is not specified, WI will show the value in the combo. 
  ' Values must be unique.
  AIComboData = comboProp & SEP_1 & CStr(orderStart) & SEP_1 & CStr(orderDelta)
  
  ' used as index; first env. var. will be selected in combo
  idx = -1
  
  ' Read the source file, line by line
  strLine = srcFile.ReadLine ' skip first line
  Do While srcFile.AtEndOfStream <> True
    idx = idx + 1
    strLine = srcFile.ReadLine

    ' Split by separator |
    Dim parts
    parts = Split(strLine, "|")
    ub = UBound(parts) ' largest idx, i.e. (size-1)

    If ub >= 0 Then '(not empty)
      lang_label = parts(0)
      lang_enum = parts(1)   

      ' add this as entry to the combobox
      AIComboData = AIComboData & SEP_2 & lang_label & SEP_1 & lang_label
    
      ' English UK is default'
      if InStr(lang_label, "English") > 0 and InStr(lang_label, "UK") > 0 Then       
        ' select the first item in the ComboBox
        Session.Property(comboProp) = lang_label             
        Session.Property(selectedProp) = lang_enum
      End If

      ' store attached data in an installer property
      Session.Property("LANGUAGE_"+SanitisePropName(lang_label)) = lang_enum

    End If    
  Loop
  
  ' Close the file
  srcFile.Close  
  
  ' Set the Property that will be used as input by the AI PopulateComboBox
  ' Custom Action
  Session.Property("AI_COMBOBOX_DATA") = AIComboData
  
  ' Invoke the Custom Action -> this could also have been done from a 
  ' DoAction Control Event
  Session.DoAction("PopulateComboBox")  

End  Function

Function SanitisePropName(prop_name)
    prop_name = Replace(prop_name," ","") 
    prop_name = Replace(prop_name,")","") 
    prop_name = Replace(prop_name,"(","") 
    ' TODO: remove nonascii too!
    SanitisePropName = prop_name
End  Function

' helper method if you want to log something for debugging
Function Log(label, stuff)
  Session.Property("Log") = "" ' to force 'value changed'
  Session.Property("Log") = label + ":  " + stuff
End  Function

' -----------------------------------------------------------------------------
' @info This function is executed when a new selection has been made in the 
'       UI language combo box   
' -----------------------------------------------------------------------------
Function UiLanguageSelected
  Const comboProp = "COMBO_UI_LANGUAGE"
  Const SEP_1 = "#"
  Const SEP_2 = "|"
  Dim strValue, order, orderDelta, AIListBoxData
  
  ' Get attached info from property
  selectedLanguage = Session.Property(comboProp)
  lang_enum = Session.Property("LANGUAGE_" + SanitisePropName(selectedLanguage))

  ' Store the  enum as a property: we'll need to use this for writing to XML
  Session.Property("UI_LANGUAGE_SELECTED") = lang_enum

End Function

' -----------------------------------------------------------------------------
' @info This function is executed when a new selection has been made in the 
'       typing language combo box   
' -----------------------------------------------------------------------------
Function TypingLanguageSelected
  Const comboProp = "COMBO_TYPING_LANGUAGE"
  Const SEP_1 = "#"
  Const SEP_2 = "|"
  Dim strValue, order, orderDelta, AIListBoxData
  
  ' Get attached info from property
  selectedLanguage = Session.Property(comboProp)
  lang_enum = Session.Property("LANGUAGE_" + SanitisePropName(selectedLanguage))

  ' Store the enum as a property: we'll need to use this for writing to XML
  Session.Property("TYPING_LANGUAGE_SELECTED") = lang_enum

End Function

