using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

using SimuSurvey360.Instruments;
using SimuSurvey360.Screens;

namespace SimuSurvey360
{
    public enum RenderingView
    {
        NavigationView, SurveyingView
    };
    class SceneController
    {


        #region Data Member
        private Arguments _Arguments;
        private LinkedList<Instrument> _Instruments;
        private LinkedList<Instrument> _SurveyingInstruments;//Clone the _Instruments for surveying window
        private int _InstrumentCount;
        private LinkedListNode<Instrument> _SelectedInstrument;

        private LinkedListNode<Instrument> _PtrInstrument;


        private ViewController _ViewController;
        private SurveyingWindowController _SurveyingWindowController;
        private Coordinates _Coordinates;
        private ContentManager _ContentManager;
        //private int _InstrumentPtr;

        GameComponent_Terrain_GetHeight terrain;
        InspectionCamera camera;
        GameComponentBall ball;
        GraphicsDevice Device;

        GameComponent_SkySphere[] skySphere;

        public Camera _TotalstationCamera ;

        List<VisualObject> _visualObjects;
// skc_tank
        Matrix tank_projectionMatrix;
        Model[] tank_terrain0 = new Model[2];
        Model tank_terrain ;
        HeightMapInfo heightMapInfo;
        float TankTerrain_Y; 


        Model SetPoint; 
        Model Box_A, Box_B, Box_C, Box_D;
        SystemState State;

        int _v_cnt = 0 ;




        #endregion

        public SceneController()
        {
            _Arguments = new Arguments();
            _InstrumentCount = 0;
            _SelectedInstrument = null;
            _Instruments = new LinkedList<Instrument>();
            _SurveyingWindowController = new SurveyingWindowController();
            //_InstrumentPtr = 0;
        }
        public SceneController( SystemState _State, Viewport port, Viewport surveyingPort, GraphicsDevice graphicsDevice)
        {
            State = _State;
            Device = graphicsDevice;
            _Arguments = new Arguments();
            _InstrumentCount = 0;
            _SelectedInstrument = null;
            _Instruments = new LinkedList<Instrument>();
            _SurveyingInstruments = new LinkedList<Instrument>();
            _ViewController = new ViewController(port, new Rectangle(0,0,_Arguments.SCENE_SIZE,_Arguments.SCENE_SIZE));
            // _Coordinates = new Coordinates(graphicsDevice,new Rectangle(0,0,_Arguments.SCENE_SIZE,_Arguments.SCENE_SIZE));

            skySphere = new GameComponent_SkySphere[5];
            
            //Surveying Window Init
            _SurveyingWindowController = new SurveyingWindowController(graphicsDevice, surveyingPort, new Rectangle(0, 0, _Arguments.SCENE_SIZE, _Arguments.SCENE_SIZE));


            _visualObjects = new List<VisualObject>();
            _visualObjects.Add(new VisualObject("tree01", @"tree01", Matrix.CreateTranslation(200f, 0f, 20f), 0, null, 0.3f, true));
            _visualObjects.Add(new VisualObject("ntuboy8", @"ntuboy8\ntuboy8", Matrix.CreateRotationY(MathHelper.Pi / 2f) * Matrix.CreateTranslation(-250f, 0f, 0f), 0, null, 0.1f, true));
            _visualObjects.Add(new VisualObject("stone", @"Model_Other\stone\stone", /* Matrix.CreateRotationY(MathHelper.Pi / 2f) * */ Matrix.CreateTranslation(100f, 0f, 0f), 0, null, 0.2f, true));
            _visualObjects.Add(new VisualObject("housestory01", @"Model_Other\housestory\housestory",  Matrix.CreateRotationY( -MathHelper.Pi / 2f) *  Matrix.CreateTranslation(150f, 0f, -400f), 0, null, 0.005f, true));
            _visualObjects.Add(new VisualObject("housestory02", @"Model_Other\housestory\housestory", Matrix.CreateTranslation(30f, -5f, -600f), 0, null, 0.005f, true));
            //_visualObjects.Add(new VisualObject("sauna", @"Model_Other\sauna\sauna", Matrix.CreateTranslation(0f, 0f, 0f), 0, null, 1f, true)); // 看不到
            _visualObjects.Add(new VisualObject("tree10_1", @"Model_Other\tree10\tree10", Matrix.CreateTranslation(-200f, 0f, -500f), 0, null, 0.06f, true));
            _visualObjects.Add(new VisualObject("tree10_2", @"Model_Other\tree10\tree10", Matrix.CreateRotationY( MathHelper.Pi / 4f ) * Matrix.CreateTranslation(-250f, 0f, -450f), 0, null, 0.05f, true));
            _visualObjects.Add(new VisualObject("tree10_3", @"Model_Other\tree10\tree10", Matrix.CreateRotationY(-MathHelper.Pi / 3f ) * Matrix.CreateTranslation(-300f, 0f, -420f), 0, null, 0.07f, true));
            _visualObjects.Add(new VisualObject("tree10_4", @"Model_Other\tree10\tree10", Matrix.CreateRotationY(-MathHelper.Pi) * Matrix.CreateTranslation(150f, 0f, -500f), 0, null, 0.08f, true)); 
            //_visualObjects.Add(new VisualObject("CSIE_Building", @"CSIE_Building/CSIE_Building", /*Matrix.CreateRotationY(MathHelper.Pi / 2f) * Matrix.CreateTranslation(-250f, 0f, 0f)*/ Matrix.Identity, 0, null, 0.1f, true));


// skc_tank
            TankTerrain_Y = 64f; 
            float aspectRatio = Device.Viewport.Width /
                (float)Device.Viewport.Height;
            tank_projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.ToRadians(45.0f), aspectRatio, 1f, 10000);


