﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using CsvHelper.Configuration;
using System.Globalization;

namespace Horker.CsvHelper.Tests
{
    public class CsvLoaderTest
    {
        private string CreateTestFile(string data)
        {
            var file = Path.GetTempFileName();
            File.WriteAllText(file, data);
            return file;
        }

        [Fact]
        public void TestLoadToDictionary()
        {
            string data = @"
xxx,yyy,zzz
10,20,30
11,21,31
";
            var file = CreateTestFile(data);

            var config = new Config()
            {
                CsvHelperConfiguration = new CsvConfiguration(CultureInfo.CurrentCulture)
            };

            using (var reader = new StreamReader(file))
            using (var loader = new CsvLoader(reader, config))
            {
                var dic = loader.LoadToDictionary();
                Assert.Equal(new string[] { "20", "21" }, dic["yyy"]);
            }
        }

        [Fact]
        public void TestTypeConversion()
        {
            string data = @"
xxx,yyy,zzz
10,20,30
11,21,31
";
            var file = CreateTestFile(data);

            var config = new Config()
            {
                CsvHelperConfiguration = new CsvConfiguration(CultureInfo.CurrentCulture),
                ColumnTypes = new Dictionary<string, Type>()
                {
                    { "zzz", typeof(int) }
                }
            };

            using (var reader = new StreamReader(file))
            using (var loader = new CsvLoader(reader, config))
            {
                var dic = loader.LoadToDictionary();
                Assert.Equal(new string[] { "20", "21" }, dic["yyy"]);
                Assert.Equal(new int[] { 30, 31 }, dic["zzz"]);
            }
        }

        [Fact]
        public void TestTypeConversion2()
        {
            string data = @"
xxx,yyy,zzz
10,20,2019/1/1
11,21,2019/2/1
";
            var file = CreateTestFile(data);

            var config = new Config()
            {
                CsvHelperConfiguration = new CsvConfiguration(CultureInfo.CurrentCulture),
                ColumnTypes = new Dictionary<string, Type>()
                {
                    { "zzz", typeof(DateTime) }
                }
            };

            using (var reader = new StreamReader(file))
            using (var loader = new CsvLoader(reader, config))
            {
                var dic = loader.LoadToDictionary();
                Assert.Equal(new string[] { "20", "21" }, dic["yyy"]);
                Assert.Equal(new DateTime[] { DateTime.Parse("2019/1/1"), DateTime.Parse("2019/2/1") }, dic["zzz"]);
            }
        }
    }
}
