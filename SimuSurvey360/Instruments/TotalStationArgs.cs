using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimuSurvey360.Instruments
{
    class TotalStationArgs : InstrumentArgs
    {
        //==Properties (Values controled from outside the class)==
        public float TripodRotationValue;//the angle (degree) between main body and tripod
        public float TripodLength;
        public float TelescopeRotationValue;
        public float TribrachRotationValue;

        public TotalStationArgs()
        {
            Type = InstrumentType.TotalStation;
        }
    }
}
