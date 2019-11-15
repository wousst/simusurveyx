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
    public class settingError : Microsoft.Xna.Framework.DrawableGameComponent
    {

        SystemState State; 

        SpriteBatch spriteBatch;
        SpriteFont spriteFont;
        SpriteSheet spriteSheet;
        SpriteSheet recordMenuSheet;


        private float fKeyPressCheckDelay = 0.25f;
        private float fTotalElapsedTime = 0;
        private int ghMenu_exitstate = -1;
        // private Texture2D tMenuBk, tSelector;
        private Rectangle ghMenu_DestRect = new Rectangle(0,0,1280,720);

        private ContentManager ghMenu_Content;
        const int Item_Ns = 4 ;
        static int _SelectedItem = 0;

        Vector2 Item_StartPos = new Vector2(380, 600);
        Vector2 Item_Distance = new Vector2(110, 0);
        Texture2D Bk;
        Vector3[] _PosR = new Vector3[4];

        int Y_DispOffset;

        float _Error_X, _Error_Y, _Error_Z; // degree, +-1
        public float Error_X
        {
            get { return _Error_X; }
            set { _Error_X = value; }
        }
        public float Error_Y
        {
            get { return _Error_Y; }
            set { _Error_Y = value; }
        }
        public float Error_Z
        {
            get { return _Error_Z; }
            set { _Error_Z = value; }
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

        public settingError(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
        }

        public settingError(SystemState _State, Game game)
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

            MenuItem[0].Text = @"X-Axis";
            // MenuItem[0].Picture = ContentReader 
            MenuItem[1].Text = @"Y-Axis";
            MenuItem[2].Text = @"Z-Axis";
            MenuItem[2].Text = @"Reset";


            Bk = ghMenu_Content.Load<Texture2D>("SettingError");


            base.Initialize();
        }

        protected override void LoadGraphicsContent(bool loadAllContent)
        {

            if (loadAllContent)
            {
                spriteBatch = new SpriteBatch(GraphicsDevice);
                spriteFont = ghMenu_Content.Load<SpriteFont>("Arial24");
                spriteSheet = ghMenu_Content.Load<SpriteSheet>(@"SpriteSheet\SpriteSheet");
                recordMenuSheet = ghMenu_Content.Load<SpriteSheet>(@"ghmenu\ghMenu");

            }
            base.LoadGraphicsContent(loadAllContent);
        }

        protected void Update_Entry(GamePadState GPad, KeyboardState KBoard, GameTime gameTime)
        {
            ghMenu_exitstate = 0;
            // First, handle moving left or right
            if ((GPad.ThumbSticks.Left.Y > 0.25f) || (GPad.DPad.Up == ButtonState.Pressed) || KBoard.IsKeyDown(Keys.Up) )
            {
                if ( _SelectedItem > 0 )
                     _SelectedItem -= 1;

                fTotalElapsedTime = 0.0f;
            }
            else if ((GPad.ThumbSticks.Left.Y < -0.25f) || (GPad.DPad.Down == ButtonState.Pressed) || KBoard.IsKeyDown(Keys.Down))
            {
                if ( _SelectedItem < Item_Ns - 1 )
                _SelectedItem += 1;
                fTotalElapsedTime = 0.0f;
            }
            else if (GPad.ThumbSticks.Left.X != 0f )
            {

                switch (  _SelectedItem )
                { 
                    case 0 :

                        _Error_X += (float)GPad.ThumbSticks.Left.X * 0.003f ;
                        if (_Error_X > 1.0f) _Error_X = 1.0f;
                        if (_Error_X < -1.0f) _Error_X = -1.0f;
                        break;
                    case 1 :
                        _Error_Y += (float)GPad.ThumbSticks.Left.X * 0.003f;
                        if (_Error_Y > 1.0f) _Error_Y = 1.0f;
                        if (_Error_Y < -1.0f) _Error_Y = -1.0f;
                        break;
                    case 2 :
                        _Error_Z += (float)GPad.ThumbSticks.Left.X * 0.003f;
                        if (_Error_Z > 1.0f) _Error_Z = 1.0f;
                        if (_Error_Z < -1.0f) _Error_Z = -1.0f;
                        break;
                }

            }



            // Up, Down 以後擴充
            //if (GPad.ThumbSticks.Left.X < -0.25f || (GPad.DPad.Left == ButtonState.Pressed) || KBoard.IsKeyDown(Keys.Left))
            //{
            //    fTotalElapsedTime = 0.0f;
            //}
            //else if (GPad.ThumbSticks.Left.X > 0.25f || (GPad.DPad.Right == ButtonState.Pressed) || KBoard.IsKeyDown(Keys.Right))
            //{
            //    fTotalElapsedTime = 0.0f;
            //}





            //  選定
            if (GPad.Buttons.A == ButtonState.Pressed)
            {
            //    ghMenu_exitstate = 1;
            //    this.Enabled = false;
            //    this.Visible = false;
                if (_SelectedItem == 3)
                {
                    _Error_X = _Error_Y = _Error_Z = 0f;
                }


            }
            // B 放棄, 先當作 Start 用
            if (GPad.Buttons.B == ButtonState.Pressed || KBoard.IsKeyDown(Keys.Escape))
            {
                ghMenu_exitstate = 2;
                this.Enabled = false;
                this.Visible = false;
            }
            // Start 離開
            else if (GPad.Buttons.Start == ButtonState.Pressed || KBoard.IsKeyDown(Keys.Enter))
            {
                //ghMenu_exitstate = 1;
                //this.Enabled = false;
                //this.Visible = false;
            }


            if (GPad.ThumbSticks.Left.Y != 0)
            {
                int tDisp;

                tDisp = Y_DispOffset + (int)(GPad.ThumbSticks.Left.Y) * -20;


                if ((tDisp > 0) && (tDisp < 360))
                {
                    Y_DispOffset = tDisp;
                }
                else if (tDisp < 0)
                    Y_DispOffset = 0;
                else if(tDisp > 360)
                    Y_DispOffset = 360;
            }

            if (KBoard.IsKeyDown(Keys.Up))
            {
                if ( Y_DispOffset >= 40 )
                    Y_DispOffset -= 40;
                else
                    Y_DispOffset = 0;
            }
            else if (KBoard.IsKeyDown(Keys.Down))
            {
                if ( Y_DispOffset <= 320 )
                    Y_DispOffset += 40;
                else
                    Y_DispOffset = 360 ;
            }


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

            // Draw_Entry(spriteBatch);
            String TmpStr;
            int x_pos = 650;
            int y_pos = 250;
            int y_line = 40; 




                TmpStr = "Systematic Error Setting" ; 
                spriteBatch.DrawString(spriteFont, TmpStr,
                                                       new Vector2(x_pos, y_pos), Color.Black);
                y_pos += y_line;
                y_pos += y_line;
                TmpStr = System.String.Format("X: {0:f3}", _Error_X);
                if ( _SelectedItem == 0 )
                spriteBatch.DrawString(spriteFont, TmpStr,
                                                           new Vector2(x_pos, y_pos), Color.Red);
                else
                spriteBatch.DrawString(spriteFont, TmpStr,
                                                       new Vector2(x_pos, y_pos), Color.Black);
                y_pos += y_line;
                TmpStr = System.String.Format("Y: {0:f3}", _Error_Y);
                if (_SelectedItem == 1)
                spriteBatch.DrawString(spriteFont, TmpStr,
                                                           new Vector2(x_pos, y_pos), Color.Red);
                else
                spriteBatch.DrawString(spriteFont, TmpStr,
                                                       new Vector2(x_pos, y_pos), Color.Black);
                y_pos += y_line;
                TmpStr = System.String.Format("Z: {0:f3}", _Error_Z);
                if (_SelectedItem == 2)
                spriteBatch.DrawString(spriteFont, TmpStr,
                                                       new Vector2(x_pos, y_pos), Color.Red);
                else
                spriteBatch.DrawString(spriteFont, TmpStr,
                                                       new Vector2(x_pos, y_pos), Color.Black);

                y_pos += y_line;
                y_pos += y_line;

                TmpStr = "Reset";
                if (_SelectedItem == 3)
                spriteBatch.DrawString(spriteFont, TmpStr,
                                                       new Vector2(x_pos, y_pos), Color.Red);
                else
                spriteBatch.DrawString(spriteFont, TmpStr,
                                                       new Vector2(x_pos, y_pos), Color.Black);

 
            spriteBatch.End();

            base.Draw(gameTime);
        }

    }
}