// Motor driver module pins
const int motor1_enable_pin = 5;
const int motor1_direction_pin1 = 4;
const int motor1_direction_pin2 = 3;
const int motor2_enable_pin = 6;
const int motor2_direction_pin1 = 7;
const int motor2_direction_pin2 = 8;
const int motor3_enable_pin = 9;
const int motor3_direction_pin1 = 10;
const int motor3_direction_pin2 = 11;

// Motor speeds
int motor1_speed = 0;
int motor2_speed = 0;
int motor3_speed = 0;

void setup() {
  // Set the motor driver module pins as outputs
  pinMode(motor1_enable_pin, OUTPUT);
  pinMode(motor1_direction_pin1, OUTPUT);
  pinMode(motor1_direction_pin2, OUTPUT);
  pinMode(motor2_enable_pin, OUTPUT);
  pinMode(motor2_direction_pin1, OUTPUT);
  pinMode(motor2_direction_pin2, OUTPUT);
  pinMode(motor3_enable_pin, OUTPUT);
  pinMode(motor3_direction_pin1, OUTPUT);
  pinMode(motor3_direction_pin2, OUTPUT);

  // Set the motor enable pins to HIGH to turn on the motors
  digitalWrite(motor1_enable_pin, HIGH);
  digitalWrite(motor2_enable_pin, HIGH);
  digitalWrite(motor3_enable_pin, HIGH);
}

void loop() {
  // Read the motor speeds from some input source, such as a joystick or sensor
  motor1_speed = 255;
  motor2_speed = 128;
  motor3_speed = 64;

  // Set the motor directions based on the sign of the speed
  digitalWrite(motor1_direction_pin1, motor1_speed >= 0 ? HIGH : LOW);
  digitalWrite(motor1_direction_pin2, motor1_speed < 0 ? HIGH : LOW);
  digitalWrite(motor2_direction_pin1, motor2_speed >= 0 ? HIGH : LOW);
  digitalWrite(motor2_direction_pin2, motor2_speed < 0 ? HIGH : LOW);
  digitalWrite(motor3_direction_pin1, motor3_speed >= 0 ? HIGH : LOW);
  digitalWrite(motor3_direction_pin2, motor3_speed < 0 ? HIGH : LOW);

  // Set the motor speeds by writing to the enable pins with the absolute speed value
  analogWrite(motor1_enable_pin, abs(motor1_speed));
  analogWrite(motor2_enable_pin, abs(motor2_speed));
  analogWrite(motor3_enable_pin, abs(motor3_speed));
}
