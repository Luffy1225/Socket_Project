@echo off
setlocal

REM 讀取RunParameter.ini中的參數，並儲存在局部變數中
for /f "tokens=2 delims==" %%a in ('findstr "ClientCount" RunParameter.ini') do (
    set "ClientCount=%%a"
)

for /f "tokens=2 delims==" %%a in ('findstr "ServerName" RunParameter.ini') do (
    set "ServerName=%%a"
)

for /f "tokens=2 delims==" %%a in ('findstr "ConnectIP" RunParameter.ini') do (
    set "ConnectIP=%%a"
)

for /f "tokens=2 delims==" %%a in ('findstr "ConnectPort" RunParameter.ini') do (
    set "ConnectPort=%%a"
)

REM 去除多餘的引號
set "ServerName=%ServerName:"=%"
set "ConnectIP=%ConnectIP:"=%"
set "ConnectPort=%ConnectPort:"=%"

REM 執行Server_Socket.exe，不等待結束
 start "" SocketServer.exe %ServerName%


REM 根據ClientCount的數量執行Client_Socket.exe，不等待結束
for /l %%i in (1,1,%ClientCount%) do (
    start "" SocketClient.exe %ConnectIP% %ConnectPort%
)

endlocal
