/*
 ������ָ�
 *ֹͣ/��λ��<ST>
 *����ֹͣ��<SI>
 *�򵥲�̬���
 * -----------------------------------------------------------
 *ת����<FW��-1>
 *���<BW��-1>
 *��ת��<LT��-1>
 *��ת��<RT��-1>
 *
 *��Ȥ�Ķ������
 * -----------------------------------------------------------
 *ҡͷ��<SX��-1>
 *������<BX��-1>
 *�ڶ���<WX��-1>
 *��ڶ���<WY��-1>
 *ҡ���ң�<WZ��-1>
 *����ţ�<TX��-1>
 *�����ţ�<TY��-1>
 *����ҽţ�<TZ��-1>
 *ҡ���ȣ�<LX��-1>
 *ҡ���ȣ�<LY��-1>
 *ҡ�����ȣ�<LZ��-1>
 *------------------------------------------------------------
*/
#include <Servo.h>
#include <SoftwareSerial.h>

#define SERIAL_SPEED 9600  //��ʱ�������ó�115200

//������������
const int SERVO_LEFT_HIP   = 2;
const int SERVO_LEFT_FOOT  = 3;
const int SERVO_RIGHT_HIP  = 4;
const int SERVO_RIGHT_FOOT = 5;

//��ʱΪ��ǰ��
#define FRONT_JOINT_HIPS 1

//Ϊ����������м�ֵ
const int LEFT_HIP_CENTRE = 1480;
const int LEFT_HIP_MIN    = LEFT_HIP_CENTRE - 500;
const int LEFT_HIP_MAX    = LEFT_HIP_CENTRE + 500;

const int LEFT_FOOT_CENTRE = 1360;
const int LEFT_FOOT_MIN    = LEFT_FOOT_CENTRE - 500;
const int LEFT_FOOT_MAX    = LEFT_FOOT_CENTRE + 500;

const int RIGHT_HIP_CENTRE = 1586;
const int RIGHT_HIP_MIN    = RIGHT_HIP_CENTRE - 500;
const int RIGHT_HIP_MAX    = RIGHT_HIP_CENTRE + 500;

const int RIGHT_FOOT_CENTRE = 1560;
const int RIGHT_FOOT_MIN    = RIGHT_FOOT_CENTRE - 500;
const int RIGHT_FOOT_MAX    = RIGHT_FOOT_CENTRE + 500;

int LeftHipCentre()              { return LEFT_HIP_CENTRE; }
int LeftHipIn(int millisecs)     { return LEFT_HIP_CENTRE + (FRONT_JOINT_HIPS * millisecs); }
int LeftHipOut(int millisecs)    { return LEFT_HIP_CENTRE - (FRONT_JOINT_HIPS * millisecs); }

int RightHipCentre()             { return RIGHT_HIP_CENTRE; }
int RightHipIn(int millisecs)    { return RIGHT_HIP_CENTRE - (FRONT_JOINT_HIPS * millisecs); }
int RightHipOut(int millisecs)   { return RIGHT_HIP_CENTRE + (FRONT_JOINT_HIPS * millisecs); }

int LeftFootFlat()               { return LEFT_FOOT_CENTRE; }
int LeftFootUp(int millisecs)    { return LEFT_FOOT_CENTRE - millisecs; }
int LeftFootDown(int millisecs)  { return LEFT_FOOT_CENTRE + millisecs; }

int RightFootFlat()              { return RIGHT_FOOT_CENTRE; }
int RightFootUp(int millisecs)   { return RIGHT_FOOT_CENTRE + millisecs; }
int RightFootDown(int millisecs) { return RIGHT_FOOT_CENTRE - millisecs; }


const int TWEEN_TIME_VALUE = 0;
const int LEFT_HIP_VALUE   = 1;
const int LEFT_FOOT_VALUE  = 2;
const int RIGHT_HIP_VALUE  = 3;
const int RIGHT_FOOT_VALUE = 4;


const int FOOT_DELTA = 150;
const int HIP_DELTA  = FRONT_JOINT_HIPS * 120;


//�����������ʵ�ֲ�̬�ģ���һ���Ǳ�ʾ����������Ĳ�̬���������еĵ�һ��Ԫ��-
//-��ʾ������������ʱ��

int standStraightAnim[][5] = {
    
    { 1, 0, 0, 0, 0 },
    
    { 300, LeftHipCentre(), LeftFootFlat(), RightHipCentre(), RightFootFlat() }
};

//ǰ��
int walkForwardAnim[][5] = {
    
    { 8, 0, 0, 0, 0 },
    
    { 300, LeftHipCentre(), LeftFootUp(FOOT_DELTA), RightHipCentre(), RightFootDown(FOOT_DELTA) },
    
    { 300, LeftHipIn(HIP_DELTA), LeftFootUp(FOOT_DELTA), RightHipOut(HIP_DELTA), RightFootDown(FOOT_DELTA) },
    
    { 300, LeftHipIn(HIP_DELTA), LeftFootFlat(), RightHipOut(HIP_DELTA), RightFootFlat() },
    
    { 300, LeftHipIn(HIP_DELTA), LeftFootDown(FOOT_DELTA), RightHipOut(HIP_DELTA), RightFootUp(FOOT_DELTA) },
    
    { 300, LeftHipCentre(), LeftFootDown(FOOT_DELTA), RightHipCentre(), RightFootUp(FOOT_DELTA) },
    
    { 300, LeftHipOut(HIP_DELTA), LeftFootDown(FOOT_DELTA), RightHipIn(HIP_DELTA), RightFootUp(FOOT_DELTA) },
    
    { 300, LeftHipOut(HIP_DELTA), LeftFootFlat(), RightHipIn(HIP_DELTA), RightFootFlat() },
    
    { 300, LeftHipOut(HIP_DELTA), LeftFootUp(FOOT_DELTA), RightHipIn(HIP_DELTA), RightFootDown(FOOT_DELTA) }
};


