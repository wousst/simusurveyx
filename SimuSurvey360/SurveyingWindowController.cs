using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SimuSurvey360.Instruments;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace SimuSurvey360
{
    class SurveyingWindowController
    {
        private ViewController _ViewController;
        private LinkedListNode<Instrument> _SelectedInstrument;
        private LinkedList<Instrument> _Instruments;
        private Coordinates _Coordinates;
        private ContentManager _ContentManager;
        private Texture2D _BlackLine;
        private Viewport _Viewport;
        private int _LineWidth;

        public SurveyingWindowController()
        {
        }
        public SurveyingWindowController(GraphicsDevice graphicsDevice,Viewport port, Rectangle field)
        {
            _ViewController = new ViewController(port,field);//initialize to the one identical to the scene controller
            _Coordinates = new Coordinates(graphicsDevice, field);
            _Viewport = port;
            _LineWidth = 1;
        }

        //Syncronization with Scene Controller
        public void InstrumentsSyncronization(LinkedList<Instrument> instruments)
        {
            _Instruments = instruments;
        }
        public void InstrumentsSyncronization(LinkedListNode<Instrument> selectedInstrument)
        {
            _SelectedInstrument = selectedInstrument;
            //Syncronize the viewer in viewcontroller and the selected instrument
            _ViewController.Initialization(_SelectedInstrument.Value);
        }

        public void LoadContent(ContentManager contentManager)
        {
            _ContentManager = contentManager;

            _BlackLine = contentManager.Load<Texture2D>("gradient");

            //Coordinates
            _Coordinates.LoadContent();
        }

        public void Update()
        {
            //Update View
            _ViewController.Update();

            //Update Coordinates
            _Coordinates.Update(_ViewController.World, _ViewController.View, _ViewController.Projection);

            //Update Instruments
            foreach (Instrument instrument in _Instruments)
            {
                switch (instrument.Type)
                {
                    case InstrumentType.TotalStation:
                    case InstrumentType.Leveling:
                    case InstrumentType.Theodolite:
                        ((TotalStation)instrument).Update(_ViewController.World, _ViewController.View, _ViewController.Projection);
                        break;
                    case InstrumentType.Level:
                        ((Level)instrument).Update(_ViewController.World, _ViewController.View, _ViewController.Projection);
                        break;
                    case InstrumentType.Ruler:
                        ((Ruler)instrument).Update(_ViewController.World, _ViewController.View, _ViewController.Projection);
                        break;
                }
            }
        }

        public void Draw3D()
        {
            _Coordinates.Draw();
            foreach (Instrument instrument in _Instruments)
            {
                switch (instrument.Type)
                {
                    case InstrumentType.TotalStation:
                        ((TotalStation)instrument).Draw();
                        break;
                    case InstrumentType.Level:
                        ((Level)instrument).Draw();
                        break;
                    case InstrumentType.Ruler:
                        ((Ruler)instrument).Draw();
                        break;
                }
            }
        }

        public void DrawCorssLine(SpriteBatch spriteBatch)
        {
            Rectangle paint = new Rectangle(_Viewport.Width / 2, 0, _LineWidth, _Viewport.Height);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            spriteBatch.Draw(_BlackLine, paint, Color.Black);
            spriteBatch.End();

            paint = new Rectangle((_Viewport.Width / 2) - 1, _Viewport.Height * 15 / 32, _LineWidth * 3, _Viewport.Height * 1 / 16);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            spriteBatch.Draw(_BlackLine, paint, Color.Black);
            spriteBatch.End();

            /*paint = new Rectangle(_Viewport.Width / 2, _Viewport.Height * 9 / 16, _LineWidth, _Viewport.Height * 7 / 16);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            spriteBatch.Draw(_BlackLine, paint, Color.White);
            spriteBatch.End();*/

            paint = new Rectangle(0, _Viewport.Height / 2, _Viewport.Width, _LineWidth);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            spriteBatch.Draw(_BlackLine, paint, Color.Black);
            spriteBatch.End();

            paint = new Rectangle(_Viewport.Width * 15 / 32, (_Viewport.Height / 2) - 1, _Viewport.Width * 1 / 16, _LineWidth * 3);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            spriteBatch.Draw(_BlackLine, paint, Color.Black);
            spriteBatch.End();

            /*paint = new Rectangle(_Viewport.Width * 9 / 16, _Viewport.Height / 2, _Viewport.Width * 7 / 16, _LineWidth);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            spriteBatch.Draw(_BlackLine, paint, Color.White);
            spriteBatch.End();*/

            //Sub lines
            paint = new Rectangle(_Viewport.Width * 3 / 8, _Viewport.Height / 4, _Viewport.Width / 4, _LineWidth);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            spriteBatch.Draw(_BlackLine, paint, Color.Black);
            spriteBatch.End();

            paint = new Rectangle(_Viewport.Width * 3 / 8, _Viewport.Height * 3 / 4, _Viewport.Width / 4, _LineWidth);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            spriteBatch.Draw(_BlackLine, paint, Color.Black);
            spriteBatch.End();

            paint = new Rectangle(_Viewport.Width / 4, _Viewport.Height * 3 / 8, _LineWidth, _Viewport.Height / 4);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            spriteBatch.Draw(_BlackLine, paint, Color.Black);
            spriteBatch.End();

            paint = new Rectangle(_Viewport.Width * 3 / 4, _Viewport.Height * 3 / 8, _LineWidth, _Viewport.Height / 4);
            spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            spriteBatch.Draw(_BlackLine, paint, Color.Black);
            spriteBatch.End();
        }
    }
}