            if (_visualObjects.Count > 0)
            {
                foreach (VisualObject v in _visualObjects)
                {
                    v.Initialize();
                }
            }
   
        }
        public float GetHeight(Vector3 _pos )
        {

            //_pos = terrain.GetHeight(_pos);
            //return _pos.Y; 
            float _posY;
            if (heightMapInfo.IsOnHeightmap(_pos))
            {
                float minimumHeight;
                Vector3 normal;
                heightMapInfo.GetHeightAndNormal
                    (_pos, out minimumHeight, out normal);
                _posY = minimumHeight + TankTerrain_Y;
                return _posY;
            }
            else
                return 0f;

        }
        // remove later
        public void ChangeCamera( int CameraN )
        {



        }

        public void ChangeTerrain(Texture2D t_Terrain, Texture2D t_Texture)
        {

            terrain = new GameComponent_Terrain_GetHeight(Device,
                 t_Terrain,
                 t_Texture);
        }

        public void ChangeTankTerrain( int _idx )
        {
            string _s = "terrain" + System.String.Format("{0:d02}", _idx + 1);
            tank_terrain = _ContentManager.Load<Model>(_s);



            heightMapInfo = tank_terrain.Tag as HeightMapInfo;

            foreach (VisualObject v in _visualObjects)
            {

                float _hv = GetHeight(new Vector3(v.World.M41, v.World.M42, v.World.M43));
                v.WorldYOffset = _hv;
                
            }


        }


        public void moveVisualObject(string Name, Matrix _Matrix)
        {
            foreach (VisualObject v in _visualObjects)
            {
                if (v.Name == Name )
                {
                    // v.World = Matrix.CreateTranslation(NewPos);
                    v.World = _Matrix;
                    float _hv = GetHeight(new Vector3(v.World.M41, v.World.M42, v.World.M43));
                    v.WorldYOffset = _hv;
                }
            }
        }

        public void setVisualObject(string Name, bool isVisible )
        {
            foreach (VisualObject v in _visualObjects)
            {
                if (v.Name == Name)
                {
                    v.IsVisible = isVisible; 
                }
            }
        }


        public void LoadContent()
        {


            tank_terrain = _ContentManager.Load<Model>("terrain01");

            heightMapInfo = tank_terrain.Tag as HeightMapInfo;

            if (heightMapInfo == null)
            {
                string message = "The terrain model did not have a HeightMapInfo " +
                    "object attached. Are you sure you are using the " +
                    "TerrainProcessor?";
                throw new InvalidOperationException(message);
            }

            for ( int i = 0 ; i < 5; i++ )
            skySphere[i] = new GameComponent_SkySphere(_ContentManager.Load<Model>("sky" + (i+1).ToString()));

            foreach (VisualObject v in _visualObjects)
            {
                v.LoadContent(_ContentManager);
            }

            foreach (Instrument instrument in _Instruments)
            {
                switch (instrument.Type)
                {
                    case InstrumentType.TotalStation:
                    case InstrumentType.Leveling:
                    case InstrumentType.Theodolite:
                        ((TotalStation)instrument).Load(_ContentManager);
                        break;
                    case InstrumentType.Level:
                        ((Level)instrument).Load(_ContentManager);
                        break;
                    case InstrumentType.Ruler:
                        ((Ruler)instrument).Load(_ContentManager);
                        break;
                    case InstrumentType.Pole:
                        ((Pole)instrument).Load(_ContentManager);
                        break;
                    case InstrumentType.Mirror:
                        ((Mirror)instrument).Load(_ContentManager);
                        break;
                    case InstrumentType.HintBox:
                        ((HintBox)instrument).Load(_ContentManager, 0 );
                        break;
                    case InstrumentType.SetPoint:
                        ((SetPoint)instrument).Load(_ContentManager, 0);
                        break;
                    case InstrumentType.RulerPad:
                        ((RulerPad)instrument).Load(_ContentManager, 0);
                        break;
                }
            }

            // remove later
            foreach (Instrument instrument in _SurveyingInstruments)
            {
                switch (instrument.Type)
                {
                    case InstrumentType.TotalStation:
                        ((TotalStation)instrument).Load(_ContentManager);
                        break;
                    case InstrumentType.Level:
                        ((Level)instrument).Load(_ContentManager);
                        break;
                    case InstrumentType.Ruler:
                        ((Ruler)instrument).Load(_ContentManager);
                        break;
                    case InstrumentType.Pole:
                        ((Pole)instrument).Load(_ContentManager);
                        break;
                    case InstrumentType.Mirror:
                        ((Mirror)instrument).Load(_ContentManager);
                        break;
                    case InstrumentType.HintBox:
                        ((HintBox)instrument).Load(_ContentManager, 0 );
                        break;
                    case InstrumentType.SetPoint:
                        ((SetPoint)instrument).Load(_ContentManager, 0);
                        break;
                    case InstrumentType.RulerPad:
                        ((RulerPad)instrument).Load(_ContentManager, 0);
                        break;
                }
            }
        }

        public void Update(Camera _currentCamera, GameTime gameTime)
        { 
            
                if ( _v_cnt > 0 )
                {
                    _v_cnt -=1 ;
                    if (_v_cnt == 0)
                    GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);           
                }

            // skc remove
            _ViewController.Update();

            //Update Coordinates
            // _Coordinates.Update(_ViewController.World, _ViewController.View, _ViewController.Projection);
            if ( State.SkySphereRotate )
            skySphere[State.SkyIndex].Update( gameTime );
            // In face, it's empty
            // terrain.Update();


