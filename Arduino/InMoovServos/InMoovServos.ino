// Settings
#define ARDUINO_SERIAL_STANDARD 1

#if defined(ARDUINO_ARCH_ESP32)
#define ESP32_BLUETOOTH_STANDARD 0
#define ESP32_BLUETOOTH_LE 0
#define ESP32_WIFI 0

#if defined(ESP32_BLUETOOTH_STANDARD) || defined(ESP32_BLUETOOTH_LE) || defined(ESP32_WIFI)
#undef ARDUINO_SERIAL_STANDARD
#endif // Check bluetooth / wifi
#endif // ESP32

#if ESP32_BLUETOOTH_STANDARD
#define BLUETOOTH_STD_NAME "InMoovSharpBT"
#endif
#if ESP32_BLUETOOTH_LE
#define BLUETOOTH_LE_NAME "InMoovSharpBT-LE"
#define SERVICE_UUID "4fafc201-1fb5-459e-8fcc-c5c9c331914b"
#define CHARACTERISTIC_UUID "beb5483e-36e1-4688-b7f5-ea07361b26a8"
#endif
#if ESP32_WIFI
#define WIFI_SSID "InMoovSharpWifi"
#define WIFI_PASSWORD "inmoovsharp"
#endif

// ESP32 World
#if defined(ARDUINO_ARCH_ESP32)
#include <ESP32Servo.h>
#if ESP32_BLUETOOTH_STANDARD
#include "BluetoothSerial.h"
BluetoothSerial SerialBT;
#elif ESP32_BLUETOOTH_LE
#include <BLEDevice.h>
#include <BLEUtils.h>
#include <BLEServer.h>
#include <BLE2902.h>
BLEServer* pServer = nullptr;
BLECharacteristic* pCharacteristic = nullptr;
#elif ESP32_WIFI
#include <WiFi.h>
#include <WiFiClient.h>
#include <WiFiServer.h>
WiFiServer server(80);
IPAddress local_IP(192, 168, 1, 1);
IPAddress gateway(192, 168, 1, 1);
IPAddress subnet(255, 255, 255, 0);
IPAddress primaryDNS(8, 8, 8, 8);
IPAddress secondaryDNS(8, 8, 4, 4);
#endif // ESP32_WIFI/Bluetooth
const int PinStart = 12;
const int PinEnd = 27;
const int BoardPinEnd = PinEnd;
const int DefaultBaudRate = 115200;
#else
// Arduino World
#include <Servo.h>
const int PinStart = 2;
const int PinEnd = 53;
#if defined(ARDUINO_AVR_MEGA2560)
const int BoardPinEnd = PinEnd;
#else
const int BoardPinEnd = 13;
#endif
const int DefaultBaudRate = 9600;
#endif

const int ServoCount = PinEnd - PinStart;
const int BufferLength = BoardPinEnd - PinStart;
const int ServoMin = 0;
const int ServoNeutral = 90;
const int ServoMax = 180;
const int MaxBufferSize = 63;

// The trame is
// Servo 0 (Pin2) => Value [0; 180], Disable if value is upper to ServoMax
// byteArray[index] => Value [0; 180]

// Servos
Servo servos[ServoCount];
// Values
int values[ServoCount];
// Activation
int servoActivation[ServoCount];
int lastServoActivation[ServoCount];

#if ESP32_BLUETOOTH_LE
class InMoovBLEServerCB : public BLEServerCallbacks {
  void onConnect(BLEServer* pServer) override {
    Serial.println("BLE Client Connected");
  }

  void onDisconnect(BLEServer* pServer) override {
    Serial.println("BLE Client Disconnected");
  }
};

class InMoovBLECharacteristicCallbacks : public BLECharacteristicCallbacks {
  void onWrite(BLECharacteristic* pCharacteristic) override {
    std::string value = pCharacteristic->getValue();

    if (value.length() > 0) {
      Serial.println("Received over BLE: ");
      if (value.length() != ServoCount) {
        Serial.println("Data size mismatch, ignoring.");
        return;
      }

      for (int i = 0; i < value.length(); i++) {
        values[i] = static_cast<unsigned char>(value[i]);
        servoActivation[i] = values[i] <= ServoMax ? 1 : 0;

        // Pour le débogage, afficher les valeurs reçues
        Serial.printf("Value for servo %d: %d\n", i, values[i]);
      }
    }
  }
};
#endif

