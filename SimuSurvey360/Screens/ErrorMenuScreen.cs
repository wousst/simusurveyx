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
    class ErrorMenuScreen : MenuScreen
    {
        #region Fields

        MenuEntry XMenuEntry;
        MenuEntry YMenuEntry;
        MenuEntry ZMenuEntry;
        MenuEntry ResetMenuEntry;
        MenuEntry DistanceMenuEntry;




        static double currentX = 0f;
        static double currentY = 0f;
        static double currentZ = 0f;

        bool distance_error = true; 
        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public ErrorMenuScreen()
            : base("Error Setting")
        {
            // Create our menu entries.
            XMenuEntry = new MenuEntry(string.Empty);
            YMenuEntry = new MenuEntry(string.Empty);
            ZMenuEntry = new MenuEntry(string.Empty);
            ResetMenuEntry = new MenuEntry("Reset 3-Axis Error");
            DistanceMenuEntry = new MenuEntry("Distance Error");
            MenuEntry backMenuEntry = new MenuEntry("Back");

            SetMenuEntryText();



            // Hook up menu event handlers.
            XMenuEntry.Selected += XMenuEntrySelected;
            XMenuEntry.Righted += XMenuEntrySelected;
            XMenuEntry.Lefted += XMenuEntryLefted;

            YMenuEntry.Selected += YMenuEntrySelected;
            YMenuEntry.Righted += YMenuEntrySelected;
            YMenuEntry.Lefted += YMenuEntryLefted;

            ZMenuEntry.Selected += ZMenuEntrySelected;
            ZMenuEntry.Righted += ZMenuEntrySelected;
            ZMenuEntry.Lefted += ZMenuEntryLefted;

            DistanceMenuEntry.Selected += DistanceEntrySelected;
            DistanceMenuEntry.Lefted += DistanceEntrySelected;
            DistanceMenuEntry.Righted += DistanceEntrySelected;

            ResetMenuEntry.Selected += ResetMenuEntrySelected;

            backMenuEntry.Selected += OnCancel;
            
            // Add entries to the menu.
            MenuEntries.Add(XMenuEntry);
            MenuEntries.Add(YMenuEntry);
            MenuEntries.Add(ZMenuEntry);
            MenuEntries.Add(ResetMenuEntry);
            MenuEntries.Add(DistanceMenuEntry);
            MenuEntries.Add(backMenuEntry);


        }


        public override void LoadContent(ContentManager content, SystemState _State)
        {
            State = _State;
            _Panel = content.Load<Texture2D>("GradientPanel");
            currentX = State.ErrorX;
            currentY = State.ErrorY;
            currentZ = State.ErrorZ;
            SetMenuEntryText();
        }


        /// <summary>
        /// Fills in the latest values for the options screen menu text.
        /// </summary>
        void SetMenuEntryText()
        {
            XMenuEntry.Text = "X-Axis: " + System.String.Format("{0:f2}", currentX);
            YMenuEntry.Text = "Y-Axis: " + System.String.Format("{0:f2}", currentY);
            ZMenuEntry.Text = "Z-Axis: " + System.String.Format("{0:f2}", currentZ);
            DistanceMenuEntry.Text = "Distance Error: " + (distance_error ? "on" : "off");
        }


        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the menu entry is selected.
        /// </summary>
        void XMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            currentX  += 0.01f;
            if (currentX > 1f)
                currentX = -1.00f;
            State.ErrorX = (float)currentX; 
            SetMenuEntryText();
        }
        void XMenuEntryLefted(object sender, PlayerIndexEventArgs e)
        {          
            if (currentX > -1f)
                currentX -= 0.01f;
            State.ErrorX = (float)currentX; 
            SetMenuEntryText();
        }
        void YMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            currentY += 0.01f;
            if (currentY > 1f)
                currentY = -1.00f;
            State.ErrorY = (float)currentY; 
            SetMenuEntryText();
        }
        void YMenuEntryLefted(object sender, PlayerIndexEventArgs e)
        {
            if (currentY > -1f)
                currentY -= 0.01f;
            State.ErrorY = (float)currentY; 
            SetMenuEntryText();
        }
        void ZMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            currentZ += 0.01f;
            if (currentZ > 1f)
                currentZ = -1.00f;
            State.ErrorZ = (float)currentZ; 
            SetMenuEntryText();
        }
        void ZMenuEntryLefted(object sender, PlayerIndexEventArgs e)
        {
            if (currentZ > -1f)
                currentZ -= 0.01f;
            State.ErrorZ = (float)currentZ; 
            SetMenuEntryText();
        }

        /// <summary>
        /// Event handler for when the Distance menu entry is selected.
        /// </summary>
        void DistanceEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            distance_error = !distance_error;
            State.DistanceErrorEnable = distance_error; 
            SetMenuEntryText();
        }

        /// <summary>
        /// Event handler for when the Elf menu entry is selected.
        /// </summary>
        void ResetMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            currentX = currentY = currentZ = 0f;
            SetMenuEntryText();
        }


        #endregion
    }
}