            // 調整攝影機高度
            float _hcam = GetHeight(_currentCamera.Position);
            if (_currentCamera.Position.Y < (_hcam + 3))
            {
                _currentCamera.FlatCamera(_hcam + 3);
                GamePad.SetVibration(PlayerIndex.One, 0.5f, 0.5f);
                _v_cnt = 15;
            }
            else if (Vector3.Distance(_currentCamera.Position, Vector3.Zero) > 700)
            {

                
                Vector3 _v = _currentCamera.Position;
                _v.Normalize();
                _currentCamera.Position = 700f * _v;
                GamePad.SetVibration(PlayerIndex.One, 0.5f, 0.5f);
                _v_cnt = 15;

            }


            if (State.PickedObj != -1)
            {
                if (_currentCamera.Position.Y != (_hcam + State.Ins_H0))
                {
                    _currentCamera.Position = new Vector3(_currentCamera.Position.X, _hcam + State.Ins_H0, _currentCamera.Position.Z);
                    _currentCamera.Target = _currentCamera.Position + _currentCamera.Direction; 
                }
            }
 

            //Syncronization with Surveying Window
            if (_SelectedInstrument != null)
                _SurveyingWindowController.InstrumentsSyncronization(_SelectedInstrument);

            // 做一次就可以, 換新的 Terrain 再做一次
            //foreach (VisualObject v in _visualObjects)
            //{

            //    float _hv = GetHeight( new Vector3( v.World.M41,v.World.M42, v.World.M43 ) );
            //    v.WorldYOffset = _hv; 
            //    v.Update(gameTime);
            //}



            // Update Surveying Window
            // _SurveyingWindowController.Update();

            //Update Instruments
            AdjustTerrainHeight( _currentCamera );


  }


        public void AdjustTerrainHeight(Camera _currentCamera)
        {
            foreach (Instrument instrument in _Instruments)
            {
                // Terrain 
                Vector3 _pos = new Vector3(instrument.WorldPosition.X, 0f, instrument.WorldPosition.Z);
                float _h = GetHeight(_pos);
                switch (instrument.Type)
                {
                    case InstrumentType.TotalStation:
                    case InstrumentType.Leveling:
                    case InstrumentType.Theodolite:
                        if (instrument.Type == State.InsType)
                        {
                            ((TotalStation)instrument).WorldYOffset = _h;
                            ((TotalStation)instrument).Update(_ViewController.World, _currentCamera.View, _currentCamera.Projection);
                        }
                        break;
                    case InstrumentType.Level: // remove later 
                        ((Level)instrument).Update(_ViewController.World, _currentCamera.View, _currentCamera.Projection);
                        break;
                    case InstrumentType.Ruler:
                        ((Ruler)instrument).WorldYOffset = _h;
                        ((Ruler)instrument).Update(_ViewController.World, _currentCamera.View, _currentCamera.Projection);
                        break;
                    case InstrumentType.Pole:
                        ((Pole)instrument).WorldYOffset = _h;
                        ((Pole)instrument).Update(_ViewController.World, _currentCamera.View, _currentCamera.Projection);
                        break;
                    case InstrumentType.Mirror:
                        ((Mirror)instrument).WorldYOffset = _h;
                        ((Mirror)instrument).Update(_ViewController.World, _currentCamera.View, _currentCamera.Projection);
                        break;
                    case InstrumentType.HintBox:
                        ((HintBox)instrument).WorldYOffset = _h;
                        ((HintBox)instrument).Update(_ViewController.World, _currentCamera.View, _currentCamera.Projection);
                        break;
                    case InstrumentType.SetPoint:
                        ((SetPoint)instrument).WorldYOffset = _h;
                        ((SetPoint)instrument).Update(_ViewController.World, _currentCamera.View, _currentCamera.Projection);
                        break;
                    case InstrumentType.RulerPad:
                        ((RulerPad)instrument).WorldYOffset = _h;
                        ((RulerPad)instrument).Update(_ViewController.World, _currentCamera.View, _currentCamera.Projection);
                        break;
                }
            }

        }

        public void Draw3D( Camera _currentCamera, GameTime gameTime, RenderingView option)
        {
             skySphere[State.SkyIndex].View = _currentCamera.View; // _ViewController.View;
             skySphere[State.SkyIndex].Projection = _currentCamera.Projection;  //_ViewController.Projection;

             // terrain.view = _currentCamera.View; //_ViewController.View;
             // terrain.projection = _currentCamera.Projection; // _ViewController.Projection;

            foreach (VisualObject v in _visualObjects)
            {
                if ( v.Name == "tree01" )  
                v.Draw(gameTime, _currentCamera, new Vector3(0.7f, 0.7f, 0.5f), 0.7f);
                else
                v.Draw(gameTime, _currentCamera, new Vector3(0.7f, 0.7f, 0.5f), 1.0f);
            }
            // 第二個場景開放 LensFlare, 所以先關掉背景
            // if ( State.SkyIndex != 1 ) 

            if ( State.Background_N == 0 )
              skySphere[State.SkyIndex].Draw();



            // terrain.Draw();
// skc_tank
            DrawModel(tank_terrain, _currentCamera /* .View */);

            int _n;

            switch(option)
            {
                case RenderingView.NavigationView:
                    // _Coordinates.Draw();
                    foreach (Instrument instrument in _Instruments)
                    {
                        switch (instrument.Type)
                        {
                            case InstrumentType.TotalStation:
                            case InstrumentType.Leveling:
                            case InstrumentType.Theodolite:
                                if (instrument.Type == State.InsType )
                                    ((TotalStation)instrument).Draw();
                                break;
                            case InstrumentType.Level:
                                ((Level)instrument).Draw();
                                break;
                            case InstrumentType.Ruler:
                                //if (State.RulerType == 0)
                                //{
                                    _n = Convert.ToInt32 ( instrument.Name.Substring(instrument.Name.Length - 1, 1) );
                                    if ( _n <= State.Ruler_Ns )
                                    ((Ruler)instrument).Draw();
                                //}
                                break;
                            case InstrumentType.Pole:
                                //if (State.RulerType == 1)
                                //{
                                    _n = Convert.ToInt32(instrument.Name.Substring(instrument.Name.Length - 1, 1));
                                    if (_n <= State.Pole_Ns)
                                    ((Pole)instrument).Draw();
                                //}
                                break;
                            case InstrumentType.Mirror:
                                if (State.Mirror_Ns > 0)
                                {
                                    _n = Convert.ToInt32(instrument.Name.Substring(instrument.Name.Length - 1, 1));
                                    if (_n <= State.Mirror_Ns)
                                        ((Mirror)instrument).Draw();
                                }
                                break;
                            case InstrumentType.HintBox:
                                _n = Convert.ToInt32(instrument.Name.Substring(instrument.Name.Length - 1, 1));
                                if ((_n == 0 ) && ( State.SetPoint0_Enabled))
                                    ((HintBox)instrument).Draw();
                                else if ((_n > 0 )&& (_n <= State.SetPoint_Ns))
                                ((HintBox)instrument).Draw();
                                break;
                            case InstrumentType.SetPoint:
                                _n = Convert.ToInt32(instrument.Name.Substring(instrument.Name.Length - 1, 1));
                                if ((_n == 0) && (State.SetPoint0_Enabled))
                                    ((SetPoint)instrument).Draw();
                                else if ((_n > 0) && (_n <= State.SetPoint_Ns))
                                ((SetPoint)instrument).Draw();
                                break;
                            case InstrumentType.RulerPad:
                                _n = Convert.ToInt32(instrument.Name.Substring(instrument.Name.Length - 1, 1));

                                if ((_n > 0) && (_n <= State.RulerPad_Ns))
                                    ((RulerPad)instrument).Draw();
                                break;
                        }
                    }
                    break;
                case RenderingView.SurveyingView:
                    _SurveyingWindowController.Draw3D();
                    break;
        }
        }


        void DrawModel(Model model, Camera  _currentCamera /* Matrix viewMatrix */ )
        {
            Matrix[] boneTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index] * Matrix.CreateTranslation( 0f, TankTerrain_Y , 0f);
                    effect.View = _currentCamera.View;
                    effect.Projection = _currentCamera.Projection; 


                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;

                    // Set the fog to match the black background color
                    effect.FogEnabled = true;
                    effect.FogColor = Vector3.Zero;
                    effect.FogStart = 2000;
                    effect.FogEnd = 4000;

