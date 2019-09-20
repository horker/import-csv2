# pscsvhelper

## Overview

This is a PowerShell module to import CSV files. Compared to the built-in `Import-Csv` cmdlet, it offers the following advantages:

- Flexible parsing options: specifying delimiter/escaping/quoting characters, allowing comments, skipping blank lines, ignoring quotes, and trimming spaces
- Various output data format: a sequence of `PSObject`s, a `DataTable`, an `OrderedDictionary`, and mapping to a .NET class
- Type conversion of fields

This module is built on top of [CsvHelper](https://joshclose.github.io/CsvHelper/), a well-known .NET library for reading and writing CSV files.

This module is implemented in C# for performance.

## Installation

This module is published in the [PowerShell Gallery](https://powershellgaallery.com/pscvshelper)

```PowerShell
PS C:\> Install-Module pscsvhelper
```

## Usage

This module exports a single cmdlet: `Import-Csv2`. By default, this cmdlet reads a CSV file and produces a sequence of `PSObject` objects as the built-in `Import-Csv` cmdlet does.

```PowerShell
PS C:\> Import-Csv2 iris.csv | ft

Sepal.Length Sepal.Width Petal.Length Petal.Width Species
------------ ----------- ------------ ----------- -------
5.1          3.5         1.4          0.2         setosa
4.9          3           1.4          0.2         setosa
4.7          3.2         1.3          0.2         setosa
    :
```

With the `-AsDictionary` parameter, it returns an `OrderedDictionary` object that contains a `List<T>` object for each field. This option is fast to load and memory-efficient.

```PowerShell
PS C:\> Import-Csv2 iris.csv -AsDictionary

Name                           Value
----                           -----
Sepal.Length                   {5.1, 4.9, 4.7, 4.6...}
Sepal.Width                    {3.5, 3, 3.2, 3.1...}
Petal.Length                   {1.4, 1.4, 1.3, 1.5...}
Petal.Width                    {0.2, 0.2, 0.2, 0.2...}
Species                        {setosa, setosa, setosa, setosa...}
```

You can also obtain a `DataTable` object by invoking the cmdlet with `-AsDataTable` instead of `-AsDictionary`.

As another option, you can map the data to a certain .NET class by specifying the `-RecordType` parameter. Mapping from the header fields to the class properties can be defined in the `-ColumnNameMap` parameter.

```PowerShell
PS C:\> class IrisRecord {
>>     [string]$Species
>>     [double]$SepalLength
>>     [double]$SepalWidth
>>     [double]$PetalLength
>>     [double]$PetalWidth
>> }

PS C:\> Import-Csv2 iris.csv -RecordType ([IrisRecord]) -ColumnNameMap @{
>>     "Sepal.Length" = "SepalLength"
>>     "Sepal.Width" = "SepalWidth"
>>     "Petal.Length" = "PetalLength"
>>     "Petal.Width" = "PetalWidth"
>> } | ft

Species SepalLength SepalWidth PetalLength PetalWidth
------- ----------- ---------- ----------- ----------
setosa          5.1        3.5         1.4        0.2
setosa          4.9          3         1.4        0.2
setosa          4.7        3.2         1.3        0.2
    :
```

You can specify field types by the `-ColumnType` parameter.

```PowerShell
PS C:\> $data = import-Csv2 iris.csv -AsDictionary -ColumnTypes @{ "Sepal.Width" = [double] }

PS C:\> $data["Sepal.Width"].GetType().FullName
System.Collections.Generic.List`1[[System.Double, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]
```

### Parsing options

The following parsing options are available:

|Parameter|Description|
|---------|-----------|
|-Delimiter|Specifies a delimiter character.|
|-EscapeChar|Specifies a escaping character.|
|-QuoteChar|Specifies a quoting character.|
|-NoHeaderRecord|Indicates if the CSV file has a header record.|
|-ColumnNames|Specifies column names to override the header record.|
|-ColumnNameMap|Specifies mapping from the header field names to the column names of the output.|
|-AllowComments|Indicates to allow comments that begin with '#' by default.|
|-CommentChar|Specifies a comment character.|
|-IgnoreQuote|Indicates to ignore quoting.|
|-KeepBlankLines|Indicates if blank lines should be ignored when reading.|
|-Strict|Indicates to raise an error when the number of fields is different from that of the header record.|

For the cmoplete reference, see the [help topic](https://github.com/horker/pscsvhelper/blob/docs/Import-Csv2.md) of the cmdlet.

## License

This module is licensed under the MIT license.

The `CsvHelper` library included in this module is subject to its own license. Refer to the [project site](https://joshclose.github.io/CsvHelper/),