using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Text;

namespace SimuSurvey360.Instruments
{
    class Mirror : Instrument
    {

        #region Fields

        // The XNA framework Model object that we are going to display.
        Model _BodyModel;

        //Bones and their indivisual transformation
        //Don't change the matrix directly, just change the value of properties
        ModelBone _Part1;//lowest part
        ModelBone _Part2;


        Matrix _Part1TransformInit;
        Matrix _Part2TransformInit;



        Matrix _Part1Transform;
        Matrix _Part2Transform;


        private float _Part1Y;
        private float _Part2Y;

        // 中間變化過程使用
        private float _RulerLength; // 伸出長度
        private int _state; // 0.. 3, ruler 伸出狀態

        //The transformation matrix for rendering
        Matrix[] _BodyBoneTransforms;

        //Value controled from outside the class

        //Default Value
        // private const float ROTATION_VALUE = 30;
        // private const float TELESCOPE_ROTATION_VALUE = 0;
        // private const float UBODY_ROTATION_VALUE = 0;
        
        
        private float[] RulerPart_YMAX ;


        protected const float RULER_ROTATION_VALUE = 0;

        protected float _RulerRotationValue;
        protected Matrix rulerRotation;

        // protected float _WorldYOffset; // for Terrain 
        protected Matrix levelTranslation;


        Effect effect0;
        Effect effectPost;

        // Render target
        RenderTarget2D renderTarget;
        Texture2D SceneTexture;

        #endregion


        public float RulerRotationValue
        {
            get { return _RulerRotationValue; }
            set { _RulerRotationValue = value; }
        }

        public Mirror()
        {
        }

        public Mirror(InstrumentType type)
        {
            _Type = type;
            _RulerRotationValue = RULER_ROTATION_VALUE;
            _RulerLength = 0;
            _state = 0; 
            RulerPart_YMAX = new float[4] ;
            // 每段伸出最大長度
            RulerPart_YMAX [0] = 0f;
            RulerPart_YMAX [1] = 55f;

            _Part1Y = _Part2Y = 0f;



        }
        // 外面判斷目前伸出到哪一段
        public int State
        {
            get { return _state; }
            set
            {
                _state = State;
            }
        }

        public int FixedState
        {
            get 
            {
                if ( ( _RulerLength == 0 ) || 
                     ( _RulerLength == RulerPart_YMAX[1] ))
                    return 1; 
                else
                    return 0 ;

            }
        }

        public float RulerLengthMax
        {
            get { return RulerPart_YMAX[1] ; }
        }


        public float RulerLength
        {
            get { return _RulerLength; }
            set
            {
                if ( value <= 0)
                {
                    _Part2Y = 0f ; 
                    _RulerLength = 0 ;
                }
                else if (value <= RulerPart_YMAX[1])
                {
                    _Part2Y = value;
                    _RulerLength = value;

                }
                else
                {
                    _Part2Y = RulerPart_YMAX[1];
                    _RulerLength = RulerPart_YMAX[1];
                }
                _Part1Transform = Matrix.Identity; // 第一節不會伸出
                _Part2Transform = Matrix.CreateTranslation(0f, 0f, _Part2Y);
            
            }
        }

        public void Load(ContentManager content)
        {
            _BodyModel = content.Load<Model>("Mirror");
            _Part1 = _BodyModel.Bones["Cylinder15"];
            _Part2 = _BodyModel.Bones["Cylinder17"];


            _Part1TransformInit = _Part1.Transform; // 第一節不會伸出
            _Part2TransformInit = _Part2.Transform;


            _Part1Transform = Matrix.Identity;
            _Part2Transform = Matrix.Identity;

            // 測試完全伸長
            //_Part1Transform = _Part1.Transform;
            //_Part2Transform = _Part2.Transform * Matrix.CreateTranslation(0f, 0f, RulerPart_YMAX[1]);


            // Allocate the transform matrix array.
              _BodyBoneTransforms = new Matrix[_BodyModel.Bones.Count];
            // _BodyModel.CopyAbsoluteBoneTransformsTo(_BodyBoneTransforms);

            effect0 = content.Load<Effect>("Shader");
            effectPost = content.Load<Effect>("PostProcess");

        }

        public RulerArgs WarpInstrment()
        {
            RulerArgs rulerArgs = new RulerArgs();
            rulerArgs.WorldPosition = this.WorldPosition;
            return rulerArgs;
        }

        public void SetInstrument(RulerArgs args)
        {
            this.WorldPosition = args.WorldPosition;
        }

        public new void Update(Matrix world, Matrix view, Matrix projection)
        {
            //Compute World, View and Tripod in advance
            base.Update(world, view, projection);

            rulerRotation = Matrix.CreateRotationY(MathHelper.ToRadians( _RulerRotationValue ));
            levelTranslation = Matrix.CreateTranslation(0f, _WorldYOffset, 0f);
            
        }

        public void Draw()
        {
            //==Apply matrices to the relevant bones==

             

            _Part1.Transform = _Part1TransformInit * _Part1Transform;
            _Part2.Transform = _Part2TransformInit * _Part2Transform;


            _BodyModel.Root.Transform = Matrix.CreateRotationX(MathHelper.ToRadians(-90)) * Matrix.CreateTranslation(0f, 77f, 0f) * modelRotation * modelScale * rulerRotation * worldTranslation * levelTranslation;// *_world;

            // Look up combined bone matrices for the entire model.
            _BodyModel.CopyAbsoluteBoneTransformsTo(_BodyBoneTransforms);


            foreach (ModelMesh mesh in _BodyModel.Meshes)
            {
               foreach (ModelMeshPart part in mesh.MeshParts)
                {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = _BodyBoneTransforms[mesh.ParentBone.Index];
                    effect.View = _view;
                    effect.Projection = _projection;
                    effect.EnableDefaultLighting();

                    mesh.Draw();
                }
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
