@echo off
setlocal enabledelayedexpansion

rem 讀取配置檔案
for /f "tokens=1,* delims==" %%a in ('findstr /r /c:"^[^;]" RunParamater.ini') do (
    set "key=%%a"
    set "value=%%b"
    
    rem 去除鍵和值中的空白
    set "key=!key: =!"
    set "value=!value: =!"
    
    rem 去除值中的引號
    set "value=!value:"=!"

    rem 根據鍵名設置參數
    if "!key!"=="ClientCount" set "iterations=!value!"
    if "!key!"=="ServerName" set "ServerName=!value!"
)

rem 顯示結果
echo ClientCount=%iterations%
echo ServerName=%ServerName%

rem 設定 .exe 文件的路徑
set "serverExePath=Server_Socket.exe"
set "clientExePath=Client_Socket.exe"

rem 執行 Server_Socket.exe 不等待
echo Running Server_Socket.exe...
start "" "%serverExePath%"

rem 根據 iterations 執行 Client_Socket.exe
echo Running Client_Socket.exe %iterations% times...
for /l %%i in (1,1,%iterations%) do (
    echo Running Client_Socket.exe iteration %%i...
    start "" "%clientExePath%"
)

echo All Client_Socket.exe executions initiated.
endlocal
pause
