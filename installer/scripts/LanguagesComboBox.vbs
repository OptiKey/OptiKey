' -----------------------------------------------------------------------------
' @info Function that will populate the eye tracker ComboBox from a static text
'       file
' -----------------------------------------------------------------------------
Function PopulateEyeTrackerCombo()
  Const comboProp = "COMBO_EYE_TRACKER"
  Const orderStart = 10
  Const orderDelta = 5

  ' This file contains the list of eye trackers, enums and descriptions
  strFile = Session.Property("TempFolder") & "eyetrackers.txt"
  PopulateEyeTrackerComboFromFile(strFile), comboProp, orderStart, orderDelta
End  Function

' -----------------------------------------------------------------------------
' @info Helper for PopulateEyeTrackerCombo. Reads the specified file line by line
'       and constructs the value for the AI_COMBOBOX_DATA Property.
'       After this it calls the predefined AI Custom Action "PopulateComboBox"
'       which will retrieve and parse the AI_COMBOBOX_DATA Property and do the
'       actual insertion.
' -----------------------------------------------------------------------------
Function PopulateEyeTrackerComboFromFile(filespec, comboProp, orderStart, orderDelta)
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

    if ub >= 0 Then '(not empty)
      tracker_label = parts(0)
      tracker_enum = parts(1)      
      tracker_extra_info = parts(2)

      ' add this as entry to the combobox
      AIComboData = AIComboData & SEP_2 & tracker_label & SEP_1 & tracker_label
      
      If idx = 0 Then
        ' select the first item (index 0) in the ComboBox
        Session.Property(comboProp) = tracker_label        
        UpdateEyeTracker(tracker_enum), tracker_extra_info
      End If    

      ' store attached data in an installer property
      Session.Property("EYETRACKER_" + tracker_label) = strLine

    End if
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


' -----------------------------------------------------------------------------
' @info Update UI and properties for new eye tracker
' -----------------------------------------------------------------------------
Function UpdateEyeTracker(tracker_enum, tracker_extra_info)
  
  ' insert line feeds if present
  tracker_extra_info = Replace(tracker_extra_info,"\n",vbLf) 
  
  ' Update the text label displaying extra info
  Session.Property("EYETRACKER_TEXT") = tracker_extra_info

  ' Store the eyetracker enum as a property: we'll need to use this for writing to XML
  Session.Property("EYETRACKER_SELECTED") = tracker_enum

End Function

' -----------------------------------------------------------------------------
' @info This function is executed when a new selection has been made in the 
'       eye tracker combo box   
' -----------------------------------------------------------------------------
Function EyeTrackerSelected
  Const comboProp = "COMBO_EYE_TRACKER"
  Const SEP_1 = "#"
  Const SEP_2 = "|"
  Dim strValue, order, orderDelta, AIListBoxData
  
  ' Get attached info from property
  selectedTracker = Session.Property(comboProp)
  trackerInfo = Session.Property("EYETRACKER_" + selectedTracker)

  Dim parts
  parts = Split(trackerInfo, "|")
  tracker_label = parts(0)
  tracker_enum = parts(1)      
  tracker_extra_info = parts(2)
  
  UpdateEyeTracker(tracker_enum), tracker_extra_info

End Function

