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

        public void Start()
        {
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

        private async Task HandleClient(TcpClient client)  //接收input from client
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

        private void HandleInput()  //server 下指令
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
                    Send_Message(input);
                    Refresh();
                }
                else if (low_input == "list()" )
                {
                    List_All_Clients();
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
            Print_Tool.WriteLine("Name: " + Server_Name, ConsoleColorType.Default);
            Print_Tool.WriteLine("IP  : " + Host_IP, ConsoleColorType.Default);
            Print_Tool.WriteLine("Port: " + Port, ConsoleColorType.Default);
            Print_Tool.WriteLine("\nServer端:   \n\n\n", ConsoleColorType.Notice);
        }

        public void Refresh()
        {
            Show_Info();
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


    }

    class Program
    {
        static void Main()
        {
            Print_Tool.WriteLine("Server端:   \n\n\n", ConsoleColorType.Notice);
            Server_Socket server = new Server_Socket("Server");
            server.Start();
        }
    }
}
