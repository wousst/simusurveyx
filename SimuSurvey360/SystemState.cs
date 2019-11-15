using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SimuSurvey360.Instruments;
namespace SimuSurvey360
{


    public class SystemState
    {

        public enum S_State
        {
            STATE_NONE,
            STATE_INIT,             // 遊戲開始
            STATE_OPENING,          // 小動畫
            STATE_MENU_WAIT_START,  // 等待 Start 或 A鍵
            STATE_MENU_SCENARIO,    // 選擇場景, 圖形式, 單層
            STATE_MENU_MAIN,        // 選擇功能, 文字式, 掛圖片及說明文字
            STATE_NAVIGATION,       // 移動物件狀態, 自己, 儀器, 尺1, 尺2
            STATE_INPUT,            // 輸入測量結果
            STATE_MENU_SURVEYING,   // 測量進行狀態
            STATE_MENU_RESULT,      // 列出測量結果, 可以進入REPLAY
            STATE_MENU_REPLAY,      // 重播狀態
            STATE_MENU_OPTIONS     // 效果選單
        };
        S_State _CurrState, _PrevState;
        S_State _Wait2ProcessState = S_State.STATE_NONE; 
        const int _Max_MenuScenario = 5 ;
        const int _Max_NaviState = 4;

        InstrumentType _InsType;       // 0 = TotalStation, 1= Leveling 2 = Theodolite
        int _RulerType;     // 0 = Ruler, 2 = Pole, skc, remove later 
        int _Ruler_Ns;
        int _Pole_Ns;
        int _Mirror_Ns;
        int _SetPoint_Ns;
        int _RulerPad_Ns; 
        int _OperationMode; // 0 = 1st Person, 1 = Remote 

        int _SelectItemMenuScenario;
        int _SelectItemMenuMain;

        int _PeakedObj, _Ready2PeakObj ;
        float _Ins_H0; // Height of camera when the object was peaked
        int[] SetPointState = new int[4];
        bool _SetPoint0_Enabled;

        int _TrainingN;

        float _BaseHeight;
        float _BaseNE; 

        bool _Preference_Random_Enabled;
        bool _SkySphereRotate;
        bool _RadarOff;
        int _SkyIndex; 
 
        // if Z= 0 = inValid, 1 = Valid 
        Vector3[] _TP = new Vector3[2];
        Vector3[] _SurveyP = new Vector3[4];  

        Vector3 _Ins_Position;
        Vector3[] _Ruler_Position = new Vector3[4];
        Vector3[] _Pole_Position = new Vector3[4];
        Vector3[] _Mirror_Position = new Vector3[4];
        float[] _Ruler_Length = new float[4];
        float[] _Pole_Length = new float[4];
        float[] _Mirror_Length = new float[4];
        Vector3[] _SetPoint_Position = new Vector3[5];
        Vector3[] _RulerPad_Position = new Vector3[4];
        int[] _Ruler_Attach = new int[4]; // possible = 0.. 4
        float _RelativeAngleBase;
        float _ErrorX, _ErrorY, _ErrorZ;
        bool _DistanceErrorEnable = true ;
        bool _AudioEnable = true;

        int _Background_N = 0;

        public enum S_State_Peak
        {
            STATE_NONE = 0,
            STATE_INS,
            STATE_R1,
            STATE_R2,
            STATE_R3,
            STATE_R4,
            STATE_R5

        }

        public enum S_State_Navigation
        {
            STATE_ME = 0,
            STATE_INSTRUMENT,
            STATE_RULE1,
            STATE_RULE2,
            STATE_RULE3,
            STATE_RULE4,
            STATE_RULE5,
            // 可以擴充
        }
        // 需要知道每一個物體的坐標資訊
        S_State_Navigation _CurrNaviState; 




        public enum S_State_Input
        {
            STATE_LAYER1,   // Items
            STATE_LEYER2,   // OSKB
        }
        int InputItemIndex; 

        public enum S_State_Surveying
        {
            STATE_LAYOUT1,   // 1大 + 左1小 + 右2小 
            STATE_LAYOUT2,   // 1大
            STATE_LAYOUT3,   // 1大 + 左1小
        }

        public enum S_State_Replay  // 重播狀態
        {
            STATE_MENU_STOP, 
            STATE_MENU_PLAY,
            STATE_MENU_PAUSE, 
        }
        int PlaySpeed, PlayIndex; 

