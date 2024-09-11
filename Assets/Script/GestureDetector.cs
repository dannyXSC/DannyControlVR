using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NetMQ;
using NetMQ.Sockets;
using Oculus.Interaction;
using UnityEngine.XR;

public class GestureDetector : MonoBehaviour
{
    // Hand Objects
    public OVRHand leftHand;
    public OVRHand rightHand;
    public OVRSkeleton leftHandSkeleton;
    public OVRSkeleton rightHandSkeleton;
    
    // public OVRPassthroughLayer PassthroughLayerManager;
    private List<OVRBone> _rightHandFingerBones;
    private List<OVRBone> _leftHandFingerBones;
    
    // Network enablers
    private bool _connectionEstablished;
    private NetworkManager _netConfig;
    private PushSocket _rightClient;
    
    
    private void InitializeBones()
    {
        if (_rightHandFingerBones == null || _rightHandFingerBones.Count != rightHandSkeleton.Bones.Count)
        {
            _rightHandFingerBones = new List<OVRBone>(rightHandSkeleton.Bones);
        }
    
        if (_leftHandFingerBones == null || _leftHandFingerBones.Count != leftHandSkeleton.Bones.Count)
        {
            _leftHandFingerBones = new List<OVRBone>(leftHandSkeleton.Bones);
        }
    }
    
    public List<Vector3> GetRightHandData()
    {
        List<Vector3> rightHandGestureData = new List<Vector3>();
    
        foreach (var bone in _rightHandFingerBones)
        {
            var bonePositionRight = bone.Transform.position;
            rightHandGestureData.Add(bonePositionRight);
        }
    
        return rightHandGestureData;
    }
    
    public List<Vector3> GetLeftHandData()
    {
        List<Vector3> leftHandGestureData = new List<Vector3>();
    
        foreach (var bone in _leftHandFingerBones)
        {
            var bonePositionLeft = bone.Transform.position;
            leftHandGestureData.Add(bonePositionLeft);
        }
    
        return leftHandGestureData;
    }
    
    public void CreateTcpConnection()
    {
        if (!_netConfig.NetworkAvailable())
        {
            _connectionEstablished = false;
            return;
        }
    
        if (_connectionEstablished)
        {
            return;
        }
    
        var right_address = _netConfig.GetRightKeypointPortNumAddress();
        // var address = _netConfig.GetTestAddress();
    
        _rightClient = new PushSocket();
        _rightClient.Connect(right_address);
        _connectionEstablished = true;
    }
    
    // Function to serialize the Vector3 List
    public static string SerializeVector3List(List<Vector3> gestureData)
    {
        string vectorString = "";
        foreach (Vector3 vec in gestureData)
            vectorString = vectorString + vec.x + "," + vec.y + "," + vec.z + "|";
        if (vectorString.Length > 0)
            vectorString = vectorString.Substring(0, vectorString.Length - 1) + ":";
    
        return vectorString;
    }
    
    private void Initialize()
    {
        InitializeBones();
        if (_rightHandFingerBones.Count > 0 && _leftHandFingerBones.Count > 0)
        {
            CreateTcpConnection();
        }
    }
    
    private bool IsInitialized()
    {
        bool boneStatus = _rightHandFingerBones.Count > 0 && _leftHandFingerBones.Count > 0;
        bool connectionStatus = _connectionEstablished;
    
        return boneStatus && connectionStatus;
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        // Getting the Network Config Updater gameobject
        GameObject netConfGameObject = GameObject.Find("NetworkConfigsLoader");
        _netConfig = netConfGameObject.GetComponent<NetworkManager>();
    
        // init
        Initialize();
    }
    
    void Debug()
    {
        DebugGizmos.Color = Color.red;
        DebugGizmos.DrawLine(new Vector3(0, 0, 0), new Vector3(0, 0, 1));
        DebugGizmos.Color = Color.green;
        DebugGizmos.DrawLine(new Vector3(0, 0, 0), new Vector3(0, 1, 0));
        DebugGizmos.Color = Color.blue;
        DebugGizmos.DrawLine(new Vector3(0, 0, 0), new Vector3(1, 0, 0));
    
        // DebugGizmos.Color = Color.cyan;
    
        // DebugGizmos.DrawPoint(rightHand.transform.position);
    
        if (IsInitialized())
        {
            var origin = _rightHandFingerBones[0].Transform.position;
            var p1 = _rightHandFingerBones[9].Transform.position;
            var p2 = _rightHandFingerBones[16].Transform.position;
            DebugGizmos.Color = Color.red;
            DebugGizmos.DrawLine(origin,p1);
            DebugGizmos.Color = Color.green;
            DebugGizmos.DrawLine(origin,p2);
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        // Debug();
    
    
        if (IsInitialized())
        {
            var right_data = GetRightHandData();
            string packedRightData = SerializeVector3List(right_data);
            _rightClient.SendFrame("absolute:" + packedRightData);
        }
        else
        {
            Initialize();
        }
    }
    
    private void OnDisable()
    {
        _rightClient?.Dispose();
        NetMQConfig.Cleanup(false);
    }
}