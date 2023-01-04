
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class ReqRepClient : MonoBehaviour
{
    public enum ClientStatus
    {
        Inactive,
        Activating,
        Active,
        Deactivating
    }

    [SerializeField] private string host;
    [SerializeField] private string init_port;
    [SerializeField] private string observation_data_port;

    private ReqRepListener _listener;
    private ClientStatus _clientStatus = ClientStatus.Inactive;

    private byte[] observationPixelsArray;
    private bool newObservationArrayReady = false;
    private int reward = 0;
    private bool touchRewardPriority = false;
    private List<byte[]> observationFeaturesList;

    private List<string> comProtocol;

    private void Start()
    {
        _listener = new ReqRepListener(host, init_port, observation_data_port, HandleInitMessage, HandleResponseMessage);
        _listener.InitialisationRequestMessage();

        EventManager.Instance.onStartClient.AddListener(OnStartClient);
        EventManager.Instance.onClientStarted.AddListener(() => _clientStatus = ClientStatus.Active);
        EventManager.Instance.onStopClient.AddListener(OnStopClient);
        EventManager.Instance.onClientStopped.AddListener(() => _clientStatus = ClientStatus.Inactive);

        EventManager.Instance.onPixelsObservationReady.AddListener(SaveNewPixelsObservation);
        EventManager.Instance.onRewardStructureChange.AddListener(SaveNewReward);
        EventManager.Instance.onFeaturesObservationReady.AddListener(SaveNewFeaturesObservation);


        comProtocol = GameObject.Find("Rat").GetComponent<CommunicationProtocol>().observationsComProtocol;

        observationFeaturesList = new List<byte[]>() {BitConverter.GetBytes(0.0f)};

        OnStartClient();
    }


    private void OnDestroy()
    {
        if (_clientStatus != ClientStatus.Inactive)
            OnStopClient();
    }

    private void OnStartClient()
    {
        _clientStatus = ClientStatus.Activating;
        _listener.Start();
    }

    private void OnStopClient()
    {
        _clientStatus = ClientStatus.Deactivating;
        _listener.Stop();
    }

    private void HandleInitMessage(string message)
    {
        Debug.Log(message);
    }

    private void HandleResponseMessage(string message, ResponseSocket repSocket)
    {
        TimeSpan timeout = new(0, 0, 1);

        if (message == comProtocol[0]) // "Pixels"
            SendPixels(repSocket, timeout);

        if (message == comProtocol[1]) // "Features"
            SendFeatures(repSocket, timeout);

        if (message == comProtocol[2]) // "Everything"
        {
            SendPixels(repSocket, timeout);
            SendFeatures(repSocket, timeout);
        }

        SendReward(repSocket, timeout);

    }

    private void SendPixels(ResponseSocket repSocket, TimeSpan timeout)
    {
        while (!newObservationArrayReady) {}
        
        repSocket.TrySendFrame(timeout, observationPixelsArray, true);

        newObservationArrayReady = false;
    }

    private void SendFeatures(ResponseSocket repSocket, TimeSpan timeout)
    {
        foreach(byte[] feature in observationFeaturesList)
        {
            repSocket.TrySendFrame(timeout, feature, true);
        }
        
    }

    private void SendReward(ResponseSocket repSocket, TimeSpan timeout)
    {
        string stringReward = reward.ToString();
        repSocket.TrySendFrame(timeout, stringReward, false);
    }


    private void SaveNewPixelsObservation(byte[] array)
    {
        observationPixelsArray = array;
        newObservationArrayReady = true;
    }

    private void SaveNewReward(int new_reward)
    {
        if(!touchRewardPriority)
            reward = new_reward;
        touchRewardPriority = false;
    }

    // This is called from the Visual Scripting Graph in the VRGame Object,
    // by the Transition (Success State -> Running_Trial State), 
    // if there is a Reward Port touched (OnEnter) event.
    private void SaveNewRewardDueToPortTouched()
    {
        reward = RewardStructure.Instance.PokedAfterTarget;
        touchRewardPriority = true;
    }

    private void SaveNewFeaturesObservation(List<byte[]> features)
    {
        observationFeaturesList = features;
    }

}
        
