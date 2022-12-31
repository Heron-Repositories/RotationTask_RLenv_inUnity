
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections;
using UnityEngine;

namespace ReqRep
{
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
        private bool newObservationArray = false;

        private void Start()
        {
            _listener = new ReqRepListener(host, init_port, observation_data_port, HandleInitMessage, HandleResponseMessage);
            _listener.InitialisationRequestMessage();

            EventManager.Instance.onStartClient.AddListener(OnStartClient);
            EventManager.Instance.onClientStarted.AddListener(() => _clientStatus = ClientStatus.Active);
            EventManager.Instance.onStopClient.AddListener(OnStopClient);
            EventManager.Instance.onClientStopped.AddListener(() => _clientStatus = ClientStatus.Inactive);

            EventManager.Instance.onObservationReady.AddListener(SaveNewObservation);

            OnStartClient();
        }


        private void OnDestroy()
        {
            if (_clientStatus != ClientStatus.Inactive)
                OnStopClient();
        }


        private void HandleInitMessage(string message)
        {
            Debug.Log(message);
        }

        private void HandleResponseMessage(string message, ResponseSocket repSocket)
        {
            //Debug.Log("A. Receiving Request for new Observation");
            TimeSpan timeout = new(0, 0, 1);
            while (!newObservationArray)
            {
                //Debug.Log("B. Waiting for new Observation Array to be ready");
            }
            //Debug.Log("C. Sending Out the new Observation Array");
            repSocket.TrySendFrame(timeout, observationArray);
            //Debug.Log("D. New Observation Array Send");
            //Debug.Log("--------------------------------");
            newObservationArray = false;
        }

        private void SaveNewObservation(byte[] array)
        {
            //Debug.Log("7. Receining Observation Ready Message and Saving Observation Array");
            observationArray = array;
            newObservationArray = true;
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
    }
}