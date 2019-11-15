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
    public class GameComponent_Terrain_GetHeight // : Microsoft.Xna.Framework.DrawableGameComponent
    {
        VertexPositionColorTexture[] vertices; // 頂點 陣列
        VertexBuffer vertexBuffer; // 頂點緩衝區
        VertexDeclaration vertexDeclaration; // 頂點格式 (每個頂點 包含什麼內容)

        short[] indices; // 索引 陣列
        IndexBuffer indexBuffer; // 索引緩衝區

        BasicEffect effect; // 基本特效

        GraphicsDevice device; //繪圖設備

        public Matrix world = Matrix.Identity; // 世界 觀測 投影 矩陣

        public Matrix view = Matrix.CreateLookAt(new Vector3(0.0f, 20.0f, 20.0f),
                                                      Vector3.Zero,
                                                      Vector3.Up);

        public Matrix projection = Matrix.CreatePerspectiveFieldOfView(
                                                 MathHelper.ToRadians(45.0f),
                                                 1.333f, 1.0f, 10000.0f);

        Texture2D heightMap;  // 地形高度圖
        public int terrainWidth;  // 地形高度圖的寬
        public int terrainHeight; // 地形高度圖的高
        float[,] heights; // 地形高度 二維陣列

        Texture2D texture; // 地形 紋理圖

        public GameComponent_Terrain_GetHeight(GraphicsDevice _GraphicsDevice, Texture2D heightMap, Texture2D texture)
            // : base(game)
        {
            // TODO: Construct any child components here
            this.device = _GraphicsDevice;

            this.heightMap = heightMap;
            terrainWidth = heightMap.Width;
            terrainHeight = heightMap.Height;

            this.texture = texture;

            effect = new BasicEffect(device, null); // 效果

            Getheights();  // 得到 地形高度 二維陣列
            BuildVertexBuffer(); // 建立 頂點緩衝區
            BuildIndexBuffer();  // 建立 索引緩衝區

            world = Matrix.CreateTranslation(-terrainWidth / 2.0f, 0, -terrainHeight / 2.0f);

        }

        // 得到 地形高度 二維陣列
        private void Getheights()
        {
            Color[] Colors = new Color[terrainWidth * terrainHeight];
            heightMap.GetData(Colors);

            heights = new float[terrainWidth, terrainHeight];

            for (int z = 0; z < terrainHeight; z++)
                for (int x = 0; x < terrainWidth; x++)
                    heights[x, z] = Colors[x + z * terrainWidth].R / 8.0f; // 5.0, 8.0
        }

        // 建立 頂點緩衝區
        private void BuildVertexBuffer()
        {
            // 頂點 陣列
            vertices = new VertexPositionColorTexture[terrainWidth * terrainHeight];
            for (int x = 0; x < terrainWidth; x++)
            {
                for (int z = 0; z < terrainHeight; z++)
                {
                    vertices[x + z * terrainWidth].Position = new Vector3(x, heights[x, z], z);
                    vertices[x + z * terrainWidth].Color = Color.White;
                    vertices[x + z * terrainWidth].TextureCoordinate.X = x / 64.0f;
                    vertices[x + z * terrainWidth].TextureCoordinate.Y = z / 64.0f;
                }
            }

            // 頂點格式
            vertexDeclaration = new VertexDeclaration(device, 
                                                  VertexPositionColorTexture.VertexElements);

            // 建立 頂點緩衝區
            vertexBuffer = new VertexBuffer(device,
                vertices.Length * VertexPositionColorTexture.SizeInBytes,
                BufferUsage.WriteOnly);
            // 將 頂點資料 複製入 頂點緩衝區 內
            vertexBuffer.SetData(vertices);
            // vertexBuffer.SetData<VertexPositionColorTexture>(vertices);

        }

        // 建立 索引緩衝區
        private void BuildIndexBuffer()
        {
            // 索引 陣列
            indices = new short[(terrainWidth - 1) * (terrainHeight - 1) * 6];
            int counter = 0;
            for (int z = 0; z < terrainHeight - 1; z++)
            {
                for (int x = 0; x < terrainWidth - 1; x++)
                {
                    short topLeft = (short)(x + z * terrainWidth);
                    short topRight = (short)((x + 1) + z * terrainWidth);
                    short lowerLeft = (short)(x + (z + 1) * terrainWidth);
                    short lowerRight = (short)((x + 1) + (z + 1) * terrainWidth);

                    indices[counter++] = topLeft;
                    indices[counter++] = lowerRight;
                    indices[counter++] = lowerLeft;

                    indices[counter++] = topLeft;
                    indices[counter++] = topRight;
                    indices[counter++] = lowerRight;
                }
            }

            // 建立 索引緩衝區
            indexBuffer = new IndexBuffer(device,
                indices.Length * 4,
                BufferUsage.WriteOnly, IndexElementSize.SixteenBits);
            // 將 索引資料 複製入 索引緩衝區 內
            indexBuffer.SetData(indices);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>


        public Vector3 GetHeight(Vector3 Pos)
        {
            // 將 要偵測的點 轉回到 地形平移前的 位置
            Vector2 blockPosition = new Vector2(
                Pos.X - world.Translation.X,
                Pos.Z - world.Translation.Z);

            // 檢查 這個點 是否 還落在 地形 的 XZ 平面上
            if (blockPosition.X >= 0 && blockPosition.X < terrainWidth-1 &&
                blockPosition.Y >= 0 && blockPosition.Y < terrainHeight-1 )
            {
                // 算出 這個點 在 頂點 和 頂點之間 的偏移値
                Vector2 blockOffset = new Vector2(
                    blockPosition.X - (int)blockPosition.X,
                    blockPosition.Y - (int)blockPosition.Y);

                // 得到和 這個點 最靠近的上一個 頂點
                int Index_X = (int)blockPosition.X;
                int Index_Y = (int)blockPosition.Y;

                // 得到和靠近的上一個頂點 的斜對角的 下一個頂點
                int Index_X_Next = Index_X + 1 ;
                int Index_Y_Next = Index_Y + 1 ;

                // 得到 落點 所在 的四個 頂點 的 高度
                float height1 = heights[Index_X_Next, Index_Y];
                float height2 = heights[Index_X, Index_Y];
                float height3 = heights[Index_X_Next, Index_Y_Next];
                float height4 = heights[Index_X, Index_Y_Next];

                // 左上三角形
                float IncX, IncY;
                if (blockOffset.X > blockOffset.Y)
                {
                    IncX = height1 - height2;
                    IncY = height3 - height1;
                }
                // 右下三角形
                else
                {
                    IncY = height4 - height2;
                    IncX = height3 - height4;
                }

                // 用 線性 估算出 在格中 的 高度
                Pos.Y = height2 + IncX * blockOffset.X + IncY * blockOffset.Y;
            }

            return Pos;
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update()
        {
            // TODO: Add your update code here

            // base.Update(gameTime);
        }

        public void Draw()
        {
            // 效果 三大矩陣 設定
            effect.World = world;
            effect.View = view;
            effect.Projection = projection;

            effect.VertexColorEnabled = true;  // 使用 頂點顏色 效果

            effect.Texture = texture;
            effect.TextureEnabled = true;

            device.VertexDeclaration = vertexDeclaration; // 頂點格式
            device.Vertices[0].SetSource(vertexBuffer, 0, VertexPositionColorTexture.SizeInBytes); // 頂點來源
            device.Indices = indexBuffer;

            // device.RenderState.FillMode = FillMode.WireFrame;
            
            effect.Begin(); // 效果 開始

            foreach (EffectPass CurrentPass in effect.CurrentTechnique.Passes)
            {
                CurrentPass.Begin();

                device.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                    0, 0,
                    vertices.Length, // 頂點數目
                    0,
                    indices.Length / 3); // 三角形 數目

                CurrentPass.End();
            }
            effect.End();
        }
    }
}