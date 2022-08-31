using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;


namespace SocketClient
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class Clinet : Window
    {

        public Clinet()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var clientInput = new ClientInputInfo();
            var client = new ClientSocket();
            var isUIEmpty = CheckAndCopyUIInput(clientInput);

            if (isUIEmpty)
            {
                ServerStatus.Text = "please fill all required fields";
            }
            else
            {
                var status = client.CreateSocketClient(clientInput);
                ServerStatus.Text = status.Status;
            }
            
        }

        private bool CheckAndCopyUIInput(ClientInputInfo clientInput)
        {
            if (!string.IsNullOrWhiteSpace(ServerIPTextBox.Text) &&
                !string.IsNullOrWhiteSpace(ServerPortTextBox.Text) &&
                !string.IsNullOrWhiteSpace(ServerFileNameTextBox.Text) &&
                !string.IsNullOrWhiteSpace(ClientFilePathTextBox.Text))
            {  // Show Information on UI
                clientInput.IP = ServerIPTextBox.Text;
                clientInput.Port = ServerPortTextBox.Text;
                clientInput.FileName = ServerFileNameTextBox.Text;
                clientInput.FilePath = ClientFilePathTextBox.Text;
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    public class ClientSocket
    {

        public ClientOutputInfo CreateSocketClient(ClientInputInfo inputInfo)
        {
            ClientOutputInfo outputInfo = new ClientOutputInfo() { Status = "Successful" };
            try
            {
                IPAddress ipAddress = IPAddress.Parse(inputInfo.IP);
                int nPort = int.Parse(inputInfo.Port);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, nPort);
                Socket clientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(remoteEP);

                // Encode the data string into a byte array.  
                byte[] msg = Encoding.ASCII.GetBytes(inputInfo.FileName);
                // Send the data through the socket.  
                int bytesSent = clientSocket.Send(msg);

                byte[] bytes = new byte[1024];
                int bytesRec = clientSocket.Receive(bytes);
                string remoteFileStatus = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                if (remoteFileStatus == "OK")
                {
                    // receive file
                    const int arSize = 100;
                    byte[] buffer = new byte[arSize];
                    int readBytes = -1;
                    int blockCtr = 0;
                    int totalReadBytes = 0;

                    string outPath = $@"{inputInfo.FilePath+'/'+inputInfo.FileName}";
                    Stream strm = new FileStream(outPath, FileMode.OpenOrCreate);
                    while (readBytes != 0)
                    {
                        readBytes = clientSocket.Receive(buffer, 0, arSize, SocketFlags.None, out SocketError errorCode);
                        blockCtr++;
                        totalReadBytes += readBytes;
                        strm.Write(buffer, 0, readBytes);
                    }
                }
                else
                {
                    outputInfo.Status = remoteFileStatus;
                }

                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
            catch (Exception ex)
            {
                outputInfo.Status = ex.Message;
            }
            return outputInfo;
        }
    }

    public class ClientInputInfo
    {
        public string IP { get; set; } = "";
        public string Port { get; set; } = "";
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
    }

    public class ClientOutputInfo
    {
        public string Status { get; set; } = "";
    }
}
