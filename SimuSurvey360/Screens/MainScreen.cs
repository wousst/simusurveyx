using System;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SimuSurvey360;
using SimuSurvey360.Instruments;

namespace SimuSurvey360.Screens
{
    class MainScreen :GameScreen
    {


        #region Fields
// Bloom
        SpriteBatch spriteBatch;

        Effect bloomExtractEffect;
        Effect bloomCombineEffect;
        Effect gaussianBlurEffect;

        ResolveTexture2D resolveTarget;
        RenderTarget2D renderTarget1;
        RenderTarget2D renderTarget2;


        // Choose what display settings the bloom should use.
        public BloomSettings Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        BloomSettings settings = BloomSettings.PresetSettings[2];


        // Optionally displays one of the intermediate buffers used
        // by the bloom postprocess, so you can see exactly what is
        // being drawn into each rendertarget.
        public enum IntermediateBuffer
        {
            PreBloom,
            BlurredHorizontally,
            BlurredBothWays,
            FinalResult,
        }

        public IntermediateBuffer ShowBuffer
        {
            get { return showBuffer; }
            set { showBuffer = value; }
        }

        IntermediateBuffer showBuffer = IntermediateBuffer.FinalResult;


// LensFlare


        // How big is the circular glow effect?
        const float glowSize = 400;

        // How big a rectangle should we examine when issuing our occlusion queries?
        // Increasing this makes the flares fade out more gradually when the sun goes
        // behind scenery, while smaller query areas cause sudden on/off transitions.
        const float querySize = 600;




        // These are set by the main game to tell us the position of the camera and sun.
        // skc, 直接用 _currentCamera
        // public Matrix View;
        // public Matrix Projection;

        // public Vector3 LightDirection = Vector3.Normalize(new Vector3(-1, -0.1f, 0.3f));
        
        public Vector3 LightDirection = Vector3.Normalize(new Vector3(-100, -10f, -80f));


        // Graphics objects.
        Texture2D glowSprite;
        // SpriteBatch spriteBatch;
        BasicEffect basicEffect;
        VertexDeclaration vertexDeclaration;
        VertexPositionColor[] queryVertices;


        // An occlusion query is used to detect when the sun is hidden behind scenery.
        OcclusionQuery occlusionQuery;
        bool occlusionQueryActive;
        float occlusionAlpha;


        // The lensflare effect is made up from several individual flare graphics,
        // which move across the screen depending on the position of the sun. This
        // helper class keeps track of the position, size, and color for each flare.
        class Flare
        {
            public Flare(float position, float scale, Color color, string textureName)
            {
                Position = position;
                Scale = scale;
                Color = color;
                TextureName = textureName;
            }

            public float Position;
            public float Scale;
            public Color Color;
            public string TextureName;
            public Texture2D Texture;
        }


        // Array describes the position, size, color, and texture for each individual
        // flare graphic. The position value lies on a line between the sun and the
        // center of the screen. Zero places a flare directly over the top of the sun,
        // one is exactly in the middle of the screen, fractional positions lie in
        // between these two points, while negative values or positions greater than
        // one will move the flares outward toward the edge of the screen. Changing
        // the number of flares, or tweaking their positions and colors, can produce
        // a wide range of different lensflare effects without altering any other code.
        Flare[] flares =
        {
            new Flare(-0.5f, 0.7f, new Color( 50,  25,  50), "flare1"),
            new Flare( 0.3f, 0.4f, new Color(100, 255, 200), "flare1"),
            new Flare( 1.2f, 1.0f, new Color(100,  50,  50), "flare1"),
            new Flare( 1.5f, 1.5f, new Color( 50, 100,  50), "flare1"),

            new Flare(-0.3f, 0.7f, new Color(200,  50,  50), "flare2"),
            new Flare( 0.6f, 0.9f, new Color( 50, 100,  50), "flare2"),
            new Flare( 0.7f, 0.4f, new Color( 50, 200, 200), "flare2"),

            new Flare(-0.7f, 0.7f, new Color( 50, 100,  25), "flare3"),
            new Flare( 0.0f, 0.6f, new Color( 25,  25,  25), "flare3"),
            new Flare( 2.0f, 1.4f, new Color( 25,  50, 100), "flare3"),
        };















        ContentManager _content;
        SpriteFont gameFont;

        Vector2 playerPosition = new Vector2(100, 100);
        Vector2 enemyPosition = new Vector2(100, 100);

        Random random = new Random();
        Rectangle _InfoDisplayArea;
        Rectangle _SubInfoDisplayArea;
        Rectangle _SubInfoDisplayArea2;
        Viewport _MainViewport;
        Viewport _SurveyingViewport;
        Texture2D _SideTexture;
        Ray _ray;

        
        // 使 Ruler 面向Instrument 
        Vector3 InstrumentPos;
        int _Accessory_Ns; 

        int CameraN;
        public Camera _currentCamera;

        Camera _TotalstationCamera; 

        Camera[] _Camera;
        //Camera _Camera0, // First Person, Near
        //       _Camera1, // behind tripod
        //       _Camera2, // Ruler1
        //       _Camera3; // Ruler2

        //SimuSurvey

        MouseState _preState;
        MouseState _currentState;


        // SystemState State; 
        public Vector3 InstrumentLocation
        {
            get { return InstrumentPos; }
        }
        public Camera TotalstationCamera
        {
            get { return _TotalstationCamera; }
        }
        public float TelescopeRotationValue
        {
            get {
                if ((_SceneController.SelectedInstrument.Type == InstrumentType.TotalStation)||
                    (_SceneController.SelectedInstrument.Type == InstrumentType.Leveling)||
                    (_SceneController.SelectedInstrument.Type == InstrumentType.Theodolite))
                {
                    return ((TotalStation)_SceneController.SelectedInstrument).TelescopeRotationValue;
                }
                else
                    return 0f;
            }
            set
            {
                if ((_SceneController.SelectedInstrument.Type == InstrumentType.TotalStation) ||
                    (_SceneController.SelectedInstrument.Type == InstrumentType.Leveling) ||
                    (_SceneController.SelectedInstrument.Type == InstrumentType.Theodolite))
                {
                    ((TotalStation)_SceneController.SelectedInstrument).TelescopeRotationValue = value;
                }
            }
        }
        public float TribrachRotationValue
        {
            get
            {
                if ((_SceneController.SelectedInstrument.Type == InstrumentType.TotalStation) ||
                    (_SceneController.SelectedInstrument.Type == InstrumentType.Leveling) ||
                    (_SceneController.SelectedInstrument.Type == InstrumentType.Theodolite))
                {
                    return ((TotalStation)_SceneController.SelectedInstrument).TribrachRotationValue;
                }
                else
                    return 0f;
            }
        }


        public float TotalStationCameraHight 
        {
            get { return _TotalstationCamera.Position.Y; }
        }

        public float Accessory_Ns
        {
            get { return _Accessory_Ns; }
        }

        public Ray _Ray
        {
            get { return _ray; }
        }


        public enum OperatingMode
        {
            Navigation, Instrument, TextInput
        };
        public enum TripodControlOption
        {
            Length,Rotation
        };
        public enum UpperBodyControlOption
        {
            Tribrach,Telescope
        };
        public SceneController _SceneController;
        OperatingMode _Mode;
        TripodControlOption _TripodMode;
        UpperBodyControlOption _UpperBodyMode;
        bool _IsShowSurveyingView;
        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public MainScreen()
        {
            _Camera = new Camera[6]; 
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
            _Mode = OperatingMode.Navigation;
            _InfoDisplayArea = new Rectangle(100, 100, 600, 450);
            _SubInfoDisplayArea = new Rectangle(450, 550, 300, 50);
            _SubInfoDisplayArea2 = new Rectangle(0, 550, 300, 50);
            _TripodMode = TripodControlOption.Length;
            _IsRightStickReady = false;
            _IsShowSurveyingView = false;
            _preState = Mouse.GetState();
        }

        #region Input Handler



      

//  這些東西應該通通刪除
        //B
        //Right Stick
        void OnButtonBDown()
        {
            _IsButtonBReady = true;
        }
        void OnButtonBUp()
        {
            if (_IsButtonBReady)
            {
                _Mode = OperatingMode.Navigation;
                _IsButtonBReady = false;
            }
        }
        //X
        void OnButtonXDown()
        {
            _IsButtonXReady = true;
        }
        void OnButtonXUp()
        {
            if (_IsButtonXReady)
            {
                _Mode = OperatingMode.Instrument;
                _IsButtonXReady = false;
            }
        }
        //Y
        void OnButtonYDown()
        {
            _IsButtonYReady = true;
        }
        void OnButtonYUp()
        {
            if (_IsButtonYReady)
            {
                ExchangeSurveyingWindow();
                _IsButtonYReady = false;
            }
        }
        //Right Trigger
        void OnRightTriggerButtonDown()
        {
            _IsRightTriggerReady = true;
        }
        void OnRightTriggerButtonUp()
        {
            if (_IsRightTriggerReady)
            {
                _SceneController.ChangeInstrument(true);
                _IsRightTriggerReady = false;
            }
        }
        //Left Trigger
        void OnLeftTriggerButtonDown()
        {
            _IsLeftTriggerReady = true;
        }
        void OnLeftTriggerButtonUp()
        {
            if (_IsLeftTriggerReady)
            {
                _SceneController.ChangeInstrument(false);
                _IsLeftTriggerReady = false;
            }
        }
        //Right Stick
        void OnRightStickButtonDown()
        {
            _IsRightStickReady = true;
        }
        void OnRightStickButtonUp()
        {
            if (_IsRightStickReady)
            {
                ChangeTripodMode();
                _IsRightStickReady = false;
            }
        }
        #endregion


// 以上應該刪除




        #region Operating Mode Control
        void ChangeTripodMode()
        {
            if (_TripodMode == TripodControlOption.Length)
                _TripodMode = TripodControlOption.Rotation;
            else
                _TripodMode = TripodControlOption.Length;
        }
        #endregion

        private void ExchangeSurveyingWindow()
        {
            if (_IsShowSurveyingView)
                _IsShowSurveyingView = false;
            else
                _IsShowSurveyingView = true;
        }





