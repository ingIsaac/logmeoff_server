using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cassia;
using System.Net.Sockets;
using System.Net;

namespace logmeoff_server
{
    class Program
    {
        private byte[] data = new byte[1024];
        private Socket s_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private List<object[]> client_sockets = new List<object[]>();
        int port = 6401;
        ITerminalServer server;
        string version = "v. 1.1.0.0";

        static void Main(string[] args)
        {
            //Check if already open
            System.Diagnostics.Process[] p = System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName);
            if (p.Length > 1)
            {
                Environment.Exit(0);
            }
            //---------------------------->
            Console.Title = "LogMeOFF";
            new Program().setUpServer();
            bool r = true;
            while (r)
            {
               if(Console.ReadKey().Key == ConsoleKey.Escape)
                {
                    r = false;
                }
            }
        }

        private void setUpServer()
        {
            s_socket.Bind(new IPEndPoint(IPAddress.Any, port));
            s_socket.Listen(50);
            s_socket.BeginAccept(new AsyncCallback(acceptCallBack), null);
            initServer();
            Console.WriteLine("Iniciado Servidor...  " + version);
            Console.WriteLine("Iniciado el servidor en puerto: " + port);
            Console.WriteLine("LogMeOFF por Isaac Solís Ramírez.");
        }

        private void acceptCallBack(IAsyncResult AR)
        {
            try
            {
                Socket socket = s_socket.EndAccept(AR);
                socket.BeginReceive(data, 0, data.Length, SocketFlags.None, new AsyncCallback(reciveCallBack), socket);
                //Continue
                s_socket.BeginAccept(new AsyncCallback(acceptCallBack), null);
            }
            catch (Exception)
            {
                //Do nothing
            }
        }

        private void reciveCallBack(IAsyncResult AR)
        {
            try
            {
                Socket socket = (Socket)AR.AsyncState;
                int r = socket.EndReceive(AR);
                string user = string.Empty;
                if (r > 0)
                {
                    byte[] d = new byte[r];
                    Array.Copy(data, d, r);
                    user = Encoding.Default.GetString(d);
                }
                if (user != string.Empty)
                {
                    disconnectUSER(user);
                }
                else
                {
                    socket.Close();
                }
            }
            catch (Exception)
            {
                Console.WriteLine("[Error] problema al recibir los bytes de información.");
            }
        }

        private void initServer()
        {
            try
            {
                ITerminalServicesManager manager = new TerminalServicesManager();
                server = manager.GetLocalServer();
                server.Open();                                    
            }
            catch (Exception)
            {
                Console.WriteLine("[Error] no se ha podido identificar el terminal server.");
            }
        }

        private void disconnectUSER(string user)
        {
            try
            {               
                foreach (ITerminalServicesSession session in server.GetSessions())
                {
                    if (session.UserName == user)
                    {
                        session.Disconnect();
                        session.Logoff();
                        Console.WriteLine("-Cerrando Sesión Usuario: " + user);
                    }
                }               
            }
            catch (Exception)
            {
                Console.WriteLine("[Error] ha surgido un problema durante el cierre de sesión de un usuario.");
            }         
        }
    }
}
