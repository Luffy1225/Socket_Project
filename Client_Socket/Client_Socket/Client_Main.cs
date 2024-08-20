using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using Luffy_Tool;



namespace Client_Socket
{

    class InputHandler
    {
        private Client_Socket _client;

        public InputHandler(Client_Socket client)
        {
            _client = client;
        }

        public void StartHandling()
        {
            while (true)
            {
                Print_Tool.WriteLine("輸入文字: ", ConsoleColorType.Default);

                string input = Console.ReadLine();
                string command = input.ToLower();

                if (command == "close()")
                {
                    _client.Close();
                    break;
                }
                else if (command == "greet()")
                {
                    _client.Greeting();
                }
                else if (command == "cls()")
                {
                    _client.Refresh();
                }
                else if (command == "history()")
                {
                    _client.Send_Message(_client.Target_Server_Name, "history()");
                }
                else if (command == "showinfo()")
                {
                    _client.Show_Param();
                }
                else if (command == "help()")
                {
                    _client.Help();
                }
                else
                {
                    _client.Send_Message(input);
                }
            }
        }


    }



    class Client_Socket
    {
        private string _Client_Name;  //目前client 名稱
        private string _Ip = "127.0.0.1";
        private int _Port = 8080;
        public string Client_Name
        {
            get
            {
                return _Client_Name;
            }
            set
            {
                if (value == "")
                {
                    _Client_Name = Default_Client_Name;
                }
                else
                {
                    _Client_Name = value;
                }
            }
        } // 空 = 預設
        public string Ip
        {
            get
            {
                return _Ip;
            }
            set
            {
                _Ip = Socket_Tool.Is_IPv4(value) ? value : Default_Ip;
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


        internal string Target_Server_Name;  //目標server名稱

        private TcpClient client;
        private NetworkStream stream;

        private int Connect_Timeout = 100;
        private int Retry_Time = 10;  //重連次數

        private bool _isRunning = true;
        public bool Connected
        {
            get
            {
                if (client == null)
                    return false;
                else
                    return client.Connected;

            }
        }

        #region Default Parameter
        
        static private string Default_Client_Name = Get_Random_Name(); ///要改
        static private string Default_Ip = "127.0.0.1";
        static private int Default_Port = 8080;
        #endregion

        public Client_Socket()
        {
            Client_Name = Default_Client_Name;
            Ip = Default_Ip;
            Port = Default_Port;
        }

        public Client_Socket(string myname)
        {
            Client_Name = myname;

            Ip = Default_Ip;
            Port = Default_Port;
        }

        public Client_Socket(string name, string ipstr, int port)
        {
            Client_Name = name;
            Ip = ipstr;
            Port = port;
        }

        


        public void Connect()
        {
            Print_Tool.WriteLine("目標 : " + Ip + ":" + Port, ConsoleColorType.Notice);
            for (int i = 0; i < Retry_Time; i++)
            {
                try
                {
                    client = new TcpClient(Ip, Port);
                    Refresh();
                    Print_Tool.WriteLine("已連線至伺服器", ConsoleColorType.Announce);
                    stream = client.GetStream();

                    Greeting();
                    Read_Message();
                    break;
                }
                catch (Exception ex)
                {
                    Print_Tool.WriteLine(ex.Message + " (已嘗試" + (i + 1) + "/" + Retry_Time+ ")", ConsoleColorType.Warning);
                    Thread.Sleep(Connect_Timeout); // 延遲 0.5 秒後重試
                }
                
            }

        }

        public void Connect(string ip, int port)
        {
            Ip = ip;
            Port = port;

            Connect();
            

        }

        public void Start()
        {
            Show_Info();
            Connect();

            if (Connected)
            {
                Task.Run(() => Handle_Receive_Message());

                InputHandler inputHandler = new InputHandler(this);
                inputHandler.StartHandling();
                //HandleInput();
            }
            else
            {
                Print_Tool.WriteLine("連線失敗", ConsoleColorType.Error);
            }


        }

        public void Close()
        {
            Send_Message("Server", "close()");
            _isRunning = false;
            Print_Tool.WriteLine("Closing", ConsoleColorType.Warning);
            stream.Close();
            client.Close();
        }



        public void Send_Message(string message)// 發送資料至伺服器 一定是send color
        {
            message = Client_Name + " : " + message; 

            byte[] data = Encoding.Unicode.GetBytes(message);

            stream.Write(data, 0, data.Length);

            Print_Tool.WriteLine(message, ConsoleColorType.Send);

        }

        public void Send_Message(string towho, string message)
        {
            message = "To " + towho + " : " + message;
            Send_Message(message);
        }



        public void Read_Message()// Client 讀取 Server 輸入
        {
            byte[] buffer = new byte[1024];

            try
            {
                if (client.Connected)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string response = Encoding.Unicode.GetString(buffer, 0, bytesRead);
                    Print_Tool.WriteLine(response, ConsoleColorType.Reply); //顯示



                    string from_who = Get_Target(response);
                    string message = response.Substring(from_who.Length + 2).Trim(); //移除 誰丟出訊息
                    string command = message.ToLower(); // Command 全部為小寫

                    bool toMe = To_Me_ORNOT(message); //是不是再跟自己說話
                    
                    if (toMe)
                    {
                        command = Get_Command(command);

                        //command = response.Substring(from_who.Length + 2).Trim(); //移除 誰丟出訊息
                        Print_Tool.WriteLine(from_who + " IS CALLING MEEE!!!! ", ConsoleColorType.Notice);


                        if (message.Contains("Hello . This is Server: "))
                        {
                            Target_Server_Name = Get_Server_Name_From_Greeting(message);
                        }
                        if (command == "cls()")
                        {
                            Refresh();
                        }
                        else if (command == "close()")
                        {
                            Close();
                        }
                        else if (command == "greet()")
                        {
                            Greeting();
                        }


                    }
                    




                }


            }
            catch(Exception ex)
            {
                Print_Tool.WriteLine(ex.Message, ConsoleColorType.Error);

            }

        }

        public void Handle_Receive_Message()
        {
            while (_isRunning)
            {
                try
                {
                    Read_Message();
                    Thread.Sleep(50);
                }
                catch (Exception ex)
                {
                    Print_Tool.WriteLine("伺服器斷線" + ex.Message, ConsoleColorType.Error);
                    break;
                }
            }
        }









        #region Private Function

        public void Set_Default()
        {
            Client_Name = Default_Client_Name;
            Ip = "127.0.0.1";
            Port = 8080;
        }//預留


        private string Get_Target(string msg)
        {
            string target;

            string[] chunks = msg.Split(new[] { " :" }, StringSplitOptions.None);
            target = chunks[0].Trim();

            return target;
        }

        private string Get_Command(string msg) // 移除 To "name" : //剩下指令
        {
            string target;

            string[] chunks = msg.Split(new[] { " :" }, StringSplitOptions.None);
            target = chunks[1].Trim();

            return target;
        }

        private string Get_Server_Name_From_Greeting(string greet_reply)
        {
            string pattern = @"Server:\s*(.*)"; //?
            Match match = Regex.Match(greet_reply, pattern);

            if (match.Success)
            {
                string serverName = match.Groups[1].Value;
                //Console.WriteLine(serverName);
                
                return serverName;
            }else
                return "";
        
        }

        private bool To_Me_ORNOT(string msg)  //Ex: msg = "To Default_Client8351 : Hello . This is Server: Server" 
        {
            // 定義要匹配的前綴
            string[] prefixes = { "to ", "To " };

            // 檢查訊息是否以其中一個前綴開頭
            foreach (string prefix in prefixes)
            {
                if (msg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    // 取得以 ':' 為分隔符的部分
                    int colonIndex = msg.IndexOf(':');
                    if (colonIndex > prefix.Length)
                    {
                        // 提取用戶名稱並修剪多餘的空白
                        string name = msg.Substring(prefix.Length, colonIndex - prefix.Length).Trim();
                        bool pass = (name.Equals("all", StringComparison.OrdinalIgnoreCase));


                        // 比較用戶名稱是否與指定的 clientName 相符
                        return ((name == Client_Name) || pass);
                    }
                }
            }

            // 如果未找到有效的用戶名稱，返回 false
            return false;
        }

        private static string Get_Random_Name()
        {

            if (Default_Client_Name == null)
            {
                Random rnd = new Random();
                string name = "Default_Client";

                int rint = rnd.Next(0, 10000); // 生成介於 0 到 9999 的隨機數字
                string rstring = rint.ToString("D4");
                name += rstring;
                return name;
            }
            return "";
        }

        #endregion


        #region 功能區
        public void Greeting()
        {
            Send_Message("Hello Server. This is sent from " + Client_Name);
        }
        public void Show_Info()
        {
            Console.Clear();
            Print_Tool.WriteLine("Client端:", ConsoleColorType.Notice);
            Print_Tool.WriteLine("Name: " + Client_Name, ConsoleColorType.Default);
            Print_Tool.WriteLine("IP  : " + Ip, ConsoleColorType.Default);
            Print_Tool.WriteLine("Port: " + Port, ConsoleColorType.Default);
            Print_Tool.WriteLine("\n\n", ConsoleColorType.Default);
        }

        public void Refresh()
        {
            Show_Info();
        }

        internal void Show_Param()
        {
            Send_Message("Name = " + Client_Name);
            Send_Message("IP = " + Ip);
            Send_Message("Port = " + Port);
        }

        internal void Help()
        {
            Print_Tool.WriteLine("https://github.com/Luffy1225/Socket_Project", ConsoleColorType.Announce);
        }

        #endregion

    }

    class Client_Main
    {
        public static void Keyin_Param(out string name, out string ip, out int port) //可能為空
        {
            Print_Tool.WriteLine("輸入Client名字:", ConsoleColorType.Default);
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
                        portstr = args[3];
                        break;
                    default:
                        Print_Tool.WriteLine("參數輸入: " + args.Length + " 個 不合規定!", ConsoleColorType.Error);
                        break;
                }
            }
            else // 使用者 點擊 輸入參數
                Keyin_Param(out name, out ip, out port);


            Client_Socket Client = new Client_Socket(name, ip, port);
            Client.Set_Default();
            Client.Start();


            Console.ReadKey();
        }


    }
}