        protected void Bloom_LoadContent()
        {
            // spriteBatch = new SpriteBatch(ScreenManager.GraphicsDevice);
            // 這裡要改一下
            spriteBatch = ScreenManager.SpriteBatch;

            bloomExtractEffect = _content.Load<Effect>("BloomExtract");
            bloomCombineEffect = _content.Load<Effect>("BloomCombine");
            gaussianBlurEffect = _content.Load<Effect>("GaussianBlur");


            // Look up the resolution and format of our main backbuffer.
            PresentationParameters pp = ScreenManager.GraphicsDevice.PresentationParameters;

            int width = pp.BackBufferWidth;
            int height = pp.BackBufferHeight;

            SurfaceFormat format = pp.BackBufferFormat;

            // Create a texture for reading back the backbuffer contents.
            resolveTarget = new ResolveTexture2D(ScreenManager.GraphicsDevice, width, height, 1,
                format);

            // Create two rendertargets for the bloom processing. These are half the
            // size of the backbuffer, in order to minimize fillrate costs. Reducing
            // the resolution in this way doesn't hurt quality, because we are going
            // to be blurring the bloom images in any case.
            width /= 2;
            height /= 2;

            renderTarget1 = new RenderTarget2D(ScreenManager.GraphicsDevice, width, height, 1,
                format);
            renderTarget2 = new RenderTarget2D(ScreenManager.GraphicsDevice, width, height, 1,
                format);
        }


        protected  void LensFlare_LoadContent()
        {
            // Create a SpriteBatch for drawing the glow and flare sprites.
            spriteBatch = new SpriteBatch(ScreenManager.GraphicsDevice);

            // Load the glow and flare textures.
            glowSprite = _content.Load<Texture2D>("glow");

            foreach (Flare flare in flares)
            {
                flare.Texture = _content.Load<Texture2D>(flare.TextureName);
            }

            // Effect and vertex declaration for drawing occlusion query polygons.
            basicEffect = new BasicEffect(ScreenManager.GraphicsDevice, null);

            basicEffect.View = Matrix.Identity;
            basicEffect.VertexColorEnabled = true;

            vertexDeclaration = new VertexDeclaration(ScreenManager.GraphicsDevice,
                                                VertexPositionColor.VertexElements);

            // Create vertex data for the occlusion query polygons.
            queryVertices = new VertexPositionColor[4];

            queryVertices[0].Position = new Vector3(-querySize / 2, -querySize / 2, -1);
            queryVertices[1].Position = new Vector3(querySize / 2, -querySize / 2, -1);
            queryVertices[2].Position = new Vector3(querySize / 2, querySize / 2, -1);
            queryVertices[3].Position = new Vector3(-querySize / 2, querySize / 2, -1);

            // Create the occlusion query object.
            occlusionQuery = new OcclusionQuery(ScreenManager.GraphicsDevice);
        }




        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        /// 
        public override void LoadContent(ContentManager content, SystemState _State)
        {









            State = _State; 
            //if (content == null)
            //    content = new ContentManager(ScreenManager.Game.Services, "Content");

            _content = content; 


            gameFont = _content.Load<SpriteFont>("gamefont");
            _SideTexture = _content.Load<Texture2D>("gradient");

            //Main Viewport
            _MainViewport = ScreenManager.GraphicsDevice.Viewport;
            //Surveying Viewport
            _SurveyingViewport = new Viewport();
            _SurveyingViewport.X = _InfoDisplayArea.Width / 2 - 150 + 100;
            _SurveyingViewport.Y = _InfoDisplayArea.Height / 2 - 133;
            _SurveyingViewport.Width = 300;
            _SurveyingViewport.Height = 266;

            _SceneController = new SceneController( State, _MainViewport,_SurveyingViewport, ScreenManager.GraphicsDevice);
            // once the load has finished, we use ResetElapsedTime to tell the game's
            // timing mechanism that we have just finished a very long frame, and that
            // it should not try to catch up.
            ScreenManager.Game.ResetElapsedTime();

            

            // A real game would probably have more content than this sample, so
            // it would take longer to load. We simulate that by delaying for a
            // while, giving you a chance to admire the beautiful loading screen.
            Thread.Sleep(1000);
// skc, 改變順序
            _SceneController.BeginAddInstrument(content);

            _SceneController.AddInstrument(SimuSurvey360.Instruments.InstrumentType.TotalStation, 1);
            _SceneController.AddInstrument(SimuSurvey360.Instruments.InstrumentType.Leveling, 1);
            _SceneController.AddInstrument(SimuSurvey360.Instruments.InstrumentType.Theodolite, 1);

            _SceneController.AddInstrument(SimuSurvey360.Instruments.InstrumentType.Ruler, 1);
            _SceneController.AddInstrument(SimuSurvey360.Instruments.InstrumentType.Ruler, 2);
            _SceneController.AddInstrument(SimuSurvey360.Instruments.InstrumentType.Ruler, 3);
            _SceneController.AddInstrument(SimuSurvey360.Instruments.InstrumentType.Ruler, 4);

            _SceneController.AddInstrument(SimuSurvey360.Instruments.InstrumentType.Pole, 1);
            _SceneController.AddInstrument(SimuSurvey360.Instruments.InstrumentType.Pole, 2);
            _SceneController.AddInstrument(SimuSurvey360.Instruments.InstrumentType.Pole, 3);
            _SceneController.AddInstrument(SimuSurvey360.Instruments.InstrumentType.Pole, 4);

            _SceneController.AddInstrument(SimuSurvey360.Instruments.InstrumentType.Mirror, 1);
            _SceneController.AddInstrument(SimuSurvey360.Instruments.InstrumentType.Mirror, 2);
            _SceneController.AddInstrument(SimuSurvey360.Instruments.InstrumentType.Mirror, 3);
            _SceneController.AddInstrument(SimuSurvey360.Instruments.InstrumentType.Mirror, 4);

            _SceneController.AddInstrument(SimuSurvey360.Instruments.InstrumentType.HintBox, 0);
            _SceneController.AddInstrument(SimuSurvey360.Instruments.InstrumentType.HintBox, 1);
            _SceneController.AddInstrument(SimuSurvey360.Instruments.InstrumentType.HintBox, 2);
            _SceneController.AddInstrument(SimuSurvey360.Instruments.InstrumentType.HintBox, 3);
            _SceneController.AddInstrument(SimuSurvey360.Instruments.InstrumentType.HintBox, 4);

            _SceneController.AddInstrument(SimuSurvey360.Instruments.InstrumentType.SetPoint, 0);
            _SceneController.AddInstrument(SimuSurvey360.Instruments.InstrumentType.SetPoint, 1);
            _SceneController.AddInstrument(SimuSurvey360.Instruments.InstrumentType.SetPoint, 2);
            _SceneController.AddInstrument(SimuSurvey360.Instruments.InstrumentType.SetPoint, 3);
            _SceneController.AddInstrument(SimuSurvey360.Instruments.InstrumentType.SetPoint, 4);

            _SceneController.AddInstrument(SimuSurvey360.Instruments.InstrumentType.RulerPad, 1);
            _SceneController.AddInstrument(SimuSurvey360.Instruments.InstrumentType.RulerPad, 2);

            _Accessory_Ns = 3;

            _SceneController.EndAddInstrument();

            SetInstrumentWithName("TotalStation_1");


            for (int i = 0; i < 6; i++)
            {
                _Camera[i] = new Camera("MainCamera",
                    ScreenManager.GraphicsDevice.Viewport.Width,
                    ScreenManager.GraphicsDevice.Viewport.Height,
                    ScreenManager.GraphicsDevice.Viewport.AspectRatio,
                    MathHelper.Pi / 3f,
                    // MathHelper.PiOver2 * 1.9f ,
                    0.05f,  // 設太小的話 Zoom 到遠處會閃
                    10000,
                    true);

                _Camera[i].Initialize(new Vector3(30, 35f, 130), new Vector3(0, 35f, 70));
            }
            _currentCamera = _Camera[0];
            _TotalstationCamera = _SceneController._TotalstationCamera;



            // Bloom
            Bloom_LoadContent();
            LensFlare_LoadContent();


        }


        // CameraN : -1 = Next 
        public void ChangeCamera( int CameraN )
        {

            // _SceneController.ChangeCamera( CameraN );
            //if (CameraN == -1)
            //{
            //    if (CameraN < 6)
            //        CameraN += 1;
            //    else
            //        CameraN = 0;
            //    _currentCamera = _Camera[CameraN];
            //}
            // skc test 
            // remove later
            if (CameraN == -1)
            {
                if (_currentCamera == _Camera[0])
                    _currentCamera = _TotalstationCamera;
                else
                    _currentCamera = _Camera[0];
            }
            else
            {
                switch (CameraN)
                {
                    case 0:
                        _currentCamera = _Camera[0];
                        ((TotalStation)(_SceneController.SelectedInstrument)).HideTelescope = false; 
                        break;
                    case 1:
                        _currentCamera = _TotalstationCamera;
                        ((TotalStation)(_SceneController.SelectedInstrument)).HideTelescope = true ; 
                        break;
                }
            }



        }

        public float GetHeight(Vector3 _v3)
        {
            return _SceneController.GetHeight(_v3);
        }

        public void SetCamera(Vector3 eyePosition, Vector3 TargetPosition)
        {
            _Camera[0].Initialize(eyePosition, TargetPosition);
        }


        public void ChangeTerrain( Texture2D tTerrain, Texture2D tTexture)
        {

            // _SceneController.ChangeTerrain(tTerrain, tTexture);

        }

        public void ChangeTankTerrain( int _idx )
        {

            _SceneController.ChangeTankTerrain( _idx );

        }

        public void moveVisualObject(string Name, Matrix  _Matrix)
        {
            _SceneController.moveVisualObject( Name, _Matrix);
        }

        public void setVisualObject(string Name, bool isVisible )
        {
            _SceneController.setVisualObject(Name, isVisible);
        }

        public Vector3 GetInstrumentPositionWithName(String name)
        {
            return _SceneController.GetInstrumentPositionWithName(name);

        }


        public void SetInstrumentWithName(String name)
        {

            _SceneController.SelectInstrumentWithName( name );

        }

        public void SetInstrumentError(float _eX, float _eY, float _eZ )
        {

            // 這裡會跟 Game1 的 Draw 裡面呼叫 WithName 互相影響, 改寫為下面那一段
            //Instrument TmpSelectedIns = _SceneController.SelectedInstrument ; 
            // _SceneController.SelectInstrumentWithName(@"TotalStation_1");
            //((TotalStation)(_SceneController.SelectedInstrument)).Error_X = _eX;
            //((TotalStation)(_SceneController.SelectedInstrument)).Error_Y = _eY;
            //((TotalStation)(_SceneController.SelectedInstrument)).Error_Z = _eZ;
            //_SceneController.SelectedInstrument = TmpSelectedIns;


            ((TotalStation)(_SceneController.PtrInstrument)).Error_X = _eX;
            ((TotalStation)(_SceneController.PtrInstrument)).Error_Y = _eY;
            ((TotalStation)(_SceneController.PtrInstrument)).Error_Z = _eZ;

        }
        public void SetInstrumentRadarIndex(int _i)
        {
            _SceneController.SelectedInstrument.RadarIndex = _i;
        }
        public int GetInstrumentRadarIndex()
        {
            return _SceneController.SelectedInstrument.RadarIndex ;
        }

