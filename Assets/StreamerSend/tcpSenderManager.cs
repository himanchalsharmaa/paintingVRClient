using UnityEngine;
using System.Net.Sockets;
using System.Threading.Tasks;
using System;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Android;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections;
using System.Net.Http;
using System.Net;
using System.Text;

public class tcpSenderManager : MonoBehaviour
{
    private string ipaddress ; //192.168.29.154 //192.168.1.13
    private static tcpSenderManager instance;
    public TcpClient tcpClient;
    public NetworkStream networkStream;
    public TMP_Text textlog;
    private int port;
    public int tryout = 0;
    Thread clientthread;
    Thread discoveryThread;
    ConcurrentQueue<string> debuglogs = new ConcurrentQueue<string>();
    string lasterror = "";
    private const int DiscoveryPort = 53123;

    private void Awake()
    {
        if (instance == null)
        {
            StartCoroutine(threadloghandler());
            instance = this;
            tcpClient = new TcpClient();
            Application.logMessageReceived += HandleLog;
            DontDestroyOnLoad(gameObject);
            discoveryThread = new Thread(ServerDiscovery) { IsBackground = true };
            discoveryThread.Start();
            //   ConnectToServer();
        }
        else
        {
            //Do nothing for now
        }
    }

    private void ServerDiscovery()
    {

        UdpClient udpClient = new UdpClient(DiscoveryPort);
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, DiscoveryPort);
       // udpClient.Client.Bind(endPoint);
        while (true)
        {
            debuglogs.Enqueue("lsitening on:" + endPoint.ToString());
            byte[] data = udpClient.Receive(ref endPoint);
            string message = Encoding.ASCII.GetString(data);
            if (message.StartsWith("TheDirector:"))
            {
                debuglogs.Enqueue("in server discovery6");
                Debug.Log("in server discovery6");
                string[] parts = message.Split(':');
                if (parts.Length == 2)
                {
                    ipaddress = endPoint.Address.ToString();
                    port = int.Parse(parts[1]);
                    debuglogs.Enqueue(ipaddress + ":" + port);
                    // Now you can use the discovered IP and port to connect to the server
                    ConnectToServer();
                    break;
                }
            }
        }
    }
    void HandleLog(string logString, string stackTrace, LogType type)
    {
        textlog.text += logString+"<br>";
    }

    public void ConnectToServer()
    {
        clientthread = new Thread(ClientStreamAsync) { IsBackground = true };
       // Debug.Log("Calling thread start");
        debuglogs.Enqueue("Calling thread start");
        clientthread.Start();
    }

    async void ClientStreamAsync()
    {
       // debuglogs.Enqueue("Started Thread");
        while (true)
        {
            try
            {
                tcpClient = new TcpClient();
                tcpClient.NoDelay = true;
                tcpClient.Client.NoDelay = true;
                await tcpClient.ConnectAsync(ipaddress, port);       //In case something is holding up the connect method but listener is there then wait 30 seconds
              //  Debug.Log("Connected on: " + ipaddress + ":" + port);
                debuglogs.Enqueue("Connected to ip: " + tcpClient.Connected);
                if (tcpClient.Connected)
                {
                    networkStream = tcpClient.GetStream();
                    break;
                }
            }
            catch (Exception e)
            {
                if (e.ToString() != lasterror)
                {
                    lasterror = e.ToString();
                    debuglogs.Enqueue("No connection Made" + e);
                }
            }
        }
    }
    public TcpClient GetTcpClient()
    {
        return tcpClient;
    }

    public NetworkStream GetNetworkStream()
    {
        return networkStream;
    }
    private void OnApplicationQuit()
    {
        discoveryThread.Abort();
        if (tcpClient != null)
        {
            tcpClient.Close();
        }
    }
    private void OnDisable()
    {
        discoveryThread.Abort();
        if (tcpClient != null)

        {
          //  Debug.Log("Closed");
            tcpClient.Close();
        }
    }

    IEnumerator threadloghandler()
    {
      //  Debug.Log("In coroutine");
        while(true)
        {
            yield return new WaitForEndOfFrame();
            
                if(debuglogs.Count > 0)
                {
                    string log;
                    debuglogs.TryDequeue(out log);
                    Debug.Log(log);
                }
            
        }
    }
}
