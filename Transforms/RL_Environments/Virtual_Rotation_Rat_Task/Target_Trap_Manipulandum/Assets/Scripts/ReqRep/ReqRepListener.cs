using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;

public class ReqRepListener
{
    private readonly string _host;
    private readonly string _init_port;
    private readonly string _observation_data_port;
    private readonly Action<string> _initMessageCallback;
    private readonly Action<string, ResponseSocket> _repMessageCallback;

    private Thread _clientThread;
    private bool _clientCancelled;

    //private readonly ConcurrentQueue<string> _messageQueue = new ConcurrentQueue<string>();

    public ReqRepListener(string host, string init_port, string observation_data_port, Action<string> initMessageCallback, Action<string, ResponseSocket> repMessageCallback)
    {
        _host = host;
        _init_port = init_port;
        _observation_data_port = observation_data_port;
        _initMessageCallback = initMessageCallback;
        _repMessageCallback = repMessageCallback;

    }

    public void Start()
    {
        _clientCancelled = false;
        _clientThread = new Thread(ListenerWork);
        _clientThread.Start();
        EventManager.Instance.onClientStarted.Invoke();
    }

    public void Stop()
    {
        _clientCancelled = true;
        _clientThread?.Join();
        _clientThread = null;
        EventManager.Instance.onClientStopped.Invoke();
    }

    private void ListenerWork()
    {
        AsyncIO.ForceDotNet.Force();
        using (var repSocket = new ResponseSocket())
        {
            
            repSocket.Options.SendHighWatermark = 1;
            repSocket.Options.ReceiveHighWatermark = 1;
            repSocket.Connect($"tcp://{_host}:{_observation_data_port}");
            while (!_clientCancelled)
            {
                if (!repSocket.TryReceiveFrameString(Encoding.ASCII, out var message)) continue;
                {
                    //_messageQueue.Enqueue(message);
                    _repMessageCallback(message, repSocket);
                }
            }
            repSocket.Close();
        }
        NetMQConfig.Cleanup();
    }

    public void InitialisationRequestMessage()
    {
        var messageReceived = false;
        string message = "";
        AsyncIO.ForceDotNet.Force();

        var timeout = new TimeSpan(0, 0, 2);
        using (var socket = new RequestSocket())
        {
            socket.Options.SendHighWatermark = 1;
            socket.Options.ReceiveHighWatermark = 1;
            socket.Connect($"tcp://{_host}:{_init_port}");
            if (socket.TrySendFrame("Unity has started"))
            {
                messageReceived = socket.TryReceiveFrameString(timeout, out message);
            }
        }

        if (!messageReceived)
            message = "No message from Heron";
        _initMessageCallback(message);
    }


}