        public void ReSetInstrumentLength()
        {
        _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.ObjLength, 0f );
        }

// 選了再設定位置
        public float SetInstrument( Vector3 Pos)
        {
            float _h;
            _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.SetTranslation, (Object)Pos);
            float dx = InstrumentPos.X - _SceneController.SelectedInstrument.WorldPosition.X;
            float dz = InstrumentPos.Z - _SceneController.SelectedInstrument.WorldPosition.Z;
            Vector3 _p = new Vector3(dx, 0f, dz);
            if ((_SceneController.SelectedInstrument.Type == InstrumentType.TotalStation)||
                (_SceneController.SelectedInstrument.Type == InstrumentType.Leveling)||
                (_SceneController.SelectedInstrument.Type == InstrumentType.Theodolite))
            {
                // 記住 Instrument 位置( X,Z ) -> 稍後改成不需要參考
                InstrumentPos = _SceneController.SelectedInstrument.WorldPosition;

            }
            else if ((_SceneController.SelectedInstrument.Type == InstrumentType.Ruler) || (_SceneController.SelectedInstrument.Type == InstrumentType.Pole )
                || (_SceneController.SelectedInstrument.Type == InstrumentType.Mirror)
                || (_SceneController.SelectedInstrument.Type == InstrumentType.HintBox)) 
            {
                // 自動將標尺面對儀器

                float testv = (float)Math.Atan(dx / dz);
                float _Angle = MathHelper.ToDegrees((float)Math.Atan(dx / dz));
                if (dz < 0) _Angle += 180;
                _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.SetRotationY, _Angle);
            }


                _h = _SceneController.GetHeight(Pos );
                return _h;
                // _SceneController.SelectedInstrument.WorldYOffset = _h;
             


        }
        

        private void Bloom_UnloadContent()
        {
            resolveTarget.Dispose();
            renderTarget1.Dispose();
            renderTarget2.Dispose();
        }
        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            Bloom_UnloadContent();
            _content.Unload();
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update( GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
            // skc, remove later
            if (IsActive)
            {
                //const float randomization = 10;
                //enemyPosition.X += (float)(random.NextDouble() - 0.5) * randomization;
                //enemyPosition.Y += (float)(random.NextDouble() - 0.5) * randomization;


                //Vector2 targetPosition = new Vector2(200, 200);

                //enemyPosition = Vector2.Lerp(enemyPosition, targetPosition, 0.05f);
                _TotalstationCamera = _SceneController._TotalstationCamera; 
                _SceneController.Update( _currentCamera, gameTime);
            }
        }


        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput( SystemState State, InputState Input, GameTime gameTime )
        {
            if ( Input == null)
                throw new ArgumentNullException("Input");


            if (( State.CurrState != SystemState.S_State.STATE_NAVIGATION ) &&
                ( State.CurrState != SystemState.S_State.STATE_MENU_SURVEYING ))
            {
                return; 
            }



            // Look up inputs for the active player profile.
            int playerIndex = (int)ControllingPlayer.Value;

            KeyboardState keyboardState = Input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = Input.CurrentGamePadStates[playerIndex];

            // The game pauses either if the user presses the pause button, or if
            // they unplug the active gamepad. This requires us to keep track of
            // whether a gamepad was ever plugged in, because we don't want to pause
            // on PC if they are playing with a keyboard and have no gamepad at all!
            // the GamePad must be unpluged

            bool gamePadDisconnected = !gamePadState.IsConnected &&
                                        Input.GamePadWasConnected[playerIndex];

            float time = (float)gameTime.ElapsedGameTime.TotalMilliseconds;

#region KeyBoardInput
            // PC Keyboard


            // 以下都是判斷狀態, 非一組Down/Up, 暫時用另外的讀取, 狀態轉換應該採用 input 
            KeyboardState newState;  
            newState = Keyboard.GetState();


            _ray.Position = _currentCamera.Position;
            _ray.Direction = _currentCamera.Direction;

            // switch(_Mode)
            switch ( State.CurrState )
            {
            // case OperatingMode.Navigation:
                case SystemState.S_State.STATE_NAVIGATION:

                    if (State.OperationMode == 0)
                    {
                        if (State.PickedObj != -1)
                        {
                            if ((newState.IsKeyDown(Keys.W)) || (newState.IsKeyDown(Keys.Up)))
                                _currentCamera.MoveForwardHorizontal(time * 0.06f);
                            else if ((newState.IsKeyDown(Keys.S)) || (newState.IsKeyDown(Keys.Down)))
                                _currentCamera.MoveForwardHorizontal(time * -0.06f);
                        }
                        else
                        {

                            if ((newState.IsKeyDown(Keys.W))|| (newState.IsKeyDown(Keys.Up)))
                                _currentCamera.MoveForward(time * 0.06f);
                            else if ((newState.IsKeyDown(Keys.S)) || (newState.IsKeyDown(Keys.Down)))
                                _currentCamera.MoveForward(time * -0.06f);
                        }

                        if ((newState.IsKeyDown(Keys.A)) || (newState.IsKeyDown(Keys.Left)))
                                _currentCamera.Turning(time * -0.03f);
                        else if ((newState.IsKeyDown(Keys.D)) || (newState.IsKeyDown(Keys.Right)))
                                _currentCamera.Turning(time * 0.03f);

                            else if (newState.IsKeyDown(Keys.Z))
                                _currentCamera.MoveUpDown(time * 0.03f);
                            else if (newState.IsKeyDown(Keys.X))
                                _currentCamera.MoveUpDown(time * -0.03f);

                            if ((State.Ready2PickObj == -1) || (State.Ready2PickObj > 0))
                            {
                                if (newState.IsKeyDown(Keys.T))
                                    _currentCamera.RotateVertical(time * 0.03f);
                                else if (newState.IsKeyDown(Keys.G))
                                    _currentCamera.RotateVertical(time * -0.03f);
                            }


                            // 多一個操作 更好用
                            if ((State.PickedObj == -1) && (State.Ready2PickObj == -1))
                            {
                                if (newState.IsKeyDown(Keys.I))
                                    _currentCamera.RotateVertical(time * 0.03f);
                                if (newState.IsKeyDown(Keys.K))
                                    _currentCamera.RotateVertical(time * -0.03f);
                            }

                        if (State.PickedObj != -1)
                        {
                            // 之前已經選好, 根據 Camera 移動
                            // 參考 _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.IncreaseTranslation, new Vector3(0, 0, time * -0.03f));



                            _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.Attach2Camera, new Vector3(0, 0, 0));
                        
                        
                        
                        }

                        else if (State.Ready2PickObj != -1)
                        {
                            switch (State.Ready2PickObj)
                            {
                                case  0:

                                    if (newState.IsKeyDown(Keys.I))
                                        _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.ObjLength, time * 0.003f);
                                    else if (newState.IsKeyDown(Keys.K))
                                        _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.ObjLength, time * -0.003f);

                                    if (State.InsType != InstrumentType.Leveling )
                                    {
                                        if (newState.IsKeyDown(Keys.T))
                                            _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.TelescopeRotation, time * -0.03f);
                                        else if (newState.IsKeyDown(Keys.G))
                                            _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.TelescopeRotation, time * 0.03f);
                                    }

                                    if (newState.IsKeyDown(Keys.F))
                                        _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.UpperBodyRotation, time * 0.03f);
                                    else if (newState.IsKeyDown(Keys.H))
                                        _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.UpperBodyRotation, time * -0.03f);



                                    break;
                                case 1: case 2: case 3: case 4:

                                    if (_SceneController.SelectedInstrument.Type == InstrumentType.Ruler)
                                    {
                                        Ruler ruler = (Ruler)_SceneController.SelectedInstrument;
                                        if (newState.IsKeyDown(Keys.I))
                                        {
                                            if (ruler.RulerLength < ruler.RulerLengthMax)
                                            {
                                                float _len = ruler.RulerLength + 0.3f;
                                                _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.ObjLength, _len);

                                            }
                                        }
                                        else if (newState.IsKeyDown(Keys.K))
                                        {
                                            if (ruler.RulerLength > 0.3)
                                                _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.ObjLength, ruler.RulerLength - 0.3f);

                                        }

                                    }
                                    break;
                                case 21: case 22: case 23: case 24:
                                    // InstrumentType.Pole
                                    // {
                                        Pole pole = (Pole)_SceneController.SelectedInstrument;
                                        if (newState.IsKeyDown(Keys.I))
                                        {
                                            if (pole.RulerLength < pole.RulerLengthMax)
                                            {
                                                float _len = pole.RulerLength + 0.3f;
                                                _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.ObjLength, _len);

                                            }
                                        }
                                        else if (newState.IsKeyDown(Keys.K))
                                        {
                                            if (pole.RulerLength > 0.3)
                                                _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.ObjLength, pole.RulerLength - 0.3f);

                                        }

                                    // }




                                    break;

                                case 101: case 102: case 103: case 104:

                                    Mirror mirror = (Mirror)_SceneController.SelectedInstrument;
                                    if (newState.IsKeyDown(Keys.I))
                                    {
                                        if (mirror.RulerLength < mirror.RulerLengthMax)
                                        {
                                            float _len = mirror.RulerLength + 0.3f;
                                            _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.ObjLength, _len);

                                        }
                                    }
                                    else if (newState.IsKeyDown(Keys.K))
                                    {
                                        if (mirror.RulerLength > 0.3)
                                            _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.ObjLength, mirror.RulerLength - 0.3f);

                                    }
                                    break;



                            }

                        }







                    }
                    else
                    {



                        switch (State.CurrNaviState)
                        {
                            case SystemState.S_State_Navigation.STATE_ME:



                                //int _deltaX, _deltaY, _deltaZ;
                                //_currentState = Mouse.GetState();
                                //_deltaX = _currentState.X - _preState.X;
                                //_deltaY = _currentState.Y - _preState.Y;
                                //_deltaZ = _currentState.ScrollWheelValue - _preState.ScrollWheelValue;

                                //Mouse.SetPosition(ScreenManager.GraphicsDevice.Viewport.Width / 2, ScreenManager.GraphicsDevice.Viewport.Height / 2);
                                //_preState = Mouse.GetState();

                                //_currentCamera.RotateVertical(_deltaY / 10);
                                //_currentCamera.Turning(_deltaX / 10);
                                //_currentCamera.Zoom(-_deltaZ / 10);



                                if (newState.IsKeyDown(Keys.W))
                                    _currentCamera.MoveForward(time * 0.06f);
                                else if (newState.IsKeyDown(Keys.S))
                                    _currentCamera.MoveForward(time * -0.06f);
                                else if (newState.IsKeyDown(Keys.A))
                                    // _currentCamera.MoveHorizontal(time * 0.06f);
                                    _currentCamera.Turning(time * -0.03f);
                                else if (newState.IsKeyDown(Keys.D))
                                    // _currentCamera.MoveHorizontal(time * -0.06f);
                                    _currentCamera.Turning(time * 0.03f);

                                else if (newState.IsKeyDown(Keys.Z))
                                    _currentCamera.MoveUpDown(time * 0.03f);
                                else if (newState.IsKeyDown(Keys.X))
                                    _currentCamera.MoveUpDown(time * -0.03f);





                                if (newState.IsKeyDown(Keys.T))
                                    _currentCamera.RotateVertical(time * 0.03f);
                                else if (newState.IsKeyDown(Keys.G))
                                    _currentCamera.RotateVertical(time * -0.03f);


                                // Right Thumbstick, 上下

                                //if (newState.IsKeyDown(Keys.I))
                                //    _SceneController.UserPan(SceneController.Direction.Up, 1);
                                //else if (newState.IsKeyDown(Keys.K))
                                //    _SceneController.UserPan(SceneController.Direction.Down, 1);


                                break;
                            case SystemState.S_State_Navigation.STATE_INSTRUMENT:
                            case SystemState.S_State_Navigation.STATE_RULE1:
                            case SystemState.S_State_Navigation.STATE_RULE2:
                            case SystemState.S_State_Navigation.STATE_RULE3:

                                // 有問題, 應該以自己為基準
                                if (newState.IsKeyDown(Keys.W))
                                    _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.IncreaseTranslation, new Vector3(0, 0, time * -0.03f));
                                else if (newState.IsKeyDown(Keys.S))
                                    _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.IncreaseTranslation, new Vector3(0, 0, time * 0.03f));
                                else if (newState.IsKeyDown(Keys.A))
                                    _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.IncreaseTranslation, new Vector3(time * -0.03f, 0, 0));
                                else if (newState.IsKeyDown(Keys.D))
                                    _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.IncreaseTranslation, new Vector3(time * 0.03f, 0, 0));

                                // 移動後(才) 算 Ruler 對準 Instrument 角度
                                // Gamepad 那邊不需要再做一次
                                if ((_SceneController.SelectedInstrument.Type == InstrumentType.TotalStation) ||
                                    (_SceneController.SelectedInstrument.Type == InstrumentType.Leveling) ||
                                    (_SceneController.SelectedInstrument.Type == InstrumentType.Theodolite))
                                {
                                    // 記住 Instrument 位置( X,Z )
                                    InstrumentPos = _SceneController.SelectedInstrument.WorldPosition;



                                    // Right Thumbstick
                                    switch (_TripodMode)
                                    {
                                        case TripodControlOption.Rotation: //Rotation
                                            _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.TripodRotation, gamePadState.ThumbSticks.Right.Y);
                                            break;
                                        case TripodControlOption.Length: //Length
                                            if (newState.IsKeyDown(Keys.T))
                                                _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.ObjLength, time * 0.01f);
                                            else if (newState.IsKeyDown(Keys.G))
                                                _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.ObjLength, time * -0.01f);



                                            if (newState.IsKeyDown(Keys.I))
                                                _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.TelescopeRotation, time * -0.03f);
                                            else if (newState.IsKeyDown(Keys.K))
                                                _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.TelescopeRotation, time * 0.03f);


                                            if (newState.IsKeyDown(Keys.J))
                                                _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.UpperBodyRotation, time * 0.03f);
                                            else if (newState.IsKeyDown(Keys.L))
                                                _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.UpperBodyRotation, time * -0.03f);


                                            // skc, 腳架往外
                                            // _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.TripodRotation, time * 0.03f);

                                            break;

                                    }

                                }
                                else if (_SceneController.SelectedInstrument.Type == InstrumentType.Ruler)
                                {
                                    // 自動將標尺面對儀器


                                    //float dx = InstrumentPos.X - _SceneController.SelectedInstrument.WorldPosition.X;
                                    //float dz = InstrumentPos.Z - _SceneController.SelectedInstrument.WorldPosition.Z;
                                    //float testv = (float)Math.Atan(dx / dz);
                                    //float _Angle = MathHelper.ToDegrees((float)Math.Atan(dx / dz));
                                    //_SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.SetRotationY, _Angle);

                                    AutoFace2Ins();

                                    Ruler ruler = (Ruler)_SceneController.SelectedInstrument;
                                    if (newState.IsKeyDown(Keys.I))
                                    {
                                        if (ruler.RulerLength < ruler.RulerLengthMax)
                                        {
                                            float _len = ruler.RulerLength + 0.3f;
                                            _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.ObjLength, _len);

                                        }
                                    }
                                    else if (newState.IsKeyDown(Keys.K))
                                    {
                                        if (ruler.RulerLength > 0.3)
                                            _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.ObjLength, ruler.RulerLength - 0.3f);

                                    }
                                }
                                else if (_SceneController.SelectedInstrument.Type == InstrumentType.Pole)
                                {
                                    // 不需要面對儀器

                                    Pole pole = (Pole)_SceneController.SelectedInstrument;
                                    if (newState.IsKeyDown(Keys.I))
                                    {
                                        if (pole.RulerLength < pole.RulerLengthMax)
                                        {
                                            float _len = pole.RulerLength + 0.3f;
                                            _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.ObjLength, _len);

                                        }
                                    }
                                    else if (newState.IsKeyDown(Keys.K))
                                    {
                                        if (pole.RulerLength > 0.3)
                                            _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.ObjLength, pole.RulerLength - 0.3f);

                                    }
                                }
                                else if (_SceneController.SelectedInstrument.Type == InstrumentType.Pole)
                                {
                                    // 自動面對儀器
                                    AutoFace2Ins();
                                    Mirror mirror = (Mirror)_SceneController.SelectedInstrument;
                                    if (newState.IsKeyDown(Keys.I))
                                    {
                                        if (mirror.RulerLength < mirror.RulerLengthMax)
                                        {
                                            float _len = mirror.RulerLength + 0.3f;
                                            _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.ObjLength, _len);

                                        }
                                    }
                                    else if (newState.IsKeyDown(Keys.K))
                                    {
                                        if (mirror.RulerLength > 0.3)
                                            _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.ObjLength, mirror.RulerLength - 0.3f);

                                    }
                                }

                                break;

                        }
                    }


            break;

            // case OperatingMode.Instrument:
                case SystemState.S_State.STATE_MENU_SURVEYING:

            if (State.InsType != InstrumentType.Leveling )
            {

                float VA = TelescopeRotationValue;
                int PrevSide = 0;
                VA = (VA + 90) % 360;
                if (VA < 0) VA += 360;
                float PrevVA = VA;

                float _amount = 0.03f;
                if (((VA > 89f) && (VA < 91f)) || ((VA > 269f) && (VA < 271f)))
                    _amount = 0.003f;

                if (VA <= 180)
                {
                    if (newState.IsKeyDown(Keys.T))
                        _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.TelescopeRotation, time * -_amount);
                    else if (newState.IsKeyDown(Keys.G))
                        _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.TelescopeRotation, time * _amount);
                    else if (newState.IsKeyDown(Keys.Up))
                        _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.TelescopeRotation, time * -_amount * 0.01f );
                    else if (newState.IsKeyDown(Keys.Down))
                        _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.TelescopeRotation, time * _amount * 0.01f );
                }
                else
                {
                    if (newState.IsKeyDown(Keys.T))
                        _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.TelescopeRotation, time * _amount);
                    else if (newState.IsKeyDown(Keys.G))
                        _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.TelescopeRotation, time * -_amount);
                    else if (newState.IsKeyDown(Keys.Up))
                        _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.TelescopeRotation, time * _amount * 0.01f);
                    else if (newState.IsKeyDown(Keys.Down))
                        _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.TelescopeRotation, time * -_amount * 0.01f);
                }

                VA = TelescopeRotationValue;
                VA = (VA + 90) % 360;
                if (VA < 0) VA += 360;
                if ((PrevVA < 5f) && (VA > 355f))
                {
                    VA = 355f;
                    TelescopeRotationValue = VA - 90;
                }
                else if ((VA < 5f) && (PrevVA > 355f))
                {
                    VA = 5f;
                    TelescopeRotationValue = VA - 90;
                }
                else if ((PrevVA <= 180f) && (VA > 180f))
                {
                    VA = 185f;
                    TelescopeRotationValue = VA - 90;
                }
                else if ((VA <= 180f) && (PrevVA > 180f))
                {
                    VA = 175f;
                    TelescopeRotationValue = VA - 90;
                }


            }


