using UnityEngine;

namespace DataLoggers.DataPoints
{
    internal struct FixationDataPoint
    {
        public float TimeMs;
        public Quaternion HRot, LLocRot, RLocRot, LRot, RRot;
        public float LConf, RConf;
    }
}
