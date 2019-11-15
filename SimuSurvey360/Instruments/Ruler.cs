using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Text;

namespace SimuSurvey360.Instruments
{
    class Ruler : Instrument
    {

        #region Fields

        // The XNA framework Model object that we are going to display.
        Model _BodyModel;

        //Bones and their indivisual transformation
        //Don't change the matrix directly, just change the value of properties
        ModelBone _Part1;//lowest part
        ModelBone _Part2;
        ModelBone _Part3;
        ModelBone _Part4;//topest part

        Matrix _Part1TransformInit;
        Matrix _Part2TransformInit;
        Matrix _Part3TransformInit;
        Matrix _Part4TransformInit;


        Matrix _Part1Transform;
        Matrix _Part2Transform;
        Matrix _Part3Transform;
        Matrix _Part4Transform;

        private float _Part1Y;
        private float _Part2Y;
        private float _Part3Y;
        private float _Part4Y;
        // 中間變化過程使用
        private float _RulerLength; // 伸出長度
        private int _state; // 0.. 3, ruler 伸出狀態

        //The transformation matrix for rendering
        Matrix[] _BodyBoneTransforms;

        //Value controled from outside the class

        //Default Value
        private const float ROTATION_VALUE = 30;
        private const float TELESCOPE_ROTATION_VALUE = 0;
        private const float UBODY_ROTATION_VALUE = 0;
        
        
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

        public Ruler()
        {
        }