// Bloom 測試中

            if (newState.IsKeyDown(Keys.Y))
            {
/*
                if ( Settings.BaseIntensity > 0.1f ) 
                Settings.BaseIntensity -= 0.1f;  
                else if ( Settings.BaseIntensity > 0f )
                    Settings.BaseIntensity = 0.0001f;  
 
*/ 
          

            Settings.BloomThreshold = 0f;
            Settings.BlurAmount = 2f;
            Settings.BloomIntensity = 1f;
            Settings.BaseIntensity = 0.1f;
            Settings.BloomSaturation = 1f;
            Settings.BaseSaturation = 1f;


            }
            else if (newState.IsKeyDown(Keys.U))
            {
/*
                if (Settings.BaseIntensity < 0.8f)
                    Settings.BaseIntensity += 0.1f; 
*/

                Settings.BloomThreshold = 0.6f;
                Settings.BlurAmount = 4f;
                Settings.BloomIntensity = 2f;
                Settings.BaseIntensity = 1f;
                Settings.BloomSaturation = 0.8f;
                Settings.BaseSaturation = 0.7f;

 }




            if (newState.IsKeyDown(Keys.A))
                _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.UpperBodyRotation, time * 0.03f);
            else if (newState.IsKeyDown(Keys.D))
                _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.UpperBodyRotation, time * -0.03f);
            if (newState.IsKeyDown(Keys.Left))
                _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.UpperBodyRotation, time * 0.0001f);
            else if (newState.IsKeyDown(Keys.Right))
                _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.UpperBodyRotation, time * -0.0001f);

            else if (newState.IsKeyDown(Keys.I))
                _currentCamera.Zoom(0.97f);
            else if (newState.IsKeyDown(Keys.K))
                _currentCamera.Zoom(1.03f);

            //_ray.Position = _SceneController._TotalstationCamera.Position;
            //_ray.Direction = _SceneController._TotalstationCamera.Direction;



            break;

            break;
            }

            //PlayerIndex _playerIndex;
            //if (Input.IsNewKeyPress(Keys.P))
            //{
            //    _Mode = OperatingMode.Navigation;
            //}
            //else if (Input.IsNewKeyPress(Keys.O))
            //{
            //    _Mode = OperatingMode.Instrument;
            //}





