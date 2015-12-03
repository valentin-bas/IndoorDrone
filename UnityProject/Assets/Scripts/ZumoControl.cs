using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using System.Collections.Generic;

public class ZumoControl : MonoBehaviour
{
    public Drone Drone;
    public int AngleOffset = 300;

    public string IpAddr = "192.168.1.27";

    public float DelayBetweenMotortUpdates = 0.03f;
    public UnityEngine.UI.Text rightMotorText;
    public UnityEngine.UI.Text leftMotorText;
    private float _motorUpdateTimer = 0.0f;
    private int _leftSpeed;
    private int _rightSpeed;
    private float _oldh;
    private float _oldv;

    public float DelayBetweenBatteryUpdates = 10.0f;
    public UnityEngine.UI.Text batteryVoltageText;
    private float _batteryUpdateTimer = 0.0f;
    private int _batteryVoltage;

    public AsyncSocketClient _client;

    private float maxSpeed = 300.0f;
    public void SetSpeed(float f)
    {
        maxSpeed = f;
    }

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

                if (packet.id == 's')
                {
                    string[] tokens = packet.args.Split('-');
                    if (tokens.Length == 2)
                    {
                        float front = float.Parse(tokens[0]);
                        float right = float.Parse(tokens[1]);

                        if (right != 0.0f)
                        {
                            Drone.right = right / 100;
                        }
                        if (front != 0.0f)
                        {
                            Drone.up = front / 100;
                        }
                        Drone.left = -1.0f;
                        Drone.down = -1.0f;
                        Drone.Dirty = true;

                        Debug.DrawLine(Drone.transform.position, Drone.transform.position + Drone.transform.up * Drone.up, Color.red, 0.1f);
                        Debug.DrawLine(Drone.transform.position, Drone.transform.position + Drone.transform.right * Drone.right, Color.red, 0.1f);
                    }
                }
                else if (packet.id == 'a')
                {
                    int angle = int.Parse(packet.args) - AngleOffset;
                    Debug.Log(angle);

                    Drone.transform.rotation = Quaternion.AngleAxis(angle, -Vector3.forward);
                }
                else if (packet.id == 'b')
                {
                    if (batteryVoltageText != null)
                    {
                        batteryVoltageText.text = "Battery Voltage : " + packet.args;
                    }
                }
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

        //snap h and v
        //----------------------------------
        float tmph = h;
        float tmpv = v;
        if (_oldh != 0.0f && Mathf.Abs(_oldh) > Mathf.Abs(h))
            h = 0.0f;
        else if (h != 0.0f)
            h = h > 0.0f ? 1.0f : -1.0f;
        if (_oldv != 0.0f && Mathf.Abs(_oldv) > Mathf.Abs(v))
            v = 0.0f;
        else if (v != 0.0f)
            v = v > 0.0f ? 1.0f : -1.0f;
        _oldh = tmph;
        _oldv = tmpv;
        //----------------------------------

        int leftspeed = (int)(v * maxSpeed + h * maxSpeed);
        int rightspeed = (int)(v * maxSpeed + -h * maxSpeed);
        bool changed = _leftSpeed != leftspeed && _rightSpeed != rightspeed;

        if (changed)
        {
            if (leftMotorText != null)
            {
                leftMotorText.text = "LeftMotor : " + leftspeed;
            }
            if (rightMotorText != null)
            {
                rightMotorText.text = "RightMotor : " + rightspeed;
            }
            if (_client.Connected)
            {
                byte[] data = Encoding.ASCII.GetBytes("l" + leftspeed.ToString() +
                                                     "\nr" + rightspeed.ToString() + "\n");
                _client.Send(data);

                StartCoroutine(Stop(1.0f));
            }
            _leftSpeed = leftspeed;
            _rightSpeed = rightspeed;
        }
    }

    private IEnumerator Stop(float t)
    {
        yield return new WaitForSeconds(t);

        byte[] data = Encoding.ASCII.GetBytes("l0\nr0\n");
        _client.Send(data);
        enabled = false;
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