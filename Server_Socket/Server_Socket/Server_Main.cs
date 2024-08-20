using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Luffy_Tool;


namespace Server_Socket
{

    class InputHandler
    {
        private Server_Socket _server;

        public InputHandler(Server_Socket server)
        {
            _server = server;
        }

        public void StartHandling()  // Server 輸出
        {
            while (true)
            {
                Print_Tool.WriteLine("輸入文字: ", ConsoleColorType.Default);

                string input = Console.ReadLine();
                string command = input.ToLower();

                if (command == "close()")
                {
                    _server.Close();
                    break;
                }
                else if (command == "cls()")
                {
                    _server.cls();
                }
                else if (command == "list()")
                {
                    _server.List_All_Clients();
                }
                else if (command == "history()")
                {
                    _server.Show_History();
                }
                else if (command == "clearhis()")
                {
                    _server.Clear_History();
                }
                else if (command == "showinfo()")
                {
                    _server.Show_Param();
                }
                else if (command == "greet()")
                {
                    _server.Every_Greet();
                }
                else if (command == "help()")
                {
                    _server.Help();
                }
                else
                {
                    _server.Send_Message(input);
                }
            }
        }


    }

    class Server_Socket
    {
        private string _Server_Name;
        private string _Host_IP;
        private int _Port;
        public string Server_Name
        {
            get
            {
                return _Server_Name;
            }
            set
            {
                if (value == "")
                {
                    _Server_Name = Default_Server_Name;
                }
                else
                {
                    _Server_Name = value;
                }
            }
        } // 空 = 預設
        public string Host_IP
        {
            get
            {
                return _Host_IP;
            }
            set
            {
                _Host_IP = Socket_Tool.Is_IPv4(value) ? value : Default_Host_IP;
            }
        }// 空 = 預設
        public int Port
        {
            get
            {
                return _Port;
            }
            set
            {
                _Port = Socket_Tool.Is_Port(value) ? value : Default_Port;

            }
        }// 空 = 預設



        private TcpListener Server;
        private List<TcpClient> Clients = new List<TcpClient>();


        private List<string> history = new List<string>();


        #region Default Parameter
        static private string Default_Server_Name = "Server";
        static private string Default_Host_IP = "127.0.0.1";
        static private int Default_Port = 8080;
        #endregion

        public Server_Socket()
        {
            Server_Name = Default_Server_Name;
            Host_IP = Default_Host_IP;
            Port = Default_Port;
        }

        public Server_Socket(string name)
        {
            Server_Name = name;

            Host_IP = Default_Host_IP;
            Port = Default_Port;
        }

        public Server_Socket(string name, string hosting_ip, int port)
        {
            Server_Name = name;
            Host_IP = hosting_ip;
            Port = port;
        }






        public void Start()
        {
            Show_Info();

            IPAddress ipAddress = IPAddress.Parse(Host_IP);
            Server = new TcpListener(ipAddress, Port);
            Print_Tool.WriteLine(Server_Name + " is Hosting at " + Host_IP + ":" + Port, ConsoleColorType.Notice);
            Server.Start();
            Print_Tool.WriteLine("伺服器正在運行...", ConsoleColorType.Announce);

            Task.Run(() => AcceptClients());


            InputHandler inputHandler = new InputHandler(this);
            inputHandler.StartHandling();
        }

        public void Close()
        {
            Print_Tool.WriteLine("Server 關閉中...", ConsoleColorType.Warning);

            List<TcpClient> clientsCopy = Clients.ToList(); // List 複製   以防止for迴圈中 有人修改 list內容導致 程式崩潰

            foreach (var client in clientsCopy)
            {
                client.GetStream().Close();
                client.Close();
            }
            Server.Stop();
        }




        public void Send_Message(string message)
        {
            message = Server_Name + " : " + message;
            history.Add(message);

            byte[] data = Encoding.Unicode.GetBytes(message);

            List<TcpClient> clientsCopy = Clients.ToList(); // List 複製   以防止for迴圈中 有人修改 list內容導致 程式崩潰

            foreach (var client in clientsCopy)
            {
                NetworkStream stream = client.GetStream();
                stream.Write(data, 0, data.Length);
            }
            Print_Tool.WriteLine(message, ConsoleColorType.Send);
        }

        public void Send_Message(string towho, string message)
        {
            message = "To " + towho + " : " + message;
            Send_Message(message);
        }

        private async Task AcceptClients()
        {
            while (true)
            {
                TcpClient client = await Server.AcceptTcpClientAsync();
                Clients.Add(client);
                Print_Tool.WriteLine("客戶端已連線", ConsoleColorType.Announce);
                Task.Run(() => HandleClient(client));
            }
        }

