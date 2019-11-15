using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Text;

namespace SimuSurvey360.Instruments
{
    class HintBox : Instrument
    {

        #region Fields

        Model _BodyModel;
        ModelBone _Part1;
        Matrix _Part1TransformInit;
        Matrix _Part1Transform;
        Matrix[] _BodyBoneTransforms;
      
        protected const float RULER_ROTATION_VALUE = 0;

        protected float _ObjRotationValue;
        protected Matrix ObjRotation;

        // protected float _WorldYOffset; // for Terrain 
        protected Matrix levelTranslation;


        Effect effect0;
        Effect effectPost;


        #endregion


        public float ObjRotationValue
        {
            get { return _ObjRotationValue; }
            set { _ObjRotationValue = value; }
        }

        public HintBox()
        {
        }

        public HintBox(InstrumentType type)
        {
            _Type = type;
            _ObjRotationValue = RULER_ROTATION_VALUE;
        }

        public void Load(ContentManager content, int n )
        {
            // _Name.Substring(_Name.Length - 1 , 1);
            _BodyModel = content.Load<Model>(@"Box_" + _Name.Substring(_Name.Length - 1 , 1));
            _Part1 = _BodyModel.Bones["Box01"];

            _Part1TransformInit = _Part1.Transform; 
            _Part1Transform = Matrix.Identity;
            _BodyBoneTransforms = new Matrix[_BodyModel.Bones.Count];
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

            ObjRotation = Matrix.CreateRotationY(MathHelper.ToRadians( _ObjRotationValue ));
            levelTranslation = Matrix.CreateTranslation(0f, _WorldYOffset, 0f);
            
        }

        public void Draw()
        {
            //==Apply matrices to the relevant bones==
 
            _Part1.Transform = _Part1TransformInit * _Part1Transform;
            _BodyModel.Root.Transform = Matrix.CreateTranslation( new Vector3(0f,20f,0f)) * modelRotation * modelScale * ObjRotation * worldTranslation * levelTranslation;// *_world;

            // Look up combined bone matrices for the entire model.
            _BodyModel.CopyAbsoluteBoneTransformsTo(_BodyBoneTransforms);
            foreach (ModelMesh mesh in _BodyModel.Meshes)
            {
               foreach (ModelMeshPart part in mesh.MeshParts)
                {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = _BodyBoneTransforms[mesh.ParentBone.Index];
                    // effect.AmbientLightColor = new Vector3(0.8f, 0.7f, 0.7f);
                    // effect.EmissiveColor = new Vector3(1.0f, 0.2f, 0.3f);

                    // effect.SpecularColor = new Vector3(0.9f, .3f, .1f);

                    float _b = 0.8f;
                    effect.DiffuseColor = new Vector3(_b, _b, _b);
                    effect.AmbientLightColor = new Vector3(_b, _b, _b);
                    effect.LightingEnabled = true; 

                    effect.View = _view;
                    effect.Projection = _projection;
                    // effect.EnableDefaultLighting();
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