// 從 Lens Flare copy 過來試試看, 造成地上塊狀反光

                    effect.LightingEnabled = true;
                    effect.DiffuseColor = new Vector3(1f);
                    effect.AmbientLightColor = new Vector3(0.5f);


                    if ( State.Background_N == 0 )
                        effect.DirectionalLight0.Enabled = false;
                    else
                        effect.DirectionalLight0.Enabled = true;
                    
                    
                    effect.DirectionalLight0.DiffuseColor = Vector3.One;


                    // effect.DirectionalLight0.Direction = lensFlare.LightDirection;
                    effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-100, -10f, -80f));
                    effect.FogEnabled = true;
                    effect.FogStart = 1000;
                    effect.FogEnd = 4000;
                    effect.FogColor = Color.CornflowerBlue.ToVector3();




                }

                mesh.Draw();
            }
        }



        public void Draw2D(SpriteBatch spriteBatch,SpriteFont font)
        {
            return;
            spriteBatch.Begin();

            spriteBatch.DrawString(font, SelectedInstrument.Name, new Vector2(0,0), Color.WhiteSmoke);

            spriteBatch.End();
        }
        public void Draw2D(SpriteBatch spriteBatch, SpriteFont font, Vector2 position)
        {
            return;
            spriteBatch.Begin();

            spriteBatch.DrawString(font, SelectedInstrument.Name, position, Color.Silver);

            spriteBatch.End();
        }
        public void Draw2DInfo(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, MainScreen.TripodControlOption option)//Draw the information in the subdisplay area
        {
            // skc
            return;
            if (_SelectedInstrument.Value.Type == InstrumentType.TotalStation || _SelectedInstrument.Value.Type == InstrumentType.Level)
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(font,option.ToString(), position, Color.Silver);
                spriteBatch.End();
            }
        }
        public void Draw2DInfo(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, MainScreen.UpperBodyControlOption option)//Draw the information in the subdisplay area
        {
            // skc
            return;
            if (_SelectedInstrument.Value.Type == InstrumentType.TotalStation || _SelectedInstrument.Value.Type == InstrumentType.Level)
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(font, option.ToString(), position, Color.Silver);
                spriteBatch.End();
            }
        }
        public void Draw2DSurveyingWindow(SpriteBatch spriteBatch)
        {
            _SurveyingWindowController.DrawCorssLine(spriteBatch);
        }
        #region Instrument
        public enum SetInstrumentOption
        {
            IncreaseTranslation, SetTranslation, SetRotationY, ObjLength, TripodRotation, UpperBodyRotation, TelescopeRotation, Attach2Camera
        };

        public InstrumentArgs WarpInstrment()
        {
            InstrumentArgs args = null;
            switch (_SelectedInstrument.Value.Type)
            {
                case InstrumentType.Ruler:
                    args = (InstrumentArgs)((Ruler)_SelectedInstrument.Value).WarpInstrment();
                    break;
                case InstrumentType.Pole:
                    args = (InstrumentArgs)((Pole)_SelectedInstrument.Value).WarpInstrment();
                    break;
                case InstrumentType.Mirror:
                    args = (InstrumentArgs)((Mirror)_SelectedInstrument.Value).WarpInstrment();
                    break;
                case InstrumentType.TotalStation:
                case InstrumentType.Leveling:
                case InstrumentType.Theodolite:
                    args = (InstrumentArgs)((TotalStation)_SelectedInstrument.Value).WarpInstrment();
                    break;
            }
            return args;
        }
        public void ChangeInstrument(bool isForwards)
        {
            //Change to the current instrument
           /* for (int i = 0; i < _InstrumentPtr; i++)
                _SelectedInstrument = _Instruments.First.Next;*/
            if (isForwards)
            {
                /*_InstrumentPtr++;
                _InstrumentPtr = ((_InstrumentPtr) % _Instruments.Count);*/

                if (_SelectedInstrument != _Instruments.Last)
                    _SelectedInstrument = _SelectedInstrument.Next;
                else
                    _SelectedInstrument = _Instruments.First;
            }
            else
            {
               /* _InstrumentPtr--;
                if (_InstrumentPtr < 0)
                    _InstrumentPtr = _Instruments.Count-1;*/

                if (_SelectedInstrument != _Instruments.First)
                    _SelectedInstrument = _SelectedInstrument.Previous;
                else
                    _SelectedInstrument = _Instruments.Last;
            }
        }

