using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Schema;

namespace CCServer
{
    class ServerSend
    {
        private static void SendTCPData(int toClient, Packet packet)
        {
            //prepare packet to be sent

            //Write length writes the length of the buffer as an int to the very front of the packet
            packet.WriteLength();
            Server.clients[toClient].tcp.SendData(packet);
        }

        private static void SendTCPDataToAll(Packet packet)
        {
            packet.WriteLength();
            for(int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(packet);
            }
        }

        private static void SendTCPDataToAll(int exceptClient, Packet packet)
        {
            packet.WriteLength();
            for(int i = 1; i <= Server.MaxPlayers; i++)
            {
                if(i != exceptClient)
                {
                    Server.clients[i].tcp.SendData(packet);
                }
            }
        }

        public static void Welcome(int toClient, string msg)
        {
            //Packet is IDisposable, so using block to clean up after it is no longer needed
            using (Packet packet = new Packet((int)ServerPackets.welcome))
            {
                //Write the msg and the client to the packet
                packet.Write(msg);
                packet.Write(toClient);

                SendTCPData(toClient, packet);
            }
        }
    }
}
