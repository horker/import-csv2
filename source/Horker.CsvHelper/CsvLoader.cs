using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace Horker.CsvHelper
{
    public class CsvLoader : IDisposable
    {
        private StreamReader _reader;
        private Config _config;
        private string[] _columnNames;
        private CsvParser _parser;

        public CsvLoader(StreamReader reader, Config config)
        {
            _reader = reader;
            _config = config;
            _parser = new CsvParser(reader, config.CsvHelperConfiguration);

            InitializeColumnNames();
        }

        public void Dispose()
        {
            _parser.Dispose();
        }

        private void InitializeColumnNames()
        {
            Debug.Assert(_columnNames == null);

            if (_config.CsvHelperConfiguration.HasHeaderRecord)
            {
                _columnNames = _parser.Read();
                if (_config.ColumnNames != null)
                    _columnNames = _config.ColumnNames;
            }
            else
            {
                _columnNames = _config.ColumnNames ?? new string[0];
            }
        }

        private void EnumerateRecords(Action<string[], int> operation)
        {
            int lineNumber = 0;
            while (true)
            {
                ++lineNumber;

                var record = _parser.Read();
                if (record == null)
                    break;

                if (_config.Strict)
                {
                    if (record.Length > _columnNames.Length)
                        throw new ApplicationException($"Too many columns at line {lineNumber}");

                    if (record.Length < _columnNames.Length)
                        throw new ApplicationException($"Number of columns are not enough at line {lineNumber}");
                }

                operation.Invoke(record, lineNumber);
            }
        }

        public OrderedDictionary LoadToDictionary()
        {
            var columns = new List<List<string>>(_columnNames.Length);

            int i;
            for (i = 0; i < _columnNames.Length; ++i)
                columns.Add(new List<string>(_config.InitialCapacity));

            EnumerateRecords((record, lineNumber) => {
                for (var j = record.Length - columns.Count; j > 0; --j)
                {
                    var newColumn = new List<string>(_config.InitialCapacity);
                    if (columns.Count > 0)
                    {
                        for (var k = 0; k < columns[0].Count; ++k)
                            newColumn.Add(string.Empty);
                    }
                    columns.Add(newColumn);
                }

                for (i = 0; i < record.Length; ++i)
                    columns[i].Add(record[i]);

                for (; i < columns.Count; ++i)
                    columns[i].Add(string.Empty);

            });

            var result = new OrderedDictionary();

            for (i = 0; i < Math.Min(_columnNames.Length, columns.Count); ++i)
                result.Add(_columnNames[i], columns[i]);

            for (; i < columns.Count; ++i)
                result.Add("Column" + (i + 1), columns[i]);

            return result;
        }
    }
}
