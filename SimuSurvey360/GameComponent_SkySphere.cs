using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;

namespace SimuSurvey360
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class GameComponent_SkySphere // : Microsoft.Xna.Framework.DrawableGameComponent
    {
        Model myModel;  //宣告一個 模型物件 全域變數
        Matrix[] transforms; // 宣告一個 骨架轉換矩陣 全域變數

        public Vector3 Position = new Vector3(0.0f, 10.0f, 0.0f); // 3D 物件的 位置
        public float Yaw = 0.0f;  // 第一人稱的Y軸旋轉角度

        public Matrix View = Matrix.CreateLookAt(new Vector3(0.0f, 20.0f, 20.0f),
                                                      Vector3.Zero,
                                                      Vector3.Up);
        public Matrix Projection = Matrix.CreatePerspectiveFieldOfView(
                                                 MathHelper.ToRadians(45.0f),
                                                 1.333f, 1.0f, 10000.0f);

        public GameComponent_SkySphere( Model model )
            // : base(game)
        {
            // TODO: Construct any child components here
            myModel = model;
            transforms = new Matrix[myModel.Bones.Count];
            myModel.CopyAbsoluteBoneTransformsTo(transforms);
        }



        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            Yaw += elapsedTime * 0.001f;
            // base.Update(gameTime);
        }

        public void Draw()
        {
            foreach (ModelMesh mesh in myModel.Meshes)
            {
                // 設定網格的呈現效果 (世界、觀測、投影矩陣)
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    //effect.LightingEnabled = true;
                    //effect.DiffuseColor = new Vector3(0.3f, 0.3f, 0.3f);
                    //effect.AmbientLightColor = new Vector3 ( 0.3f, 0.3f, 0.3f );
                    effect.World = transforms[mesh.ParentBone.Index] *
                                   Matrix.CreateScale(25.0f) *
                                   Matrix.CreateRotationY(MathHelper.ToRadians(Yaw)); // *
                                   // Matrix.CreateTranslation(0,-10,0) *
                                   // Matrix.CreateTranslation(Position);  //

                    effect.View = View;
                    effect.Projection = Projection;
                }
                // 畫出在 模型 中的 某一個 網格 
                mesh.Draw();
            }
        }
    }
}