        private async Task HandleClient(TcpClient client)  // Server 讀取 Client 輸入
        {
            NetworkStream stream = client.GetStream();
            try
            {
                while (true)
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                        break;

                    string response = Encoding.Unicode.GetString(buffer, 0, bytesRead);
                    history.Add(response);


                    Print_Tool.WriteLine(response, ConsoleColorType.Reply);

                    string fromwho = Get_Target(response);
                    string message = response.Substring(fromwho.Length + 2).Trim();
                    string command = message.ToLower(); // Command 全部為小寫
                    command = Get_Command(command);


                    if (message.StartsWith("Hello Server. This is sent from "))
                    {
                        Greeting(fromwho);
                    }
                    else if (command == "list()")
                    {
                        Send_Message(fromwho, Clients.Count + " In the Server");
                    }
                    else if (command == "close()")
                    {
                        Send_Message("好的 我將關閉連線 " + response);
                        client.Close();
                    }
                    else if (command == "history()")
                    {
                        Show_History();
                    }
                    else
                    {
                        //Send_Message("Received you Message : " + response);
                    }
                }
            }
            catch (Exception ex)
            {
                Print_Tool.WriteLine("發生錯誤" + ex.Message, ConsoleColorType.Error);
            }
            finally
            {
                client.Close();
                Clients.Remove(client);
            }


        }







        #region Private Function

        private void Set_Default()
        {
            Server_Name = "Server";
            Host_IP = "127.0.0.1";
            Port = 8080;
        } //預留


        private string Get_Target(string msg)
        {
            string[] chunks = msg.Split(new[] { " :" }, StringSplitOptions.None);
            return chunks[0].Trim();
        }

        private string Get_Command(string msg)
        {
            // 找到冒號後的部分
            if (msg.Contains("to"))
            {
                int colonIndex = msg.IndexOf(':');
                if (colonIndex >= 0)
                {
                    // 移除"To xxxxxxxxxxx : " 只保留 cmd()
                    return msg.Substring(colonIndex + 1).Trim();
                }
                return msg; // 如果沒有找到冒號，直接返回原始消息
            }
            return msg;
        }







        #endregion




        #region 功能區

        internal void cls()
        {
            Send_Message("cls");
            Refresh();
        }
        public void Refresh()
        {
            Show_Info();
        }

        internal void Greeting(string who = "")
        {
            //Send_Message("Hello " + who + ". This is " + Server_Name);
            Send_Message(who, "Hello . This is Server: " + Server_Name);
        }
        public void Show_Info()
        {
            Console.Clear();
            Print_Tool.WriteLine("Server端:", ConsoleColorType.Warning);
            Print_Tool.WriteLine("Name: " + Server_Name, ConsoleColorType.Default);
            Print_Tool.WriteLine("IP  : " + Host_IP, ConsoleColorType.Default);
            Print_Tool.WriteLine("Port: " + Port, ConsoleColorType.Default);
            Print_Tool.WriteLine("\n\n", ConsoleColorType.Default);

        }
        internal void List_All_Clients()
        {
            if (Clients.Count == 0)
                Print_Tool.WriteLine("0 Clients in the Server", ConsoleColorType.Default);

            for (int i = 0; i < Clients.Count; i++)
            {
                Print_Tool.WriteLine("No. " + i + " " + Clients[i].Client.ToString(), ConsoleColorType.Default);
            }
            Print_Tool.WriteLine(Clients.Count + " In the Server", ConsoleColorType.Notice);


        }
        internal void Show_History(string towho = "")
        {
            if (history.Count == 0)
            {
                Print_Tool.WriteLine("No History..", ConsoleColorType.Warning);

            }


            for (int i = 0; i < history.Count; i++)
            {
                if (towho == "") // 如果沒有要傳給別人 就server自己顯示
                    Print_Tool.WriteLine(history[i], ConsoleColorType.Announce);
                else
                    Send_Message(towho, history[i]);
            }
        }
        internal void Clear_History()
        {
            Print_Tool.WriteLine("Clear History", ConsoleColorType.Warning);
            history.Clear();
        }
        internal void Show_Param()
        {
            Send_Message("Server = " + Server_Name);
            Send_Message("IP = " + Host_IP);
            Send_Message("Port = " + Port);
        }
        internal void Every_Greet()
        {
            Send_Message("all", "greet()");
        }
        internal void Help()
        {
            Print_Tool.WriteLine("https://github.com/Luffy1225/Socket_Project", ConsoleColorType.Announce);
        }

        #endregion

    }

    class Program
    {
        public static void Keyin_Param(out string name, out string ip, out int port) //可能為空
        {
            Print_Tool.WriteLine("輸入Server名字:", ConsoleColorType.Notice);
            name = Console.ReadLine();

            Print_Tool.WriteLine("輸入Server IP:", ConsoleColorType.Default);
            ip = Console.ReadLine();

            Print_Tool.WriteLine("輸入Server Port:", ConsoleColorType.Default);
            string portstr = Console.ReadLine();

            if (!int.TryParse(portstr, out port))
            {
                port = -1; // 轉換失敗
            }
        }

        static void Main(string[] args)
        {
            string name = "";
            string ip = "";
            int port = -1;
            string portstr = "";

            if (args.Length > 0) //腳本執行 帶有參數
            {
                switch (args.Length)
                {
                    case 1:
                        name = args[0];
                        break;
                    case 2:
                        name = args[0];
                        ip = args[1];
                        break;
                    case 3:
                        name = args[0];
                        ip = args[1];
                        portstr = args[2];
                        break;
                    default:
                        Print_Tool.WriteLine("參數輸入: " + args.Length + " 個 不合規定!", ConsoleColorType.Error);
                        break;
                }
            }
            else // 使用者 點擊 輸入參數
                Keyin_Param(out name, out ip, out port);


            Server_Socket Server = new Server_Socket(name, ip, port);
            Server.Start();


            Console.ReadKey();
        }



    }
}
