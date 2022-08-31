using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocketClient;
using SocketServer;
using System;

namespace SocketTest
{
    [TestClass]
    public class SocketServerClientTest
    {
        [TestMethod]   
        public void TestServerClient()
        {
            var serverSocket = new ServerSocket();
            serverSocket.SetServerKeepListening(false);
            serverSocket.CreateServerSocket();

            var clientSocket = new ClientSocket();
            var result = clientSocket.CreateSocketClient(new ClientInputInfo
            {
                IP = "192.168.0.222",
                Port = "14000",
                FileName = "parrot.jpg",
                FilePath = @"D:\"               
            });

            Assert.AreEqual("Successful", result.Status);
        }   
    }
}
