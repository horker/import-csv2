using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace Horker.CsvHelper
{
    public class CsvLoader : IDisposable
    {
        private readonly int BufferSize = 1024;

        private StreamReader _reader;
        private Config _config;
        private string[] _columnNames;
        private CsvReader _csvReader;
        private Type[] _columnTypes;
        private MemberMapData[] _memberMapData;

        public string[] ColumnNames => _columnNames;

        public CsvLoader(StreamReader reader, Config config)
        {
            _reader = reader;
            _config = config;
            _csvReader = new CsvReader(reader, config.CsvHelperConfiguration);

            InitializeColumnNames();
            InitializeMemberMapData();
        }

        public void Dispose()
        {
            _csvReader.Dispose();
        }

        private void InitializeColumnNames()
        {
            Debug.Assert(_columnNames == null);

            if (_config.CsvHelperConfiguration.HasHeaderRecord)
            {
                if (_csvReader.Read() && _csvReader.ReadHeader())
                    _columnNames = _csvReader.Context.HeaderRecord;
            }

            if (_config.ColumnNames != null)
                _columnNames = _config.ColumnNames;

            if (_config.ColumnNameMap != null)
            {
                for (var i = 0; i < _columnNames.Length; ++i)
                {
                    if (_config.ColumnNameMap.Contains(_columnNames[i]))
                        _columnNames[i] = (string)_config.ColumnNameMap[_columnNames[i]];
                }
            }

            if (_columnNames == null)
                _columnNames = new string[0];
        }

        private void InitializeMemberMapData()
        {
            _columnTypes = new Type[_columnNames.Length];
            _memberMapData = new MemberMapData[_columnNames.Length];

            if (_config.ColumnTypes == null)
                return;

            foreach (DictionaryEntry entry in _config.ColumnTypes)
            {
                var name = (string)entry.Key;
                var type = (Type)entry.Value;

                var context = _csvReader.Context;
                var found = false;
                for (var i = 0; i < _columnNames.Length; ++i)
                {
                    if (_columnNames[i] == name)
                    {
                        var converters = _config.CsvHelperConfiguration.TypeConverterCache.GetConverter(type);
                        var options = context.ReaderConfiguration.TypeConverterOptionsCache.GetOptions(type);
                        options.CultureInfo = options.CultureInfo ?? _config.Culture;

                        var mapData = new MemberMapData(null)
                        {
                            TypeConverter = converters,
                            TypeConverterOptions = options
                        };

                        _columnTypes[i] = type;
                        _memberMapData[i] = mapData;
                        found = true;
                    }
                }

                if (!found)
                    throw new ArgumentException($"The ColumnTypes parameter contains unknown column name: {name}");
            }
        }

        public IEnumerable<string[]> EnumerateRecords()
        {
            while (_csvReader.Read())
            {
                var record = _csvReader.Context.Record;

                if (_config.Strict)
                {
                    if (record.Length > _columnNames.Length)
                        throw new ApplicationException($"Too many columns at line {_csvReader.Context.Row}");

                    if (record.Length < _columnNames.Length)
                        throw new ApplicationException($"Number of columns are not enough at line {_csvReader.Context.Row}");
                }

                yield return record;
            }
        }

        public object Convert(string value, int index)
        {
            if (_memberMapData[index] == null)
                return value;

            return _memberMapData[index].TypeConverter.ConvertFromString(value, _csvReader, _memberMapData[index]);
        }

        public void ConvertColumn<T>(List<T> result, List<string> data, int index)
        {
            if (typeof(T) == typeof(string))
            {
                foreach (var e in data)
                    result.Add((T)(object)e);
            }
            else
            {
                foreach (var e in data)
                    result.Add((T)Convert(e, index));
            }
        }

        public OrderedDictionary LoadToDictionary()
        {
            // Initialize intermediate buffer lists.

            var columns = new List<List<string>>(_columnNames.Length);

            int i;
            for (i = 0; i < _columnNames.Length; ++i)
                columns.Add(new List<string>(BufferSize));

            // Initialize result lists and converter methods.

            IList[] converted = null;
            MethodInfo[] converterMethods = null;

            if (_config.ColumnTypes != null)
            {
                converted = new IList[_columnNames.Length];
                converterMethods = new MethodInfo[_columnNames.Length];

                for (i = 0; i < _columnNames.Length; ++i)
                {
                    var type = _columnTypes[i] ?? typeof(string);

                    var m = typeof(List<>).MakeGenericType(new Type[] { type }).GetConstructor(new Type[] { typeof(int) });
                    converted[i] = (IList)m.Invoke(new object[] { _config.InitialCapacity });
                    converterMethods[i] = typeof(CsvLoader).GetMethod("ConvertColumn").MakeGenericMethod(new Type[] { type });
                }
            }

            foreach (var record in EnumerateRecords())
            {
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

                if (converted != null && columns.Count == BufferSize)
                {
                    for (i = 0; i < columns.Count; ++i)
                    {
                        converterMethods[i].Invoke(this, new object[] { converted[i], columns[i], i });
                        columns[i].Clear();
                    }
                }
            }

            var result = new OrderedDictionary();

            if (converted == null)
            {
                for (i = 0; i < Math.Min(_columnNames.Length, columns.Count); ++i)
                    result.Add(_columnNames[i], columns[i]);

                for (; i < columns.Count; ++i)
                    result.Add("Column" + (i + 1), columns[i]);
            }
            else
            {
                for (i = 0; i < columns.Count; ++i)
                    converterMethods[i].Invoke(this, new object[] { converted[i], columns[i], i });

                for (i = 0; i < Math.Min(_columnNames.Length, columns.Count); ++i)
                    result.Add(_columnNames[i], converted[i]);

                for (; i < columns.Count; ++i)
                    result.Add("Column" + (i + 1), converted[i]);
            }

            return result;
        }
    }
}
