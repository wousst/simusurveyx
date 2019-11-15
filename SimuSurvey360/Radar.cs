using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SimuSurvey360.Instruments;
namespace SimuSurvey360
{
    public class Radar
    {
        private Texture2D PlayerDotImage;
        private Texture2D EnemyInsDotImage, EnemyDotImage;
        private Texture2D RadarImage;
        Texture2D Dir_1, Dir_2, Dir_3, Dir_4; 
        int _ms, _s; 
        // Local coords of the radar image's center, used to offset image when being drawn
        private Vector2 RadarImageCenter;

        // Distance that the radar can "see"
        private float _RadarRange = 500.0f;
        private float _RadarRangeSquared ; // = _RadarRange * _RadarRange;

        // Radius of radar circle on the screen
        private const float RadarScreenRadius0 = 150.0f; // 100

        // This is the center position of the radar hud on the screen. 
        static Vector2 RadarCenterPos0 = new Vector2(1100, 220); //(1150, 250)
     
        public float RadarRange
        {
            get { return _RadarRange; }
            set { _RadarRange = value; _RadarRangeSquared = value * value; }
        }
        public Radar(ContentManager Content, string playerDotPath, string enemyInsDotPath, string enemyDotPath, string radarImagePath)
        {
            PlayerDotImage = Content.Load<Texture2D>(playerDotPath);
            EnemyInsDotImage = Content.Load<Texture2D>(enemyInsDotPath);
            EnemyDotImage = Content.Load<Texture2D>(enemyDotPath);
            RadarImage = Content.Load<Texture2D>(radarImagePath);

            RadarImageCenter = new Vector2(RadarImage.Width * 0.5f, RadarImage.Height * 0.5f);

            _RadarRangeSquared = _RadarRange * _RadarRange;

            Dir_1 = Content.Load<Texture2D>("Dir_1");
            Dir_2 = Content.Load<Texture2D>("Dir_2");
            Dir_3 = Content.Load<Texture2D>("Dir_3");
            Dir_4 = Content.Load<Texture2D>("Dir_4");
        }

        public void Draw(GameTime gameTime, int Mode, SpriteBatch spriteBatch, float _fangle, Vector3 playerPos, ref List<Entity> enemies)
        {
            // The last parameter of the color determines how transparent the radar circle will be
            Vector2 RadarCenterPos = RadarCenterPos0;
            float RadarScreenRadius ;
            if (Mode == 0)
            {
                RadarCenterPos = RadarCenterPos0;
                RadarScreenRadius = RadarScreenRadius0;
            }
            else
            {
                RadarCenterPos = RadarCenterPos0 + new Vector2( 30, 0);
                RadarScreenRadius = 110 ;
            }


            _ms += gameTime.ElapsedGameTime.Milliseconds;

            if (_ms >= 1000)
            {
                _ms -= 1000;
                _s += 1; 
            }
            int _ims;
            byte _c;
            if ((_s % 2) == 0)
                _ims = ( 128 - 32 ) + ((_ms) * 64 / 1000);
            else
                _ims = (128 + 32) - ((_ms) * 64 / 1000);
            _c = (byte)_ims;
            byte _b = (byte)((int)_c + 60);

            if ( Mode == 0 ) // Navigation Mode Only
            {
                spriteBatch.Draw(Dir_1, RadarCenterPos,
                               null,
                               Color.White,
                               _fangle - MathHelper.PiOver2, new Vector2(0, 0), 1f, SpriteEffects.None, 0);
                spriteBatch.Draw(Dir_2, RadarCenterPos,
                               null,
                               Color.White,
                               _fangle, new Vector2(0, 0), 1f, SpriteEffects.None, 0);
                spriteBatch.Draw(Dir_3, RadarCenterPos,
                               null,
                               Color.White,
                               _fangle + MathHelper.PiOver2, new Vector2(0, 0), 1f, SpriteEffects.None, 0);
                spriteBatch.Draw(Dir_4, RadarCenterPos,
                               null,
                               Color.White,
                               _fangle + MathHelper.Pi, new Vector2(0, 0), 1f, SpriteEffects.None, 0);

            }
           
            spriteBatch.Draw(RadarImage, RadarCenterPos, null, new Color(_b, _b, _b, 100), 0.0f, RadarImageCenter, RadarScreenRadius / (RadarImage.Height * 0.5f), SpriteEffects.None, 0.0f);

            // If enemy is in range
            foreach (Entity thisEnemy in enemies)
            {
                Vector2 diffVect = new Vector2(thisEnemy.Position.X - playerPos.X, thisEnemy.Position.Z - playerPos.Z);
                float distance = diffVect.LengthSquared();

                // Check if enemy is within RadarRange
                if (distance < _RadarRangeSquared)
                {
                    // Scale the distance from world coords to radar coords
                    diffVect *= RadarScreenRadius / _RadarRange;

                    // We rotate each point on the radar so that the player is always facing UP on the radar
                    diffVect = Vector2.Transform(diffVect, Matrix.CreateRotationZ(_fangle));

                    // Offset coords from radar's center
                    diffVect += RadarCenterPos;

                    // We scale each dot so that enemies that are at higher elevations have bigger dots, and enemies
                    // at lower elevations have smaller dots.
                    // float scaleHeight = 1.0f + ((thisEnemy.Position.Y - playerPos.Y) / 200.0f);
                    float scaleHeight = 1.0f; 
                    // Draw enemy dot on radar
                    if (( thisEnemy.InsType == InstrumentType.TotalStation ) || ( thisEnemy.InsType == InstrumentType.Leveling ) ||
                        ( thisEnemy.InsType == InstrumentType.Theodolite ))
                        spriteBatch.Draw(EnemyInsDotImage, diffVect, null, Color.White, 0.0f, new Vector2(0.0f, 0.0f), scaleHeight, SpriteEffects.None, 0.0f);
                    else
                        spriteBatch.Draw(EnemyDotImage, diffVect, null, Color.White, 0.0f, new Vector2(0.0f, 0.0f), scaleHeight, SpriteEffects.None, 0.0f);
                }
            }

            // Draw player's dot last
            spriteBatch.Draw(PlayerDotImage, RadarCenterPos, Color.White);
        }
    }
}