        /// <summary>
        /// Get or Set the value displayed in the text input box.  This is the value updated by the
        /// user as they enter text.
        /// </summary>
        /// 
        public bool RadarOff
        {
            get { return _RadarOff; }
            set { _RadarOff = value; }
        }
        public float ErrorX
        {
            get { return _ErrorX; }
            set { _ErrorX = value; }
        }
        public float ErrorY
        {
            get { return _ErrorY; }
            set { _ErrorY = value; }
        }
        public float ErrorZ
        {
            get { return _ErrorZ; }
            set { _ErrorZ = value; }
        }
        public bool DistanceErrorEnable
        {
            get { return _DistanceErrorEnable; }
            set { _DistanceErrorEnable = value; }
        }

        public bool AudioEnable
        {
            get { return _AudioEnable; }
            set { _AudioEnable = value; }
        }

        public int Background_N
        {
            get { return _Background_N; }
            set { _Background_N = value; }
        }

        public float RelativeAngleBase
        {
            get { return _RelativeAngleBase; }
            set { _RelativeAngleBase = value; }
        }

        public S_State CurrState
        {
            get { return _CurrState; }
            set { _PrevState = _CurrState;  _CurrState = value; }
        }

        public S_State PrevState
        {
            get { return _PrevState; }
            set { _PrevState = value; }
        }

        public S_State Wait2ProcessState
        {
            get { return _Wait2ProcessState; }
            set { _Wait2ProcessState = value; }
        }

        public S_State_Navigation CurrNaviState
        {
            get { return _CurrNaviState; }
            set { _CurrNaviState = value; }
        }


        
        public InstrumentType InsType
        {
            get { return _InsType; }
            set { _InsType = value; }
        }
        // 0 = Ruler, 2 = Pole, skc, remove it latr
        //public int RulerType
        //{
        //    get { return _RulerType; }
        //    set { _RulerType = value; }
        //}

        public int Ruler_Ns
        {
            get { return _Ruler_Ns; }
            set { _Ruler_Ns = value; }
        }

        public int Pole_Ns
        {
            get { return _Pole_Ns; }
            set { _Pole_Ns = value; }
        }
        public int Mirror_Ns
        {
            get { return _Mirror_Ns; }
            set { _Mirror_Ns = value; }
        }
        public int SetPoint_Ns
        {
            get { return _SetPoint_Ns; }
            set { _SetPoint_Ns = value; }
        }
        public int RulerPad_Ns
        {
            get { return _RulerPad_Ns; }
            set { _RulerPad_Ns = value; }
        }
        public int OperationMode
        {
            get { return _OperationMode; }
            set { _OperationMode = value; }
        }


        public int PickedObj
        {
            get { return _PeakedObj; }
            set { _PeakedObj = value; }
        }



        public int Ready2PickObj
        {
            get { return _Ready2PeakObj; }
            set { _Ready2PeakObj = value; }
        }


        public float Ins_H0
        {
            get { return _Ins_H0; }
            set { _Ins_H0 = value; }
        }

        public int Max_MenuScenario
        {
            get { return _Max_MenuScenario; }
        }

        public int Max_NaviState
        {
            get { return _Max_NaviState; }
        }

        public int SelectItemMenuScenario 
        {
            get { return _SelectItemMenuScenario; }
            set { _SelectItemMenuScenario = value; }
        }


        public int SelectItemMenuMain
        {
            get { return _SelectItemMenuMain; }
            set { _SelectItemMenuMain = value; }
        }


        public Vector3 Ins_Position
        {
            get { return _Ins_Position; }
            set { _Ins_Position = value; }
        }



        public bool SetPoint0_Enabled
        {
            get { return _SetPoint0_Enabled; }
            set { _SetPoint0_Enabled = value; }
        }


        public bool Preference_Random_Enabled
        {
            get { return _Preference_Random_Enabled; }
            set { _Preference_Random_Enabled = value; }
        }


        public bool SkySphereRotate
        {
            get { return _SkySphereRotate; }
            set { _SkySphereRotate = value; }
        }

        public int SkyIndex
        {
            get { return _SkyIndex; }
            set { _SkyIndex = value; }
        }

        public int TrainingN
        {
            get { return _TrainingN; }
            set { _TrainingN = value; }
        }

