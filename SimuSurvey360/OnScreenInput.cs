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
    public class OnscreenInput : Microsoft.Xna.Framework.DrawableGameComponent
    {

        SystemState State; 

        SpriteBatch spriteBatch;
        SpriteFont spriteFont;
        Texture2D tMenuBk;
        Texture2D tText, tTextBk;

        // Texture2D tSelector;


        // SpriteSheet spriteSheet;
        // SpriteSheet ghMenuSheet;


        private float fKeyPressCheckDelay = 0.25f;
        private float fTotalElapsedTime = 0;
        private int ghMenu_exitstate = -1;
        // private Texture2D tMenuBk, tSelector;
        private Rectangle ghMenu_DestRect = new Rectangle(0,0,1280,720);

        private ContentManager ghMenu_Content;
        const int Item_Ns = 3 ; // 先用常數, 以後要能從外面設定
        static int _SelectedItem = 0;

        Vector2 Item_StartPos = new Vector2(380, 600);
        Vector2 Item_Distance = new Vector2(110, 0);

        private PlayerIndex ghMenu_CurrentPlayer = PlayerIndex.One;

        /// <summary>
        /// Returns the method the user used to exit the ghMenu.  If -1, the ghMenu hasn't
        /// yet been displayed.  0=ghMenu is Active, 1=User exited via B/Enter (Done), 2=User exited via 
        /// Back/Escape (Abort)
        /// </summary>
        public int ExitState
        {
            get { return ghMenu_exitstate; }
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

        public OnscreenInput(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
        }

        public OnscreenInput(SystemState _State, Game game)
            : base(game)
        {
            State = _State;
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
            {
                MenuItem[i] = new MenuItemStru();
                MenuItem[i].Text = i.ToString() + @".";
            }

            //MenuItem[0].Text = @"1.";
            //MenuItem[1].Text = @"2.";
            //MenuItem[2].Text = @"3.";
            //MenuItem[3].Text = @"4.";
            //MenuItem[4].Text = @"5.";




            base.Initialize();
        }

        protected override void LoadGraphicsContent(bool loadAllContent)
        {

            if (loadAllContent)
            {
                spriteBatch = new SpriteBatch(GraphicsDevice);
                spriteFont = ghMenu_Content.Load<SpriteFont>("Arial");
                tMenuBk = ghMenu_Content.Load<Texture2D>(@"onscreenkeyboard\1280\1280_digitpad_3L");
                tText = ghMenu_Content.Load<Texture2D>(@"onscreenkeyboard\1280\blanktext");
                tTextBk = ghMenu_Content.Load<Texture2D>(@"onscreenkeyboard\1280\blanktext_bk");
                // tSelector = ghMenu_Content.Load<Texture2D>(@"content\onscreenkeyboard\Selector_Long");

                // 先保留, 不用再刪掉
                // spriteSheet = ghMenu_Content.Load<SpriteSheet>(@"SpriteSheet\SpriteSheet");
                // ghMenuSheet = ghMenu_Content.Load<SpriteSheet>(@"ghmenu\ghMenu");

            }
            base.LoadGraphicsContent(loadAllContent);
        }

        protected void Update_Entry(GamePadState GPad, KeyboardState KBoard, GameTime gameTime)
        {
            ghMenu_exitstate = 0;
            // Left, Right
            if ((GPad.ThumbSticks.Left.X < -0.25f) || (GPad.DPad.Left == ButtonState.Pressed))
            {
                fTotalElapsedTime = 0.0f;
            }
            if ((GPad.ThumbSticks.Left.X > 0.25f) || (GPad.DPad.Right == ButtonState.Pressed))
            {
                fTotalElapsedTime = 0.0f;
            }



            // Up, Down
            if (GPad.ThumbSticks.Left.Y < -0.25f || (GPad.DPad.Down == ButtonState.Pressed))
            {
                if ( _SelectedItem > 0 )
                     _SelectedItem -= 1;
                fTotalElapsedTime = 0.0f;
            }
            else if (GPad.ThumbSticks.Left.Y > 0.25f || (GPad.DPad.Up == ButtonState.Pressed))
            {
                if ( _SelectedItem < Item_Ns - 1 )
                _SelectedItem += 1;
                fTotalElapsedTime = 0.0f;
            }





            //  選定
            if (GPad.Buttons.A == ButtonState.Pressed)
            {
                ghMenu_exitstate = 1;
                this.Enabled = false;
                this.Visible = false;

            }
            // B 放棄, 先當作 Start 用
            else if (GPad.Buttons.B == ButtonState.Pressed || KBoard.IsKeyDown(Keys.Escape))
            {
                ghMenu_exitstate = 2;
                this.Enabled = false;
                this.Visible = false;
            }
            // Start 離開
            else if (GPad.Buttons.Start == ButtonState.Pressed || KBoard.IsKeyDown(Keys.Enter))
            {
                ghMenu_exitstate = 1;
                this.Enabled = false;
                this.Visible = false;
            }

        }

        protected void Draw_Entry(SpriteBatch sbBatch)
        {

            // 底圖
            // sbBatch.Draw( tMenuBk , ghMenu_DestRect, Color.White);
            sbBatch.Draw(tMenuBk, new Vector2(0, 0),
                            new Rectangle(0,0,tMenuBk.Width, tMenuBk.Height),
                          // ghMenu_DestRect,
                          Color.White,
                          0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0);



            for ( int i = 0 ; i < Item_Ns ; i++ )
            {
                int HLX = (int)(Item_StartPos.X)  + (int)(Item_Distance.X * i) ;
                int HLY = (int)(Item_StartPos.Y);

                spriteBatch.DrawString(spriteFont, MenuItem[i].Text,
                                                   new Vector2(30, 120 + i * 95), Color.LightYellow);

                if ( _SelectedItem == i ) // Highlight
                {
                    sbBatch.Draw(tText, new Vector2(30, 170 + i * 95),
                                   new Rectangle(0, 0, tText.Width, tText.Height),
                                   Color.White,
                                   0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0);

                }
                else // Normal 
                {
                    sbBatch.Draw(tTextBk, new Vector2(30, 170 + i * 95),
                                   new Rectangle(0, 0, tText.Width, tText.Height),
                                   Color.White,
                                   0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0);

                }




            }

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
            Draw_Entry(spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }

    }
}