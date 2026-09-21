using UnityEngine;
using M2MqttUnity;
using uPLibrary.Networking.M2Mqtt.Messages;

public class MQTTManager : M2MqttUnityClient
{
    public static MQTTManager Instance;

    private const string TOPIC = "hvac/heatpumptwin/iman2026";
    private const float RECONNECT_COOLDOWN = 5f;

    private bool _isConnecting;
    private float _lastConnectAttemptTime = -999f;
    private string _pendingPublish;

    protected override void Awake()
    {
        Instance = this;
        brokerAddress = "test.mosquitto.org";
        brokerPort = 1883;
        // client.Connect() used to run synchronously on Unity's main thread,
        // so whenever the broker was slow/unreachable it froze the whole
        // app — including Fusion's networking — for up to this timeout,
        // long enough to kill an active Fusion session. Now that
        // M2MqttUnityClient.DoConnect() runs the actual socket work on a
        // background thread (see that file), reconnecting from gameplay
        // code is safe again.
        timeoutOnConnection = 5000;
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void OnConnected()
    {
        base.OnConnected();
        _isConnecting = false;
        Debug.Log("✅ MQTT Connected!");

        if (_pendingPublish != null)
        {
            Publish(_pendingPublish);
            _pendingPublish = null;
        }
    }

    protected override void OnConnectionFailed(string errorMessage)
    {
        base.OnConnectionFailed(errorMessage);
        _isConnecting = false;
    }

    protected override void SubscribeTopics() { }
    protected override void UnsubscribeTopics() { }
    protected override void DecodeMessage(string topic, byte[] message) { }

    public void PublishSelected() => Publish("selected");
    public void PublishSuspended() => Publish("suspended");
    public void PublishApproved() => Publish("approved");
    public void PublishRejected() => Publish("rejected");

    private void Publish(string message)
    {
        if (client != null && client.IsConnected)
        {
            client.Publish(TOPIC,
                System.Text.Encoding.UTF8.GetBytes(message),
                MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE,
                false);
            Debug.Log("📤 Published: " + message);
        }
        else
        {
            Debug.LogWarning("⚠️ MQTT not connected — will publish once (re)connected.");
            _pendingPublish = message;
            TryReconnect();
        }
    }

    private void TryReconnect()
    {
        if (_isConnecting) return;
        if (Time.time - _lastConnectAttemptTime < RECONNECT_COOLDOWN) return;

        _isConnecting = true;
        _lastConnectAttemptTime = Time.time;
        Debug.Log("🔄 MQTT reconnecting in the background...");
        Connect();
    }
}