        //  此處為輸出使用所以是已經  x4  
        public float BaseHeight 
        {
            get { return _BaseHeight; }
            set { _BaseHeight = value; }
        }
        public float BaseNE
        {
            get { return _BaseNE; }
            set { _BaseNE = value; }
        }
        // init
        public SystemState()
        {
            _Preference_Random_Enabled = false;

            _CurrState = S_State.STATE_INIT;
            _PrevState = S_State.STATE_INIT;
            for (int i = 0; i < 4; i++)
                _Ruler_Attach[i] = -1;


            // if Z= 0 = inValid, 1 = Valid 
            _TP[0].Z = 0f;
            _TP[1].Z = 0f;
            for (int i = 0; i < 4; i++)
            _SurveyP[i].Z = 0f;  

        }


        public void setTP( Vector3 _v)
        {
            if (_TP[1].Z == 0)
            {
                if (_TP[0].Z == 0)
                {
                    _TP[0].X = _v.X; _TP[0].Y = _v.Y; _TP[0].Z = 1;
                }
                else 
                {
                    if ((_v.X != _TP[1].X) || (_v.Y != _TP[1].Y))
                    {
                        _TP[1].X = _v.X; _TP[1].Y = _v.Y; _TP[1].Z = 1;
                    }
                }
            }
            else
            {
                if ((_v.X != _TP[1].X) || (_v.Y != _TP[1].Y))
                {
                    _TP[0].X = _TP[1].X; _TP[0].Y = _TP[1].Y;
                    _TP[1].X = _v.X; _TP[1].Y = _v.Y;
                }
            }              
        }

        public Vector3 getTP(int i)
        {
            return _TP[i]; 
        }

        public void CheckTP()
        {
            int i; float _min = 10000;
            int _idx = 0;
            float[] _dm = new float[4];
            for (i = 0; i < _Ruler_Ns; i++) // should be 3
            {
                float _d0 = Vector3.Distance(_SetPoint_Position[0], _Ruler_Position[i]);
                float _d1 = Vector3.Distance(_SetPoint_Position[1], _Ruler_Position[i]);
                _dm[i] = ( _d0 < _d1 )? _d0 : _d1;              
            }
           
            for (i = 0; i < _Ruler_Ns; i++) // should be 3
            {
                if (_dm[i] < _min)
                {
                    _min = _dm[i];
                    _idx = i;
                }
            }
            setTP(_Ruler_Position[_idx]);

        }

        public Vector3 getRuler_Position(int i)
        {
            return _Ruler_Position[i];
        }

        public void setRuler_Position( int i, Vector3 _v)
        {
            _Ruler_Position[i] = _v;
        }

        public Vector3 getPole_Position(int i)
        {
            return _Pole_Position[i];
        }

        public void setPole_Position(int i, Vector3 _v)
        {
            _Pole_Position[i] = _v;
        }

        public Vector3 getMirror_Position(int i)
        {
            return _Mirror_Position[i];
        }
        public void setMirror_Position(int i, Vector3 _v)
        {
            _Mirror_Position[i] = _v;
        }


        public float getRuler_Length(int i)
        {
            return _Ruler_Length[i];
        }

        public void setRuler_Length(int i, float _l)
        {
            _Ruler_Length[i] = _l;
        }

        public float getPole_Length(int i)
        {
            return _Pole_Length[i];
        }

        public void setPole_Length(int i, float _l)
        {
            _Pole_Length[i] = _l;
        }

        public float getMirror_Length(int i)
        {
            return _Mirror_Length[i];
        }

        public void setMirror_Length(int i, float _l)
        {
            _Mirror_Length[i] = _l;
        }

        public Vector3 getSetPoint_Position(int i)
        {
            return _SetPoint_Position[i];
        }

        public void setSetPoint_Position(int i, Vector3 _v)
        {
            _SetPoint_Position[i] = _v;
        }


        public Vector3 getRulerPad_Position(int i)
        {
            return _RulerPad_Position[i];
        }

        public void setRulerPad_Position(int i, Vector3 _v)
        {
            _RulerPad_Position[i] = _v;
        }

        public void DegreeToDMS_String(float _Degree, out string TmpStr)
        {
            int _d0, _d1, _d2;
            float _r;
            // string TmpStr;
            _d0 = (int)_Degree;
            _r = _Degree - (float)_d0;
            _d1 = (int)(_r * 60f);
            _r = ((_r * 60f) - (float)_d1) * 60f;
            _d2 = (int)( _r + 0.5f);
            TmpStr = System.String.Format("{0:d02}.{1:d02}'{2:d02}\"", _d0, _d1, _d2);
            

        }

    }
}
