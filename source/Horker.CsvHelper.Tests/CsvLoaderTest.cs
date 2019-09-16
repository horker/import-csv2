using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using CsvHelper.Configuration;

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
                CsvHelperConfiguration = new Configuration()
            };

            using (var reader = new StreamReader(file))
            using (var loader = new CsvLoader(reader, config))
            {
                var dic = loader.LoadToDictionary();
                Assert.Equal(new string[] { "20", "21" }, dic["yyy"]);
            }
        }
    }
}