//����
int walkBackwardAnim[][5] = {
    
    { 8, 0, 0, 0, 0 },

    { 300, LeftHipCentre(), LeftFootUp(FOOT_DELTA), RightHipCentre(), RightFootDown(FOOT_DELTA) },
    
    { 300, LeftHipOut(HIP_DELTA), LeftFootUp(FOOT_DELTA), RightHipIn(HIP_DELTA), RightFootDown(FOOT_DELTA) },

    { 300, LeftHipOut(HIP_DELTA), LeftFootFlat(), RightHipIn(HIP_DELTA), RightFootFlat() },
        
    { 300, LeftHipOut(HIP_DELTA), LeftFootDown(FOOT_DELTA), RightHipIn(HIP_DELTA), RightFootUp(FOOT_DELTA) },
    
    { 300, LeftHipCentre(), LeftFootDown(FOOT_DELTA), RightHipCentre(), RightFootUp(FOOT_DELTA) },
    
    { 300, LeftHipIn(HIP_DELTA), LeftFootDown(FOOT_DELTA), RightHipOut(HIP_DELTA), RightFootUp(FOOT_DELTA) },
    
    { 300, LeftHipIn(HIP_DELTA), LeftFootFlat(), RightHipOut(HIP_DELTA), RightFootFlat() },
    
    { 300, LeftHipIn(HIP_DELTA), LeftFootUp(FOOT_DELTA), RightHipOut(HIP_DELTA), RightFootDown(FOOT_DELTA) }
};


int walkEndAnim[][5] = {
    { 2, 0, 0, 0, 0 },
    
    { 300, LeftHipCentre(), LeftFootUp(FOOT_DELTA), RightHipCentre(), RightFootDown(FOOT_DELTA) },

    { 300, LeftHipCentre(), LeftFootFlat(), RightHipCentre(), RightFootFlat() }
};

//����
int turnLeftAnim[][5] = {
    { 6, 0, 0, 0, 0 },
    
    { 300, LeftHipCentre(), LeftFootUp(FOOT_DELTA), RightHipCentre(), RightFootDown(FOOT_DELTA) },
    
    { 300, LeftHipIn(HIP_DELTA), LeftFootUp(FOOT_DELTA), RightHipIn(HIP_DELTA), RightFootDown(FOOT_DELTA) },

    { 300, LeftHipIn(HIP_DELTA), LeftFootFlat(), RightHipIn(HIP_DELTA), RightFootFlat() },

    { 300, LeftHipIn(HIP_DELTA), LeftFootDown(FOOT_DELTA), RightHipIn(HIP_DELTA), RightFootUp(FOOT_DELTA) },

    { 300, LeftHipCentre(), LeftFootDown(FOOT_DELTA), RightHipCentre(), RightFootUp(FOOT_DELTA) },

    { 300, LeftHipCentre(), LeftFootFlat(), RightHipCentre(), RightFootFlat() }
};

//����
int turnRightAnim[][5] = {
    { 6, 0, 0, 0, 0 },
    
    { 300, LeftHipCentre(), LeftFootDown(FOOT_DELTA), RightHipCentre(), RightFootUp(FOOT_DELTA) },

    { 300, LeftHipIn(HIP_DELTA), LeftFootDown(FOOT_DELTA), RightHipIn(HIP_DELTA), RightFootUp(FOOT_DELTA) },

    { 300, LeftHipIn(HIP_DELTA), LeftFootFlat(), RightHipIn(HIP_DELTA), RightFootFlat() },

    { 300, LeftHipIn(HIP_DELTA), LeftFootUp(FOOT_DELTA), RightHipIn(HIP_DELTA), RightFootDown(FOOT_DELTA) },

    { 300, LeftHipCentre(), LeftFootUp(FOOT_DELTA), RightHipCentre(), RightFootDown(FOOT_DELTA) },

    { 300, LeftHipCentre(), LeftFootFlat(), RightHipCentre(), RightFootFlat() }
};

//�ζ�
int shakeHeadAnim[][5] = {

    { 4, 0, 0, 0, 0 },
    
    { 150, LeftHipOut(HIP_DELTA), LeftFootFlat(), RightHipIn(HIP_DELTA), RightFootFlat() },
    
    { 150, LeftHipCentre(), LeftFootFlat(), RightHipCentre(), RightFootFlat() },

    { 150, LeftHipIn(HIP_DELTA), LeftFootFlat(), RightHipOut(HIP_DELTA), RightFootFlat() },

    { 150, LeftHipCentre(), LeftFootFlat(), RightHipCentre(), RightFootFlat() }    
};



int wobbleAnim[][5] = {

    { 4, 0, 0, 0, 0 },

    { 300, LeftHipCentre(), LeftFootUp(FOOT_DELTA), RightHipCentre(), RightFootDown(FOOT_DELTA) },

    { 300, LeftHipCentre(), LeftFootFlat(), RightHipCentre(), RightFootFlat() },

    { 300, LeftHipCentre(), LeftFootDown(FOOT_DELTA), RightHipCentre(), RightFootUp(FOOT_DELTA) },

    { 300, LeftHipCentre(), LeftFootFlat(), RightHipCentre(), RightFootFlat() }    
};


