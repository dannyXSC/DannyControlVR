using System;
using UnityEngine;
using NetMQ;

[System.Serializable]
public class NetworkConfiguration
{
    public string IPAddress;
    public string rightkeyptPortNum;

    public string leftkeyptPortNum;
    public string camPortNum;
    public string graphPortNum;
    public string resolutionPortNum;

    public string PausePortNum;

    public string rightgripperPortNum;

    public string leftgripperPortNum;

    public string LeftPausePortNum;

    public string RightPausePortNum;

    public string LeftGripperRotatePortNum;

    public string RightGripperRotatePortNum;

    public string Test;
    

    public bool isIPAllocated()
    {
        if (String.Equals(IPAddress, "undefined"))
            return false;
        else
            return true;
    }
}

public class NetworkManager : MonoBehaviour
{
    // Loading the Network Configurations
    public NetworkConfiguration netConfig;

    // To indicate no IP
    private bool _ipNotFound;

    public bool NetworkAvailable()
    {
        return !_ipNotFound;
    }

    public string GetTestAddress()
    {
        return "tcp://" + netConfig.IPAddress + ":" + netConfig.Test;
    }
    
    public string GetRightKeypointPortNumAddress()
    {
        return "tcp://" + netConfig.IPAddress + ":" + netConfig.rightkeyptPortNum;
    }
    
    public string GetLeftKeypointPortNumAddress()
    {
        return "tcp://" + netConfig.IPAddress + ":" + netConfig.leftkeyptPortNum;
    }
    
    public string GetCamAddress()
    {
        return "tcp://" + netConfig.IPAddress + ":" + netConfig.camPortNum;
    }

    public void ChangeIPAddress(string ipAddress)
    {
        netConfig.IPAddress = ipAddress;
        _ipNotFound = false;
    }

    void Start()
    {
        var jsonFile = Resources.Load<TextAsset>("Configurations/Network");
        netConfig = JsonUtility.FromJson<NetworkConfiguration>(jsonFile.text);

        // init ip found flag
        _ipNotFound = true;

        // for test
        ChangeIPAddress("10.177.63.71");
    }

    void Update()
    {
        // _socket.SendFrame("Hello");
    }
}