#endregion

#region GamePadInput

            // Xbox 360 GamePad

            //if ((Input.IsPauseGame(ControllingPlayer)) || gamePadDisconnected)
            //{
            //    // 座標畫面,  奇怪, 看起來會不斷的呼叫, check !
            //    ScreenManager.AddScreen(new InfoScreen(_SceneController.WarpInstrment()), ControllingPlayer);
            //}
            //else
            {
                switch ( State.CurrState )
                // switch(_Mode)
                {
                case SystemState.S_State.STATE_NAVIGATION:

                        if (State.OperationMode == 0)
                        {


                            if (State.PickedObj != -1)
                            {
                                if (gamePadState.ThumbSticks.Left.Y != 0)
                                    _currentCamera.MoveForwardHorizontal(gamePadState.ThumbSticks.Left.Y);

                            }
                            else
                            {

                                if (gamePadState.ThumbSticks.Left.Y != 0)
                                    _currentCamera.MoveForward(gamePadState.ThumbSticks.Left.Y);

                            }

                            if (gamePadState.ThumbSticks.Left.X != 0)
                                _currentCamera.Turning(gamePadState.ThumbSticks.Left.X);



                            if ((State.Ready2PickObj == -1) || (State.Ready2PickObj > 0))
                            {


                                if (Input.CurrentGamePadStates[0].IsButtonDown(Buttons.DPadUp))
                                    _currentCamera.RotateVertical(time * -0.03f);

                                if (Input.CurrentGamePadStates[0].IsButtonDown(Buttons.DPadDown))
                                    _currentCamera.RotateVertical(time * +0.03f);



                            }

                            // 多一個操作 更好用
                            if ((State.PickedObj == -1) && (State.Ready2PickObj == -1))
                            {
                                _currentCamera.RotateVertical(-gamePadState.ThumbSticks.Right.Y);
                            }


                            if (State.PickedObj != -1)
                            {
                                // 之前已經選好, 根據 Camera 移動
                                // 參考 _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.IncreaseTranslation, new Vector3(0, 0, time * -0.03f));



                                _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.Attach2Camera, new Vector3(0, 0, 0));



                            }

                            else if (State.Ready2PickObj != -1)
                            {
                                switch (State.Ready2PickObj)
                                {
                                    case 0:

                                        if ( gamePadState.ThumbSticks.Right.Y != 0f ) 
                                        _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.ObjLength, gamePadState.ThumbSticks.Right.Y * 0.1f );

                                        if (State.InsType != InstrumentType.Leveling )
                                        {
                                            if (Input.CurrentGamePadStates[0].IsButtonDown(Buttons.DPadUp))
                                                _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.TelescopeRotation, time * 0.01f);

                                            if (Input.CurrentGamePadStates[0].IsButtonDown(Buttons.DPadDown))
                                                _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.TelescopeRotation, time * -0.01f);
                                        }


                                        if (Input.CurrentGamePadStates[0].IsButtonDown(Buttons.DPadLeft))
                                            _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.UpperBodyRotation, time * 0.01f);

                                        if (Input.CurrentGamePadStates[0].IsButtonDown(Buttons.DPadRight))
                                            _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.UpperBodyRotation, time * -0.01f);



                                        break;
                                    case 1:
                                    case 2:
                                    case 3:
                                    case 4:
                                        //if (_SceneController.SelectedInstrument.Type == InstrumentType.Ruler)
                                        //{
                                            Ruler ruler = (Ruler)_SceneController.SelectedInstrument;

                                            if (gamePadState.ThumbSticks.Right.Y != 0f)
                                                _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.ObjLength, ruler.RulerLength + gamePadState.ThumbSticks.Right.Y);



                                        //}
                                        //else // InstrumentType.Pole
                                        //{

                                        //}




                                        break;

                                    case 21:
                                    case 22:
                                    case 23:
                                    case 24:


                                            Pole pole = (Pole)_SceneController.SelectedInstrument;
                                            if (gamePadState.ThumbSticks.Right.Y != 0f)
                                                _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.ObjLength, pole.RulerLength + gamePadState.ThumbSticks.Right.Y * 0.5f );



                                        break;
                                    case 101: case 102: case 103: case 104:

                                        Mirror mirror = (Mirror)_SceneController.SelectedInstrument;

                                        if (gamePadState.ThumbSticks.Right.Y != 0f)
                                            _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.ObjLength, mirror.RulerLength + gamePadState.ThumbSticks.Right.Y);



                                        break; 
                                }

                            }




// 改到這裡為止





                        }
                        else
                        {

                            switch (State.CurrNaviState)
                            {
                                case SystemState.S_State_Navigation.STATE_ME:

                                    // case OperatingMode.Navigation:

                                    // Left Thumbstick, 自己在平面上移動 

                                    if (gamePadState.ThumbSticks.Left.Y != 0)
                                        _currentCamera.MoveForward(gamePadState.ThumbSticks.Left.Y);

                                    if (gamePadState.ThumbSticks.Left.X != 0)
                                        _currentCamera.Turning(gamePadState.ThumbSticks.Left.X);

                                    if (gamePadState.ThumbSticks.Right.Y != 0)
                                        _currentCamera.RotateVertical(-gamePadState.ThumbSticks.Right.Y);


                                    if (Input.CurrentGamePadStates[0].IsButtonDown(Buttons.DPadUp))
                                        _currentCamera.MoveUpDown(time * 0.03f);

                                    if (Input.CurrentGamePadStates[0].IsButtonDown(Buttons.DPadDown))
                                        _currentCamera.MoveUpDown(time * -0.03f);



                                    break;

                                case SystemState.S_State_Navigation.STATE_INSTRUMENT:
                                case SystemState.S_State_Navigation.STATE_RULE1:
                                case SystemState.S_State_Navigation.STATE_RULE2:
                                    // case OperatingMode.Instrument:

                                    // Left Thumbstick
                                    // 有問題, 應該以自己為基準
                                    if (gamePadState.ThumbSticks.Left.Y != 0)
                                        _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.IncreaseTranslation, new Vector3(0, 0, -gamePadState.ThumbSticks.Left.Y));
                                    if (gamePadState.ThumbSticks.Left.X != 0)
                                        _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.IncreaseTranslation, new Vector3(gamePadState.ThumbSticks.Left.X, 0, 0));


                                    if (_SceneController.SelectedInstrument.Type == InstrumentType.TotalStation)
                                    {
                                        // Right Thumbstick
                                        switch (_TripodMode)
                                        {
                                            case TripodControlOption.Rotation: //Rotation, skc, remove later
                                                _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.TripodRotation, gamePadState.ThumbSticks.Right.Y);
                                                break;
                                            case TripodControlOption.Length: //Length
                                                // _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.ObjLength, gamePadState.ThumbSticks.Right.Y);
                                                if (Input.CurrentGamePadStates[0].IsButtonDown(Buttons.DPadUp))
                                                    _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.ObjLength, time * 0.01f);
                                                if (Input.CurrentGamePadStates[0].IsButtonDown(Buttons.DPadDown))
                                                    _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.ObjLength, time * -0.01f);

                                                break;
                                        }
                                        _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.TelescopeRotation, gamePadState.ThumbSticks.Right.Y);
                                        _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.UpperBodyRotation, gamePadState.ThumbSticks.Left.X);




                                    }
                                    else if (_SceneController.SelectedInstrument.Type == InstrumentType.Ruler)
                                    {
                                        Ruler ruler = (Ruler)_SceneController.SelectedInstrument;
                                        if (gamePadState.ThumbSticks.Right.Y != 0f)
                                            _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.ObjLength, ruler.RulerLength + gamePadState.ThumbSticks.Right.Y);

                                    }
                                    else if (_SceneController.SelectedInstrument.Type == InstrumentType.Pole)
                                    {
                                        Pole pole = (Pole)_SceneController.SelectedInstrument;
                                        if (gamePadState.ThumbSticks.Right.Y != 0f)
                                            _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.ObjLength, pole.RulerLength + gamePadState.ThumbSticks.Right.Y);

                                    }
                                    else if (_SceneController.SelectedInstrument.Type == InstrumentType.Mirror)
                                    {
                                        Mirror mirror = (Mirror)_SceneController.SelectedInstrument;
                                        if (gamePadState.ThumbSticks.Right.Y != 0f)
                                            _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.ObjLength, mirror.RulerLength + gamePadState.ThumbSticks.Right.Y);

                                    }


                                    break;
                            }
                        }
                        break;
                case SystemState.S_State.STATE_MENU_SURVEYING:
                        // 先開放轉動

                        //_currentCamera.Turning( gamePadState.ThumbSticks.Left.X );
                        //_currentCamera.Zoom( 1.0f - ( gamePadState.ThumbSticks.Right.Y ) * 0.01f );
                        //_currentCamera.Turning(gamePadState.Triggers.Right * 0.01f );
                        //_currentCamera.Turning(-gamePadState.Triggers.Left * 0.01f);

                        // _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.TelescopeRotation, gamePadState.ThumbSticks.Right.Y);
                        if (State.InsType != InstrumentType.Leveling)
                        {

                            float VA = TelescopeRotationValue;
                            int PrevSide = 0;
                            VA = (VA + 90) % 360;
                            if (VA < 0) VA += 360;
                            float PrevVA = VA;

                            float _amount = 0.05f;
                            // if (((VA > 89f) && (VA < 91f)) || ((VA > 269f) && (VA < 271f)))
                            //    _amount = 0.0001f;

                            if (_amount != 0f)
                            {
                                if (VA <= 180)
                                {
                                    //if (Input.CurrentGamePadStates[0].IsButtonDown(Buttons.DPadUp))
                                    //    _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.TelescopeRotation, time * -0.01f);
                                    //else if (Input.CurrentGamePadStates[0].IsButtonDown(Buttons.DPadDown))
                                    //    _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.TelescopeRotation, time * +0.01f);
                                    //if (gamePadState.ThumbSticks.Right.Y > 0f)
                                    //    _amount = (float)((Math.Exp(gamePadState.ThumbSticks.Right.Y)-1f) / Math.Exp(1.0));
                                    //else if (gamePadState.ThumbSticks.Right.Y < 0f)
                                    //    _amount = -(float)((Math.Exp(-gamePadState.ThumbSticks.Right.Y)-1f) / Math.Exp(1.0));
                                    //else
                                    //    _amount = 0f;
                                    if (Math.Abs(_amount) < 0.5f)
                                        _amount = _amount * 0.001f;
                                    _amount = gamePadState.ThumbSticks.Right.Y * gamePadState.ThumbSticks.Right.Y * gamePadState.ThumbSticks.Right.Y * gamePadState.ThumbSticks.Right.Y * gamePadState.ThumbSticks.Right.Y;



                                    //if (((gamePadState.ThumbSticks.Right.Y > 0f) && (gamePadState.ThumbSticks.Right.Y < 0.5f)) ||
                                    //   ((gamePadState.ThumbSticks.Right.Y < 0f) && (gamePadState.ThumbSticks.Right.Y > -0.5f)))
                                    //    _amount = 0.0001f;
                                    //else
                                    //    _amount = 0.05f;
                                    _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.TelescopeRotation, _amount);

                                }
                                else
                                {
                                    //if (Input.CurrentGamePadStates[0].IsButtonDown(Buttons.DPadUp))
                                    //    _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.TelescopeRotation, time * 0.01f);
                                    //else if (Input.CurrentGamePadStates[0].IsButtonDown(Buttons.DPadDown))
                                    //    _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.TelescopeRotation, time * -0.01f);
                                    //if (((gamePadState.ThumbSticks.Right.Y > 0f) && (gamePadState.ThumbSticks.Right.Y < 0.5f)) ||
                                    //    ((gamePadState.ThumbSticks.Right.Y < 0f) && (gamePadState.ThumbSticks.Right.Y > -0.5f)))
                                    //    _amount = 0.0001f;
                                    //else
                                    //    _amount = 0.05f;
                                    //_SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.TelescopeRotation, (gamePadState.ThumbSticks.Right.Y) * _amount);

                                    if (Math.Abs(_amount) < 0.5f)
                                        _amount = _amount * 0.001f;
                                    _amount = gamePadState.ThumbSticks.Right.Y * gamePadState.ThumbSticks.Right.Y * gamePadState.ThumbSticks.Right.Y * gamePadState.ThumbSticks.Right.Y * gamePadState.ThumbSticks.Right.Y;


                                    _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.TelescopeRotation, -_amount);

                                }
                            }
                            VA = TelescopeRotationValue;
                            VA = (VA + 90) % 360;
                            if (VA < 0) VA += 360;
                            if ((PrevVA < 5f) && (VA > 355f))
                            {
                                VA = 355f;
                                TelescopeRotationValue = VA - 90;
                            }
                            else if ((VA < 5f) && (PrevVA > 355f))
                            {
                                VA = 5f;
                                TelescopeRotationValue = VA - 90;
                            }
                            else if ((PrevVA <= 180f) && (VA > 180f))
                            {
                                VA = 185f;
                                TelescopeRotationValue = VA - 90;
                            }
                            else if ((VA <= 180f) && (PrevVA > 180f))
                            {
                                VA = 175f;
                                TelescopeRotationValue = VA - 90;
                            }
                        }
                        if ( gamePadState.ThumbSticks.Left.X != 0f )
                        _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.UpperBodyRotation, -gamePadState.ThumbSticks.Left.X *0.3f );
                        if ( gamePadState.Triggers.Left != 0f )
                        _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.UpperBodyRotation, gamePadState.Triggers.Left * 0.002 );
                        if ( gamePadState.Triggers.Right != 0f )
                        _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.UpperBodyRotation, -gamePadState.Triggers.Right * 0.002);
                        // _currentCamera.Zoom(1.0f - (gamePadState.ThumbSticks.Right.Y) * 0.01f);


                            if (Input.CurrentGamePadStates[0].IsButtonDown(Buttons.DPadUp))
                                _currentCamera.Zoom(1.0f - time * 0.0005f);
                            else if (Input.CurrentGamePadStates[0].IsButtonDown(Buttons.DPadDown))
                                _currentCamera.Zoom(1.0f + time * 0.0005f);

                        // 前面 key 有 assign 
                        //_ray.Position = _SceneController._TotalstationCamera.Position;
                        //_ray.Direction = _SceneController._TotalstationCamera.Direction; 


                        break;


                        // _SceneController.UserPan(SceneController.Direction.West, gamePadState.ThumbSticks.Left.X);
                    break;
                }

