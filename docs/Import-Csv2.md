---
external help file: Horker.CsvHelper.PowerShell.dll-Help.xml
Module Name: pscsvhelper
online version: https://github.com/horker/pscsvhelper/blob/docs/Import-Csv2.md
schema: 2.0.0
---

# Import-Csv2

## SYNOPSIS
Import a CSV file.

## SYNTAX

```
Import-Csv2 [[-Path] <String>] [[-Encoding] <Encoding>] [-AllowComments] [[-BufferSize] <Int32>]
 [[-CommentChar] <Char>] [[-Delimiter] <String>] [[-EscapeChar] <Char>] [-NoHeaderRecord] [-KeepBlankLines]
 [-IgnoreQuote] [[-QuoteChar] <Char>] [[-TrimOption] <TrimOptions>] [[-InitialCapacity] <Int32>]
 [[-ColumnNames] <String[]>] [-Strict] [[-RecordType] <Type>] [-AsDictionary] [-AsDataTable]
 [[-ColumnTypes] <IDictionary>] [-Culture <CultureInfo>] [-ColumnNameMap <IDictionary>]
 [-Configuration <Configuration>] [-InputObject <String>] [<CommonParameters>]
```

## DESCRIPTION
Import a CSV file.

## EXAMPLES

## PARAMETERS

### -AllowComments
Sets a value indicating if comments are allowed. True to allow commented out lines, otherwise false.

This parameter corresponds to `CsvHelper.Configuration.Configuration.AllowComments`.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -AsDataTable
Indicates to produce an output as a `DataTable` object.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: 17
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -AsDictionary
Indicates to produce an output as a `OrderedDictionary` object.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: 16
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BufferSize
Sets the size of the buffer used for reading CSV files. Default is 2048.

This parameter corresponds to `CsvHelper.Configuration.Configuration.BufferSize`.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: 3
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ColumnNames
Specifies new column names.

This parameter will be ignored when the `-RecordType` or `-AsDataTable` parameter is specified.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: 13
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ColumnTypes
Specifies the types of columns. It should be a dictionary of a column name and its type.

This parameter will be ignored when the `-RecordType` parameter is specified.

```yaml
Type: IDictionary
Parameter Sets: (All)
Aliases:

Required: False
Position: 18
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CommentChar
Sets the character used to denote a line that is commented out. Default is '#'.

This parameter corresponds to `CsvHelper.Configuration.Configuration.Comment`.

```yaml
Type: Char
Parameter Sets: (All)
Aliases:

Required: False
Position: 4
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Delimiter
Sets the delimiter used to separate fields. Default is `CultureInfo.TextInfo.ListSeparator`.

This parameter corresponds to `CsvHelper.Configuration.Configuration.Delimiter`.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 5
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Encoding
Specifies the text encoding of the input file.

```yaml
Type: Encoding
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -EscapeChar
Sets the escape character used to escape a quote inside a field. Default is '"'.

This parameter corresponds to `CsvHelper.Configuration.Configuration.Escape`.

```yaml
Type: Char
Parameter Sets: (All)
Aliases:

Required: False
Position: 6
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IgnoreQuote
Sets a value indicating if quotes should be ignored when parsing and treated like any other character.

This parameter corresponds to `CsvHelper.Configuration.Configuration.IgnoreQuote`.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: 9
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InitialCapacity
Specifies the initial capacity of the List object that is created when the `-AsDictionary` parameter is set.

This parameter is effective in combination with hte `-AsDictionary` parameter.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: 12
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -KeepBlankLines
Sets a value indicating if blank lines should be ignored when reading. True to ignore, otherwise false. Default is false.

This parameter corresponds to `CsvHelper.Configuration.Configuration.IgnoreBlankLines`, but its meaning is opposite.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: 8
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Path
Indicates the input CSV file.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -QuoteChar
Sets the character used to quote fields. Default is '"'.

This parameter corresponds to `CsvHelper.Configuration.Configuration.Quote`.

```yaml
Type: Char
Parameter Sets: (All)
Aliases:

Required: False
Position: 10
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RecordType
Indicates the record type into which the record will be mapped.

```yaml
Type: Type
Parameter Sets: (All)
Aliases:

Required: False
Position: 15
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Strict
Indicates to raise an error when the number of fields is different from that of the header record.

This parameter will be ignored when the `-RecordType` or `-AsDataTable` parameter is specified.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: 14
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TrimOption
Sets the field trimming options. Default is `Trim`.

```yaml
Type: TrimOptions
Parameter Sets: (All)
Aliases:
Accepted values: None, Trim, InsideQuotes

Required: False
Position: 11
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoHeaderRecord
Sets a value indicating if the CSV file has a header record. Default is false.

This parameter corresponds to `CsvHelper.Configuration.Configuration.HasHeaderRecord`, but its meaning is opposite.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ColumnNameMap
Specifies the mapping from the field names in the input file to the field names in the output object.

With the `-RecordType` parameter, this parameter represents the mapping from the class member names to the field names or the column index in the input file.

```yaml
Type: IDictionary
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Culture
Specifies the culture that is used when type conversion is performed.

This parameter corresponds to `CsvHelper.Configuration.Configuration.CultureInfo`.

```yaml
Type: CultureInfo
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Configuration
Specifies a `CsvHelper.Configuration.Configuration` object.

```yaml
Type: Configuration
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputObject
Specifies the CSV strings to be converted to objects.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
