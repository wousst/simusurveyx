using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace SimuSurvey360
{
    public class VisualObject
    {
        string _name;
        float _scale; 
        Matrix ScaleMatrix ;
        protected float _WorldYOffset;          // for Terrain 

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        string _modelName;

        public string ModelName
        {
            get { return _modelName; }
            set { _modelName = value; }
        }

        Model _model;

        public Model Model
        {
            get { return _model; }
            set { _model = value; }
        }

        Matrix _world;

        public Matrix World
        {
            get { return _world; }
            set { _world = value; }
        }


        bool _isVisible;

        public bool IsVisible
        {
            get { return _isVisible; }
            set { _isVisible = value; }
        }

        int _schedule;

        public int Schedule
        {
            get { return _schedule; }
            set { _schedule = value; }
        }

        Texture2D _texture;

        public Texture2D _Texture
        {
            get { return _texture; }
            set { _texture = value; }
        }


        public float WorldYOffset
        {
            get { return _WorldYOffset; }
            set { _WorldYOffset = value; }
        }
        
        public VisualObject(string name, string file, Matrix world, int time, Texture2D texture0, float scale, bool isVisable)
        {
            _name = name;
            _modelName = file;
            _world = world;
            _schedule = time;
            IsVisible = isVisable;
            _texture = texture0;
            _scale = scale;
            ScaleMatrix = Matrix.CreateScale( _scale, _scale, _scale);

        }

        public virtual void Initialize()
        {
     
        }

        public virtual void LoadContent(ContentManager Content)
        {
            _model = Content.Load<Model>(_modelName);
        }

        public virtual void Draw(GameTime gameTime, Camera camera, Vector3 Light, float Alpha )
        {
            if (_isVisible)
            {

                Matrix[] transforms = new Matrix[_model.Bones.Count];

                _model.CopyAbsoluteBoneTransformsTo(transforms);

                foreach (ModelMesh mesh in _model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.World = transforms[mesh.ParentBone.Index] * ScaleMatrix * World * Matrix.CreateTranslation(0f, _WorldYOffset, 0f);
                        effect.View = camera.View;
                        effect.Projection = camera.Projection;
                        effect.EnableDefaultLighting();
                        // effect.SpecularColor = Light;
                        effect.Alpha = Alpha ;



                        
                        if (_texture != null)
                        {
                            effect.Texture = _texture;
                            effect.TextureEnabled = true;
                            effect.SpecularColor = new Vector3(0);
                        }
                        else
                        effect.TextureEnabled = false; 
                    }
                    mesh.Draw();
                }
            }
        }


        public virtual void Update(GameTime gameTime)
        {
            // if (gameTime.TotalGameTime.TotalSeconds > _schedule)
            //     _isVisible = true;
        }

    }
}
