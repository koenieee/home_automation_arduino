#include <IRremote.h>
#include <ServoTimer2.h>
#include <PololuLedStrip.h>



ServoTimer2 myservo;  // create servo object to control a servo 
// twelve servo objects can be created on most boards
int state = 0;

int state2 = 0;


IRsend irsend;

//###LEDSTRIP

PololuLedStrip<2> ledStrip;

#define LED_COUNT 59
rgb_color colors[LED_COUNT];

int numberOfChars = 0;
int numberOfColors = 0;
int mode = 0;

int ledOrCurtain = 0;


void setup() 
{ 

  //myservo.attach(9);  // attaches the servo on pin 9 to the servo object 
  Serial.begin(115200);

  Serial.println("Select-mode");
  pinMode(6,INPUT_PULLUP);
  pinMode(5,INPUT_PULLUP);
  while(mode == 0){

    if(Serial.available() == 1){

      char ch = Serial.read();
      // Serial.flush();
      switch(ch) {
      case 'l' : 
        ledOrCurtain = 1; 
        Serial.println("You selected led-control"); 
        break;
      case 'g' :  
        ledOrCurtain = 0; 
        Serial.println("You selected curtain"); 
        break;
      case 'r' :  
        ledOrCurtain = 2; 
        Serial.println("You selected analog-rgb"); 
        break;
      }

      mode = 1;
    }
    // Serial.println("5: " + digitalRead(5));
    //     Serial.println(digitalRead(6), DEC);
    if(digitalRead(5) == HIGH && digitalRead(6) == LOW){
      //ledOrCurtain = 3;
      //Serial.println("Waiting to go low");
      delay(1000);
      if(digitalRead(5) == LOW){
        mode = 1;
      }
    }

  }
  Serial.println("Going into loop");
} 

long parseHex (void) {
  long Value=0; 
  char C;
  delay(100);
  while (Serial.available()>0) {
    C= tolower(Serial.read());
    if ((C>='0')&&(C<='9'))
      C=C-'0';
    else
      if ((C>='a') && (C<='f'))
        C=C-'a'+10;
      else
        return Value;
    Value=  C+(Value<<4);
  };
  return Value;
}



void loop() 
{ //0 is voor naar boven, 180 is voor naar beneden
  //   press1 = digitalRead(button1);
  // else {
  //Serial.println("Ready");
  if(ledOrCurtain == 0){

    if ( Serial.available() == 1) {
      char xp = Serial.read();
      switch(xp) {
      case 'w':
        myservo.attach(3);
        myservo.write(1000);
        break;
      case 's':
        myservo.attach(3);
        myservo.write(1600);
        break;
      case 'd':
        // myservo.write(90);
        myservo.detach();
        break;
      }

    }
  }
  if(ledOrCurtain == 1){
    if (Serial.available() >= 3)
    {

      // Read the color from the computer.
      rgb_color color;

      color.green = Serial.read();
      Serial.flush();
      color.red = Serial.read();
      Serial.flush();
      color.blue = Serial.read();


      colors[numberOfColors] = color;
      numberOfColors++;

      if(numberOfColors == LED_COUNT){

        /* for (int i = 0; i < 4; i++){
         Serial.println(colors[i].green, DEC);
         Serial.println(colors[i].red, DEC);
         Serial.println(colors[i].blue, DEC);
         Serial.println();
         
         }
         Serial.println("=====================" );*/
        numberOfColors = 0;
        // Write to the LED strip.
        ledStrip.write(colors, LED_COUNT);  
      }

    } 

  }
  if(ledOrCurtain == 2){
    if (Serial.available() > 0) {
      for (int i = 0; i < 7; i++) {
        irsend.sendNEC(parseHex(), 32); // Sony TV power code
        delay(50);
      }
    }

  }
    if(digitalRead(5) == HIGH && state == 0){ // enable movement
      delay(300);
      if(digitalRead(6) == HIGH){ //going down
      Serial.println("Going down");
        myservo.attach(3);
        
       // 
        myservo.write(1600);
      }
      else { //going up
      Serial.println("Going up");
     // delay(500);
        myservo.attach(3);
       // 
        myservo.write(1000);
      }
      
      state = 1;


    } 
    else if(digitalRead(5) == LOW && state == 1){
      state = 0;
      delay(200);
      //myservo.write(1500);
     // delay(200);
      myservo.detach();
    }
 
} 