void setup() {
  Serial.begin(DefaultBaudRate);

#if ESP32_BLUETOOTH_STANDARD
  SerialBT.begin(BLUETOOTH_STD_NAME);
  Serial.println("Bluetooth Device Ready");
#elif ESP32_BLUETOOTH_LE
  BLEDevice::init(BLUETOOTH_LE_NAME);
  pServer = BLEDevice::createServer();
  pServer->setCallbacks(new InMoovBLEServerCB());

  BLEService* pService = pServer->createService(SERVICE_UUID);
  pCharacteristic = pService->createCharacteristic(
    CHARACTERISTIC_UUID,
    BLECharacteristic::PROPERTY_READ | BLECharacteristic::PROPERTY_WRITE | BLECharacteristic::PROPERTY_NOTIFY);
  pCharacteristic->addDescriptor(new BLE2902());
  pCharacteristic->setCallbacks(new InMoovBLECharacteristicCallbacks());

  pService->start();
  BLEAdvertising* pAdvertising = BLEDevice::getAdvertising();
  pAdvertising->start();
  Serial.println("BLE Service Started");
#elif ESP32_WIFI
  WiFi.softAPConfig(local_IP, gateway, subnet);
  WiFi.softAP(WIFI_SSID, WIFI_PASSWORD);

  server.begin();
  Serial.print("Wifi server active at: ");
  Serial.println(WiFi.softAPIP());
#endif

#if defined(ARDUINO_ARCH_ESP32)
  for (int i = 0; i < 4 && i * 4 < ServoCount; i++) {
    ESP32PWM::allocateTimer(i);
  }
#endif

  for (int i = 0; i < ServoCount; i++) {
    values[i] = ServoNeutral;  // Neutral
    servoActivation[i] = 0;
    lastServoActivation[i] = 0;
#if defined(ARDUINO_ARCH_ESP32)
    servos[i].setPeriodHertz(50);
#endif
  }
}

#if ARDUINO_SERIAL_STANDARD
void FetchDataSerial() {
  // Read data from the Unity App, see the header for the trame
  int dataCount = Serial.available();

  if (dataCount > 0) {
    Serial.println(dataCount);
  }

  if (dataCount != ServoCount) {
    // Flush the buffer if full.e:\Unity\InMoovUnity\Arduino\InMoovServos\Servo.cpp
    if (dataCount == MaxBufferSize) {
      while (Serial.available()) {
        Serial.read();
      }
    }

    // Return while it doesn't have the expected size.
    return;
  }

  int i = 0;
  while (Serial.available() > 0) {
    values[i] = Serial.read();
    servoActivation[i] = values[i] <= ServoMax ? 1 : 0;
    i++;
  }
}
#endif

#if ESP32_BLUETOOTH_STANDARD
void FetchDataESPBluetooth() {
  // Read data from the Unity App, see the header for the trame
  int dataCount = SerialBT.available();

  if (dataCount > 0) {
    SerialBT.println(dataCount);
  }

  if (dataCount != ServoCount) {
    // Flush the buffer if full.e:\Unity\InMoovUnity\Arduino\InMoovServos\Servo.cpp
    if (dataCount == MaxBufferSize) {
      while (SerialBT.available()) {
        SerialBT.read();
      }
    }

    // Return while it doesn't have the expected size.
    return;
  }

  int i = 0;
  while (Serial.available() > 0) {
    values[i] = SerialBT.read();
    servoActivation[i] = values[i] <= ServoMax ? 1 : 0;
    i++;
  }
}
#endif

#if ESP32_WIFI
void FetchDataESPWifi() {
  WiFiClient client = server.available();
  if (!client) return;

  Serial.println("Client Connected.");

  while (client.connected()) {
    int dataCount = client.available();

    if (dataCount > 0) {
      Serial.println(dataCount);

      if (dataCount != ServoCount) {
        // Flush the buffer if full or incorrect size
        if (dataCount == MaxBufferSize) {
          while (client.available()) {
            client.read();
          }
        }
        // Return while it doesn't have the expected size
        return;
      }

      int i = 0;
      while (client.available() > 0 && i < ServoCount) {
        values[i] = client.read();
        servoActivation[i] = values[i] <= ServoMax ? 1 : 0;
        i++;
      }
    }

    client.stop();
    Serial.println("Client disconnected.");
  }
}
#endif

void loop() {
#if ARDUINO_SERIAL_STANDARD
  FetchDataSerial();
#endif

#if ESP32_BLUETOOTH_STANDARD
  FetchDataESPBluetooth();
#endif

#if ESP32_WIFI
  FetchDataESPWifi();
#endif

  // Apply values to the servos
  for (int i = 0; i < BufferLength; i++) {
    // Check if we need to enable or disable the servo
    if (servoActivation[i] != lastServoActivation[i]) {
      if (servoActivation[i] > 0) {
        servos[i].attach(i + PinStart);
      } else {
        servos[i].detach();
      }
      lastServoActivation[i] = servoActivation[i];
    }

    // Apply the value if enabled.
    if (servoActivation[i] > 0) {
      servos[i].write(values[i]);
    }
  }

  delay(10);  // Reduce loop cycling rate
}