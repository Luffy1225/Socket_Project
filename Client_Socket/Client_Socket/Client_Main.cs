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
    class Client_Socket
    {
        string Client_Name;  //目前client 名稱

        string Target_Server_Name;  //目標server名稱
        
        string Ip = "127.0.0.1";
        int Port = 8080;


        TcpClient client;

        NetworkStream stream;

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

        public Client_Socket(string myname)
        {
            Client_Name = myname;
            Ip = "127.0.0.1";
            Port = 8080;
        }

        public Client_Socket(string myname, string ipstr, int port)
        {
            Client_Name = myname;
            Ip = ipstr;
            Port = port;
        }

        public void Connect()
        {
            Print_Tool.WriteLine("目標 : " + Ip + ":" + Port, ConsoleColorType.Notice);
            for (int i = 0; i < 5; i++)
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
                    Print_Tool.WriteLine(ex.Message + " (已嘗試" + (i + 1) + "/5)", ConsoleColorType.Warning);
                    Thread.Sleep(500); // 延遲 0.5 秒後重試
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
                HandleInput();
            }
            else
            {
                Print_Tool.WriteLine("連線失敗", ConsoleColorType.Error);
            }


        }

        public void Close()
        {
            Send_Message("Server", "close()");
            client.Close();
        }

        public void Send_Message(string message)// 發送資料至伺服器 一定是send color
        {
            message = Client_Name + " : " + message; 

            byte[] data = Encoding.Unicode.GetBytes(message);

            stream.Write(data, 0, data.Length);

            Print_Tool.WriteLine(message, ConsoleColorType.Send);

        }

        private void Send_Message(string towho, string message)
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
        public void HandleInput() // Client 輸出到 server
        {
            while (true)
            {
                Print_Tool.WriteLine("輸入文字: ", ConsoleColorType.Default);
                string input = Console.ReadLine();
                string command = input.ToLower();
                if (command == "close()")
                {
                    Send_Message("我將關閉連線");
                    Print_Tool.WriteLine("Client關閉中...", ConsoleColorType.Warning);
                    client.Close();
                    break;
                }
                else if (command == "greet()")
                {
                    Greeting();
                }
                else if (command == "cls()")
                {
                    Refresh();
                }
                else if (command == "history()")
                {
                    Send_Message(Target_Server_Name, "history()");
                }
                else if (command == "showinfo()")
                {
                    Show_Param();
                }
                else if (command == "help()")
                {
                    Help();
                }
                else
                {
                    Send_Message(input);
                }
            }
        }

        public void Handle_Receive_Message()
        {
            while (true)
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



        public void Greeting()
        {
            Send_Message("Hello Server. This is sent from " + Client_Name);
        }

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


        private void Show_Param()
        {
            Send_Message("Name = " + Client_Name);
            Send_Message("IP = " + Ip);
            Send_Message("Port = " + Port);
        }

        private void Help()
        {
            Print_Tool.WriteLine("https://github.com/Luffy1225/Socket_Project", ConsoleColorType.Announce);
        }

    }

    class Client_Main
    {
        private static bool random = true; // 靜態變數

        public static void Keyin_Param(out string name, out string ip, out int port)
        {
            Random rnd = new Random();

            Print_Tool.WriteLine("輸入Client名字:", ConsoleColorType.Default);
            name = Console.ReadLine();
            if (name == ""){
                name = "Default_Client";
                if (random == true)
                {
                    int rint = rnd.Next(0, 10000); // 生成介於 0 到 9999 的隨機數字
                    string rstring = rint.ToString("D4");
                    name += rstring;
                }
                Print_Tool.WriteLine("Name = " + name, ConsoleColorType.Notice);
            }

            Print_Tool.WriteLine("輸入目標Server IP:", ConsoleColorType.Default);
            ip = Console.ReadLine();
            if (ip == "")
                ip = "127.0.0.1";
            Print_Tool.WriteLine("IP = " + ip, ConsoleColorType.Notice);

            Print_Tool.WriteLine("輸入目標Server Port:", ConsoleColorType.Default);
            string portstr = Console.ReadLine();
            port = 8080;
            if (portstr != "")
                port = int.Parse(portstr);
            Print_Tool.WriteLine("Port = " + port.ToString(), ConsoleColorType.Notice);

        }


        static void Main(string[] args)
        {
            string name, ip;
            int port;
            Keyin_Param(out name, out ip, out port);

            Client_Socket Client = new Client_Socket(name, ip, port);
            Client.Start();




            Console.ReadLine();
        }
    }

}
