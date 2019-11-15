using Microsoft.Xna.Framework;

namespace SimuSurvey360.Instruments
{
    class InstrumentArgs
    {
        #region Field
        public Instruments.InstrumentType Type;
        public Vector3 WorldPosition;//instrument position (N, H, E) coordinates
        #endregion
    }
}
