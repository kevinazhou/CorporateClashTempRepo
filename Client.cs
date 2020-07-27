using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace CCServer
{
    class Client
    {
        //4mb
        public static int dataBufferSize = 4096;
        public int id;
        public TCP tcp;

        public Client(int clientID)
        {
            id = clientID;
            tcp = new TCP(id);
        }

        //Stores the TcpClient instance
        public class TCP
        {
            public TcpClient socket;

            private readonly int id;
            //NetworkStream: for sending and receiving data over stream sockets
            private NetworkStream stream;
            private byte[] receiveBuffer;
            public TCP(int _id)
            {
                id = _id;
            }

            public void Connect(TcpClient _socket)
            {
                socket = _socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();

                receiveBuffer = new byte[dataBufferSize];

                //WAITING FOR DATA FROM GAME CLIENT
                //begins async read operation
                //buffer to read data into, begin reading at 0 byte offset, max bytes to read
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                //SENDING WELCOME PACKET to the client
                Console.WriteLine("Sending welcome packet");
                ServerSend.Welcome(id, "Welcome to the server!");
            }

            public void SendData(Packet packet)
            {
                //try to catch errors
                try
                {
                    if(socket != null)
                    {
                        //BEGIN WRITING DATA TO UNITY CLIENT
                        stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending data to player {id} via TCP: {ex}");
                }
            }

            private void ReceiveCallback(IAsyncResult res)
            {
                try
                {
                    //waits for pending async read to complete
                    //res is a reference to the pending async request
                    int byteLength = stream.EndRead(res);
                    if(byteLength <= 0)
                    {
                        //TODO: Disconnect
                        return;
                    }

                    byte[] data = new byte[byteLength];

                    //copy received bytes into new array
                    Array.Copy(receiveBuffer, data, byteLength);

                    //TODO: HANDLE THE DATA HERE

                    //Continue reading data from the stream
                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error receiving TCP data: {ex}");
                    //TODO: Disconnect client
                }
            }
        }
    }
}
