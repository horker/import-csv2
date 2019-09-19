using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace Horker.CsvHelper
{
    [Cmdlet("Import", "Csv2")]
    public class ImportCsv2 : PSCmdlet
    {
        [Parameter(Position = 0, Mandatory = false)]
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

        [Parameter(Position = 22, Mandatory = false, ValueFromPipeline = true)]
        public string InputObject = null;

        private Config _config;

        private BlockingCollection<object> _output;
        private CsvLoader _loader;
        private ManualResetEvent _completeEvent;
        private Exception _exception;

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

            if (AsDataTable)
            {
                // The current version does not support combination of -AsDataTable and input stream.

                if (string.IsNullOrEmpty(Path))
                {
                    WriteError(new ErrorRecord(new ArgumentException("-Path is required when -AsDataTable is set"), "", ErrorCategory.InvalidArgument, null));
                    return;
                }

                using (var reader = new StreamReader(Path, Encoding))
                using (var csvReader = new CsvReader(reader, _config.CsvHelperConfiguration))
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

                return;
            }

            if (!string.IsNullOrEmpty(Path))
            {
                using (var reader = new StreamReader(Path, Encoding))
                using (var loader = new CsvLoader(reader, _config))
                {
                    LoadFile(loader);
                }

                return;
            }

            _output = new BlockingCollection<object>();
            _completeEvent = new ManualResetEvent(false);

            _loader = new CsvLoader(null, _config);

            var thread = new Thread(() => {
                try
                {
                    LoadFile(_loader);
                }
                catch (Exception e)
                {
                    _exception = e;
                }
                finally
                {
                    _completeEvent.Set();
                }
            });
            thread.Name = "Import-Csv2 loader thread";
            thread.Start();
        }

        protected override void ProcessRecord()
        {
            if (InputObject == null || _output == null)
                return;

            _loader.WriteLine(InputObject);

            while (_output.Count > 0)
            {
                var output = _output.Take();
                WriteObject(output);
            }
        }

        protected override void EndProcessing()
        {
            if (_output == null)
                return;

            try
            {
                _loader.Finish();
                _completeEvent.WaitOne();

                if (_exception != null)
                {
                    WriteError(new ErrorRecord(_exception, "", ErrorCategory.InvalidOperation, null));
                    return;
                }

                while (_output.Count > 0)
                {
                    var output = _output.Take();
                    WriteObject(output);
                }
            }
            finally
            {
                _loader.Dispose();
            }
        }

        private void Output(object value)
        {
            if (_output == null)
                WriteObject(value);
            else
                _output.Add(value);
        }

        private void LoadFile(CsvLoader loader)
        {
            if (RecordType != null)
            {
                if (ColumnNameMap != null)
                {
                    var gm = typeof(ImportCsv2).GetMethod("DefineClassMap", BindingFlags.NonPublic | BindingFlags.Instance);
                    var m = gm.MakeGenericMethod(new Type[] { RecordType });
                    m.Invoke(this, new object[0]);
                }

                while (loader.Read())
                {
                    var r = loader.GetRecord(RecordType);
                    Output(r);
                }
            }
            else
            {
                if (AsDictionary)
                    Output(loader.LoadToDictionary());
                else
                    EnumerateAsPSObject(loader);
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
            while (loader.Read())
            {
                var columnNames = loader.ColumnNames;
                var record = loader.Record;
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

                Output(pso);
            }
        }
    }
}
