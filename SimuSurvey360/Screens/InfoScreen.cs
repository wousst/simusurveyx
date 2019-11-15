using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimuSurvey360.Instruments;

namespace SimuSurvey360.Screens
{
    class InfoScreen : MenuScreen
    {
        #region Field
        Rectangle _InfoDisplayArea;
        InstrumentArgs _Argument;
        int _SpaceSize;
        int _LineWidth;
        int _FontSize; //this is an approximate size
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        public InfoScreen(InstrumentArgs arg)
            : base("Properties")
        {
            // Flag that there is no need for the game to transition
            // off when the pause menu is on top of it.
            IsPopup = true;

            // Create our menu entries.
            MenuEntry resumeGameMenuEntry = new MenuEntry("Resume");
            
            // Hook up menu event handlers.
            resumeGameMenuEntry.Selected += OnCancel;

            // Add entries to the menu.
            MenuEntries.Add(resumeGameMenuEntry);

            //Internal 
            _Argument = arg;
            _InfoDisplayArea = new Rectangle(50, 100, 700, 380);
            _HeightOffset = _InfoDisplayArea.Height;
            _SpaceSize = 10;
            _LineWidth = 40;
            _FontSize = 12;
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Quit Game menu entry is selected.
        /// </summary>
        void QuitGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            const string message = "Are you sure you want to quit this game?";

           /* MessageBoxScreen confirmQuitMessageBox = new MessageBoxScreen(message);

            confirmQuitMessageBox.Accepted += ConfirmQuitMessageBoxAccepted;

            ScreenManager.AddScreen(confirmQuitMessageBox, ControllingPlayer);*/
        }


        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to quit" message box. This uses the loading screen to
        /// transition from the game back to the main menu screen.
        /// </summary>
        void ConfirmQuitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            // skc remark
            //LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(),
            //                                               new MainScreen());
        }


        #endregion

        #region Draw


        /// <summary>
        /// Draws the pause menu screen. This darkens down the gameplay screen
        /// that is underneath us, and then chains to the base MenuScreen.Draw.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);
            DrawInfo();

            
            base.Draw(gameTime);
        }

        private void DrawInfo()
        {
            // remove later
            return;
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            spriteBatch.Begin();

            //Generic Properties
            spriteBatch.DrawString(font, "Position: ", new Vector2(_InfoDisplayArea.Left, _InfoDisplayArea.Top), Color.Wheat);
            spriteBatch.DrawString(font, "E Coordinate:", new Vector2(_InfoDisplayArea.Left + 2*_FontSize + _SpaceSize, _InfoDisplayArea.Top+_LineWidth), Color.Wheat);
            spriteBatch.DrawString(font, _Argument.WorldPosition.X.ToString(), new Vector2(_InfoDisplayArea.Left +  19* _FontSize + _SpaceSize, _InfoDisplayArea.Top + _LineWidth), Color.Wheat);
            spriteBatch.DrawString(font, "N Coordinate:", new Vector2(_InfoDisplayArea.Left + 2 * _FontSize + _SpaceSize, _InfoDisplayArea.Top + _LineWidth*2), Color.Wheat);
            spriteBatch.DrawString(font, _Argument.WorldPosition.Z.ToString(), new Vector2(_InfoDisplayArea.Left + 19 * _FontSize + _SpaceSize, _InfoDisplayArea.Top + _LineWidth*2), Color.Wheat);
            //spriteBatch.Draw(ScreenManager.blankTexture, _InfoDisplayArea, Color.Black);

            if (_Argument.Type == InstrumentType.TotalStation)//Total Station Properties
            {
                TotalStationArgs targs = (TotalStationArgs)_Argument;
                spriteBatch.DrawString(font, "Tripod: ", new Vector2(_InfoDisplayArea.Left, _InfoDisplayArea.Top+ _LineWidth*3), Color.Wheat);
                spriteBatch.DrawString(font, "Length:", new Vector2(_InfoDisplayArea.Left + 2 * _FontSize + _SpaceSize, _InfoDisplayArea.Top + _LineWidth*4), Color.Wheat);
                spriteBatch.DrawString(font, targs.TripodLength.ToString(), new Vector2(_InfoDisplayArea.Left + 11 * _FontSize + _SpaceSize, _InfoDisplayArea.Top + _LineWidth*4), Color.Wheat);
                spriteBatch.DrawString(font, "Rotation:", new Vector2(_InfoDisplayArea.Left + 2 * _FontSize + _SpaceSize, _InfoDisplayArea.Top + _LineWidth * 5), Color.Wheat);
                spriteBatch.DrawString(font, targs.TripodRotationValue.ToString(), new Vector2(_InfoDisplayArea.Left + 14 * _FontSize + _SpaceSize, _InfoDisplayArea.Top + _LineWidth * 5), Color.Wheat);
            }

            spriteBatch.End();
        }
        #endregion
    }
}