int wobbleLeftAnim[][5] = {

    { 2, 0, 0, 0, 0 },

    { 300, LeftHipCentre(), LeftFootUp(FOOT_DELTA), RightHipCentre(), RightFootDown(FOOT_DELTA) },

    { 300, LeftHipCentre(), LeftFootFlat(), RightHipCentre(), RightFootFlat() },
};



int wobbleRightAnim[][5] = {
    { 2, 0, 0, 0, 0 },
    
    { 300, LeftHipCentre(), LeftFootDown(FOOT_DELTA), RightHipCentre(), RightFootUp(FOOT_DELTA) },
 
    { 300, LeftHipCentre(), LeftFootFlat(), RightHipCentre(), RightFootFlat() }    
};


int tapFeetAnim[][5] = {

    { 2, 0, 0, 0, 0 },

    { 500, LeftHipCentre(), LeftFootUp(FOOT_DELTA), RightHipCentre(), RightFootUp(FOOT_DELTA) },

    { 500, LeftHipCentre(), LeftFootFlat(), RightHipCentre(), RightFootFlat() },
};



int tapLeftFootAnim[][5] = {

    { 2, 0, 0, 0, 0 },

    { 500, LeftHipCentre(), LeftFootUp(FOOT_DELTA), RightHipCentre(), RightFootFlat() },

    { 500, LeftHipCentre(), LeftFootFlat(), RightHipCentre(), RightFootFlat() },
};



int tapRightFootAnim[][5] = {

    { 2, 0, 0, 0, 0 },

    { 500, LeftHipCentre(), LeftFootFlat(), RightHipCentre(), RightFootUp(FOOT_DELTA) },

    { 500, LeftHipCentre(), LeftFootFlat(), RightHipCentre(), RightFootFlat() },
};


int bounceAnim[][5] = {
    { 2, 0, 0, 0, 0 },

    { 500, LeftHipCentre(), LeftFootDown(300), RightHipCentre(), RightFootDown(300) },

    { 500, LeftHipCentre(), LeftFootFlat(), RightHipCentre(), RightFootFlat() },
};


int shakeLegsAnim[][5] = {
    { 14, 0, 0, 0, 0 },
    
    { 300, LeftHipCentre(), LeftFootUp(FOOT_DELTA), RightHipCentre(), RightFootDown(FOOT_DELTA) },

    { 100, LeftHipCentre(), LeftFootUp(FOOT_DELTA), RightHipIn(HIP_DELTA), RightFootDown(FOOT_DELTA) },

    { 100, LeftHipCentre(), LeftFootUp(FOOT_DELTA), RightHipCentre(), RightFootDown(FOOT_DELTA) },

    { 100, LeftHipCentre(), LeftFootUp(FOOT_DELTA), RightHipOut(HIP_DELTA), RightFootDown(FOOT_DELTA) },

    { 100, LeftHipCentre(), LeftFootUp(FOOT_DELTA), RightHipCentre(), RightFootDown(FOOT_DELTA) },

    { 100, LeftHipCentre(), LeftFootUp(FOOT_DELTA), RightHipIn(HIP_DELTA), RightFootDown(FOOT_DELTA) },
  
    { 100, LeftHipCentre(), LeftFootUp(FOOT_DELTA), RightHipCentre(), RightFootDown(FOOT_DELTA) },

    { 300, LeftHipCentre(), LeftFootFlat(), RightHipCentre(), RightFootFlat() },
  
    { 300, LeftHipCentre(), LeftFootDown(FOOT_DELTA), RightHipCentre(), RightFootUp(FOOT_DELTA) },
  
    { 100, LeftHipIn(HIP_DELTA), LeftFootDown(FOOT_DELTA), RightHipCentre(), RightFootUp(FOOT_DELTA) },
  
    { 100, LeftHipCentre(), LeftFootDown(FOOT_DELTA), RightHipCentre(), RightFootUp(FOOT_DELTA) },

    { 100, LeftHipOut(HIP_DELTA), LeftFootDown(FOOT_DELTA), RightHipCentre(), RightFootUp(FOOT_DELTA) },

    { 100, LeftHipCentre(), LeftFootDown(FOOT_DELTA), RightHipCentre(), RightFootUp(FOOT_DELTA) },

    { 300, LeftHipCentre(), LeftFootFlat(), RightHipCentre(), RightFootFlat() }    
};