/* 暫時都關掉
                // Navigation 狀態
                if (gamePadState.IsButtonDown(Buttons.B))
                    OnButtonBDown();
                if (gamePadState.IsButtonUp(Buttons.B))
                    OnButtonBUp();
                // Instrument 狀態
                if (gamePadState.IsButtonDown(Buttons.X))
                    OnButtonXDown();
                if (gamePadState.IsButtonUp(Buttons.X))
                    OnButtonXUp();
                // 子視窗
                if (_Mode == OperatingMode.Navigation)
                {
                    if (gamePadState.IsButtonDown(Buttons.Y))
                        OnButtonYDown();
                    if (gamePadState.IsButtonUp(Buttons.Y))
                        OnButtonYUp();
                }

*/
                /* if (gamePadState.Buttons.Start == ButtonState.Pressed)
                       System.Environment.Exit(Environment.ExitCode);  */
            }
        }
#endregion



        public void AutoFace2Ins()
        {

            float dx;
            float dz;
            float testv;
            float _Angle;

            for (int i = 0; i < State.Ruler_Ns; i++)
            {
                // if (State.RulerType == 0)
                    SetInstrumentWithName(@"Ruler_" + (i+1).ToString());
                // else
                //     SetInstrumentWithName(@"Pole_" + (i+1).ToString());

                dx = State.Ins_Position.X - State.getRuler_Position(i).X;
                dz = State.Ins_Position.Z - State.getRuler_Position(i).Z;

                testv = (float)Math.Atan(dx / dz);
                _Angle = MathHelper.ToDegrees((float)Math.Atan(dx / dz));
                if (dz < 0)
                    _Angle += 180;
                _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.SetRotationY, _Angle);
            }

            // Pole 似乎不需要轉正, 以下Code 可以省略
            for (int i = 0; i < State.Pole_Ns; i++)
            {
                // if (State.RulerType == 0)
                SetInstrumentWithName(@"Pole_" + (i + 1).ToString());
                // else
                //     SetInstrumentWithName(@"Pole_" + (i+1).ToString());

                dx = State.Ins_Position.X - State.getPole_Position(i).X;
                dz = State.Ins_Position.Z - State.getPole_Position(i).Z;

                testv = (float)Math.Atan(dx / dz);
                _Angle = MathHelper.ToDegrees((float)Math.Atan(dx / dz));
                if (dz < 0)
                    _Angle += 180;
                _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.SetRotationY, _Angle);
            }



            if (State.Mirror_Ns > 0)
            {

                for (int i = 0; i < State.Mirror_Ns; i++)
                {

                    SetInstrumentWithName(@"Mirror_" + (i + 1).ToString());


                    dx = State.Ins_Position.X - State.getMirror_Position(i).X;
                    dz = State.Ins_Position.Z - State.getMirror_Position(i).Z;

                    testv = (float)Math.Atan(dx / dz);
                    _Angle = MathHelper.ToDegrees((float)Math.Atan(dx / dz));
                    if (dz < 0)
                        _Angle += 180;
                    _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.SetRotationY, _Angle);
                }

            }
            if (State.RulerPad_Ns > 0)
            {

                for (int i = 0; i < State.RulerPad_Ns; i++)
                {

                    SetInstrumentWithName(@"RulerPad_" + (i + 1).ToString());


                    dx = State.Ins_Position.X - State.getRulerPad_Position(i).X;
                    dz = State.Ins_Position.Z - State.getRulerPad_Position(i).Z;

                    testv = (float)Math.Atan(dx / dz);
                    _Angle = MathHelper.ToDegrees((float)Math.Atan(dx / dz));
                    if (dz < 0)
                        _Angle += 180;
                    _SceneController.SetInstrument(_currentCamera, SceneController.SetInstrumentOption.SetRotationY, _Angle);
                }

            }

        }

        public void AutoAdjust2SetPoint(int _n)
        {
            Vector3 _p ; 
            float _d0 ; 
            int _idx = -1;
            if (_n == 0)
                _p = State.Ins_Position ; 
            else if ( _n <= 20 ) // Ruler/ Pole 
                _p = State.getRuler_Position(_n - 1);
            else if (_n <= 100) // Ruler/ Pole 
                _p = State.getPole_Position(_n - 20 - 1);
            else if (_n <= 120) // Mirror
                _p = State.getMirror_Position(_n - 100 - 1);
            else // RulerPad
            {
                return; 
            }

            float dx = InstrumentPos.X ;
            float dz = InstrumentPos.Z ;
            float _min = 100000;
            Vector3 _tar_p;
            if (State.SetPoint0_Enabled)
            {
                _tar_p = State.getSetPoint_Position(0);
                _d0 = Vector3.Distance(new Vector3(_p.X, 0f, _p.Z), new Vector3(_tar_p.X, 0f, _tar_p.Z)); 
                if ( _d0 < _min )
                {
                    _min = _d0 ; 
                    _idx = 0 ; 
                }

            }
            for (int i = 1; i <= State.SetPoint_Ns; i++)
            {
                _tar_p = State.getSetPoint_Position(i);
                _d0 = Vector3.Distance( new Vector3(_p.X, 0f, _p.Z), new Vector3(_tar_p.X, 0f, _tar_p.Z)) ; 
                if ( _d0 < _min )
                {
                    _min = _d0 ; 
                    _idx = i ; 
                }
            }

            for (int i = 0; i < State.RulerPad_Ns; i++)
            {
                _tar_p = State.getRulerPad_Position( i );
                _d0 = Vector3.Distance(new Vector3(_p.X, 0f, _p.Z), new Vector3(_tar_p.X, 0f, _tar_p.Z));
                if (_d0 < _min)
                {
                    _min = _d0;
                    _idx = 120 + i;
                }
            }


            if (( _idx != -1 ) && ( _min < 10 ))
            {
            if (_n == 0) // Instrument
            {
                
                SetInstrumentWithName(@"TotalStation_1");
                if ( _idx < 120 ) 
                    SetInstrument(State.getSetPoint_Position( _idx ));
                else
                    SetInstrument(State.getRulerPad_Position(_idx - 120));
            }

            else if (_n <= 20) // Ruler
            {


                SetInstrumentWithName(@"Ruler_" + _n.ToString());

                Vector3 _p1;
                if (_idx < 120)
                    _p1 = State.getSetPoint_Position(_idx);
                else
                    _p1 = State.getRulerPad_Position(_idx - 120);

                _p1.Y = 0f;
                SetInstrument(_p1);

            }
            else if (_n <= 100) // Pole
            {

                SetInstrumentWithName(@"Pole_" + ( _n - 20 ).ToString());

                Vector3 _p1;
                if (_idx < 120)
                    _p1 = State.getSetPoint_Position(_idx);
                else
                    _p1 = State.getRulerPad_Position(_idx - 120);

                _p1.Y = 0f;
                SetInstrument(_p1);

            }
            else if (_n <= 120) // 101.. 120, Mirror
            {
                SetInstrumentWithName(@"Mirror_" + (_n - 100).ToString());

                Vector3 _p1;
                if (_idx < 120)
                    _p1 = State.getSetPoint_Position(_idx);
                else
                    _p1 = State.getRulerPad_Position(_idx - 120);
                _p1.Y = 0f;
                SetInstrument(_p1);
            }
            else if (_n <= 200) // 121.. 200
            {
                // needn't
            }
            }


        }




        private void Bloom_Draw(GameTime gameTime)
        {
            // Resolve the scene into a texture, so we can
            // use it as input data for the bloom processing.
            ScreenManager.GraphicsDevice.ResolveBackBuffer(resolveTarget);

            // Pass 1: draw the scene into rendertarget 1, using a
            // shader that extracts only the brightest parts of the image.
            bloomExtractEffect.Parameters["BloomThreshold"].SetValue(
                Settings.BloomThreshold);

            DrawFullscreenQuad(resolveTarget, renderTarget1,
                               bloomExtractEffect,
                               IntermediateBuffer.PreBloom);

            // Pass 2: draw from rendertarget 1 into rendertarget 2,
            // using a shader to apply a horizontal gaussian blur filter.
            SetBlurEffectParameters(1.0f / (float)renderTarget1.Width, 0);

            DrawFullscreenQuad(renderTarget1.GetTexture(), renderTarget2,
                               gaussianBlurEffect,
                               IntermediateBuffer.BlurredHorizontally);

            // Pass 3: draw from rendertarget 2 back into rendertarget 1,
            // using a shader to apply a vertical gaussian blur filter.
            SetBlurEffectParameters(0, 1.0f / (float)renderTarget1.Height);

            DrawFullscreenQuad(renderTarget2.GetTexture(), renderTarget1,
                               gaussianBlurEffect,
                               IntermediateBuffer.BlurredBothWays);

            // Pass 4: draw both rendertarget 1 and the original scene
            // image back into the main backbuffer, using a shader that
            // combines them to produce the final bloomed result.
            ScreenManager.GraphicsDevice.SetRenderTarget(0, null);

            EffectParameterCollection parameters = bloomCombineEffect.Parameters;

            parameters["BloomIntensity"].SetValue(Settings.BloomIntensity);
            parameters["BaseIntensity"].SetValue(Settings.BaseIntensity);
            parameters["BloomSaturation"].SetValue(Settings.BloomSaturation);
            parameters["BaseSaturation"].SetValue(Settings.BaseSaturation);

            ScreenManager.GraphicsDevice.Textures[1] = resolveTarget;

            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;

            DrawFullscreenQuad(renderTarget1.GetTexture(),
                               viewport.Width, viewport.Height,
                               bloomCombineEffect,
                               IntermediateBuffer.FinalResult);
        }


        /// <summary>
        /// Helper for drawing a texture into a rendertarget, using
        /// a custom shader to apply postprocessing effects.
        /// </summary>
        void DrawFullscreenQuad(Texture2D texture, RenderTarget2D renderTarget,
                                Effect effect, IntermediateBuffer currentBuffer)
        {
            ScreenManager.GraphicsDevice.SetRenderTarget(0, renderTarget);

            DrawFullscreenQuad(texture,
                               renderTarget.Width, renderTarget.Height,
                               effect, currentBuffer);

            ScreenManager.GraphicsDevice.SetRenderTarget(0, null);
        }


        /// <summary>
        /// Helper for drawing a texture into the current rendertarget,
        /// using a custom shader to apply postprocessing effects.
        /// </summary>
        void DrawFullscreenQuad(Texture2D texture, int width, int height,
                                Effect effect, IntermediateBuffer currentBuffer)
        {
            spriteBatch.Begin(SpriteBlendMode.None,
                              SpriteSortMode.Immediate,
                              SaveStateMode.None);

            // Begin the custom effect, if it is currently enabled. If the user
            // has selected one of the show intermediate buffer options, we still
            // draw the quad to make sure the image will end up on the screen,
            // but might need to skip applying the custom pixel shader.
            if (showBuffer >= currentBuffer)
            {
                effect.Begin();
                effect.CurrentTechnique.Passes[0].Begin();
            }

            // Draw the quad.
            spriteBatch.Draw(texture, new Rectangle(0, 0, width, height), Color.White);
            spriteBatch.End();

            // End the custom effect.
            if (showBuffer >= currentBuffer)
            {
                effect.CurrentTechnique.Passes[0].End();
                effect.End();
            }
        }


        /// <summary>
        /// Computes sample weightings and texture coordinate offsets
        /// for one pass of a separable gaussian blur filter.
        /// </summary>
        void SetBlurEffectParameters(float dx, float dy)
        {
            // Look up the sample weight and offset effect parameters.
            EffectParameter weightsParameter, offsetsParameter;

            weightsParameter = gaussianBlurEffect.Parameters["SampleWeights"];
            offsetsParameter = gaussianBlurEffect.Parameters["SampleOffsets"];

            // Look up how many samples our gaussian blur effect supports.
            int sampleCount = weightsParameter.Elements.Count;

            // Create temporary arrays for computing our filter settings.
            float[] sampleWeights = new float[sampleCount];
            Vector2[] sampleOffsets = new Vector2[sampleCount];

            // The first sample always has a zero offset.
            sampleWeights[0] = ComputeGaussian(0);
            sampleOffsets[0] = new Vector2(0);

            // Maintain a sum of all the weighting values.
            float totalWeights = sampleWeights[0];

            // Add pairs of additional sample taps, positioned
            // along a line in both directions from the center.
            for (int i = 0; i < sampleCount / 2; i++)
            {
                // Store weights for the positive and negative taps.
                float weight = ComputeGaussian(i + 1);

                sampleWeights[i * 2 + 1] = weight;
                sampleWeights[i * 2 + 2] = weight;

                totalWeights += weight * 2;

                // To get the maximum amount of blurring from a limited number of
                // pixel shader samples, we take advantage of the bilinear filtering
                // hardware inside the texture fetch unit. If we position our texture
                // coordinates exactly halfway between two texels, the filtering unit
                // will average them for us, giving two samples for the price of one.
                // This allows us to step in units of two texels per sample, rather
                // than just one at a time. The 1.5 offset kicks things off by
                // positioning us nicely in between two texels.
                float sampleOffset = i * 2 + 1.5f;

                Vector2 delta = new Vector2(dx, dy) * sampleOffset;

                // Store texture coordinate offsets for the positive and negative taps.
                sampleOffsets[i * 2 + 1] = delta;
                sampleOffsets[i * 2 + 2] = -delta;
            }

            // Normalize the list of sample weightings, so they will always sum to one.
            for (int i = 0; i < sampleWeights.Length; i++)
            {
                sampleWeights[i] /= totalWeights;
            }

            // Tell the effect about our new filter settings.
            weightsParameter.SetValue(sampleWeights);
            offsetsParameter.SetValue(sampleOffsets);
        }


        /// <summary>
        /// Evaluates a single point on the gaussian falloff curve.
        /// Used for setting up the blur filter weightings.
        /// </summary>
        float ComputeGaussian(float n)
        {
            float theta = Settings.BlurAmount;

            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) *
                           Math.Exp(-(n * n) / (2 * theta * theta)));
        }





