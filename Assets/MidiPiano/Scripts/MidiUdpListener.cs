using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class MidiUdpListener
{
    public const int NOTE_ON = 144;
    public const int NOTE_OFF = 128;

    public int port = 4289;

    private UdpClient client;
    private IPEndPoint sender;

    public bool Running { get; set; }

    private bool[] notePressed = new bool[88];


    public event Action<int> NoteOn;
    public event Action<int> NoteOff;

    public void Start()
    {
        sender = new IPEndPoint(IPAddress.Any, port);
        client = new UdpClient(port);
        //client.Client.ReceiveTimeout = 5000;
        Running = true;
        ThreadPool.QueueUserWorkItem(AcquireFromUDP, null);
    }

    public bool IsKeyDown(int key)
    {
        return notePressed[key];
    }

    public List<int> GetPressedKeys()
    {
        var keys = new List<int>(10);
        for (int i = 0; i < notePressed.Length; i++)
        {
            if (notePressed[i])
                keys.Add(i);
        }
        return keys;
    }

    private void AcquireFromUDP(System.Object nullval)
    {
        while (Running)
        {
            byte[] receivedData = client.Receive(ref sender);
            if (receivedData.Length > 0) { HandleData(receivedData); }
        }
    }

    private void HandleData(byte[] data)
    {
        byte cmd = data[0];
        byte note = data[1];

        int noteIndex = note - 24;

        switch (cmd)
        {
            case NOTE_ON:
                notePressed[noteIndex] = true;
                if (NoteOn != null)
                    NoteOn(noteIndex);
                break;
            case NOTE_OFF:
                notePressed[noteIndex] = false;
                if (NoteOff != null)
                    NoteOff(noteIndex);
                break;
        }
    }

    public void Stop()
    {
        Debug.Log("Close socket");
        client.Client.Close();
        Running = false;
    }
}

