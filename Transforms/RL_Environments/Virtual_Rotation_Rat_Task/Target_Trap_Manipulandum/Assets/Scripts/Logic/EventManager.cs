
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    public UnityEvent onStartClient;
    public UnityEvent onClientStarted;
    public UnityEvent onStopClient;
    public UnityEvent onClientStopped;
    public UnityEvent onNeedingNewObservation;

    [System.Serializable]
    public class StringEvent : UnityEvent<string> { }
    public class ByteArrayEvent : UnityEvent<byte[]> { }
    public class FloatEvent: UnityEvent<float>{ }

    public class ScreenResolutionEvent: UnityEvent<int, int> { }


    public StringEvent onUpdatedAction;
    public StringEvent onParametersChange;
    public ByteArrayEvent onObservationReady;
    public FloatEvent onNewMoveSnapReceived;
    public FloatEvent onNewRotateSnapReceived;
    public ScreenResolutionEvent onNewScreenResolution;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            onStartClient = new();
            onClientStarted = new();
            onStopClient = new();
            onClientStopped = new();
            onNeedingNewObservation = new();

            onUpdatedAction = new();
            onParametersChange = new();
            onObservationReady = new();

            onNewMoveSnapReceived = new();
            onNewRotateSnapReceived = new();
            onNewScreenResolution = new();
        }

        else
            Destroy(this);
    }
}
