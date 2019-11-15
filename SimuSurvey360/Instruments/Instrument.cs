using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Text;
namespace SimuSurvey360.Instruments
{
    public enum InstrumentType
    {
        TotalStation,
        Leveling,
        Theodolite,
        Level,
        Ruler,
        Pole,
        Mirror,
        HintBox,
        SetPoint, 
        RulerPad
    };
    //enum InstrumentSubType
    //{
    //    None,
    //    TotalStation,
    //    Leveling,
    //    Theodolite
    //};
    class Instrument
    {
        #region Fields
        //==Properties (Values controled from outside the class)==
        protected Vector3 _WorldPosition;//instrument position (N, H, E) coordinates
        protected float _Level;//the height of the instrument
        protected float _ModelScaleValue;//to match the model for fbx and the system coordinate

        protected float _ModelRatationYValue;//to match the model for fbx and the system coordinate

        protected InstrumentType _Type;
        // protected InstrumentSubType _SubType;
        protected String _Name;

        //==Default Value==
        protected Vector3 POSITION = new Vector3(0, 0, 0);
        protected const float MODEL_SCALE_VALUE = 4f ;

        //==View ==
        protected Matrix _view;
        protected Matrix _projection;
        protected Matrix _world;

        //==Inner Calculation==
        protected Matrix worldTranslation;      //Translate the instrument to _worldPostion from origin
        protected Matrix modelRotation;         //Rotate the whole model 90 degree along X axis, 這是為了 Model 與 XNA 對應
        protected Matrix modelScale;            //Scale down the model for MODEL_SCALE_VALUE

        protected float _WorldYOffset;          // for Terrain 

        protected int _RadarIndex; 
        
        #endregion
       
        public Instrument(InstrumentType type)
        {
            //Global
            _WorldPosition = POSITION;
            _ModelScaleValue = MODEL_SCALE_VALUE;
            _ModelRatationYValue = 0f; 
            _Type = type;
        }

        public Instrument()
        {
            //Global
            _WorldPosition = POSITION;
            _ModelScaleValue = MODEL_SCALE_VALUE;
        }

        //Compute Model Import Part and World Translation part
        public void Update(Matrix world, Matrix view, Matrix projection)
        {
            //==Calculate matrices based on the current animation position==
            modelRotation = Matrix.CreateRotationY(MathHelper.ToRadians(_ModelRatationYValue)); ;
            modelScale = Matrix.CreateScale((1 / _ModelScaleValue), (1 / _ModelScaleValue), (1 / _ModelScaleValue));

            //World
            worldTranslation = Matrix.CreateTranslation(_WorldPosition.X, _WorldPosition.Y, _WorldPosition.Z);

            //==View==
            _world = world;
            _view = view;
            _projection = projection;
        }

        public Vector3 WorldPosition
        {
            get { return _WorldPosition; }
            set { _WorldPosition = value; }
        }

        public float WorldYOffset
        {
            get { return _WorldYOffset; }
            set { _WorldYOffset = value; }
        }


        public float Level
        {
            get { return _Level; }
            set { _Level = value; }
        }

        public InstrumentType Type
        {
            get { return _Type; }
        }

        //public InstrumentSubType SubType
        //{
        //    get { return _SubType; }
        //}

        public String Name
        {
            set { _Name = value; }
            get { return _Name; }
        }

        public float ModelRatationYValue
        {
            set { _ModelRatationYValue = value; }
            get { return _ModelRatationYValue; }
        }

        public float N_Coordinate
        {
            set { _WorldPosition.Z = value; }
            get { return _WorldPosition.Z; }
        }

        public float E_Coordinate
        {
            set { _WorldPosition.X = value; }
            get { return _WorldPosition.X; }
        }
       public int RadarIndex
        {
            set { _RadarIndex = value; }
            get { return _RadarIndex; }
        }


    }
}