// skc 
        public void SetActiveInstrument(int n)
        {
                if ( n >=_Instruments.Count )
                    n = _Instruments.Count - 1;

                _SelectedInstrument = _Instruments.First;
                for ( int i = 0 ; i < n ; i++ )
                    _SelectedInstrument = _SelectedInstrument.Next;

        }

        public void BeginAddInstrument(ContentManager contentManager)
        {
            _ContentManager = contentManager;
        }
        public void EndAddInstrument()
        {
            LoadContent();
            _SurveyingWindowController.InstrumentsSyncronization(_SurveyingInstruments);
            _SurveyingWindowController.LoadContent(_ContentManager);
        }
        public void AddInstrument(Instruments.InstrumentType type, int n)
        {
            switch(type)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
            {
                case InstrumentType.TotalStation:
                    _Instruments.AddLast(new LinkedListNode<Instrument>(new TotalStation(InstrumentType.TotalStation)));
                    _Instruments.Last.Value.Name = _Instruments.Last.Value.Type.ToString() +  "_" +  n.ToString() ;
                    _SurveyingInstruments.AddLast(new LinkedListNode<Instrument>(new TotalStation(InstrumentType.TotalStation)));
                    break;
                case InstrumentType.Leveling:
                    _Instruments.AddLast(new LinkedListNode<Instrument>(new TotalStation(InstrumentType.Leveling)));
                    _Instruments.Last.Value.Name = _Instruments.Last.Value.Type.ToString() + "_" + n.ToString();
                    _SurveyingInstruments.AddLast(new LinkedListNode<Instrument>(new TotalStation(InstrumentType.Leveling)));
                    break;
                case InstrumentType.Theodolite:
                    _Instruments.AddLast(new LinkedListNode<Instrument>(new TotalStation(InstrumentType.Theodolite)));
                    _Instruments.Last.Value.Name = _Instruments.Last.Value.Type.ToString() + "_" + n.ToString();
                    _SurveyingInstruments.AddLast(new LinkedListNode<Instrument>(new TotalStation(InstrumentType.Theodolite)));
                    break;
                case InstrumentType.Level:
                    _Instruments.AddLast(new LinkedListNode<Instrument>(new Level(InstrumentType.Level)));
                    _Instruments.Last.Value.Name = _Instruments.Last.Value.Type.ToString() + "_" + /* _Instruments.Count.ToString()*/ n.ToString();
                    _SurveyingInstruments.AddLast(new LinkedListNode<Instrument>(new Level(InstrumentType.Level)));
                    break;
                case InstrumentType.Ruler:
                    _Instruments.AddLast(new LinkedListNode<Instrument>(new Ruler(InstrumentType.Ruler)));
                    _Instruments.Last.Value.Name = _Instruments.Last.Value.Type.ToString() + "_" + /* _Instruments.Count.ToString() */ n.ToString() ;
                    _SurveyingInstruments.AddLast(new LinkedListNode<Instrument>(new Ruler(InstrumentType.Ruler)));
                    break;
                case InstrumentType.Pole:
                    _Instruments.AddLast(new LinkedListNode<Instrument>(new Pole(InstrumentType.Pole)));
                    // _Instruments.Last.Value.Name = _Instruments.Last.Value.Type.ToString() + "_" + _Instruments.Count.ToString();
                    // 暫時先用 Ruler 當名字
                    _Instruments.Last.Value.Name = "Pole" + "_" + /* _Instruments.Count.ToString() */ n.ToString();
                    _SurveyingInstruments.AddLast(new LinkedListNode<Instrument>(new Pole(InstrumentType.Pole)));
                    break;
                case InstrumentType.Mirror:
                    _Instruments.AddLast(new LinkedListNode<Instrument>(new Mirror(InstrumentType.Mirror)));
                    _Instruments.Last.Value.Name = _Instruments.Last.Value.Type.ToString() + "_" +  n.ToString();
                    _SurveyingInstruments.AddLast(new LinkedListNode<Instrument>(new Mirror(InstrumentType.Mirror)));
                    break;
                case InstrumentType.HintBox:
                    _Instruments.AddLast(new LinkedListNode<Instrument>(new HintBox(InstrumentType.HintBox)));
                    _Instruments.Last.Value.Name = "HintBox" + "_" + /* _Instruments.Count.ToString() */ n.ToString();
                    _SurveyingInstruments.AddLast(new LinkedListNode<Instrument>(new HintBox(InstrumentType.HintBox)));
                    _SurveyingInstruments.Last.Value.Name = "HintBox" + "_" +  n.ToString();
                    break;
                case InstrumentType.SetPoint:
                    _Instruments.AddLast(new LinkedListNode<Instrument>(new SetPoint(InstrumentType.SetPoint)));
                    _Instruments.Last.Value.Name = "SetPoint" + "_" + n.ToString();
                    _SurveyingInstruments.AddLast(new LinkedListNode<Instrument>(new SetPoint(InstrumentType.SetPoint)));

                    break;
                case InstrumentType.RulerPad:
                    _Instruments.AddLast(new LinkedListNode<Instrument>(new RulerPad(InstrumentType.RulerPad)));
                    _Instruments.Last.Value.Name = "RulerPad" + "_" + n.ToString();
                    _SurveyingInstruments.AddLast(new LinkedListNode<Instrument>(new RulerPad(InstrumentType.RulerPad)));

                    break;

            }
            _SelectedInstrument = _Instruments.Last ;
            //_InstrumentPtr = _Instruments.Count-1;
        }

        public void RemoveInstrument()
        {
            /*if (index < 0 || index > _InstrumentCount)
                return;*/
            if (_SelectedInstrument != null)
                _Instruments.Remove(_SelectedInstrument);
        }


        public Vector3 GetInstrumentPositionWithName( String name)
        {
            if (name == "TotalStation_1")
            {
                if (State.InsType == InstrumentType.Leveling)
                    name = "Leveling_1";
                else if (State.InsType == InstrumentType.Theodolite)
                    name = "Theodolite_1";
            }

            LinkedListNode<Instrument> ptr = _Instruments.First;
            do
            {
                if (ptr.Value.Name == name)
                {
                    // _SelectedInstrument = ptr;
                    return ptr.Value.WorldPosition + new Vector3( 0f, ptr.Value.WorldYOffset, 0f) ;
                }
                else
                    ptr = ptr.Next;
            } while (ptr != null);
            return Vector3.Zero;
        }

        public void SelectInstrumentWithName(String name)
        {
            bool SetCamera = false;
            if (name == "TotalStation_1")
            {
                if (State.InsType == InstrumentType.Leveling)
                    name = "Leveling_1";
                else if (State.InsType == InstrumentType.Theodolite)
                    name = "Theodolite_1";
                SetCamera = true; 
            }
            LinkedListNode<Instrument> ptr = _Instruments.First;
            do
            {
                if (ptr.Value.Name == name)
                {
                    _SelectedInstrument = ptr;

                    if (SetCamera)
                    {
                        _TotalstationCamera = ((TotalStation)_SelectedInstrument.Value)._Camera;
                        _PtrInstrument = _SelectedInstrument; 
                    }
                    return;
                }
                else
                    ptr = ptr.Next;
            } while (ptr != null);

        }

        public Instrument PtrInstrument
        {
            get { return _PtrInstrument.Value; }
        }

        //== Set  and Get properties for selected instrument==
        public void SetInstrument(Camera _Camera, SetInstrumentOption option, Object obj)
        {
            float value;
            if (_ViewController== null || SelectedInstrument == null)
                return;

            switch (SelectedInstrument.Type)
            {
                case InstrumentType.Level:
                case InstrumentType.TotalStation:
                case InstrumentType.Leveling:
                case InstrumentType.Theodolite:
                    TwoPartsInstrument instrument = (TwoPartsInstrument)SelectedInstrument;
                    switch (option)
                    {
                        case SetInstrumentOption.IncreaseTranslation://Increase Global Translation
                            Vector3 translation = (Vector3)obj;
                            instrument.WorldPosition += translation;
                            break;
                        case SetInstrumentOption.SetTranslation://Set Global Translation
                            translation = (Vector3)obj;
                            // 避免儀器在半空中
                            instrument.WorldPosition = new Vector3(translation.X, 0f, translation.Z); ;
                            break;
                        case SetInstrumentOption.SetRotationY://Set Global Rotation
                            value = (float)obj;
                            instrument.ModelRatationYValue = value;
                            break;

                        case SetInstrumentOption.ObjLength://Tripod length
                            value = (float)obj;
                            float OldLength = instrument.TripodLength;
                            instrument.TripodLength += value;
                            if (instrument.TripodLength != OldLength)
                            {
                                GamePad.SetVibration(PlayerIndex.One, 0f, 0.2f);
                                _v_cnt = 3;
                            }
                            break;
                        case SetInstrumentOption.TripodRotation://Tripod Rotation
                            value = (float)obj;
                            instrument.TripodRotationValue += value;
                            break;
                        case SetInstrumentOption.UpperBodyRotation://Increase Upper Part Rotation
                            value = float.Parse(obj.ToString());
                            instrument.TribrachRotationValue += value;

                            // GamePad.SetVibration(PlayerIndex.One, 0f, 0.2f);
                            // _v_cnt = 3;
                            break;
                        case SetInstrumentOption.TelescopeRotation://Increase Telescope Rotation
                            value = float.Parse(obj.ToString());
                            instrument.TelescopeRotationValue += value;
                            break;
                        case SetInstrumentOption.Attach2Camera:
                            Vector3 _v = new Vector3(_Camera.Direction.X, 0f, _Camera.Direction.Z);
                            _v.Normalize();
                            instrument.WorldPosition = new Vector3(_Camera.Position.X, 0f, _Camera.Position.Z) + 20 * _v; 

                            break;
                    }
                    // instrument.Update(_ViewController.World, _ViewController.View, _ViewController.Projection);
                    instrument.Update(Matrix.Identity, _Camera.View, _Camera.Projection);
                    break;
                case InstrumentType.Ruler:
                    Ruler ruler = (Ruler)SelectedInstrument;
                    switch (option)
                    {
                        case SetInstrumentOption.IncreaseTranslation://Increase Global Translation
                            Vector3 translation = (Vector3)obj;
                            ruler.WorldPosition += translation;
                            break;
                        case SetInstrumentOption.SetTranslation://Set Global Translation
                            translation = (Vector3)obj;
                            ruler.WorldPosition = translation;
                            break;
                        case SetInstrumentOption.SetRotationY://Set Global Rotation
                            value = (float)obj;
                            ruler.ModelRatationYValue = value;
                            break;

                        case SetInstrumentOption.ObjLength:
                            value = (float)obj;
                            // instrument.TripodLength += value;

                            ruler.RulerLength = value;
                            int _i = Convert.ToInt32(SelectedInstrument.Name.Substring(SelectedInstrument.Name.Length - 1, 1))-1;
                            if ( _i >= 0 )
                            State.setRuler_Length(_i, ruler.RulerLength);
                            break;
                        case SetInstrumentOption.Attach2Camera:
                            Vector3 _v = new Vector3(_Camera.Direction.X, 0f, _Camera.Direction.Z);
                            _v.Normalize();
                            ruler.WorldPosition = new Vector3(_Camera.Position.X, 0f, _Camera.Position.Z) + 20 * _v; 

                            break;
                    }
                    // ruler.Update(_ViewController.World, _ViewController.View, _ViewController.Projection);
                    ruler.Update(Matrix.Identity, _Camera.View, _Camera.Projection);
                    break;

                case InstrumentType.Pole:
                    Pole pole = (Pole)SelectedInstrument;
                    switch (option)
                    {
                        case SetInstrumentOption.IncreaseTranslation://Increase Global Translation
                            Vector3 translation = (Vector3)obj;
                            pole.WorldPosition += translation;
                            break;
                        case SetInstrumentOption.SetTranslation://Set Global Translation
                            translation = (Vector3)obj;
                            pole.WorldPosition = translation;
                            break;
                        case SetInstrumentOption.SetRotationY://Set Global Rotation
                            value = (float)obj;
                            pole.ModelRatationYValue = value;
                            break;

                        case SetInstrumentOption.ObjLength:
                            value = (float)obj;
                            // instrument.TripodLength += value;

                            pole.RulerLength = value;
                            int _i = Convert.ToInt32( SelectedInstrument.Name.Substring(SelectedInstrument.Name.Length - 1, 1))-1;
                            if ( _i >=  0 )
                            State.setPole_Length(_i,pole.RulerLength);
                            break;
                        case SetInstrumentOption.Attach2Camera:
                            Vector3 _v = new Vector3(_Camera.Direction.X, 0f, _Camera.Direction.Z);
                            _v.Normalize();
                            pole.WorldPosition = new Vector3(_Camera.Position.X, 0f, _Camera.Position.Z) + 20 * _v; 

                            break;
                    }
                    // ruler.Update(_ViewController.World, _ViewController.View, _ViewController.Projection);
                    pole.Update(Matrix.Identity, _Camera.View, _Camera.Projection);
                    break;
                case InstrumentType.Mirror:
                    Mirror mirror = (Mirror)SelectedInstrument;
                    switch (option)
                    {
                        case SetInstrumentOption.IncreaseTranslation://Increase Global Translation
                            Vector3 translation = (Vector3)obj;
                            mirror.WorldPosition += translation;
                            break;
                        case SetInstrumentOption.SetTranslation://Set Global Translation
                            translation = (Vector3)obj;
                            mirror.WorldPosition = translation;
                            break;
                        case SetInstrumentOption.SetRotationY://Set Global Rotation
                            value = (float)obj;
                            mirror.ModelRatationYValue = value;
                            break;

                        case SetInstrumentOption.ObjLength:
                            value = (float)obj;
                            // instrument.TripodLength += value;
                            float OldLength = mirror.RulerLength;
                            mirror.RulerLength = value;

                            if (mirror.RulerLength != OldLength)
                            {
                                GamePad.SetVibration(PlayerIndex.One, 0f, 0.2f);
                                _v_cnt = 3;
                            }


                            int _i = Convert.ToInt32(SelectedInstrument.Name.Substring(SelectedInstrument.Name.Length - 1, 1)) - 1;
                            if (_i >= 0)
                                State.setMirror_Length(_i, mirror.RulerLength);
                            break;
                        case SetInstrumentOption.Attach2Camera:
                            Vector3 _v = new Vector3(_Camera.Direction.X, 0f, _Camera.Direction.Z);
                            _v.Normalize();
                            mirror.WorldPosition = new Vector3(_Camera.Position.X, 0f, _Camera.Position.Z) + 20 * _v;

                            break;
                    }
                    // ruler.Update(_ViewController.World, _ViewController.View, _ViewController.Projection);
                    mirror.Update(Matrix.Identity, _Camera.View, _Camera.Projection);
                    break;
                case InstrumentType.HintBox:
                    HintBox hintBox = (HintBox)SelectedInstrument;
                    switch (option)
                    {
                        case SetInstrumentOption.IncreaseTranslation://Increase Global Translation
                            Vector3 translation = (Vector3)obj;
                            hintBox.WorldPosition += translation;
                            break;
                        case SetInstrumentOption.SetTranslation://Set Global Translation
                            translation = (Vector3)obj;
                            hintBox.WorldPosition = translation;
                            break;
                        case SetInstrumentOption.SetRotationY://Set Global Rotation
                            value = (float)obj;
                            hintBox.ModelRatationYValue = value;
                            break;

                    }
                    // ruler.Update(_ViewController.World, _ViewController.View, _ViewController.Projection);
                    hintBox.Update(Matrix.Identity, _Camera.View, _Camera.Projection);
                    break;
                case InstrumentType.SetPoint:
                    SetPoint SetPoint = (SetPoint)SelectedInstrument;
                    switch (option)
                    {
                        case SetInstrumentOption.IncreaseTranslation://Increase Global Translation
                            Vector3 translation = (Vector3)obj;
                            SetPoint.WorldPosition += translation;
                            break;
                        case SetInstrumentOption.SetTranslation://Set Global Translation
                            translation = (Vector3)obj;
                            SetPoint.WorldPosition = translation;
                            break;
                        case SetInstrumentOption.SetRotationY://Set Global Rotation
                            value = (float)obj;
                            SetPoint.ModelRatationYValue = value;
                            break;
                        case SetInstrumentOption.Attach2Camera:
                            Vector3 _v = new Vector3(_Camera.Direction.X, 0f, _Camera.Direction.Z);
                            _v.Normalize();
                            SetPoint.WorldPosition = new Vector3(_Camera.Position.X, 0f, _Camera.Position.Z) + 20 * _v;
                            int _i = Convert.ToInt32 ( SetPoint.Name.Substring(SetPoint.Name.Length - 1, 1) );
                            float _h = GetHeight(SetPoint.WorldPosition);
                            Vector3 _v3 = new Vector3(  SetPoint.WorldPosition.X, _h, SetPoint.WorldPosition.Z );
                            // State.setSetPoint_Position(_i, SetPoint.WorldPosition);
                            State.setSetPoint_Position(_i, _v3 );
                            break;

                    }
                    // ruler.Update(_ViewController.World, _ViewController.View, _ViewController.Projection);
                    SetPoint.Update(Matrix.Identity, _Camera.View, _Camera.Projection);
                    break;
                case InstrumentType.RulerPad:
                    RulerPad RulerPad = (RulerPad)SelectedInstrument;
                    switch (option)
                    {
                        case SetInstrumentOption.IncreaseTranslation://Increase Global Translation
                            Vector3 translation = (Vector3)obj;
                            RulerPad.WorldPosition += translation;
                            break;
                        case SetInstrumentOption.SetTranslation://Set Global Translation
                            translation = (Vector3)obj;
                            RulerPad.WorldPosition = translation;
                            break;
                        case SetInstrumentOption.SetRotationY://Set Global Rotation
                            value = (float)obj;
                            RulerPad.ModelRatationYValue = value;
                            break;
                        case SetInstrumentOption.Attach2Camera:
                            Vector3 _v = new Vector3(_Camera.Direction.X, 0f, _Camera.Direction.Z);
                            _v.Normalize();
                            RulerPad.WorldPosition = new Vector3(_Camera.Position.X, 0f, _Camera.Position.Z) + 20 * _v;

                            break;

                    }
                    RulerPad.Update(Matrix.Identity, _Camera.View, _Camera.Projection);

                    break;

            }
            InstrumentSyncronization();
        }
        // skc, remove later 
        private void InstrumentSyncronization()
        {
            
            if (_SurveyingInstruments == null)
                return;

            //Syncronize _Instruments and _SurveyingInstruments
            LinkedListNode<Instrument> ptr = _Instruments.First;
            LinkedListNode<Instrument> ptr2 = _SurveyingInstruments.First;
            for (int i = 0; i < _Instruments.Count; i++)
            {
                switch (ptr.Value.Type)
                {
                    case InstrumentType.TotalStation:
                    case InstrumentType.Leveling:
                    case InstrumentType.Theodolite:
                        ((TotalStation)ptr2.Value).SetInstrument(((TotalStation)ptr.Value).WarpInstrment());
                        break;
                    case InstrumentType.Ruler:
                        ((Ruler)ptr2.Value).SetInstrument(((Ruler)ptr.Value).WarpInstrment());
                        break;
                    case InstrumentType.Pole:
                        ((Pole)ptr2.Value).SetInstrument(((Pole)ptr.Value).WarpInstrment());
                        break;
                    case InstrumentType.HintBox:
                        ((HintBox)ptr2.Value).SetInstrument(((HintBox)ptr.Value).WarpInstrment());
                        break;

                }
                ptr = ptr.Next;
                ptr2 = ptr2.Next;
            }
            _SurveyingWindowController.InstrumentsSyncronization(_SurveyingInstruments);
        }
        #endregion
