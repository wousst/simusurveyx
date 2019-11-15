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
    public class InspectionCamera : Microsoft.Xna.Framework.GameComponent
    {
        public float cameraRotationX = 0;  // 相機 X 軸 旋轉角度
        public float cameraRotationY = 0;  // 相機 Y 軸 旋轉角度
        public float cameraDistance = 10;  // 相機 Z 軸 位置
        public Vector3 InspectionPoint = new Vector3(0, 0, 0); // 相機的 注視點

        public Matrix view; // 由 上面 四個變數 算出的 視覺矩陣

        public Matrix projection =        //  投影矩陣
                 Matrix.CreatePerspectiveFieldOfView(
                                  MathHelper.PiOver4,  // 視角 45度
                                  1.333f, // 螢幕 寬高比
                                  1,      // 最近的Z軸截點
                                  1000);  // 最遠的Z軸截點 

        public InspectionCamera(Game game)
             : base(game)
        {
            // TODO: Construct any child components here
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>


        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here
            KeyboardState newState;  // 宣告一個KeyboardState 結構的變數
            newState = Keyboard.GetState();  //得到目前鍵盤每一個按鍵的狀況

            float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (newState.IsKeyDown(Keys.W))   //判斷 W 鍵是否已經被按下
                cameraRotationX += time * 0.1f;

            if (newState.IsKeyDown(Keys.S))   //判斷 S 鍵是否已經被按下
                cameraRotationX -= time * 0.1f;

            if (cameraRotationX > 90.0f)
                cameraRotationX = 90.0f;
            else if (cameraRotationX < -90.0f)
                cameraRotationX = -90.0f;

            if (newState.IsKeyDown(Keys.D))   //判斷 D 鍵是否已經被按下
                cameraRotationY -= time * 0.1f;

            if (newState.IsKeyDown(Keys.A))   //判斷 A 鍵是否已經被按下
                cameraRotationY += time * 0.1f;

            if (newState.IsKeyDown(Keys.X))   //判斷 X 鍵是否已經被按下
                cameraDistance += time * 0.025f;

            if (newState.IsKeyDown(Keys.Z))   //判斷 Z 鍵是否已經被按下
                cameraDistance -= time * 0.025f;

            if (cameraDistance > 400) cameraDistance = 400;
            else if (cameraDistance < 2) cameraDistance = 2;


            if (newState.IsKeyDown(Keys.Space))   //判斷 空白鍵 是否已經被按下
            {
                cameraRotationX = 30;
                cameraRotationY = 0;
                cameraDistance = 200;
            }

            view = Matrix.CreateRotationY(MathHelper.ToRadians(cameraRotationY)) *
                   Matrix.CreateRotationX(MathHelper.ToRadians(cameraRotationX)) *
                   Matrix.CreateLookAt(new Vector3(0, 0, cameraDistance),
                                              InspectionPoint, Vector3.Up);


            // base.Update(gameTime);
        }
    }
}