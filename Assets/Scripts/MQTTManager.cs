using UnityEngine;
using M2MqttUnity;
using uPLibrary.Networking.M2Mqtt.Messages;

public class MQTTManager : M2MqttUnityClient
{
    public static MQTTManager Instance;

    private const string TOPIC = "hvac/heatpumptwin/iman2026";
    private string _pendingPublish = null;

    protected override void Awake()
    {
        Instance = this;
        brokerAddress = "test.mosquitto.org";
        brokerPort = 1883;
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
        Debug.Log("✅ MQTT Connected!");

        if (_pendingPublish != null)
        {
            Publish(_pendingPublish);
            _pendingPublish = null;
        }
    }

    protected override void SubscribeTopics() { }
    protected override void UnsubscribeTopics() { }
    protected override void DecodeMessage(string topic, byte[] message) { }

    public void PublishPending() => Publish("pending");
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
            Debug.LogWarning("⚠️ MQTT not connected, reconnecting...");
            _pendingPublish = message;
            Connect();
        }
    }
}