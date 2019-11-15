using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Text;

namespace SimuSurvey360.Instruments
{
    class TotalStation : TwoPartsInstrument
    {
        public Camera _Camera;
        bool _HideTelescope;

        float _Error_X, _Error_Y, _Error_Z; // degree, +-1
        public float Error_X {
             get { return _Error_X ; }
            set { _Error_X = value ; }
        }
        public float Error_Y
        {
            get { return _Error_Y; }
            set { _Error_Y = value; }
        }
        public float Error_Z
        {
            get { return _Error_Z; }
            set { _Error_Z = value; }
        }
            
        public bool HideTelescope {
            get { return _HideTelescope;}
            set { _HideTelescope = value; }
        }

        public TotalStation()
        {
            
        }

        public TotalStation(InstrumentType type)
        {
            _Type = type;
            _HideTelescope = false;
            ResetTwoPartsInstrument();
        }

        public void Load(ContentManager content)
        {
            //Load Models from XNB objects and extract the transform matrices
            //==Tripod==





            //==Upper Body==
            switch (_Type)
            {
                case InstrumentType.TotalStation:

                    _LowerBodyModel = content.Load<Model>("Tripod_Large");
                    _TripodHead = _LowerBodyModel.Bones["Tripod_head "];  // 注意, 名稱空一格

                    _UpperBodyModel = content.Load<Model>("Leica");
                    _Tribrach = _UpperBodyModel.Bones["tribrach"];
                    _UpperBody = _UpperBodyModel.Bones["upper_body"];
                    _Telescope = _UpperBodyModel.Bones["telescope"];
                    break;
                case InstrumentType.Leveling:
                    // _LowerBodyModel = content.Load<Model>("SmallTripod");
                    _LowerBodyModel = content.Load<Model>("Tripod_Small");
                    _TripodHead = _LowerBodyModel.Bones[0];
                    // H= 10cm
                    // _UpperBodyModel = content.Load<Model>("Leica_Level");
                    // H= 9.62cm
                    _UpperBodyModel = content.Load<Model>("Nikon_Level");
                    _Tribrach = _UpperBodyModel.Bones["tribrach"];
                    _UpperBody = _UpperBodyModel.Bones["horizontal_scale"];
                    _Telescope = _UpperBodyModel.Bones["telescope"];
                    break;
                case InstrumentType.Theodolite:
                    _LowerBodyModel = content.Load<Model>("Tripod_Large");
                    _TripodHead = _LowerBodyModel.Bones["Tripod_head "];  // 注意, 名稱空一格

                    _UpperBodyModel = content.Load<Model>("Nikon_Theo");
                    _Tribrach = _UpperBodyModel.Bones["tribrach"];
                    _UpperBody = _UpperBodyModel.Bones["upper_body"];
                    _Telescope = _UpperBodyModel.Bones["telescope"];
                    break;
            }


            _Leg1_1 = _LowerBodyModel.Bones["Upper_leg_1"];
            _Leg1_2 = _LowerBodyModel.Bones["Lower_leg_1"];
            _Leg2_1 = _LowerBodyModel.Bones["Upper_leg_2"];
            _Leg2_2 = _LowerBodyModel.Bones["Lower_leg_2"];
            _Leg3_1 = _LowerBodyModel.Bones["Upper_leg_3"];
            _Leg3_2 = _LowerBodyModel.Bones["Lower_leg_3"];

            _TripodHeadTransform = _TripodHead.Transform;
            _Leg1_1Transform = _Leg1_1.Transform;
            _Leg1_2Transform = _Leg1_2.Transform;
            _Leg2_1Transform = _Leg2_1.Transform;
            _Leg2_2Transform = _Leg2_2.Transform;
            _Leg3_1Transform = _Leg3_1.Transform;
            _Leg3_2Transform = _Leg3_2.Transform;

            _TribrachTransform = _Tribrach.Transform;
            _UpperBodyTransform = _UpperBody.Transform;
            _TelescopeTransform = _Telescope.Transform;

            // Allocate the transform matrix array.
            _LowerBodyBoneTransforms = new Matrix[_LowerBodyModel.Bones.Count];
            _UpperBodyBoneTransforms = new Matrix[_UpperBodyModel.Bones.Count];

            // _LowerBodyModel.CopyAbsoluteBoneTransformsTo(_LowerBodyBoneTransforms);
            // _UpperBodyModel.CopyAbsoluteBoneTransformsTo(_UpperBodyBoneTransforms);


            _Camera = new Camera(this.Name,
                1280,
                720,
                1280f / 720f,
                MathHelper.PiOver4,
                0.01f,
                10000,
                true);
            _Camera.Initialize((Matrix.CreateTranslation(0, 38, -2)).Translation,
                (Matrix.CreateTranslation(0, 38, -100)).Translation);

            _Error_X = 0f ;
            _Error_Y = 0f ;
            _Error_Z = 0f ;

        }

        public TotalStationArgs WarpInstrment()
        {
            TotalStationArgs totalStationArgs = new TotalStationArgs();
            totalStationArgs.WorldPosition = WorldPosition;
            totalStationArgs.TripodLength = TripodLength;
            totalStationArgs.TelescopeRotationValue =TelescopeRotationValue;
            totalStationArgs.TripodRotationValue =TripodRotationValue;
            totalStationArgs.TribrachRotationValue = TribrachRotationValue;
            return totalStationArgs;
        }
        public void SetInstrument(TotalStationArgs args)
        {
            WorldPosition = args.WorldPosition;
            TripodLength = args.TripodLength;
            TelescopeRotationValue = args.TelescopeRotationValue;
            TripodRotationValue = args.TripodRotationValue;
            TribrachRotationValue = args.TribrachRotationValue;
        }

