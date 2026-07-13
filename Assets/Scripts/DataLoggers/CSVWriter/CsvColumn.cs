using System;

namespace DataLoggers.CSVWriter
{
    internal sealed class CsvColumn<T>
    {
        internal CsvColumn(string header, Func<T, object> selector, string format)
        {
            Header = header;
            Selector = selector;
            Format = format;
        }

        internal string Header { get; }
        internal Func<T, object> Selector { get; }
        internal string Format { get; }
    }
}
