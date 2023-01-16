using UnityEngine;


public class PubSubClient : MonoBehaviour
{
    public enum ClientStatus
    {
        Inactive,
        Activating,
        Active,
        Deactivating
    }
    
    [SerializeField] private string host;
    [SerializeField] private string port;
    private PubSubListener _listener;
    private ClientStatus _clientStatus = ClientStatus.Inactive;

    private void Start()
    {
        _listener = new PubSubListener(host, port, HandleMessage);

        EventManager.Instance.onStartClient.AddListener(OnStartClient);
        EventManager.Instance.onClientStarted.AddListener(() => _clientStatus = ClientStatus.Active);
        EventManager.Instance.onStopClient.AddListener(OnStopClient);
        EventManager.Instance.onClientStopped.AddListener(() => _clientStatus = ClientStatus.Inactive);

        OnStartClient();
    }

    private void Update()
    {
        if (_clientStatus == ClientStatus.Active)
        {
            _listener.DigestMessage();
        }
    }

    private void OnDestroy()
    {
        if (_clientStatus != ClientStatus.Inactive)
            OnStopClient();
    }

    // Functions that deal with the starting and ending of the PubSub client
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
    /// <c>HandleMessage</c> is called when the agent publishises that it has either done an action or needs a controllable parameter of the environment changed.
    /// </summary>
    private void HandleMessage(string message)
    {
        string message_data = message.Substring(message.IndexOf("=")).Substring(1);
        if (message.Contains("Action"))
        {
            EventManager.Instance.onUpdatedAction.Invoke(message_data);

        }

        if (message.Contains("Parameter"))
        {
            EventManager.Instance.onParametersChange.Invoke(message_data);
        }
    }

    
}
