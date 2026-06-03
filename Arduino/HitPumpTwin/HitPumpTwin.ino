#include <WiFiS3.h>
#include <ArduinoMqttClient.h>

const char* WIFI_SSID = "khoone";
const char* WIFI_PASS = "imansamira2019";

const char* BROKER = "test.mosquitto.org";
const int PORT = 1883;
const char* TOPIC = "hvac/heatpumptwin/iman2026";

const int PIN_RED   = 12;
const int PIN_GREEN = 11;
const int PIN_BLUE  = 10;

WiFiClient wifiClient;
MqttClient mqttClient(wifiClient);

void setColor(bool r, bool g, bool b) {
    digitalWrite(PIN_RED,   r ? HIGH : LOW);
    digitalWrite(PIN_GREEN, g ? HIGH : LOW);
    digitalWrite(PIN_BLUE,  b ? HIGH : LOW);
}

void setup() {
    Serial.begin(9600);
    pinMode(PIN_RED,   OUTPUT);
    pinMode(PIN_GREEN, OUTPUT);
    pinMode(PIN_BLUE,  OUTPUT);

    // پیش‌فرض — سبز
    setColor(false, true, false);

    WiFi.begin(WIFI_SSID, WIFI_PASS);
    while (WiFi.status() != WL_CONNECTED) { delay(500); Serial.print("."); }
    Serial.println("\n✅ WiFi!");

    while (!mqttClient.connect(BROKER, PORT)) { delay(1000); Serial.print("."); }
    Serial.println("✅ MQTT!");

    mqttClient.subscribe(TOPIC);
    Serial.println("📡 Subscribed: " + String(TOPIC));
}

void loop() {
    mqttClient.poll();
    if (mqttClient.available()) {
        String msg = "";
        while (mqttClient.available()) msg += (char)mqttClient.read();
        Serial.println("📥 " + msg);

        if (msg == "pending")
            setColor(true, false, false);    // قرمز
        else if (msg == "approved")
            setColor(false, true, false);    // سبز
        else if (msg == "rejected")
            setColor(false, false, true);    // آبی
    }
}