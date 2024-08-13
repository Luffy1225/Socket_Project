# Socket_Project


# Client-Server Socket Communication

## 通訊格式

在這個系統中，客戶端（Client）和伺服器（Server）之間的通訊是基於文字訊息的。以下是通訊格式的詳細說明：

### 訊息格式

0. **格式說明**
    
    主要分成兩大類:
    1. 在Server中傳遞訊息 / 下命令
    - `來源者 : 說出的訊息(或命令)`
    2. 跟 目標者 說 / 下命令
    - `來源者 : To 目標者 : 對目標說出的訊息(或命令)`

    EX: `Default_Client9078 : Hello Server. This is sent from Default_Client9078`

1. **發送給指定客戶端**
To <Client_Name> : <Message>


- 例如：`To Default_Client8351 : Hello, how are you?`

2. **一般訊息**
<Client_Name> : <Message>


- 例如：`Default_Client8351 : Hello Server!`

3. **伺服器回應訊息**
Server: <Server_Name> : <Message>


- 例如：`Server: MainServer : Welcome to the server!`

### 指令格式

1. **`cls()`**
- 清除客戶端/伺服器的螢幕資訊。

2. **`close()`**
- 關閉客戶端連線或伺服器。

3. **`history()`**
- 請求伺服器發送歷史訊息。

4. **`list()`**
- 請求伺服器列出所有連線的客戶端。

## 客戶端功能

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
string name = "Client1";
string ip = "127.0.0.1";
int port = 8080;

Client_Socket client = new Client_Socket(name, ip, port);
client.Show_Info();
client.Connect();

if (client.Connected)
{
 client.Start();
}
else
{
 Print_Tool.WriteLine("連線失敗", ConsoleColorType.Error);
}
伺服器示例
csharp
複製程式碼
Server_Socket server = new Server_Socket("MainServer");
server.Start();
