using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace DataLoggers.CSVWriter
{
    internal sealed class CsvWriter<T> : IDisposable
    {
        private readonly TextWriter writer;
        private readonly IReadOnlyList<CsvColumn<T>> columns;
        private readonly StringBuilder lineBuilder = new StringBuilder();

        internal CsvWriter(TextWriter writer, CsvDefinition<T> definition)
        {
            this.writer = writer;
            columns = new List<CsvColumn<T>>(definition.Columns);
            WriteHeader();
        }

        internal void WriteRecord(T record)
        {
            lineBuilder.Clear();

            for (int i = 0; i < columns.Count; i++)
            {
                if (i > 0)
                {
                    lineBuilder.Append(',');
                }

                CsvColumn<T> column = columns[i];
                object value = column.Selector(record);
                lineBuilder.Append(Format(value, column.Format));
            }

            writer.WriteLine(lineBuilder.ToString());
        }

        internal void WriteRecords(IEnumerable<T> records)
        {
            foreach (T record in records)
            {
                WriteRecord(record);
            }
        }

        public void Dispose() => writer.Dispose();

        private void WriteHeader()
        {
            lineBuilder.Clear();

            for (int i = 0; i < columns.Count; i++)
            {
                if (i > 0)
                {
                    lineBuilder.Append(',');
                }

                lineBuilder.Append(columns[i].Header);
            }

            writer.WriteLine(lineBuilder.ToString());
        }

        private static string Format(object value, string format)
        {
            if (value is IFormattable formattable)
            {
                return formattable.ToString(format, CultureInfo.InvariantCulture);
            }

            return value.ToString();
        }
    }
}
