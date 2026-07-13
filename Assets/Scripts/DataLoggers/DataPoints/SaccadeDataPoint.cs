using UnityEngine;

namespace DataLoggers.DataPoints
{
    internal struct SaccadeDataPoint
    {
        public float TimeMs, TargetX, TargetY;
        public Quaternion HRot, LLocRot, RLocRot, LRot, RRot;
        public float LConf, RConf;
    }
}
