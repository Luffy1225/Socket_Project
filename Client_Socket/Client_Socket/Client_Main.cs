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


            Task.Run(() => Handle_Receive_Message());

            HandleInput();
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

        public void Read_Message()// 讀取來自Server的訊息
        {
            byte[] buffer = new byte[1024];

            try
            {
                if (client.Connected)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string response = Encoding.Unicode.GetString(buffer, 0, bytesRead);


                    string from_who = Get_Target(response);
                    string message = response.Substring(from_who.Length + 2).Trim(); //移除 誰丟出訊息
                    string command = message.ToLower(); // Command 全部為小寫

                    bool toMe = To_Me_ORNOT(message); //是不是再跟自己說話

                    if (toMe)
                    {
                        if (message.Contains("Hello . This is Server: "))
                        {
                            Target_Server_Name = Get_Server_Name_From_Greeting(message);
                            Print_Tool.WriteLine("Server Name : " + Target_Server_Name +" CONFIRMED!",ConsoleColorType.Notice);
                        }
                        if (command == "cls()")
                        {
                            Refresh();
                        }
                        else if (command == "close()")
                        {
                            Close();
                        }

                    }
                    



                    Print_Tool.WriteLine(response, ConsoleColorType.Reply);

                }


            }
            catch(Exception ex)
            {
                Print_Tool.WriteLine(ex.Message, ConsoleColorType.Error);

            }

        }

        public void Greeting()
        {
            Send_Message("Hello Server. This is sent from " + Client_Name);
        }

        public string Get_Target(string msg)
        {
            string target;

            string[] chunks = msg.Split(new[] { " :" }, StringSplitOptions.None);
            target = chunks[0].Trim();

            return target;
        }

        public string Get_Server_Name_From_Greeting(string greet_reply)
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

        public bool To_Me_ORNOT(string msg)
        {
            // 根據 "To " 分割字串
            string[] parts = msg.Split(new[] { "To " }, StringSplitOptions.None);

            // 確保分割後有兩部分，且第二部分包含 " :"，才能提取目標
            if (parts.Length == 2)
            {
                // 根據 " :" 分割第二部分
                string[] chunks = parts[1].Split(new[] { " :" }, StringSplitOptions.None);

                // 取得目標名稱並去除前後空白
                string target = chunks[0].Trim();
                string low_target = target.ToLower(); // 大小都有檢查

                // 判斷目標名稱是否符合條件
                if (target == Client_Name || low_target == "all" )
                    return true;
            }

            
            return false;
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
                    Print_Tool.WriteLine("發生錯誤" + ex.Message, ConsoleColorType.Error);
                    break;
                }
            }
        }

        public void HandleInput()
        {
            while (true)
            {
                Print_Tool.WriteLine("輸入文字: ", ConsoleColorType.Default);
                string input = Console.ReadLine();
                if (input == "close()" || input == "Close()")
                {
                    Send_Message("我將關閉連線");
                    Print_Tool.WriteLine("Client關閉中...", ConsoleColorType.Warning);
                    client.Close();
                    break;
                }
                else
                {
                    Send_Message(input);
                }
            }
        }

        public void Show_Info()
        {
            Console.Clear();
            Print_Tool.WriteLine("Name: " + Client_Name, ConsoleColorType.Default);
            Print_Tool.WriteLine("IP  : " + Ip, ConsoleColorType.Default);
            Print_Tool.WriteLine("Port: " + Port, ConsoleColorType.Default);
            Print_Tool.WriteLine("\nClient端:   \n\n\n", ConsoleColorType.Notice);
        }

        public void Refresh()
        {
            Show_Info();
        }

    }

    class Client_Main
    {
        private static bool random = true; // 靜態變數

        public static void Keyin_Param(out string name, out string ip, out int port)
        {
            
            Random rnd = new Random();


            Print_Tool.WriteLine("輸入名字:", ConsoleColorType.Default);
            name = Console.ReadLine();
            if (name == ""){
                name = "Default_Client";
                if (random == true)
                {
                    int rint = rnd.Next(0, 10000); // 生成介於 0 到 9999 的隨機數字
                    string rstring = rint.ToString("D4");
                    name += rstring;
                }

            }

            Print_Tool.WriteLine("輸入IP:", ConsoleColorType.Default);
            ip = Console.ReadLine();
            if (ip == "")
                ip = "127.0.0.1";

            Print_Tool.WriteLine("輸入Port:", ConsoleColorType.Default);
            string portstr = Console.ReadLine();
            port = 8080;
            if (portstr != "")
                port = int.Parse(portstr);

        }


        static void Main(string[] args)
        {
            string name, ip;
            int port;
            Keyin_Param(out name,out ip,out port);

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



            Console.ReadLine();
        }
    }

}
