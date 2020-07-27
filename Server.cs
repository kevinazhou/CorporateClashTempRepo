using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Xml.Serialization;
using System.Diagnostics.Contracts;

namespace CCServer
{
    class Server
    {
        //Max Players int, can only be set within Server class
        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }

        //client ids are the keys
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

        private static TcpListener tcpListener;

        public static void Start(int max, int portNum)
        {
            MaxPlayers = max;

            Port = portNum;
            Console.WriteLine("Starting server...");

            InitializeServerData();

            //Listen for client activity on all network interfaces
            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();

            //begins async operation to accept incoming connection attempt
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Console.WriteLine($"Server started on port {Port}.");
        }

        private static void TCPConnectCallback(IAsyncResult result)
        {
            //asynchronously accepts incoming connection attempt and creates a new TcpClient
            TcpClient client = tcpListener.EndAcceptTcpClient(result);

            //Awaits another connection attempt
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            //print out connecting client's ip and port
            Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}...");

            for(int i = 1; i <= MaxPlayers; i++)
            {
                if(clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(client);
                    Console.WriteLine("Client " + i + " connected");
                    return;
                }
            }

            Console.WriteLine($"{ client.Client.RemoteEndPoint} failed to connect: server full!");
        }

        private static void InitializeServerData()
        {
            for(int i = 1; i <= MaxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }
        }
    }
}
