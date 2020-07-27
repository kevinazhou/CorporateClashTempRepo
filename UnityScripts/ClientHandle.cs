using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class for handling data from server
public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet packet)
    {
        //Read in the order they were written
        string msg = packet.ReadString();
        int myID = packet.ReadInt();

        Debug.Log($"Message from server: {msg}");
        Client.instance.myID = myID;

        //TODO: send welcome received packet
    }
}
