using UnityEngine;
using GestureTestPhase = DataLoggers.TestPhase.TestPhase;

namespace DataLoggers.DataPoints
{
    internal struct CombinedGestureDataPoint
    {
        public float TimeMs;
        public GestureTestPhase Phase;
        public float PhaseTimeMs, TargetX, TargetY;
        public Quaternion HRot, LLocRot, LRot, RLocRot, RRot;
        public float LeftBlinkWeight, RightBlinkWeight;
        public float LConf, RConf;
    }
}