int shakeLeftLegAnim[][5] = {
   
    { 12, 0, 0, 0, 0 },

    { 300, LeftHipCentre(), LeftFootDown(FOOT_DELTA), RightHipCentre(), RightFootUp(FOOT_DELTA) },

    { 100, LeftHipIn(HIP_DELTA), LeftFootDown(FOOT_DELTA), RightHipCentre(), RightFootUp(FOOT_DELTA) },

    { 100, LeftHipCentre(), LeftFootDown(FOOT_DELTA), RightHipCentre(), RightFootUp(FOOT_DELTA) },

    { 100, LeftHipOut(HIP_DELTA), LeftFootDown(FOOT_DELTA), RightHipCentre(), RightFootUp(FOOT_DELTA) },
    
    { 100, LeftHipCentre(), LeftFootDown(FOOT_DELTA), RightHipCentre(), RightFootUp(FOOT_DELTA) },

    { 100, LeftHipIn(HIP_DELTA), LeftFootDown(FOOT_DELTA), RightHipCentre(), RightFootUp(FOOT_DELTA) },
    
    { 100, LeftHipCentre(), LeftFootDown(FOOT_DELTA), RightHipCentre(), RightFootUp(FOOT_DELTA) },
 
    { 100, LeftHipOut(HIP_DELTA), LeftFootDown(FOOT_DELTA), RightHipCentre(), RightFootUp(FOOT_DELTA) },

    { 100, LeftHipCentre(), LeftFootDown(FOOT_DELTA), RightHipCentre(), RightFootUp(FOOT_DELTA) },

    { 100, LeftHipIn(HIP_DELTA), LeftFootDown(FOOT_DELTA), RightHipCentre(), RightFootUp(FOOT_DELTA) },
 
    { 100, LeftHipCentre(), LeftFootDown(FOOT_DELTA), RightHipCentre(), RightFootUp(FOOT_DELTA) },

    { 300, LeftHipCentre(), LeftFootFlat(), RightHipCentre(), RightFootFlat() }    
};


int shakeRightLegAnim[][5] = {

    { 12, 0, 0, 0, 0 },
    
    { 300, LeftHipCentre(), LeftFootUp(FOOT_DELTA), RightHipCentre(), RightFootDown(FOOT_DELTA) },

    { 100, LeftHipCentre(), LeftFootUp(FOOT_DELTA), RightHipIn(HIP_DELTA), RightFootDown(FOOT_DELTA) },
    
    { 100, LeftHipCentre(), LeftFootUp(FOOT_DELTA), RightHipCentre(), RightFootDown(FOOT_DELTA) },
    
    { 100, LeftHipCentre(), LeftFootUp(FOOT_DELTA), RightHipOut(HIP_DELTA), RightFootDown(FOOT_DELTA) },

    { 100, LeftHipCentre(), LeftFootUp(FOOT_DELTA), RightHipCentre(), RightFootDown(FOOT_DELTA) },

    { 100, LeftHipCentre(), LeftFootUp(FOOT_DELTA), RightHipIn(HIP_DELTA), RightFootDown(FOOT_DELTA) },

    { 100, LeftHipCentre(), LeftFootUp(FOOT_DELTA), RightHipCentre(), RightFootDown(FOOT_DELTA) },
    
    { 100, LeftHipCentre(), LeftFootUp(FOOT_DELTA), RightHipOut(HIP_DELTA), RightFootDown(FOOT_DELTA) },

    { 100, LeftHipCentre(), LeftFootUp(FOOT_DELTA), RightHipCentre(), RightFootDown(FOOT_DELTA) },

    { 100, LeftHipCentre(), LeftFootUp(FOOT_DELTA), RightHipIn(HIP_DELTA), RightFootDown(FOOT_DELTA) },

    { 100, LeftHipCentre(), LeftFootUp(FOOT_DELTA), RightHipCentre(), RightFootDown(FOOT_DELTA) },

    { 300, LeftHipCentre(), LeftFootFlat(), RightHipCentre(), RightFootFlat() },    
};


int setServosAnim1[][5] = {
    { 1, 0, 0, 0, 0 },

    { 0, LeftHipCentre(), LeftFootFlat(), RightHipCentre(), RightFootFlat() }
};

int setServosAnim2[][5] = {
    { 1, 0, 0, 0, 0 },
    
    { 0, LeftHipCentre(), LeftFootFlat(), RightHipCentre(), RightFootFlat() }
};

//������
Servo servoLeftHip;
Servo servoLeftFoot;
Servo servoRightHip;
Servo servoRightFoot;

const int millisBetweenAnimUpdate = 20;

long timeAtLastAnimUpdate;

int  (*currAnim)[5];      
int  (*finishAnim)[5];    
long timeAtStartOfFrame; 
int  targetFrame;        
int  animNumLoops;     
char animCompleteStr[3] = "--"; 

bool animInProgress;    

int  (*nextAnim)[5];     
                          
int  (*nextFinishAnim)[5]; 

int  nextAnimNumLoops;  

char nextAnimCompleteStr[3] = "--"; 

bool interruptInProgressAnim; 


int  currLeftHip;
int  currLeftFoot;
int  currRightHip;
int  currRightFoot;

int  startLeftHip;
int  startLeftFoot;
int  startRightHip;
int  startRightFoot;

const char START_CHAR = '<';
const char END_CHAR   = '>';
const char SEP_CHAR   = ',';

const int PARSER_WAITING = 0; 
const int PARSER_COMMAND = 1; 
const int PARSER_PARAM1  = 2;
const int PARSER_PARAM2  = 3; 
const int PARSER_PARAM3  = 4; 
const int PARSER_PARAM4  = 5;
const int PARSER_PARAM5  = 6; 
const int PARSER_EXECUTE = 7; 
int currParserState = PARSER_WAITING; 
char currCmd[3] = "--";
int currCmdIndex;
const int CMD_LENGTH = 2;

int currParam1Val;
int currParam2Val;
int currParam3Val;
int currParam4Val;
int currParam5Val;
int currParamIndex;
boolean currParamNegative;
const int MAX_PARAM_LENGTH = 6;


