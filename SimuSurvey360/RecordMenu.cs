using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using SpriteSheetRuntime;

namespace SimuSurvey360
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class recordMenu : Microsoft.Xna.Framework.DrawableGameComponent
    {

        SystemState State; 

        SpriteBatch spriteBatch;
        SpriteFont spriteFont;
        SpriteFont spriteFontSmall;
        SpriteSheet spriteSheet;
        SpriteSheet recordMenuSheet;


        private float fKeyPressCheckDelay = 0.25f;
        private float fTotalElapsedTime = 0;
        private int ghMenu_exitstate = -1;
        // private Texture2D tMenuBk, tSelector;
        private Rectangle ghMenu_DestRect = new Rectangle(0,0,1280,720);

        private ContentManager ghMenu_Content;
        const int Item_Ns = 5 ;
        static int _SelectedItem = 0;

        Vector2 Item_StartPos = new Vector2(380, 600);
        Vector2 Item_Distance = new Vector2(110, 0);
        Texture2D Bk;
        Vector3[] _PosR = new Vector3[4];
        Vector3[] _PosP = new Vector3[4];
        Vector3[] _PosM = new Vector3[4];
        Vector4 _PosI; // 第四個是儀器高度
        int Y_DispOffset;





        /* 依據 State.TrainingN */
        // 0: 水準測量: TP1, TP2, 分別顯示後視, 前視, 相減高程差, 最後結果
        //    外面要傳  TP1, TP2 位置進來

        // 1: 水平角測量: A 點正倒鏡, B 點正倒鏡, 最後 < APB

        // 2. 垂直角: B 點對A 點天頂距 正鏡,倒鏡
        //            B 點對C 點2M 處 天頂距 正鏡,倒鏡
        //            B 點對C 點3M 處 天頂距 正鏡,倒鏡
        //            以B 點看, A點 和C點的水平角 正倒鏡, 平均, 最後 < ABC
        //            AB 距離, BC 水平距離
        //            C 點座標 N, E
        //            C 點高程 Hc
        // 事先給定 A點 N, E, H, Ia, B點 N, E, 




        public Vector4 PosI
        {
            get { return _PosI; }
            set { _PosI = value; }
        }


        public Vector3 PosR1
        {
            get { return _PosR[0]; }
            set { _PosR[0] = value; }
        }

        public Vector3 PosR2
        {
            get { return _PosR[1]; }
            set { _PosR[1] = value; }
        }

        public Vector3 PosR3
        {
            get { return _PosR[2]; }
            set { _PosR[2] = value; }
        }

        public Vector3 PosR4
        {
            get { return _PosR[3]; }
            set { _PosR[3] = value; }
        }

        public Vector3 PosP1
        {
            get { return _PosP[0]; }
            set { _PosP[0] = value; }
        }

        public Vector3 PosP2
        {
            get { return _PosP[1]; }
            set { _PosP[1] = value; }
        }

        public Vector3 PosP3
        {
            get { return _PosP[2]; }
            set { _PosP[2] = value; }
        }

        public Vector3 PosP4
        {
            get { return _PosP[3]; }
            set { _PosP[3] = value; }
        }


        public Vector3 PosM1
        {
            get { return _PosM[0]; }
            set { _PosM[0] = value; }
        }

        public Vector3 PosM2
        {
            get { return _PosM[1]; }
            set { _PosM[1] = value; }
        }

        public Vector3 PosM3
        {
            get { return _PosM[2]; }
            set { _PosM[2] = value; }
        }

        public Vector3 PosM4
        {
            get { return _PosM[3]; }
            set { _PosM[3] = value; }
        }
        private PlayerIndex ghMenu_CurrentPlayer = PlayerIndex.One;

        /// <summary>
        /// Returns the method the user used to exit the ghMenu.  If -1, the ghMenu hasn't
        /// yet been displayed.  0=ghMenu is Active, 1=User exited via B/Enter (Done), 2=User exited via 
        /// Back/Escape (Abort)
        /// </summary>
        public int ExitState
        {
            get { return ghMenu_exitstate; }
            set { ghMenu_exitstate = value; }
        }

        /// <summary>
        /// Get or Set the current Player for gamepad input.
        /// </summary>
        public int SelectedItem
        {
            get { return _SelectedItem; }
            set { _SelectedItem = value; }
        }

        public class MenuItemStru
        {
            public string Text;
            public Texture2D Picture;
            public Vector2 Location;
        };

        MenuItemStru[] MenuItem;   

        public recordMenu(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
        }

        public recordMenu(SystemState _State, Game game)
            : base(game)
        {
            State = _State;
            Y_DispOffset = 0; 
        }
        

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here
            ghMenu_Content = new ContentManager( Game.Services );
            ghMenu_Content.RootDirectory = "Content";
            MenuItem = new MenuItemStru[ Item_Ns ];
            for (int i = 0; i < Item_Ns; i++)
                MenuItem[i] = new MenuItemStru() ; 

            MenuItem[0].Text = @"1. Leveling";
            // MenuItem[0].Picture = ContentReader 
            MenuItem[1].Text = @"2. Theodolite";
            MenuItem[2].Text = @"3. Total Station";
            MenuItem[3].Text = @"4. The Fourth Scenario";
            MenuItem[4].Text = @"5. The Fifth Scenario";

            Bk = ghMenu_Content.Load<Texture2D>("record");


            base.Initialize();
        }

        protected override void LoadGraphicsContent(bool loadAllContent)
        {

            if (loadAllContent)
            {
                spriteBatch = new SpriteBatch(GraphicsDevice);
                // spriteFont = ghMenu_Content.Load<SpriteFont>("Arial24");
                // spriteFontSmall = ghMenu_Content.Load<SpriteFont>("LCD18");
                spriteFont = ghMenu_Content.Load<SpriteFont>("LCD18");
                spriteSheet = ghMenu_Content.Load<SpriteSheet>(@"SpriteSheet\SpriteSheet");
                recordMenuSheet = ghMenu_Content.Load<SpriteSheet>(@"ghmenu\ghMenu");

            }
            base.LoadGraphicsContent(loadAllContent);
        }

        protected void Update_Entry(GamePadState GPad, KeyboardState KBoard, GameTime gameTime)
        {
            ghMenu_exitstate = 0;
            // First, handle moving left or right
            if ((GPad.ThumbSticks.Left.X < -0.25f) || (GPad.DPad.Left == ButtonState.Pressed) || KBoard.IsKeyDown(Keys.Left) )
            {
                if ( _SelectedItem > 0 )
                     _SelectedItem -= 1;

                fTotalElapsedTime = 0.0f;
            }
            if ((GPad.ThumbSticks.Left.X > 0.25f) || (GPad.DPad.Right == ButtonState.Pressed) || KBoard.IsKeyDown(Keys.Right))
            {
                if ( _SelectedItem < Item_Ns - 1 )
                _SelectedItem += 1;
                fTotalElapsedTime = 0.0f;
            }



            // Up, Down 以後擴充
            if (GPad.ThumbSticks.Left.Y < -0.25f || (GPad.DPad.Down == ButtonState.Pressed) || KBoard.IsKeyDown(Keys.Down))
            {
                fTotalElapsedTime = 0.0f;
            }
            else if (GPad.ThumbSticks.Left.Y > 0.25f || (GPad.DPad.Up == ButtonState.Pressed) || KBoard.IsKeyDown(Keys.Up))
            {
                fTotalElapsedTime = 0.0f;
            }





            //  選定
            //if (GPad.Buttons.A == ButtonState.Pressed)
            //{
            //    ghMenu_exitstate = 1;
            //    this.Enabled = false;
            //    this.Visible = false;

            //}
            // B 放棄, 先當作 Start 用
            else if (GPad.Buttons.B == ButtonState.Pressed || KBoard.IsKeyDown(Keys.Escape) || KBoard.IsKeyDown(Keys.P) )
            {
                ghMenu_exitstate = 2;
                this.Enabled = false;
                this.Visible = false;
            }
            // Start 離開
            else if (GPad.Buttons.Start == ButtonState.Pressed)
            {
                ghMenu_exitstate = 1;
                this.Enabled = false;
                this.Visible = false;
            }


            //if (GPad.ThumbSticks.Left.Y != 0)
            //{
            //    int tDisp;

            //    tDisp = Y_DispOffset + (int)(GPad.ThumbSticks.Left.Y) * -20;


            //    if ((tDisp > 0) && (tDisp < 360))
            //    {
            //        Y_DispOffset = tDisp;
            //    }
            //    else if (tDisp < 0)
            //        Y_DispOffset = 0;
            //    else if(tDisp > 360)
            //        Y_DispOffset = 360;
            //}

            //if (KBoard.IsKeyDown(Keys.Up))
            //{
            //    if ( Y_DispOffset >= 40 )
            //        Y_DispOffset -= 40;
            //    else
            //        Y_DispOffset = 0;
            //}
            //else if (KBoard.IsKeyDown(Keys.Down))
            //{
            //    if ( Y_DispOffset <= 320 )
            //        Y_DispOffset += 40;
            //    else
            //        Y_DispOffset = 360 ;
            //}


        }

        protected void Draw_Entry(SpriteBatch sbBatch)
        {




        }


        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            fTotalElapsedTime += elapsed;

            if (fTotalElapsedTime > fKeyPressCheckDelay)
            {
                Update_Entry(GamePad.GetState(ghMenu_CurrentPlayer), Keyboard.GetState(), gameTime);
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            spriteBatch.Draw(Bk, new Vector2(0, 0),
                         new Rectangle(0, Y_DispOffset, 1280, 720),
                          Color.White,
                          0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0);
            Color _c = Color.Black ; 
            // Draw_Entry(spriteBatch);
            String TmpStr;
            int x_pos = 50;
            int y_pos = 50;
            int y_line = 30;
            float _dx;
            float _dz;
            float _a0, _a1,_angle;
            float _AB, _BC, _CB, _CD, _DA;
            Vector3 _sP01, _sP02, _sP03, _sP04;
            int R_Ns = 0 ; 
            for (int i = 0; i < State.Ruler_Ns; i++)
            {
                // if ( State.RulerType == 0 )
                    TmpStr = "Ruler" + (i+1).ToString() ; 
                // else
                //     TmpStr = "Pole" + (i + 1).ToString(); 
                spriteBatch.DrawString(spriteFont, TmpStr,
                                                       new Vector2(x_pos, y_pos), _c);
                y_pos += y_line;
                TmpStr = System.String.Format("N: {0:f3}M", _PosR[i].X * 4.0f / 100);
                spriteBatch.DrawString(spriteFont, TmpStr,
                                                       new Vector2(x_pos, y_pos), _c);
                y_pos += y_line; 
                TmpStr = System.String.Format("E: {0:f3}M", _PosR[i].Y * 4.0f / 100);
                spriteBatch.DrawString(spriteFont, TmpStr,
                                                       new Vector2(x_pos, y_pos), _c);
                y_pos += y_line; 
                TmpStr = System.String.Format("Z: {0:f3}M", (_PosR[i].Z * 4.0f + State.BaseHeight) / 100);
                spriteBatch.DrawString(spriteFont, TmpStr,
                                                       new Vector2(x_pos, y_pos), _c);
                y_pos += y_line; y_pos += y_line;
                
                if (++R_Ns == 4)
                {
                    x_pos += 200;
                    y_pos = 50;
                    R_Ns = 0;
                }

            }

            for (int i = 0; i < State.Pole_Ns; i++)
            {

                TmpStr = "Pole" + (i + 1).ToString(); 
                spriteBatch.DrawString(spriteFont, TmpStr,
                                                       new Vector2(x_pos, y_pos), _c);
                y_pos += y_line; 
                TmpStr = System.String.Format("N: {0:f3}M", _PosP[i].X * 4.0f / 100);
                spriteBatch.DrawString(spriteFont, TmpStr,
                                                       new Vector2(x_pos, y_pos), _c);
                y_pos += y_line; 
                TmpStr = System.String.Format("E: {0:f3}M", _PosP[i].Y * 4.0f / 100);
                spriteBatch.DrawString(spriteFont, TmpStr,
                                                       new Vector2(x_pos, y_pos), _c);
                y_pos += y_line; 
                TmpStr = System.String.Format("Z: {0:f3}M", (_PosP[i].Z * 4.0f + State.BaseHeight) / 100);
                spriteBatch.DrawString(spriteFont, TmpStr,
                                                       new Vector2(x_pos, y_pos), _c);
                y_pos += y_line; y_pos += y_line;
                if (++R_Ns == 4)
                {
                    x_pos += 200;
                    y_pos = 50;
                    R_Ns = 0;
                }
            }

            for (int i = 0; i < State.Mirror_Ns; i++)
            {

                TmpStr = "Mirror" + (i + 1).ToString();
                spriteBatch.DrawString(spriteFont, TmpStr,
                                                       new Vector2(x_pos, y_pos), _c);
                y_pos += y_line;
                TmpStr = System.String.Format("N: {0:f3}M", _PosM[i].X * 4.0f / 100);
                spriteBatch.DrawString(spriteFont, TmpStr,
                                                       new Vector2(x_pos, y_pos), _c);
                y_pos += y_line;
                TmpStr = System.String.Format("E: {0:f3}M", _PosM[i].Y * 4.0f / 100);
                spriteBatch.DrawString(spriteFont, TmpStr,
                                                       new Vector2(x_pos, y_pos), _c);
                y_pos += y_line;
                TmpStr = System.String.Format("Z: {0:f3}M", (_PosM[i].Z * 4.0f + State.BaseHeight) / 100);
                spriteBatch.DrawString(spriteFont, TmpStr,
                                                       new Vector2(x_pos, y_pos), _c);
                y_pos += y_line; y_pos += y_line;
                if (++R_Ns == 4)
                {
                    x_pos += 200;
                    y_pos = 50;
                    R_Ns = 0;
                }
            }

                        x_pos += 200;
                        y_pos = 50;
                        R_Ns = 0;
                        for (int i = 0; i < State.SetPoint_Ns ; i++)
                        {
                            string _A = ((char)(65+i)).ToString(); 
                            TmpStr = "Point " + _A ;// ; (i + 1).ToString();
                            spriteBatch.DrawString(spriteFont, TmpStr,
                                                                   new Vector2(x_pos, y_pos), _c);
                            y_pos += y_line;
                            TmpStr = System.String.Format("N: {0:f3}M", ( -State.getSetPoint_Position(i+1).Z + State.BaseNE ) * 4f / 100f);
                            spriteBatch.DrawString(spriteFont, TmpStr,
                                                                   new Vector2(x_pos, y_pos), _c);
                            y_pos += y_line;
                            TmpStr = System.String.Format("E: {0:f3}M", ( State.getSetPoint_Position(i + 1).X  + State.BaseNE ) * 4f / 100f);
                            spriteBatch.DrawString(spriteFont, TmpStr,
                                                                   new Vector2(x_pos, y_pos), _c);
                            y_pos += y_line;
                            TmpStr = System.String.Format("Z: {0:f3}M", (State.getSetPoint_Position(i + 1).Y * 4f + State.BaseHeight) / 100f);
                            spriteBatch.DrawString(spriteFont, TmpStr,
                                                                   new Vector2(x_pos, y_pos), _c);
                            y_pos += y_line;
                            y_pos += y_line;
                            if (++R_Ns == 4)
                            {
                                x_pos += 200;
                                y_pos = 50;
                                R_Ns = 0;
                            }
                        }

                        if (State.SetPoint0_Enabled)
                        {
                
                            TmpStr = "Point P" ;
                            spriteBatch.DrawString(spriteFont, TmpStr,
                                                                   new Vector2(x_pos, y_pos), _c);
                            y_pos += y_line;
                            TmpStr = System.String.Format("N: {0:f3}M", ( -State.getSetPoint_Position(0).Z + State.BaseNE ) * 4f / 100f);
                            spriteBatch.DrawString(spriteFont, TmpStr,
                                                                   new Vector2(x_pos, y_pos), _c);
                            y_pos += y_line;
                            TmpStr = System.String.Format("E: {0:f3}M",( State.getSetPoint_Position(0).X + State.BaseNE )  * 4f / 100f);
                            spriteBatch.DrawString(spriteFont, TmpStr,
                                                                   new Vector2(x_pos, y_pos), _c);
                            y_pos += y_line;
                            TmpStr = System.String.Format("Z: {0:f3}M", (State.getSetPoint_Position(0).Y * 4f + State.BaseHeight) / 100f);
                            spriteBatch.DrawString(spriteFont, TmpStr,
                                                                   new Vector2(x_pos, y_pos), _c);
                            y_pos += y_line;
                            y_pos += y_line;
                            if (++R_Ns == 4)
                            {
                                x_pos += 200;
                                y_pos = 50;
                                R_Ns = 0;
                            }

                        }
                        x_pos += 200;
                        y_pos = 50;
                        R_Ns = 0;

                        TmpStr = "Station";
                        spriteBatch.DrawString(spriteFont, TmpStr,
                                                               new Vector2(x_pos, y_pos), _c);
                        y_pos += y_line;
                        TmpStr = System.String.Format("N: {0:f3}M", _PosI.X * 4.0f /100.0f );
                        spriteBatch.DrawString(spriteFont, TmpStr,
                                                               new Vector2(x_pos, y_pos), _c);
                        y_pos += y_line;
                        TmpStr = System.String.Format("E: {0:f3}M", _PosI.Y * 4.0f / 100.0f);
                        spriteBatch.DrawString(spriteFont, TmpStr,
                                                               new Vector2(x_pos, y_pos), _c);
                        y_pos += y_line;
                        TmpStr = System.String.Format("Z: {0:f3}M", (_PosI.Z * 4.0f + State.BaseHeight) / 100.0f);
                        spriteBatch.DrawString(spriteFont, TmpStr,
                                                               new Vector2(x_pos, y_pos), _c);
                        y_pos += y_line;
                        TmpStr = System.String.Format("H: {0:f3}M", _PosI.W * 4.0f / 100.0f);
                        spriteBatch.DrawString(spriteFont, TmpStr,
                                                               new Vector2(x_pos, y_pos), _c);


                        _sP01 = State.getSetPoint_Position(1);
                        _sP02 = State.getSetPoint_Position(2);
                        _sP03 = State.getSetPoint_Position(3);
                        _sP04 = State.getSetPoint_Position(4);


                        x_pos += 200;
                        y_pos = 50;
                        R_Ns = 0;

                        switch (State.TrainingN)
                        {
                            case 0:



                                TmpStr = "Elavation Difference(A,B)";
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                TmpStr = System.String.Format("= {0:f3}M", ((State.getSetPoint_Position(2).Y - State.getSetPoint_Position(1).Y ) * 4f ) / 100f);
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;


                                break;



                            case 1:



                                _dx = State.getSetPoint_Position(1).X - State.getSetPoint_Position(0).X;
                                _dz = State.getSetPoint_Position(1).Z - State.getSetPoint_Position(0).Z;
                                _a0 = (float)MathHelper.ToDegrees((float)Math.Atan(_dz / _dx));
                                if (_dz < 0)
                                    _a0 += 180;
                                _dx = State.getSetPoint_Position(2).X - State.getSetPoint_Position(0).X;
                                _dz = State.getSetPoint_Position(2).Z - State.getSetPoint_Position(0).Z;
                                _a1 = (float)MathHelper.ToDegrees((float)Math.Atan(_dz / _dx));
                                if (_dz < 0)
                                    _a1 += 180;
                                _angle = _a1 > _a0?_a1-_a0:_a0-_a1;
                               

                                TmpStr = "Horizontal Angle";
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                State.DegreeToDMS_String(_angle, out TmpStr);
                                // TmpStr = System.String.Format("APB = {0:D2}.{1:D2}'{2:D2}\"", 100, 10, 10);
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;


                                break; 

                            case 2:



                                // A 點
                                // A 頂點 
                                // 如果用標尺
                                // float _Th = _sP01.Y + 154.4f/4f ; 
                                // 標覘
                                float _Th = _sP01.Y + 116.4f / 4f;
                                float _add = State.getMirror_Length(0) / 4f;
                                _Th += _add;
                                // B 視點 
                                float _Bh =_sP02.Y + _PosI.W;  
                                // AB 水平距離 
                                _AB = Vector3.Distance ( new Vector3 ( _sP01.X, 0f, _sP01.Z ), new Vector3 ( _sP02.X, 0f, _sP02.Z ));
                                float _a = (float)Math.Atan((_Th - _Bh) / _AB);
                                float _aDegree = 90f - MathHelper.ToDegrees(_a);

                                TmpStr = "Zenith Angle A";
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                State.DegreeToDMS_String(_aDegree, out TmpStr);
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                y_pos += y_line;
                                TmpStr = "Vertical Angle A";
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                State.DegreeToDMS_String(180f-_aDegree, out TmpStr);
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                y_pos += y_line;

                                // 另一個垂直角 = 180 - _aDegree 

                                // C_2m 點
                                // C_2m 頂點 
                                _Th = _sP03.Y + 200f / 4f;
                                
                                // B, C_2m 水平距離 
                                _CB = Vector3.Distance(new Vector3(_sP03.X, 0f, _sP03.Z), new Vector3(_sP02.X, 0f, _sP02.Z));
                                _a = (float)Math.Atan((_Th - _Bh) / _CB);
                                float _c2mDegree = 90f - MathHelper.ToDegrees(_a);


                                TmpStr = "Zenith Angle C2m";
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                State.DegreeToDMS_String(_c2mDegree, out TmpStr);
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                y_pos += y_line;
                                TmpStr = "Vertical Angle C2m";
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                State.DegreeToDMS_String(180f - _c2mDegree, out TmpStr);
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                y_pos += y_line;


                                // C_3m 點
                                // C_3m 頂點 
                                _Th = _sP03.Y + 300f / 4f;                             
                                _a = (float)Math.Atan((_Th - _Bh) / _CB);
                                float _c3mDegree = 90f - MathHelper.ToDegrees(_a);


                                TmpStr = "Zenith Angle C3m";
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                State.DegreeToDMS_String(_c3mDegree, out TmpStr);
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                y_pos += y_line;
                                TmpStr = "Vertical Angle C3m";
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                State.DegreeToDMS_String(180f - _c3mDegree, out TmpStr);
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;





                                _dx = State.getSetPoint_Position(1).X - State.getSetPoint_Position(2).X;
                                _dz = -State.getSetPoint_Position(1).Z + State.getSetPoint_Position(2).Z;
                                _a0 = (float)MathHelper.ToDegrees((float)Math.Atan(_dz / _dx));
                                if (_dx < 0)
                                    _a0 += 180;
                                float __a0 = 90f - _a0;
                                if (__a0 < 0) __a0 += 360f;
                                    

                                x_pos = 950;
                                y_pos = 50;

                                TmpStr = "Azimuth A";
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                State.DegreeToDMS_String(__a0, out TmpStr);
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                y_pos += y_line;


                                _dx = State.getSetPoint_Position(3).X - State.getSetPoint_Position(2).X;
                                _dz = -State.getSetPoint_Position(3).Z + State.getSetPoint_Position(2).Z;
                                _a1 = (float)MathHelper.ToDegrees((float)Math.Atan(_dz / _dx));
                                if (_dx < 0)
                                    _a1 += 180;
                                float __a1 = 90f - _a1;
                                if (__a1 < 0) __a1 += 360f;



                                TmpStr = "Azimuth C";
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                State.DegreeToDMS_String(__a1, out TmpStr);
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                y_pos += y_line;
                                // 上面的 _AB, _CB
                                // 
                                TmpStr = "AB";
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                TmpStr = System.String.Format("= {0:f3}M", (_AB * 4f) / 100f);
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;

                                TmpStr = "CB";
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                TmpStr = System.String.Format("= {0:f3}M", (_CB * 4f) / 100f);
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                y_pos += y_line;


 
                                TmpStr = "Point C" ;
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                TmpStr = System.String.Format("N: {0:f3}M", (-State.getSetPoint_Position(3).Z + State.BaseNE) * 4f / 100f);
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                TmpStr = System.String.Format("E: {0:f3}M", (State.getSetPoint_Position(3).X + State.BaseNE) * 4f / 100f);
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                TmpStr = System.String.Format("Z: {0:f3}M", (State.getSetPoint_Position(3).Y * 4f + State.BaseHeight) / 100f);
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;

                                break;

                            case 3:

                                _dx = State.getSetPoint_Position(2).X - State.getSetPoint_Position(1).X;
                                _dz = -State.getSetPoint_Position(2).Z + State.getSetPoint_Position(1).Z;
                                _a0 = (float)MathHelper.ToDegrees((float)Math.Atan(_dz / _dx));
                                if (_dx < 0)
                                    _a0 += 180;
                                _dx = State.getSetPoint_Position(4).X - State.getSetPoint_Position(1).X;
                                _dz = -State.getSetPoint_Position(4).Z + State.getSetPoint_Position(1).Z;
                                _a1 = (float)MathHelper.ToDegrees((float)Math.Atan(_dz / _dx));
                                if (_dx < 0)
                                    _a1 += 180;
                                _angle = _a1 > _a0 ? _a1 - _a0 : _a0 - _a1;
                                if (_angle < 180f)
                                _angle = 360f - _angle; 

                                TmpStr = "Outer Angle A";
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                State.DegreeToDMS_String(_angle, out TmpStr);
                                // TmpStr = System.String.Format("APB = {0:D2}.{1:D2}'{2:D2}\"", 100, 10, 10);
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                y_pos += y_line;


                                _dx = State.getSetPoint_Position(3).X - State.getSetPoint_Position(2).X;
                                _dz = -State.getSetPoint_Position(3).Z + State.getSetPoint_Position(2).Z;
                                _a0 = (float)MathHelper.ToDegrees((float)Math.Atan(_dz / _dx));
                                if (_dx < 0)
                                    _a0 += 180;
                                _dx = State.getSetPoint_Position(1).X - State.getSetPoint_Position(2).X;
                                _dz = -State.getSetPoint_Position(1).Z + State.getSetPoint_Position(2).Z;
                                _a1 = (float)MathHelper.ToDegrees((float)Math.Atan(_dz / _dx));
                                if (_dx < 0)
                                    _a1 += 180;
                                _angle = _a1 > _a0 ? _a1 - _a0 : _a0 - _a1;
                                if ( _angle < 180f )
                                _angle = 360f - _angle;

                                TmpStr = "Outer Angle B";
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                State.DegreeToDMS_String(_angle, out TmpStr);
                                // TmpStr = System.String.Format("APB = {0:D2}.{1:D2}'{2:D2}\"", 100, 10, 10);
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                y_pos += y_line;

                                _dx = State.getSetPoint_Position(4).X - State.getSetPoint_Position(3).X;
                                _dz = -State.getSetPoint_Position(4).Z + State.getSetPoint_Position(3).Z;
                                _a0 = (float)MathHelper.ToDegrees((float)Math.Atan(_dz / _dx));
                                if (_dx < 0)
                                    _a0 += 180;
                                _dx = State.getSetPoint_Position(2).X - State.getSetPoint_Position(3).X;
                                _dz = -State.getSetPoint_Position(2).Z + State.getSetPoint_Position(3).Z;
                                _a1 = (float)MathHelper.ToDegrees((float)Math.Atan(_dz / _dx));
                                if (_dx < 0)
                                    _a1 += 180;
                                _angle = _a1 > _a0 ? _a1 - _a0 : _a0 - _a1;
                                if (_angle < 180f)
                                _angle = 360f - _angle;

                                TmpStr = "Outer Angle C";
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                State.DegreeToDMS_String(_angle, out TmpStr);
                                // TmpStr = System.String.Format("APB = {0:D2}.{1:D2}'{2:D2}\"", 100, 10, 10);
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                y_pos += y_line;


                                _dx = State.getSetPoint_Position(1).X - State.getSetPoint_Position(4).X;
                                _dz = -State.getSetPoint_Position(1).Z + State.getSetPoint_Position(4).Z;
                                _a0 = (float)MathHelper.ToDegrees((float)Math.Atan(_dz / _dx));
                                if (_dx < 0)
                                    _a0 += 180;
                                _dx = State.getSetPoint_Position(3).X - State.getSetPoint_Position(4).X;
                                _dz = -State.getSetPoint_Position(3).Z + State.getSetPoint_Position(4).Z;
                                _a1 = (float)MathHelper.ToDegrees((float)Math.Atan(_dz / _dx));
                                if (_dx < 0)
                                    _a1 += 180;
                                _angle = _a1 > _a0 ? _a1 - _a0 : _a0 - _a1;
                                if (_angle < 180f)
                                _angle = 360f - _angle;

                                TmpStr = "Outer Angle D";
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                State.DegreeToDMS_String(_angle, out TmpStr);
                                // TmpStr = System.String.Format("APB = {0:D2}.{1:D2}'{2:D2}\"", 100, 10, 10);
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                y_pos += y_line;

                                _AB = Vector3.Distance(new Vector3(_sP01.X, 0f, _sP01.Z), new Vector3(_sP02.X, 0f, _sP02.Z));
                                _BC = Vector3.Distance(new Vector3(_sP02.X, 0f, _sP02.Z), new Vector3(_sP03.X, 0f, _sP03.Z));
                                _CD = Vector3.Distance(new Vector3(_sP03.X, 0f, _sP03.Z), new Vector3(_sP04.X, 0f, _sP04.Z));
                                _DA = Vector3.Distance(new Vector3(_sP04.X, 0f, _sP04.Z), new Vector3(_sP01.X, 0f, _sP01.Z));


                                TmpStr = "AB";
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                TmpStr = System.String.Format("= {0:f3}M", (_AB * 4f) / 100f);
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;

                                TmpStr = "BC";
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                TmpStr = System.String.Format("= {0:f3}M", (_BC * 4f) / 100f);
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;

                                TmpStr = "CD";
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                TmpStr = System.String.Format("= {0:f3}M", (_CD * 4f) / 100f);
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;

                                TmpStr = "DA";
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;
                                TmpStr = System.String.Format("= {0:f3}M", (_DA * 4f) / 100f);
                                spriteBatch.DrawString(spriteFont, TmpStr,
                                                                       new Vector2(x_pos, y_pos), _c);
                                y_pos += y_line;




                                break;




                        }
            spriteBatch.End();

            base.Draw(gameTime);
        }

    }
}