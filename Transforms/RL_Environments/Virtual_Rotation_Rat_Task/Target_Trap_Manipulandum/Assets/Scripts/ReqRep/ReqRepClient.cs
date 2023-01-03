
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections;
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

    private byte[] observationArray;
    private bool newObservationArrayReady = false;
    private int reward = 0;

    private void Start()
    {
        _listener = new ReqRepListener(host, init_port, observation_data_port, HandleInitMessage, HandleResponseMessage);
        _listener.InitialisationRequestMessage();

        EventManager.Instance.onStartClient.AddListener(OnStartClient);
        EventManager.Instance.onClientStarted.AddListener(() => _clientStatus = ClientStatus.Active);
        EventManager.Instance.onStopClient.AddListener(OnStopClient);
        EventManager.Instance.onClientStopped.AddListener(() => _clientStatus = ClientStatus.Inactive);

        EventManager.Instance.onObservationReady.AddListener(SaveNewObservation);
        EventManager.Instance.onRewardStructureChange.AddListener(SaveNewReward);
        EventManager.Instance.onRewardPortTouched.AddListener(SaveNewRewardDueToPortTouched);


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
        while (!newObservationArrayReady)
        {
            //Debug.Log("B. Waiting for new Observation Array to be ready");
        }

        TimeSpan timeout = new(0, 0, 1);
        repSocket.TrySendFrame(timeout, observationArray, true);
        string stringReward = reward.ToString();
        repSocket.TrySendFrame(timeout, stringReward, false);

        newObservationArrayReady = false;
    }

    private void SaveNewObservation(byte[] array)
    {
        observationArray = array;
        newObservationArrayReady = true;
    }

    private void SaveNewReward(RewardStructure _rew_struct)
    {
        reward = (int)_rew_struct;
    }

    private void SaveNewRewardDueToPortTouched()
    {
        reward = (int)RewardStructure.PokedAfterTarget;
    }

}
        
