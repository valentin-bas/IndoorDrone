using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;

public class ZumoControl : MonoBehaviour
{
    public UnityEngine.UI.Text rightMotorText;
    public UnityEngine.UI.Text leftMotorText;

    public float DelayBetweenNetUpdate = 0.1f;

    private SynchronousSocketClient _client;
    private float _netUpdateTimer = 0.0f;

    private int _leftSpeed;
    private int _rightSpeed;

    // Use this for initialization
    void Start ()
    {
        _client = new SynchronousSocketClient();
        _client.StartClient();
        _leftSpeed = 0;
        _rightSpeed = 0;
    }
	
	// Update is called once per frame
	void Update ()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        int leftspeed = (int)(v * 400.0f + h * 400.0f);
        int rightspeed = (int)(v * 400.0f + -h * 400.0f);

        bool changed = _leftSpeed != leftspeed && _rightSpeed != rightspeed;

        _netUpdateTimer += Time.deltaTime;
        if (_netUpdateTimer > DelayBetweenNetUpdate)
        {
            if (changed)
            {
                leftMotorText.text = "LeftMotor : " + leftspeed;
                rightMotorText.text = "RightMotor : " + rightspeed;
                if (_client.Connected)
                    _client.Send(leftspeed, rightspeed);
                _leftSpeed = leftspeed;
                _rightSpeed = rightspeed;
            }
            _netUpdateTimer = 0.0f;
        }
    }

    void Stop()
    {
        _client.StopClient();
    }
}

public class SynchronousSocketClient
{
    Socket _socket = null;

    public bool Connected = false;

    public void StartClient()
    {
        // Data buffer for incoming data.
        byte[] bytes = new byte[1024];

        // Connect to a remote device.
        try
        {
            // Establish the remote endpoint for the socket.
            // This example uses port 11000 on the local computer.
            IPHostEntry ipHostInfo = Dns.Resolve("192.168.1.20");
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 1338);

            // Create a TCP/IP  socket.
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Connect the socket to the remote endpoint. Catch any errors.
            try
            {
                _socket.Connect(remoteEP);
                Debug.Log("Socket connected to " + _socket.RemoteEndPoint.ToString());
                Connected = true;
            }
            catch (ArgumentNullException ane)
            {
                Debug.Log("ArgumentNullException : " + ane.ToString());
            }
            catch (SocketException se)
            {
                Debug.Log("SocketException : " + se.ToString());
            }
            catch (Exception e)
            {
                Debug.Log("Unexpected exception : " + e.ToString());
            }

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

    public bool Send(int leftMotorSpeed, int rightMotorSpeed)
    {
        try
        {
            // Encode the data string into a byte array.
            byte[] msg = Encoding.ASCII.GetBytes("l" + leftMotorSpeed.ToString() +
                                                 "\nr" + rightMotorSpeed.ToString() + "\n");

            // Send the data through the socket.
            int bytesSent = _socket.Send(msg);
        }
        catch (SocketException se)
        {
            Debug.Log("SocketException : " + se.ToString());
            return false;
        }
        return true;
    }

    public void Receive()
    {
        // Receive the response from the remote device.
        //int bytesRec = _socket.Receive(bytes);
        //Console.WriteLine("Echoed test = {0}",
        //    Encoding.ASCII.GetString(bytes, 0, bytesRec));
    }
}