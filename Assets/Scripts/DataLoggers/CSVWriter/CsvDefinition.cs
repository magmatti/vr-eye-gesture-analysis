using System;
using System.Collections.Generic;

namespace DataLoggers.CSVWriter
{
    internal sealed class CsvDefinition<T>
    {
        private readonly List<CsvColumn<T>> columns = new List<CsvColumn<T>>();

        internal IReadOnlyList<CsvColumn<T>> Columns => columns;

        internal CsvDefinition<T> Column(
            string header,
            Func<T, object> selector,
            string format = null)
        {
            columns.Add(new CsvColumn<T>(header, selector, format));
            return this;
        }
    }
}
