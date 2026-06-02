#include <WiFiS3.h>
#include <ArduinoMqttClient.h>

const char* WIFI_SSID = "khoone";
const char* WIFI_PASS = "imansamira2019";

const char* BROKER = "192.168.0.20";
const int PORT = 1883;
const char* TOPIC = "hvac/heatpumptwin/iman2026";

const int PIN_RED = 12;
const int PIN_GREEN = 13;

WiFiClient wifiClient;
MqttClient mqttClient(wifiClient);

void setup()
{
    Serial.begin(9600);
    pinMode(PIN_RED, OUTPUT);
    pinMode(PIN_GREEN, OUTPUT);

    // حالت پیش‌فرض — سبز (دستگاه داره کار می‌کنه)
    digitalWrite(PIN_RED, LOW);
    digitalWrite(PIN_GREEN, HIGH);

    WiFi.begin(WIFI_SSID, WIFI_PASS);
    while (WiFi.status() != WL_CONNECTED) { delay(500); Serial.print("."); }
    Serial.println("\n✅ WiFi!");

    while (!mqttClient.connect(BROKER, PORT)) { delay(1000); Serial.print("."); }
    Serial.println("✅ MQTT!");

    mqttClient.subscribe(TOPIC);
    Serial.println("📡 Subscribed: " + String(TOPIC));
}

void loop()
{
    mqttClient.poll();
    if (mqttClient.available())
    {
        String msg = "";
        while (mqttClient.available()) msg += (char)mqttClient.read();
        Serial.println("📥 " + msg);

        if (msg == "pending")
        {
            digitalWrite(PIN_RED, HIGH);
            digitalWrite(PIN_GREEN, LOW);
        }
        else if (msg == "approved")
        {
            digitalWrite(PIN_RED, LOW);
            digitalWrite(PIN_GREEN, HIGH);
        }
        else if (msg == "rejected")
        {
            digitalWrite(PIN_RED, LOW);
            digitalWrite(PIN_GREEN, HIGH);  // برمی‌گرده به سبز
        }
    }
}