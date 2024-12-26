using UnityEngine;
using UnityEngine.UI;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using TMPro;

public class ControlIndicator : MonoBehaviour
{
    public RawImage StreamBorder;
    public TMP_Text TaskText;
    // private RequestSocket state_socket;

    private bool connectionEstablished = false;
    private NetworkManager netConfig;
    private string stateAddress;
    private string stageAddress;
    private Thread infoCollecter;
    private OperationState state = OperationState.UNKNOWN;
    private string task_text_raw = "NONE";

    enum OperationState
    {
        STOP,
        RUNNING,
        READY,
        UNKNOWN
    }

    public void StartReceive()
    {
        // Check if communication address is available
        stateAddress = netConfig.GetSwitchAddress();
        bool AddressAvailable = !String.Equals(stateAddress, "tcp://:");
        stageAddress = netConfig.GetStageAddress();
        AddressAvailable = AddressAvailable && !String.Equals(stageAddress, "tcp://:");

        if (AddressAvailable)
        {
            infoCollecter = new Thread(getInfo);
            infoCollecter.Start();
            connectionEstablished = true;
        }
    }

    private void getInfo()
    {
        // get preparation stage
        using (var stage_socket = new RequestSocket())
        {
            stage_socket.Connect(stageAddress);
            while (true)
            {
                stage_socket.SendFrame("");
                string data = stage_socket.ReceiveFrameString();
                task_text_raw = "Stage: " + data;
                if (String.Equals(data, "3"))
                {
                    // last anchor collection process is complete
                    stage_socket.SendFrame("success");
                    break;
                }
            }
        }

        task_text_raw = "Running";

        // get running state
        using (var state_socket = new SubscriberSocket())
        {
            state_socket.Options.ReceiveHighWatermark = 1000;
            state_socket.Connect(stateAddress);
            state_socket.Subscribe("state");
            while (true)
            {
                string data = state_socket.ReceiveFrameString();
                if (data.Length >= 6)
                {
                    data = data.Substring(6, data.Length - 6);
                    if (String.Equals(data, "0"))
                    {
                        state = OperationState.STOP;
                    }
                    else if (String.Equals(data, "1"))
                    {
                        state = OperationState.RUNNING;
                    }else if (String.Equals(data, "2"))
                    {
                        state = OperationState.READY;
                    }
                }
            }
        }
    }

    void OnDisable()
    {
        infoCollecter?.Abort();
        NetMQConfig.Cleanup(false);
    }

    public void Start()
    {
        // Getting the Network Config Updater gameobject
        GameObject netConfGame = GameObject.Find("NetworkConfigsLoader");
        netConfig = netConfGame.GetComponent<NetworkManager>();
    }


    public void Update()
    {
        // StreamBorder.color = Color.green;
        if (connectionEstablished)
        {
            switch (state)
            {
                case OperationState.UNKNOWN:
                    StreamBorder.color = Color.black;
                    break;
                case OperationState.RUNNING:
                    StreamBorder.color = Color.green;
                    break;
                case OperationState.STOP:
                    StreamBorder.color = Color.red;
                    break;
                case OperationState.READY:
                    StreamBorder.color = Color.blue;
                    break;
            }

            TaskText.text = task_text_raw;
        }
        else
        {
            StartReceive();
        }
    }
}