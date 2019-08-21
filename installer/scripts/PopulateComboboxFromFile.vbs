' -----------------------------------------------------------------------------
' @info Function that will populate the ComboBox with the attached Property
'       MY_COMBO with the environment variables obtained by redirecting the
'       output of the "set" command
' -----------------------------------------------------------------------------
Function PopulateComboWithEnvVars()
  Const comboProp = "MY_COMBO"
  Const orderStart = 10
  Const orderDelta = 5

  ' This file will contain the output of the "set" command:
  ' all the environment variables and their values (one per line)
  ' with the format:
  ' var1=value_var1
  ' var2=value_var2
  ' ...  
  strFile = Session.Property("TempFolder") & "eyetrackers.txt"
  PopulateComboFromFile(strFile), comboProp, orderStart, orderDelta
End  Function


Class Eyetracker

    Public enumName
    Public friendlyName
    Public info
    Public extra

End Class  

' -----------------------------------------------------------------------------
' @info Helper for the above function. Reads the specified file line by line
'       and constructs the value for the AI_COMBOBOX_DATA Property.
'       After this it calls the predefined AI Custom Action "PopulateComboBox"
'       which will retrieve and parse the AI_COMBOBOX_DATA Property and do the
'       actual insertion.
' -----------------------------------------------------------------------------
Function PopulateComboFromFile(filespec, comboProp, orderStart, orderDelta)
  Dim fso, strLine, pos, strEnvVarName, idx
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
  
  Dim eyetrackerDict
  Set eyetrackerDict = CreateObject("Scripting.Dictionary")

  ' Read the source file, line by line
  strLine = srcFile.ReadLine ' skip first line
  Do While srcFile.AtEndOfStream <> True
    idx = idx + 1
    strLine = srcFile.ReadLine

    ' Split by separator |
    Dim parts
    parts = Split(strLine, "|")
    ub = UBound(parts) ' largest idx, i.e. (size-1)

    ' TODO: store info, enums etc in lists, and recover using the index of the combobox. 
    ' though I suspect we need to figure out the index ourselves by searching for current value
    ' in list of values
    '
    ' OR: for every eyetracker create an installer property with the other info in it.
    ' this is perhaps more tractable despite being somewhat ugly

    if ub >= 0 Then '(not empty)
      tracker_label = parts(0)
      tracker_enum = parts(1)      
      tracker_extra_info = parts(2)

      ' the whole line will be inserted as value and the env. var. name as text
      AIComboData = AIComboData & SEP_2 & tracker_label & SEP_1 & tracker_label
      
      If idx = 0 Then
        ' select the first item (index 0) in the ComboBox
        Session.Property(comboProp) = tracker_label
        Session.Property("EYETRACKER_TEXT") = tracker_extra_info
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
  
  ' Store tracker data dict as property
  ' Session.Property("EYETRACKER_DICT") = CStr(eyetrackerDict)
  
  ' Invoke the Custom Action -> this could also have been done from a 
  ' DoAction Control Event
  Session.DoAction("PopulateComboBox")  
End  Function


' -----------------------------------------------------------------------------
' @info This function is executed when the Add button from ListBoxDemoDlgA and
'       ListBoxDemoDlgB is clicked.     
' -----------------------------------------------------------------------------
Function EyeTrackerNext
  Const comboProp = "MY_COMBO"
  Const listBoxProp = "MY_LISTBOX"
  Const SEP_1 = "#"
  Const SEP_2 = "|"
  Dim strValue, order, orderDelta, AIListBoxData
  
  selectedTracker = Session.Property(comboProp)
  trackerInfo = Session.Property("EYETRACKER_" + selectedTracker)

  Session.Property("EYETRACKER_TEXT") = trackerInfo

  ' Set tracker = eyetrackerDict(selectedTracker)  
  ' Session.Property("EYETRACKER_TEXT") = Session.Property("EYETRACKER_DICT")

End Function


