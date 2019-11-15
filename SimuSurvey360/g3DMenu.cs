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
using SimuSurvey360.Instruments;
namespace SimuSurvey360
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class g3DMenu : Microsoft.Xna.Framework.DrawableGameComponent
    {

        SystemState State; 

        SpriteBatch spriteBatch;
        SpriteFont spriteFont;
        // SpriteSheet spriteSheet;
        // SpriteSheet ghMenuSheet;
        Camera _currentCamera, _mainCamera;

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
        List<VisualObject> _visualObjects;
        Camera _Camera;

        Texture2D[] tex;
        Texture2D Bk;
        string[] Text;
        Vector3[] CamPos;
        TotalStation _TotalStation;
        TotalStation _Leveling;
        TotalStation _Theodolite;
        KeyboardState PrevKeyState, CurrKeyState;
        GamePadState PrevGPadState, CurrGPadState;
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

  

        public g3DMenu(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
        }

        public g3DMenu(SystemState _State, Game game)
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

            ghMenu_Content = new ContentManager(Game.Services);
            ghMenu_Content.RootDirectory = "Content";



            tex = new Texture2D[Item_Ns];

            tex[0] = ghMenu_Content.Load<Texture2D>("main1");
            tex[1] = ghMenu_Content.Load<Texture2D>("main2");
            tex[2] = ghMenu_Content.Load<Texture2D>("main3");
            tex[3] = ghMenu_Content.Load<Texture2D>("main4");
            tex[4] = ghMenu_Content.Load<Texture2D>("main5");
            Bk = ghMenu_Content.Load<Texture2D>("menu01_bk");
            _visualObjects = new List<VisualObject>();
            _visualObjects.Add(new VisualObject("P05", "P05", Matrix.CreateTranslation(80f, 0f, -200f), 0, tex[4],1f, true));
            _visualObjects.Add(new VisualObject("P04", "P04", Matrix.CreateTranslation(60f, 0f, -150f), 0, tex[3],1f, true));
            _visualObjects.Add(new VisualObject("P03", "P03", Matrix.CreateTranslation(40f, 0f, -100f), 0, tex[2],1f, true));
            _visualObjects.Add(new VisualObject("P02", "P02", Matrix.CreateTranslation(20f, 0f, -50f), 0, tex[1],1f, true));
            _visualObjects.Add(new VisualObject("P01", "P01", Matrix.Identity, 0, tex[0],1f, true));


            CamPos = new Vector3[Item_Ns];
            CamPos[0] = new Vector3(80f, 20f, 200f);
            CamPos[1] = new Vector3(100f, 20f, 150f);
            CamPos[2] = new Vector3(120f, 20f, 100f);
            CamPos[3] = new Vector3(140f, 20f, 50f);
            CamPos[4] = new Vector3(160f, 20f, 0f);
            // Target, x = x0-30, y = 50, z=Z0-200 

            Text = new string[Item_Ns];

            Text[0] = @"1. Leveling Surveying";
            Text[1] = @"2. Horizonal Angle Surveying";
            Text[2] = @"3. Vertical Angle Surveying";
            Text[3] = @"4. Traverse Closed Surveying";
            Text[4] = @"5. Free Model";


            if (_visualObjects.Count > 0)
            {
                foreach (VisualObject v in _visualObjects)
                {
                    v.Initialize();
                }
            }

            _TotalStation = new TotalStation( InstrumentType.TotalStation) ;
            _Leveling = new TotalStation(InstrumentType.Leveling);
            _Theodolite = new TotalStation(InstrumentType.Theodolite); 
            base.Initialize();
        }

        protected override void LoadGraphicsContent(bool loadAllContent)
        {

            if (loadAllContent)
            {
                spriteBatch = new SpriteBatch(GraphicsDevice);
                // spriteFont = ghMenu_Content.Load<SpriteFont>("Jokeman");
                spriteFont = ghMenu_Content.Load<SpriteFont>("menufont28");
                foreach (VisualObject v in _visualObjects)
                {
                    v.LoadContent(this.ghMenu_Content);
                }

                _mainCamera = new Camera("MainCamera",
                    GraphicsDevice.Viewport.Width,
                    GraphicsDevice.Viewport.Height,
                    GraphicsDevice.Viewport.AspectRatio,
                    MathHelper.PiOver2 / 2.2f,
                    190f, 
                    10000,
                    true);
                _mainCamera.Initialize(new Vector3(80, 20, 200), new Vector3(50, 50, 0));


                _Camera = new Camera("Camera",
                    GraphicsDevice.Viewport.Width,
                    GraphicsDevice.Viewport.Height,
                    GraphicsDevice.Viewport.AspectRatio,
                    MathHelper.PiOver2 / 2.2f,
                    0.01f,
                    1000f,
                    true);
                _Camera.Initialize(new Vector3(-11, 20, 30), new Vector3(-11, 30, 0));

                _currentCamera = _mainCamera;
                _TotalStation.Load(ghMenu_Content);
                _TotalStation.WorldPosition = new Vector3(0f, 0f, 0f);

                _Leveling.Load(ghMenu_Content);
                _Leveling.WorldPosition = new Vector3(0f, 0f, 0f);
                float _l = _Leveling.TripodLength;
                _Leveling.TripodLength = _l + 3f; 
                _Theodolite.Load(ghMenu_Content);
                _Theodolite.WorldPosition = new Vector3(0f, 0f, 0f);


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

                fTotalElapsedTime = 0.0f;
            }
            if ((GPad.ThumbSticks.Left.X > 0.25f) || (GPad.DPad.Right == ButtonState.Pressed) || KBoard.IsKeyDown(Keys.Right))
            {
                if (_SelectedItem < Item_Ns - 1)
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
            if (( GPad.Buttons.A == ButtonState.Pressed ) && ( PrevGPadState.Buttons.A == ButtonState.Released ))
            {
                ghMenu_exitstate = 1;
                this.Enabled = false;
                this.Visible = false;

            }
            // B 放棄, 先當作 Start 用
            else if (GPad.Buttons.Back == ButtonState.Pressed || KBoard.IsKeyDown(Keys.Escape))
            {
                ghMenu_exitstate = 2;
                this.Enabled = false;
                this.Visible = false;
            }
            // Start 離開
            else if (GPad.Buttons.Start == ButtonState.Pressed || (KBoard.IsKeyDown(Keys.Enter) && (PrevKeyState.IsKeyUp(Keys.Enter))))
            {
                ghMenu_exitstate = 1;
                this.Enabled = false;
                this.Visible = false;
            }
            PrevGPadState = GPad; 
            PrevKeyState = KBoard;


        }

        protected void Draw_Entry(GameTime gameTime, SpriteBatch sbBatch)
        {

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float target_offset = (elapsed / 0.01666f) * 2f; 
            if (_currentCamera.Position.X != CamPos[_SelectedItem].X)
            {
                Vector3 NewPosition = _currentCamera.Position;
                float dx = CamPos[_SelectedItem].X - _currentCamera.Position.X;
                if (dx > 0f)
                {
                    if ( dx > target_offset )
                        dx = target_offset;
                    else if (dx > 2f)
                        dx = 2f;
                    NewPosition = _currentCamera.Position + new Vector3(dx, 0f, -2.5f * dx);
                }
                else if (dx < 0f)
                {
                    if (dx < -target_offset)
                        dx = -target_offset;
                    else if (dx < -2f)
                        dx = -2f;
                    NewPosition = _currentCamera.Position + new Vector3(dx, 0f, -2.5f * dx);

                }

                _currentCamera.Initialize(NewPosition, new Vector3(NewPosition.X - 30f, 50f, NewPosition.Z - 200f));


            }

        }


        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {


            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            fTotalElapsedTime += elapsed;

            if (fTotalElapsedTime > fKeyPressCheckDelay)
            {
                Update_Entry(GamePad.GetState(PlayerIndex.One), Keyboard.GetState(), gameTime);
            }

            foreach (VisualObject v in _visualObjects)
            {
                v.Update(gameTime);
            }
            // _TotalStation.Update(Matrix.Identity, _Camera.View, _Camera.Projection);
            _TotalStation.Update(Matrix.Identity, _Camera.View, _Camera.Projection);

            switch (_SelectedItem)
            {
                case 0:
                    _Leveling.Update(Matrix.Identity, _Camera.View, _Camera.Projection);
                    break;
                case 2:
                    _Theodolite.Update(Matrix.Identity, _Camera.View, _Camera.Projection);
                    break;
                case 1:
                case 3:
                case 4:

                    _TotalStation.Update(Matrix.Identity, _Camera.View, _Camera.Projection);
                    break;
            }

            _Leveling.TribrachRotationValue += 0.2f;
            _Theodolite.TribrachRotationValue += 0.2f;
            _TotalStation.TribrachRotationValue += 0.2f;


            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            spriteBatch.Draw(Bk, new Vector2(0, 0),
                         new Rectangle(0, 0, 1280, 720),
                          Color.White,
                          0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0);

            for (int i = 0; i < Item_Ns; i++)
            {

                if (_SelectedItem == i)
                    spriteBatch.DrawString(spriteFont, Text[i],
                                                       new Vector2(180, 180), Color.DarkSlateGray);

            }

            Draw_Entry(gameTime, spriteBatch);


            spriteBatch.End();



            GraphicsDevice.RenderState.DepthBufferEnable = true;
            // 所有加在 VisualObject List 中的模型
            foreach (VisualObject v in _visualObjects)
            {
                v.Draw(gameTime, _currentCamera, new Vector3(0.7f, 0.7f, 0.5f), 0.8f);
            }


            switch (_SelectedItem)
            {
                case 0:
                    _Leveling.Draw();
                    break;
                case 2:
                    _Theodolite.Draw();
                    break;
                case 1:
                case 3:
                case 4:

                    _TotalStation.Draw();
                    break;
            }



            base.Draw(gameTime);
            
            
            





        }

    }
}