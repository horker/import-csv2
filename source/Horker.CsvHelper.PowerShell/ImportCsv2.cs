using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace Horker.CsvHelper
{
    [Cmdlet("Import", "Csv2")]
    public class ImportCsv2 : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = true)]
        public string Path;

        [Parameter(Position = 1, Mandatory = false)]
        public Encoding Encoding = Encoding.Default;

        [Parameter(Position = 2, Mandatory = false)]
        public SwitchParameter AllowComments = false;

        [Parameter(Position = 3, Mandatory = false)]
        public int BufferSize = 2048;

        [Parameter(Position = 4, Mandatory = false)]
        public char CommentChar = '#';

        [Parameter(Position = 5, Mandatory = false)]
        public string Delimiter = null;

        [Parameter(Position = 6, Mandatory = false)]
        public char EscapeChar;

        [Parameter(Position = 7, Mandatory = false)]
        public SwitchParameter NoHeaderRecord = false;

        [Parameter(Position = 8, Mandatory = false)]
        public SwitchParameter KeepBlankLines = false;

        [Parameter(Position = 9, Mandatory = false)]
        public SwitchParameter IgnoreQuote = false;

        [Parameter(Position = 10, Mandatory = false)]
        public char QuoteChar;

        [Parameter(Position = 11, Mandatory = false)]
        public TrimOptions TrimOption = TrimOptions.Trim;

        [Parameter(Position = 12, Mandatory = false)]
        public int InitialCapacity = 1024;

        [Parameter(Position = 13, Mandatory = false)]
        public string[] ColumnNames = null;

        [Parameter(Position = 14, Mandatory = false)]
        public SwitchParameter Strict = false;

        [Parameter(Position = 15, Mandatory = false)]
        public Type RecordType = null;

        [Parameter(Position = 16, Mandatory = false)]
        public SwitchParameter AsDictionary = false;

        [Parameter(Position = 17, Mandatory = false)]
        public SwitchParameter AsDataTable = false;

        [Parameter(Position = 18, Mandatory = false)]
        public IDictionary ColumnTypes = null;

        [Parameter(Position = 19, Mandatory = false)]
        public CultureInfo Culture = CultureInfo.CurrentCulture;

        [Parameter(Position = 20, Mandatory = false)]
        public IDictionary ColumnNameMap = null;

        [Parameter(Position = 21, Mandatory = false)]
        public Configuration Configuration = null;

        private Config _config;

        protected override void BeginProcessing()
        {
            var csvHelperConfig = Configuration ?? new Configuration();

            csvHelperConfig.AllowComments = AllowComments;
            csvHelperConfig.BufferSize = BufferSize;
            csvHelperConfig.Comment = CommentChar;
            csvHelperConfig.HasHeaderRecord = !NoHeaderRecord;
            csvHelperConfig.IgnoreBlankLines = !KeepBlankLines;
            csvHelperConfig.IgnoreQuotes = IgnoreQuote;
            csvHelperConfig.TrimOptions = TrimOption;

            var p = MyInvocation.BoundParameters;
            if (p.ContainsKey("Delimiter"))
                csvHelperConfig.Delimiter = Delimiter;

            if (p.ContainsKey("EscapeChar"))
                csvHelperConfig.Escape = EscapeChar;

            if (p.ContainsKey("QuoteChar"))
                csvHelperConfig.Quote = QuoteChar;

            _config = new Config()
            {
                CsvHelperConfiguration = csvHelperConfig,
                InitialCapacity = InitialCapacity,
                ColumnNames = ColumnNames,
                ColumnNameMap = ColumnNameMap,
                ColumnTypes = ColumnTypes,
                Strict = Strict,
                Culture = Culture
            };

            using (var reader = new StreamReader(Path, Encoding))
            {
                if (RecordType != null)
                {
                    if (ColumnNameMap != null)
                    {
                        var gm = typeof(ImportCsv2).GetMethod("DefineClassMap", BindingFlags.NonPublic | BindingFlags.Instance);
                        var m = gm.MakeGenericMethod(new Type[] { RecordType });
                        m.Invoke(this, new object[0]);
                    }

                    using (var csvReader = new CsvReader(reader, csvHelperConfig))
                    {
                        while (csvReader.Read())
                        {
                            var r = csvReader.GetRecord(RecordType);
                            WriteObject(r);
                        }
                    }
                }
                else if (AsDataTable)
                {
                    using (var csvReader = new CsvReader(reader, csvHelperConfig))
                    using (var csvDataReader = new CsvDataReader(csvReader))
                    {
                        var dt = new DataTable();

                        if (ColumnTypes != null)
                        {
                            foreach (DictionaryEntry entry in ColumnTypes)
                                dt.Columns.Add((string)entry.Key, (Type)entry.Value);
                        }

                        dt.Load(csvDataReader);
                        WriteObject(dt);
                    }
                }
                else
                {
                    using (var loader = new CsvLoader(reader, _config))
                    {
                        if (AsDictionary)
                            WriteObject(loader.LoadToDictionary());
                        else
                            EnumerateAsPSObject(loader);
                    }
                }
            }
        }

        private void DefineClassMap<T>()
        {
            var map = new DefaultClassMap<T>();
            foreach (DictionaryEntry entry in ColumnNameMap)
            {
                var member = RecordType.GetMember((string)entry.Key)[0];
                if (entry.Value is string name)
                    map.Map(typeof(T), member).Name(name);
                else if (entry.Value is int index)
                    map.Map(typeof(T), member).Index(index);
            }

            _config.CsvHelperConfiguration.RegisterClassMap(map);
        }

        private void EnumerateAsPSObject(CsvLoader loader)
        {
            var columnNames = loader.ColumnNames;

            foreach (var record in loader.EnumerateRecords())
            {
                var pso = new PSObject();

                int i;
                for (i = 0; i < Math.Min(columnNames.Length, record.Length); ++i)
                {
                    var r = loader.Convert(record[i], i);
                    pso.Properties.Add(new PSNoteProperty(columnNames[i], r));

                }

                for (; i < columnNames.Length; ++i)
                    pso.Properties.Add(new PSNoteProperty(columnNames[i], string.Empty));

                for (; i < record.Length; ++i)
                {
                    var r = loader.Convert(record[i], i);
                    pso.Properties.Add(new PSNoteProperty("Column" + (i + 1), r));
                }

                WriteObject(pso);
            }
        }
    }
}
