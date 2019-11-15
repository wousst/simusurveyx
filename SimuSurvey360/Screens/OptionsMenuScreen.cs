#region File Description
//-----------------------------------------------------------------------------
// OptionsMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace SimuSurvey360
{
    /// <summary>
    /// The options screen is brought up over the top of the main menu
    /// screen, and gives the user a chance to configure the game
    /// in various hopefully useful ways.
    /// </summary>
    class OptionsMenuScreen : MenuScreen
    {
        #region Fields

        MenuEntry ungulateMenuEntry;
        MenuEntry languageMenuEntry;
        MenuEntry audioMenuEntry;
        MenuEntry backgroundMenuEntry;
        MenuEntry errorMenuEntry;

        enum Ungulate
        {
            BactrianCamel,
            Dromedary,
            Llama,
        }

        static Ungulate currentUngulate = Ungulate.Dromedary;

        static string[] languages = { "English", "Chinese"};
        static int currentLanguage = 0;

        static bool audio = true;

        static int background = 0 ;

        static int elf = 23;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public OptionsMenuScreen()
            : base("Options")
        {
            // Create our menu entries.
            ungulateMenuEntry = new MenuEntry(string.Empty);
            languageMenuEntry = new MenuEntry(string.Empty);
            audioMenuEntry = new MenuEntry(string.Empty);
            backgroundMenuEntry = new MenuEntry(string.Empty);
            errorMenuEntry = new MenuEntry(string.Empty);

            SetMenuEntryText();

            MenuEntry backMenuEntry = new MenuEntry("Back");

            // Hook up menu event handlers.
            ungulateMenuEntry.Selected += UngulateMenuEntrySelected;
            languageMenuEntry.Selected += LanguageMenuEntrySelected;
            audioMenuEntry.Selected += AudioMenuEntrySelected;
            backgroundMenuEntry.Selected += BackgroundMenuEntrySelected;
            errorMenuEntry.Selected += ErrorMenuEntrySelected;
            backMenuEntry.Selected += OnCancel;
            
            // Add entries to the menu.
            // MenuEntries.Add(ungulateMenuEntry);
            MenuEntries.Add(languageMenuEntry);
            MenuEntries.Add(audioMenuEntry);
            MenuEntries.Add(backgroundMenuEntry);
            MenuEntries.Add(errorMenuEntry);
            MenuEntries.Add(backMenuEntry);
        }

        public override void LoadContent(ContentManager content, SystemState _State)
        {
            State = _State;
            _Panel = content.Load<Texture2D>("GradientPanel");
            audio = State.AudioEnable;

        }
        /// <summary>
        /// Fills in the latest values for the options screen menu text.
        /// </summary>
        void SetMenuEntryText()
        {
            // ungulateMenuEntry.Text = "Ungulate: " + currentUngulate;
            languageMenuEntry.Text = "Language: " + languages[currentLanguage];
            audioMenuEntry.Text = "Audio: " + (audio ? "on" : "off");
            backgroundMenuEntry.Text = "Background: " + ( background + 1 ).ToString() ;
            // elfMenuEntry.Text = "elf: " + elf;
            errorMenuEntry.Text = "Error Settings";

        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Ungulate menu entry is selected.
        /// </summary>
        void UngulateMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            currentUngulate++;

            if (currentUngulate > Ungulate.Llama)
                currentUngulate = 0;

            SetMenuEntryText();
        }


        /// <summary>
        /// Event handler for when the Language menu entry is selected.
        /// </summary>
        void LanguageMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            currentLanguage = (currentLanguage + 1) % languages.Length;

            SetMenuEntryText();
        }


        /// <summary>
        /// Event handler for when the Frobnicate menu entry is selected.
        /// </summary>
        void AudioMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            audio = !audio;
            State.AudioEnable = audio; 
            SetMenuEntryText();
        }

        /// <summary>
        /// Event handler for when the Frobnicate menu entry is selected.
        /// </summary>
        void BackgroundMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            background = ( background + 1 ) % 2 ;
            State.Background_N = background;
            SetMenuEntryText();
        }
        /// <summary>
        /// Event handler for when the Elf menu entry is selected.
        /// </summary>
        void ErrorMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            // elf++;

            // SetMenuEntryText();
            // State.Wait2ProcessState = SystemState.S_State.STATE_MENU_OPTIONS;
            ScreenManager.AddScreen(new ErrorMenuScreen(), e.PlayerIndex);
        }


        #endregion
    }
}
