# Client-Server Socket

 - 遊戲用Socket 前往 Socket_Project : Branch : [ReadKey](https://github.com/Luffy1225/Socket_Project/tree/ReadKey)

## 使用教學 

### Client端

系統 會要求輸入 這台Client名稱 目標Server IP 目標Server Port

* 名稱預設為 `Default_ClientXXXX` (XXXX 為 隨機四碼)
* IP預設為 `127.0.0.1` 
* Port預設為 `8080`

### Server端

系統 會要求輸入 這台Server名稱 Server IP Server Port

* 名稱預設為 `Server` 
* IP預設為 `127.0.0.1` 
* Port預設為 `8080`

### !!注意連線時 Server 與 Client 的 IP Port 應為一致!!


## 通訊格式

在這個系統中，客戶端（Client）和伺服器（Server）之間的通訊是基於文字訊息的。以下是通訊格式的詳細說明：

### 訊息格式

0. **格式說明**
    
    主要分成兩大類:
    1. 在Server中傳遞訊息 / 下命令
    - `來源者 : 說出的訊息(或命令)`
        
         - 例如： `Default_Client9078 : 你好`

    2. 發送 訊息(或命令) 給 指定客戶端 To <Client_Name> :<Message>
    - `來源者 : To 目標者 : 對目標說出的訊息(或命令)`

         - 例如： `Server : To Jack : cls()`


### 功能介紹

#### 客戶端功能

1. **`cls()`**
   - 清除客戶端螢幕資訊。

1. **`close()`**
   - 關閉客戶端連線。

1. **`history()`**
   - 請求伺服器發送歷史訊息。
     
1. **`clearhis()`**
   - 清除歷史訊息。

1. **`list()`**
   - 請求伺服器列出所有連線的客戶端數量。
     
1. **`greet()`**
   - 向 Server 打招呼：Server 將會告訴 Client 他的 Server 名。
     - 正常情況下，當建立連線後，Server 會立即發送 `greet()`。

1. **`showinfo()`**
   - 傳送自己的 Name, IP, Port 到 Server。

1. **`help()`**
   - 前往文件 https://github.com/Luffy1225/Socket_Project


#### 伺服器端功能

1. **`cls()`**
   - 清除伺服器螢幕資訊。

1. **`close()`**
   - 關閉伺服器連線。

1. **`history()`**
   - 顯示歷史訊息。

1. **`list()`**
   - 列出所有連線的客戶端數量。

1. **`greet()`**
   - 要求所有客戶端執行 `greet()`。

1. **`showinfo()`**
   - 顯示伺服器的名稱、IP 地址、端口。

1. **`help()`**
   - 前往文件 [https://github.com/Luffy1225/Socket_Project](https://github.com/Luffy1225/Socket_Project)。

    #### 對客戶端下達命令

    1. **`To "客戶端名稱" : cls()`**
       - 清除 "客戶端名稱" 螢幕資訊。
    
    1. **`To "客戶端名稱" : close()`**
       - 請求 "客戶端名稱" 關閉連線（正常關閉）。
    
    1. **`To "客戶端名稱" : ban()`** // 待完成
       - 剔除 "客戶端名稱" 連線（強制中斷）。

## 範例

### 客戶端範例

```csharp
Client_Socket Client = new Client_Socket(name, ip, port);
Client.Start();
```

### 伺服器範例
```csharp
Server_Socket Server = new Server_Socket(name, ip, port);
Server.Start();
```

## 腳本使用

前往Run資料夾 點擊 `AutoRun.bat` 執行自動化執行

關於自動化執行相關參數 可以到 `RunParameter.ini` 進行修改
   - `ClientCount` : 一次開啟的Client數量
