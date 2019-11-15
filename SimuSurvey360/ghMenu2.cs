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
    public class ghMenu2 : Microsoft.Xna.Framework.DrawableGameComponent
    {

        SystemState State; 

        SpriteBatch spriteBatch;
        SpriteFont spriteFont;
        SpriteSheet spriteSheet;
        SpriteSheet ghMenuSheet;
        int Btn_State;

        private float fKeyPressCheckDelay = 0.25f;
        private float fTotalElapsedTime = 0;
        private int ghMenu_exitstate = -1;
        // private Texture2D tMenuBk, tSelector;
        private Rectangle ghMenu_DestRect = new Rectangle(0,0,1280,720);

        private ContentManager ghMenu_Content;
        const int Item_Ns = 4 ;
        static int _SelectedItem = 0;

        // Vector2 Item_StartPos = new Vector2(300, 550);
        Vector2 Item_StartPos = new Vector2(380, 550);
        Vector2 Item_Distance = new Vector2(160, 0);

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

        public ghMenu2(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
        }

        public ghMenu2(SystemState _State, Game game)
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
                MenuItem[i] = new MenuItemStru() ;

            MenuItem[0].Text = @"1. Navigation";
            //MenuItem[1].Text = @"2. Surveying";
            MenuItem[1].Text = @"2. Score/ Replay";
            MenuItem[2].Text = @"3. Options";
            MenuItem[3].Text = @"4. Exit";


            //GamePadState GPad ;
            //KeyboardState KBoard ;
            //GPad = GamePad.GetState(ghMenu_CurrentPlayer);
            //KBoard = Keyboard.GetState();
            //while (GPad.Buttons.B == ButtonState.Pressed || KBoard.IsKeyDown(Keys.P))
            //{
            //    GPad = GamePad.GetState(ghMenu_CurrentPlayer);
            //    KBoard = Keyboard.GetState();
            //}

            base.Initialize();
        }

        protected override void LoadGraphicsContent(bool loadAllContent)
        {

            if (loadAllContent)
            {
                spriteBatch = new SpriteBatch(GraphicsDevice);
                spriteFont = ghMenu_Content.Load<SpriteFont>("Jokeman");
                // tMenuBk = ghMenu_Content.Load<Texture2D>(@"content\ghmenu\ghMenu_Bk.jpg");
                // tSelector = ghMenu_Content.Load<Texture2D>(@"content\ghmenu\Selector_Small");
                spriteSheet = ghMenu_Content.Load<SpriteSheet>(@"SpriteSheet\SpriteSheet");
                ghMenuSheet = ghMenu_Content.Load<SpriteSheet>(@"ghmenu2\ghMenu2");

            }
            base.LoadGraphicsContent(loadAllContent);
        }

        protected void Update_Entry(GamePadState GPad, KeyboardState KBoard, GameTime gameTime)
        {
            ghMenu_exitstate = 0;
            // First, handle moving left or right
            if ((GPad.ThumbSticks.Left.X < -0.25f) || (GPad.DPad.Left == ButtonState.Pressed) || KBoard.IsKeyDown(Keys.Left))
            {
                if (_SelectedItem > 0)
                    _SelectedItem -= 1;
                Btn_State = 0;

                fTotalElapsedTime = 0.0f;
            }
            if ((GPad.ThumbSticks.Left.X > 0.25f) || (GPad.DPad.Right == ButtonState.Pressed) || KBoard.IsKeyDown(Keys.Right))
            {
                if (_SelectedItem < Item_Ns - 1)
                    _SelectedItem += 1;
                Btn_State = 0;
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
            if (GPad.Buttons.A == ButtonState.Pressed)
            {
                ghMenu_exitstate = 1;
                this.Enabled = false;
                this.Visible = false;

            }
            // B 放棄, 先當作 Start 用
            //else if (GPad.Buttons.Back == ButtonState.Pressed || KBoard.IsKeyDown(Keys.P))
            //{
            //    ghMenu_exitstate = 2;
            //    this.Enabled = false;
            //    this.Visible = false;
            //}
            // Start 離開
            else if (GPad.Buttons.Start == ButtonState.Pressed || KBoard.IsKeyDown(Keys.Enter))
            {
                ghMenu_exitstate = 1;
                this.Enabled = false;
                this.Visible = false;
            }

        }

        protected void Draw_Entry(SpriteBatch sbBatch, int _Step )
        {
            int Btn_Index;
            if (_Step > 10)
                _Step = 10; 
            for ( int i = 0 ; i < Item_Ns ; i++ )
            {
                int HLX = (int)(Item_StartPos.X)  + (int)(Item_Distance.X  * i) ;
                int HLY = (int)(Item_StartPos.Y);

                Btn_Index = ghMenuSheet.GetIndex("BTN_B01") + (i << 1);
                if (_SelectedItem == i) // Highlight
                {
                    // Btn_Index += 1;
                    // 先不要Show 文字
                    //spriteBatch.DrawString(spriteFont, MenuItem[i].Text,
                    //               new Vector2(350, 180), Color.Aqua);
                    float _r;                    
                    float _Factor = 0.8f + (1.3f - 0.8f) * _Step / 10.0f;
                    HLY = HLY + (int)( 128f * ( 1.3f - _Factor ));
                    sbBatch.Draw(ghMenuSheet.Texture, new Vector2(HLX, HLY),
                                   ghMenuSheet.SourceRectangle(Btn_Index),
                                   Color.White,
                                   0f, new Vector2(0, 0), _Factor, SpriteEffects.None, 0);
                }
                else
                {
                    HLY = HLY + (int)(128f * (1.3f - 0.8f));
                    sbBatch.Draw(ghMenuSheet.Texture, new Vector2(HLX, HLY),
                                   ghMenuSheet.SourceRectangle(Btn_Index),
                                   Color.White,
                                   0f, new Vector2(0, 0), 0.8f, SpriteEffects.None, 0);
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

            if ((elapsed / 0.01677f > 1.5f ) && (Btn_State < 10))
            {
                Btn_State += ( int )( elapsed / 0.01677f ) - 1 ;
            }



            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            Draw_Entry(spriteBatch, Btn_State );
            //if (Btn_State < 10)
            //    Btn_State += 1;
            //if ((elapsed / 0.01667f > 1.5f) && (Btn_State < 10))
            //{
            //    Btn_State += (int)(elapsed / 0.01667f) ;
            //}
            if ( Btn_State < 10 )
            {
                Btn_State += (int)(elapsed / 0.01666f);
            }


            spriteBatch.End();

            base.Draw(gameTime);
        }

    }
}