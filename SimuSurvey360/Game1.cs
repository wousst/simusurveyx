using System;
using System.Threading;
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
using SimuSurvey360.Screens;
using SpriteSheetRuntime;
using SimuSurvey360.Instruments;

namespace SimuSurvey360
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    ///

    public class Game1 : Microsoft.Xna.Framework.Game
    {

        const string ReleaseStr = "1.0.6";
        bool no_ghmenu2 = true;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteSheet naSheet;
        SpriteSheet spriteSheet;
        SpriteFont spriteFont;
        SpriteFont spriteFontSmall;

        // SceneController _SceneController;
        ScreenManager _ScreenManager;
        OnscreenKeyboard _oskb;
        OnscreenDigitPad _osdp;
        OnscreenInput _osinput;
        // ghMenu _ghMenu ;
        ghMenu2 _ghMenu2;
        g3DMenu _g3DMenu;
        recordMenu _recordMenu;
        settingError _settingError;
        InputState Input;
        SystemState State;
        Texture2D TitleScreen;
        Texture2D CrossLine, CrossLineRev, CrossLine_s;
        Texture2D SignUp, SignDown;
        Texture2D Dash_L, Dash_R;
        Texture2D AnglePointer, AnglePointerN, PointerRed;
        Texture2D H0001;
        // Texture2D Dir_1, Dir_2, Dir_3, Dir_4 ; 
        Vector2 Item_StartPos = new Vector2(900, 20);
        Vector2 Item_Distance = new Vector2(90, 0);
        MainScreen _MainScreen;
        MainMenuScreen _MainMenuScreen; 
        //Texture2D[] _tTerrain = new Texture2D[5];
        //Texture2D[] _tTexture = new Texture2D[5];
        // Btn 大小變化
        int NaviBtn_State ;


        Matrix worldMatrix;
        Matrix viewMatrix;
        Matrix projectionMatrix;

        BasicEffect basicEffect, basicEffect2;
        VertexDeclaration vertexDeclaration;
        VertexPositionColor[] pointList;
        VertexBuffer vertexBuffer;
        int points = 16;

        short[] lineListIndices;
        Vector3 _p0, _p1, _p2, _p3, _p4;


        SoundEffect[] SndBird;
        SoundEffect SndPick, SndDrop;  

        int FrameCnt;

        Vector3 T3;
        Vector4 T4;

        private float fKeyPressCheckDelay = 0.25f;
        private float fTotalElapsedTime = 0;


        int LastObjState = 0;
        float DistError = 0f;
        // int _Ready2PeakObj; 

        Radar radar;                            // This is the radar component
        List<Entity> Enemies;                   // A list to hold all enemies
        const int NumEnemies = 20;
        int EnemyIndex = 0 ;

        // BloomComponent bloom;



// particle
        bool Particle_Off; 

        ParticleSystem explosionParticles;
        ParticleSystem explosionSmokeParticles;
        ParticleSystem projectileTrailParticles;
        ParticleSystem smokePlumeParticles;
        ParticleSystem fireParticles;
        enum ParticleState
        {
            Explosions,
            SmokePlume,
            RingOfFire,
        };
        ParticleState currentState = ParticleState.SmokePlume;
        // ParticleState currentState = ParticleState.Explosions;

        // The explosions effect works by firing projectiles up into the
        // air, so we need to keep track of all the active projectiles.
        List<Projectile> projectiles = new List<Projectile>();

        TimeSpan timeToNextProjectile = TimeSpan.Zero;
        Random random = new Random();



        Vector3 SmokePos;
        bool ParticleEnabled = false;



        public Game1()
        {
#if XBOX
            Particle_Off = false;
#else
            Particle_Off = false;
#endif 
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.IsMouseVisible = true;
            this.Window.AllowUserResizing = true;
            this.Window.Title = "SimuSurveyX";

            // Particle
            graphics.MinimumPixelShaderProfile = ShaderProfile.PS_2_0;

            // Construct our particle system components.

            // bloom = new BloomComponent(this);
            // bloom.DrawOrder = 60;





            // bloomSettingsIndex = (bloomSettingsIndex + 1) %
            //                      BloomSettings.PresetSettings.Length;

            
            // 0 : 還不錯, 中等
            // 1 : 比較白, 像是霧
            // 2 : 彩度降低, 光暈效果不錯
            // 3 : 顏色太誇張 
            // 4 : 純粹模糊
            // 5 : 比較不誇張
            // bloom.Settings = BloomSettings.PresetSettings[2];
            // bloom.Visible = true;
            // bloom.Visible = false;
            // 原本直接使用 GamsComponent 的方式 很快就有效果, 
            // 不過因為選單跟 MainScreen 不可分而造成模糊
            // 所以後來直接把 BloomComponent 稍作修改放入 MainScreen.cs 各部分裡面


// skc_particle 



            if (!Particle_Off)
            {
                explosionParticles = new ParticleSystem(this, Content, @"Particle3D\ExplosionSettings");
                explosionSmokeParticles = new ParticleSystem(this, Content, @"Particle3D\ExplosionSmokeSettings");
                projectileTrailParticles = new ParticleSystem(this, Content, @"Particle3D\ProjectileTrailSettings");
                smokePlumeParticles = new ParticleSystem(this, Content, @"Particle3D\SmokePlumeSettings");
                fireParticles = new ParticleSystem(this, Content, @"Particle3D\FireSettings");

                // Set the draw order so the explosions and fire
                // will appear over the top of the smoke.
                smokePlumeParticles.DrawOrder = 50;
                explosionSmokeParticles.DrawOrder = 51;
                projectileTrailParticles.DrawOrder = 52;
                explosionParticles.DrawOrder = 53;
                fireParticles.DrawOrder = 54;

                // Register the particle system components.
                Components.Add(explosionParticles);
                Components.Add(explosionSmokeParticles);
                Components.Add(projectileTrailParticles);
                Components.Add(smokePlumeParticles);
                Components.Add(fireParticles);

            }
            // Components.Add(bloom);

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-g`raphic
        /// related content.  Calling base.Initialize will enumerate through any com    ``````````````````````````````````````ponents
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            // graphics.PreferredBackBufferWidth = 640;
            // graphics.PreferredBackBufferHeight = 480;
             graphics.PreferredBackBufferWidth = 1280;
             graphics.PreferredBackBufferHeight = 720;
             graphics.IsFullScreen = false ;
             graphics.ApplyChanges(); 


            // Create the screen manager component.
            Input = new InputState();
            State = new SystemState();


            Random _Rand = new Random();
            float _RandN = 0.0f;

            _RandN = -(1f/3f) + ( (float)_Rand.Next(200) / 300f ) ;
            State.ErrorX = (float)_RandN;
            _RandN = -(1f / 3f) + ((float)_Rand.Next(200) / 300f);
            State.ErrorY = (float)_RandN;
            _RandN = -(1f / 3f) + ((float)_Rand.Next(200) / 300f);
            State.ErrorZ = (float)_RandN; 



            State.OperationMode = 0;  // Peak Mode
            State.BaseNE = 256;


            _oskb = new OnscreenKeyboard( State, this );
            // Default: Visuble and Enable are true 
            _oskb.Visible = false;
            _oskb.Enabled = false;
            _oskb.DrawOrder = 200;
            _oskb.UpdateOrder = 200;

            _osdp = new OnscreenDigitPad(State, this);
            _osdp.Visible = false;
            _osdp.Enabled = false;
            _osdp.DrawOrder = 110;
            _osdp.UpdateOrder = 110;

            _osinput = new OnscreenInput(State, this);
            _osinput.Visible = false;
            _osinput.Enabled = false;
            _osinput.DrawOrder = 109;
            _osinput.UpdateOrder = 109;


            //_ghMenu = new ghMenu(State, this);
            //_ghMenu.Visible = false;
            //_ghMenu.Enabled = false;
            //_ghMenu.DrawOrder = 100;
            //_ghMenu.UpdateOrder = 100;


            _g3DMenu = new g3DMenu(State, this);
            _g3DMenu.Visible = false;
            _g3DMenu.Enabled = false;
            _g3DMenu.DrawOrder = 100;
            _g3DMenu.UpdateOrder = 100;

            _ghMenu2 = new ghMenu2(State, this);
            _ghMenu2.Visible = false;
            _ghMenu2.Enabled = false;
            _ghMenu2.DrawOrder = 101;
            _ghMenu2.UpdateOrder = 101;

            _recordMenu = new recordMenu(State, this);
            _recordMenu.Visible = false;
            _recordMenu.Enabled = false;
            _recordMenu.DrawOrder = 102;
            _recordMenu.UpdateOrder = 102;

            _settingError = new settingError(State, this);
            _settingError.Visible = false;
            _settingError.Enabled = false;
            _settingError.DrawOrder = 103;
            _settingError.UpdateOrder = 103;


            _ScreenManager = new ScreenManager( State, Input, this);
            _ScreenManager.DrawOrder = 59;

 
             Components.Add(_oskb );
             Components.Add(_osdp);
             Components.Add(_osinput);

                 // Components.Add(_ghMenu);
             Components.Add(_g3DMenu);
             Components.Add(_recordMenu);
             Components.Add(_settingError);
             Components.Add(_ghMenu2);
             Components.Add(_ScreenManager);

            // Activate the first screens.

            _MainScreen = new MainScreen();




            _ScreenManager.AddScreen( _MainScreen, PlayerIndex.One);
            // _ScreenManager.AddScreen( new MainMenuScreen(), null);


            SndBird = new SoundEffect[3]; 

            short[] lineListIndices;

            // 原本就沒開放
            /*_SceneController = new SceneControler(graphics.GraphicsDevice.Viewport);
            _SceneController.AddInstrument(SimuSurvey360.Instruments.InstrumentType.Ruler);
            _SceneController.AddInstrument(SimuSurvey360.Instruments.InstrumentType.TotalStation);*/



            //terrain = new GameComponent_Terrain_GetHeight(this,
            //    Content.Load<Texture2D>("terrain01"),
            //    Content.Load<Texture2D>("floor_128"));
            //this.Components.Add(terrain);
            //camera = new InspectionCamera(this);
            //camera.cameraRotationX = 30; //相機 角度 (X軸)
            //camera.cameraDistance = 200; //相機 距離 (Z軸)
            //this.Components.Add(camera);

            InitializeTransform();
            InitializeEffect();
            InitializeLineList();


            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("Arial24");
            spriteFontSmall = Content.Load<SpriteFont>("LCD18");
            TitleScreen = Content.Load<Texture2D>("Title");
            AnglePointer = Content.Load<Texture2D>("Pointer");
            PointerRed = Content.Load<Texture2D>("PointerRed");
            AnglePointerN = Content.Load<Texture2D>("PointerN");
            H0001 = Content.Load<Texture2D>("H0001");
            //Dir_1 = Content.Load<Texture2D>("Dir_1");
            //Dir_2 = Content.Load<Texture2D>("Dir_2");
            //Dir_3 = Content.Load<Texture2D>("Dir_3");
            //Dir_4 = Content.Load<Texture2D>("Dir_4");
            CrossLine = Content.Load<Texture2D>("crossline");
            CrossLineRev = Content.Load<Texture2D>("crosslineRev");
            CrossLine_s = Content.Load<Texture2D>("crossline_s");
            SignUp = Content.Load<Texture2D>("up");
            SignDown = Content.Load<Texture2D>("down");
            Dash_L = Content.Load<Texture2D>("Dash_L");
            Dash_R = Content.Load<Texture2D>("Dash_R");
            naSheet = Content.Load<SpriteSheet>(@"Navigation\Navigation");
            spriteSheet = Content.Load<SpriteSheet>(@"SpriteSheet\SpriteSheet");

            SndBird[0] = Content.Load<SoundEffect>(@"birds001");
            SndBird[1] = Content.Load<SoundEffect>(@"birds003");
            SndBird[2] = Content.Load<SoundEffect>(@"birds001");
            SndPick = Content.Load<SoundEffect>(@"UI_Misc01");
            SndDrop = Content.Load<SoundEffect>(@"UI_Misc02");

            //_tTerrain[0] = Content.Load<Texture2D>(@"terrain01");
            //_tTerrain[1] = Content.Load<Texture2D>(@"terrain02");
            //_tTerrain[2] = Content.Load<Texture2D>(@"terrain03");
            //_tTerrain[3] = Content.Load<Texture2D>(@"terrain04");
            //_tTexture[0] = Content.Load<Texture2D>(@"floor01");
            //_tTexture[1] = Content.Load<Texture2D>(@"floor02");
            //_tTexture[2] = Content.Load<Texture2D>(@"floor03");
            //_tTexture[3] = Content.Load<Texture2D>(@"floor03");




            State.PickedObj = -1;
            State.Ready2PickObj = -1; 
            // TODO: use this.Content to load your game content here
            //_SceneController.LoadContent(this.Content);

            InitRadar();
            InitEntities();

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// 

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>

        private void InitializeTransform()
        {

            viewMatrix = Matrix.CreateLookAt(
                new Vector3(0.0f, 0.0f, 1.0f),
                Vector3.Zero,
                Vector3.Up
                );

            projectionMatrix = Matrix.CreateOrthographicOffCenter(
                0,
                (float)GraphicsDevice.Viewport.Width,
                (float)GraphicsDevice.Viewport.Height,
                0,
                1.0f, 1000.0f);
        }

        private void InitializeEffect()
        {

            vertexDeclaration = new VertexDeclaration(
                GraphicsDevice,
                VertexPositionColor.VertexElements
                );

            basicEffect = new BasicEffect(GraphicsDevice, null);
            basicEffect.VertexColorEnabled = true;

            basicEffect2 = new BasicEffect(GraphicsDevice, null);
            basicEffect2.VertexColorEnabled = true;

            // worldMatrix = Matrix.CreateTranslation(GraphicsDevice.Viewport.Width /2 ,
            //     GraphicsDevice.Viewport.Height /2 , 0);
            worldMatrix = Matrix.Identity;
            basicEffect.World = worldMatrix;
            basicEffect.View = viewMatrix;
            basicEffect.Projection = projectionMatrix;
        }


        /// <summary>
        /// Initializes the line list.
        /// </summary>
        private void InitializeLineList()
        {

            vertexDeclaration = new VertexDeclaration(
                GraphicsDevice,
                VertexPositionColor.VertexElements
                );

            pointList = new VertexPositionColor[points];

            //for (int x = 0; x < points / 2; x++)
            //{
            //    for (int y = 0; y < 2; y++)
            //    {
            //        pointList[(x * 2) + y] = new VertexPositionColor(
            //            new Vector3(x * 100, y * 100, 0), Color.White);
            //    }
            //}

            // CrossLine 
            //pointList[0] = new VertexPositionColor(new Vector3(0, (float)GraphicsDevice.Viewport.Height / 2f, 0), Color.White);
            //pointList[1] = new VertexPositionColor(new Vector3((float)GraphicsDevice.Viewport.Width , (float)GraphicsDevice.Viewport.Height / 2f, 0), Color.White);

            //pointList[2] = new VertexPositionColor(new Vector3((float)GraphicsDevice.Viewport.Width / 2f, 0, 0), Color.White);
            //pointList[3] = new VertexPositionColor(new Vector3((float)GraphicsDevice.Viewport.Width / 2f, (float)GraphicsDevice.Viewport.Height, 0), Color.White);

            //pointList[4] = new VertexPositionColor(new Vector3(0, (float)GraphicsDevice.Viewport.Height / 2f, 0), Color.White);
            //pointList[5] = new VertexPositionColor(new Vector3((float)GraphicsDevice.Viewport.Width, (float)GraphicsDevice.Viewport.Height / 2f, 0), Color.White);

            //pointList[6] = new VertexPositionColor(new Vector3((float)GraphicsDevice.Viewport.Width / 2f, 0, 0), Color.White);
            //pointList[7] = new VertexPositionColor(new Vector3((float)GraphicsDevice.Viewport.Width / 2f, (float)GraphicsDevice.Viewport.Height, 0), Color.White);



            int MidDist = 20;
            int MidLen = 50;
           
            pointList[0] = new VertexPositionColor(new Vector3((float)GraphicsDevice.Viewport.Width / 2f - MidDist - MidLen, (float)GraphicsDevice.Viewport.Height / 2f - MidDist - MidLen, 0), Color.White);
            pointList[1] = new VertexPositionColor(new Vector3((float)GraphicsDevice.Viewport.Width / 2f - MidDist, (float)GraphicsDevice.Viewport.Height / 2f - MidDist - MidLen, 0), Color.White);

            pointList[2] = new VertexPositionColor(new Vector3((float)GraphicsDevice.Viewport.Width / 2f - MidDist - MidLen, (float)GraphicsDevice.Viewport.Height / 2f - MidDist - MidLen, 0), Color.White);
            pointList[3] = new VertexPositionColor(new Vector3((float)GraphicsDevice.Viewport.Width / 2f - MidDist - MidLen, (float)GraphicsDevice.Viewport.Height / 2f - MidDist, 0), Color.White);

            pointList[4] = new VertexPositionColor(new Vector3((float)GraphicsDevice.Viewport.Width / 2f + MidDist + MidLen, (float)GraphicsDevice.Viewport.Height / 2f - MidDist - MidLen, 0), Color.White);
            pointList[5] = new VertexPositionColor(new Vector3((float)GraphicsDevice.Viewport.Width / 2f + MidDist, (float)GraphicsDevice.Viewport.Height / 2f - MidDist - MidLen, 0), Color.White);

            pointList[6] = new VertexPositionColor(new Vector3((float)GraphicsDevice.Viewport.Width / 2f + MidDist + MidLen, (float)GraphicsDevice.Viewport.Height / 2f - MidDist - MidLen, 0), Color.White);
            pointList[7] = new VertexPositionColor(new Vector3((float)GraphicsDevice.Viewport.Width / 2f + MidDist + MidLen, (float)GraphicsDevice.Viewport.Height / 2f - MidDist, 0), Color.White);

            pointList[8] = new VertexPositionColor(new Vector3((float)GraphicsDevice.Viewport.Width / 2f - MidDist - MidLen, (float)GraphicsDevice.Viewport.Height / 2f + MidDist + MidLen, 0), Color.White);
            pointList[9] = new VertexPositionColor(new Vector3((float)GraphicsDevice.Viewport.Width / 2f - MidDist, (float)GraphicsDevice.Viewport.Height / 2f + MidDist + MidLen, 0), Color.White);

            pointList[10] = new VertexPositionColor(new Vector3((float)GraphicsDevice.Viewport.Width / 2f - MidDist - MidLen, (float)GraphicsDevice.Viewport.Height / 2f + MidDist + MidLen, 0), Color.White);
            pointList[11] = new VertexPositionColor(new Vector3((float)GraphicsDevice.Viewport.Width / 2f - MidDist - MidLen, (float)GraphicsDevice.Viewport.Height / 2f + MidDist, 0), Color.White);

            pointList[12] = new VertexPositionColor(new Vector3((float)GraphicsDevice.Viewport.Width / 2f + MidDist + MidLen, (float)GraphicsDevice.Viewport.Height / 2f + MidDist + MidLen, 0), Color.White);
            pointList[13] = new VertexPositionColor(new Vector3((float)GraphicsDevice.Viewport.Width / 2f + MidDist, (float)GraphicsDevice.Viewport.Height / 2f + MidDist + MidLen, 0), Color.White);

            pointList[14] = new VertexPositionColor(new Vector3((float)GraphicsDevice.Viewport.Width / 2f + MidDist + MidLen, (float)GraphicsDevice.Viewport.Height / 2f + MidDist + MidLen, 0), Color.White);
            pointList[15] = new VertexPositionColor(new Vector3((float)GraphicsDevice.Viewport.Width / 2f + MidDist + MidLen, (float)GraphicsDevice.Viewport.Height / 2f + MidDist, 0), Color.White);


            // Initialize the vertex buffer, allocating memory for each vertex.
            vertexBuffer = new VertexBuffer(GraphicsDevice,
                VertexPositionColor.SizeInBytes * (pointList.Length),
                BufferUsage.None);

            // Set the vertex buffer data to the array of vertices.
            vertexBuffer.SetData<VertexPositionColor>(pointList);


            // Initialize an array of indices of type short.
            //lineListIndices = new short[(points * 2) - 2];

            // Populate the array with references to indices in the vertex buffer
            //for (int i = 0; i < points - 1; i++)
            //{
            //    lineListIndices[i * 2] = (short)(i);
            //    lineListIndices[(i * 2) + 1] = (short)(i + 1);
            //}
            lineListIndices = new short[points];
            for (int i = 0; i < points; i++)
            {
                lineListIndices[i] = (short)(i);
            }


        }

        void StateTransition( GameTime gameTime )
        {
            PlayerIndex playerIndex;
            Input.Update();

            switch (State.Wait2ProcessState)
            {
                case SystemState.S_State.STATE_MENU_MAIN:
                    if (!no_ghmenu2)
                    {
                        _ghMenu2.Enabled = true;
                        _ghMenu2.Visible = true;
                    }
                    State.CurrState = State.Wait2ProcessState;
                    State.Wait2ProcessState = SystemState.S_State.STATE_NONE;
                    break;

                case SystemState.S_State.STATE_NAVIGATION:
                    _MainScreen.SetInstrumentError( State.ErrorX, State.ErrorY, State.ErrorZ );
                    State.CurrState = State.Wait2ProcessState;
                    State.Wait2ProcessState = SystemState.S_State.STATE_NONE;
                    break;
                case SystemState.S_State.STATE_MENU_SCENARIO :
                    _g3DMenu.Enabled = true;
                    _g3DMenu.Visible = true;
                    State.CurrState = State.Wait2ProcessState;
                    State.Wait2ProcessState = SystemState.S_State.STATE_NONE; 
                    break;


                case SystemState.S_State.STATE_MENU_OPTIONS:
                    _settingError.Enabled = true;
                    _settingError.Visible = true;
                    State.CurrState = State.Wait2ProcessState;
                    State.Wait2ProcessState = SystemState.S_State.STATE_NONE;                    
                    break;

                case SystemState.S_State.STATE_MENU_RESULT:                    
                    _recordMenu.ExitState = 0;
                    _recordMenu.Enabled = true;
                    _recordMenu.Visible = true;
                    State.CurrState = State.Wait2ProcessState;
                    State.Wait2ProcessState = SystemState.S_State.STATE_NONE;
                    break;

            }

            // if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            //    this.Exit();

            switch (State.CurrState)
            {

                case SystemState.S_State.STATE_INIT:             // 遊戲開始
                    
                    State.CurrState = SystemState.S_State.STATE_OPENING; 
                    break;

                case SystemState.S_State.STATE_OPENING:          // 小動畫
                    
                    // Show SimuSurvey2.PNG 於畫面中央
                    State.CurrState = SystemState.S_State.STATE_MENU_WAIT_START; 
                    break;

                case SystemState.S_State.STATE_MENU_WAIT_START:  // 等待 Start 或 A鍵
                    
                    // 持續動作
                   
                    if ((Input.IsNewButtonPress(Buttons.A, null, out playerIndex)) ||
                       (Input.IsNewKeyPress(Keys.Space, null, out playerIndex)) ||
                        (Input.IsNewKeyPress(Keys.Enter, null, out playerIndex)))
                    {
                        TitleScreen.Dispose();

                        // Show 選項 Background, Highlight 第 m 項 ( m = 0 at startup )  
                        State.CurrState = SystemState.S_State.STATE_MENU_SCENARIO;
                        State.SelectItemMenuScenario = 0; // 只有第一次Init
                        /* show menu and hightlight one */
                        _g3DMenu.SelectedItem = 0;
                        _g3DMenu.Enabled = true;
                        _g3DMenu.Visible = true;

                    }
                    else if (Input.IsNewKeyPress(Keys.F1, null, out playerIndex))
                    {
                        if (graphics.IsFullScreen)
                            graphics.IsFullScreen = false; 
                        else
                            graphics.IsFullScreen = true; 
                    }

                        
                    break;
                case SystemState.S_State.STATE_MENU_SCENARIO:    // 選擇場景, 圖形式, 單層
                    
                    if (!_g3DMenu.Visible)
                    {

                        if (_g3DMenu.ExitState == 1)
                        {
                            // 在此載入不同場景資料
                            // State.CurrState = SystemState.S_State.STATE_MENU_MAIN;
                            //_ghMenu2.SelectedItem = 0;
                            //_ghMenu2.Enabled = true;
                            //_ghMenu2.Visible = true;

                            State.CurrState = SystemState.S_State.STATE_NAVIGATION;


                            Vector3 _p;
                            float _h;
                            int _RadarIndex; 
                            Random _Rand = new Random();
                            double _RandN = 0.0f ;
                            // 目前只有兩種, 暫時先用, 日後修改
                            State.TrainingN = _g3DMenu.SelectedItem;
                            _MainScreen.ChangeTankTerrain(_g3DMenu.SelectedItem); // 調整 VisualObject 高度
                            State.SkyIndex = State.TrainingN;
                            ResetEntities();
                            _MainScreen.SetInstrumentError(State.ErrorX, State.ErrorY, State.ErrorZ);
                            State.RelativeAngleBase = 0f; 
                            switch ( _g3DMenu.SelectedItem )
                            {
                                case 0 :
                                    State.SkySphereRotate = true; 
                                    State.InsType = InstrumentType.Leveling;  // 水準儀
                                    // State.RulerType = 0;// 標尺
                                    State.Ruler_Ns = 3; // 2支
                                    State.Pole_Ns = 0; // 0支
                                    State.SetPoint_Ns = 2; // 兩個標示牌
                                    State.RulerPad_Ns = 1; // 1個尺墊
                                    State.SetPoint0_Enabled = false;
                                    State.BaseHeight = 3000; // 30M
                                    State.Mirror_Ns = 0;
                                    radar.RadarRange = 500f; 
                                // _MainScreen.ChangeTerrain(_tTerrain[0], _tTexture[0]);
                                //_MainScreen.ChangeTankTerrain(0);
                                _p = new Vector3(0f, 0f, 100f);
                                _MainScreen.SetInstrumentWithName(@"TotalStation_1");
                                _MainScreen.SetInstrument(_p);
                                State.Ins_Position = _p;
                                _RadarIndex  = AddEntities(_p, InstrumentType.Leveling);
                                _MainScreen.SetInstrumentRadarIndex(_RadarIndex); 
                                _MainScreen.TelescopeRotationValue = 0; 
                                _p = new Vector3(-50f, 0f, 100f);
                                _MainScreen.SetInstrumentWithName(@"Ruler_1");
                                _MainScreen.SetInstrument(_p);
                                _MainScreen.ReSetInstrumentLength();
                                State.setRuler_Position(0, _p);
                                _RadarIndex  = AddEntities(_p, InstrumentType.Ruler);
                                _MainScreen.SetInstrumentRadarIndex(_RadarIndex);
                                _p = new Vector3(250f, 0f, -200f);
                                _MainScreen.SetInstrumentWithName(@"Ruler_2");
                                _MainScreen.SetInstrument(_p);
                                _MainScreen.ReSetInstrumentLength();
                                State.setRuler_Position(1, _p);
                                _RadarIndex = AddEntities(_p, InstrumentType.Ruler);
                                _MainScreen.SetInstrumentRadarIndex(_RadarIndex);
                                _p = new Vector3(0f, 0f, -100f);
                                _MainScreen.SetInstrumentWithName(@"Ruler_3");
                                _MainScreen.SetInstrument(_p);
                                _MainScreen.ReSetInstrumentLength();
                                State.setRuler_Position(2, _p);
                                _RadarIndex = AddEntities(_p, InstrumentType.Ruler);
                                _MainScreen.SetInstrumentRadarIndex(_RadarIndex);
                                // test random

                                if (State.Preference_Random_Enabled)
                                {
                                   _RandN = _Rand.Next(100)/4; 
                                }


                                _p = new Vector3(-50f, 0f, 30f);
                                _MainScreen.SetInstrumentWithName(@"SetPoint_1");
                                _p.Y = _MainScreen.SetInstrument(_p);
                                State.setSetPoint_Position(1, _p);
                                _MainScreen.SetInstrumentWithName(@"HintBox_1");
                                _p.Y = 0;
                                _p += new Vector3(0, 0, -20);
                                _MainScreen.SetInstrument(_p);

                                _p = new Vector3(300f, 0f, -260f);
                                _MainScreen.SetInstrumentWithName(@"SetPoint_2");
                                _p.Y = _MainScreen.SetInstrument(_p);
                                State.setSetPoint_Position(2, _p);
                                _MainScreen.SetInstrumentWithName(@"HintBox_2");
                                _p.Y = 0;
                                _p += new Vector3(0, 0, -20);
                                _p.Y = _MainScreen.SetInstrument(_p);

                                _p = new Vector3(0f, 0f, 0f);
                                _MainScreen.SetInstrumentWithName(@"RulerPad_1");
                                _p.Y = _MainScreen.SetInstrument(_p);
                                State.setRulerPad_Position(1, _p);


                                _p = new Vector3(18, 24f, 113);
                                _h = _MainScreen.GetHeight(_p);
                                _MainScreen.SetCamera(_p + new Vector3(0f, _h, 0f), _p + new Vector3(-40, 0f, -60) + new Vector3(0f, _h, 0f));

                                _MainScreen.setVisualObject("stone", true);
                                _MainScreen.moveVisualObject("stone", Matrix.CreateTranslation(100f, 0f, 0f));

                                _MainScreen.moveVisualObject("tree01", Matrix.CreateTranslation(100f, 0, -80f));

                                _MainScreen.setVisualObject("ntuboy8", true);
                                _MainScreen.moveVisualObject("ntuboy8", Matrix.CreateRotationY(MathHelper.Pi / 2f) * Matrix.CreateTranslation(-250f, 0, 0f));

                                _MainScreen.setVisualObject("housestory01", true);
                                _MainScreen.moveVisualObject("housestory01", Matrix.CreateRotationY(-MathHelper.Pi / 2f) * Matrix.CreateTranslation(150f, 0f, -400f));

                                _MainScreen.setVisualObject("housestory02", true);
                                _MainScreen.moveVisualObject("housestory02", Matrix.CreateTranslation(30f, -5f, -600f));

                                _MainScreen.setVisualObject("tree10_1", true);
                                _MainScreen.moveVisualObject("tree10_1", Matrix.CreateTranslation(-200f, 0f, -500f));
                                _MainScreen.setVisualObject("tree10_2", true);
                                _MainScreen.moveVisualObject("tree10_2", Matrix.CreateRotationY(MathHelper.Pi / 4f) * Matrix.CreateTranslation(-250f, 0f, -450f));
                                _MainScreen.setVisualObject("tree10_3", true);
                                _MainScreen.moveVisualObject("tree10_3", Matrix.CreateRotationY(MathHelper.Pi / 4f) * Matrix.CreateTranslation(-250f, 0f, -450f));
                                _MainScreen.setVisualObject("tree10_4", true);
                                _MainScreen.moveVisualObject("tree10_4", Matrix.CreateRotationY(-MathHelper.Pi / 3f) * Matrix.CreateTranslation(-300f, 0f, -420f));

                                ParticleEnabled = true;
                                SmokePos = new Vector3(195, 80, -380);
                                    

                                State.CurrNaviState = SystemState.S_State_Navigation.STATE_ME;
                                break; 
                                case 1 :
                                State.SkySphereRotate = true; 
                                State.InsType = InstrumentType.TotalStation;   // 全站儀
                                // State.RulerType = 1; // 標桿
                                State.Ruler_Ns = 0;  // 0支
                                State.Pole_Ns = 2; // 2支
                                State.SetPoint_Ns = 2; // 兩個標示牌
                                State.RulerPad_Ns = 0; // 0個尺墊
                                State.SetPoint0_Enabled = true; // 另外有一個P點
                                State.BaseHeight = 3000; // 30M
                                State.Mirror_Ns = 0;
                                radar.RadarRange = 300f; 
                                // _MainScreen.ChangeTerrain(_tTerrain[0], _tTexture[0]);
                                //_MainScreen.ChangeTankTerrain(0);
                                _p = new Vector3(0f, 0f, 100f);
                                _MainScreen.SetInstrumentWithName(@"TotalStation_1");
                                _MainScreen.SetInstrument(_p);
                                State.Ins_Position = _p;
                                AddEntities(_p, InstrumentType.TotalStation);
                                _MainScreen.TelescopeRotationValue = 0; 
                                _p = new Vector3(-50f, 0f, 100f);

                                _MainScreen.SetInstrumentWithName(@"Pole_1");
                                _MainScreen.SetInstrument(_p);
                                _MainScreen.ReSetInstrumentLength();
                                State.setRuler_Position(0, _p);
                                AddEntities(_p, InstrumentType.Pole);
                                _p = new Vector3(90f, 0f, 0f);
                                _MainScreen.SetInstrumentWithName(@"Pole_2");
                                _MainScreen.SetInstrument(_p);
                                _MainScreen.ReSetInstrumentLength();
                                State.setRuler_Position(1, _p);
                                AddEntities(_p, InstrumentType.Pole);
                                // test random

                                if (State.Preference_Random_Enabled)
                                {
                                    _RandN = _Rand.Next(100) / 4;
                                }



                                _p = new Vector3(130f, 0f, 100f);
                                _MainScreen.SetInstrumentWithName(@"SetPoint_0");
                                _p.Y = _MainScreen.SetInstrument(_p);
                                State.setSetPoint_Position(0, _p);
                                _MainScreen.SetInstrumentWithName(@"HintBox_0");
                                _p.Y = 0;
                                _p += new Vector3(0, 0, -20);
                                _MainScreen.SetInstrument(_p);

                                _p = new Vector3(-160f, 0f, 270f);
                                _MainScreen.SetInstrumentWithName(@"SetPoint_1");
                                _p.Y = _MainScreen.SetInstrument(_p);
                                State.setSetPoint_Position(1, _p);
                                _MainScreen.SetInstrumentWithName(@"HintBox_1");
                                _p.Y = 0;
                                _p += new Vector3(0, 0, -20);
                                _MainScreen.SetInstrument(_p);

                                _p = new Vector3(140f, 0f, -160f);
                                _MainScreen.SetInstrumentWithName(@"SetPoint_2");
                                _p.Y = _MainScreen.SetInstrument(_p);
                                State.setSetPoint_Position(2, _p);
                                _MainScreen.SetInstrumentWithName(@"HintBox_2");
                                _p.Y = 0;
                                _p += new Vector3(0, 0, -20);
                                _MainScreen.SetInstrument(_p);




                                _p = new Vector3(30, 30f, 130);
                                _h = _MainScreen.GetHeight(_p);
                                _MainScreen.SetCamera(_p + new Vector3(0f, _h, 0f), _p + new Vector3(-30, 0f, -60) + new Vector3(0f, _h, 0f));


                                _MainScreen.setVisualObject("stone", false );
                                // _MainScreen.moveVisualObject("stone", Matrix.CreateTranslation(100f, 0f, 0f));

                                _MainScreen.setVisualObject("ntuboy8", false);

                                _MainScreen.setVisualObject("housestory01", true);
                                _MainScreen.moveVisualObject("housestory01", Matrix.CreateRotationY(-MathHelper.Pi / 2f) * Matrix.CreateTranslation(150f, 0f, -400f));

                                _MainScreen.setVisualObject("housestory02", true);
                                _MainScreen.moveVisualObject("housestory02", Matrix.CreateTranslation(-200f, 0, 50f));

                                _MainScreen.setVisualObject("tree10_1", true);
                                _MainScreen.moveVisualObject("tree10_1", Matrix.CreateTranslation(-200f, 0f, -500f));
                                _MainScreen.setVisualObject("tree10_2", true);
                                _MainScreen.moveVisualObject("tree10_2", Matrix.CreateRotationY(MathHelper.Pi / 4f) * Matrix.CreateTranslation(-250f, 0f, -450f));
                                _MainScreen.setVisualObject("tree10_3", true);
                                _MainScreen.moveVisualObject("tree10_3", Matrix.CreateRotationY(MathHelper.Pi / 4f) * Matrix.CreateTranslation(-250f, 0f, -450f));
                                _MainScreen.setVisualObject("tree10_4", true);
                                _MainScreen.moveVisualObject("tree10_4", Matrix.CreateRotationY(-MathHelper.Pi / 3f) * Matrix.CreateTranslation(-300f, 0f, -420f));

                                ParticleEnabled = true;
                                SmokePos = new Vector3(195, 80, -380);


                                State.CurrNaviState = SystemState.S_State_Navigation.STATE_ME;
                                break; 
                                case 2 :
                                State.SkySphereRotate = true; 
                                State.InsType = InstrumentType.Theodolite;   // 經緯儀
                                // State.RulerType = 0; // 標尺
                                State.Ruler_Ns = 2;  // 2支
                                State.Pole_Ns = 0; // 0支
                                State.SetPoint_Ns = 3; // 3個標示牌
                                State.RulerPad_Ns = 0; // 0個尺墊
                                State.SetPoint0_Enabled = false; // 無P點
                                State.BaseHeight = 3000; // 30M
                                State.Mirror_Ns = 1;
                                radar.RadarRange = 500f;    
                                // _MainScreen.ChangeTerrain(_tTerrain[0], _tTexture[0]);
                                // _MainScreen.ChangeTankTerrain(0);
                                _p = new Vector3(0f, 0f, 100f);
                                _MainScreen.SetInstrumentWithName(@"TotalStation_1");
                                _MainScreen.SetInstrument(_p);
                                State.Ins_Position = _p;
                                AddEntities(_p, InstrumentType.Theodolite);
                                _MainScreen.TelescopeRotationValue = 0; 
                                _p = new Vector3(-70f, 0f, 20f);
                                _MainScreen.SetInstrumentWithName(@"Ruler_1");
                                _MainScreen.SetInstrument(_p);
                                _MainScreen.ReSetInstrumentLength();
                                State.setRuler_Position(0, _p);
                                AddEntities(_p, InstrumentType.Ruler);
                                _p = new Vector3(190f, 0f, -70f);
                                _MainScreen.SetInstrumentWithName(@"Ruler_2");
                                _MainScreen.SetInstrument(_p);
                                _MainScreen.ReSetInstrumentLength();
                                State.setRuler_Position(1, _p);
                                AddEntities(_p, InstrumentType.Ruler);

                                _p = new Vector3(-20f, 0f, 0f);
                                _MainScreen.SetInstrumentWithName(@"Mirror_1");
                                _MainScreen.SetInstrument(_p);
                                _MainScreen.ReSetInstrumentLength();
                                State.setMirror_Position(0, _p);
                                AddEntities(_p, InstrumentType.Mirror);
                                // test random

                                if (State.Preference_Random_Enabled)
                                {
                                    _RandN = _Rand.Next(100) / 4;
                                }

                                _p = new Vector3(-210f, 0f, -10f);
                                _MainScreen.SetInstrumentWithName(@"SetPoint_1");
                                _p.Y = _MainScreen.SetInstrument(_p);
                                State.setSetPoint_Position(1, _p);
                                _MainScreen.SetInstrumentWithName(@"HintBox_1");
                                _p.Y = 0;
                                _p += new Vector3(0, 0, -20);
                                _MainScreen.SetInstrument(_p);

                                _p = new Vector3(150f, 0f, 150f);
                                _MainScreen.SetInstrumentWithName(@"SetPoint_2");
                                _p.Y = _MainScreen.SetInstrument(_p);
                                State.setSetPoint_Position(2, _p);
                                _MainScreen.SetInstrumentWithName(@"HintBox_2");
                                _p.Y = 0;
                                _p += new Vector3(0, 0, -20);
                                _MainScreen.SetInstrument(_p);

                                _p = new Vector3(270f, 0f, -130f);
                                _MainScreen.SetInstrumentWithName(@"SetPoint_3");
                                _p.Y = _MainScreen.SetInstrument(_p);
                                State.setSetPoint_Position(3, _p);
                                _MainScreen.SetInstrumentWithName(@"HintBox_3");
                                _p.Y = 0;
                                _p += new Vector3(0, 0, -20);
                                _MainScreen.SetInstrument(_p);



                                _p = new Vector3(-10, 30f, 120);
                                _h = _MainScreen.GetHeight(_p);
                                _MainScreen.SetCamera(_p + new Vector3(0f, _h, 0f), _p + new Vector3(10, 0f, -60) + new Vector3(0f, _h, 0f));

                                _MainScreen.setVisualObject("stone", false);
                                // _MainScreen.moveVisualObject("stone", Matrix.CreateTranslation(100f, 0f, 0f));

                                _MainScreen.moveVisualObject("tree01", Matrix.CreateTranslation(-300f, 0, -80f));

                                _MainScreen.setVisualObject("ntuboy8", false);
                                // _MainScreen.moveVisualObject("ntuboy8", Matrix.CreateRotationY(MathHelper.Pi / 2f) * Matrix.CreateTranslation(-250f, 0, 0f));


                                _MainScreen.setVisualObject("housestory01", true);
                                _MainScreen.moveVisualObject("housestory01", Matrix.CreateRotationY(-MathHelper.Pi / 2f) * Matrix.CreateTranslation(150f, 0f, -200f));

                                _MainScreen.setVisualObject("housestory02", true);
                                _MainScreen.moveVisualObject("housestory02", Matrix.CreateTranslation(30f, -5f, -600f));

                                _MainScreen.setVisualObject("tree10_1", true);
                                _MainScreen.moveVisualObject("tree10_1", Matrix.CreateTranslation(-200f, 0f, -500f));
                                _MainScreen.setVisualObject("tree10_2", true);
                                _MainScreen.moveVisualObject("tree10_2", Matrix.CreateRotationY(MathHelper.Pi / 4f) * Matrix.CreateTranslation(-250f, 0f, -50f));
                                _MainScreen.setVisualObject("tree10_3", true);
                                _MainScreen.moveVisualObject("tree10_3", Matrix.CreateRotationY(MathHelper.Pi / 4f) * Matrix.CreateTranslation(-250f, 0f, -50f));
                                _MainScreen.setVisualObject("tree10_4", true);
                                _MainScreen.moveVisualObject("tree10_4", Matrix.CreateRotationY(-MathHelper.Pi / 3f) * Matrix.CreateTranslation(-300f, 0f, -20f));

                                ParticleEnabled = true;
                                SmokePos = new Vector3(50f, 80f, -645f);

                                State.CurrNaviState = SystemState.S_State_Navigation.STATE_ME;

                                break; 
                                case 3 :
                                State.SkySphereRotate = false; 
                                State.InsType = InstrumentType.TotalStation;   // 全站儀
                                // State.RulerType = 1; // 標桿
                                State.Ruler_Ns = 0;  // 0 支
                                State.Pole_Ns = 2; // 2 支
                                State.SetPoint_Ns = 4; // 4個標示牌
                                State.RulerPad_Ns = 0; // 0個尺墊
                                State.SetPoint0_Enabled = false; // 無P點
                                State.BaseHeight = 3000; // 30M
                                State.Mirror_Ns = 2;
                                radar.RadarRange = 400f; 
                                // _MainScreen.ChangeTerrain(_tTerrain[0], _tTexture[0]);
                                // _MainScreen.ChangeTankTerrain(0);
                                _p = new Vector3(0f, 0f, 100f);
                                _MainScreen.SetInstrumentWithName(@"TotalStation_1");
                                _MainScreen.SetInstrument(_p);
                                State.Ins_Position = _p;
                                AddEntities(_p, InstrumentType.TotalStation);
                                _MainScreen.TelescopeRotationValue = 0; 
                                _p = new Vector3(-50f, 0f, 25f);
                                _MainScreen.SetInstrumentWithName(@"Pole_1");
                                _MainScreen.SetInstrument(_p);
                                _MainScreen.ReSetInstrumentLength();
                                State.setRuler_Position(0, _p);
                                AddEntities(_p, InstrumentType.Pole);
                                _p = new Vector3(120f, 0f, 140f);
                                _MainScreen.SetInstrumentWithName(@"Pole_2");
                                _MainScreen.SetInstrument(_p);
                                _MainScreen.ReSetInstrumentLength();
                                State.setRuler_Position(1, _p);
                                AddEntities(_p, InstrumentType.Pole);


                                _p = new Vector3(-70f, 0f, 50f);
                                _MainScreen.SetInstrumentWithName(@"Mirror_1");
                                _MainScreen.SetInstrument(_p);
                                _MainScreen.ReSetInstrumentLength();
                                State.setMirror_Position(0, _p);
                                AddEntities(_p, InstrumentType.Mirror);


                                _p = new Vector3(130f, 0f, 160f);
                                _MainScreen.SetInstrumentWithName(@"Mirror_2");
                                _MainScreen.SetInstrument(_p);
                                _MainScreen.ReSetInstrumentLength();
                                State.setMirror_Position(1, _p);
                                AddEntities(_p, InstrumentType.Mirror);


                                //_p = new Vector3(300f, 0f, 50f);
                                //_MainScreen.SetInstrumentWithName(@"Pole_3");
                                //_MainScreen.SetInstrument(_p);
                                //State.setRuler_Position(2, _p);
                                //AddEntities(_p, InstrumentType.Pole);
                                //_p = new Vector3(120f, 0f, 130f);
                                //_MainScreen.SetInstrumentWithName(@"Pole_4");
                                //_MainScreen.SetInstrument(_p);
                                //State.setRuler_Position(3, _p);
                                //AddEntities(_p, InstrumentType.Pole);


                                // test random

                                if (State.Preference_Random_Enabled)
                                {
                                    _RandN = _Rand.Next(100) / 4;
                                }

                                _p = new Vector3(-84f, 0f, 15f);
                                _MainScreen.SetInstrumentWithName(@"SetPoint_1");
                                _p.Y = _MainScreen.SetInstrument(_p);
                                State.setSetPoint_Position(1, _p);
                                _MainScreen.SetInstrumentWithName(@"HintBox_1");
                                _p.Y = 0;
                                _p += new Vector3(0, 0, -20);
                                _MainScreen.SetInstrument(_p);

                                _p = new Vector3(185f, 0f, -300f);
                                _MainScreen.SetInstrumentWithName(@"SetPoint_2");
                                _p.Y = _MainScreen.SetInstrument(_p);
                                State.setSetPoint_Position(2, _p);
                                _MainScreen.SetInstrumentWithName(@"HintBox_2");
                                _p.Y = 0;
                                _p += new Vector3(0, 0, -20);
                                _MainScreen.SetInstrument(_p);

                                _p = new Vector3(400f, 0f, 0f);
                                _MainScreen.SetInstrumentWithName(@"SetPoint_3");
                                _p.Y = _MainScreen.SetInstrument(_p);
                                State.setSetPoint_Position(3, _p);
                                _MainScreen.SetInstrumentWithName(@"HintBox_3");
                                _p.Y = 0;
                                _p += new Vector3(0, 0, -20);
                                _MainScreen.SetInstrument(_p);

                                _p = new Vector3(134f, 0f, 163f);
                                _MainScreen.SetInstrumentWithName(@"SetPoint_4");
                                _p.Y = _MainScreen.SetInstrument(_p);
                                State.setSetPoint_Position(4, _p);
                                _MainScreen.SetInstrumentWithName(@"HintBox_4"); 
                                _p.Y = 0;
                                _p += new Vector3(0, 0, -20);
                                _MainScreen.SetInstrument(_p );


                                _p = new Vector3(-30, 30f, 150);
                                _h = _MainScreen.GetHeight(_p);
                                _MainScreen.SetCamera(_p + new Vector3(0f, _h, 0f), _p + new Vector3(10, 0f, -60) + new Vector3(0f, _h, 0f));

                                _MainScreen.setVisualObject("stone", true);
                                _MainScreen.moveVisualObject("stone", Matrix.CreateTranslation(80f, 0f, 30f));

                                _MainScreen.moveVisualObject("tree01", Matrix.CreateTranslation(-100f, 0, -80f));

                                _MainScreen.setVisualObject("ntuboy8", false);
                                // _MainScreen.moveVisualObject("ntuboy8", Matrix.CreateRotationY(MathHelper.Pi / 2f) * Matrix.CreateTranslation(-250f, 0, 0f));

                                _MainScreen.setVisualObject("housestory01", true);
                                _MainScreen.moveVisualObject("housestory01", Matrix.CreateRotationY(-MathHelper.Pi / 2f) * Matrix.CreateTranslation(150f, 0f, -100f));

                                _MainScreen.setVisualObject("housestory02", true);
                                _MainScreen.moveVisualObject("housestory02", Matrix.CreateTranslation(30f, -5f, -600f));

                                _MainScreen.setVisualObject("tree10_1", true);
                                _MainScreen.moveVisualObject("tree10_1", Matrix.CreateTranslation(-200f, 0f, -500f));
                                _MainScreen.setVisualObject("tree10_2", true);
                                _MainScreen.moveVisualObject("tree10_2", Matrix.CreateRotationY(MathHelper.Pi / 4f) * Matrix.CreateTranslation(-250f, 0f, -450f));
                                _MainScreen.setVisualObject("tree10_3", true);
                                _MainScreen.moveVisualObject("tree10_3", Matrix.CreateRotationY(MathHelper.Pi / 4f) * Matrix.CreateTranslation(-250f, 0f, -450f));
                                _MainScreen.setVisualObject("tree10_4", true);
                                _MainScreen.moveVisualObject("tree10_4", Matrix.CreateRotationY(-MathHelper.Pi / 3f) * Matrix.CreateTranslation(-300f, 0f, -420f));

                                ParticleEnabled = true;
                                SmokePos = new Vector3(50f, 80f, -645f);


                                State.CurrNaviState = SystemState.S_State_Navigation.STATE_ME;

                                break;
                                case 4:
                                State.SkySphereRotate = false; 
                                State.InsType = InstrumentType.TotalStation;   // 全站儀
                                // State.RulerType = 0; // 標尺
                                State.Ruler_Ns = 2;  // 2 支
                                State.Pole_Ns = 2; // 2 支
                                State.SetPoint_Ns = 4; // 4個標示牌
                                State.RulerPad_Ns = 1; // 1個尺墊
                                State.SetPoint0_Enabled = false; // 無P點
                                State.BaseHeight = 3000; // 30M
                                State.Mirror_Ns = 2;
                                radar.RadarRange = 500f; 

                                // _MainScreen.ChangeTankTerrain(4);
                                _p = new Vector3(0f, 0f, 100f);
                                _MainScreen.SetInstrumentWithName(@"TotalStation_1");
                                _MainScreen.SetInstrument(_p);
                                State.Ins_Position = _p;
                                AddEntities(_p, InstrumentType.TotalStation);
                                _MainScreen.TelescopeRotationValue = 0; 
                                _p = new Vector3(-50f, 0f, 25f);
                                _MainScreen.SetInstrumentWithName(@"Ruler_1");
                                _MainScreen.SetInstrument(_p);
                                _MainScreen.ReSetInstrumentLength();
                                State.setRuler_Position(0, _p);
                                AddEntities(_p, InstrumentType.Ruler);
                                _p = new Vector3(160f, 0f, -140f);
                                _MainScreen.SetInstrumentWithName(@"Ruler_2");
                                _MainScreen.SetInstrument(_p);
                                _MainScreen.ReSetInstrumentLength();
                                State.setRuler_Position(1, _p);
                                AddEntities(_p, InstrumentType.Ruler);

                                _p = new Vector3(300f, 0f, 50f);
                                _MainScreen.SetInstrumentWithName(@"Pole_1");
                                _MainScreen.SetInstrument(_p);
                                _MainScreen.ReSetInstrumentLength();
                                State.setPole_Position(0, _p);
                                AddEntities(_p, InstrumentType.Pole);
                                _p = new Vector3(120f, 0f, 130f);
                                _MainScreen.SetInstrumentWithName(@"Pole_2");
                                _MainScreen.SetInstrument(_p);
                                _MainScreen.ReSetInstrumentLength();
                                State.setPole_Position(1, _p);
                                AddEntities(_p, InstrumentType.Pole);

                                _p = new Vector3(-70f, 0f, 50f);
                                _MainScreen.SetInstrumentWithName(@"Mirror_1");
                                _MainScreen.SetInstrument(_p);
                                _MainScreen.ReSetInstrumentLength();
                                State.setMirror_Position(0, _p);
                                AddEntities(_p, InstrumentType.Mirror);


                                _p = new Vector3(130f, 0f, 160f);
                                _MainScreen.SetInstrumentWithName(@"Mirror_2");
                                _MainScreen.SetInstrument(_p);
                                _MainScreen.ReSetInstrumentLength();
                                State.setMirror_Position(1, _p);
                                AddEntities(_p, InstrumentType.Mirror);



                                // test random

                                if (State.Preference_Random_Enabled)
                                {
                                    _RandN = _Rand.Next(100) / 4;
                                }

                                _p = new Vector3(-84f, 0f, 15f);
                                _MainScreen.SetInstrumentWithName(@"SetPoint_1");
                                _p.Y = _MainScreen.SetInstrument(_p);
                                State.setSetPoint_Position(1, _p);
                                _MainScreen.SetInstrumentWithName(@"HintBox_1");
                                _p.Y = 0;
                                _p += new Vector3(0, 0, -20);
                                _MainScreen.SetInstrument(_p);

                                _p = new Vector3(185f, 0f, -180f);
                                _MainScreen.SetInstrumentWithName(@"SetPoint_2");
                                _p.Y = _MainScreen.SetInstrument(_p);
                                State.setSetPoint_Position(2, _p);
                                _MainScreen.SetInstrumentWithName(@"HintBox_2");
                                _p.Y = 0;
                                _p += new Vector3(0, 0, -20);
                                _MainScreen.SetInstrument(_p);

                                _p = new Vector3(339f, 0f, 34f);
                                _MainScreen.SetInstrumentWithName(@"SetPoint_3");
                                _p.Y = _MainScreen.SetInstrument(_p);
                                State.setSetPoint_Position(3, _p);
                                _MainScreen.SetInstrumentWithName(@"HintBox_3");
                                _p.Y = 0;
                                _p += new Vector3(0, 0, -20);
                                _MainScreen.SetInstrument(_p);

                                _p = new Vector3(134f, 0f, 163f);
                                _MainScreen.SetInstrumentWithName(@"SetPoint_4");
                                _p.Y = _MainScreen.SetInstrument(_p);
                                State.setSetPoint_Position(4, _p);
                                _MainScreen.SetInstrumentWithName(@"HintBox_4");
                                _p.Y = 0;
                                _p += new Vector3(0, 0, -20);
                                _MainScreen.SetInstrument(_p);


                                _p = new Vector3(0f, 0f, 0f);
                                _MainScreen.SetInstrumentWithName(@"RulerPad_1");
                                _p.Y = _MainScreen.SetInstrument(_p);
                                State.setRulerPad_Position(1, _p);


                                _p = new Vector3(-30, 30f, 150);
                                _h = _MainScreen.GetHeight(_p);
                                _MainScreen.SetCamera(_p + new Vector3(0f, _h, 0f), _p + new Vector3(10, 0f, -60) + new Vector3(0f, _h, 0f));

                                _MainScreen.setVisualObject("stone", true);
                                _MainScreen.moveVisualObject("stone", Matrix.CreateTranslation(100f, 0f, 0f));

                                _MainScreen.moveVisualObject("tree01", Matrix.CreateRotationY(MathHelper.Pi / 2f) * Matrix.CreateTranslation(100f, 0, -380f));

                                _MainScreen.setVisualObject("ntuboy8", false );
                                // _MainScreen.moveVisualObject("ntuboy8", Matrix.CreateTranslation(-250f, 0, 0f));

                                _MainScreen.setVisualObject("housestory01", false );
                                // _MainScreen.moveVisualObject("housestory01", Matrix.CreateRotationY(-MathHelper.Pi / 2f) * Matrix.CreateTranslation(150f, 0f, -400f));

                                _MainScreen.setVisualObject("housestory02", true);
                                _MainScreen.moveVisualObject("housestory02", Matrix.CreateTranslation(30f, -5f, -600f));

                                _MainScreen.setVisualObject("tree10_1", true);
                                _MainScreen.moveVisualObject("tree10_1", Matrix.CreateTranslation(-200f, 0f, -500f));
                                _MainScreen.setVisualObject("tree10_2", true);
                                _MainScreen.moveVisualObject("tree10_2", Matrix.CreateRotationY(MathHelper.Pi / 4f) * Matrix.CreateTranslation(-250f, 0f, -450f));
                                _MainScreen.setVisualObject("tree10_3", true);
                                _MainScreen.moveVisualObject("tree10_3", Matrix.CreateRotationY(MathHelper.Pi / 4f) * Matrix.CreateTranslation(-250f, 0f, -450f));
                                _MainScreen.setVisualObject("tree10_4", true);
                                _MainScreen.moveVisualObject("tree10_4", Matrix.CreateRotationY(-MathHelper.Pi / 3f) * Matrix.CreateTranslation(-300f, 0f, -420f));

                                ParticleEnabled = true;
                                SmokePos = new Vector3(50f, 80f, -645f);


                                State.CurrNaviState = SystemState.S_State_Navigation.STATE_ME;
                                break; 
                            }
                              int _rn = _Rand.Next(3) ;
                              if ( State.AudioEnable )
                                SndBird[_rn].Play();
                             _MainScreen.AutoFace2Ins();
                            // _MainScreen._currentCamera.Initialize(new Vector3(30, 35f, 130), new Vector3(0, 35f, 70));
                            Thread.Sleep(250);
                        }
                        else if(_g3DMenu.ExitState == 2) // 要確認離開
                        {
                            this.Exit();

                        }


                    }
                    break;


                case SystemState.S_State.STATE_MENU_MAIN:        // 選擇功能, 文字式, 掛圖片及說明文字
 


// skc, no_ghmenu2
                    if (!no_ghmenu2)
                    {
                        if (!_ghMenu2.Visible)
                        {
                            if (_ghMenu2.ExitState == 1)
                            {
                                switch (_ghMenu2.SelectedItem)
                                {
                                    case 0:
                                        _MainScreen.SetInstrumentWithName(@"TotalStation_1");
                                        _MainScreen.ChangeCamera(0);
                                        _MainScreen._currentCamera.ZoomRestore(0);
                                        State.CurrState = SystemState.S_State.STATE_NAVIGATION;
                                        State.PickedObj = -1;
                                        State.Ready2PickObj = -1;

                                        break;
                                    //case 1:
                                    //    _MainScreen.SetInstrumentWithName(@"TotalStation_1"); 
                                    //    _MainScreen.ChangeCamera(1);
                                    //    _MainScreen._currentCamera.ZoomRestore(1);
                                    //    State.CurrState = SystemState.S_State.STATE_MENU_SURVEYING;
                                    //    break;
                                    case 1:
                                        
                                        //State.CurrState = SystemState.S_State.STATE_MENU_RESULT;
                                        //_recordMenu.ExitState = 0;
                                        //_recordMenu.Enabled = true;
                                        //_recordMenu.Visible = true;
                                        fTotalElapsedTime = 0.0f;
                                        State.Wait2ProcessState = SystemState.S_State.STATE_MENU_RESULT;

                                        break;
                                    case 2:
                                        //State.CurrState = SystemState.S_State.STATE_MENU_OPTIONS;
                                        //_settingError.Enabled = true;
                                        //_settingError.Visible = true;
                                        fTotalElapsedTime = 0.0f;
                                        State.Wait2ProcessState = SystemState.S_State.STATE_MENU_OPTIONS;
                                        break;
                                    case 3: // 回上一層
                                        
                                        //State.CurrState = SystemState.S_State.STATE_MENU_SCENARIO;
                                        //_g3DMenu.Enabled = true;
                                        //_g3DMenu.Visible = true;
                                        State.Wait2ProcessState = SystemState.S_State.STATE_MENU_SCENARIO; 
                                        Thread.Sleep(250);
                                        break;
                                }
                            }
                            else
                            {

                                //State.CurrState = SystemState.S_State.STATE_MENU_SCENARIO;
                                //_g3DMenu.Enabled = true;
                                //_g3DMenu.Visible = true;
                                State.Wait2ProcessState = SystemState.S_State.STATE_MENU_SCENARIO;
                                Thread.Sleep(250);

                            }
                        }
                    } // no_ghmenu2
                    break;
                case SystemState.S_State.STATE_NAVIGATION:       // 移動物件狀態, 自己, 儀器, 尺1, 尺2


                    //if (Input.IsNewButtonPress(Buttons.Y, null, out playerIndex) ||
                    //    (Input.IsNewKeyPress(Keys.Y)))
                    //{
                    //    if ( State.CurrNaviState == SystemState.S_State_Navigation.STATE_INSTRUMENT )
                    //        _MainScreen.ChangeCamera(-1);

                    //}

                    if (Input.IsNewButtonPress(Buttons.B, null, out playerIndex) ||
                        (Input.IsNewKeyPress(Keys.P, null, out playerIndex )) )
                    {
                       
                        // 以下這一段code , 因為 P 鍵一直處於 Down 狀態而無法開放
                        //GamePadState GPad;
                        //KeyboardState KBoard;
                        //GPad = GamePad.GetState(PlayerIndex.One);
                        //KBoard = Keyboard.GetState();
                        //while (GPad.Buttons.B == ButtonState.Pressed || KBoard.IsKeyDown(Keys.P))
                        //{
                        //    GPad = GamePad.GetState(PlayerIndex.One);
                        //    KBoard = Keyboard.GetState();
                        //    if ( KBoard.IsKeyUp(Keys.P) )
                        //    {
                        //         KBoard = Keyboard.GetState();
                        //    }
                        //}
                        // Thread.Sleep( 250 );
                        // Input.Update();

                        if (State.TrainingN == 0)
                        {
                            

                        }




                        // skc, no_ghmenu2
                        if (!no_ghmenu2)
                        {
                            State.CurrState = SystemState.S_State.STATE_MENU_MAIN;
                            _ghMenu2.Enabled = true;
                            _ghMenu2.Visible = true;
                        }
                        else
                        {
                            _MainMenuScreen = new MainMenuScreen();
                            _ScreenManager.AddScreen( _MainMenuScreen, null);
                            Input.Update();
                            State.CurrState = SystemState.S_State.STATE_MENU_MAIN;                            
                        }
                        State.PickedObj = -1;
                        State.Ready2PickObj = -1; 
                        Thread.Sleep( 250 );
                    }
                    // 切換人和儀器
                    if (Input.IsNewButtonPress(Buttons.X, null, out playerIndex) ||
                        (Input.IsNewKeyPress(Keys.O, null, out playerIndex)))
                    {


                        if (State.OperationMode == 0)
                        {
                            if (State.PickedObj == -1)
                            {
                                if (State.Ready2PickObj != -1) // Pick 
                                {
                                    SndPick.Play();
                                    State.PickedObj = State.Ready2PickObj;
                                    State.Ready2PickObj = -1;
                                    State.Ins_H0 = _MainScreen._currentCamera.Position.Y;
                                    if (State.Ins_H0 > 25)
                                        State.Ins_H0 = 25;



                                }
                                else
                                {
                                    State.RadarOff = State.RadarOff ? false : true;
                                }
                            }
                            else // Drop 
                            {
                                // if ( State.PeakedObj > 0 ) // only for ruler/ pole

                                SndDrop.Play();
                                _MainScreen.AutoAdjust2SetPoint(State.PickedObj);
                                _MainScreen.AutoFace2Ins();

                                if ( State.PickedObj >= 140)
                                {
                                    int _idx = State.PickedObj - 140;
                                    //  調整 HintBox 的位置, 以儀器方向為準放在後面, 需要轉角度, 或是以( 0,0 ) 為準也可以
                                    Vector3 _p = State.getSetPoint_Position ( _idx );
                                    Vector3 _p0 = State.Ins_Position ;
                                    Vector3 _d = new Vector3(_p.X, 0, _p.Z) - new Vector3( _p0.X, 0, _p0.Z );
                                    _d.Normalize();
                                    _d*= 20;
                                    _MainScreen.SetInstrumentWithName(@"HintBox_" + _idx.ToString());
                                    _p.Y = 0;
                                    // _p += new Vector3(0, 0, -20);
                                    _p += _d; 
                                    _MainScreen.SetInstrument(_p);  


                                }



                                // adjust it to be close to SetPoint
                                switch (State.PickedObj)
                                {
                                    case 0:



                                        break;
                                    case 1: case 2: case 3: case 4:





                                        break;

                                }
                                State.PickedObj = -1;

                            }  // Drop





                        } // OperationMode == 0
                        else
                        {

                            NaviBtn_State = 0;
                            // if ((int)State.CurrNaviState < ( State.Max_NaviState - 1 ))
                            if ((int)State.CurrNaviState < (1 + State.Ruler_Ns - 1))
                                State.CurrNaviState += 1;
                            else
                                State.CurrNaviState = 0;
                            _MainScreen.ChangeCamera(0); // Me
                            switch (State.CurrNaviState)
                            {
                                case SystemState.S_State_Navigation.STATE_INSTRUMENT:
                                    _MainScreen.SetInstrumentWithName(@"TotalStation_1");
                                    break;
                                case SystemState.S_State_Navigation.STATE_RULE1:
                                    _MainScreen.SetInstrumentWithName(@"Ruler_1");
                                    break;
                                case SystemState.S_State_Navigation.STATE_RULE2:
                                    _MainScreen.SetInstrumentWithName(@"Ruler_2");
                                    break;
                                case SystemState.S_State_Navigation.STATE_RULE3:
                                    _MainScreen.SetInstrumentWithName(@"Ruler_3");
                                    break;

                            }

                        }



                    } // X 按鍵


                    break;
                case SystemState.S_State.STATE_MENU_SURVEYING:   // 測量進行狀態

                    if (Input.IsNewButtonPress(Buttons.B, null, out playerIndex) ||
                        (Input.IsNewKeyPress(Keys.P, null, out playerIndex )))
                    {
                        _MainScreen.ChangeCamera(0);
                        _MainScreen._currentCamera.ZoomRestore(0);
                        // skc, no_ghmenu2
                        // State.CurrState = SystemState.S_State.STATE_MENU_MAIN;
                        // _ghMenu2.Enabled = true;
                        // _ghMenu2.Visible = true;

                        // change camera position, or it will jump again

                        _MainScreen._currentCamera.MoveForward(-8f);
                        State.CurrState = SystemState.S_State.STATE_NAVIGATION;

                        Thread.Sleep(250);
                    }
                    else if (Input.IsNewButtonPress(Buttons.A, null, out playerIndex) ||
                        (Input.IsNewKeyPress(Keys.Enter, null, out playerIndex))) 
                    {

                        State.RelativeAngleBase = _MainScreen.TribrachRotationValue;

                    }
                    else if (Input.IsNewButtonPress(Buttons.Y, null, out playerIndex))
                    {
                        State.CurrState = SystemState.S_State.STATE_INPUT;

                        //_osinput.Enabled = true;
                        //_osinput.Visible = true;   

                        _osdp.Enabled = true;
                        _osdp.Visible = true;
       

                        Thread.Sleep(250);
                    }

                    break;
                case SystemState.S_State.STATE_INPUT:            // 輸入測量結果, 應該只有跳回 SURVEYING
                    //if (!_oskb.Visible) // Input 必須自己負責跳出
                    if (!_osdp.Visible) // Input 必須自己負責跳出
                    {
                        
                        State.CurrState = SystemState.S_State.STATE_MENU_SURVEYING;
                    }

                    break;
                case SystemState.S_State.STATE_MENU_RESULT:      // 列出測量結果, 可以進入REPLAY
                    /*
                    if (Input.IsNewButtonPress(Buttons.A, null, out playerIndex) ||
                        (Input.IsNewKeyPress(Keys.Enter)))
                    {
                        // 先remark 
                        // State.CurrState = SystemState.S_State.STATE_MENU_REPLAY;
                        State.CurrState = SystemState.S_State.STATE_MENU_MAIN;
                        _ghMenu2.Enabled = true;
                        _ghMenu2.Visible = true;
                        Thread.Sleep(250);

                    }

                    if (Input.IsNewButtonPress(Buttons.B, null, out playerIndex) ||
                        (Input.IsNewKeyPress(Keys.P)))
                    {
                        State.CurrState = SystemState.S_State.STATE_MENU_MAIN;
                        _ghMenu2.Enabled = true;
                        _ghMenu2.Visible = true;
                        Thread.Sleep(250);
                    }
                     */

                    if (_recordMenu.ExitState > 0 )
                    {
                        if (!no_ghmenu2)
                        {
                            State.CurrState = SystemState.S_State.STATE_MENU_MAIN;
                            _ghMenu2.Enabled = true;
                            _ghMenu2.Visible = true;
                            
                        }
                        else
                        {
                            State.CurrState = SystemState.S_State.STATE_NAVIGATION; 
                        }
                        Thread.Sleep(250);

                    }
                    break;
                case SystemState.S_State.STATE_MENU_REPLAY:      // 重播狀態

                    // 依據經過時間移動 Camera


                    if (Input.IsNewButtonPress(Buttons.B, null, out playerIndex))
                    {
                        State.CurrState = SystemState.S_State.STATE_MENU_RESULT;
                        _ghMenu2.Enabled = true;
                        _ghMenu2.Visible = true;
                    }

                    if (Input.IsNewButtonPress(Buttons.A, null, out playerIndex))
                    {
                        // PAUSE/ RESUME
                    }

                    break;
                case SystemState.S_State.STATE_MENU_OPTIONS:     // 效果選單 or HELP    


                    if (_settingError.ExitState > 0)
                    {
                        State.CurrState = SystemState.S_State.STATE_MENU_MAIN;
                        _ghMenu2.Enabled = true;
                        _ghMenu2.Visible = true;
                        _MainScreen.SetInstrumentError(_settingError.Error_X, _settingError.Error_Y, _settingError.Error_Z);
                        Thread.Sleep(250);

                    }

                    //if (Input.IsNewButtonPress(Buttons.A, null, out playerIndex) ||
                    //    Input.IsNewButtonPress(Buttons.B, null, out playerIndex))
                    //{
                    //    State.CurrState = SystemState.S_State.STATE_MENU_MAIN;
                    //    _ghMenu2.Enabled = true;
                    //    _ghMenu2.Visible = true;
                        
                    //}

                    break;           

            }




            if (Input.IsNewButtonPress(Buttons.A, null, out playerIndex))
            {
                // _oskb.Enabled = true;
                // _oskb.Visible = true;
                //_ghMenu.Enabled = true;
                //_ghMenu.Visible = true;
            }



            //if (Input.IsNewKeyPress(Keys.U, null, out playerIndex ))
            //{
            //    _oskb.Enabled = true;
            //    _oskb.Visible = true;
            //}

        }

        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            TimeSpan TS;

            if (gameTime.IsRunningSlowly)
                TS = this.TargetElapsedTime;
            else
                TS = gameTime.ElapsedGameTime;
/*
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            fTotalElapsedTime += elapsed;

            if (fTotalElapsedTime > fKeyPressCheckDelay)
            {
                UpdateInput();
                StateTransition(gameTime);            
            }

*/
            UpdateInput();
            StateTransition(gameTime);
            int _RadarIndex;

            _p0 = _MainScreen.GetInstrumentPositionWithName(@"TotalStation_1");
            // for Navigation Mode Only
            
            if ((State.CurrState == SystemState.S_State.STATE_NAVIGATION) || (State.CurrState == SystemState.S_State.STATE_MENU_SURVEYING))
            {
                // _RadarIndex = _MainScreen.GetInstrumentRadarIndex();
                Enemies[ 0 ].SetPosition( _p0 );
            }
              
            Vector3[] _p = new Vector3[4];
            Vector3[] _p2 = new Vector3[4];
            Vector3[] _pm = new Vector3[4];
            Vector3[] _prp = new Vector3[4];

            // if (State.RulerType == 0)
            // {
                int i;
                for (i = 0; i < State.Ruler_Ns; i++)
                {
                    _p[i] = _MainScreen.GetInstrumentPositionWithName(@"Ruler_" + (i + 1).ToString());
                    // for Navigation Mode Only

                    if ((State.CurrState == SystemState.S_State.STATE_NAVIGATION) || (State.CurrState == SystemState.S_State.STATE_MENU_SURVEYING))
                    {
                        if (i < State.Ruler_Ns)
                        {
                            // _RadarIndex = _MainScreen.GetInstrumentRadarIndex();
                            Enemies[ i + 1 ].SetPosition(_p[i]);
                        }
                    }
                     
                }

            // }
            // else
            // {
                for (i = 0; i < State.Pole_Ns; i++)
                {
                    _p2[i] = _MainScreen.GetInstrumentPositionWithName(@"Pole_" + (i + 1).ToString());


                    if ((State.CurrState == SystemState.S_State.STATE_NAVIGATION) || (State.CurrState == SystemState.S_State.STATE_MENU_SURVEYING))
                    {
                        if (i < State.Pole_Ns)
                        {
                            // _RadarIndex = _MainScreen.GetInstrumentRadarIndex();
                            Enemies[i + 1].SetPosition(_p2[i]);
                        }
                    }

                }
            // }

            // if (State.Mirror_Ns > 0)
            // {
                for (i = 0 ; i < State.Mirror_Ns ; i++ )
                {
                    _pm[i] = _MainScreen.GetInstrumentPositionWithName(@"Mirror_" + (i + 1).ToString());

                    if ((State.CurrState == SystemState.S_State.STATE_NAVIGATION) || (State.CurrState == SystemState.S_State.STATE_MENU_SURVEYING))
                    {
                        if (i < State.Mirror_Ns)
                        {
                            // _RadarIndex = _MainScreen.GetInstrumentRadarIndex();
                            Enemies[ 1+ State.Ruler_Ns + i ].SetPosition(_pm[i]);
                        }
                    }

                }
            // }

            // if (State.RulerPad_Ns > 0)
            // {
                for (i = 0; i < State.RulerPad_Ns; i++)
                {
                    _prp[i] = _MainScreen.GetInstrumentPositionWithName(@"RulerPad_" + (i + 1).ToString());
                }
            //}



            State.Ins_Position = _p0;
            for ( i = 0 ; i < State.Ruler_Ns ; i++ )
                State.setRuler_Position(i, _p[i]);

            for (i = 0; i < State.Pole_Ns; i++)
                State.setPole_Position(i, _p2[i]);

            for (i = 0; i < State.Mirror_Ns; i++)
                State.setMirror_Position(i, _pm[i]);

            for (i = 0; i < State.RulerPad_Ns; i++)
                State.setRulerPad_Position(i, _prp[i]);


            // 這一段以後簡化一下, check 一下高度可否直接用各物件的 Y 
            T4.X = (-_p0.Z + State.BaseNE ) ;  // N
            T4.Y = (_p0.X + State.BaseNE ) ;   // E
            T4.Z = _MainScreen.GetHeight(_p0) ;
            T4.W = _MainScreen.TotalStationCameraHight - T4.Z;                   // HI
            _recordMenu.PosI = T4;


            T3.X = (-_p[0].Z + State.BaseNE) ;  // N
            T3.Y = (_p[0].X + State.BaseNE) ;   // E
            T3.Z = _MainScreen.GetHeight(_p[0]) ; 
            _recordMenu.PosR1 = T3 ;

            T3.X = (-_p[1].Z + State.BaseNE) ;  // N
            T3.Y = (_p[1].X + State.BaseNE) ;   // E
            T3.Z = _MainScreen.GetHeight(_p[1]) ; 
            _recordMenu.PosR2 = T3;

            T3.X = (-_p[2].Z + State.BaseNE) ;  // N
            T3.Y = (_p[2].X + State.BaseNE) ;   // E
            T3.Z = _MainScreen.GetHeight(_p[2]) ;
            _recordMenu.PosR3 = T3;

            T3.X = (-_p[3].Z + State.BaseNE) ;  // N
            T3.Y = (_p[3].X + State.BaseNE) ;   // E
            T3.Z = _MainScreen.GetHeight(_p[3]) ;
            _recordMenu.PosR4 = T3;



            T3.X = (-_p2[0].Z + State.BaseNE);  // N
            T3.Y = (_p2[0].X + State.BaseNE);   // E
            T3.Z = _MainScreen.GetHeight(_p2[0]);
            _recordMenu.PosP1 = T3;

            T3.X = (-_p2[1].Z + State.BaseNE);  // N
            T3.Y = (_p2[1].X + State.BaseNE);   // E
            T3.Z = _MainScreen.GetHeight(_p2[1]);
            _recordMenu.PosP2 = T3;

            T3.X = (-_p[2].Z + State.BaseNE);  // N
            T3.Y = (_p[2].X + State.BaseNE);   // E
            T3.Z = _MainScreen.GetHeight(_p2[2]);
            _recordMenu.PosP3 = T3;

            T3.X = (-_p2[3].Z + State.BaseNE);  // N
            T3.Y = (_p2[3].X + State.BaseNE);   // E
            T3.Z = _MainScreen.GetHeight(_p2[3]);
            _recordMenu.PosP4 = T3;





            T3.X = (-_pm[0].Z + State.BaseNE);  // N
            T3.Y = (_pm[0].X + State.BaseNE);   // E
            T3.Z = _MainScreen.GetHeight(_pm[0]);
            _recordMenu.PosM1 = T3;

            T3.X = (-_pm[1].Z + State.BaseNE);  // N
            T3.Y = (_pm[1].X + State.BaseNE);   // E
            T3.Z = _MainScreen.GetHeight(_pm[1]);
            _recordMenu.PosM2 = T3;

            T3.X = (-_pm[2].Z + State.BaseNE);  // N
            T3.Y = (_pm[2].X + State.BaseNE);   // E
            T3.Z = _MainScreen.GetHeight(_pm[2]);
            _recordMenu.PosM3 = T3;

            T3.X = (-_pm[3].Z + State.BaseNE);  // N
            T3.Y = (_pm[3].X + State.BaseNE);   // E
            T3.Z = _MainScreen.GetHeight(_pm[3]);
            _recordMenu.PosM4 = T3;


// 因為RulerPad 目前不在 recordMenu 中, 所以先不 Update 
            //_BoundingBoxR0 = new BoundingBox(_p2 - new Vector3(10, 100, 10), _p2 + new Vector3(10, 100, 10));
            //_BoundingBoxR1 = new BoundingBox(_p3 - new Vector3(10, 100, 10), _p3 + new Vector3(10, 100, 10));






            // TODO: Add your update logic here
            //_SceneController.Update();


            // terrain.view = camera.view;
            // terrain.projection = camera.projection;
            // ball.Position = terrain.GetHeight(ball.Position);
            // ball.view = camera.view;
            // ball.projection = camera.projection; 
           
            // skc, 不用 Update, 因為是被動改變位置, 無主動 
            foreach (Entity thisEnemy in Enemies)
            {
                // thisEnemy.Update(gameTime, heightMapInfo);
            }

// Particle
            if ((State.CurrState == SystemState.S_State.STATE_NAVIGATION) || (State.CurrState == SystemState.S_State.STATE_MENU_SURVEYING))
            {
                switch (currentState)
                {
                    case ParticleState.Explosions:
                        UpdateExplosions(gameTime);
                        break;

                    case ParticleState.SmokePlume:
                        UpdateSmokePlume();
                        break;

                    case ParticleState.RingOfFire:
                        UpdateFire();
                        break;
                }
            }

            UpdateProjectiles(gameTime);






            base.Update(gameTime);

        }

        /// <summary>
        /// Helper for updating the explosions effect.
        /// </summary>
        void UpdateExplosions(GameTime gameTime)
        {
            if (!Particle_Off)
            {
                timeToNextProjectile -= gameTime.ElapsedGameTime;

                if (timeToNextProjectile <= TimeSpan.Zero)
                {
                    // Create a new projectile once per second. The real work of moving
                    // and creating particles is handled inside the Projectile class.
                    projectiles.Add(new Projectile(explosionParticles,
                                                   explosionSmokeParticles,
                                                   projectileTrailParticles));

                    timeToNextProjectile += TimeSpan.FromSeconds(1);
                }
            }
        }
        /// <summary>
        /// Helper for updating the list of active projectiles.
        /// </summary>
        void UpdateProjectiles(GameTime gameTime)
        {
            int i = 0;

            while (i < projectiles.Count)
            {
                if (!projectiles[i].Update(gameTime))
                {
                    // Remove projectiles at the end of their life.
                    projectiles.RemoveAt(i);
                }
                else
                {
                    // Advance to the next projectile.
                    i++;
                }
            }
        }
        /// <summary>
        /// Helper for updating the smoke plume effect.
        /// </summary>
        void UpdateSmokePlume()
        {
            // This is trivial: we just create one new smoke particle per frame.
            if ( !Particle_Off && ParticleEnabled ) 
            smokePlumeParticles.AddParticle( SmokePos /* new Vector3( 195,80,-380 ) */, Vector3.Zero);
        }


        /// <summary>
        /// Helper for updating the fire effect.
        /// </summary>
        void UpdateFire()
        {
            const int fireParticlesPerFrame = 20;

            // Create a number of fire particles, randomly positioned around a circle.
            for (int i = 0; i < fireParticlesPerFrame; i++)
            {
                fireParticles.AddParticle(RandomPointOnCircle(), Vector3.Zero);
            }

            // Create one smoke particle per frmae, too.
            smokePlumeParticles.AddParticle(RandomPointOnCircle(), Vector3.Zero);
        }
        /// <summary>
        /// Helper used by the UpdateFire method. Chooses a random location
        /// around a circle, at which a fire particle will be created.
        /// </summary>
        Vector3 RandomPointOnCircle()
        {
            const float radius = 30;
            const float height = 40;

            double angle = random.NextDouble() * Math.PI * 2;

            float x = (float)Math.Cos(angle);
            float y = (float)Math.Sin(angle);

            return new Vector3(x * radius, y * radius + height, 0);
        }








        float vibrationAmount = 0.0f;

        void UpdateInput()
        {
            // Get the current gamepad state.
            /*GamePadState currentState = GamePad.GetState(PlayerIndex.One);

            // Process input only if connected and button A is pressed.
            if (currentState.IsConnected && currentState.Buttons.A ==
                ButtonState.Pressed)
            {
                // Button A is currently being pressed; add vibration.
                vibrationAmount =
                    MathHelper.Clamp(vibrationAmount + 0.03f, 0.0f, 1.0f);
                GamePad.SetVibration(PlayerIndex.One,
                    vibrationAmount, vibrationAmount);
            }
            else
            {
                // Button A is not being pressed; subtract some vibration.
                vibrationAmount =
                    MathHelper.Clamp(vibrationAmount - 0.05f, 0.0f, 1.0f);
                GamePad.SetVibration(PlayerIndex.One,
                    vibrationAmount, vibrationAmount);
            }*/
        }



        protected void Draw_Entry(SpriteBatch sbBatch, int _Step )
        {
            int Btn_Index;
            return; 
/*
            for (int i = 0; i < 3 ; i++)
            {
                int HLX = (int)(Item_StartPos.X) + (int)(  84 * i);
                int HLY = (int)(Item_StartPos.Y);


                if ((i == 2) && (State.RulerType == 1))
                {
                        Btn_Index = naSheet.GetIndex("NA_B01") + (3 << 1);
                }
                else
                    Btn_Index = naSheet.GetIndex("NA_B01") + (i << 1);

                if (State.OperationMode == 0)
                {
                    int _n;
                    if (State.PickedObj == -1)
                        _n = 0;
                    else
                    // _n = State.PeakedObj + 1;
                    {
                        if (State.PickedObj == 0)
                            _n = 1;
                        else
                            _n = 2;
                    }


                    if (_n == i) // Highlight
                    {
                        Btn_Index += 1;
                        float _r;
                        _r = (76f / 128f);
                        float _Factor = _r + (1.0f - _r) * _Step / 10.0f;
                        sbBatch.Draw(naSheet.Texture, new Vector2(HLX, HLY),
                                       naSheet.SourceRectangle(Btn_Index),
                                       Color.White,
                                       0f, new Vector2(0, 0), _Factor, SpriteEffects.None, 0);
                    }
                    else
                    {
                        sbBatch.Draw(naSheet.Texture, new Vector2(HLX, HLY),
                                       naSheet.SourceRectangle(Btn_Index),
                                       Color.White,
                                       0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0);
                    }


                }

                else
                {

                    if ((int)State.CurrNaviState == i) // Highlight
                    {
                        Btn_Index += 1;
                        float _r;
                        _r = (76f / 128f);
                        float _Factor = _r + (1.0f - _r) * _Step / 10.0f;
                        sbBatch.Draw(naSheet.Texture, new Vector2(HLX, HLY),
                                       naSheet.SourceRectangle(Btn_Index),
                                       Color.White,
                                       0f, new Vector2(0, 0), _Factor, SpriteEffects.None, 0);
                    }
                    else
                    {
                        sbBatch.Draw(naSheet.Texture, new Vector2(HLX, HLY),
                                       naSheet.SourceRectangle(Btn_Index),
                                       Color.White,
                                       0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0);
                    }

                }
            }
*/
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// 

        protected override void Draw(GameTime gameTime)
        {
            string TmpStr;
            GraphicsDevice.Clear(Color.CornflowerBlue);
            // graphics.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

            // TODO: Add your drawing code here




            base.Draw(gameTime);


            if (State.CurrState == SystemState.S_State.STATE_MENU_WAIT_START)
            {
                // string TmpStr;
                spriteBatch.Begin();
                spriteBatch.Draw(TitleScreen, new Vector2(0, 0),
                               new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                               Color.White,
                               0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0);

                // Beta Release 
                if (ReleaseStr != "")
                {
                    spriteBatch.DrawString(spriteFont, ReleaseStr,
                                           new Vector2(1078, 183), Color.Gray);
                    spriteBatch.DrawString(spriteFont, ReleaseStr,
                                           new Vector2(1082, 183), Color.Gray);
                    spriteBatch.DrawString(spriteFont, ReleaseStr,
                                           new Vector2(1082, 187), Color.Black);
                    spriteBatch.DrawString(spriteFont, ReleaseStr,
                                           new Vector2(1080, 185), Color.DarkOrange);
                }
                int glowIndex = spriteSheet.GetIndex("glow1");
                if ( (int)(float)gameTime.TotalGameTime.TotalSeconds  % 3 == 0)
                {
                    glowIndex += (int)((float)gameTime.TotalGameTime.TotalSeconds * 20) % 7;
                    spriteBatch.Draw(spriteSheet.Texture, new Rectangle(735, 465, 200, 200),
                                     spriteSheet.SourceRectangle(glowIndex), Color.White);
                }
                else
                    glowIndex = 0;

                spriteBatch.End();
            }
            else if (State.CurrState == SystemState.S_State.STATE_NAVIGATION)
            {


            int _n;
            float _dist;
            if (State.PickedObj == 0)
            {
                ShowInsCenterLine( gameTime.TotalGameTime.Seconds & 0x01 );
            }
            else
            {


                bool _ifObj = ClosetObject(1, out _n, out _dist);
                if ((_ifObj) && (_dist < 40))
                {
                    if (State.PickedObj == -1)
                    {
                        
                        ShowRectangle();

                        //if (State.Ready2PickObj == -1)
                        {
                            State.Ready2PickObj = _n;
                            switch (State.Ready2PickObj)
                            {
                                case 0:
                                    _MainScreen.SetInstrumentWithName(@"TotalStation_1"); // 這一行還是需要, 因為要轉 Camera時參考到儀器
                                    // skc, no_ghmenu2
                                    float _d = Vector3.Distance(_MainScreen._currentCamera.Position, new Vector3(State.Ins_Position.X, _MainScreen.TotalStationCameraHight, State.Ins_Position.Z)) ; 
                                    if ( _d < 5 )
                                    {
                                        _MainScreen.ChangeCamera(1);
                                        _MainScreen._currentCamera.ZoomRestore(1);
                                        State.CurrState = SystemState.S_State.STATE_MENU_SURVEYING;

                                    }
                                    break;
                                case 1:
                                case 2:
                                case 3:
                                case 4:
                                    // if (State.RulerType == 0)
                                        _MainScreen.SetInstrumentWithName(@"Ruler_" + State.Ready2PickObj.ToString());
                                   //  else
                                   //     _MainScreen.SetInstrumentWithName(@"Pole_" + State.Ready2PickObj.ToString());
                                    break;

                                case 21:
                                case 22:
                                case 23:
                                case 24:
                                         _MainScreen.SetInstrumentWithName(@"Pole_" + ( State.Ready2PickObj - 20 ).ToString());
                                         break;
                                case 101:
                                case 102:
                                case 103:
                                case 104:
                                    _MainScreen.SetInstrumentWithName(@"Mirror_" + (State.Ready2PickObj - 100).ToString());
                                    break;
                                case 121:
                                case 122:
                                case 123:
                                case 124:
                                    _MainScreen.SetInstrumentWithName(@"RulerPad_" + (State.Ready2PickObj - 120).ToString());
                                    break;
                                case 140:
                                case 141:
                                case 142:
                                case 143:
                                case 144:
                                    _MainScreen.SetInstrumentWithName(@"SetPoint_" + (State.Ready2PickObj - 140).ToString());
                                    break;

                            }
                        }

                    }
                }
                else
                    State.Ready2PickObj = -1;


            }
                spriteBatch.Begin();
                Draw_Entry(spriteBatch, NaviBtn_State );
                if ( NaviBtn_State < 10 )
                     NaviBtn_State += 1;
// Radar
                if (!State.RadarOff)
                {

                    float _fangle = (float)Math.Atan(_MainScreen._currentCamera.Direction.X / _MainScreen._currentCamera.Direction.Z);
                    if (_MainScreen._currentCamera.Direction.Z > 0)
                        _fangle += MathHelper.Pi;
                    radar.Draw(gameTime, 0, spriteBatch, _fangle, _MainScreen._currentCamera.Position, ref Enemies);

                    if ((State.Ready2PickObj != -1) || (State.PickedObj != -1))
                    {
                        Color _c;
                        int _np;
                        TmpStr = "";
                        if (State.Ready2PickObj != -1)
                        {
                            _np = State.Ready2PickObj;
                            _c = Color.Black;
                            TmpStr += "TARGET : ";
                        }
                        else
                        {
                            _np = State.PickedObj;
                            _c = Color.DarkOrange;
                            TmpStr += "MOVE : ";
                        }

                        switch (_np)
                        {
                            case 0:
                                TmpStr += @"INSTRUMENT";
                                break;
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                                // if (State.RulerType == 0)
                                TmpStr += @"RULER " + _np.ToString();
                                // else
                                //     TmpStr += @"POLE " + _np.ToString();
                                break;
                            case 21:
                            case 22:
                            case 23:
                            case 24:
                                TmpStr += @"POLE " + (_np - 20).ToString();
                                break;
                            case 101:
                            case 102:
                            case 103:
                            case 104:
                                TmpStr += @"MIRROR " + (_np - 100).ToString();
                                break;
                            case 121:
                            case 122:
                            case 123:
                            case 124:
                                TmpStr += @"RULER PAD " + (_np - 120).ToString();
                                break;
                            case 140:
                            case 141:
                            case 142:
                            case 143:
                            case 144:
                                TmpStr += @"POINT " + (_np - 140).ToString();
                                break;

                        }

                        spriteBatch.DrawString(spriteFontSmall, TmpStr,
                                                               new Vector2(1000, 400), _c);



                    }


                }
                //spriteBatch.Draw(H0001, new Vector2(50, 20),
                //               new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                //               Color.White,
                //               0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0);



                //spriteBatch.Draw(AnglePointerN, new Vector2(1150, 250),
                //               new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                //               Color.White,
                //               _fangle - MathHelper.PiOver2, new Vector2(0, 0), 1f, SpriteEffects.None, 0);


                //spriteBatch.Draw(Dir_1, new Vector2(1150, 250),
                //               new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                //               Color.White,
                //               _fangle  - MathHelper.PiOver2, new Vector2(0, 0), 1f, SpriteEffects.None, 0);
                //spriteBatch.Draw(Dir_2, new Vector2(1150, 250),
                //               new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                //               Color.White,
                //               _fangle , new Vector2(0, 0), 1f, SpriteEffects.None, 0);
                //spriteBatch.Draw(Dir_3, new Vector2(1150, 250),
                //               new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                //               Color.White,
                //               _fangle  + MathHelper.PiOver2, new Vector2(0, 0), 1f, SpriteEffects.None, 0);
                //spriteBatch.Draw(Dir_4, new Vector2(1150, 250),
                //               new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                //               Color.White,
                //               _fangle  +  MathHelper.Pi, new Vector2(0, 0), 1f, SpriteEffects.None, 0);


// Particle




                if (!Particle_Off && ParticleEnabled)
                {
                    explosionParticles.SetCamera(_MainScreen._currentCamera.View, _MainScreen._currentCamera.Projection);
                    explosionSmokeParticles.SetCamera(_MainScreen._currentCamera.View, _MainScreen._currentCamera.Projection);
                    projectileTrailParticles.SetCamera(_MainScreen._currentCamera.View, _MainScreen._currentCamera.Projection);
                    smokePlumeParticles.SetCamera(_MainScreen._currentCamera.View, _MainScreen._currentCamera.Projection);
                    fireParticles.SetCamera(_MainScreen._currentCamera.View, _MainScreen._currentCamera.Projection);
                }




                spriteBatch.End();

            }
            else if (State.CurrState == SystemState.S_State.STATE_MENU_SURVEYING)
            {

                spriteBatch.Begin();


/*

            // The effect is a compiled effect created and compiled elsewhere
            // in the application.
            GraphicsDevice.VertexDeclaration = vertexDeclaration;
            basicEffect.Begin();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                    PrimitiveType.LineList,
                    pointList,
                    0,  // vertex buffer offset to add to each element of the index buffer
                    8,  // number of vertices in pointList
                    lineListIndices,  // the index buffer
                    0,  // first index element to read
                    4   // number of primitives to draw
                );
                GraphicsDevice.RenderState.FillMode = FillMode.Solid;
                pass.End();
            }
            basicEffect.End();



*/



                Vector3 Location = _MainScreen.InstrumentLocation;

                float VA = _MainScreen.TelescopeRotationValue;
                float HA = _MainScreen.TribrachRotationValue;
                float HA_r = -State.RelativeAngleBase % 360; // 設定時參考 _MainScreen.TribrachRotationValue
                if (HA_r < 0) HA_r += 360f; 

                VA = ( 90 + VA ) % 360;  
                HA = -HA % 360 ;
                if (VA < 0) VA += 360f;
                if (HA < 0) HA += 360f;

                float HA_v = HA - HA_r;
                HA_v %= 360;
                if (HA_v < 0) HA_v += 360f;


                if (State.TrainingN == 3) // 導線測量使用單線
                {

                    spriteBatch.Draw(CrossLine_s, new Vector2(0, 0),
                                   new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                                   Color.White,
                                   0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0);
                    if (VA <= 180)
                    {

                        spriteBatch.Draw(SignUp, new Vector2(410, 35),
                                        new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                                        Color.White,
                                        0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0);
                    }
                    else
                    {

                        spriteBatch.Draw(SignDown, new Vector2(410, 35),
                                        new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                                        Color.White,
                                        0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0);
                        TmpStr = System.String.Format("Rev");

                        spriteBatch.DrawString(spriteFont, TmpStr,
                                                               new Vector2(350, 35), Color.Brown);

                    }

                }
                else
                {
                    if (VA <= 180)
                    {
                        spriteBatch.Draw(CrossLine, new Vector2(0, 0),
                                       new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                                       Color.White,
                                       0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0);
                        spriteBatch.Draw(SignUp, new Vector2(410, 35),
                                        new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                                        Color.White,
                                        0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0);
                    }
                    else
                    {
                        spriteBatch.Draw(CrossLineRev, new Vector2(0, 0),
                                       new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                                       Color.White,
                                       0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0);
                        spriteBatch.Draw(SignDown, new Vector2(410, 35),
                                        new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                                        Color.White,
                                        0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0);
                        TmpStr = System.String.Format("Rev");

                        spriteBatch.DrawString(spriteFont, TmpStr,
                                                               new Vector2(350, 35), Color.Brown);

                    }
                }
                int _d0, _d1, _d2;
                float _r;
                if (State.InsType != InstrumentType.Leveling)
                {
                    spriteBatch.Draw(Dash_L, new Vector2(0, 0),
                                       new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                                       Color.White,
                                       0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0);

                    TmpStr = "Vertical";
                    spriteBatch.DrawString(spriteFont, TmpStr,
                                                           new Vector2(80, 330), Color.DimGray);

                    // TmpStr = VA.ToString("0.0");

                    _d0 = (int)VA;
                    _r = VA - (float)_d0;
                    _d1 = (int)(_r * 60f);
                    _r = ((_r * 60f) - (float)_d1) * 60f;
                    _d2 = (int)_r;
                    TmpStr = System.String.Format("{0:d02}.{1:d02}'{2:d02}\"", _d0, _d1, _d2);
                    spriteBatch.DrawString(spriteFont, TmpStr,
                                                           new Vector2(80, 375), Color.DimGray);

                    spriteBatch.Draw(AnglePointer, new Vector2(165, 568),
                                   new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                                   Color.White,
                                   MathHelper.ToRadians(VA - 90), new Vector2(0, 0), 1f, SpriteEffects.None, 0);


                }

                spriteBatch.Draw(Dash_R, new Vector2(0, 0),
                                   new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                                   Color.White,
                                   0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0);

                TmpStr = "Horizontal";
                spriteBatch.DrawString(spriteFont, TmpStr,
                                                       new Vector2(1080, 330), Color.DimGray);

 
                // TmpStr = HA.ToString("0.0");
                _d0 = (int)HA_v;
                _r = HA_v - (float)_d0; 
                _d1 = (int)( _r * 60f );
                _r = ((_r * 60f) - (float)_d1) * 60f; 
                _d2 = (int)_r ;
                TmpStr = System.String.Format("{0:d02}.{1:d02}'{2:d02}\"", _d0, _d1, _d2);

                spriteBatch.DrawString(spriteFont, TmpStr,
                                                       new Vector2(1080, 375), Color.DimGray);

                


                float _s;
                if (HA < 90)
                    _s = (HA / 90f) * 0.3f + 0.8f;
                else if (HA > 270)
                    _s = ((360f - HA)/90f) * 0.3f + 0.8f;
                else
                    _s = 1.0f;

                // Relative 
                spriteBatch.Draw(PointerRed, new Vector2(1133, 550),
                               new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                               Color.White,
                               MathHelper.ToRadians( HA_r - 90 ), new Vector2(0, 0), 1.0f, SpriteEffects.None, 0);


                spriteBatch.Draw(AnglePointer, new Vector2(1133, 550),
                               new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight),
                               Color.White,
                               MathHelper.ToRadians( HA - 90), new Vector2(0, 0), _s, SpriteEffects.None, 0);

/*
                T4.X = (-_p0.Z + 128) * 4;  // N
                T4.Y = (_p0.X + 128) * 4;   // E
                T4.Z = _MainScreen.GetHeight(_p0) * 4;
                T4.W = _MainScreen.TotalStationCameraHight * 4 - T4.Z;   
                */
                TmpStr = "Station" ;
                spriteBatch.DrawString(spriteFont, TmpStr,
                                                       new Vector2(80, 50), Color.DimGray);
                TmpStr = System.String.Format("N:{0:f3}", ( -State.Ins_Position.Z + 128 ) * 4.0f /100.0f);
                spriteBatch.DrawString(spriteFont, TmpStr,
                                                       new Vector2(80, 90), Color.DimGray);
                TmpStr = System.String.Format("E:{0:f3}", (State.Ins_Position.X + 128) * 4.0f/100.0f);
                spriteBatch.DrawString(spriteFont, TmpStr,
                                                       new Vector2(80, 130), Color.DimGray);
                float _h = _MainScreen.GetHeight(State.Ins_Position) * 4.0f/100.0f ;
                TmpStr = System.String.Format("Z:{0:f3}", _h);
                spriteBatch.DrawString(spriteFont, TmpStr,
                                                       new Vector2(80, 170), Color.DimGray);
                TmpStr = System.String.Format("HI:{0:f3}", _MainScreen.TotalStationCameraHight * 4.0f / 100.0f - _h);
                spriteBatch.DrawString(spriteFont, TmpStr,
                                                       new Vector2(80, 210), Color.DimGray);

            int _n;
            float _dist;
            bool _ifObj = ClosetObject(0, out _n, out _dist);

            if (_ifObj)
            {
                if (LastObjState == 0)
                {
                    if (State.DistanceErrorEnable)
                    {
                        Random _Rand = new Random();
                        float _RandN = 0.0f;

                        _RandN = (float)_Rand.Next(10) / 2000f;
                        DistError = -0.0025f + _RandN;
                    }
                    else
                        DistError = 0f;
                }



                if (_n > 100)
                    TmpStr = "Mirror" + (_n - 100).ToString();
                else if (_n > 20)
                {
                    TmpStr = "Pole" + ( _n - 20 ).ToString();
                }
                else
                {
                    // if (State.RulerType == 0)
                    TmpStr = "Ruler" + _n.ToString();
                    // else
                    //    TmpStr = "Pole" + _n.ToString();
                }
                spriteBatch.DrawString(spriteFont, TmpStr,
                                                       new Vector2(1080, 50), Color.DimGray);

                // if (State.TrainingN != 2)
                if ( (( _n > 100 ) && ( _n <= 120 )) && ( State.TrainingN != 2 )) // Mirror, 第三題是經緯儀無距離
                {
                    float _d = (float)((_dist * 4.0f) / 100.0);
                    if (_d > 20)
                        DistError *= 2f; 

                    TmpStr = System.String.Format("D:{0:f3}M", ((_dist * 4.0f) / 100.0) + DistError);
                    spriteBatch.DrawString(spriteFont, TmpStr,
                                                           new Vector2(1080, 90), Color.DimGray);
                }


                ShowRectangle();
                if (_MainScreen._currentCamera.Fov < MathHelper.ToRadians(15))
                    _MainScreen._currentCamera.NearPlane = _dist / 2;
                else
                    _MainScreen._currentCamera.NearPlane = 0f;
                LastObjState = 1;

            }
            else
            {
                _MainScreen._currentCamera.NearPlane = 0f;
                LastObjState = 0;
            }




            if( !Particle_Off && ParticleEnabled )
            {
                explosionParticles.SetCamera(_MainScreen._currentCamera.View, _MainScreen._currentCamera.Projection);
                explosionSmokeParticles.SetCamera(_MainScreen._currentCamera.View, _MainScreen._currentCamera.Projection);
                projectileTrailParticles.SetCamera(_MainScreen._currentCamera.View, _MainScreen._currentCamera.Projection);
                smokePlumeParticles.SetCamera(_MainScreen._currentCamera.View, _MainScreen._currentCamera.Projection);
                fireParticles.SetCamera(_MainScreen._currentCamera.View, _MainScreen._currentCamera.Projection);
            }

            // Radar 畫面黑黑的, 暫不開放

            if (!State.RadarOff)
            {

                float _fangle = (float)Math.Atan(_MainScreen._currentCamera.Direction.X / _MainScreen._currentCamera.Direction.Z);
                if (_MainScreen._currentCamera.Direction.Z > 0)
                    _fangle += MathHelper.Pi;
                radar.Draw(gameTime, 1, spriteBatch, _fangle, _MainScreen._currentCamera.Position, ref Enemies);
            }


                spriteBatch.End();

            } // STATE_MENU_SURVEYING 

        } // Draw 



protected bool ClosetObject(int Toward, out int n, out float dist)
{
    // Toward : 0 = Instrument for Telescope , 1 = Camera for 3D navigation
    // return : 0 = Instrument, 1..100 = Ruler or Pole, 101.. 120 = Mirror , 121.. 140 = RulerPad, 141 ..160 = Setpoint 
                int Tor ; 

                Vector3 _p0 = _MainScreen.GetInstrumentPositionWithName(@"TotalStation_1");
                Vector3 _p_cam = _MainScreen._currentCamera.Position; 
                Vector3[] _p1 = new Vector3[4];
                Vector3[] _p2 = new Vector3[4];
                Vector3[] _p1m = new Vector3[4];
                Vector3[] _p1rp = new Vector3[4];
                Vector3[] _p1sp = new Vector3[4];
                Vector3 _p1sp0 = new Vector3();

                BoundingBox _BoundingBoxR0; 
                BoundingBox[] _BoundingBoxR = new BoundingBox[4];
                BoundingBox[] _BoundingBoxP = new BoundingBox[4];
                BoundingBox[] _BoundingBoxRm = new BoundingBox[4];
                BoundingBox[] _BoundingBoxRrp = new BoundingBox[4];
                BoundingBox[] _BoundingBoxRsp = new BoundingBox[4];
                BoundingBox _BoundingBoxRsp0 = new BoundingBox(); 

                if (Toward == 0)
                    Tor = 10;
                else
                    Tor = 5;
                for (int i = 0; i < State.Ruler_Ns; i++ )
                {
                    // if (State.RulerType == 0)
                    _p1[i] = _MainScreen.GetInstrumentPositionWithName(@"Ruler_" + ( i + 1).ToString());
                    // else

                }

                for (int i = 0; i < State.Pole_Ns; i++)
                {
                    _p2[i] = _MainScreen.GetInstrumentPositionWithName(@"Pole_" + (i + 1).ToString());
                }

        for (int i = 0; i < State.Mirror_Ns; i++)
        {
            _p1m[i] = _MainScreen.GetInstrumentPositionWithName(@"Mirror_" + (i + 1).ToString());
            _BoundingBoxRm[i] = new BoundingBox(_p1m[i] - new Vector3(Tor, 0, Tor), _p1m[i] + new Vector3(Tor, ( 160f ) / 4f, Tor));
        }
        for (int i = 0; i < State.RulerPad_Ns; i++)
        {
            _p1rp[i] = _MainScreen.GetInstrumentPositionWithName(@"RulerPad_" + (i + 1).ToString());
            _BoundingBoxRrp[i] = new BoundingBox(_p1rp[i] - new Vector3(Tor, 0, Tor), _p1rp[i] + new Vector3(Tor, ( 160f ) / 4f, Tor));
        }

        if (State.SetPoint0_Enabled)
        {
            _p1sp0 = _MainScreen.GetInstrumentPositionWithName(@"SetPoint_0" );
            _BoundingBoxRsp0 = new BoundingBox(_p1sp0 - new Vector3(Tor, 0, Tor), _p1sp0 + new Vector3(Tor, (50f) / 4f, Tor));
        }

        for (int i = 0; i < State.SetPoint_Ns; i++) // smaller
        {
            _p1sp[i] = _MainScreen.GetInstrumentPositionWithName(@"SetPoint_" + (i + 1).ToString());
            _BoundingBoxRsp[i] = new BoundingBox(_p1sp[i] - new Vector3(Tor, 0, Tor), _p1sp[i] + new Vector3(Tor, (50f) / 4f, Tor));
        }


        for (int i = 0; i < State.Ruler_Ns; i++)
        {
            float _j = State.getRuler_Length(i);
            // note, this value hasn't been initialized yet at the beginning
            if (_j < 0f)
                _j = 0f;
            _BoundingBoxR[i] = new BoundingBox(_p1[i] - new Vector3(Tor, 0, Tor), _p1[i] + new Vector3(Tor, ( _j + 160f ) / 4f , Tor));
        }


        for (int i = 0; i < State.Pole_Ns; i++)
        {
            float _j = State.getPole_Length(i);
            // note, this value hasn't been initialized yet at the beginning
            if (_j < 0f)
                _j = 0f;
            _BoundingBoxP[i] = new BoundingBox(_p2[i] - new Vector3(Tor, 0, Tor), _p2[i] + new Vector3(Tor, (_j + 160f) / 4f, Tor));
        }

                float[] _ff = new float[4];
                bool[] _in = new bool[4];

                float[] _ff2 = new float[4];
                bool[] _in2 = new bool[4];

                float[] _ffm = new float[4];
                float[] _ffrp = new float[4];
                float[] _ffsp = new float[4];
                float _ffsp0 = 0f;
                bool[] _inm = new bool[4];
                bool[] _inrp = new bool[4];
                bool[] _insp = new bool[4];
                bool _insp0 = false;

                float _min;
                int _idx = -1 ;

                for (int i = 0; i < 4; i++)
                {
                    _ff[i] = 0f;
                    _ff2[i] = 0f;
                    _in[i] = false;
                    _in2[i] = false;
                    _ffm[i] = 0f;
                    _inm[i] = false;
                    _inrp[i] = false;
                    _insp[i] = false;
                }
                _min = 10000f;


            if (Toward == 1)
                {
                _BoundingBoxR0 = new BoundingBox(_p0 - new Vector3(Tor, 0f, Tor), _p0 + new Vector3(Tor, 200f/4f, Tor));
                if (_MainScreen._Ray.Intersects(_BoundingBoxR0) != null)
                {
                    _min = Vector3.Distance(_p_cam, _p0); _idx = 0; 
                }

                }

            for (int i = 0; i < State.Ruler_Ns; i++)
                if (_MainScreen._Ray.Intersects(_BoundingBoxR[i]) != null)
                {
                    if (Toward == 0)
                    {
                        _ff[i] = Vector3.Distance(new Vector3(_p0.X, 0f, _p0.Z), new Vector3(_p1[i].X, 0f, _p1[i].Z)); _in[i] = true;
                    }
                    else
                    {
                        _ff[i] = Vector3.Distance(new Vector3(_p_cam.X, 0f, _p_cam.Z), new Vector3(_p1[i].X, 0f, _p1[i].Z)); _in[i] = true;
                    }
                }

            for (int i = 0; i < State.Pole_Ns; i++)
                if (_MainScreen._Ray.Intersects(_BoundingBoxP[i]) != null)
                {
                    if (Toward == 0)
                    {
                        _ff2[i] = Vector3.Distance(new Vector3(_p0.X, 0f, _p0.Z), new Vector3(_p2[i].X, 0f, _p2[i].Z)); _in2[i] = true;
                    }
                    else
                    {
                        _ff2[i] = Vector3.Distance(new Vector3(_p_cam.X, 0f, _p_cam.Z), new Vector3(_p2[i].X, 0f, _p2[i].Z)); _in2[i] = true;
                    }
                }

            for (int i = 0; i < State.Mirror_Ns; i++)
                if (_MainScreen._Ray.Intersects(_BoundingBoxRm[i]) != null)
                {
                    if (Toward == 0)
                    {
                        _ffm[i] = Vector3.Distance(new Vector3(_p0.X, 0f, _p0.Z), new Vector3(_p1m[i].X, 0f, _p1m[i].Z)); _inm[i] = true;
                    }
                    else
                    {
                        _ffm[i] = Vector3.Distance(new Vector3(_p_cam.X, 0f, _p_cam.Z), new Vector3(_p1m[i].X, 0f, _p1m[i].Z)); _inm[i] = true;
                    }
                }
            if (Toward == 1)
            {
                for (int i = 0; i < State.RulerPad_Ns; i++)
                    if (_MainScreen._Ray.Intersects(_BoundingBoxRrp[i]) != null)
                    {
                            _ffrp[i] = Vector3.Distance(new Vector3(_p_cam.X, 0f, _p_cam.Z), new Vector3(_p1rp[i].X, 0f, _p1rp[i].Z)); _inrp[i] = true;
                    }

                if ( State.SetPoint0_Enabled)
                    if (_MainScreen._Ray.Intersects(_BoundingBoxRsp0) != null)
                    {
                            _ffsp0 = Vector3.Distance(new Vector3(_p_cam.X, 0f, _p_cam.Z), new Vector3(_p1sp0.X, 0f, _p1sp0.Z)); _insp0 = true;
                    }

                for (int i = 0; i < State.SetPoint_Ns; i++)
                    if (_MainScreen._Ray.Intersects(_BoundingBoxRsp[i]) != null)
                    {
                            _ffsp[i] = Vector3.Distance(new Vector3(_p_cam.X, 0f, _p_cam.Z), new Vector3(_p1sp[i].X, 0f, _p1sp[i].Z)); _insp[i] = true;
                    }


            }






            for (int i = 0; i < State.Ruler_Ns; i++)
            {
                if ((_in[i]) && (_ff[i] < _min))
                {
                    _min = _ff[i];
                    _idx = i + 1 ;
                }
            }

            for (int i = 0; i < State.Pole_Ns; i++)
            {
                if ((_in2[i]) && (_ff2[i] < _min))
                {
                    _min = _ff2[i];
                    _idx = 20 + i + 1;
                }
            }

            for (int i = 0; i < State.Mirror_Ns; i++)
            {
                if ((_inm[i]) && (_ffm[i] < _min))
                {
                    _min = _ffm[i];
                    _idx = 100 + i + 1;
                }
            }

            for (int i = 0; i < State.RulerPad_Ns; i++)
            {
                if ((_inrp[i]) && (_ffrp[i] < _min))
                {
                    _min = _ffrp[i];
                    _idx = 120 + i + 1;
                }
            }

            if ( State.SetPoint0_Enabled )
            {
                if ((_insp0) && (_ffsp0 < _min))
                {
                    _min = _ffsp0;
                    _idx = 140 ;
                }
            }

            for (int i = 0; i < State.SetPoint_Ns; i++)
            {
                if ((_insp[i]) && (_ffsp[i] < _min))
                {
                    _min = _ffsp[i];
                    _idx = 140 + i + 1;
                }
            }

            if (_idx != -1)
            {
                n = _idx;
                dist = _min;
                return true;

            }
            else
            {
                n = -1;
                dist = 0f;
                return false;
            }




} // ClosetObject

protected void ShowInsCenterLine( int _color )
{
    VertexBuffer vertexBuffer;
    Color gridColor ; 
    int vertexCount = 7 ; 
    VertexPositionColor[] vertices = new VertexPositionColor[vertexCount*2];
    Vector3 _p0 = _MainScreen.TotalstationCamera.Position ; 
    // Vector3 _p1 = _p0 ;
    // _p1.Y = 0;
    Vector3 _p1 = State.Ins_Position; 
    float _len = 0.5f;
    Vector3 _cross00 = _p1 + new Vector3(-_len, 3f, 0f);
    Vector3 _cross01 = _p1 + new Vector3(_len, 3f, 0f);
    Vector3 _cross10 = _p1 + new Vector3(0f, 3f, -_len);
    Vector3 _cross11 = _p1 + new Vector3(0f, 3f, _len);
    if ( _color == 0 )
        gridColor = new Color(0xa0, 0xf0, 0xf0);
    else
        gridColor = new Color(0x0, 0x0, 0x0);
    // 對準目鏡中心時開放此位移
    //_p0.Z +=2.5f;
    //_p1.Z +=2.5f;
 
    vertices[0] = new VertexPositionColor(_p0, gridColor);
    vertices[1] = new VertexPositionColor(_p1, gridColor);
    vertices[2] = new VertexPositionColor(_cross00, gridColor);
    vertices[3] = new VertexPositionColor(_cross01, gridColor);
    vertices[4] = new VertexPositionColor(_cross10, gridColor);
    vertices[5] = new VertexPositionColor(_cross11, gridColor);
    vertices[6] = new VertexPositionColor(_cross00, gridColor);
    vertices[7] = new VertexPositionColor(_p1, gridColor);
    vertices[8] = new VertexPositionColor(_cross01, gridColor);
    vertices[9] = new VertexPositionColor(_p1, gridColor); ;
    vertices[10] = new VertexPositionColor(_cross10, gridColor);
    vertices[11] = new VertexPositionColor(_p1, gridColor);
    vertices[12] = new VertexPositionColor(_cross11, gridColor);
    vertices[13] = new VertexPositionColor(_p1, gridColor);

    // 這一段應該搬到外面
    vertexBuffer = new VertexBuffer(GraphicsDevice, vertexCount * 2 * 
                                                 VertexPositionColor.SizeInBytes,
                                                 BufferUsage.WriteOnly);
    vertexBuffer.SetData<VertexPositionColor>(vertices);


    GraphicsDevice.VertexDeclaration = vertexDeclaration;
    GraphicsDevice.Vertices[0].SetSource(vertexBuffer, 0,
                                         VertexPositionColor.SizeInBytes);

    basicEffect2.World = worldMatrix; // modify later
    basicEffect2.View = _MainScreen._currentCamera.View;
    basicEffect2.Projection = _MainScreen._currentCamera.Projection;
    basicEffect2.VertexColorEnabled = true;
    basicEffect2.Alpha = 0.6f;
    basicEffect2.Begin();
    foreach (EffectPass pass in basicEffect2.CurrentTechnique.Passes)
    {
        pass.Begin();
        GraphicsDevice.DrawPrimitives(PrimitiveType.LineList, 0, vertexCount); 
        pass.End();
    }
    basicEffect2.End();

}



protected void ShowRectangle()
{
    // The effect is a compiled effect created and compiled elsewhere
    // in the application.
    GraphicsDevice.VertexDeclaration = vertexDeclaration;

    basicEffect.Begin();
    foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
    {
        pass.Begin();
        GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
            PrimitiveType.LineList,
            pointList,
            0,  // vertex buffer offset to add to each element of the index buffer
            points,  // number of vertices in pointList
            lineListIndices,  // the index buffer
            0,  // first index element to read
            8   // number of primitives to draw
        );
        GraphicsDevice.RenderState.FillMode = FillMode.Solid;
        pass.End();
    }
    basicEffect.End();
} // ShowRectangle

// Load the radar component
private void InitRadar()
{
    radar = new Radar(Content, "redDotSmall", "gradient", "yellowDotSmall", "blackDotLarge");
}

// Initialize the player and enemy entities
private void InitEntities()
{
    Enemies = new List<Entity>();

    EnemyIndex = 0;
    //for (int i = 0; i < NumEnemies; i++)
    //{
    //    Entity enemy = new Entity(Content, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f) );
    //    Enemies.Add(enemy);
    //}
 
}


private int AddEntities(Vector3 _p, InstrumentType _InsType )
{
    int _ret; 
    Entity enemy = new Entity(  _p, _InsType );
    Enemies.Add( enemy );
    _ret = EnemyIndex++ ;
    return _ret;
    // 之後參考 Enemies.ElementAtOrDefault(i)
}
private void ResetEntities()
{
    Enemies.Clear();
    EnemyIndex = 0;
}

    } // Game1
} // NameSpace
