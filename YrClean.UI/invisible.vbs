Dim command
command = ""

For Each argument In WScript.Arguments
	If command <> "" Then command = command & " "
	command = command & """" & argument & """"
Next

CreateObject("Wscript.Shell").Run command, 0, False