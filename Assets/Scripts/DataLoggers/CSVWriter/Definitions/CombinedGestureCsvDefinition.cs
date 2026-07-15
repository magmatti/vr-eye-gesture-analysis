using DataLoggers.DataPoints;

namespace DataLoggers.CSVWriter.Definitions
{
    internal static class CombinedGestureCsvDefinition
    {
        internal static readonly CsvDefinition<CombinedGestureDataPoint> Definition =
            new CsvDefinition<CombinedGestureDataPoint>()
                .Column("Time_ms", point => point.TimeMs, "F0")
                .Column("Phase", point => point.Phase)
                .Column("PhaseTime_ms", point => point.PhaseTimeMs, "F0")
                .Column("TargetRotX", point => point.TargetX, "F2")
                .Column("TargetRotY", point => point.TargetY, "F2")
                .Column("HeadRotX", point => point.HRot.x, "F5")
                .Column("HeadRotY", point => point.HRot.y, "F5")
                .Column("HeadRotZ", point => point.HRot.z, "F5")
                .Column("HeadRotW", point => point.HRot.w, "F5")
                .Column("LeftLocalRotX", point => point.LLocRot.x, "F5")
                .Column("LeftLocalRotY", point => point.LLocRot.y, "F5")
                .Column("LeftLocalRotZ", point => point.LLocRot.z, "F5")
                .Column("LeftLocalRotW", point => point.LLocRot.w, "F5")
                .Column("LeftWorldRotX", point => point.LRot.x, "F5")
                .Column("LeftWorldRotY", point => point.LRot.y, "F5")
                .Column("LeftWorldRotZ", point => point.LRot.z, "F5")
                .Column("LeftWorldRotW", point => point.LRot.w, "F5")
                .Column("RightLocalRotX", point => point.RLocRot.x, "F5")
                .Column("RightLocalRotY", point => point.RLocRot.y, "F5")
                .Column("RightLocalRotZ", point => point.RLocRot.z, "F5")
                .Column("RightLocalRotW", point => point.RLocRot.w, "F5")
                .Column("RightWorldRotX", point => point.RRot.x, "F5")
                .Column("RightWorldRotY", point => point.RRot.y, "F5")
                .Column("RightWorldRotZ", point => point.RRot.z, "F5")
                .Column("RightWorldRotW", point => point.RRot.w, "F5")
                .Column("LeftBlinkWeight", point => point.LeftBlinkWeight, "F5")
                .Column("RightBlinkWeight", point => point.RightBlinkWeight, "F5")
                .Column("LeftConfidence", point => point.LConf, "F2")
                .Column("RightConfidence", point => point.RConf, "F2");
    }
}
