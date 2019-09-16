using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
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
        public SwitchParameter NoHeader = false;

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

        protected override void BeginProcessing()
        {
            var csvHelperConfig = new Configuration()
            {
                AllowComments = AllowComments,
                BufferSize = BufferSize,
                Comment = CommentChar,
                HasHeaderRecord = !NoHeader,
                IgnoreBlankLines = !KeepBlankLines,
                IgnoreQuotes = IgnoreQuote,
                TrimOptions = TrimOption
            };

            var p = MyInvocation.BoundParameters;
            if (p.ContainsKey("Delimiter"))
                csvHelperConfig.Delimiter = Delimiter;

            if (p.ContainsKey("EscapeChar"))
                csvHelperConfig.Escape = EscapeChar;

            if (p.ContainsKey("QuoteChar"))
                csvHelperConfig.Quote = QuoteChar;

            var config = new Config()
            {
                CsvHelperConfiguration = csvHelperConfig,
                InitialCapacity = InitialCapacity,
                ColumnNames = ColumnNames,
                Strict = Strict
            };

            using (var reader = new StreamReader(Path, Encoding))
            {
                WriteObject(CsvLoader.LoadToDictionary(reader, config));
            }
        }
    }
}