        public Ruler(InstrumentType type)
        {
            _Type = type;
            _RulerRotationValue = RULER_ROTATION_VALUE;
            _RulerLength = 0;
            _state = 0; 
            RulerPart_YMAX = new float[4] ;
            // 每段伸出最大長度
            RulerPart_YMAX [0] = 0f;
            RulerPart_YMAX [1] = 113.25f;
            RulerPart_YMAX [2] = 115.848f;
            RulerPart_YMAX [3] = 116.601f;
            _Part1Y = _Part2Y = _Part3Y = _Part4Y = 0f;



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
                     ( _RulerLength == RulerPart_YMAX[1] ) ||
                     ( _RulerLength == (RulerPart_YMAX[1] + RulerPart_YMAX[2]) ) ||
                     ( _RulerLength == (RulerPart_YMAX[1] + RulerPart_YMAX[2] + RulerPart_YMAX[3]) ))
                    return 1; 
                else
                    return 0 ;

            }
        }

        public float RulerLengthMax
        {
            get { return RulerPart_YMAX[1] + RulerPart_YMAX[2] + RulerPart_YMAX[3]; }
        }


        public float RulerLength
        {
            get { return _RulerLength; }
            set
            {
                if ( value <= 0)
                {
                    _Part2Y = _Part3Y = _Part4Y = 0f ; 
                    _RulerLength = 0 ;
                }
                else if (value <= RulerPart_YMAX[1])
                {
                    _Part2Y = value;
                     _Part3Y = _Part4Y = 0f;
                     _RulerLength = value;

                }
                else if (value <= (RulerPart_YMAX[1] + RulerPart_YMAX[2]))
                {
                    _Part2Y = RulerPart_YMAX[1];
                    _Part3Y = value - RulerPart_YMAX[1];
                    _Part4Y = 0f;
                    _RulerLength = value;                     
                }
                else if (RulerLength <= (RulerPart_YMAX[1] + RulerPart_YMAX[2] + RulerPart_YMAX[3]))
                {
                    _Part2Y = RulerPart_YMAX[1];
                    _Part3Y = RulerPart_YMAX[2];
                    _Part4Y = value - (RulerPart_YMAX[1] + RulerPart_YMAX[2]);
                    _RulerLength = value;
                }
                else
                {
                    _Part2Y = RulerPart_YMAX[1];
                    _Part3Y = RulerPart_YMAX[2];
                    _Part4Y = RulerPart_YMAX[3];
                    _RulerLength = RulerPart_YMAX[1] + RulerPart_YMAX[2] + RulerPart_YMAX[3] ;
                }
                _Part1Transform = Matrix.Identity; // 第一節不會伸出
                _Part2Transform = Matrix.CreateTranslation(0f, 0f, _Part2Y);
                _Part3Transform = Matrix.CreateTranslation(0f, 0f, _Part3Y);
                _Part4Transform = Matrix.CreateTranslation(0f, 0f, _Part4Y);
            
            }
        }

        public void Load(ContentManager content)
        {
            _BodyModel = content.Load<Model>("Ruler_5m_4");
            _Part1 = _BodyModel.Bones["Box16"];
            _Part2 = _BodyModel.Bones["Box18"];
            _Part3 = _BodyModel.Bones["Box28"];
            _Part4 = _BodyModel.Bones["Box19"];

            _Part1TransformInit = _Part1.Transform; // 第一節不會伸出
            _Part2TransformInit = _Part2.Transform;
            _Part3TransformInit = _Part3.Transform;
            _Part4TransformInit = _Part4.Transform;

            _Part1Transform = Matrix.Identity;
            _Part2Transform = Matrix.Identity;
            _Part3Transform = Matrix.Identity;
            _Part4Transform = Matrix.Identity;
            // 測試標尺完全伸長
            //_Part1Transform = _Part1.Transform;
            //_Part2Transform = _Part2.Transform * Matrix.CreateTranslation(0f, 0f, RulerPart_YMAX[1]);
            //_Part3Transform = _Part3.Transform * Matrix.CreateTranslation(0f, 0f, RulerPart_YMAX[2]);
            //_Part4Transform = _Part4.Transform * Matrix.CreateTranslation(0f, 0f, RulerPart_YMAX[3]);

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

            /*
            // Calculate matrices based on the current animation position.
            //Translation Part
            Matrix worldTranslation;        //Translate the instrument to _worldPostion from origin
            Matrix levelTranslation;         //Translate the body with respect to rotation value and length of the tripods
            Matrix tripodTranslation;       //Translate the lower legs of tripod to desired length
            float tripodTranslationValue;

            //Rotation Part
            Matrix tripodRotation;         //Rotate the legs of tripod to desired degree
            Matrix telescopeRotation;  //Rotate the telescope part
            Matrix UbodyRotation;        //Rotate the Upper Body
            Matrix modelRotation;        //Rotate the whole model 90 degree along X axis

            //Scaling Part
            Matrix modelScale;            //Scale down the model for MODEL_SCALE_VALUE

            //Model import
            modelRotation = Matrix.CreateRotationX(MathHelper.ToRadians(-90));
            modelScale = Matrix.CreateScale((1 / _modelScaleValue), (1 / _modelScaleValue), (1 / _modelScaleValue));

            //Main Body's Level
            tripodRotation = Matrix.CreateRotationX(MathHelper.ToRadians(-_rotationValue));
            tripodTranslationValue = (float)((_lowerLegLength + _upperLegLength) * (Math.Cos((double)MathHelper.ToRadians(_rotationValue))));
            levelTranslation = Matrix.CreateTranslation(0, tripodTranslationValue, 0);
            _level = tripodTranslationValue;

            //Tripod
            tripodTranslationValue = _lowerLegLength;
            tripodTranslation = Matrix.CreateTranslation(0, 0, -_lowerLegLength * _modelScaleValue);

            // Apply matrices to the relevant bones.
            _Leg1_1.Transform = _Leg1_1Transform * tripodRotation;
            _Leg1_2.Transform = _Leg1_2Transform * tripodTranslation * tripodRotation;
            _Leg2_1.Transform = _Leg2_1Transform * tripodRotation;
            _Leg2_2.Transform = _Leg2_2Transform * tripodTranslation * tripodRotation;
            _Leg3_1.Transform = _Leg3_1Transform * tripodRotation;
            _Leg3_2.Transform = _Leg3_2Transform * tripodTranslation * tripodRotation;

            //Upper Body
            telescopeRotation = Matrix.CreateRotationX(MathHelper.ToRadians(_telescopeRotationValue));
            UbodyRotation = Matrix.CreateRotationZ(MathHelper.ToRadians(_UbodyRotationValue));

            // Apply matrices to the relevant bones.
            _Ubody.Transform = _UbodyTransform * UbodyRotation;
            _telescope.Transform = _telescopeTransform * telescopeRotation;

            //World Coordinare
            worldTranslation = Matrix.CreateTranslation(_worldPosition.X, _worldPosition.Y, _worldPosition.Z);

            // Set the world matrix as the root transform of the model.
            _model.Root.Transform = modelScale * modelRotation * worldTranslation * world * levelTranslation;
            _Umodel.Root.Transform = modelScale * modelRotation * worldTranslation * world * levelTranslation;

            */
            // Look up combined bone matrices for the entire model.
            
        }

        public void Draw()
        {
            //==Apply matrices to the relevant bones==

             

            _Part1.Transform = _Part1TransformInit * _Part1Transform;
            _Part2.Transform = _Part2TransformInit * _Part2Transform;
            _Part3.Transform = _Part3TransformInit * _Part3Transform;
            _Part4.Transform = _Part4TransformInit * _Part4Transform;


            //Body
            //_UpperBody.Transform = _UpperBodyTransform ;

            // Set the world matrix as the root transform of the model.
            _BodyModel.Root.Transform = /* Matrix.CreateRotationX(MathHelper.ToRadians(-90)) * */ modelRotation * modelScale * rulerRotation * worldTranslation * levelTranslation;// *_world;

            // Look up combined bone matrices for the entire model.
            _BodyModel.CopyAbsoluteBoneTransformsTo(_BodyBoneTransforms);
/* 原始版本
            foreach (ModelMesh mesh in _BodyModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = _BodyBoneTransforms[mesh.ParentBone.Index];
                    effect.View = _view;
                    effect.Projection = _projection;
                    effect.EnableDefaultLighting();
                }
                effectPost.Begin();
                effectPost.CurrentTechnique.Passes[0].Begin();
                mesh.Draw();
                effectPost.CurrentTechnique.Passes[0].End();
                effectPost.End();
            }

*/




            //effect0.CurrentTechnique = effect0.Techniques["GlossMap"];
            //effect0.Begin();


            //foreach (EffectPass pass in effect0.CurrentTechnique.Passes)
            //{
            //    pass.Begin();
            // Draw the model.
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
                    float _b = 0.5f;
                    effect.DiffuseColor = new Vector3(_b, _b, _b);
                    effect.AmbientLightColor = new Vector3(_b, _b, _b);
                    // effect.SpecularColor = new Vector3(0.7f, 0.7f, 0.7f);
                    // effect.SpecularPower = 0.7f; 
                    effect.LightingEnabled = true; 



                    // Render our meshpart

                    //graphics.GraphicsDevice.Vertices[0].SetSource(mesh.VertexBuffer, part.StreamOffset, part.VertexStride);
                    //graphics.GraphicsDevice.Indices = mesh.IndexBuffer;
                    //graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                    //                                              part.BaseVertex, 0, part.NumVertices,
                    //                                              part.StartIndex, part.PrimitiveCount);
                    
                }
                
                }
               mesh.Draw();
            }
            //pass.End();
            //}



            //effectPost.Begin();
            //effectPost.CurrentTechnique.Passes[0].Begin();
            
            //effectPost.CurrentTechnique.Passes[0].End();
            //effectPost.End();


        }
        public float WorldYOffset
        {
            get { return _WorldYOffset; }
            set { _WorldYOffset = value; }
        }
       
    }
}
