
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
    public  partial class OnscreenKeyboard : Microsoft.Xna.Framework.DrawableGameComponent
    {
        /// <summary>
        /// The OnscreenKeyboard DrawableGameComponent provides a full-screen (1280x720 raw size)
        /// On-Screen Keyboard for text input in games.  It looks similar to the onscreen keyboard
        /// used on the XBox 360.
        /// </summary>
        /// <param name="game">The Game object the the component is registering with.</param>
        SystemState State; 
        public OnscreenKeyboard(Game game)
           : base(game)
        {
            // TODO: Construct any child components here
        }
        public OnscreenKeyboard(SystemState _State, Game game)
            : base(game)
        {
            State = _State;
        }

        //  SpriteBatch Object we will use to draw to the Display
        private SpriteBatch spriteBatch;
        
        // Texture objects we will be using
        private Texture2D t2dFont;
        private Texture2D[] t2dKeyboard = new Texture2D[3];
        private Texture2D[] t2dKeySelector = new Texture2D[2];
        private Texture2D[] t2dPlayerIcons = new Texture2D[4];

        // Control Variables
        private int oskb_EntryStartX = 260;
        private int oskb_EntryStartY = 167;
        private int oskb_EntryHilightPos = 0;
        private int oskb_EntryHilightX = 362;
        private int oskb_EntryHilightY = 265;
        private int oskb_EntryHilightVerticalSpace = 50;
        private int oskb_EntryHilightHorizontalSpace = 50;
        private int oskb_EntryHilightGapSize = 50;
        private int oskb_EntryHilightGapLocation = 7;
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
        private int oskb_KeyboardShiftState = 0;
        private int oskb_MaxLength = 39;
        private int oskb_exitstate = -1;
        private string oskb_PromptText = "Please record the surveying result:";
        private int oskb_PromptX = 50;
        private int oskb_PromptY = 120;
        private Color oskb_PromptColor = Color.Black;
        private ContentManager oskb_Content;
        private string[] sImageNames = new string[] { @"content\onscreenkeyboard\1280\1280_keyboard_1",
                                                      @"content\onscreenkeyboard\1280\1280_keyboard_2",
                                                      @"content\onscreenkeyboard\1280\1280_keyboard_3" };
        private Rectangle oskb_DestRect = new Rectangle(0,0,1280,720);
        private Rectangle oskb_SourceRrect = new Rectangle(0,0,1280,720);

        private char[] cKeyValues = new char[] {   'a', 'b', 'c', 'd', 'e', 'f', 'g', '1', '2', '3', 
                                                   'h', 'i', 'j', 'k', 'l', 'm', 'n', '4', '5', '6',
                                                   'o', 'p', 'q', 'r', 's', 't', 'u', '7', '8', '9',
                                                   'v', 'w', 'x', 'y', 'z', '-', '@', '_', '0', '.',

                                                   'A', 'B', 'C', 'D', 'E', 'F', 'G', '1', '2', '3', 
                                                   'H', 'I', 'J', 'K', 'L', 'M', 'N', '4', '5', '6',
                                                   'O', 'P', 'Q', 'R', 'S', 'T', 'U', '7', '8', '9',
                                                   'V', 'W', 'X', 'Y', 'Z', '-', '@', '_', '0', '.',

                                                   ',', ';', ':', '\'', '"', '!', '?', '1', '2', '3', 
                                                   '[', ']', '{', '}', '`', '$', '%', '4', '5', '6',
                                                   '<', '>', '(', ')', '#', '~', '\\', '7', '8', '9',
                                                   '|', '=', '*', '/', '+', '-', '@', '&', '0', '.',
             
                                                   ' ', ' '
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
                t2dKeyboard[0] = oskb_Content.Load<Texture2D>(sImageNames[0]);
                t2dKeyboard[1] = oskb_Content.Load<Texture2D>(sImageNames[1]);
                t2dKeyboard[2] = oskb_Content.Load<Texture2D>(sImageNames[2]);
                t2dKeySelector[0] = oskb_Content.Load<Texture2D>(@"content\onscreenkeyboard\Selector_Small");
                t2dKeySelector[1] = oskb_Content.Load<Texture2D>(@"content\onscreenkeyboard\Selector_Long");
                t2dPlayerIcons[0] = oskb_Content.Load<Texture2D>(@"content\onscreenkeyboard\player1");
                t2dPlayerIcons[1] = oskb_Content.Load<Texture2D>(@"content\onscreenkeyboard\player2");
                t2dPlayerIcons[2] = oskb_Content.Load<Texture2D>(@"content\onscreenkeyboard\player3");
                t2dPlayerIcons[3] = oskb_Content.Load<Texture2D>(@"content\onscreenkeyboard\player4");
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

            // ======= Determine which "keyboard" to display =======
            // Default to displaying the current default keyboard
            oskb_KeyboardShiftState = oskb_DefaultKeyboard;

            // If the Left Trigger is down, display the shifted keyboard
            if (GPad.Triggers.Left > 0.25f)
            {
                oskb_KeyboardShiftState = oskb_ShiftedKeyboard;
            }

            // If the Right Trigger is down, display Keyboard 2 (Symbols)
            if (GPad.Triggers.Right > 0.25f)
            {
                oskb_KeyboardShiftState = 2;
            }

            if (KBoard.IsKeyDown(Keys.LeftShift) || KBoard.IsKeyDown(Keys.RightShift))
            {
                oskb_KeyboardShiftState = oskb_ShiftedKeyboard;
            }

            // If the player clicks the left thumbstick, swap the default and shifted keyboards
            if (GPad.Buttons.LeftStick == ButtonState.Pressed)
            {
                if (oskb_DefaultKeyboard == 0)
                {
                    oskb_DefaultKeyboard = 1;
                    oskb_ShiftedKeyboard = 0;
                }
                else
                {
                    oskb_DefaultKeyboard = 0;
                    oskb_ShiftedKeyboard = 1;
                }
                // Add an extra 0.1f to the delay (by setting the elapsed to a negative number) because
                // clicking the thumbsticks often will result in double-swapping (which is worthless).
                fTotalElapsedTime = -0.10f;
            }

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

            if (oskb_EntryHilightPos > 41)
            {
                oskb_EntryHilightPos = 41;
            }

            // Now handle moving Up or Down
            if (GPad.ThumbSticks.Left.Y < -0.25f || (GPad.DPad.Down == ButtonState.Pressed))
            {
                if (oskb_EntryHilightPos < 31)
                {
                    oskb_EntryHilightPos += 10;
                    fTotalElapsedTime = 0.0f;
                }
                else
                {
                    if (oskb_EntryHilightPos < 36)
                    {
                        oskb_EntryHilightPos = 40;
                    }
                    else
                    {
                        oskb_EntryHilightPos = 41;
                    }
                }
            }

            if (GPad.ThumbSticks.Left.Y > 0.25f || (GPad.DPad.Up == ButtonState.Pressed))
            {
                oskb_EntryHilightPos -= 10;
                fTotalElapsedTime = 0.0f;
            }

            if (oskb_EntryHilightPos < 0)
            {
                oskb_EntryHilightPos += 10;
            }

            if (oskb_EntryHilightPos > 41)
            {
                oskb_EntryHilightPos -= 10;
            }

            // If the user presses the A button, take the appropriate action
            if (GPad.Buttons.A == ButtonState.Pressed && oskb_EntryString.Length < oskb_MaxLength)
            {
                // If the hilight is in the normal key area (0-40) add that character at the current
                // cursor location in the string.
                if (oskb_EntryHilightPos < 40)
                {
                    oskb_EntryString = AddCharToString(oskb_EntryString, oskb_CursorLocation,
                                    cKeyValues[oskb_EntryHilightPos + (oskb_KeyboardShiftState * 40)].ToString());
                    oskb_CursorLocation++;
                    fTotalElapsedTime = 0.0f;
                }

                // If we are on the "space" object (40) add the space at the current cursor location.
                if (oskb_EntryHilightPos == 40)
                {
                    oskb_EntryString = AddCharToString(oskb_EntryString, oskb_CursorLocation, " ");
                    oskb_CursorLocation++;
                    fTotalElapsedTime = 0.0f;
                }

                // If we are on the backspace, remove the character at the current cursor location.
                if (oskb_EntryHilightPos == 41)
                {
                    // Make sure our string isn't already empty
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
            bool Shifted = ((KBoard.IsKeyDown(Keys.LeftShift)) || (KBoard.IsKeyDown(Keys.RightShift)));

            if (KBoard.IsKeyDown(Keys.A)) { AddKey = true; if (Shifted) key = "A"; else key = "a"; }
            if (KBoard.IsKeyDown(Keys.B)) { AddKey = true; if (Shifted) key = "B"; else key = "b"; }
            if (KBoard.IsKeyDown(Keys.C)) { AddKey = true; if (Shifted) key = "C"; else key = "c"; }
            if (KBoard.IsKeyDown(Keys.D)) { AddKey = true; if (Shifted) key = "D"; else key = "d"; }
            if (KBoard.IsKeyDown(Keys.E)) { AddKey = true; if (Shifted) key = "E"; else key = "e"; }
            if (KBoard.IsKeyDown(Keys.F)) { AddKey = true; if (Shifted) key = "F"; else key = "f"; }
            if (KBoard.IsKeyDown(Keys.G)) { AddKey = true; if (Shifted) key = "G"; else key = "g"; }
            if (KBoard.IsKeyDown(Keys.H)) { AddKey = true; if (Shifted) key = "H"; else key = "h"; }
            if (KBoard.IsKeyDown(Keys.I)) { AddKey = true; if (Shifted) key = "I"; else key = "i"; }
            if (KBoard.IsKeyDown(Keys.J)) { AddKey = true; if (Shifted) key = "J"; else key = "j"; }
            if (KBoard.IsKeyDown(Keys.K)) { AddKey = true; if (Shifted) key = "K"; else key = "k"; }
            if (KBoard.IsKeyDown(Keys.L)) { AddKey = true; if (Shifted) key = "L"; else key = "l"; }
            if (KBoard.IsKeyDown(Keys.M)) { AddKey = true; if (Shifted) key = "M"; else key = "m"; }
            if (KBoard.IsKeyDown(Keys.N)) { AddKey = true; if (Shifted) key = "N"; else key = "n"; }
            if (KBoard.IsKeyDown(Keys.O)) { AddKey = true; if (Shifted) key = "O"; else key = "o"; }
            if (KBoard.IsKeyDown(Keys.P)) { AddKey = true; if (Shifted) key = "P"; else key = "p"; }
            if (KBoard.IsKeyDown(Keys.Q)) { AddKey = true; if (Shifted) key = "Q"; else key = "q"; }
            if (KBoard.IsKeyDown(Keys.R)) { AddKey = true; if (Shifted) key = "R"; else key = "r"; }
            if (KBoard.IsKeyDown(Keys.S)) { AddKey = true; if (Shifted) key = "S"; else key = "s"; }
            if (KBoard.IsKeyDown(Keys.T)) { AddKey = true; if (Shifted) key = "T"; else key = "t"; }
            if (KBoard.IsKeyDown(Keys.U)) { AddKey = true; if (Shifted) key = "U"; else key = "u"; }
            if (KBoard.IsKeyDown(Keys.V)) { AddKey = true; if (Shifted) key = "V"; else key = "v"; }
            if (KBoard.IsKeyDown(Keys.W)) { AddKey = true; if (Shifted) key = "W"; else key = "w"; }
            if (KBoard.IsKeyDown(Keys.X)) { AddKey = true; if (Shifted) key = "X"; else key = "x"; }
            if (KBoard.IsKeyDown(Keys.Y)) { AddKey = true; if (Shifted) key = "Y"; else key = "y"; }
            if (KBoard.IsKeyDown(Keys.Z)) { AddKey = true; if (Shifted) key = "Z"; else key = "z"; }

            if (KBoard.IsKeyDown(Keys.D1)) { AddKey = true; if (Shifted) key = "!"; else key = "1"; }
            if (KBoard.IsKeyDown(Keys.D2)) { AddKey = true; if (Shifted) key = "@"; else key = "2"; }
            if (KBoard.IsKeyDown(Keys.D3)) { AddKey = true; if (Shifted) key = "#"; else key = "3"; }
            if (KBoard.IsKeyDown(Keys.D4)) { AddKey = true; if (Shifted) key = "$"; else key = "4"; }
            if (KBoard.IsKeyDown(Keys.D5)) { AddKey = true; if (Shifted) key = "%"; else key = "5"; }
            if (KBoard.IsKeyDown(Keys.D6)) { AddKey = true; if (Shifted) key = "^"; else key = "6"; }
            if (KBoard.IsKeyDown(Keys.D7)) { AddKey = true; if (Shifted) key = "&"; else key = "7"; }
            if (KBoard.IsKeyDown(Keys.D8)) { AddKey = true; if (Shifted) key = "*"; else key = "8"; }
            if (KBoard.IsKeyDown(Keys.D9)) { AddKey = true; if (Shifted) key = "("; else key = "9"; }
            if (KBoard.IsKeyDown(Keys.D0)) { AddKey = true; if (Shifted) key = ")"; else key = "0"; }

            if (KBoard.IsKeyDown(Keys.OemTilde)) { AddKey = true; if (Shifted) key = "~"; else key = "`"; }
            if (KBoard.IsKeyDown(Keys.OemSemicolon)) { AddKey = true; if (Shifted) key = ":"; else key = ";"; }
            if (KBoard.IsKeyDown(Keys.OemQuotes)) { AddKey = true; if (Shifted) key = "\""; else key = "'"; }
            if (KBoard.IsKeyDown(Keys.OemQuestion)) { AddKey = true; if (Shifted) key = "?"; else key = "/"; }
            if (KBoard.IsKeyDown(Keys.OemPlus)) { AddKey = true; if (Shifted) key = "+"; else key = "="; }
            if (KBoard.IsKeyDown(Keys.OemPipe)) { AddKey = true; if (Shifted) key = "|"; else key = "\\"; }
            if (KBoard.IsKeyDown(Keys.OemPeriod)) { AddKey = true; if (Shifted) key = ">"; else key = "."; }
            if (KBoard.IsKeyDown(Keys.OemOpenBrackets)) { AddKey = true; if (Shifted) key = "{"; else key = "["; }
            if (KBoard.IsKeyDown(Keys.OemCloseBrackets)) { AddKey = true; if (Shifted) key = "}"; else key = "]"; }
            if (KBoard.IsKeyDown(Keys.OemMinus)) { AddKey = true; if (Shifted) key = "_"; else key = "-"; }
            if (KBoard.IsKeyDown(Keys.OemComma)) { AddKey = true; if (Shifted) key = "<"; else key = ","; }
            if (KBoard.IsKeyDown(Keys.OemComma)) { AddKey = true; if (Shifted) key = "<"; else key = ","; }
            if (KBoard.IsKeyDown(Keys.Space)) { AddKey = true; key = " "; }

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
            sbBatch.Draw(t2dKeyboard[oskb_KeyboardShiftState], oskb_DestRect, Color.White);
            
            int curicon = 0;
            // 畫出圓圈的 1/4 highlight, Paayer 1..4 
            if (oskb_CurrentPlayer == PlayerIndex.Two) { curicon = 1; }
            if (oskb_CurrentPlayer == PlayerIndex.Three) { curicon = 2; }
            if (oskb_CurrentPlayer == PlayerIndex.Four) { curicon = 3; }
            sbBatch.Draw(t2dPlayerIcons[curicon], new Rectangle(oskb_PlayerIconX, oskb_PlayerIconY, 35, 35), Color.White);

            // skc, Draw Prompt
            WriteText(sbBatch, oskb_PromptText, oskb_PromptX, oskb_PromptY, oskb_PromptColor);
            WriteText(sbBatch, sBeforeCursor + oskb_sCursorChar + sAfterCursor, oskb_EntryStartX, oskb_EntryStartY, Color.Black);

            // skc, Draw each button when mouse cursor enter
            if (oskb_EntryHilightPos < 40)
            {
                int HLX = oskb_EntryHilightX + (oskb_EntryHilightHorizontalSpace * (oskb_EntryHilightPos % 10));
                if (oskb_EntryHilightPos % 10 >= oskb_EntryHilightGapLocation)
                {
                    HLX += oskb_EntryHilightGapSize;
                }
                int HLY = oskb_EntryHilightY + (oskb_EntryHilightVerticalSpace * (oskb_EntryHilightPos / 10));
                // skc, 只要畫一小塊白色
                sbBatch.Draw(t2dKeySelector[0], new Rectangle(HLX, HLY, 44, 44), Color.White);
            }

            if (oskb_EntryHilightPos == 40)
            {
                sbBatch.Draw(t2dKeySelector[1], new Rectangle(oskb_EntryHilightX + 2, oskb_EntryHilightY + (oskb_EntryHilightVerticalSpace * 4) + 13, 266, 40), Color.White);
            }

            if (oskb_EntryHilightPos == 41)
            {
                sbBatch.Draw(t2dKeySelector[1], new Rectangle(oskb_EntryHilightX + 280, oskb_EntryHilightY + (oskb_EntryHilightVerticalSpace * 4) + 13, 266, 40), Color.White);
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


