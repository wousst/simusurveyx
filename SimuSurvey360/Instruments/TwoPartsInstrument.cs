using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Text;

namespace SimuSurvey360.Instruments
{
    class TwoPartsInstrument : Instrument
    {
        #region Fields

        //==Models==
        protected Model _LowerBodyModel;
        protected Model _UpperBodyModel;

        //==Bones and Transfroms (Don't change the matrix directly, change the value of properties)==
        //Upper Body Part
        protected ModelBone _Tribrach;  // the triangular part
        protected ModelBone _Telescope;
        protected ModelBone _UpperBody;//the main body of the upper part

        protected Matrix _UpperBodyTransform;
        protected Matrix _TribrachTransform;
        protected Matrix _TelescopeTransform;
        //Lower Body Part
        protected ModelBone _TripodHead;//the main body of the lower part
        protected ModelBone _Leg1_1;
        protected ModelBone _Leg1_2;
        protected ModelBone _Leg2_1;
        protected ModelBone _Leg2_2;
        protected ModelBone _Leg3_1;
        protected ModelBone _Leg3_2;

        protected Matrix _TripodHeadTransform;
        protected Matrix _Leg1_1Transform;
        protected Matrix _Leg1_2Transform;
        protected Matrix _Leg2_1Transform;
        protected Matrix _Leg2_2Transform;
        protected Matrix _Leg3_1Transform;
        protected Matrix _Leg3_2Transform;

        //The transformation matrix for rendering
        protected Matrix[] _LowerBodyBoneTransforms;
        protected Matrix[] _UpperBodyBoneTransforms;


        //==Properties (Values controled from outside the class)==
        protected float _TripodRotationValue;//the angle (degree) between main body and tripod
        protected float _UpperLegLength;//the length of the tripod
        protected float _LowerLegLength;//the length of the tripod
        protected float _TelescopeRotationValue;
        protected float _TribrachRotationValue;

        //==Default Value==  
        
        protected const float TRIPOD_ROTATION_VALUE = 30;   // 腳架往外張開
        protected const float TELESCOPE_ROTATION_VALUE = 0; // + 為往下( 大的那一邊 )
        protected const float TRIBRACH_ROTATION_VALUE = 0;  // + 為右邊( 大的那一邊 )
        // 以下這幾個值, 要除以 Instruments 中的 MODEL_SCALE_VALUE, 要不然要改 Model 
        protected const float DEFAULT_UPPER_LEG_LENGTH = 110f ;  //110 ; // 55;
        protected const float DEFAULT_LOWER_LEG_LENGTH = 10f ; // 10 ; // 5;
        protected const float LOWER_LEG_LENGTH = 70f ; // 70; // 35;
        protected const float DEFAULT_UPPER_LEG_LENGTH_SHORT = 97f;  //110 ; // 55;
        protected const float DEFAULT_LOWER_LEG_LENGTH_SHORT = 10f; // 10 ; // 5;
        protected const float LOWER_LEG_LENGTH_SHORT = 62f; // 70; // 35;
        // protected const float HEIGHT = DEFAULT_UPPER_LEG_LENGTH + DEFAULT_LOWER_LEG_LENGTH;

        //==Inner Calculation==
        //Translation Part
        protected Matrix levelTranslation;         //Translate the body with respect to rotation value and length of the tripods
        protected Matrix tripodTranslation;       //Translate the lower legs of tripod to desired length
        protected float tripodTranslationValue;

        //Rotation Part
        protected Matrix tripodRotation;         //Rotate the legs of tripod to desired degree
        protected Matrix telescopeRotation;  //Rotate the telescope part
        protected Matrix tribrachRotation;        //Rotate the Upper Body


        protected float upper_leg_length;
        protected float lower_leg_length;

        // protected float _WorldYOffset; // for Terrain 

        #endregion

        public TwoPartsInstrument(InstrumentType type)
        {
            //Tripod
            _UpperLegLength = DEFAULT_UPPER_LEG_LENGTH / MODEL_SCALE_VALUE ;
            _LowerLegLength = DEFAULT_LOWER_LEG_LENGTH / MODEL_SCALE_VALUE ;
            _TripodRotationValue = TRIPOD_ROTATION_VALUE;
            _Level = (DEFAULT_UPPER_LEG_LENGTH_SHORT + DEFAULT_LOWER_LEG_LENGTH_SHORT) / MODEL_SCALE_VALUE;

            //Upper Body
            _TelescopeRotationValue = TELESCOPE_ROTATION_VALUE;
            _TribrachRotationValue = TRIBRACH_ROTATION_VALUE;

            //base
            _Type = type;
        }

        public TwoPartsInstrument()
        {
            //Tripod
            _UpperLegLength = DEFAULT_UPPER_LEG_LENGTH / MODEL_SCALE_VALUE ;
            _LowerLegLength = DEFAULT_LOWER_LEG_LENGTH / MODEL_SCALE_VALUE ;
            _TripodRotationValue = TRIPOD_ROTATION_VALUE;
            _Level = (DEFAULT_UPPER_LEG_LENGTH_SHORT + DEFAULT_LOWER_LEG_LENGTH_SHORT) / MODEL_SCALE_VALUE;

            //Upper Body
            _TelescopeRotationValue = TELESCOPE_ROTATION_VALUE;
            _TribrachRotationValue = TRIBRACH_ROTATION_VALUE;
        }


