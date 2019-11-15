using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Text;

namespace SimuSurveyXNA.Instruments
{
    class OnePartInstrument 
    {
     /*#region Fields

        //==Models==
        protected Model _BodyModel;

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
        protected const float TRIPOD_ROTATION_VALUE = 30;
        protected const float TELESCOPE_ROTATION_VALUE = 0;
        protected const float TRIBRACH_ROTATION_VALUE = 0;
        protected const float UPPER_LEG_LENGTH = 25;
        protected const float LOWER_LEG_LENGTH = 5;
        protected const float HEIGHT = UPPER_LEG_LENGTH + LOWER_LEG_LENGTH;

        //==Inner Calculation==
        //Translation Part
        protected Matrix levelTranslation;         //Translate the body with respect to rotation value and length of the tripods
        protected Matrix tripodTranslation;       //Translate the lower legs of tripod to desired length
        protected float tripodTranslationValue;

        //Rotation Part
        protected Matrix tripodRotation;         //Rotate the legs of tripod to desired degree
        protected Matrix telescopeRotation;  //Rotate the telescope part
        protected Matrix tribrachRotation;        //Rotate the Upper Body
        #endregion

        public OnePartInstrument(InstrumentType type)
        {
            //Tripod
            _UpperLegLength = UPPER_LEG_LENGTH;
            _LowerLegLength = LOWER_LEG_LENGTH;
            _TripodRotationValue = TRIPOD_ROTATION_VALUE;
            _Level = HEIGHT;

            //Upper Body
            _TelescopeRotationValue = TELESCOPE_ROTATION_VALUE;
            _TribrachRotationValue = TRIBRACH_ROTATION_VALUE;

            //base
            _Type = type;
        }

        public OnePartInstrument()
        {
            //Tripod
            _UpperLegLength = UPPER_LEG_LENGTH;
            _LowerLegLength = LOWER_LEG_LENGTH;
            _TripodRotationValue = TRIPOD_ROTATION_VALUE;
            _Level = HEIGHT;

            //Upper Body
            _TelescopeRotationValue = TELESCOPE_ROTATION_VALUE;
            _TribrachRotationValue = TRIBRACH_ROTATION_VALUE;
        }

        //Compute Tripod part
        public new void Update(Matrix world, Matrix view, Matrix projection)
        {
            //Compute World in advance
            base.Update(world, view, projection);

            //Level
            tripodRotation = Matrix.CreateRotationX(MathHelper.ToRadians(-_TripodRotationValue));
            tripodTranslationValue = (float)((_LowerLegLength + _UpperLegLength) * (Math.Cos((double)MathHelper.ToRadians(_TripodRotationValue))));
            levelTranslation = Matrix.CreateTranslation(0, tripodTranslationValue, 0);
            _Level = tripodTranslationValue;

            //Tripod
            tripodTranslationValue = _LowerLegLength;
            tripodTranslation = Matrix.CreateTranslation(0, 0, -_LowerLegLength * _ModelScaleValue);
        }

        public void Draw()
        {
            if (_LowerBodyModel == null) //model is unloaded or not loaded yet
                return;

            //==Apply matrices to the relevant bones==

            //Tripod
            _Leg1_1.Transform = _Leg1_1Transform * tripodRotation;
            _Leg1_2.Transform = _Leg1_2Transform * tripodTranslation * tripodRotation;
            _Leg2_1.Transform = _Leg2_1Transform * tripodRotation;
            _Leg2_2.Transform = _Leg2_2Transform * tripodTranslation * tripodRotation;
            _Leg3_1.Transform = _Leg3_1Transform * tripodRotation;
            _Leg3_2.Transform = _Leg3_2Transform * tripodTranslation * tripodRotation;

            // Set the world matrix as the root transform of the model.
            _LowerBodyModel.Root.Transform = modelScale * modelRotation * worldTranslation * _world * levelTranslation;

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
            set { _TelescopeRotationValue = value; }
        }

        public float TripodLength
        {
            get { return _LowerLegLength + _UpperLegLength; }
            set { _LowerLegLength = (value - _UpperLegLength); }
        }*/
    }
}
