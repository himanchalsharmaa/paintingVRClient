using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Concurrent;
using TMPro;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using System.Net.Http;

public class preLaunchSend : MonoBehaviour
{
    public GameObject[] xrrig;
    tcpSenderManager tcpSM;
    private TcpClient tcpClient;
    private bool connected = false;
    public TMP_Text info;
    NetworkStream stream;
    ConcurrentQueue<byte[]> bytearraytowrite = new ConcurrentQueue<byte[]>();
    string lastscenename ;
    controllerInputActions cia;
    bool runtimeexception = false;

    void Start()
    {
        tcpSM = GetComponent<tcpSenderManager>();
        if (tcpSM)
        {
          cia = GetComponent<controllerInputActions> ();
          StartCoroutine(getclientandstream());
          lastscenename = SceneManager.GetActiveScene().name;
          Application.logMessageReceived += HandleLog;
        }
        else
        {
            Debug.Log("Cannot find instance of tcpSenderManager");
        }
        
    }
    private void Update()
    {
        if (runtimeexception)
        {
            stream.Close();
            tcpClient.Close();
            Debug.Log(tcpClient.Connected);
            tcpSM.ConnectToServer();
            StopCoroutine("fillquewithdata");
            StartCoroutine("getclientandstream");
            runtimeexception = false;
            Debug.Log("Inside runtime exception");
        }
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        info.text += logString + "<br>";
    }

    IEnumerator getclientandstream()
    {
        while (true)
        {
                yield return new WaitForSeconds(1);
                tcpClient = tcpSM.GetTcpClient();
            tcpClient.NoDelay = true;
            tcpClient.Client.NoDelay = true;
                stream = tcpSM.GetNetworkStream();
                if (tcpClient != null && tcpClient.Connected && stream.CanWrite) 
                { 
                Debug.Log("got both");
                bytearraytowrite.Clear();
                connected = true;Task.Factory.StartNew(streamWriteonThread);
                StartCoroutine("fillquewithdata");
                break; 
                }
        }
    }
    IEnumerator fillquewithdata()
    {
       // Debug.Log("Started Writing Thread");
        while (true)
        {
            if (connected)
            {
                byte[] sendarr = getfinalbytearray(xrrig, cia.btnpress, SceneManager.GetActiveScene().name);
                bytearraytowrite.Enqueue(sendarr);
                /*                byte[] sendarr = getnetworkstream(xrrig);
                                bytearraytowrite.Enqueue(sendarr);
                                byte[] sendbtn = getbuttoninfo(cia.btnpress, SceneManager.GetActiveScene().name);
                                bytearraytowrite.Enqueue(sendbtn);*/
            }
            yield return new WaitForEndOfFrame();
        }
    }

    void streamWriteonThread()
    {
//        int exceptioncount = 0;
        while (true)
        {
            if (bytearraytowrite.Count > 0 && connected)
            {
                try
             {
                byte[] towrite;

                if (bytearraytowrite.TryDequeue(out towrite))
                {
                    stream.Write(towrite, 0, towrite.Length);
                    Debug.Log("Written to stream by thread: "+ towrite.Length);
                }
                else { continue; }
             }
            catch (Exception e)
             {
                    runtimeexception = true;
                    Debug.Log(e);
                    break;
             }
            }
        }  
    }

    byte[] getfinalbytearray(GameObject[] game, string buttoninfo, string sceneName)
    {
        List<byte> byteList = new List<byte>();
        // Writing GameObject's PRS begins

        foreach (GameObject go in game)
        {
            float[] prsArray = new float[] { go.transform.localPosition.x, go.transform.localPosition.y, go.transform.localPosition.z,
            go.transform.rotation.x, go.transform.rotation.y, go.transform.rotation.z, go.transform.rotation.w };

            foreach (float value in prsArray)
            {
                byteList.AddRange(BitConverter.GetBytes(value));
            }
        }

        //Ends
        //Writing button and sceneinfo begins

        if (buttoninfo != "")
        {
            //   Debug.Log(buttoninfo);
            byteList.Add(1);
            byteList.Add(byte.Parse("" + buttoninfo.Length));
            for (int i = 0; i < buttoninfo.Length; i++)
            {
                byteList.Add(byte.Parse("" + buttoninfo[i])); //If one button is pressed, this will transfer 2 bytes instead of 8
            }
            Debug.Log(buttoninfo);
        }
        else
        {
            byteList.Add(0);
        }
        //For Scene change
        if (sceneName != lastscenename)
        {
            lastscenename = sceneName;
            byteList.Add(1);
            byteList.Add(byte.Parse("" + sceneName.Length));
            byteList.AddRange(Encoding.UTF8.GetBytes(sceneName));
        }
        else
        {
            byteList.Add(0);
        }
        if (buttoninfo != "")
        {
            // Debug.Log(buttoninfo);
        }

        //Ends
        //byteList.InsertRange(0, BitConverter.GetBytes(byteList.Count));
        return byteList.ToArray();
    }
/*
    byte[] getnetworkstream(GameObject[] game)
    {
        List<byte> byteList = new List<byte>();
        foreach (GameObject go in game)
        {
            float[] prsArray = new float[] { go.transform.localPosition.x, go.transform.localPosition.y, go.transform.localPosition.z,
            go.transform.rotation.x, go.transform.rotation.y, go.transform.rotation.z, go.transform.rotation.w };

            foreach (float value in prsArray)
            {
                byteList.AddRange(BitConverter.GetBytes(value));
            }
        }
        return byteList.ToArray();
    }

    byte[] getbuttoninfo(string buttoninfo, string sceneName)
    {
      //  Debug.Log("BTN Info: " + buttoninfo);
        List<byte> byteList = new List<byte>();
        //For button presses
        if (buttoninfo != "")
        {
         //   Debug.Log(buttoninfo);
            byteList.Add(1); 
            byteList.Add(byte.Parse("" + buttoninfo.Length)); 
            for (int i = 0; i < buttoninfo.Length; i++)
            {
                byteList.Add(byte.Parse("" + buttoninfo[i])); //If one button is pressed, this will transfer 2 bytes instead of 8
            }
        }
        else
        {
            byteList.Add(0);
        }
        //For Scene change
        if (sceneName != lastscenename)
        {
            lastscenename = sceneName;
            byteList.Add(1);
            byteList.Add(byte.Parse("" + sceneName.Length));
            byteList.AddRange(Encoding.UTF8.GetBytes(sceneName));
        }
        else
        {
            byteList.Add(0);
        }
        if (buttoninfo != "")
        {
           // Debug.Log(buttoninfo);
        }

        return byteList.ToArray();
    }
*/

}
