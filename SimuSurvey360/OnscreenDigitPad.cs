
#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
#endregion

namespace SimuSurvey360
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public  partial class OnscreenDigitPad : Microsoft.Xna.Framework.DrawableGameComponent
    {
        /// <summary>
        /// The OnscreenKeyboard DrawableGameComponent provides a full-screen (1280x720 raw size)
        /// On-Screen Keyboard for text input in games.  It looks similar to the onscreen keyboard
        /// used on the XBox 360.
        /// </summary>
        /// <param name="game">The Game object the the component is registering with.</param>
        SystemState State; 
        public OnscreenDigitPad(Game game)
           : base(game)
        {
            // TODO: Construct any child components here
        }
        public OnscreenDigitPad(SystemState _State, Game game)
            : base(game)
        {
            State = _State;
        }

        //  SpriteBatch Object we will use to draw to the Display
        private SpriteBatch spriteBatch;
        
        // Texture objects we will be using
        private Texture2D t2dFont;
        private Texture2D t2dKeyboard ; // = new Texture2D;
        private Texture2D t2dKeySelector; // = new Texture2D;


        // Control Variables
        private int oskb_EntryStartX = 520;
        private int oskb_EntryStartY = 167;
        private int oskb_EntryHilightPos = 0;
        private int oskb_EntryHilightX = 610;
        private int oskb_EntryHilightY = 265;
        private int oskb_EntryHilightVerticalSpace = 50;
        private int oskb_EntryHilightHorizontalSpace = 50;

        private int oskb_PlayerIconX = 1115;
        private int oskb_PlayerIconY = 100;
        private string oskb_EntryString = "";
        private int oskb_CursorLocation = 0;
        private float oskb_fCursorFlashDelay = 0.25f;  // cursor flash freq.
        private float oskb_fCursorCounter = 0.0f;
        private string oskb_sCursorChar = "|";
        private int oskb_DefaultKeyboard = 0;
        private int oskb_ShiftedKeyboard = 1;
        private float fKeyPressCheckDelay = 0.25f; // skc, 0.1 -> 0.25 would be better 
        private float fTotalElapsedTime = 0;
        private int oskb_MaxLength = 12;
        private int oskb_exitstate = -1;
        private string oskb_PromptText = "Survey record:";
        private int oskb_PromptX = 510;
        private int oskb_PromptY = 120;
        private Color oskb_PromptColor = Color.Black;
        private ContentManager oskb_Content;
        private string[] sImageNames = new string[] {@"content\onscreenkeyboard\1280\1280_digitpad_3R"};
        private Rectangle oskb_DestRect = new Rectangle(0,0,1280,720);
        private Rectangle oskb_SourceRrect = new Rectangle(0,0,1280,720);

        private char[] cKeyValues = new char[] {  '1', '2', '3', 
                                                  '4', '5', '6',
                                                  '7', '8', '9',
                                                  '_', '0', '.',
                                                 };
        private PlayerIndex oskb_CurrentPlayer=PlayerIndex.One;

        /// <summary>
        /// Get or Set the value displayed in the text input box.  This is the value updated by the
        /// user as they enter text.
        /// </summary>
        public string Text
        {
            get { return oskb_EntryString; }
            set { oskb_EntryString = value; }
        }

        /// <summary>
        /// Get or Set the maximum number of characters that will be allowed in the text entry box.
        /// </summary>
        public int MaxLength
        {
            get { return oskb_MaxLength; }
            set { oskb_MaxLength = value; }
        }

        /// <summary>
        /// Returns the method the user used to exit the Onscreen Keyboard.  If -1, the keyboard hasn't
        /// yet been displayed.  0=OSKB is Active, 1=User exited via B/Enter (Done), 2=User exited via 
        /// Back/Escape (Abort)
        /// </summary>
        public int ExitState
        {
            get { return oskb_exitstate; }
        }

        /// <summary>
        /// Get or Set the prompt text displayed at the top of the Onscreen Keyboard.
        /// </summary>
        public string Prompt
        {
            get { return oskb_PromptText; }
            set { oskb_PromptText = value; }
        }

        /// <summary>
        /// Get or Set the color in which to display the prompt text at the top of the Onscreen Keyboard
        /// </summary>
        public Color PromptColor
        {
            get { return oskb_PromptColor; }
            set { oskb_PromptColor = value; }
        }

        /// <summary>
        /// Get or Set the current Player for gamepad input.
        /// </summary>
        public PlayerIndex Player
        {
            get { return oskb_CurrentPlayer; }
            set { oskb_CurrentPlayer = value; }
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            oskb_Content = new ContentManager(Game.Services);
            base.Initialize();
        }

        protected override void LoadGraphicsContent(bool loadAllContent)
        {

            if (loadAllContent)
            {
                // If we aren't using 1280x720 (the default resolution) check for our two
                // alternatives : 1024x768 and 800x600.  These are the most common display
                // resolutions, so we have custom bitmaps to support them.  Adding another
                // resolution would be as simple as creating the bitmap and making a copy
                // of one of these "if" blocks and changing the parameters appropriately.


                spriteBatch = new SpriteBatch(GraphicsDevice);
                t2dKeyboard = oskb_Content.Load<Texture2D>(sImageNames[0]);
                t2dKeySelector = oskb_Content.Load<Texture2D>(@"content\onscreenkeyboard\Selector_Small");

                t2dFont = oskb_Content.Load<Texture2D>(@"content\onscreenkeyboard\Courier30pt");
            }
            base.LoadGraphicsContent(loadAllContent);
        }

        /// <summary>
        /// A very simple text writing method using a mono-spaced font to output text.  Supports font colors
        /// </summary>
        /// <param name="spriteBatch">SpriteBatch object to be used when drawing.  Must be in a Draw state already.</param>
        /// <param name="sTextOut">Text to output to the screen.</param>
        /// <param name="x">X Coordinate for the upper left corner of draw location.</param>
        /// <param name="y">Y Coordinate for the upper left corner of draw location.</param>
        /// <param name="colorTint">Color to draw the text in.</param>
        public void WriteText(SpriteBatch spriteBatch, string sTextOut, int x, int y, Color colorTint)
        {
            // Very simple text output method.  Uses a mono-spaced font to keep things easy
           
            int iFontX = 0;
            int iFontY = 0;
            int iFontHeight = 30;
            int iFontWidth = 19;
            int iFontAsciiStart = 32;
            int iFontAsciiEnd = 126;
            int iOutChar;
            for (int i = 0; i < sTextOut.Length; i++)
            {
                iOutChar = (int)sTextOut[i];
                if ((iOutChar >= iFontAsciiStart) & (iOutChar <= iFontAsciiEnd))
                {
                    spriteBatch.Draw(t2dFont,
                                     new Rectangle(x + (iFontWidth * i), y, iFontWidth-1, iFontHeight),
                                     new Rectangle(iFontX + ((iOutChar - iFontAsciiStart) * iFontWidth),
                                     iFontY, iFontWidth-1, iFontHeight),
                                     colorTint);
                }
            }
        }

        /// <summary>
        /// Adds a substring to the given string at the given location.  Returns the result as a string.
        /// </summary>
        /// <param name="sInitial">String that will serve as the base string to add characters to.</param>
        /// <param name="iPos">Location withing sInitial that the character(s) will be added.</param>
        /// <param name="sToAdd">String to add to sInitial at the iPos location.</param>
        /// <returns>Resultant string with inserted characters.</returns>
        protected string AddCharToString(string sInitial, int iPos, string sToAdd)
        {
            if (iPos > sInitial.Length)
            {
                return sInitial + sToAdd;
            }
            string sBefore = sInitial.Substring(0, iPos);
            string sAfter = sInitial.Substring(iPos);
            return sBefore + sToAdd + sAfter;
        }

        /// <summary>
        /// Remove a single character from a string at the indicated position.  Result is returned as a string.
        /// </summary>
        /// <param name="sInitial">String that will serve as the base string to remove a character from.</param>
        /// <param name="iPos">Location to remove character from sInitial.</param>
        /// <returns>Resultant string without the removed character.</returns>
        protected string RemoveCharFromString(string sInitial, int iPos)
        {
            if (iPos == sInitial.Length)
            {
                return sInitial.Substring(0, sInitial.Length - 1);
            }

            string sBefore = sInitial.Substring(0, iPos - 1);
            string sAfter = sInitial.Substring(iPos);
            return sBefore + sAfter;
        }

        protected void Update_TextEntry(GamePadState GPad, KeyboardState KBoard, GameTime gameTime)
        {
            // Set the exit state to 0 to indicate that we are active
            oskb_exitstate = 0;

            // ======= Manage the "flashing cursor" =======
            // Accumulate the time since the last cursor flash
            oskb_fCursorCounter += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // If it's time to flash the cursor, just swap the current cursor char with
            // the alternate character (space or vertical bar)
            if (oskb_fCursorCounter > oskb_fCursorFlashDelay)
            {
                if (oskb_sCursorChar == "|")
                {
                    oskb_sCursorChar = " ";
                }
                else
                {
                    oskb_sCursorChar = "|";
                }
                oskb_fCursorCounter = 0.0f;
            }



            // If the player clicks the left thumbstick, swap the default and shifted keyboards
            //if (GPad.Buttons.LeftStick == ButtonState.Pressed)
            //{
            //    if (oskb_DefaultKeyboard == 0)
            //    {
            //        oskb_DefaultKeyboard = 1;
            //        oskb_ShiftedKeyboard = 0;
            //    }
            //    else
            //    {
            //        oskb_DefaultKeyboard = 0;
            //        oskb_ShiftedKeyboard = 1;
            //    }
            //    fTotalElapsedTime = -0.10f;
            //}

            // Check for moving the hilight on the Onscreen Keyboard

            // First, handle moving left or right
            if ((GPad.ThumbSticks.Left.X < -0.25f) || (GPad.DPad.Left == ButtonState.Pressed))
            {
                oskb_EntryHilightPos -= 1;
                fTotalElapsedTime = 0.0f;
            }

            if ((GPad.ThumbSticks.Left.X > 0.25f) || (GPad.DPad.Right == ButtonState.Pressed))
            {
                oskb_EntryHilightPos += 1;
                fTotalElapsedTime = 0.0f;
            }

            if (oskb_EntryHilightPos < 0)
            {
                oskb_EntryHilightPos = 0;
            }

            if (oskb_EntryHilightPos > 11)
            {
                oskb_EntryHilightPos = 11;
            }

            // Now handle moving Up or Down
            if (GPad.ThumbSticks.Left.Y < -0.25f || (GPad.DPad.Down == ButtonState.Pressed))
            {
                if (oskb_EntryHilightPos < 9)
                {
                    oskb_EntryHilightPos += 3;
                    fTotalElapsedTime = 0.0f;
                }
                else
                {
                        oskb_EntryHilightPos = 11;
                }
            }

            if (GPad.ThumbSticks.Left.Y > 0.25f || (GPad.DPad.Up == ButtonState.Pressed))
            {
                if ( oskb_EntryHilightPos > 2 )
                     oskb_EntryHilightPos -= 3;
                fTotalElapsedTime = 0.0f;
            }

            if (oskb_EntryHilightPos < 0)
            {
                oskb_EntryHilightPos += 3;
            }

            if (oskb_EntryHilightPos > 11)
            {
                oskb_EntryHilightPos -= 3;
            }

            // If the user presses the A button, take the appropriate action
            if (GPad.Buttons.A == ButtonState.Pressed && oskb_EntryString.Length < oskb_MaxLength)
            {
                // If the hilight is in the normal key area (0-40) add that character at the current
                // cursor location in the string.
                if (oskb_EntryHilightPos < 12)
                {
                    oskb_EntryString = AddCharToString(oskb_EntryString, oskb_CursorLocation,
                                    cKeyValues[oskb_EntryHilightPos].ToString());
                    oskb_CursorLocation++;
                    fTotalElapsedTime = 0.0f;
                }


            }

            // Pressing the Y button adds a space at the current cursor location
            //if (GPad.Buttons.Y == ButtonState.Pressed && oskb_EntryString.Length < oskb_MaxLength)
            //{
            //    oskb_EntryString = AddCharToString(oskb_EntryString, oskb_CursorLocation, " ");
            //    oskb_CursorLocation++;
            //    fTotalElapsedTime = 0.0f;
            //}

            // Pressing the X button deletes the character at the current location
            if (GPad.Buttons.X == ButtonState.Pressed)
            {
                // Make sure the string isn't aready empty
                if (oskb_EntryString.Length > 0)
                {
                    // Make sure we aren't at cursor location 0 (nothing to delete prior to the cursor)
                    if (oskb_CursorLocation > 0)
                    {
                        oskb_EntryString = RemoveCharFromString(oskb_EntryString, oskb_CursorLocation);
                        oskb_CursorLocation--;
                    }
                }
                fTotalElapsedTime = 0.0f;
            }

            // Handle moving the cursor within the input line.  The left shoulder button moves left.
            if (GPad.Buttons.LeftShoulder == ButtonState.Pressed)
            {
                if (oskb_CursorLocation > 0)
                {
                    oskb_CursorLocation--;
                }
                fTotalElapsedTime = 0.0f;
            }

            // The right shoulder button moves right.
            if (GPad.Buttons.RightShoulder == ButtonState.Pressed)
            {
                if (oskb_CursorLocation < oskb_EntryString.Length)
                {
                    oskb_CursorLocation++;
                }
                fTotalElapsedTime = 0.0f;
            }

            // Handle methods to exit the screen
            if (GPad.Buttons.B == ButtonState.Pressed || KBoard.IsKeyDown(Keys.Escape))
            {
                oskb_exitstate = 2;
                this.Enabled = false;
                this.Visible = false;
            }

            if (GPad.Buttons.Start == ButtonState.Pressed || KBoard.IsKeyDown(Keys.Enter))
            {
                oskb_exitstate = 1;
                this.Enabled = false;
                this.Visible = false;
            }

            // If on the computer, allow the user to press keyboard keys to enter text instead of using
            // the onscreen keyboard.

            bool AddKey = false;
            string key = "";


            if (KBoard.IsKeyDown(Keys.D1)) { AddKey = true; key = "1"; }
            if (KBoard.IsKeyDown(Keys.D2)) { AddKey = true; key = "2"; }
            if (KBoard.IsKeyDown(Keys.D3)) { AddKey = true; key = "3"; }
            if (KBoard.IsKeyDown(Keys.D4)) { AddKey = true; key = "4"; }
            if (KBoard.IsKeyDown(Keys.D5)) { AddKey = true; key = "5"; }
            if (KBoard.IsKeyDown(Keys.D6)) { AddKey = true; key = "6"; }
            if (KBoard.IsKeyDown(Keys.D7)) { AddKey = true; key = "7"; }
            if (KBoard.IsKeyDown(Keys.D8)) { AddKey = true; key = "8"; }
            if (KBoard.IsKeyDown(Keys.D9)) { AddKey = true; key = "9"; }
            if (KBoard.IsKeyDown(Keys.D0)) { AddKey = true; key = "0"; }


            if (KBoard.IsKeyDown(Keys.OemPeriod)) { AddKey = true; key = "."; }


            if (AddKey && oskb_EntryString.Length < oskb_MaxLength)
            {
                oskb_EntryString = AddCharToString(oskb_EntryString, oskb_CursorLocation, key);
                oskb_CursorLocation++;
                fTotalElapsedTime = 0.0f;
            }

            // Handle backspace
            if (KBoard.IsKeyDown(Keys.Back))
            {
                // Make sure the string isn't aready empty
                if (oskb_EntryString.Length > 0)
                {
                    // Make sure we aren't at cursor location 0 (nothing to delete prior to the cursor)
                    if (oskb_CursorLocation > 0)
                    {
                        oskb_EntryString = RemoveCharFromString(oskb_EntryString, oskb_CursorLocation);
                        oskb_CursorLocation--;
                    }
                }
                fTotalElapsedTime = 0.0f;
            }

            // Handle moving the cursor within the input line.
            if (KBoard.IsKeyDown(Keys.Left))
            {
                if (oskb_CursorLocation > 0)
                {
                    oskb_CursorLocation--;
                }
                fTotalElapsedTime = 0.0f;
            }

            // The right shoulder button moves right.
            if (KBoard.IsKeyDown(Keys.Right))
            {
                if (oskb_CursorLocation < oskb_EntryString.Length)
                {
                    oskb_CursorLocation++;
                }
                fTotalElapsedTime = 0.0f;
            }
        }

        // 不用參數 , 直接參考 oskb_EntryHilightPos
        protected void Draw_TextEntry(SpriteBatch sbBatch)
        {
            string sBeforeCursor = oskb_EntryString.Substring(0, oskb_CursorLocation);
            string sAfterCursor = "";
            if (oskb_CursorLocation < oskb_EntryString.Length)
            {
                sAfterCursor = oskb_EntryString.Substring(oskb_CursorLocation);
            }
            // Draws a text entry screen to allow user to input their name
            // 整個 KeyBoard 底圖
            sbBatch.Draw(t2dKeyboard, oskb_DestRect, Color.White);
            

            // skc, Draw Prompt
            WriteText(sbBatch, oskb_PromptText, oskb_PromptX, oskb_PromptY, oskb_PromptColor);
            WriteText(sbBatch, sBeforeCursor + oskb_sCursorChar + sAfterCursor, oskb_EntryStartX, oskb_EntryStartY, Color.Black);

            // skc, Draw each button when mouse cursor enter
            if (oskb_EntryHilightPos < 12)
            {
                int HLX = oskb_EntryHilightX + (oskb_EntryHilightHorizontalSpace * (oskb_EntryHilightPos % 3));
                int HLY = oskb_EntryHilightY + (oskb_EntryHilightVerticalSpace * (oskb_EntryHilightPos / 3));
                // skc, 只要畫一小塊白色
                sbBatch.Draw(t2dKeySelector, new Rectangle(HLX, HLY, 44, 44), Color.White);
            }



        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public  override  void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            fTotalElapsedTime += elapsed;

            if (fTotalElapsedTime > fKeyPressCheckDelay)
            {
                Update_TextEntry(GamePad.GetState(oskb_CurrentPlayer), Keyboard.GetState(), gameTime);
            }

             base.Update(gameTime);
        }

        public  override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            Draw_TextEntry(spriteBatch);
            spriteBatch.End();

             base.Draw(gameTime);
        }
    }
}


