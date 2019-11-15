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


        private float fKeyPressCheckDelay = 0.25f;
        private float fTotalElapsedTime = 0;
        private int ghMenu_exitstate = -1;
        // private Texture2D tMenuBk, tSelector;
        private Rectangle ghMenu_DestRect = new Rectangle(0,0,1280,720);

        private ContentManager ghMenu_Content;
        const int Item_Ns = 5 ;
        static int _SelectedItem = 0;

        Vector2 Item_StartPos = new Vector2(400, 600);
        Vector2 Item_Distance = new Vector2(100, 0);

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
            for ( int i = 0 ; i < 5 ; i++ )
                MenuItem[i] = new MenuItemStru() ; 

            MenuItem[0].Text = @"1. Leveling";
            // MenuItem[0].Picture = ContentReader 
            MenuItem[1].Text = @"2. Theodolite";
            MenuItem[2].Text = @"3. Total Station";
            MenuItem[3].Text = @"4. The Fourth Scenario";
            MenuItem[4].Text = @"5. The Fifth Scenario";




            base.Initialize();
        }

        protected override void LoadGraphicsContent(bool loadAllContent)
        {

            if (loadAllContent)
            {
                spriteBatch = new SpriteBatch(GraphicsDevice);
                spriteFont = ghMenu_Content.Load<SpriteFont>("Arial");
                // tMenuBk = ghMenu_Content.Load<Texture2D>(@"content\ghmenu\ghMenu_Bk.jpg");
                // tSelector = ghMenu_Content.Load<Texture2D>(@"content\ghmenu\Selector_Small");
                spriteSheet = ghMenu_Content.Load<SpriteSheet>(@"SpriteSheet\SpriteSheet");
                ghMenuSheet = ghMenu_Content.Load<SpriteSheet>(@"ghmenu\ghMenu");

            }
            base.LoadGraphicsContent(loadAllContent);
        }

        protected void Update_Entry(GamePadState GPad, KeyboardState KBoard, GameTime gameTime)
        {
            ghMenu_exitstate = 0;
            // First, handle moving left or right
            if ((GPad.ThumbSticks.Left.X < -0.25f) || (GPad.DPad.Left == ButtonState.Pressed))
            {
                if ( _SelectedItem > 0 )
                     _SelectedItem -= 1;

                fTotalElapsedTime = 0.0f;
            }
            if ((GPad.ThumbSticks.Left.X > 0.25f) || (GPad.DPad.Right == ButtonState.Pressed))
            {
                if ( _SelectedItem < Item_Ns - 1 )
                _SelectedItem += 1;
                fTotalElapsedTime = 0.0f;
            }



            // Up, Down 以後擴充
            if (GPad.ThumbSticks.Left.Y < -0.25f || (GPad.DPad.Down == ButtonState.Pressed))
            {
                fTotalElapsedTime = 0.0f;
            }
            else if (GPad.ThumbSticks.Left.Y > 0.25f || (GPad.DPad.Up == ButtonState.Pressed))
            {
                fTotalElapsedTime = 0.0f;
            }





            //  選定
            if (GPad.Buttons.A == ButtonState.Pressed)
            {


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
            sbBatch.Draw(ghMenuSheet.Texture, new Vector2(0, 0),
                          ghMenuSheet.SourceRectangle("menu01_bk"),
                          // ghMenu_DestRect,
                          Color.White,
                          0f, new Vector2(0, 0), 1.5f, SpriteEffects.None, 0);






            for ( int i = 0 ; i < Item_Ns ; i++ )
            {
                int HLX = (int)(Item_StartPos.X)  + (int)(Item_Distance.X * i) ;
                int HLY = (int)(Item_StartPos.Y);

                if ( _SelectedItem == i ) // Highlight
                {
                    int Btn_Index = ghMenuSheet.GetIndex("BTN_B01");
                    Btn_Index += i << 1;
                    sbBatch.Draw(ghMenuSheet.Texture, new Vector2(350, 130),
                                   ghMenuSheet.SourceRectangle(Btn_Index),
                                   Color.White,
                                   0f, new Vector2(0, 0), 5f, SpriteEffects.None, 0);



                    spriteBatch.DrawString(spriteFont, MenuItem[i].Text,
                                                       new Vector2(320, 20), Color.LightYellow);

                    // sbBatch.Draw( tSelector, new Rectangle(HLX, HLY, 44, 44), Color.White);
                    sbBatch.Draw(ghMenuSheet.Texture, new Vector2(HLX, HLY),
                                   ghMenuSheet.SourceRectangle("Selector_Small"),                                   
                                   Color.White,
                                   0f, new Vector2(0, 0), 2f, SpriteEffects.None, 0);

                }
                else // Normal 
                {


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