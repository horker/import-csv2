using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace Horker.CsvHelper
{
    public static class CsvLoader
    {
        public static OrderedDictionary LoadToDictionary(StreamReader reader, Config config)
        {
            using (var parser = new CsvParser(reader, config.CsvHelperConfiguration))
            {
                string[] columnNames;
                if (config.CsvHelperConfiguration.HasHeaderRecord)
                {
                    columnNames = parser.Read();
                    if (config.ColumnNames != null)
                        columnNames = config.ColumnNames;
                }
                else
                {
                    columnNames = config.ColumnNames ?? new string[0];
                }

                var columns = new List<List<string>>(columnNames.Length);

                int i;
                for (i = 0; i < columnNames.Length; ++i)
                    columns.Add(new List<string>(config.InitialCapacity));

                int lineNumber = 0;
                while (true)
                {
                    ++lineNumber;

                    var record = parser.Read();
                    if (record == null)
                        break;

                    if (record.Length > columns.Count)
                    {
                        if (config.Strict)
                            throw new ApplicationException($"Too many columns at line {lineNumber}");

                        for (var j = record.Length - columns.Count; j > 0; --j)
                        {
                            var newColumn = new List<string>(config.InitialCapacity);
                            if (columns.Count > 0)
                            {
                                for (var k = 0; k < columns[0].Count; ++k)
                                    newColumn.Add(string.Empty);
                            }
                            columns.Add(newColumn);
                        }
                    }
                    else
                    {
                        if (config.Strict && record.Length < columns.Count)
                            throw new ApplicationException($"Number of columns are not enough at line {lineNumber}");
                    }

                    for (i = 0; i < record.Length; ++i)
                        columns[i].Add(record[i]);

                    for (; i < columns.Count; ++i)
                        columns[i].Add(string.Empty);
                }

                var result = new OrderedDictionary();

                for (i = 0; i < Math.Min(columnNames.Length, columns.Count); ++i)
                    result.Add(columnNames[i], columns[i]);

                for (; i < columns.Count; ++i)
                    result.Add("Column" + (i + 1), columns[i]);

                return result;
            }
        }
    }
}
