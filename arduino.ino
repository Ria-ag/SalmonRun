#include <Servo.h>

// Tweezer / foil input
const int tweezerPin = A3;

// FSR inputs
const int fsrPinP1 = A0;
const int fsrPinP2 = A1;

// Outputs
const int vibeMotorPin = 3;
const int servoPin = 10;

// FSR threshold
const int fsrTouchThreshold = 980;

// Servo countdown time
const unsigned long delayTime = 10000;

// Servo angles
const int servoStartAngle = 90;
const int servoEndAngle = 0;

Servo gateServo;

bool timerActive = false;
bool servoAlreadyMoved = false;

bool lastFSRPressedP1 = false;
bool lastFSRPressedP2 = false;

unsigned long fsrTouchTime = 0;

void setup() {
  Serial.begin(115200);

  pinMode(tweezerPin, INPUT_PULLUP);

  pinMode(fsrPinP1, INPUT_PULLUP);
  pinMode(fsrPinP2, INPUT_PULLUP);

  pinMode(vibeMotorPin, OUTPUT);

  gateServo.attach(servoPin);

  // Start at 90 degrees
  gateServo.write(servoStartAngle);
}

void loop() {
  int tweezerState = digitalRead(tweezerPin);
  bool touchingFoil = (tweezerState == LOW);

  int fsrValueP1 = analogRead(fsrPinP1);
  int fsrValueP2 = analogRead(fsrPinP2);

  bool currentFSRPressedP1 = (fsrValueP1 < fsrTouchThreshold);
  bool currentFSRPressedP2 = (fsrValueP2 < fsrTouchThreshold);

  // Vibration motor turns on when tweezers touch foil.
  if (touchingFoil) {
    digitalWrite(vibeMotorPin, HIGH);
  } else {
    digitalWrite(vibeMotorPin, LOW);
  }

  // Start timer when either FSR is first pressed.
  // Only start if the servo has not already moved.
  if (!servoAlreadyMoved &&
      ((currentFSRPressedP1 && !lastFSRPressedP1) ||
       (currentFSRPressedP2 && !lastFSRPressedP2))) {
    if (!timerActive) {
      timerActive = true;
      fsrTouchTime = millis();
    }
  }

  lastFSRPressedP1 = currentFSRPressedP1;
  lastFSRPressedP2 = currentFSRPressedP2;

  // After countdown, rotate once from 90 to 0.
  if (timerActive && !servoAlreadyMoved && (millis() - fsrTouchTime >= delayTime)) {
    gateServo.write(servoEndAngle);
    servoAlreadyMoved = true;
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
