Set objShell = CreateObject("WScript.Shell")
Set objFSO = CreateObject("Scripting.FileSystemObject")
strCurDir = objFSO.GetParentFolderName(WScript.ScriptFullName)
objShell.Run chr(34) & strCurDir & "\gitauto.bat" & chr(34), 0
Set objShell = Nothing
Set objFSO = Nothing