// LensFlare


        public void LensFlare_Draw(GameTime gameTime)
        {
            // The sun is infinitely distant, so it should not be affected by the
            // position of the camera. Floating point math doesn't support infinitely
            // distant vectors, but we can get the same result by making a copy of our
            // view matrix, then resetting the view translation to zero. Pretending the
            // camera has not moved position gives the same result as if the camera
            // was moving, but the light was infinitely far away. If our flares came
            // from a local object rather than the sun, we would use the original view
            // matrix here.

            // skc, 這裡要使用 _camera 
            Matrix infiniteView = _currentCamera.View; //  View;
            Matrix Projection = _currentCamera.Projection; 


            infiniteView.Translation = Vector3.Zero;

            // Project the light position into 2D screen space.
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;

            Vector3 projectedPosition = viewport.Project(-LightDirection, Projection,
                                                         infiniteView, Matrix.Identity);

            // Don't draw any flares if the light is behind the camera.
            if ((projectedPosition.Z < 0) || (projectedPosition.Z > 1))
                return;
            

            Vector2 lightPosition = new Vector2(projectedPosition.X,
                                                projectedPosition.Y);

            // Check whether the light is hidden behind the scenery.
            UpdateOcclusion(lightPosition);

            // If it is visible, draw the flare effect.
            if (occlusionAlpha > 0)
            {
                DrawGlow(lightPosition);
                DrawFlares(lightPosition);
            }

            RestoreRenderStates();
        }


        /// <summary>
        /// Mesures how much of the sun is visible, by drawing a small rectangle,
        /// centered on the sun, but with the depth set to as far away as possible,
        /// and using an occlusion query to measure how many of these very-far-away
        /// pixels are not hidden behind the terrain.
        /// 
        /// The problem with occlusion queries is that the graphics card runs in
        /// parallel with the CPU. When you issue drawing commands, they are just
        /// stored in a buffer, and the graphics card can be as much as a frame delayed
        /// in getting around to processing the commands from that buffer. This means
        /// that even after we issue our occlusion query, the occlusion results will
        /// not be available until later, after the graphics card finishes processing
        /// these commands.
        /// 
        /// It would slow our game down too much if we waited for the graphics card,
        /// so instead we delay our occlusion processing by one frame. Each time
        /// around the game loop, we read back the occlusion results from the previous
        /// frame, then issue a new occlusion query ready for the next frame to read
        /// its result. This keeps the data flowing smoothly between the CPU and GPU,
        /// but also causes our data to be a frame out of date: we are deciding
        /// whether or not to draw our lensflare effect based on whether it was
        /// visible in the previous frame, as opposed to the current one! Fortunately,
        /// the camera tends to move slowly, and the lensflare fades in and out
        /// smoothly as it goes behind the scenery, so this out-by-one-frame error
        /// is not too noticeable in practice.
        /// </summary>
        void UpdateOcclusion(Vector2 lightPosition)
        {
            // Give up if the current graphics card does not support occlusion queries.
            if (!occlusionQuery.IsSupported)
                return;

            if (occlusionQueryActive)
            {
                // If the previous query has not yet completed, wait until it does.
                if (!occlusionQuery.IsComplete)
                    return;

                // Use the occlusion query pixel count to work
                // out what percentage of the sun is visible.
                const float queryArea = querySize * querySize;

                occlusionAlpha = Math.Min(occlusionQuery.PixelCount / queryArea, 1);
            }

            // Set renderstates for drawing the occlusion query geometry. We want depth
            // tests enabled, but depth writes disabled, and we set ColorWriteChannels
            // to None to prevent this query polygon actually showing up on the screen.
            RenderState renderState = ScreenManager.GraphicsDevice.RenderState;

            renderState.DepthBufferEnable = true;
            renderState.DepthBufferWriteEnable = false;
            renderState.AlphaTestEnable = false;
            renderState.ColorWriteChannels = ColorWriteChannels.None;

            // Set up our BasicEffect to center on the current 2D light position.
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;

            basicEffect.World = Matrix.CreateTranslation(lightPosition.X,
                                                         lightPosition.Y, 0);

            basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0,
                                                                        viewport.Width,
                                                                        viewport.Height,
                                                                        0, 0, 1);

            basicEffect.Begin();
            basicEffect.CurrentTechnique.Passes[0].Begin();

            ScreenManager.GraphicsDevice.VertexDeclaration = vertexDeclaration;

            // Issue the occlusion query.
            occlusionQuery.Begin();

            ScreenManager.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleFan,
                                              queryVertices, 0, 2);

            occlusionQuery.End();

            basicEffect.CurrentTechnique.Passes[0].End();
            basicEffect.End();

            // Reset renderstates.
            renderState.ColorWriteChannels = ColorWriteChannels.All;
            
            renderState.DepthBufferWriteEnable = true;

            occlusionQueryActive = true;
        }


        /// <summary>
        /// Draws a large circular glow sprite, centered on the sun.
        /// </summary>
        void DrawGlow(Vector2 lightPosition)
        {
            Vector4 color = new Vector4(1, 1, 1, occlusionAlpha);
            Vector2 origin = new Vector2(glowSprite.Width, glowSprite.Height) / 2;
            float scale = glowSize * 2 / glowSprite.Width;

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);

            spriteBatch.Draw(glowSprite, lightPosition, null, new Color(color), 0,
                             origin, scale, SpriteEffects.None, 0);

            spriteBatch.End();
        }


        /// <summary>
        /// Draws the lensflare sprites, computing the position
        /// of each one based on the current angle of the sun.
        /// </summary>
        void DrawFlares(Vector2 lightPosition)
        {
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;

            // Lensflare sprites are positioned at intervals along a line that
            // runs from the 2D light position toward the center of the screen.
            Vector2 screenCenter = new Vector2(viewport.Width, viewport.Height) / 2;

            Vector2 flareVector = screenCenter - lightPosition;

            // Draw the flare sprites using additive blending.
            spriteBatch.Begin(SpriteBlendMode.Additive);

            foreach (Flare flare in flares)
            {
                // Compute the position of this flare sprite.
                Vector2 flarePosition = lightPosition + flareVector * flare.Position;

                // Set the flare alpha based on the previous occlusion query result.
                Vector4 flareColor = flare.Color.ToVector4();

                flareColor.W *= occlusionAlpha;

                // Center the sprite texture.
                Vector2 flareOrigin = new Vector2(flare.Texture.Width,
                                                  flare.Texture.Height) / 2;

                // Draw the flare.
                spriteBatch.Draw(flare.Texture, flarePosition, null,
                                 new Color(flareColor), 1, flareOrigin,
                                 flare.Scale, SpriteEffects.None, 0);
            }

            spriteBatch.End();
        }


        /// <summary>
        /// Sets renderstates back to their default values after we finish drawing
        /// the lensflare, to avoid messing up the 3D terrain rendering.
        /// </summary>
        void RestoreRenderStates()
        {
            RenderState renderState = ScreenManager.GraphicsDevice.RenderState;

            renderState.DepthBufferEnable = true;
            renderState.AlphaTestEnable = false;
            renderState.AlphaBlendEnable = false;

            SamplerState samplerState = ScreenManager.GraphicsDevice.SamplerStates[0];

            samplerState.AddressU = TextureAddressMode.Wrap;
            samplerState.AddressV = TextureAddressMode.Wrap;
        }














        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,Color.Gray, 0, 0);
            switch (State.SkyIndex)
            {
                case 0:
                    ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.CornflowerBlue, 0, 0);
                    break;
                case 1:
                    ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.SlateBlue, 0, 0);
                    break;
                case 2:
                    ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.LightBlue, 0, 0);
                    break;
                case 3:
                    ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.DarkOrange, 0, 0);
                    break;
                case 4:
                    ScreenManager.GraphicsDevice.Clear(ClearOptions.Target, Color.LightGray, 0, 0);
                    break;
            }


            // 畫主要 3D 物件
            RenderMainView( State, gameTime);

            // 顯示狀態 ( Navigation/ Instrument ) 
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            //spriteBatch.Begin();
            //spriteBatch.DrawString(gameFont, _Mode.ToString(), new Vector2(0, 0), Color.Wheat);
            //spriteBatch.End();

            // 顯示儀器選擇狀態
            switch(_Mode)
            {
                case OperatingMode.Instrument:
                    //_SceneController.Draw2D(spriteBatch, gameFont, new Vector2(_SubInfoDisplayArea2.Left, _SubInfoDisplayArea2.Top));
                    //_SceneController.Draw2DInfo(spriteBatch, gameFont, new Vector2(_SubInfoDisplayArea.Left, _SubInfoDisplayArea.Top), _TripodMode);
                break;
                case OperatingMode.Navigation:
                    
                    break;
            }



            // 測量儀器小視窗
            switch (_Mode)
            {
                case OperatingMode.Instrument:
                    break;
                case OperatingMode.Navigation:
                    if (_IsShowSurveyingView)
                    {
                            spriteBatch.Begin();
                            spriteBatch.Draw(_SideTexture,
                                new Rectangle(_SurveyingViewport.X,_SurveyingViewport.Y,_SurveyingViewport.Width,_SurveyingViewport.Height),Color.White);
                            spriteBatch.End();
                            RenderSurveyingView(gameTime, spriteBatch);
                     }
                    break;
            }
            
            // If the game is transitioning on or off, fade it out to black.
            // skc, remark, check later 
            // if  ( TransitionPosition > 0 )
            //    ScreenManager.FadeBackBufferToBlack( 255 - TransitionAlpha );


            // 因為有天空情況下, 地上倒影很嚴重, 先關閉 Lens Flare --> 沒改善, check SceneController.cs 
            // if (State.Background_N == 1)

            LensFlare_Draw(gameTime);
            Bloom_Draw(gameTime);

        }

        // Render the 3D Navigation View
        private void RenderMainView( SystemState State, GameTime gameTime)
        {
            //store the current viewport width
            Rectangle oriViewportSize = new Rectangle(ScreenManager.GraphicsDevice.Viewport.X, ScreenManager.GraphicsDevice.Viewport.Y
                , ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height);

            //get a copy of the current graphics device viewport
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;

            // 原來設計, 周邊範圍不用
            //viewport.Width = _InfoDisplayArea.Width;
            //viewport.Height = _InfoDisplayArea.Height;
            //viewport.X = _InfoDisplayArea.X;
            //viewport.Y = _InfoDisplayArea.Y;

            //update the graphics device viewport
            // ScreenManager.GraphicsDevice.Viewport = viewport;

            RecoverGraphicDeviceState();
            _SceneController.Draw3D( _currentCamera, gameTime, RenderingView.NavigationView);

            //restore the viewport
            // skc, seems useless
            viewport.Width = oriViewportSize.Width;
            viewport.Height = oriViewportSize.Height;
            viewport.X = oriViewportSize.X;
            viewport.Y = oriViewportSize.Y;
            ScreenManager.GraphicsDevice.Viewport = viewport;
        }
        //Render the Surveying View
        private void RenderSurveyingView(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //store the current viewport width
            Rectangle oriViewportSize = new Rectangle(ScreenManager.GraphicsDevice.Viewport.X, ScreenManager.GraphicsDevice.Viewport.Y
                , ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height);

            //get a copy of the current graphics device viewport
            Viewport viewport = _SurveyingViewport;

            //set the viewport width to DisplayArea          
            viewport.Width -=  2*_SceneController.SimuSurveyArguments.SURVEYING_VIEW_MARGIN;
            viewport.Height -= 2*_SceneController.SimuSurveyArguments.SURVEYING_VIEW_MARGIN;
            viewport.X += _SceneController.SimuSurveyArguments.SURVEYING_VIEW_MARGIN;
            viewport.Y += _SceneController.SimuSurveyArguments.SURVEYING_VIEW_MARGIN;

            //update the graphics device viewport
            ScreenManager.GraphicsDevice.Viewport = viewport;

            RecoverGraphicDeviceState();
            ScreenManager.GraphicsDevice.Clear(Color.Gray);
            _SceneController.Draw3D( _currentCamera, gameTime, RenderingView.SurveyingView);
            _SceneController.Draw2DSurveyingWindow(spriteBatch);

            //restore the viewport
            viewport = _MainViewport;

            //update the graphics device viewport
            ScreenManager.GraphicsDevice.Viewport = viewport;
        }
        #endregion
    }
}
