using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Luffy_Tool;


namespace Server_Socket
{
    class Server_Socket
    {
        string Server_Name;
        string Host_IP = "127.0.0.1";
        int Port = 8080;

        TcpListener Server;
        List<TcpClient> Clients = new List<TcpClient>();


        List<string> history = new List<string>();

        public Server_Socket(string name)
        {
            Server_Name = name;
        }

        public Server_Socket(string name, string hosting_ip, int port)
        {
            Server_Name = name;
            Host_IP = hosting_ip;
            Port = port;
        }

        public void Refresh()
        {
            Show_Info();
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

            HandleInput();
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
                    

                    if (message.StartsWith("Hello Server. This is sent from "))
                    {
                        Greeting(fromwho);
                    }
                    else if (command == "list()")
                    {
                        Send_Message(fromwho,  Clients.Count + " In the Server");
                    }
                    else if (command == "close()")
                    {
                        Send_Message("好的 我將關閉連線 " + response);
                        client.Close();
                    }
                    else if (command == "history()")
                    {
                        Show_History(fromwho);
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

        private void HandleInput()  // Server 輸出到 Client
        {
            while (true)
            {
                Print_Tool.WriteLine("輸入文字: ", ConsoleColorType.Default);

                string input = Console.ReadLine();
                string low_input = input.ToLower();
                if (input.Equals("close()", StringComparison.OrdinalIgnoreCase))
                {
                    Print_Tool.WriteLine("Server 關閉中...", ConsoleColorType.Warning);
                    foreach (var client in Clients)
                    {
                        client.Close();
                    }
                    Server.Stop();
                    break;
                }
                else if (low_input == "cls()" )
                {
                    //Send_Message(input);
                    Refresh();
                }
                else if (low_input == "list()" )
                {
                    List_All_Clients();
                }
                else if (low_input == "history()")
                {
                    Show_History();
                }
                else if (low_input == "showinfo()")
                {
                    Show_Param();
                }
                else if (low_input == "greet()")
                {
                    Every_Greet();
                }
                else if (low_input == "help()")
                {
                    Help();
                }
                else
                {
                    Send_Message(input);
                }
            }
        }

        private void Send_Message(string message)
        {
            message = Server_Name + " : " + message;
            history.Add(message);

            byte[] data = Encoding.Unicode.GetBytes(message);
            foreach (var client in Clients)
            {
                NetworkStream stream = client.GetStream();
                stream.Write(data, 0, data.Length);
            }
            Print_Tool.WriteLine(message, ConsoleColorType.Send);
        }

        private void Send_Message(string towho, string message)
        {
            message = "To " + towho + " : " + message;
            Send_Message(message);
        }


        private void Greeting(string who = "")
        {
            //Send_Message("Hello " + who + ". This is " + Server_Name);
            Send_Message(who, "Hello . This is Server: " + Server_Name);
        }

        private string Get_Target(string msg)
        {
            string[] chunks = msg.Split(new[] { " :" }, StringSplitOptions.None);
            return chunks[0].Trim();
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


        private void List_All_Clients()
        {
            if (Clients.Count == 0)
                Print_Tool.WriteLine("0 Clients in the Server", ConsoleColorType.Default);

            for (int i = 0; i < Clients.Count; i++)
            {
                Print_Tool.WriteLine("No. "+ i + Clients[i].ToString() , ConsoleColorType.Default);
            }
        }

        private void Show_History(string towho ="")
        {
            for (int i = 0; i < history.Count; i++)
            {
                if (towho == "") // 如果沒有要傳給別人 就server自己顯示
                    Print_Tool.WriteLine(history[i] , ConsoleColorType.Announce);
                else 
                    Send_Message(towho, history[i]);
            }
        }

        private void Show_Param()
        {
            Send_Message("Server = " + Server_Name);
            Send_Message("IP = " + Host_IP);
            Send_Message("Port = " + Port);
        }

        private void Every_Greet()
        {
            Send_Message("all", "greet()");
        }


        private void Help()
        {
            Print_Tool.WriteLine("https://github.com/Luffy1225/Socket_Project", ConsoleColorType.Announce);
        }

    }

    class Program
    {

        public static void Keyin_Param(out string name, out string ip, out int port)
        {
            Print_Tool.WriteLine("輸入Server名字:", ConsoleColorType.Default);
            name = Console.ReadLine();
            if (name == "")
                name = "Server";
            Print_Tool.WriteLine("Server = " + name, ConsoleColorType.Notice);


            Print_Tool.WriteLine("輸入Server IP:", ConsoleColorType.Default);
            ip = Console.ReadLine();
            if (ip == "")
                ip = "127.0.0.1";
            Print_Tool.WriteLine("IP = " + ip, ConsoleColorType.Notice);


            Print_Tool.WriteLine("輸入Server Port:", ConsoleColorType.Default);
            string portstr = Console.ReadLine();
            port = 8080;
            if (portstr != "")
                port = int.Parse(portstr);
            Print_Tool.WriteLine("Port = " + port.ToString(), ConsoleColorType.Notice);

        }


        static void Main()
        {
            string name, ip;
            int port;
            Keyin_Param(out name, out ip, out port);

            Server_Socket Server = new Server_Socket(name, ip, port);
            Server.Start();








            Console.ReadKey();
        }
    }
}
