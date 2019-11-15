using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SimuSurvey360.Instruments;
namespace SimuSurvey360
{
    public class Entity
    {
        // The tank's model - a fearsome sight.
        // private Model Model;

        public Vector3 Position = Vector3.Zero;
        private Vector3 Velocity = Vector3.Zero;
        private bool _Visible;
        InstrumentType _InsType; 
        Matrix RotationMatrix = Matrix.Identity;
       
        public bool Visible
        {
            get { return _Visible; }
            set { _Visible = value; }

        }

        public InstrumentType InsType
        {
            get { return _InsType; }
            set { _InsType = value; }
        }

        public Entity(/* ContentManager Content,*/  Vector3 inPosition, InstrumentType _Type /* , Vector3 inVelocity  , string ModelPath */ )
        {
            this.Position = inPosition;
            this._InsType = _Type;
            // this.Velocity = inVelocity;

            // Model = Content.Load<Model>(ModelPath);
        }
        /* skc, not used */
        public void Move(GameTime gameTime, float xAmount, float yAmount)
        {
            this.Position.X += xAmount * gameTime.ElapsedGameTime.Milliseconds;
            this.Position.Z += yAmount * gameTime.ElapsedGameTime.Milliseconds;
        }


        public void SetPosition( Vector3 _Position)
        {
            this.Position = _Position; 

        }


        public void Draw(ref Matrix ViewMatrix, ref Matrix ProjectionMatrix)
        {
            /*
            foreach (ModelMesh mesh in this.Model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = RotationMatrix * Matrix.CreateTranslation(Position);
                    effect.View = ViewMatrix;
                    effect.Projection = ProjectionMatrix;

                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;

                    // Set the fog to match the black background color
                    effect.FogEnabled = true;
                    effect.FogColor = Vector3.Zero;
                    effect.FogStart = 1000;
                    effect.FogEnd = 3200;
                }
                mesh.Draw();
            }
             */
        }

        public void Update(GameTime gameTime, Vector3 Position )
        {
            // Move(gameTime, Velocity.X, Velocity.Z);
            // skc, й|е╝н╫зя
            // CheckForWallCollision((heightMapInfo.HeightmapWidth - 1.0f) * 0.5f, (heightMapInfo.HeightmapHeight - 1.0f) * 0.5f);

            // float minimumHeight = 0.0f;

            // heightMapInfo.GetHeight(Position, out minimumHeight);

            // Position.Y = minimumHeight;
        }

        // Checks if entity has passed or hit a wall, and if so, sets their position properly and then reflects them
        // off of the wall.
        public void CheckForWallCollision(float width, float height)
        {
            if (Position.X >= width)
            {
                Velocity.X *= -1;
                Position.X = width;
            }
            else if (Position.X <= -width)
            {
                Velocity.X *= -1;
                Position.X = -width;
            }

            if (Position.Z >= height)
            {
                Velocity.Z *= -1;
                Position.Z = height;
            }
            else if (Position.Z <= -height)
            {
                Velocity.Z *= -1;
                Position.Z = -height;
            }
        }
    }
}
