using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Text;

namespace SimuSurvey360.Instruments
{
    class Level : TwoPartsInstrument
    {
        public Level()
        {
            
        }

        public Level(InstrumentType type)
        {
            _Type = type;
        }

        public void Load(ContentManager content)
        {
            //Load Models from XNB objects and extract the transform matrices
            //==Tripod==
            _LowerBodyModel = content.Load<Model>("SmallTripod");
            _TripodHead = _LowerBodyModel.Bones[0];
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

            //==Upper Body==
            _UpperBodyModel = content.Load<Model>("Nikon");
            _Tribrach = _UpperBodyModel.Bones["tribrach"];
            _Telescope = _UpperBodyModel.Bones["telescope"];
            _UpperBody = _UpperBodyModel.Bones["horizontal_scale"];

            _TribrachTransform = _Tribrach.Transform;
            _TelescopeTransform = _Telescope.Transform;
            _UpperBodyTransform = _UpperBody.Transform;

            // Allocate the transform matrix array.
            _LowerBodyBoneTransforms = new Matrix[_LowerBodyModel.Bones.Count];
            _UpperBodyBoneTransforms = new Matrix[_UpperBodyModel.Bones.Count];
        }

        //Compute Upper Body part
        public new void Update(Matrix world, Matrix view, Matrix projection)
        {
            //Compute World, View and Tripod in advance
            base.Update(world, view, projection);

            //Upper Body
            telescopeRotation = Matrix.CreateRotationX(MathHelper.ToRadians(_TelescopeRotationValue));
            tribrachRotation = Matrix.CreateRotationZ(MathHelper.ToRadians(_TribrachRotationValue));
        }

        public new void Draw()
        {
            if (_UpperBodyModel == null) //model is unloaded or not loaded yet
                return;

            //==Apply matrices to the relevant bones==

            //Upper Body
            //_UpperBody.Transform = _UpperBodyTransform ;
            _Telescope.Transform = _TelescopeTransform * telescopeRotation * tribrachRotation;

            // Set the world matrix as the root transform of the model.
            _UpperBodyModel.Root.Transform = modelScale * modelRotation * worldTranslation * _world;

            // Look up combined bone matrices for the entire model.
            _UpperBodyModel.CopyAbsoluteBoneTransformsTo(_UpperBodyBoneTransforms);
            //Draw the model.
            base.Draw();
            foreach (ModelMesh mesh in _UpperBodyModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = _UpperBodyBoneTransforms[mesh.ParentBone.Index];
                    effect.View = _view;
                    effect.Projection = _projection;
                    effect.EnableDefaultLighting();
                }
                mesh.Draw();
            }
        }
    }
}
