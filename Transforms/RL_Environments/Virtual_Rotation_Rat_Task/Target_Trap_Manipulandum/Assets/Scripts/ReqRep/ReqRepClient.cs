
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


    // Functions that deal with starting and stopping the Request Reply Client
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

    /// <summary>
    /// <c>HandleInitMessage</c> is called when the agent sents a response as an initial handshake between the agent's process and the environment's (this game) process.
    /// </summary>
    private void HandleInitMessage(string message)
    {
        Debug.Log(message);
    }

    /// <summary>
    /// <c>HandleResponseMessage</c> is called when the agent sents a request for an observation to the environment.
    /// </summary>
    /// <param name="message">the message from the agent describing what observation it needs. The possible values should be defined in the CommunicationProtocol</param>
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

    // Functions that deal with the different requests by the agent (like camera observations, feature observations and rewards)

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
        if (reward == RewardStructure.Instance.RewPortPokedCorrectly)
        {
            reward = RewardStructure.Instance.NotMoved;
        }
    }

    // Functions that create the buffers for the different responses of the environment to the agent (camera obesrvations, features observations and reward)
    // These functions can change according to a particular environment requirements

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
        reward = RewardStructure.Instance.RewPortPokedCorrectly;
        touchRewardPriority = true;
        Debug.Log(reward);
    }

    private void SaveNewFeaturesObservation(List<byte[]> features)
    {
        observationFeaturesList = features;
    }

}
        
