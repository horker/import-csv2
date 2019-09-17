using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;

namespace Horker.CsvHelper
{
    public class Config
    {
        public Configuration CsvHelperConfiguration { get; set; } = null;
        public int InitialCapacity { get; set; } = 1024;
        public string[] ColumnNames { get; set; } = null;
        public IDictionary ColumnTypes { get; set; } = null;
        public bool Strict = false;
    }
}