' -----------------------------------------------------------------------------
' @info This function is executed when the Add button from ListBoxDemoDlgA and
'       ListBoxDemoDlgB is clicked.     
' -----------------------------------------------------------------------------
Function ListAddButton
  Const listBoxProp = "MY_LISTBOX"
  Const SEP_1 = "#"
  Const SEP_2 = "|"
  Dim strValue, order, orderDelta, AIListBoxData
  
  strValue = Session.Property("VALUE_PROP")
  strValue = Trim(strValue)
  If strValue = "" Then
    MsiMsgBox("Please enter an IP address or address range.")
    Exit Function
  End If
  
  ' retrieve the order for the current item
  order = Session.Property("LIST_ORDER")
  orderDelta = Session.Property("LIST_ORDER_DELTA")
  
  ' ListBoxProp#Order|value
  AIListBoxData = listBoxProp & SEP_1 & order & SEP_2 & strValue
  
  ' Set the Property that will be used as input by the AI PopulateListBox
  ' Custom Action
  Session.Property("AI_LISTBOX_DATA") = AIListBoxData
  
  ' Invoke the Custom Action that will populate the ListBox
  Session.DoAction("PopulateListBox")
  
  ' Test if an attempt was made to insert an already existent value
  ' In this case, the Property AI_LISTBOX_DATA will be set to:
  ' ERROR_DUPLICATE_ITEM: value
  AIListBoxData = Session.Property("AI_LISTBOX_DATA")
  If InStr(AIListBoxData, "ERROR_DUPLICATE_ITEM") > 0 Then
    MsiMsgBox("IP address/range " & Chr(34) & strValue & Chr(34) &_ 
      " already exists.")
  Else
    ' update the LIST_ORDER Property by adding the value of LIST_ORDER_DELTA
    Session.Property("LIST_ORDER") = CStr(CInt(order) + CInt(orderDelta))
  End If
  
  ' Clear the already inserted VALUE_PROP Property and select the newly
  ' inserted item in the listbox control; 
  ' The 2 SetProperty Control Events are not required in this case because
  ' we are using the "twin dialog method"
  Session.Property("VALUE_PROP") = ""
  Session.Property(listBoxProp) = strValue  
End Function


' -----------------------------------------------------------------------------
' @info This function is executed when the Remove button from ListBoxDemoDlgA 
'       and ListBoxDemoDlgB is clicked.
' -----------------------------------------------------------------------------
Function ListRemoveButton
  Const listBoxProp = "MY_LISTBOX"
  Const SEP_2 = "|"

  ' ListBoxProp|value
  AIListBoxData = listBoxProp & SEP_2 & Session.Property(listBoxProp)

  ' Set the Property that will be used as input by the AI DeleteFromListBox
  ' Custom Action
  Session.Property("AI_LISTBOX_DATA") = AIListBoxData

  ' Invoke the Custom Action that will delete the item from the ListBox
  Session.DoAction("DeleteFromListBox")
  
  ' Reset the Property of the ListBox
  Session.Property(listBoxProp) = ""  
End Function


' -----------------------------------------------------------------------------
' @info This function is executed when the "Remove all" button from 
'       ListBoxDemoDlgA and ListBoxDemoDlgB is clicked.
' -----------------------------------------------------------------------------
Function ListRemoveAllButton
  Const listBoxProp = "MY_LISTBOX"
  
  ' Set the Property that will be used as input by the AI DeleteFromListBox
  ' Custom Action (with the Property attached to the ListBox)
  Session.Property("AI_LISTBOX_DATA") = listBoxProp

  ' Invoke the Custom Action that will delete all the items from the ListBox
  Session.DoAction("DeleteFromListBox")
  
  ' Reset the Property of the ListBox
  Session.Property(listBoxProp) = ""
End Function


' -----------------------------------------------------------------------------
' @info This function is executed when the "Extract" button from 
'       ListBoxDemoDlgA and ListBoxDemoDlgB is clicked.
' -----------------------------------------------------------------------------
Function ListExtractButton
  Const listBoxProp = "MY_LISTBOX"

  ' Set the Property that will be used as input by the AI ExtractListBoxData
  ' Custom Action (with the Property attached to the ListBox)
  Session.Property("AI_LISTBOX_DATA") = listBoxProp
  
  ' Invoke the Custom Action that will extract the ListBox data
  Session.DoAction("ExtractListBoxData")
End Function


' -----------------------------------------------------------------------------
' @info Displays a Windows Installer message box
' -----------------------------------------------------------------------------
Function MsiMsgBox(msg)
  Const msiMessageTypeUser = &H03000000
  Set record = Session.Installer.CreateRecord(1)
  record.StringData(0) = "[1]"
  record.StringData(1) = CStr(msg)
  Session.Message msiMessageTypeUser, record
End Function