void setup() 
{
    Serial.begin(9600);

    servoLeftHip.attach(  SERVO_LEFT_HIP,   LEFT_HIP_MIN,   LEFT_HIP_MAX);
    servoLeftFoot.attach( SERVO_LEFT_FOOT,  LEFT_FOOT_MIN,  LEFT_FOOT_MAX);
    servoRightHip.attach( SERVO_RIGHT_HIP,  RIGHT_HIP_MIN,  RIGHT_HIP_MAX);
    servoRightFoot.attach(SERVO_RIGHT_FOOT, RIGHT_FOOT_MIN, RIGHT_FOOT_MAX);

    setup_Parser();//��ʼ������
    setup_Animation();
}

void loop() 
{
    loop_Parser();//��ȡ�������������������ֵ
    
    loop_Animation();//����ֵ������Ӧ�Ķ���
}



void setup_Parser()
{
    currParserState = PARSER_WAITING;
    Serial.println("<OK>");
}



void loop_Parser()
{
    
    if (Serial.available() > 0)
    {

        if (currParserState == PARSER_WAITING)
        {
            if (c == START_CHAR)
            {
                currParserState = PARSER_COMMAND;
                currCmdIndex = 0;
                currCmd[0] = '-';
                currCmd[1] = '-';
                currParam1Val = 0;
                currParam2Val = 0;
                currParam3Val = 0;
                currParam4Val = 0;
                currParam5Val = 0;
            }
        }
    

        else if (currParserState == PARSER_COMMAND)
        {
        
            if (c == SEP_CHAR)
            {
                if (currCmdIndex == CMD_LENGTH)
                {
                    currParserState = PARSER_PARAM1;
                    currParamIndex = 0;
                    currParamNegative = false;
                }
                else
                {
                    currParserState = PARSER_WAITING;
                }
            }

            else if (c == END_CHAR)
            {
                if (currCmdIndex == CMD_LENGTH)
                {
                    currParserState = PARSER_EXECUTE;
                }
                else
                {
                    currParserState = PARSER_WAITING;
                }
            }
    
            else if ( (currCmdIndex >= CMD_LENGTH) || (c < 'A') || (c > 'Z') )
            {
                currParserState = PARSER_WAITING;
            }
      
            else
            {
                currCmd[currCmdIndex] = c;
                currCmdIndex++;
            }
        }

        else if (currParserState == PARSER_PARAM1)
        {
            
            if (c == SEP_CHAR)
            {
                if (currParamNegative)
                {
                    currParam1Val = -1 * currParam1Val;
                }

                currParserState = PARSER_PARAM2;
                currParamIndex = 0;
                currParamNegative = false;
            }

            else if (c == END_CHAR)
            {
                if (currParamNegative)
                {
                    currParam1Val = -1 * currParam1Val;
                }

                currParserState = PARSER_EXECUTE;
            }
      
            else if ( (currParamIndex == 0) && (c == '-') )
            {
                currParamNegative = true;
                currParamIndex++;
            }
            

            else if ( (currParamIndex >= MAX_PARAM_LENGTH) || (c < '0') || (c > '9') )
            {
                currParserState = PARSER_WAITING;
            }

            else
            {
                int currDigitVal = c - '0';
                currParam1Val = (currParam1Val * 10) + currDigitVal;
                currParamIndex++;
            }
        }
    
        else if (currParserState == PARSER_PARAM2)
        {
            if (c == SEP_CHAR)
            {
                if (currParamNegative)
                {
                    currParam2Val = -1 * currParam2Val;
                }

                currParserState = PARSER_PARAM3;
                currParamIndex = 0;
                currParamNegative = false;
            }

            else if (c == END_CHAR)
            {
                if (currParamNegative)
                {
                    currParam2Val = -1 * currParam2Val;
                }

                currParserState = PARSER_EXECUTE;
            }
      
            else if ( (currParamIndex == 0) && (c == '-') )
            {
                currParamNegative = true;
                currParamIndex++;
            }
            
            else if ( (currParamIndex >= MAX_PARAM_LENGTH) || (c < '0') || (c > '9') )
            {
                currParserState = PARSER_WAITING;
            }

            else
            {
                int currDigitVal = c - '0';
                currParam2Val = (currParam2Val * 10) + currDigitVal;
                currParamIndex++;
            }
        }
    
        else if (currParserState == PARSER_PARAM3)
        {
            if (c == SEP_CHAR)
            {
                if (currParamNegative)
                {
                    currParam3Val = -1 * currParam3Val;
                }

                currParserState = PARSER_PARAM4;
                currParamIndex = 0;
                currParamNegative = false;
            }
      
            else if (c == END_CHAR)
            {
                if (currParamNegative)
                {
                    currParam3Val = -1 * currParam3Val;
                }

                currParserState = PARSER_EXECUTE;
            }

            else if ( (currParamIndex == 0) && (c == '-') )
            {
                currParamNegative = true;
                currParamIndex++;
            }

            else if ( (currParamIndex >= MAX_PARAM_LENGTH) || (c < '0') || (c > '9') )
            {
                currParserState = PARSER_WAITING;
            }

            else
            {
                int currDigitVal = c - '0';
                currParam3Val = (currParam3Val * 10) + currDigitVal;
                currParamIndex++;
            }
        }
    

        else if (currParserState == PARSER_PARAM4)
        {
            if (c == SEP_CHAR)
            {
                if (currParamNegative)
                {
                    currParam4Val = -1 * currParam4Val;
                }

                currParserState = PARSER_PARAM5;
                currParamIndex = 0;
                currParamNegative = false;
            }
      
            else if (c == END_CHAR)
            {
                if (currParamNegative)
                {
                    currParam4Val = -1 * currParam4Val;
                }

                currParserState = PARSER_EXECUTE;
            }

            else if ( (currParamIndex == 0) && (c == '-') )
            {
                currParamNegative = true;
                currParamIndex++;
            }
            
            else if ( (currParamIndex >= MAX_PARAM_LENGTH) || (c < '0') || (c > '9') )
            {
                currParserState = PARSER_WAITING;
            }

            else
            {
                int currDigitVal = c - '0';
                currParam4Val = (currParam4Val * 10) + currDigitVal;
                currParamIndex++;
            }

        }

        else if (currParserState == PARSER_PARAM5)
        {
 
            if (c == END_CHAR)
            {
                if (currParamNegative)
                {
                    currParam5Val = -1 * currParam5Val;
                }
                currParserState = PARSER_EXECUTE;
            }

            else if ( (currParamIndex == 0) && (c == '-') )
            {
                currParamNegative = true;
                currParamIndex++;
            }
            
            else if ( (currParamIndex >= MAX_PARAM_LENGTH) || (c < '0') || (c > '9') )
            {
                currParserState = PARSER_WAITING;
            }

            else
            {
                int currDigitVal = c - '0';
                currParam5Val = (currParam5Val * 10) + currDigitVal;
                currParamIndex++;
            }

        }
    
        /////////////////�жϸ�������////////////////////////////
    
        if (currParserState == PARSER_EXECUTE)
        {

            if ((currCmd[0] == 'O') && (currCmd[1] == 'K'))
            {
                Serial.println("<OK>");
            }
            
            else if ((currCmd[0] == 'S') && (currCmd[1] == 'V'))
            {
                int tweenTime = currParam1Val;
                if (currParam1Val < 0)
                {
                    tweenTime = 0;
                }
                SetServos(tweenTime, currParam2Val, currParam3Val, currParam4Val, currParam5Val, "SV");
            }

            else if ((currCmd[0] == 'S') && (currCmd[1] == 'T'))
            {
                StopAnim("ST");
            }

            else if ((currCmd[0] == 'S') && (currCmd[1] == 'I'))
            {
                StopAnimImmediate("SI");
            }

            else if ((currCmd[0] == 'F') && (currCmd[1] == 'W'))
            {
                int numTimes = currParam1Val;
                if (currParam1Val < 0)
                {
                    numTimes = -1;
                }
                
                PlayAnimNumTimes(walkForwardAnim, walkEndAnim, numTimes, "FW");
            }

            else if ((currCmd[0] == 'B') && (currCmd[1] == 'W'))
            {
                int numTimes = currParam1Val;
                if (currParam1Val < 0)
                {
                    numTimes = -1;
                }
                
                PlayAnimNumTimes(walkBackwardAnim, walkEndAnim, numTimes, "BW");
            }

            else if ((currCmd[0] == 'L') && (currCmd[1] == 'T'))
            {
                int numTimes = currParam1Val;
                if (currParam1Val < 0)
                {
                    numTimes = -1;
                }
                
                PlayAnimNumTimes(turnLeftAnim, NULL, numTimes, "LT");
            }

            else if ((currCmd[0] == 'R') && (currCmd[1] == 'T'))
            {
                int numTimes = currParam1Val;
                if (currParam1Val < 0)
                {
                    numTimes = -1;
                }
                
                PlayAnimNumTimes(turnRightAnim, NULL, numTimes, "RT");
            }
            
            else if ((currCmd[0] == 'S') && (currCmd[1] == 'X'))
            {
                int numTimes = currParam1Val;
                if (currParam1Val < 0)
                {
                    numTimes = -1;
                }
                
                PlayAnimNumTimes(shakeHeadAnim, NULL, numTimes, "SX");
            }

            else if ((currCmd[0] == 'B') && (currCmd[1] == 'X'))
            {
                int numTimes = currParam1Val;
                if (currParam1Val < 0)
                {
                    numTimes = -1;
                }
                
                PlayAnimNumTimes(bounceAnim, NULL, numTimes, "BX");
            }

            else if ((currCmd[0] == 'W') && (currCmd[1] == 'X'))
            {
                int numTimes = currParam1Val;
                if (currParam1Val < 0)
                {
                    numTimes = -1;
                }
                
                PlayAnimNumTimes(wobbleAnim, NULL, numTimes, "WX");
            }
            
            else if ((currCmd[0] == 'W') && (currCmd[1] == 'Y'))
            {
                int numTimes = currParam1Val;
                if (currParam1Val < 0)
                {
                    numTimes = -1;
                }
                
                PlayAnimNumTimes(wobbleLeftAnim, NULL, numTimes, "WY");
            }
            
            else if ((currCmd[0] == 'W') && (currCmd[1] == 'Z'))
            {
                int numTimes = currParam1Val;
                if (currParam1Val < 0)
                {
                    numTimes = -1;
                }
                
                PlayAnimNumTimes(wobbleRightAnim, NULL, numTimes, "WZ");
            }
            
            else if ((currCmd[0] == 'T') && (currCmd[1] == 'X'))
            {
                int numTimes = currParam1Val;
                if (currParam1Val < 0)
                {
                    numTimes = -1;
                }
                
                PlayAnimNumTimes(tapFeetAnim, NULL, numTimes, "TX");
            }
            
            else if ((currCmd[0] == 'T') && (currCmd[1] == 'Y'))
            {
                int numTimes = currParam1Val;
                if (currParam1Val < 0)
                {
                    numTimes = -1;
                }
                
                PlayAnimNumTimes(tapLeftFootAnim, NULL, numTimes, "TY");
            }
            
            else if ((currCmd[0] == 'T') && (currCmd[1] == 'Z'))
            {
                int numTimes = currParam1Val;
                if (currParam1Val < 0)
                {
                    numTimes = -1;
                }
                
                PlayAnimNumTimes(tapRightFootAnim, NULL, numTimes, "TZ");
            }
            
            else if ((currCmd[0] == 'L') && (currCmd[1] == 'X'))
            {
                int numTimes = currParam1Val;
                if (currParam1Val < 0)
                {
                    numTimes = -1;
                }
                
                PlayAnimNumTimes(shakeLegsAnim, NULL, numTimes, "LX");
            }
            
            else if ((currCmd[0] == 'L') && (currCmd[1] == 'Y'))
            {
                int numTimes = currParam1Val;
                if (currParam1Val < 0)
                {
                    numTimes = -1;
                }
                
                PlayAnimNumTimes(shakeLeftLegAnim, NULL, numTimes, "LY");
            }
            
            else if ((currCmd[0] == 'L') && (currCmd[1] == 'Z'))
            {
                int numTimes = currParam1Val;
                if (currParam1Val < 0)
                {
                    numTimes = -1;
                }
                
                PlayAnimNumTimes(shakeRightLegAnim, NULL, numTimes, "LZ");
            }
            currParserState = PARSER_WAITING;
        }
    }
}


