using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Text;

public class MQTTManager : MonoBehaviour
{
    public static MQTTManager Instance;

    private TcpClient _tcp;
    private NetworkStream _stream;
    private bool _connected = false;

    private const string BROKER = "192.168.0.20";
    private const int PORT = 1883;
    private const string TOPIC = "hvac/heatpumptwin/iman2026";
    private const string CLIENT_ID = "UnityHVAC";

    void Awake() => Instance = this;
    void Start() => StartCoroutine(ConnectRoutine());

    IEnumerator ConnectRoutine()
    {
        yield return new WaitForSeconds(1f);

        System.Threading.Tasks.Task.Run(() =>
        {
            try
            {
                _tcp = new TcpClient();
                _tcp.Connect(BROKER, PORT);
                _stream = _tcp.GetStream();

                // MQTT CONNECT packet
                byte[] clientIdBytes = Encoding.UTF8.GetBytes(CLIENT_ID);
                byte[] packet = new byte[16 + clientIdBytes.Length];
                int i = 0;
                packet[i++] = 0x10; // CONNECT
                packet[i++] = (byte)(14 + clientIdBytes.Length);
                packet[i++] = 0x00; packet[i++] = 0x04;
                packet[i++] = (byte)'M'; packet[i++] = (byte)'Q';
                packet[i++] = (byte)'T'; packet[i++] = (byte)'T';
                packet[i++] = 0x04; // Protocol Level
                packet[i++] = 0x02; // Connect Flags
                packet[i++] = 0x00; packet[i++] = 0x3C; // Keep Alive
                packet[i++] = 0x00; packet[i++] = (byte)clientIdBytes.Length;
                foreach (var b in clientIdBytes) packet[i++] = b;

                _stream.Write(packet, 0, packet.Length);

                byte[] response = new byte[4];
                _stream.Read(response, 0, 4);

                if (response[3] == 0x00)
                    _connected = true;
            }
            catch (System.Exception e)
            {
                Debug.LogError("❌ " + e.Message);
            }
        });

        yield return new WaitForSeconds(2f);
        if (_connected) {
            Debug.Log("✅ MQTT Connected!");
            StartCoroutine(KeepAlive());
        }
        else
            Debug.LogError("❌ MQTT Failed!");
    }

    public void PublishPending() => StartCoroutine(PublishRoutine("pending"));
    public void PublishApproved() => StartCoroutine(PublishRoutine("approved"));
    public void PublishRejected() => StartCoroutine(PublishRoutine("rejected"));

    IEnumerator PublishRoutine(string message)
    {
        if (!_connected)
        {
            Debug.LogWarning("⚠️ Not connected! Reconnecting...");
            yield return StartCoroutine(ConnectRoutine());
        }

        System.Threading.Tasks.Task.Run(() =>
        {
            try
            {
                byte[] topicBytes = Encoding.UTF8.GetBytes(TOPIC);
                byte[] msgBytes = Encoding.UTF8.GetBytes(message);
                int remaining = 2 + topicBytes.Length + msgBytes.Length;

                byte[] packet = new byte[2 + remaining];
                int i = 0;
                packet[i++] = 0x30;
                packet[i++] = (byte)remaining;
                packet[i++] = (byte)(topicBytes.Length >> 8);
                packet[i++] = (byte)(topicBytes.Length & 0xFF);
                foreach (var b in topicBytes) packet[i++] = b;
                foreach (var b in msgBytes) packet[i++] = b;

                _stream.Write(packet, 0, packet.Length);
                _stream.Flush(); // اضافه شد
                Debug.Log("📤 Published: " + message);
            }
            catch (System.Exception e)
            {
                _connected = false;
                Debug.LogError("❌ Publish error: " + e.Message);
            }
        });

        yield return null;
    }
    IEnumerator KeepAlive()
    {
        while (true)
        {
            yield return new WaitForSeconds(20f);
            if (_connected && _stream != null)
            {
                System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        byte[] ping = { 0xC0, 0x00 };
                        _stream.Write(ping, 0, 2);
                    }
                    catch { _connected = false; }
                });
            }
        }
    }
    void OnDestroy()
    {
        _stream?.Close();
        _tcp?.Close();
    }
}