# Client-Server Socket

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

2. **`close()`**
   - 關閉客戶端連線。

3. **`history()`**
   - 請求伺服器發送歷史訊息。

4. **`list()`**
   - 請求伺服器列出所有連線的客戶端數量。
     
5. **`greet()`**
   - 向Server 打招呼 Server將會告訴Client 他的Server名， (正常情況下 當建立連線後 會立即發送greet() )。

6. **`showinfo()`**
   - 傳送自己的Name, Ip, Port 到Server。

7. **`help()`**
   - 前往文件 https://github.com/Luffy1225/Socket_Project。

#### 伺服器端功能

1. **`cls()`**
   - 清除伺服器螢幕資訊。

2. **`close()`**
   - 關閉伺服器連線。

3. **`history()`**
   - 顯示歷史訊息。

4. **`list()`**
   - 列出所有連線的客戶端數量。
  
5. **`greet()`**
   - 要求所有Client greet()。

6. **`showinfo()`**
   - 顯示Server的Name, Ip, Port。

7. **`help()`**
   - 前往文件 https://github.com/Luffy1225/Socket_Project。

    ##### 對客戶端下達命令

1. **`To "客戶端名稱" : cls()`**
   - 清除 "客戶端名稱" 螢幕資訊。

2. **`To "客戶端名稱" : close()`**
   - 請求 "客戶端名稱" 關閉連線 (正常關閉)。

3. **`To "客戶端名稱" : ban()`** // 待完成
   - 剔除 "客戶端名稱" 連線 (強制中斷)。



### 初始化

- `Client_Socket(string myname)`
- 使用預設 IP（127.0.0.1）和埠（8080）初始化客戶端。
- `Client_Socket(string myname, string ipstr, int port)`
- 使用指定的 IP 和埠初始化客戶端。

### 主要方法

- `Connect()`
- 連線到伺服器。如果連線失敗，將重試最多 5 次。

- `Start()`
- 開始處理接收訊息並處理用戶輸入。

- `Send_Message(string message)`
- 向伺服器發送訊息。

- `Read_Message()`
- 讀取來自伺服器的訊息並處理。

- `Greeting()`
- 向伺服器發送問候訊息。

- `Close()`
- 關閉客戶端連線。

- `Show_Info()`
- 顯示客戶端資訊（名稱、IP 和埠）。

- `Refresh()`
- 清除並顯示客戶端資訊。

## 伺服器功能

### 初始化

- `Server_Socket(string name)`
- 使用預設 IP（127.0.0.1）和埠（8080）初始化伺服器。
- `Server_Socket(string name, string hosting_ip, int port)`
- 使用指定的 IP 和埠初始化伺服器。

### 主要方法

- `Start()`
- 開始伺服器，接受客戶端連線並處理輸入。

- `AcceptClients()`
- 接受並處理新的客戶端連線。

- `HandleClient(TcpClient client)`
- 處理客戶端的輸入和訊息。

- `Send_Message(string message)`
- 向所有連線的客戶端發送訊息。

- `Send_Message(string towho, string message)`
- 向指定的客戶端發送訊息。

- `List_All_Clients()`
- 列出所有連線的客戶端。

- `Show_History(string towho = "")`
- 顯示或發送訊息歷史。

- `Show_Info()`
- 顯示伺服器資訊（名稱、IP 和埠）。

- `Refresh()`
- 清除並顯示伺服器資訊。

## 範例

### 客戶端示例

```csharp
Client_Socket Client = new Client_Socket(name, ip, port);
Client.Start();
```

伺服器示例
```csharp
Server_Socket Server = new Server_Socket(name, ip, port);
Server.Start();
```
