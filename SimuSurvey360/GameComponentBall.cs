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
    public class GameComponentBall // : Microsoft.Xna.Framework.DrawableGameComponent
    {
        Model myModel;  //宣告一個 模型物件 全域變數
        Matrix[] transforms; // 宣告一個 骨架轉換矩陣 全域變數

        public Vector3 Position = new Vector3(200,0,0);  // 位置 (50,10,0 ) for Ruler with Tree
        
        public Matrix view = Matrix.CreateLookAt(new Vector3(0.0f, 20.0f, 20.0f),
                                                      Vector3.Zero,
                                                      Vector3.Up);

        public Matrix projection = Matrix.CreatePerspectiveFieldOfView(
                                                 MathHelper.ToRadians(45.0f),
                                                 1.333f, 1.0f, 10000.0f);

        public GameComponentBall(GraphicsDevice _GraphicsDevice, Model model)
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
        public void Update()
        {
            // TODO: Add your update code here
            // float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            KeyboardState newState;  // 宣告一個KeyboardState 結構的變數
            newState = Keyboard.GetState();  //得到目前鍵盤每一個按鍵的狀況
            
            //if (newState.IsKeyDown(Keys.Right))   //判斷Up鍵是否已經被按下
            //    Position.X += 1;

            //if (newState.IsKeyDown(Keys.Left))  //判斷Down鍵是否已經被按下
            //    Position.X -= 1;

            //if (newState.IsKeyDown(Keys.Up))   //判斷Up鍵是否已經被按下
            //    Position.Z -= 1;

            //if (newState.IsKeyDown(Keys.Down))  //判斷Down鍵是否已經被按下
            //    Position.Z += 1;

            // base.Update(gameTime);
        }

        //protected override void Draw(GameTime gameTime)
        public void Draw()
        {
            //Position.Y = 30;

            foreach (ModelMesh mesh in myModel.Meshes)
            {
                // 設定網格的呈現效果 (世界、觀測、投影矩陣)
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index] *
                                   Matrix.CreateTranslation(Position);  
                    effect.View = view;
                    effect.Projection = projection;
                }
                // 畫出在 模型 中的 某一個 網格 
                mesh.Draw();
            }
        }
    }
}