using DataLoggers.DataPoints;

namespace DataLoggers.CSVWriter.Definitions
{
    internal static class BlinkCsvDefinition
    {
        internal static readonly CsvDefinition<BlinkDataPoint> Definition =
            new CsvDefinition<BlinkDataPoint>()
                .Column("Time_ms", point => point.TimeMs, "F0")
                .Column("LeftBlinkWeight", point => point.LeftBlinkWeight, "F5")
                .Column("RightBlinkWeight", point => point.RightBlinkWeight, "F5")
                .Column("LeftConfidence", point => point.LConf, "F2")
                .Column("RightConfidence", point => point.RConf, "F2");
    }
}
