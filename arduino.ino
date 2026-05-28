#include <Servo.h>

// Tweezer / foil input
const int tweezerPin = A3;

// FSR inputs
const int fsrPinP1 = A0;
const int fsrPinP2 = A1;

// Outputs
const int vibeMotorPin = 3;
const int servoPin = 9;

// FSR threshold
const int fsrTouchThreshold = 980;

// Servo countdown time
const unsigned long delayTime = 10000;

Servo gateServo;

bool timerActive = false;

bool lastFSRPressedP1 = false;
bool lastFSRPressedP2 = false;

unsigned long fsrTouchTime = 0;

void setup() {
  Serial.begin(115200);

  // INPUT_PULLUP means the pin reads HIGH normally
  // and LOW when connected to GND.
  pinMode(tweezerPin, INPUT_PULLUP);

  pinMode(fsrPinP1, INPUT_PULLUP);
  pinMode(fsrPinP2, INPUT_PULLUP);

  pinMode(vibeMotorPin, OUTPUT);

  gateServo.attach(servoPin);
  gateServo.write(0);
}

void loop() {
  // Read tweezers
  int tweezerState = digitalRead(tweezerPin);
  bool touchingFoil = (tweezerState == LOW);

  // Read FSRs
  int fsrValueP1 = analogRead(fsrPinP1);
  int fsrValueP2 = analogRead(fsrPinP2);

  bool currentFSRPressedP1 = (fsrValueP1 < fsrTouchThreshold);
  bool currentFSRPressedP2 = (fsrValueP2 < fsrTouchThreshold);

  // Vibration motor turns on when tweezers touch foil
  if (touchingFoil) {
    digitalWrite(vibeMotorPin, HIGH);
  } else {
    digitalWrite(vibeMotorPin, LOW);
  }

  // Start timer when either FSR is first pressed
  if ((currentFSRPressedP1 && !lastFSRPressedP1) ||
      (currentFSRPressedP2 && !lastFSRPressedP2)) {
    if (!timerActive) {
      timerActive = true;
      fsrTouchTime = millis();
    }
  }

  lastFSRPressedP1 = currentFSRPressedP1;
  lastFSRPressedP2 = currentFSRPressedP2;

  // Servo opens after countdown finishes
  if (timerActive && (millis() - fsrTouchTime >= delayTime)) {
    gateServo.write(90);
    delay(800);
    gateServo.write(0);
    delay(200);
    timerActive = false;
  }

  float secondsLeft = 0.0;

  if (timerActive) {
    secondsLeft = (float)(delayTime - (millis() - fsrTouchTime)) / 1000.0;
  }

  // Send to Unity
  // Format:
  // touchingFoil,timerActive,secondsLeft,p1FSRPressed,p1FSRValue,p2FSRPressed,p2FSRValue
  Serial.print(touchingFoil ? 1 : 0);
  Serial.print(",");
  Serial.print(timerActive ? 1 : 0);
  Serial.print(",");
  Serial.print(secondsLeft, 2);
  Serial.print(",");
  Serial.print(currentFSRPressedP1 ? 1 : 0);
  Serial.print(",");
  Serial.print(fsrValueP1);
  Serial.print(",");
  Serial.print(currentFSRPressedP2 ? 1 : 0);
  Serial.print(",");
  Serial.println(fsrValueP2);

  delay(16);
}
