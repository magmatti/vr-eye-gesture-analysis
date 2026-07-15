using System.Collections.Generic;
using System.IO;

namespace DataLoggers.CSVWriter
{
    internal static class CsvFile
    {
        internal static void Write<T>(
            string path,
            IEnumerable<T> records,
            CsvDefinition<T> definition)
        {
            using (var writer = new CsvWriter<T>(new StreamWriter(path), definition))
            {
                writer.WriteRecords(records);
            }
        }
    }
}