void PlayAnim(int animToPlay[][5], int finishAnim[][5], const char *completeStr)
{
    PlayAnimNumTimes(animToPlay, finishAnim, 1, completeStr);
}


void LoopAnim(int animToPlay[][5], int finishAnim[][5], const char *completeStr)
{
    PlayAnimNumTimes(animToPlay, finishAnim, -1, completeStr);
}


void PlayAnimNumTimes(int animToPlay[][5], int finishAnim[][5], int numTimes, const char *completeStr)
{

    nextAnim         = animToPlay;
    nextFinishAnim   = finishAnim;
    nextAnimNumLoops = numTimes;

    if (completeStr == NULL)
    {
        nextAnimCompleteStr[0] = '-';
        nextAnimCompleteStr[1] = '-';
    }
    else
    {
        nextAnimCompleteStr[0] = completeStr[0];
        nextAnimCompleteStr[1] = completeStr[1];
    }
}


void StopAnim(const char *completeStr)
{
    PlayAnimNumTimes(standStraightAnim, NULL, 1, completeStr);
}


void StopAnimImmediate(const char *completeStr)
{
 
    interruptInProgressAnim = true;
    PlayAnimNumTimes(standStraightAnim, NULL, 1, completeStr);
}

void SetServos(int tweenTime, int leftHip, int leftFoot, int rightHip, int rightFoot, const char* completeStr)
{

    if (completeStr == NULL)
    {
        nextAnimCompleteStr[0] = '-';
        nextAnimCompleteStr[1] = '-';
    }
    else
    {
        nextAnimCompleteStr[0] = completeStr[0];
        nextAnimCompleteStr[1] = completeStr[1];
    }
    
    int (*tweenServoData)[5];
    if (currAnim != setServosAnim1)
    {
        tweenServoData = setServosAnim1;
    }
    else
    {
        tweenServoData = setServosAnim2;
    }
    
    tweenServoData[1][TWEEN_TIME_VALUE] = tweenTime;
    tweenServoData[1][LEFT_HIP_VALUE]   = LeftHipIn(leftHip);
    tweenServoData[1][LEFT_FOOT_VALUE]  = LeftFootUp(leftFoot);
    tweenServoData[1][RIGHT_HIP_VALUE]  = RightHipIn(rightHip);
    tweenServoData[1][RIGHT_FOOT_VALUE] = RightFootUp(rightFoot);
    
    PlayAnim(tweenServoData, NULL, completeStr);
}


