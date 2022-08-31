
using System.Collections.Generic;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;

namespace SocketServer
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class Server : Window
    {
        private delegate void UpdateUI(string sMessage);

        public Server()
        {
            InitializeComponent();
            var serverSocket = new ServerSocket();
            SetTextBlock(serverSocket.serverInfo);
            serverSocket.CreateServerSocket();
        }

        private void SetTextBlock(ServerInfo info)
        {
            info.ClientIp = ClientIPTextblock;
            info.ClientPort = ClientPortTextblock;
            info.ClientFileName = ClientFileNameTextblock;
            info.ServerStatus = ServerStatusTextblock;
        }
    }

    public class ServerSocket
    {
        Socket m_server;
        Thread m_thrListening;
        public ServerInfo serverInfo;
        private int serverPort = 14000;
        private bool serverKeepListening = true;

        public ServerSocket()
        {
            serverInfo = new ServerInfo();
        }

        public void CreateServerSocket()
        {
            try
            {
                //Set Port Number and get local IP address               
                IPAddress localAddr = LocalIPAddress();
                IPEndPoint localEndPoint = new IPEndPoint(localAddr, serverPort);
                // Create Server 
                m_server = new Socket(localAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                if (m_server.Connected)
                {
                    m_server.Shutdown(SocketShutdown.Both);
                }
                m_server.Bind(localEndPoint);
                m_server.Listen(10);
                m_thrListening = new Thread(Listening);
                m_thrListening.Start();

            }
            catch (Exception ex)
            {
                UpdateStatus(ex.Message, serverInfo.ServerStatus);
            }

        }

        //Show text on WPF TextBox
        private void UpdateStatus(string sStatus, TextBlock block)
        {
            if (block == null)
                return;

            if (!block.Dispatcher.CheckAccess())
            {
                block.Dispatcher.BeginInvoke(new Action(() =>
                {
                    block.Text = sStatus;
                }));

            }
            else
            {
                block.Text = sStatus;
            }
            Thread.Sleep(1);
        }

        private void Listening()
        {

            try
            {
                do
                {
                    UpdateStatus("Waiting for clients", serverInfo.ServerStatus);
                    //  this server will wait until clients connent to this server 
                    Socket handler = m_server.Accept();
                    var remoteEndPoint = (IPEndPoint)handler.RemoteEndPoint;
                    var remoteIp = remoteEndPoint.Address.ToString();
                    var remotePort = remoteEndPoint.Port.ToString();
                    UpdateStatus(remoteIp, serverInfo.ClientIp);
                    UpdateStatus(remotePort, serverInfo.ClientPort);


                    // get file name from client
                    byte[] bytes = new Byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    var clientfileName = Encoding.ASCII.GetString(bytes, 0, bytesRec);                  
                    UpdateStatus(clientfileName, serverInfo.ClientFileName);

                    //get file path
                    string fileName = $@"C:\Users\pc\Pictures\{clientfileName}";

                    // check if file exist 
                    var isFileExist = File.Exists(fileName);
                    byte[] msg = new byte[1024];
                    if (isFileExist)
                    {
                        //Send OK message and file to Client
                        msg = Encoding.ASCII.GetBytes("OK");
                        handler.Send(msg);
                        Thread.Sleep(100);
                        handler.SendFile(fileName);
                    }
                    else
                    {
                        //Send Error message to Client
                        msg = Encoding.ASCII.GetBytes("File Not Found");
                        handler.Send(msg);
                    }

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                    UpdateStatus("Socket is Closed", serverInfo.ServerStatus);
                } while (serverKeepListening);

            }
            catch (SocketException ex)
            {
                UpdateStatus(ex.Message, serverInfo.ServerStatus);
            }
        }

        // get local IP Adress 
        private IPAddress LocalIPAddress()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                return null;
            }

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            return host
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }

        public void SetServerPort(int portNumber)
        {
            serverPort = portNumber;
        }

        public void SetServerKeepListening(bool value)
        {
            serverKeepListening = value;
        }

    }

    public class ServerInfo
    {
        public TextBlock ClientIp { get; set; }

        public TextBlock ClientPort { get; set; }

        public TextBlock ClientFileName { get; set; }

        public TextBlock ServerStatus { get; set; }
    }

}
