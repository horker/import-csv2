using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;

namespace Horker.CsvHelper
{
    public class ColumnNameMapHolder
    {
        public static IDictionary ColumnNameMap { get; set; }
    }

    public class DynamicClassMap<T> : ClassMap<T>
    {
        public DynamicClassMap()
        {
            AutoMap();

            var map = ColumnNameMapHolder.ColumnNameMap;

            if (map == null)
                return;

            foreach (DictionaryEntry entry in map)
            {
                var member = typeof(T).GetMember(entry.Value.ToString());
                if (member == null)
                    throw new ArgumentException($"Member '{entry.Value}' is not found in the class {typeof(T)}");

                var key = entry.Key;
                if (key is PSObject pso)
                    key = pso.BaseObject;

                if (key is string name)
                    Map(typeof(T), member[0]).Name(name);
                else if (key is int index)
                    Map(typeof(T), member[0]).Index(index);
                else
                    throw new ArgumentException("Key of -ColumnNameMap should be a string or int");
            }
        }
    }
}
