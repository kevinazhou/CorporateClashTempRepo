using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public class Client : MonoBehaviour
{
    public static Client instance;

    public static int dataBufferSize = 4096;

    public string ip = "127.0.0.1";
    public int port = 26950;
    public int myID = 0;
    public TCP tcp;

    private delegate void PacketHandler(Packet packet);
    private static Dictionary<int, PacketHandler> packetHandlers;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void Start()
    {
        tcp = new TCP();
    }

    public void ConnectToServer()
    {
        InitializeClientData();
        tcp.Connect();
    }

    public class TCP
    {
        public TcpClient socket;

        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public void Connect()
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
        }

        private void ConnectCallback(IAsyncResult res)
        {
            socket.EndConnect(res);

            if(!socket.Connected)
            {
                return;
            }

            stream = socket.GetStream();

            receivedData = new Packet();

            //WAITING FOR DATA FROM THE SERVER
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }

        private void ReceiveCallback(IAsyncResult res)
        {
            try
            {
                //waits for pending async read to complete
                //res is a reference to the pending async request
                int byteLength = stream.EndRead(res);
                if (byteLength <= 0)
                {
                    //TODO: Disconnect
                    return;
                }

                byte[] data = new byte[byteLength];

                //copy received bytes into new array
                Array.Copy(receiveBuffer, data, byteLength);

                //HANDLE THE DATA HERE
                receivedData.Reset(HandleData(data));

                //Continue reading data from the stream
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving TCP data: {ex}");
                //TODO: Disconnect client
            }
        }

        //Reorder bytes that may have arrived out of order
        private bool HandleData(byte[] data)
        {
            int packetLength = 0;

            receivedData.SetBytes(data);

            //If received data contains more than 4 unread bytes,
            //then this is the start of a packet, since the length is stored
            //as an int at the start of the packet
            if(receivedData.UnreadLength() >= 4)
            {
                packetLength = receivedData.ReadInt();
                if(packetLength <= 0)
                {
                    return true;
                }
            }

            //While there is still data to be read
            while(packetLength > 0 && packetLength <= receivedData.UnreadLength())
            {
                //Read the packet's bytes into a new byte array
                byte[] packetBytes = receivedData.ReadBytes(packetLength);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    //Create a new packet from the packetBytes
                    using (Packet packet = new Packet(packetBytes))
                    {
                        //Read the packet's ID
                        int packetID = packet.ReadInt();

                        //Now we can invoke a specific delegate function based on the packet's ID
                        packetHandlers[packetID](packet);
                    }
                });

                packetLength = 0;
                if (receivedData.UnreadLength() >= 4)
                {
                    packetLength = receivedData.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if(packetLength <= 1)
            {
                //Reset the packet
                return true;
            }
            //Otherwise don't reset the packet as there is a partial packet left
            return false;
        }
    }

    private void InitializeClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            //First packet has Welcome function to handle it
            { (int)ServerPackets.welcome, ClientHandle.Welcome }
        };
        Debug.Log("Initialized packets");
    }
}