        //Compute Upper Body part
        public new void Update(Matrix world, Matrix view, Matrix projection)
        {
            //Compute World, View and Tripod in advance
            base.Update(world, view, projection);

            //Upper Body
            if (_Type == InstrumentType.Leveling)
            {
                // Leica 
                _UpperBodyModel.Root.Transform = Matrix.CreateTranslation(0f, 0f, 3.3f) * Matrix.CreateRotationZ(MathHelper.ToRadians(180f + _TribrachRotationValue)) * _LowerBodyModel.Root.Transform;
                // Nikon
                // _UpperBodyModel.Root.Transform = Matrix.CreateRotationZ(MathHelper.ToRadians(_TribrachRotationValue)) * _LowerBodyModel.Root.Transform;
            }
            else
                _UpperBodyModel.Root.Transform = _LowerBodyModel.Root.Transform;
            //if (_Type == InstrumentType.Leveling)
            //    _UpperBodyModel.Root.Transform *= Matrix.CreateRotationX(MathHelper.ToRadians(180f));
            if (_Type == InstrumentType.Theodolite)
                telescopeRotation = Matrix.CreateRotationX(MathHelper.ToRadians(_TelescopeRotationValue + 180));
            else
            telescopeRotation = Matrix.CreateRotationX(MathHelper.ToRadians(_TelescopeRotationValue));

            if (_Type == InstrumentType.Theodolite)
                tribrachRotation = Matrix.CreateRotationZ(MathHelper.ToRadians(_TribrachRotationValue + 180 ));
            else
                tribrachRotation = Matrix.CreateRotationZ(MathHelper.ToRadians(_TribrachRotationValue));

            

            //_Tribrach.Transform = Matrix    
            //Upper Body
            if (_Type != InstrumentType.Leveling)
            _UpperBody.Transform = Matrix.CreateRotationX(MathHelper.ToRadians(_Error_X)) * Matrix.CreateRotationY(-MathHelper.ToRadians(_Error_Y)) * Matrix.CreateRotationZ(MathHelper.ToRadians(_Error_Z)) * Matrix.CreateRotationZ(MathHelper.Pi ) *_UpperBodyTransform * tribrachRotation ;

            _Telescope.Transform = telescopeRotation /* * Matrix.CreateTranslation(0f, 0f, 0f ) */ * _TelescopeTransform ;

            // 一開始 3DsMax Model 的物鏡向著 -Y 的方向 
            // Matrix.CreateTranslation(0f, 0f, 30f) 是上下兩部分的差, 
            // 先不用 _Tribrach.Transform 還無法駕馭
            float _IH;
            if (_Type == InstrumentType.Leveling) 
            {
                  // Leica= 10cm, Nikon = 9.62cm 
                _IH =  5.62f;
            }
            else if (_Type == InstrumentType.Theodolite)
            {
                _IH = 24.0f;
            }
            else
                _IH = 30f;                
                _Camera.Position = (Matrix.CreateTranslation(0f, 0f, 0f) * telescopeRotation * _TelescopeTransform * _UpperBody.Transform * Matrix.CreateTranslation(0f, 0f, _IH ) * _UpperBodyModel.Root.Transform /* * Matrix.CreateTranslation(0f, _WorldYOffset, 0f) */ ).Translation;
                _Camera.Target = (Matrix.CreateTranslation(0f, -100f, 0f) * telescopeRotation * _TelescopeTransform * _UpperBody.Transform * Matrix.CreateTranslation(0f, 0f, _IH ) * _UpperBodyModel.Root.Transform /* * Matrix.CreateTranslation(0f, _WorldYOffset, 0f) */).Translation;

            _Camera.Direction = _Camera.Target - _Camera.Position;
            _Camera.Direction.Normalize();
            _Camera.UpdateCamera();

        }

        public new void Draw()
        {
            if (_UpperBodyModel == null) //model is unloaded or not loaded yet
                return;

            //==Apply matrices to the relevant bones==



            // Set the world matrix as the root transform of the model.
            // Matrix _m = 
            // _UpperBodyModel.Root.Transform = /* Matrix.CreateRotationX(MathHelper.ToRadians(-90)) */ _TripodHeadTransform  *  modelRotation * modelScale * worldTranslation * _world /* * levelTranslation */ ;
            // _UpperBodyModel.Root.Transform =  _LowerBodyModel.Root.Transform; 


            // Look up combined bone matrices for the entire model.
             _UpperBodyModel.CopyAbsoluteBoneTransformsTo(_UpperBodyBoneTransforms);
            //Draw the model.
            // base.Draw();
            foreach (ModelMesh mesh in _UpperBodyModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {

                        effect.World = _UpperBodyBoneTransforms[mesh.ParentBone.Index];
                        // effect.EmissiveColor = new Vector3(0.7f, 0.1f, 0.1f);
                        effect.View = _view;
                        effect.Projection = _projection;
                        effect.EnableDefaultLighting();

                }
                if ( !_HideTelescope || (mesh.ParentBone.Name != "telescope"))                    
                mesh.Draw();
            }
            base.Draw();
        }
    }
}