// skc, remove later 
        #region UserMotion
        public enum Direction
        {
            North, West, South, East, Up, Down, Clockwise,AntiClockwise
        };
        public void UserPan(Direction direction, float distance)
        {
            switch (direction)
            {
                case Direction.Up://up
                    _ViewController.MoveEyesUp(distance);
                    break;
                case Direction.Down://down
                    _ViewController.MoveEyesDown(distance);
                    break;
                case Direction.East://left
                    if(Math.Abs(distance) >0.3f)
                        _ViewController.MoveEyesLeft(distance*2);
                    break;
                case Direction.West://right
                    _ViewController.MoveEyesRight(distance);
                    break;
                case Direction.North://forwards
                    if (Math.Abs(distance) > 0.3f)
                    _ViewController.UserMoveForwards(distance*2);
                    break;
                case Direction.South://backwards
                    _ViewController.UserMoveBackwards(distance);
                    break;
            }
        }

        public void UserTurn(Direction direction, float degree)
        {
            switch (direction)
            {
                case Direction.AntiClockwise://left
                    _ViewController.UserTurnLeft(degree);
                    break;
                case Direction.Clockwise://right
                    _ViewController.UserTurnLeft(-degree);
                    break;
                case Direction.Up://Up
                    _ViewController.UserTurnUp(degree);
                    break;
                case Direction.Down://Down
                    _ViewController.UserTurnUp(-degree);
                    break;
            }
        }
        #endregion

        public Instrument SelectedInstrument
        {
            get 
            {
                if (_SelectedInstrument != null)
                    return _SelectedInstrument.Value;
                else
                    return null;
            }
            set
            {

                _SelectedInstrument.Value = value; 
            }
        }

        public String[] Instruments
        {
            get
            {
                String[] names = new String[_InstrumentCount];
                int index=0;
                foreach (Instrument instrument in _Instruments)
                {
                    names[index] = instrument.Name;
                    index++;
                }
                return names;
            }
        }

        public Arguments SimuSurveyArguments
        {
            get { return _Arguments; }
        }
    }
}
