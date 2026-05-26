#include <Servo.h>

const int tweezerPin = A3;   
const int fsrPin = A0;       
const int vibeMotorPin = 3;  
const int servoPin = 9;      

const int fsrTouchThreshold = 980; 
const unsigned long delayTime = 10000; 

Servo gateServo;
bool timerActive = false;
bool lastFSRPressed = false;   
unsigned long fsrTouchTime = 0; 

void setup() {
  // Use 115200 baud rate for faster, low-latency communication with p5.js
  Serial.begin(115200); 
  
  pinMode(tweezerPin, INPUT_PULLUP);
  pinMode(vibeMotorPin, OUTPUT);
  pinMode(fsrPin, INPUT_PULLUP); 
  
  gateServo.attach(servoPin);
  gateServo.write(0); 
}

void loop() {
  int tweezerState = digitalRead(tweezerPin);
  int fsrValue = analogRead(fsrPin); 
  
  bool touchingFoil = (tweezerState == LOW);
  bool currentFSRPressed = (fsrValue < fsrTouchThreshold);
  
  // Handle instant haptics
  if (touchingFoil) {
    digitalWrite(vibeMotorPin, HIGH);
  } else {
    digitalWrite(vibeMotorPin, LOW);
  }

  // Handle Servo countdown logic
  if (currentFSRPressed && !lastFSRPressed) {
    if (!timerActive) { 
      timerActive = true;
      fsrTouchTime = millis(); 
    }
  }
  lastFSRPressed = currentFSRPressed;

  if (timerActive && (millis() - fsrTouchTime >= delayTime)) {
    gateServo.write(90);  
    delay(800);           
    gateServo.write(0);   
    delay(200);           
    timerActive = false; 
  }

  // Calculate remaining time to send to p5.js
  float secondsLeft = 0.0;
  if (timerActive) {
    secondsLeft = (float)(delayTime - (millis() - fsrTouchTime)) / 1000.0;
  }

  // SEND CLEAN DATA PACKET TO P5JS:
  // Format: touchingFoil,timerActive,secondsLeft
  Serial.print(touchingFoil ? 1 : 0);
  Serial.print(",");
  Serial.print(timerActive ? 1 : 0);
  Serial.print(",");
  Serial.println(secondsLeft, 2); // 2 decimal places

  delay(16); // Run at roughly 60Hz to match screen rendering
}
