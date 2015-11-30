﻿using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using System.Collections.Generic;

public class ZumoControl : MonoBehaviour
{


    public string IpAddr = "192.168.1.27";

    public float DelayBetweenMotortUpdates = 0.03f;
    public UnityEngine.UI.Text rightMotorText;
    public UnityEngine.UI.Text leftMotorText;
    private float _motorUpdateTimer = 0.0f;
    private int _leftSpeed;
    private int _rightSpeed;

    public float DelayBetweenBatteryUpdates = 10.0f;
    public UnityEngine.UI.Text batteryVoltageText;
    private float _batteryUpdateTimer = 0.0f;
    private int _batteryVoltage;

    private AsyncSocketClient _client;


    // Use this for initialization
    void Start()
    {
        _client = new AsyncSocketClient();
        _client.StartClient(IpAddr);
        _leftSpeed = 0;
        _rightSpeed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        _motorUpdateTimer += Time.deltaTime;
        if (_motorUpdateTimer > DelayBetweenMotortUpdates)
        {
            _SendMotorsUpdate();
            _motorUpdateTimer = 0.0f;
        }
        _batteryUpdateTimer += Time.deltaTime;
        if (_batteryUpdateTimer > DelayBetweenBatteryUpdates)
        {
            _SendBatteryUpdate();
            _batteryUpdateTimer = 0.0f;
        }
        lock(_client.PacketBuffer)
        {
            if (_client.PacketBuffer.Count > 0)
            {
                AsyncSocketClient.Packet packet = _client.PacketBuffer.Dequeue();
                if (packet.id == 'b')
                    batteryVoltageText.text = "Battery Voltage : " + packet.args;
                else
                    Debug.Log("Unknown packet " + packet.id + " - " + packet.args);
            }
        }
    }

    void Stop()
    {
        _client.StopClient();
    }

    private void _SendMotorsUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        int leftspeed = (int)(v * 400.0f + h * 400.0f);
        int rightspeed = (int)(v * 400.0f + -h * 400.0f);
        bool changed = _leftSpeed != leftspeed && _rightSpeed != rightspeed;

        if (changed)
        {
            leftMotorText.text = "LeftMotor : " + leftspeed;
            rightMotorText.text = "RightMotor : " + rightspeed;
            if (_client.Connected)
            {
                byte[] data = Encoding.ASCII.GetBytes("l" + leftspeed.ToString() +
                                                     "\nr" + rightspeed.ToString() + "\n");
                _client.Send(data);
            }
            _leftSpeed = leftspeed;
            _rightSpeed = rightspeed;
        }
    }

    private void _SendBatteryUpdate()
    {
        byte[] data = Encoding.ASCII.GetBytes("b\n");
        _client.Send(data);
    }

  
    public class AsyncSocketClient
    {
        public class Packet
        {
            public char id;
            public string args;
        }

        public bool Connected = false;

        public Queue<Packet> PacketBuffer;

        private Socket _socket = null;
        private byte[] _receiveBuffer = new byte[1024];
        private string _receiveBufferStr;

        public void StartClient(string ip)
        {
            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                // This example uses port 11000 on the local computer.
                IPHostEntry ipHostInfo = Dns.GetHostEntry(ip);
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 1338);

                // Create a TCP/IP  socket.
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.
                _socket.Connect(remoteEP);
                Debug.Log("Socket connected to " + _socket.RemoteEndPoint.ToString());
                PacketBuffer = new Queue<Packet>();
                Connected = true;
                _socket.BeginReceive(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None,
                                     new AsyncCallback(ReceiveCallback), null);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        public void StopClient()
        {
            // Release the socket.
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        public bool Send(byte[] data)
        {
            try
            {
                // Send the data through the socket.
                SocketAsyncEventArgs socketAsyncData = new SocketAsyncEventArgs();
                socketAsyncData.SetBuffer(data, 0, data.Length);
                _socket.SendAsync(socketAsyncData);
            }
            catch (SocketException se)
            {
                Debug.Log("SocketException : " + se.ToString());
                return false;
            }
            return true;
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            //Check how much bytes are recieved and call EndRecieve to finalize handshake
            int recieved = _socket.EndReceive(AR);
            if (recieved <= 0)
                return;
            _receiveBufferStr += Encoding.ASCII.GetString(_receiveBuffer, 0, recieved);
            int indexEnd =_receiveBufferStr.IndexOf("\r\n");
            while (indexEnd != -1)
            {
                Packet packet = new Packet();
                packet.id = _receiveBufferStr[0];
                packet.args = _receiveBufferStr.Substring(1, indexEnd - 1);
                _receiveBufferStr = _receiveBufferStr.Substring(indexEnd + 2);
                lock (PacketBuffer)
                {
                    PacketBuffer.Enqueue(packet);
                }
                if (string.IsNullOrEmpty(_receiveBufferStr))
                    break;
                indexEnd = _receiveBufferStr.IndexOf("\r\n");
            }
            _socket.BeginReceive(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None,
                                 new AsyncCallback(ReceiveCallback), null);
        }
    }
}