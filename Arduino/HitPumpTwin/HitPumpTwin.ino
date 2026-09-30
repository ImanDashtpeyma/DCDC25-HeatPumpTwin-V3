#include <WiFiS3.h>
#include <ArduinoMqttClient.h>

const char* WIFI_SSID = "khoone";
const char* WIFI_PASS = "imansamira2019";

const char* BROKER = "test.mosquitto.org";
const int PORT = 1883;
const char* TOPIC = "hvac/heatpumptwin/iman2026";

const int PIN_RED = 12;
const int PIN_GREEN = 11;
const int PIN_BLUE = 10;
const int PIN_RELAY = 7;
const int PIN_BUZ = 9;

WiFiClient wifiClient;
MqttClient mqttClient(wifiClient);

void setColor(bool r, bool g, bool b) {
  digitalWrite(PIN_RED, r ? HIGH : LOW);
  digitalWrite(PIN_GREEN, g ? HIGH : LOW);
  digitalWrite(PIN_BLUE, b ? HIGH : LOW);
}

void buzzApproved() {
  tone(PIN_BUZ, 1000, 100);
  delay(200);
  tone(PIN_BUZ, 1000, 100);
  delay(200);
  noTone(PIN_BUZ);
}

void buzzPending() {
  tone(PIN_BUZ, 400, 600);
  delay(700);
  noTone(PIN_BUZ);
}

void buzzRejected() {
  tone(PIN_BUZ, 800, 120);
  delay(200);
  tone(PIN_BUZ, 500, 120);
  delay(200);
  tone(PIN_BUZ, 300, 120);
  delay(200);
  noTone(PIN_BUZ);
}

void setup() {
  Serial.begin(9600);
  pinMode(PIN_RED, OUTPUT);
  pinMode(PIN_GREEN, OUTPUT);
  pinMode(PIN_BLUE, OUTPUT);
  pinMode(PIN_RELAY, OUTPUT);
  pinMode(PIN_BUZ, OUTPUT);

  // فن خاموش تا approved بیاد
  digitalWrite(PIN_RELAY, HIGH);

  // پیش‌فرض — سبز
  setColor(false, true, false);

  WiFi.begin(WIFI_SSID, WIFI_PASS);
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("\n✅ WiFi!");

  while (!mqttClient.connect(BROKER, PORT)) {
    delay(1000);
    Serial.print(".");
  }
  Serial.println("✅ MQTT!");

  mqttClient.subscribe(TOPIC);
  Serial.println("📡 Subscribed: " + String(TOPIC));

  // صدای استارتاپ
  tone(PIN_BUZ, 600, 150);
  delay(200);
  tone(PIN_BUZ, 900, 150);
  delay(200);
  tone(PIN_BUZ, 1200, 300);
  delay(400);
  noTone(PIN_BUZ);
}

void loop() {
  mqttClient.poll();
  if (mqttClient.available()) {
    String msg = "";
    while (mqttClient.available()) msg += (char)mqttClient.read();
    Serial.println("📥 " + msg);

    if (msg == "selected") {
      tone(PIN_BUZ, 400, 600);
      delay(700);
      noTone(PIN_BUZ);
    } else if (msg == "suspended") {
      setColor(false, false, true);   // آبی
      digitalWrite(PIN_RELAY, LOW);   // فن خاموش
      buzzPending();
    } else if (msg == "approved") {
      setColor(false, true, false);   // سبز
      digitalWrite(PIN_RELAY, HIGH);  // فن روشن
      buzzApproved();
    } else if (msg == "rejected") {
      setColor(true, false, false);   // قرمز
      digitalWrite(PIN_RELAY, LOW);   // فن خاموش
      buzzRejected();
    }
  }
}