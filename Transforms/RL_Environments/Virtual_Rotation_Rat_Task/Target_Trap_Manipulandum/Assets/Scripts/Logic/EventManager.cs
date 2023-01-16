
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This is a singleton that defines the events that are passed between the game's objects (not the ones defined and used within the Visual Scripting system if that is utilised)
/// </summary>
public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    public UnityEvent onStartClient;
    public UnityEvent onClientStarted;
    public UnityEvent onStopClient;
    public UnityEvent onClientStopped;
    public UnityEvent onNeedingNewObservation;
    public UnityEvent onLeftButtonPressed;
    public UnityEvent onRightButtonPressed;
    public UnityEvent onLeftButtonUnPressed;
    public UnityEvent onRightButtonUnPressed;
    public UnityEvent onRewardPortTouched;
    public UnityEvent onReseting;
    public UnityEvent onResetDone;

    [System.Serializable]
    public class StringEvent : UnityEvent<string> { }
    public class IntEvent : UnityEvent<int> { }
    public class ByteArrayEvent : UnityEvent<byte[]> { }
    public class FloatEvent: UnityEvent<float>{ }
    public class ListFOfByteArraysEvent: UnityEvent<List<byte[]>> { }
    public class ScreenResolutionEvent: UnityEvent<int, int> { }


    public StringEvent onUpdatedAction;
    public StringEvent onParametersChange;
    public IntEvent onRewardStructureChange;
    public ByteArrayEvent onPixelsObservationReady;
    public FloatEvent onNewMoveSnapReceived;
    public FloatEvent onNewRotateSnapReceived;
    public ScreenResolutionEvent onNewScreenResolution;
    public ListFOfByteArraysEvent onFeaturesObservationReady;


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
            onLeftButtonPressed = new();
            onRightButtonPressed = new();
            onLeftButtonUnPressed = new();
            onRightButtonUnPressed = new();
            onRewardPortTouched = new();
            onReseting = new();
            onResetDone = new();

            onUpdatedAction = new();
            onParametersChange = new();
            onPixelsObservationReady = new();
            onFeaturesObservationReady = new();

            onNewMoveSnapReceived = new();
            onNewRotateSnapReceived = new();
            onNewScreenResolution = new();

            onRewardStructureChange = new();
        }

        else
            Destroy(this);
    }
}