        public void ResetTwoPartsInstrument()
        {
            //Tripod
            if (_Type == InstrumentType.Leveling)
            {
                _UpperLegLength = DEFAULT_UPPER_LEG_LENGTH_SHORT / MODEL_SCALE_VALUE;
                _LowerLegLength = DEFAULT_LOWER_LEG_LENGTH_SHORT / MODEL_SCALE_VALUE;
                _Level = (DEFAULT_UPPER_LEG_LENGTH_SHORT + DEFAULT_LOWER_LEG_LENGTH_SHORT) / MODEL_SCALE_VALUE;
            }
            else
            {
                _UpperLegLength = DEFAULT_UPPER_LEG_LENGTH / MODEL_SCALE_VALUE;
                _LowerLegLength = DEFAULT_LOWER_LEG_LENGTH / MODEL_SCALE_VALUE;
                _Level = (DEFAULT_UPPER_LEG_LENGTH_SHORT + DEFAULT_LOWER_LEG_LENGTH_SHORT) / MODEL_SCALE_VALUE;
            }




        }

        //Compute Tripod part
        public new void Update(Matrix world, Matrix view, Matrix projection)
        {
            //Compute World in advance
            base.Update(world, view, projection);



            //Tripod
            _Leg1_1.Transform = _Leg1_1Transform * tripodRotation;
            _Leg1_2.Transform = _Leg1_2Transform * tripodTranslation * tripodRotation;
            _Leg2_1.Transform = _Leg2_1Transform * tripodRotation;
            _Leg2_2.Transform = _Leg2_2Transform * tripodTranslation * tripodRotation;
            _Leg3_1.Transform = _Leg3_1Transform * tripodRotation;
            _Leg3_2.Transform = _Leg3_2Transform * tripodTranslation * tripodRotation;

            // Set the world matrix as the root transform of the model.
            _LowerBodyModel.Root.Transform = /* Matrix.CreateTranslation( 0f, _WorldYOffset, 0f ) * */ Matrix.CreateRotationX(MathHelper.ToRadians(-90)) * modelRotation * modelScale * worldTranslation * _world * levelTranslation;







            //Level
            tripodRotation = Matrix.CreateRotationX(MathHelper.ToRadians(-_TripodRotationValue));
            tripodTranslationValue = (float)((_LowerLegLength + _UpperLegLength) * (Math.Cos((double)MathHelper.ToRadians(_TripodRotationValue))));
            // for Terrain
            levelTranslation = Matrix.CreateTranslation(0, tripodTranslationValue, 0) * Matrix.CreateTranslation(0f, _WorldYOffset, 0f);
            // 原始設計
            // levelTranslation = Matrix.CreateTranslation(0, tripodTranslationValue, 0);
            _Level = tripodTranslationValue;

            //Tripod
            tripodTranslationValue = _LowerLegLength;
            tripodTranslation = Matrix.CreateTranslation(0, 0, -_LowerLegLength  * _ModelScaleValue );
        }

        public void Draw()
        {
            if (_LowerBodyModel == null) //model is unloaded or not loaded yet
                return;

            //==Apply matrices to the relevant bones==

            //Tripod
            //_Leg1_1.Transform = _Leg1_1Transform * tripodRotation;
            //_Leg1_2.Transform = _Leg1_2Transform * tripodTranslation * tripodRotation;
            //_Leg2_1.Transform = _Leg2_1Transform * tripodRotation;
            //_Leg2_2.Transform = _Leg2_2Transform * tripodTranslation * tripodRotation;
            //_Leg3_1.Transform = _Leg3_1Transform * tripodRotation;
            //_Leg3_2.Transform = _Leg3_2Transform * tripodTranslation * tripodRotation;

            //// Set the world matrix as the root transform of the model.
            //_LowerBodyModel.Root.Transform = /* Matrix.CreateTranslation( 0f, _WorldYOffset, 0f ) * */ Matrix.CreateRotationX(MathHelper.ToRadians(-90)) * modelRotation * modelScale * worldTranslation * _world * levelTranslation;

            // Look up combined bone matrices for the entire model.
             _LowerBodyModel.CopyAbsoluteBoneTransformsTo(_LowerBodyBoneTransforms);

            //Draw the model.
            foreach (ModelMesh mesh in _LowerBodyModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = _LowerBodyBoneTransforms[mesh.ParentBone.Index];
                    effect.View = _view;
                    effect.Projection = _projection;
                    effect.EnableDefaultLighting();
                }
                mesh.Draw();
            }
        }

        public float TripodRotationValue
        {
            get { return _TripodRotationValue; }
            set
            {
                if (value < 90)
                    _TripodRotationValue = value;
                else
                    _TripodRotationValue = 90;
                if (value < 0)
                    _TripodRotationValue = 0;
            }
        }
        public float TribrachRotationValue
        {
            get { return _TribrachRotationValue; }
            set { _TribrachRotationValue = value; }
        }

        public float TelescopeRotationValue
        {
            get { return _TelescopeRotationValue; }
            set 
            {
                _TelescopeRotationValue = value; 
            }
        }

        public float TripodLength
        {
            get { return _LowerLegLength + _UpperLegLength; }
            set
            {
                _LowerLegLength = (value - _UpperLegLength);
                if (_Type == InstrumentType.Leveling)
                {
                    if (_LowerLegLength > (LOWER_LEG_LENGTH_SHORT / MODEL_SCALE_VALUE))
                        _LowerLegLength = LOWER_LEG_LENGTH_SHORT / MODEL_SCALE_VALUE;
                    if (_LowerLegLength < 0)
                        _LowerLegLength = 0;
                }
                else
                {
                    if (_LowerLegLength > (LOWER_LEG_LENGTH / MODEL_SCALE_VALUE))
                        _LowerLegLength = LOWER_LEG_LENGTH / MODEL_SCALE_VALUE;
                    if (_LowerLegLength < 0)
                        _LowerLegLength = 0;
                }
            }
        }
        public float WorldYOffset
        {
            get { return _WorldYOffset; }
            set { _WorldYOffset = value; }
        }

    }
}