void setup_Animation()
{

    currLeftHip   = LEFT_HIP_CENTRE;
    currLeftFoot  = LEFT_FOOT_CENTRE;
    currRightHip  = RIGHT_HIP_CENTRE;
    currRightFoot = RIGHT_FOOT_CENTRE;
    UpdateServos();
    

    startLeftHip   = currLeftHip;
    startLeftFoot  = currLeftFoot;
    startRightHip  = currRightHip;
    startRightFoot = currRightFoot;
    
    timeAtLastAnimUpdate    = millis();
    animInProgress          = false;
    interruptInProgressAnim = false;
    currAnim       = NULL;
    finishAnim     = NULL;
    nextAnim       = NULL;
    nextFinishAnim = NULL;
}


void loop_Animation()
{
    long currTime = millis();

    if (timeAtLastAnimUpdate + millisBetweenAnimUpdate > currTime)
    {      
        return;
    }
    else
    {
        timeAtLastAnimUpdate = currTime;
    }
    
   //��������ڶ�����Ҫ�˶��� && ��û�����˶� || ���жϣ�
    if ( (nextAnim != NULL) &&  (!animInProgress || interruptInProgressAnim) )
    {
        //�����ж�
        if (interruptInProgressAnim)
        {            
            Serial.print("<");
            Serial.print(animCompleteStr);
            Serial.println(",-1>");
            
            startLeftHip   = currLeftHip;
            startLeftFoot  = currLeftFoot;
            startRightHip  = currRightHip;
            startRightFoot = currRightFoot;
            
            interruptInProgressAnim = false;
        }
        //����Ҫ�˶��Ĳ�����ֵ
        currAnim           = nextAnim;
        finishAnim         = nextFinishAnim;
        animCompleteStr[0] = nextAnimCompleteStr[0];
        animCompleteStr[1] = nextAnimCompleteStr[1];
        animNumLoops = nextAnimNumLoops;//�����Ƿ�ѭ��ֵ

        //�ָ���ʼֵ
        nextAnim               = NULL; 
        nextFinishAnim         = NULL;
        nextAnimCompleteStr[0] = '-';
        nextAnimCompleteStr[1] = '-';
        
        timeAtStartOfFrame = currTime;//��¼ʱ��
        
        targetFrame = 1; //����̬������ֵ
        
        animInProgress = true;
    }

    //�����Ҫ�˶�
    if (animInProgress)
    {
        int timeInCurrFrame = currTime - timeAtStartOfFrame;

        if (timeInCurrFrame > currAnim[targetFrame][TWEEN_TIME_VALUE])
        {

            if (currAnim[targetFrame][LEFT_HIP_VALUE] >= 0)  //1��1
            {
                currLeftHip = currAnim[targetFrame][LEFT_HIP_VALUE];
            }
            if (currAnim[targetFrame][LEFT_FOOT_VALUE] >= 0)  //1��2
            {
                currLeftFoot = currAnim[targetFrame][LEFT_FOOT_VALUE];
            }
            if (currAnim[targetFrame][RIGHT_HIP_VALUE] >= 0)  //1��3
            {
                currRightHip = currAnim[targetFrame][RIGHT_HIP_VALUE];
            }
            if (currAnim[targetFrame][RIGHT_FOOT_VALUE] >= 0)  //1��4
            {
                currRightFoot = currAnim[targetFrame][RIGHT_FOOT_VALUE];
            }
            UpdateServos();
            
            startLeftHip   = currLeftHip;
            startLeftFoot  = currLeftFoot;
            startRightHip  = currRightHip;
            startRightFoot = currRightFoot;
                   
            targetFrame++; //��¼���еĲ�̬��
            timeAtStartOfFrame = currTime;

            //������еĲ�̬������ÿ�������涨�Ĳ�̬��//��������һ������       
            if (targetFrame > NumOfFrames(currAnim)) 
            {
                animInProgress = false;

                //�ж��Ƿ���ѭ��

                if ((animNumLoops < 0) && (nextAnim == NULL))
                {
                    LoopAnim(currAnim, finishAnim, animCompleteStr);
                }
                
                else if ((animNumLoops < 0) && (nextAnim != NULL))
                {
                    if (finishAnim != NULL)
                    {
                        currAnim       = finishAnim;
                        finishAnim     = NULL;                       
                        animNumLoops = 1;                            
                        timeAtStartOfFrame = currTime;                                              
                        targetFrame = 1;                                    
                        animInProgress = true;
                    }
                    else
                    {               
                        Serial.print("<");
                        Serial.print(animCompleteStr);
                        Serial.println(">");
                    }
                }
                            
                else if ((animNumLoops > 1) && (nextAnim == NULL))
                {
                    PlayAnimNumTimes(currAnim, finishAnim, animNumLoops-1, animCompleteStr);
                }
                
                else
                {
                    //�ָ���ʼֵ
                    if (finishAnim != NULL)
                    {
                        currAnim       = finishAnim;
                        finishAnim     = NULL;                        
                        animNumLoops = 1;      
                        timeAtStartOfFrame = currTime;                                              
                        targetFrame = 1;                     
                        animInProgress = true;
                    }                    
                    else
                    {
                        Serial.print("<");
                        Serial.print(animCompleteStr);
                        Serial.println(">");
                    }
                }
            }
        }
        
        
        if (animInProgress)
        {
           
            float frameTimeFraction = (currTime - timeAtStartOfFrame) / ((float) currAnim[targetFrame][TWEEN_TIME_VALUE]);
            
            if (currAnim[targetFrame][LEFT_HIP_VALUE] >= 0)
            {
                currLeftHip = startLeftHip + ((currAnim[targetFrame][LEFT_HIP_VALUE] - startLeftHip) * frameTimeFraction);
            }
            
            if (currAnim[targetFrame][LEFT_FOOT_VALUE] >= 0)
            {
                currLeftFoot = startLeftFoot + ((currAnim[targetFrame][LEFT_FOOT_VALUE] - startLeftFoot)  * frameTimeFraction);
            }
            
            if (currAnim[targetFrame][RIGHT_HIP_VALUE] >= 0)
            {
                currRightHip = startRightHip  + ((currAnim[targetFrame][RIGHT_HIP_VALUE] - startRightHip) * frameTimeFraction);
            }
            
            if (currAnim[targetFrame][RIGHT_FOOT_VALUE] >= 0)
            {
                currRightFoot = startRightFoot + ((currAnim[targetFrame][RIGHT_FOOT_VALUE] - startRightFoot) * frameTimeFraction);
            }
            
            UpdateServos();
        }
    }
}

//�����������
void UpdateServos()
{
    servoLeftHip.writeMicroseconds(currLeftHip);
    servoLeftFoot.writeMicroseconds(currLeftFoot);
    servoRightHip.writeMicroseconds(currRightHip);
    servoRightFoot.writeMicroseconds(currRightFoot);
}


//�õ�ÿ�������Ĳ�̬��
int NumOfFrames(int animData[][5])
{
    return animData[0][0];
}




